ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN

    if object_id('tempdb..#disable_td_delete_item') is null
    begin

        declare @content_id numeric, @virtual_type numeric, @published_id numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )

        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c
        on d.content_id = c.content_id

        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30),
            status_type_id numeric,
            splitted bit
        )


        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin

            select @content_id = id, @virtual_type = virtual_type from @c

            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id), status_type_id, splitted from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID
            where ca2.CONTENT_ID = @content_id

            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids

            select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
            if exists (select * from @ids where status_type_id = @published_id and splitted = 0)
                update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
            else
                update content_modification set stage_modified = GETDATE() where content_id = @content_id

            /* Drop relations to current item */
            if exists(select id from @attr_ids) and object_id('tempdb..#disable_td_delete_item_o2m_nullify') is null
            begin
                UPDATE content_attribute SET default_value = null
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND default_value IN (select char_id from @ids)

                UPDATE content_data SET data = NULL, blob_data = NULL
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)

                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
            end

            if @virtual_type = 0
            begin
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql

                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id

            delete from @ids

            delete from @attr_ids
        end
    end
END
GO
exec qp_drop_existing 'qp_get_upsert_items_sql_new', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_upsert_items_sql_new]
@table_name nvarchar(25),
@sql nvarchar(max) output
as
BEGIN
    set @sql = 'update base set '
    set @sql = @sql + ' base.modified = ci.modified, base.last_modified_by = ci.last_modified_by, base.status_type_id = ci.status_type_id, '
    set @sql = @sql + ' base.visible = ci.visible, base.archive = ci.archive '
    set @sql = @sql + ' from ' + @table_name + ' base with(rowlock) '
    set @sql = @sql + ' inner join content_item ci with(rowlock) on base.content_item_id = ci.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id'
    set @sql = @sql + ';' + CHAR(13) + CHAR(10)

    set @sql = @sql + 'insert into ' + @table_name + ' (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)'
    set @sql = @sql + ' select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by, '
    set @sql = @sql + ' case when i2.id is not null then @noneId else ci.status_type_id end as status_type_id, '
    set @sql = @sql + ' ci.visible, ci.archive '
    set @sql = @sql + ' from content_item ci left join ' + @table_name + ' base on ci.content_item_id = base.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id '
    set @sql = @sql + ' left join @ids2 i2 on ci.content_item_id = i2.id '
    set @sql = @sql + ' where base.content_item_id is null'
END
GO
ALTER TRIGGER [dbo].[tu_update_item] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
begin
    if not update(locked_by) and not update(splitted) and not UPDATE(not_for_replication)
    begin
        declare @content_id numeric
        declare @sql nvarchar(max), @table_name varchar(50), @async_table_name varchar(50)
        declare @items_list nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)

        declare @contents table
        (
            id numeric primary key
        )

        insert into @contents
        select distinct content_id from inserted
        where CONTENT_ID in (select CONTENT_ID from content where virtual_type = 0)

        create table #ids_with_splitted
        (
            id numeric primary key,
            new_splitted bit
        )

        declare @items table
        (
            id numeric primary key,
            splitted bit,
            not_for_replication bit,
            cancel_split bit
        )

        declare @ids [Ids], @ids2 [Ids]

        while exists (select id from @contents)
        begin
            select @content_id = id from @contents

            insert into @items
            select i.content_item_id, i.SPLITTED, i.not_for_replication, i.cancel_split from inserted i
            inner join content_item ci on i.content_item_id = ci.content_item_id
            where ci.CONTENT_ID = @content_id

            insert into @ids
            select id from @items

            insert into @ids2
            select id from @items where cancel_split = 1

            set @items_list = null
            select @items_list = coalesce(@items_list + ',', '') + convert(nvarchar, id) from @items

            set @sql = 'insert into #ids_with_splitted '
            set @sql = @sql + ' select content_item_id,'
            set @sql = @sql + ' case'
            set @sql = @sql + ' when curr_weight < front_weight and is_workflow_async = 1 then 1'
            set @sql = @sql + ' when curr_weight = workflow_max_weight and delayed = 1 then 1'
            set @sql = @sql + ' else 0'
            set @sql = @sql + ' end'
            set @sql = @sql + ' as new_splitted from ('
            set @sql = @sql + ' select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, '
            set @sql = @sql + ' max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, case when i2.id is not null then 0 else ciw.is_async end as is_workflow_async, '
            set @sql = @sql + ' ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed '
            set @sql = @sql + ' from content_item ci'
            set @sql = @sql + ' inner join @ids i on i.id = ci.content_item_id'
            set @sql = @sql + ' left join @ids2 i2 on i.id = ci.content_item_id'
            set @sql = @sql + ' inner join content_' + CONVERT(nvarchar, @content_id) + ' c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID'
            set @sql = @sql + ' left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id'
            set @sql = @sql + ' left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID'
            set @sql = @sql + ' left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID'
            set @sql = @sql + ' ) as main'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY', @ids = @ids, @ids2 = @ids2

            update base set base.splitted = i.new_splitted from @items base inner join #ids_with_splitted i on base.id = i.id
            update base set base.splitted = i.splitted from content_item base inner join @items i on base.CONTENT_ITEM_ID = i.id

            insert into content_item_splitted(content_item_id)
            select id from @items base where splitted = 1 and not exists (select * from content_item_splitted cis where cis.content_item_id = base.id)

            delete from content_item_splitted where content_item_id in (
                select id from @items base where splitted = 0
            )

            set @sync_ids_list = null
            select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 0 and not_for_replication = 0
            set @async_ids_list = null
            select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 1 and not_for_replication = 0

            set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
            set @async_table_name = @table_name + '_async'

            if @sync_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            if @async_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            delete from #ids_with_splitted

            delete from @contents where id = @content_id

            delete from @items
            delete from @ids
            delete from @ids2
        end

        drop table #ids_with_splitted

    end
end
GO
ALTER TRIGGER [dbo].[ti_insert_item] ON [dbo].[CONTENT_ITEM] FOR INSERT AS
BEGIN
    declare @content_id numeric
    declare @ids_list nvarchar(max)

    declare @table_name varchar(50), @sql nvarchar(max)

    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )

    insert into @contents
    select distinct i.content_id, st.STATUS_TYPE_ID from inserted i
    inner join content c on i.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID

    declare @ids [Ids]
    declare @ids2 [Ids]
    declare @noneId numeric

    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents

        insert into @ids
        select i.content_item_id from inserted i
        where i.CONTENT_ID = @content_id and i.not_for_replication = 0

        if exists (select id from @ids)
        begin

            insert into @ids2
            select i.content_item_id from inserted i
            where i.CONTENT_ID = @content_id and i.not_for_replication = 0 and i.SCHEDULE_NEW_VERSION_PUBLICATION = 1

            set @table_name = 'content_' + convert(nvarchar, @content_id)

            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @ids, @ids2 = @ids2, @noneId = @noneId

            delete from @ids2
            delete from @ids

        end

        delete from @contents where id = @content_id

    end

END
GO

GO
exec qp_drop_existing 'qp_get_m2o_ids_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_m2o_ids_multiple]
@contentId numeric,
@fieldName nvarchar(255),
@ids [Ids] READONLY
AS
BEGIN
  declare @sql nvarchar(max)
  set @sql = 'select [' + @fieldName + '], content_item_id from content_' + CAST(@contentId as nvarchar(255)) + '_united where [' + @fieldName + '] in (select id from @ids)'
  exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids
END

GO
exec qp_drop_existing 'qp_create_content_item_versions', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_create_content_item_versions]
  @ids [Ids] READONLY,
  @last_modified_by NUMERIC
AS
BEGIN
  declare @tm datetime
  select @tm = getdate()

  declare @items table (id numeric, cnt int, last_version_id int, new_version_id int, content_id numeric, max_num int)

  insert into @items (id, cnt)
  select i.ID, count(civ.content_item_version_id) from @ids i
  left join content_item_version civ on civ.content_item_id = i.id
  group by i.ID

  --print 'init completed'


  update @items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
  from @items items
  inner join content_item ci with(nolock) on items.id = ci.CONTENT_ITEM_ID
  inner join content c on c.CONTENT_ID = ci.CONTENT_ID

  --print 'max_num updated'

  declare @delete_ids [Ids]

  insert into @delete_ids
  select content_item_version_id from
  (
  select content_item_id, content_item_version_id,
  row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
  from content_item_version civ
  where content_item_id in (select id from @ids)
  ) c
  inner join @items items
  on items.id = c.content_item_id and c.num >= items.max_num

  DELETE item_to_item_version WHERE content_item_version_id in (select id from @delete_ids)
  DELETE content_item_version WHERE content_item_version_id in (select id from @delete_ids)

  --print 'exceeded deleted'

  DECLARE @NewVersions TABLE(ID INT)

  INSERT INTO content_item_version (version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by)
  output inserted.[CONTENT_ITEM_VERSION_ID] INTO @NewVersions
  SELECT @tm, 'backup', NULL, content_item_id, @last_modified_by, modified, last_modified_by
  from content_item where CONTENT_ITEM_ID in (select id from @ids);

  --print 'versions inserted'

  update @items set new_version_id = zip.version_id
  FROM @items items
  INNER JOIN
  (
    select x.item_id, y.version_id from
    (
    select id as item_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @items
    ) x
    inner join
    (
      select id as version_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @NewVersions
    ) y
    on x.num = y.num
  ) zip
  on zip.item_id = items.id

  --print 'new versions updated'


 -- -- Get Extensions info
  declare @contentIds TABLE
  (
    id numeric
  )

  insert into @contentIds
  select convert(numeric, DATA) as ids
  from
  (
  select distinct DATA from content_data
  where CONTENT_ITEM_ID in (select id from @ids)
  and DATA is not null
  and ATTRIBUTE_ID in (
  select attribute_id from CONTENT_ATTRIBUTE where content_id in (select distinct content_id from @items) and IS_CLASSIFIER = 1)
  ) as p

  --print 'contents defined'

  declare @aggregates TABLE (content_id numeric, attribute_name nvarchar(255))
  insert into @aggregates
  select ca.content_id, ca.ATTRIBUTE_NAME from CONTENT_ATTRIBUTE ca where ca.aggregated = 1 and ca.CONTENT_ID in (select * from @contentIds)

  declare @extensions TABLE (id numeric, agg_id numeric, content_id numeric)
  declare @currentContentId numeric, @currentFieldName nvarchar(255), @sql nvarchar(max)

  while exists(select * from @aggregates)
  begin

  select @currentContentId = content_id, @currentFieldName = attribute_name from @aggregates

  set @sql = 'select [' + @currentFieldName + '], content_item_id, ' + CAST(@currentContentId as nvarchar(255)) + ' as content_id from content_' + CAST(@currentContentId as nvarchar(255)) + '_united where [' + @currentFieldName + '] in (select id from @ids)'

  insert into @extensions
  exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids

  delete from @aggregates where content_id = @currentContentId
  end

  --print 'extensions received'

  declare @main_ids TABLE
  (
  version_id numeric,
  id numeric,
  content_id numeric
  )

  insert into @main_ids
  select i.new_version_id, e.agg_id, e.content_id from @extensions e inner join @items i on e.id = i.id

  insert into @main_ids
  select i.new_version_id, i.id, i.content_id from @items i

  declare @total numeric
  select @total = count(*) from @main_ids
  print @total

  --print 'main defined'

  -- Store content item data
  INSERT INTO version_content_data (attribute_id, content_item_version_id, data, blob_data, created)
  SELECT attribute_id, m.version_id, data, blob_data, @tm
  FROM content_data cd inner join @main_ids m on cd.CONTENT_ITEM_ID = m.id

  --print 'content_data saved'


  -- Store Many-to-Many slice
  INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
  SELECT m.version_id, ca.attribute_id, linked_item_id
  FROM item_link_united AS il
  INNER JOIN content_attribute AS ca ON ca.link_id = il.link_id
  INNER JOIN content_item AS ci ON ci.content_id =  ca.content_id AND ci.content_item_id = il.item_id
  inner join @main_ids m on il.item_id = m.id

  --print 'm2m saved'

  -- Store Many-to-One data
  declare @many_to_ones table (id numeric, content_id numeric, base_content_id numeric, name nvarchar(255))
  insert into @many_to_ones (id, content_id, base_content_id, name)
  select ca.attribute_id, rca.CONTENT_ID, ca.content_id, rca.ATTRIBUTE_NAME from CONTENT_ATTRIBUTE ca
  inner join CONTENT_ATTRIBUTE rca on ca.BACK_RELATED_ATTRIBUTE_ID = rca.ATTRIBUTE_ID
  inner join @main_ids i on i.content_id = ca.CONTENT_ID

  --print 'm2o defined'

  while exists(select * from @many_to_ones)
  begin

  declare @currentFieldId numeric, @currentBaseContentId numeric
  declare @currentIds [Ids]
  select @currentFieldId = id, @currentContentId = content_id, @currentBaseContentId = base_content_id, @currentFieldName = name from @many_to_ones

  insert into @currentIds
  select id from @main_ids where content_id = @currentBaseContentId

  declare @m2o_values table (id numeric, related_id numeric)
  insert into @m2o_values
  exec qp_get_m2o_ids_multiple @currentContentId, @currentFieldName, @currentIds

  INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
  SELECT i.new_version_id, @currentFieldId, v.related_id from @m2o_values v
  inner join @items i on v.id = i.id

  delete from @m2o_values

  delete from @currentIds

  delete from @many_to_ones where id = @currentFieldId
  end

  --print 'm2o saved'

  -- Write status history log
  INSERT INTO content_item_status_history
    (content_item_id, user_id, description, created, content_item_version_id,
    system_status_type_id)
  select id, @last_modified_by, 'Record version backup has been created', @tm, new_version_id, 2
  from @items
END
GO
exec qp_drop_existing 'qp_split_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_split_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1
AS
BEGIN
  declare @contentIds [Ids], @content_id numeric
  declare @ids2 table (content_id numeric, id numeric)
  insert into @ids2 select content_id, id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID

  insert into @contentIds
  select distinct content_id from @ids2

  while exists(select * From @contentIds)
  begin
    select @content_id = id from @contentIds

    declare @ids3 [Ids]
    declare @sql nvarchar(max)
    declare @cstr nvarchar(20)

    insert into @ids3
    select id from @ids2 where content_id = @content_id

    set @cstr = cast(@content_id as nvarchar(max))

    set @sql = 'insert into content_' + @cstr + '_async select * from content_' + @cstr + ' c where content_item_id in (select id from @ids) and not exists(select * from content_' + @cstr + '_async a where a.content_item_id = c.content_item_id)'
    exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids3

    delete from @contentIds where id = @content_id
  end

  insert into item_link_async select * from item_to_item ii where l_item_id in (select id from @ids) and not exists (select * from item_link_async ila where ila.item_id = ii.l_item_id)
END
GO
exec qp_drop_existing 'qp_merge_links_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_links_multiple]
@ids [Ids] READONLY,
@force_merge bit = 0
AS
BEGIN

  declare @idsWithLinks Table (id numeric, link_id numeric)

  insert into @idsWithLinks
  select i.id, ca.link_id from @ids i
  inner join content_item ci with(nolock) on ci.CONTENT_ITEM_ID = i.ID and (SPLITTED = 1 or @force_merge = 1)
  inner join content c on ci.CONTENT_ID = c.CONTENT_ID
  inner join CONTENT_ATTRIBUTE ca on ca.CONTENT_ID = c.CONTENT_ID and link_id is not null

  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @oldIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  declare @linkIds table (link_id numeric, primary key (link_id))

  insert into @newIds (id, link_id, linked_item_id)
  select ila.item_id, ila.link_id, ila.linked_item_id from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id

  insert into @oldIds (id, link_id, linked_item_id)
  select il.item_id, il.link_id, il.linked_item_id from item_link il inner join @idsWithLinks i on il.item_id = i.id and il.link_id = i.link_id

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @oldIds t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @oldIds from @oldIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_to_item from item_to_item il inner join @oldIds i on il.l_item_id = i.id and il.link_id = i.link_id and il.r_item_id = i.linked_item_id

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data) as
  (
    select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data
    from @newIds n
      inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
      inner join content c on ci.content_id = c.content_id
      inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.linked_has_data = 0 and n.linked_attribute_id is not null

  delete item_link_async from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id


END
GO
exec qp_drop_existing 'qp_merge_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1,
@force_merge bit = 0
AS
BEGIN
  declare @ids2 [Ids], @ids2str nvarchar(max)

  insert into @ids2 select id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID and (SCHEDULE_NEW_VERSION_PUBLICATION = 1 or @force_merge = 1)

  if exists(select * From @ids2)
  begin
    exec qp_merge_links_multiple @ids2, @force_merge
    UPDATE content_item with(rowlock) set not_for_replication = 1 WHERE content_item_id in (select id from @ids2)
    UPDATE content_item with(rowlock) set SCHEDULE_NEW_VERSION_PUBLICATION = 0, MODIFIED = GETDATE(), LAST_MODIFIED_BY = @last_modified_by, CANCEL_SPLIT = 0 where CONTENT_ITEM_ID in (select id from @ids2)
    SELECT @ids2str = COALESCE(@ids2str + ',', '') + cast(id as nvarchar(20)) from @ids2
    exec qp_replicate_items @ids2str
    UPDATE content_item_schedule with(rowlock) set delete_job = 0 WHERE content_item_id in (select id from @ids2)
    DELETE FROM content_item_schedule with(rowlock) WHERE content_item_id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE child_id in (select id from @ids2)

  end
END
GO
exec qp_drop_existing 'qp_assert_num_equal', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_assert_num_equal]
@id1 int,
@id2 int,
@msg nvarchar(50)
AS
BEGIN
  declare @text nvarchar(max)
  set @text = @msg + ': '
  if @id1 = @id2
  begin
    set @text = @text + 'OK'
    print @text
  end
  else
  begin
    set @text = @text + 'Failed - %d/%d'
    raiserror(@text, 11, 1, @id1, @id2)
  end
END
GO
exec qp_drop_existing 'qp_default_link_ids', 'IsScalarFunction'
GO

CREATE function [dbo].[qp_default_link_ids](@field_id numeric)
returns nvarchar(max)
AS
BEGIN
  declare @result nvarchar(max)
  SELECT @result = COALESCE(@result + ', ', '') +  cast(ARTICLE_ID as nvarchar(20))  FROM FIELD_ARTICLE_BIND where FIELD_ID = @field_id
  return @result
END
GO
ALTER PROCEDURE [dbo].[qp_update_m2m_values]
  @xmlParameter xml
AS
BEGIN
  DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
  INSERT INTO @fieldValues
  SELECT
    ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber
     ,doc.col.value('./@id', 'int') id
     ,doc.col.value('./@linkId', 'int') linkId
     ,doc.col.value('./@value', 'nvarchar(max)') value
     ,c.SPLITTED as splitted
    FROM @xmlParameter.nodes('/items/item') doc(col)
    INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')


  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @ids table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  insert into @newIds (id, link_id, linked_item_id, splitted)
  select a.id, a.linkId, b.nstr, a.splitted from @fieldValues a cross apply dbo.SplitNew(a.value, ',') b
  where b.nstr is not null and b.nstr <> '' and b.nstr <> '0'

  insert into @ids
  select item_id, link_id, linked_item_id, f.splitted
  from item_link_async ila inner join @fieldValues f
  on ila.link_id = f.linkId and ila.item_id = f.id
  where f.splitted = 1
  union all
  select item_id, link_id, linked_item_id, f.splitted
  from item_link il inner join @fieldValues f
  on il.link_id = f.linkId and il.item_id = f.id
  where f.splitted = 0

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @ids t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @ids from @ids t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_link_async from item_link_async il inner join @fieldValues f on il.item_id = f.id and il.link_id = f.linkId
  where f.splitted = 0

  delete item_link_united_full from item_link_united_full il
  where exists(
    select * from @ids i where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
    and i.splitted = 0
  )

  delete item_link_async from item_link_async il
  inner join @ids i on il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
  where i.splitted = 1

  insert into item_link_async (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 1

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 0

  delete from @newIds where dbo.qp_is_link_symmetric(link_id) = 0;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data, splitted, has_async) as
  (
  select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data,
    ci.splitted,
    case when ila.link_id is null then 0 else 1 end as has_async
  from @newIds n
    inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
    inner join content c on ci.content_id = c.content_id
    inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.id = ila.linked_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data, linked_splitted = ext.splitted, linked_has_async = ext.has_async
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.splitted = 0 and n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.splitted = 0 and n.linked_has_data = 0 and n.linked_attribute_id is not null

  insert into item_link_async (link_id, item_id, linked_item_id)
  select n.link_id, n.linked_item_id, n.id
  from @newIds n
  where n.splitted = 0 and n.linked_splitted = 1 and n.linked_has_async = 0 and n.linked_attribute_id is not null
END
GO

GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'OPTIMIZE_FOR_HIERARCHY')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD OPTIMIZE_FOR_HIERARCHY BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_OPTIMIZE_FOR_HIERARCHY DEFAULT 0
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'IS_LOCALIZATION')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD IS_LOCALIZATION BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_IS_LOCALIZATION DEFAULT 0
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_SEPARATE_REVERSE_VIEWS')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD USE_SEPARATE_REVERSE_VIEWS BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_USE_SEPARATE_REVERSE_VIEWS DEFAULT 0
END

GO
update VE_COMMAND set NAME = 'Spellchecker' where NAME = 'SpellCheck'
update VE_COMMAND set NAME = 'SpecialChar' where NAME = 'QSpecChar'
update VE_COMMAND set NAME = 'ShowBlocks', COMMAND_IN_GROUP_ORDER = 5 where NAME = 'LineBreak'
update VE_COMMAND set ALIAS = 'Show Blocks' where NAME = 'ShowBlocks'

GO


if not exists (select * From VE_COMMAND where name = 'autoFormat')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('autoFormat', 'Format Selection', 0, 3, 0, 0, 1, 1)

if not exists (select * From VE_COMMAND where name = 'CommentSelectedRange')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('CommentSelectedRange', 'Comment Selection', 0, 3, 0, 1, 1, 1)

if not exists (select * From VE_COMMAND where name = 'UncommentSelectedRange')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('UncommentSelectedRange', 'Uncomment Selection', 0, 3, 0, 2, 1, 1)

if not exists (select * From VE_COMMAND where name = 'AutoComplete')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('AutoComplete', 'Enable\Disable HTML Tag Autocomplete', 0, 3, 0, 3, 1, 1)

GO

exec qp_update_translations 'Show Blocks', 'Отображать блоки'
exec qp_update_translations 'Format Selection', 'Форматировать выбранное'
exec qp_update_translations 'Comment Selection', 'Комментировать выбранное'
exec qp_update_translations 'Uncomment Selection', 'Раскомментировать выбранное'
exec qp_update_translations 'Enable\Disable HTML Tag Autocomplete', 'Включить/Выключить автозавершение HTML-тегов'

GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[SITE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
  ALTER TABLE [dbo].[SITE]
  ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ATTRIBUTE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
  ALTER TABLE [dbo].[CONTENT_ATTRIBUTE]
  ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

EXEC qp_update_translations 'Disable list auto wrapping (ul, ol, dl)', 'Отключить автоматическое оборачивание списков (ul, ol, dl)'
GO

GO
if not exists (select * From BACKEND_ACTION where code = 'select_child_articles')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Select Child Articles', 'Select Child Articles', N'select_child_articles', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('article'), dbo.qp_action_id('select_child_articles'), N'Select Child Articles', 80, 0)

end

if not exists (select * From BACKEND_ACTION where code = 'unselect_child_articles')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Unselect Child Articles', 'Unselect Child Articles', N'unselect_child_articles', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('article'), dbo.qp_action_id('unselect_child_articles'), N'Unselect Child Articles', 90, 0)

end

update CONTEXT_MENU_ITEM set icon = 'deselect_all.gif' where ACTION_ID = dbo.qp_action_id('unselect_child_articles')
update CONTEXT_MENU_ITEM set icon = 'select_all.gif' where ACTION_ID = dbo.qp_action_id('select_child_articles')

exec qp_update_translations 'Select Child Articles', 'Выбрать дочерние статьи'
exec qp_update_translations 'Unselect Child Articles', 'Отменить выбор дочерних статей'

GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XML_DB_UPDATE]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
  CREATE TABLE dbo.XML_DB_UPDATE (
    Id int IDENTITY,
    Applied datetime NOT NULL,
    Hash nvarchar(100) NOT NULL,
    FileName nvarchar(300) NULL,
    USER_ID int NOT NULL,
    Body nvarchar(max) NULL,
    Version nvarchar(10) NULL,
    CONSTRAINT PK_XML_DB_UPDATE PRIMARY KEY CLUSTERED (Id)
  )
  ON [PRIMARY]
  TEXTIMAGE_ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XML_DB_UPDATE_ACTIONS]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
  CREATE TABLE dbo.XML_DB_UPDATE_ACTIONS (
    Id int IDENTITY,
    UpdateId int NULL,
    Ids nvarchar(max) NOT NULL,
    ParentId int NOT NULL,
    Hash nvarchar(100) NOT NULL,
    Applied datetime NOT NULL,
    UserId int NOT NULL,
    SourceXml nvarchar(max) NOT NULL,
    ResultXml nvarchar(max) NOT NULL,
    CONSTRAINT PK_XML_DB_UPDATE_ACTIONS_Id PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_XML_DB_UPDATE_ACTIONS_UpdateId FOREIGN KEY (UpdateId) REFERENCES dbo.XML_DB_UPDATE (Id)
  )
  ON [PRIMARY]
  TEXTIMAGE_ON [PRIMARY]
END

GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'MODIFIED' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
  ALTER TABLE dbo.EXTERNAL_NOTIFICATION_QUEUE ADD
  MODIFIED DATETIME NOT NULL CONSTRAINT DF_EXTERNAL_NOTIFICATION_QUEUE_MODIFIED DEFAULT(GETDATE())
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CONTENT_ID' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
    alter table [EXTERNAL_NOTIFICATION_QUEUE]
    add [CONTENT_ID] numeric NULL
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'SITE_ID' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
    alter table [EXTERNAL_NOTIFICATION_QUEUE]
    add [SITE_ID] numeric NULL
GO

GO
ALTER TRIGGER [dbo].[tbd_user] ON [dbo].[USERS]
INSTEAD OF DELETE
AS
BEGIN

  DELETE USER_GROUP_BIND FROM USER_GROUP_BIND c inner join deleted d on c.user_id = d.user_id
  DELETE USER_DEFAULT_FILTER FROM USER_DEFAULT_FILTER f inner join deleted d on f.user_id = d.user_id

  UPDATE CONTAINER SET locked = NULL, locked_by = NULL FROM CONTAINER c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_FORM SET locked = NULL, locked_by = NULL FROM CONTENT_FORM c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_ITEM SET locked = NULL, locked_by = NULL FROM CONTENT_ITEM c inner join deleted d on c.locked_by = d.user_id
  UPDATE [OBJECT] SET locked = NULL, locked_by = NULL FROM [OBJECT] c inner join deleted d on c.locked_by = d.user_id
  UPDATE OBJECT_FORMAT SET locked = NULL, locked_by = NULL FROM OBJECT_FORMAT c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE SET locked = NULL, locked_by = NULL FROM PAGE c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE_TEMPLATE SET locked = NULL, locked_by = NULL FROM PAGE_TEMPLATE c inner join deleted d on c.locked_by = d.user_id
  UPDATE [SITE] SET locked = NULL, locked_by = NULL FROM [SITE] c inner join deleted d on c.locked_by = d.user_id

  UPDATE [SITE] SET last_modified_by = 1 FROM [SITE] c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT SET last_modified_by = 1 FROM CONTENT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM SET last_modified_by = 1 FROM CONTENT_ITEM c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_SCHEDULE SET last_modified_by = 1 FROM CONTENT_ITEM_SCHEDULE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET created_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.created_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET last_modified_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ATTRIBUTE SET last_modified_by = 1 FROM CONTENT_ATTRIBUTE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE PAGE_TEMPLATE SET last_modified_by = 1 FROM PAGE_TEMPLATE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_modified_by = 1 FROM PAGE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_assembled_by = 1 FROM PAGE c inner join deleted d on c.last_assembled_by  = d.user_id
  UPDATE OBJECT SET last_modified_by = 1 FROM OBJECT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT SET last_modified_by = 1 FROM OBJECT_FORMAT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT_VERSION SET last_modified_by = 1 FROM OBJECT_FORMAT_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE FOLDER SET last_modified_by = 1 FROM FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE FOLDER_ACCESS SET last_modified_by = 1 FROM FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER SET last_modified_by = 1 FROM CONTENT_FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER_ACCESS SET last_modified_by = 1 FROM CONTENT_FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CODE_SNIPPET SET last_modified_by = 1 FROM CODE_SNIPPET c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE STYLE SET last_modified_by = 1 FROM STYLE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE STATUS_TYPE SET last_modified_by = 1 FROM STATUS_TYPE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW SET last_modified_by = 1 FROM WORKFLOW c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE SITE_ACCESS SET last_modified_by = 1 FROM SITE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ACCESS SET last_modified_by = 1 FROM CONTENT_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_ACCESS SET last_modified_by = 1 FROM CONTENT_ITEM_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW_ACCESS SET last_modified_by = 1 FROM WORKFLOW_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE USER_GROUP SET last_modified_by = 1 FROM USER_GROUP c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE USERS SET last_modified_by = 1 FROM USERS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE NOTIFICATIONS SET last_modified_by = 1 FROM NOTIFICATIONS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ITEM_STATUS_HISTORY SET user_id = 1 WHERE user_id in (select user_id from deleted)

  UPDATE CUSTOM_ACTION SET LAST_MODIFIED_BY = 1 FROM CUSTOM_ACTION c INNER JOIN deleted d on c.LAST_MODIFIED_BY = d.[USER_ID]

  UPDATE NOTIFICATIONS SET FROM_BACKENDUSER_ID = 1 FROM NOTIFICATIONS c inner join deleted d on c.FROM_BACKENDUSER_ID = d.user_id

  UPDATE ENTITY_TYPE_ACCESS SET last_modified_by = 1 FROM ENTITY_TYPE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE ACTION_ACCESS SET last_modified_by = 1 FROM ACTION_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE DB SET last_modified_by = 1 FROM DB c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE DB SET single_user_id = NULL FROM DB c inner join deleted d on c.single_user_id = d.user_id
  UPDATE VE_PLUGIN SET last_modified_by = 1 FROM VE_PLUGIN c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_STYLE SET last_modified_by = 1 FROM VE_STYLE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_COMMAND SET last_modified_by = 1 FROM VE_COMMAND c inner join deleted d on c.last_modified_by = d.user_id

  delete users from users c inner join deleted d on c.user_id = d.user_id
END

GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ITEM' AND COLUMN_NAME = 'UNIQUE_ID')
BEGIN
  ALTER TABLE dbo.CONTENT_ITEM ADD UNIQUE_ID uniqueidentifier NOT NULL CONSTRAINT DF_CONTENT_ITEM_UNIQUE_ID DEFAULT newid()
END

if not exists(select * from sys.indexes where name = 'IX_UNIQUE_ID' and [object_id] = object_id('CONTENT_ITEM'))
begin
  CREATE UNIQUE NONCLUSTERED INDEX [IX_UNIQUE_ID] ON dbo.CONTENT_ITEM ( UNIQUE_ID )
end

GO
ALTER TRIGGER [dbo].[td_content_to_content] ON [dbo].[content_to_content] AFTER DELETE
AS
BEGIN

  declare @link_id numeric, @i numeric, @count numeric

  declare @cc table (
    id numeric identity(1,1) primary key,
    link_id numeric
  )

  insert into @cc (link_id) select d.link_id from deleted d

  set @i = 1
  select @count = count(id) from @cc

  while @i < @count + 1
  begin
    select @link_id = link_id from @cc where id = @i
    exec qp_drop_link_view @link_id
    set @i = @i + 1
  end


END

GO
if not exists (select * from APP_SETTINGS where [key] = 'CONTENT_MODIFICATION_UPDATE_INTERVAL')
  insert into APP_SETTINGS
  values ('CONTENT_MODIFICATION_UPDATE_INTERVAL', '30')
GO
ALTER PROCEDURE [dbo].[qp_replicate_items]
@ids nvarchar(max),
@attr_ids nvarchar(max) = '',
@modification_update_interval numeric = -1
-- <0   throttling with default value (setting or constant)
-- 0    no throttling
-- >0   throttling with specified value

AS
BEGIN
    set nocount on
    declare @setting_name nvarchar(255) = 'CONTENT_MODIFICATION_UPDATE_INTERVAL'
    declare @setting_value nvarchar(255)
    declare @default_modification_update_interval numeric = 30
    select @setting_value = VALUE from APP_SETTINGS where [KEY] = @setting_name
    set @default_modification_update_interval = case when @setting_value is not null and ISNUMERIC(@setting_value) = 1 then convert(numeric, @setting_value) else @default_modification_update_interval end
    set @modification_update_interval = case when @modification_update_interval < 0 then @default_modification_update_interval else @modification_update_interval end

    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric
    declare @live_modified datetime, @stage_modified datetime
    declare @live_expired bit, @stage_expired bit

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        cancel_split bit,
        delayed bit,
        status_type_id numeric,
        content_id numeric
    )

    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')

    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.cancel_split = ci.cancel_split, base.delayed = ci.schedule_new_version_publication, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID

    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )

    insert into @contents
    select distinct a.content_id, st.STATUS_TYPE_ID from @articles a
    inner join content c on a.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID


    declare @articleIds [Ids], @syncIds [Ids], @syncIds2 [Ids], @asyncIds [Ids], @asyncIds2 [Ids]
    declare @noneId numeric

    insert into @articleIds select id from @articles

    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents

        if @modification_update_interval <= 0
        begin
            set @live_expired = 1
            set @stage_expired = 1
        end
        else
        begin
            select @live_modified = live_modified, @stage_modified = stage_modified from CONTENT_MODIFICATION with(nolock) where CONTENT_ID = @content_id
            set @live_expired = case when datediff(s, @live_modified, GETDATE()) >= @modification_update_interval then 1 else 0 end
            set @stage_expired = case when datediff(s, @stage_modified, GETDATE()) >= @modification_update_interval then 1 else 0 end
        end

        insert into @syncIds select id from @articles where content_id = @content_id and splitted = 0
        insert into @asyncIds select id from @articles where content_id = @content_id and splitted = 1
        insert into @syncIds2 select id from @articles where content_id = @content_id and splitted = 0 and delayed = 1

        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @syncIds
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @asyncIds

        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'

        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @syncIds, @ids2 = @syncIds2, @noneId = @noneId

            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql

            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0, @attr_ids
        end

        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @async_table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @asyncIds, @ids2 = @asyncIds2, @noneId = @noneId

            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql

            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1, @attr_ids
        end

        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and (cancel_split = 1 or (splitted = 0 and status_type_id = @published_id)))
        begin
            if (@live_expired = 1 or @stage_expired = 1)
                update content_modification with(rowlock) set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        end
        else
        begin
            if (@stage_expired = 1)
                update content_modification with(rowlock) set stage_modified = GETDATE() where content_id = @content_id
        end


        delete from @contents where id = @content_id

        delete from @syncIds
        delete from @syncIds2
        delete from @asyncIds
    end

    update content_item set not_for_replication = 0, CANCEL_SPLIT = 0 where content_item_id in (select id from @articleIds)
END
GO
ALTER PROCEDURE [dbo].[qp_replicate]
@content_item_id numeric
AS
BEGIN
  declare @list nvarchar(30)
  set @list = convert(nvarchar, @content_item_id)
  exec qp_replicate_items @list, '', 0
END
GO
exec qp_drop_existing 'qp_update_links', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_links]
    @ids Ids READONLY,
    @id numeric,
    @link_id numeric

AS
BEGIN

    declare @ids2 Ids
    declare @ids3 Ids

    insert into @ids2
    select * from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3

        insert into item_to_item (l_item_id, r_item_id, link_id)
        select id, @id, @link_id
        from @ids3 i where not exists(
            select * from item_link il where il.item_id = i.id and link_id = @link_id and il.linked_item_id = @id
        )
    end

END
GO
exec qp_drop_existing 'qp_update_values', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_values]
    @values [Values] READONLY
AS
BEGIN

    declare @values2 [Values]
    declare @values3 [Values]

    insert into @values2
    select * from @values

    while exists(select * from @values2)
    begin
        delete from @values3

        delete top(100) from @values2 output DELETED.* into @values3

        update content_data set
        data = case when ca.attribute_type_id in (9,10) then null else v.Value end,
        blob_data = case when ca.attribute_type_id in (9,10) then v.Value else null end
        from content_data cd
        inner join @values3 v on v.ArticleId = cd.content_item_id and v.FieldId = cd.attribute_id
        inner join content_attribute ca on cd.attribute_id = ca.attribute_id

    end

END
GO
exec qp_drop_existing 'qp_fast_delete', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_fast_delete]
    @ids Ids READONLY
AS
BEGIN
    select 1 as A into #disable_td_delete_item_o2m_nullify

    declare @ids2 table (id numeric primary key)
    declare @ids3 table (id numeric primary key)

    insert into @ids2
    select id from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3
        delete content_item from content_item ci inner join @ids3 i on ci.content_item_id = i.id
    end

    drop table  #disable_td_delete_item_o2m_nullify
END
GO

GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SITE' AND COLUMN_NAME = 'EXTERNAL_DEVELOPMENT')
BEGIN
    ALTER TABLE SITE ADD EXTERNAL_DEVELOPMENT BIT NOT NULL CONSTRAINT DF_SITE_EXTERNAL_DEVELOPMENT DEFAULT 1
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SITE' AND COLUMN_NAME = 'DOWNLOAD_EF_SOURCE')
BEGIN
    ALTER TABLE SITE ADD DOWNLOAD_EF_SOURCE BIT NOT NULL CONSTRAINT DF_SITE_DOWNLOAD_EF_SOURCE DEFAULT 0
END

GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'TA_HIGHLIGHT_TYPE')
	ALTER TABLE dbo.CONTENT_ATTRIBUTE
	ADD TA_HIGHLIGHT_TYPE nvarchar(50) NULL
GO

GO
exec qp_drop_existing 'qp_all_article_search', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_all_article_search]
    @p_site_id int,
    @p_user_id int,
    @p_searchparam nvarchar(4000),
    @p_order_by nvarchar(max) = N'data.[rank] DESC',
    @p_start_row int = 0,
    @p_page_size int = 0,
    @p_item_id int = null,

    @total_records int OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    declare @is_admin bit
    select @is_admin = dbo.qp_is_user_admin(@p_user_id)

    -- set number of start record by default
    IF (@p_start_row <= 0)
        BEGIN
            SET @p_start_row = 1
        END

    -- set number of finish record
    DECLARE @p_end_row AS int
    SET @p_end_row = @p_start_row + @p_page_size - 1

    -- make a query for subset of contents which enabled to access
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    if @is_admin = 0
    begin
        EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @p_user_id,
                @group_id = 0,
                @start_level = 1,
                @end_level = 4,
                @entity_name = 'content',
                @parent_entity_name = 'site',
                @parent_entity_id = @p_site_id,
                @SQLOut = @security_sql OUTPUT
    end

    -- count all records
    declare @paramdef nvarchar(4000);
    declare @query nvarchar(4000);

    create table #temp
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    create table #temp2
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    set @query = 'insert into #temp' + CHAR(13)
        + ' select content_item_id, weight, attribute_id, priority from ' + CHAR(13)
        + ' (select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority, ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number ' + CHAR(13)
        + ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
        + ' inner join content_data cd on ft.[key] = cd.content_data_id) as c where c.number = 1 order by weight desc ' + CHAR(13)
    print @query

    exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam

    IF @p_item_id is not null
    begin
        set @query = 'if not exists (select * from #temp where content_item_id = ' + cast(@p_item_id as varchar(20)) + ') insert into #temp' + CHAR(13)
        set @query = @query + ' select ' + cast(@p_item_id as varchar(20)) + ', 0, 0, 1 ' + CHAR(13)
		print @query
        exec sp_executesql @query
    end

    set @paramdef = '@site_id int';
    if @is_admin = 0
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c0 on c0.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)
            + ' inner join (' + @security_sql + ') c on c.CONTENT_ID = c0.CONTENT_ID where c0.site_id = @site_id' + CHAR(13)

        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end
    else
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c on c.CONTENT_ID = ci.CONTENT_ID where c.site_id = @site_id' + CHAR(13)
        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end

    select @total_records = count(distinct content_item_id) from #temp2

    -- main query
    declare @query_template nvarchar(4000);
    set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
        + ' (select ROW_NUMBER() OVER (ORDER BY [priority] DESC, <$_order_by_$>) AS ROW, ' + CHAR(13)
        + ' 	ci.CONTENT_ID as ParentId, ' + CHAR(13)
        + ' 	data.CONTENT_ITEM_ID as Id, ' + CHAR(13)
        + ' 	data.ATTRIBUTE_ID as FieldId, ' + CHAR(13)
        + ' 	attr.ATTRIBUTE_TYPE_ID as FieldTypeId, ' + CHAR(13)
        + ' 	c.CONTENT_NAME as ParentName, ' + CHAR(13)
        + ' 	st.STATUS_TYPE_NAME as StatusName, ' + CHAR(13)
        + ' 	ci.CREATED as Created, ' + CHAR(13)
        + ' 	ci.MODIFIED as Modified, ' + CHAR(13)
        + ' 	usr.[LOGIN] as LastModifiedByUser, ' + CHAR(13)
        + ' 	data.[rank] as Rank, ' + CHAR(13)
        + ' 	data.[priority] as [priority], ' + CHAR(13)
		+ '		ci.ARCHIVE as Archive' + CHAR(13)
        + '   from #temp2 data ' + CHAR(13)
        + '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
        + '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
        + '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID and c.site_id = @site_id ' + CHAR(13)
        + '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
        + '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
        + ' ) ' + CHAR(13)
        + ' select ' + CHAR(13)
        + ' 	ParentId, ' + CHAR(13)
        + ' 	ParentName, ' + CHAR(13)
        + ' 	Id, ' + CHAR(13)
        + ' 	FieldId, ' + CHAR(13)
        + '		(case when FieldTypeId in (9, 10) THEN cd.BLOB_DATA ELSE cd.DATA END) as Text, ' + CHAR(13)
        + ' 	dbo.qp_get_article_title_func(Id, ParentId) as Name, ' + CHAR(13)
        + ' 	StatusName, ' + CHAR(13)
        + ' 	pdc.Created, ' + CHAR(13)
        + ' 	pdc.Modified, ' + CHAR(13)
        + ' 	LastModifiedByUser, ' + CHAR(13)
        + ' 	Rank, ' + CHAR(13)
		+ '		pdc.Archive ' + CHAR(13)
        + ' from PAGED_DATA_CTE pdc ' + CHAR(13)
        + ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
        + ' where ROW between @start_row and @end_row order by row asc';


    declare @sortExp nvarchar(4000);
    set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;
    set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);
    set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
    print @query
    EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;

    drop table #temp
    drop table #temp2
END
GO

GO
exec qp_drop_existing 'qp_update_m2o', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2o]
@id numeric,
@fieldId numeric,
@value nvarchar(max),
@update_archive bit = 1
AS
BEGIN
	declare @ids table (id numeric primary key)
	declare @new_ids table (id numeric primary key);
	declare @cross_ids table (id numeric primary key);

	declare @contentId numeric, @fieldName nvarchar(255)
	select @contentId = content_id, @fieldName = attribute_name from CONTENT_ATTRIBUTE where ATTRIBUTE_ID = @fieldId

	insert into @ids
	exec qp_get_m2o_ids @contentId, @fieldName, @id

	select content_item_id
	from content_data where ATTRIBUTE_ID = @fieldId and DATA = CAST(@id as nvarchar)

	insert into @new_ids select * from dbo.split(@value, ',');

	insert into @cross_ids select t1.id from @ids t1 inner join @new_ids t2 on t1.id = t2.id
	delete from @ids where id in (select id from @cross_ids);
	delete from @new_ids where id in (select id from @cross_ids);

	if @update_archive = 0
	begin
		delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
	end

	insert into #resultIds(id, attribute_id, to_remove)
	select id, @fieldId as attribute_id, 1 as to_remove from @ids
	union all
	select id, @fieldId as attribute_id, 0 as to_remove from @new_ids
END

GO
exec qp_drop_existing 'qp_GetPermittedItemsAsQuery', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_GetPermittedItemsAsQuery]
(
  @user_id numeric(18,0)=0,
  @group_id numeric(18,0)=0,
  @start_level int=2,
  @end_level int=4,
  @entity_name varchar(100)='content_item',
  @parent_entity_name varchar(100)='',
  @parent_entity_id numeric(18,0)=0,
  @SQLOut varchar(8000) OUTPUT
)
AS

SET NOCOUNT ON

Declare @sPermissionTable varchar(200)
Declare @sHide varchar(50)
Declare @NewLine char(2)
Declare @sUnion varchar(20)
Declare @sSelectUser varchar(200)
Declare @sSelectGroup varchar(8000)
Declare @sSQL varchar(8000)
Declare @srGroupInList varchar (30)
Declare @srLevelIncrement varchar (30)
Declare @sTemp varchar(8000)
Declare @sWhereParentEntity varchar (8000)
Declare @sDefaultSQL varchar (8000)
Declare @sGroupBy varchar (200)
Declare @intIncrement int
Declare @CurrentLevelAddition int
Declare @sSQLStart varchar(300)
Declare @sSQLEnd varchar (600)

/***********************************/
/**** Declare Table Variables   ****/
/***********************************/
declare @ChildGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @ParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @UsedGroups table
(
	group_id numeric(18,0)
)

declare @TempParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)
/***********************************/

select @NewLine = CHAR(13) + CHAR(10)
Select @intIncrement = 10
Select @CurrentLevelAddition = 0
Select @sSQLStart = ' select ' + @entity_name + '_id, cast(min(pl) as int)%10 as permission_level, max(hide) as hide from ('
Select @sSQLEnd = ') as qp_zzz group by qp_zzz.' + @entity_name + '_id HAVING cast(min(pl) as int)%10 >= ' + Cast(@start_level AS varchar) + ' AND cast(min(pl) as int)%10 <= ' + Cast(@end_level AS varchar)

Select @sGroupBy =  ' group by ' + @entity_name + '_id '
Select @sWhereParentEntity = ''
select @sPermissionTable = @entity_name + '_access_PermLevel'

if @parent_entity_name != '' and @parent_entity_id != 0
Begin
   Select @sPermissionTable = @sPermissionTable + '_' + @parent_entity_name
   Select @sWhereParentEntity = ' and ' + @parent_entity_name+ '_id=' + Cast(@parent_entity_id As varchar) + ' '
End

if @entity_name = 'content'
	set @sHide = ', MAX(CONVERT(int, hide)) as hide'
else
	set @sHide = ', 0 as hide'

select @sSQL = ''
select @sTemp = null
Select @srGroupInList = '<@_group_in_list_@>'
Select @srLevelIncrement = '<@_increment_level_@>'
select @sUnion = @NewLine + ' Union All ' + @NewLine
select @sSelectUser = ' select ' + @entity_name + '_id, max(permission_level) as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where user_id=' + Cast(@user_id AS varchar) + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sSelectGroup = ' select ' + @entity_name + '_id, max(permission_level) + ' + @srLevelIncrement + ' as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where group_id in (' + @srGroupInList + ')' + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sDefaultSQL = ' select 0 as ' + @entity_name + '_id, 0 as permission_level' + @sHide + ' from ' + @sPermissionTable


if @user_id > 0
Begin
   Select @sSQL = @sSelectUser + @sGroupBy
   insert into @ChildGroups (group_id) select distinct group_id from user_group_bind where user_id = @user_id
   Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
End

if @group_id > 0 AND @user_id <= 0
Begin
   insert into @ChildGroups(group_id) values (@group_id)
End

if (select count(*) from @ChildGroups) = 0
Begin
   if @sSQL != '' Select @SQLOut = @sSQL
   else Select @SQLOut = @sDefaultSQL
   return
End

SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @ChildGroups
if @sSQL != '' Select @sSQL = @sSQL + @sUnion
Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
Select @sSQL = @sSQL + @sGroupBy

insert into @UsedGroups(group_id) select group_id from @ChildGroups

WHILE 1=1
BEGIN
    Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
    select @sTemp = null
	insert into @ParentGroups (group_id) select distinct gtg.parent_group_id from group_to_group gtg inner join @ChildGroups cg on gtg.child_group_id = cg.group_id
    if (select count(*) from @ParentGroups) = 0 BREAK

    /* need to check that parent groups are not appearing in child groups */
    insert into @TempParentGroups (group_id) select pg.group_id from @ParentGroups pg where pg.group_id not in(select cg.group_id from @ChildGroups cg) and pg.group_id not in (select group_id from @UsedGroups)
    if (select count(*) from @TempParentGroups) != 0
    Begin
		SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @TempParentGroups
		if @sSQL != '' Select @sSQL = @sSQL + @sUnion
		Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
		Select @sSQL = @sSQL + @sGroupBy
        insert into @UsedGroups (group_id) select group_id from @TempParentGroups
    End

    delete @ChildGroups
    delete @TempParentGroups
    insert into @ChildGroups (group_id) select (group_id) from @ParentGroups
    delete @ParentGroups
    CONTINUE
END

Select @SQLOut = @sSQLStart + @sSQL + @sSQLEnd
return

GO

GO
exec qp_drop_existing 'tbd_delete_content', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tbd_delete_content] ON [dbo].[CONTENT] INSTEAD OF DELETE
AS
BEGIN
	create table #disable_td_delete_item(id numeric)

	UPDATE content_attribute SET related_attribute_id = NULL
	where related_attribute_id in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)

	UPDATE content_attribute SET CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = 0
	where CLASSIFIER_ATTRIBUTE_ID in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	UPDATE content_attribute SET TREE_ORDER_FIELD = NULL
	where TREE_ORDER_FIELD in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	update content_attribute set link_id = null where link_id in (select link_id from content_link cl
	inner join deleted d on cl.content_id = d.content_id)

	delete USER_DEFAULT_FILTER from USER_DEFAULT_FILTER f
	inner join deleted d on d.content_id = f.CONTENT_ID

	delete content_to_content from content_to_content cc
	inner join deleted d on d.content_id = cc.r_content_id or d.content_id = cc.l_content_id

	delete container from container c
	inner join deleted d on d.content_id = c.content_id

	delete content_form from content_form cf
	inner join deleted d on d.content_id = cf.content_id

	delete content_item from content_item ci
	inner join deleted d on d.content_id = ci.content_id

	delete content_tab_bind from content_tab_bind ctb
	inner join deleted d on d.content_id = ctb.content_id

	delete [ACTION_CONTENT_BIND] from [ACTION_CONTENT_BIND] acb
	inner join deleted d on d.content_id = acb.content_id

	delete ca from CONTENT_ATTRIBUTE ca
	inner join CONTENT_ATTRIBUTE cad on ca.BACK_RELATED_ATTRIBUTE_ID = cad.ATTRIBUTE_ID
	inner join deleted c on cad.CONTENT_ID = c.CONTENT_ID

	delete content from content c inner join deleted d on c.content_id = d.content_id

	drop table #disable_td_delete_item
END

GO

GO
IF NOT EXISTS (  SELECT * FROM   sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_DPC')
	ALTER TABLE [dbo].[DB] ADD [USE_DPC] [bit] NOT NULL DEFAULT ((0))

GO
DECLARE @articles_with_wrong_statuses TABLE (
Site_ID int,
CONTENT_ID int,
STATUS_TYPE_ID int,
CONTENT_ITEM_ID int
)

INSERT INTO @articles_with_wrong_statuses
	SELECT c.Site_ID, c.CONTENT_ID, ci.STATUS_TYPE_ID, ci.CONTENT_ITEM_ID FROM [dbo].[CONTENT_ITEM] ci
	INNER JOIN [dbo].[CONTENT] c ON ci.CONTENT_ID = c.CONTENT_ID
	WHERE  ci.STATUS_TYPE_ID NOT IN (
					SELECT STATUS_TYPE_ID
					FROM [dbo].[STATUS_TYPE] i_st
					WHERE i_st.SITE_ID = c.SITE_ID
					)
IF EXISTS (SELECT * FROM @articles_with_wrong_statuses)
BEGIN
DECLARE @statuses_names TABLE (
	STATUS_TYPE_NAME nvarchar(255),
	STATUS_TYPE_ID int,
	NEW_SITE int
)

INSERT INTO @statuses_names
	SELECT st1.STATUS_TYPE_NAME, st1.STATUS_TYPE_ID as old_status, st1.SITE_ID as old_site from [dbo].[STATUS_TYPE] st1
	WHERE st1.STATUS_TYPE_ID IN (SELECT STATUS_TYPE_ID FROM @articles_with_wrong_statuses)

;WITH rel_betw_statuses AS (
	SELECT new_status_id, new_site_id, old_status_id FROM (
	SELECT st.SITE_ID AS new_site_id, st.STATUS_TYPE_ID AS new_status_id, stn.STATUS_TYPE_NAME, stn.STATUS_TYPE_ID AS old_status_id
		FROM [dbo].[STATUS_TYPE] st
		INNER JOIN @statuses_names stn ON st.STATUS_TYPE_NAME = stn.STATUS_TYPE_NAME
	) AS nsi
	WHERE nsi.NEW_SITE_ID IN (SELECT SITE_ID FROM @articles_with_wrong_statuses)
)

UPDATE CONTENT_ITEM
	SET STATUS_TYPE_ID = (
		SELECT NEW_STATUS_ID FROM CONTENT_ITEM AS ci
		INNER JOIN @articles_with_wrong_statuses AS t ON t.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
		INNER JOIN rel_betw_statuses AS rbs ON t.SITE_ID = rbs.new_site_id AND ci.STATUS_TYPE_ID = rbs.old_status_id
		WHERE  [dbo].[CONTENT_ITEM].CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
)
WHERE CONTENT_ITEM_ID IN (SELECT CONTENT_ITEM_ID FROM @articles_with_wrong_statuses)
END



DECLARE @workflow_rules_ids TABLE (
WORKFLOW_RULE_ID int
)

INSERT INTO @workflow_rules_ids
	SELECT workflow_rule_id FROM [dbo].[workflow_rules] wr
		WHERE SUCCESSOR_STATUS_ID NOT IN (
			SELECT STATUS_TYPE_ID FROM [dbo].[STATUS_TYPE] st
			INNER JOIN [dbo].[workflow] w ON st.SITE_ID = w.SITE_ID
			WHERE wr.WORKFLOW_ID = w.WORKFLOW_ID
	)

IF EXISTS (SELECT * FROM @workflow_rules_ids)
BEGIN
UPDATE [dbo].[WORKFLOW_RULES]
SET SUCCESSOR_STATUS_ID = (
	SELECT st2.STATUS_TYPE_ID
		FROM [dbo].[STATUS_TYPE] st1
			INNER JOIN [dbo].[STATUS_TYPE] st2 on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME
			INNER JOIN [dbo].[workflow] w on st2.SITE_ID = w.SITE_ID
		WHERE w.WORKFLOW_ID = [dbo].[workflow_rules].WORKFLOW_ID AND [dbo].[WORKFLOW_RULES].SUCCESSOR_STATUS_ID = st1.STATUS_TYPE_ID
	)
WHERE WORKFLOW_RULE_ID IN (SELECT WORKFLOW_RULE_ID FROM @workflow_rules_ids)
END

GO
exec qp_drop_existing 'tu_item_to_item', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tu_item_to_item] ON [dbo].[item_to_item] AFTER UPDATE
AS
BEGIN
declare @links table
	(
		id numeric primary key,
		item_id numeric
	)

	insert into @links
	select distinct link_id, l_item_id from inserted

	declare @link_id numeric, @item_id numeric , @query nvarchar(max)

	while exists(select id from @links)
	begin

		select @link_id = id from @links
		select 	@item_id = item_id from @links

		declare @table_name nvarchar(50), @table_name_rev nvarchar(50)
		set @table_name = 'item_link_' + cast(@link_id as varchar)
		set @table_name_rev = 'item_link_' + cast(@link_id as varchar) + '_rev'

		declare @linked_item numeric
		select @linked_item = l_item_id from inserted

		set @query = 'update ' + @table_name + ' set linked_id = @linked_item where id = @item_id'
		print @query
		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

		set @query = 'update ' + @table_name_rev + ' set linked_id = @linked_item where id = @item_id'
		print @query
		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

		delete from @links where id = @link_id
	end
END
GO

GO
ALTER TRIGGER [dbo].[tiu_content_fill] ON [dbo].[CONTENT_DATA] FOR INSERT, UPDATE AS
BEGIN
  set nocount on
  IF EXISTS(select content_data_id from inserted where not_for_replication = 0)
    BEGIN
        IF NOT (UPDATE(not_for_replication) AND EXISTS(select content_data_id from deleted))
        BEGIN

            update content_item set modified = getdate() where content_item_id in (select content_item_id from deleted where not_for_replication = 0)

            DECLARE @attribute_id NUMERIC, @attribute_type_id NUMERIC, @attribute_size NUMERIC, @default_value NVARCHAR(255), @attribute_name NVARCHAR(255), @content_id NUMERIC
            DECLARE @table_name nvarchar(50), @sql NVARCHAR(max), @ids_list nvarchar(max), @async_ids_list nvarchar(max)

            declare @ca table
            (
                id numeric primary key
            )

            insert into @ca
            select distinct attribute_id from inserted


            declare @ids table
            (
                id numeric primary key,
                splitted bit
            )

            while exists(select id from @ca)
            begin

                select @attribute_id = id from @ca

                select @attribute_name = attribute_name, @attribute_type_id = attribute_type_id, @attribute_size = attribute_size, @default_value = default_value, @content_id = content_id
                from content_attribute
                where ATTRIBUTE_ID = @attribute_id

                insert into @ids
                select i.content_item_id, ci.SPLITTED from inserted i
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = @attribute_id and ci.not_for_replication = 0 and c.virtual_type = 0

                set @ids_list = null
                select @ids_list = coalesce(@ids_list + ', ', '') + CONVERT(nvarchar, id) from @ids where splitted = 0
                set @async_ids_list = null
                select @async_ids_list = coalesce(@async_ids_list + ', ', '') + CONVERT(nvarchar, id) from @ids where splitted = 1

                set @table_name = 'content_' + CONVERT(nvarchar, @content_id)

                if @ids_list <> ''
                begin
                    exec qp_get_update_column_sql @table_name, @ids_list, @attribute_id, @attribute_type_id, @attribute_size, @default_value, @attribute_name, @sql = @sql out
                    print @sql
                    exec sp_executesql @sql
                end

                if @async_ids_list <> ''
                begin
                    set @table_name = @table_name + '_async'
                    exec qp_get_update_column_sql @table_name, @async_ids_list, @attribute_id, @attribute_type_id, @attribute_size, @default_value, @attribute_name, @sql = @sql out
                    print @sql
                    exec sp_executesql @sql
                end

                delete from @ca where id = @attribute_id

                delete from @ids
            end --while
        end --if
    end --if
END
GO
IF NOT EXISTS (  SELECT * FROM   sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT]') AND name = 'FOR_REPLICATION')
    ALTER TABLE [dbo].[CONTENT] ADD [FOR_REPLICATION] [bit] NOT NULL DEFAULT ((1))
GO
exec qp_drop_existing 'qp_update_m2m', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2m]
@id numeric,
@linkId numeric,
@value nvarchar(max),
@splitted bit = 0,
@update_archive bit = 1
AS
BEGIN
    declare @newIds table (id numeric primary key, attribute_id numeric null, has_data bit, splitted bit, has_async bit null)
    declare @ids table (id numeric primary key)
    declare @crossIds table (id numeric primary key)

    insert into @newIds (id) select * from dbo.split(@value, ',')

    IF @splitted = 1
        insert into @ids select linked_item_id from item_link_async where link_id = @linkId and item_id = @id
    ELSE
        insert into @ids select linked_item_id from item_link where link_id = @linkId and item_id = @id

    insert into @crossIds select t1.id from @ids t1 inner join @newIds t2 on t1.id = t2.id
    delete from @ids where id in (select id from @crossIds)
    delete from @newIds where id in (select id from @crossIds)

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    IF @splitted = 0
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id

    IF @splitted = 1
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)
    ELSE
        DELETE FROM item_link_united_full WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)

    IF @splitted = 1
        INSERT INTO item_link_async (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds
    ELSE
        INSERT INTO item_link (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds

    if dbo.qp_is_link_symmetric(@linkId) = 1
    begin

        with newItems (id, attribute_id, has_data, splitted, has_async) as
        (
        select
            n.id, ca.attribute_id,
            case when cd.content_item_id is null then 0 else 1 end as has_data,
            ci.splitted,
            case when ila.link_id is null then 0 else 1 end as has_async
        from @newIds n
            inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            inner join content c on ci.content_id = c.content_id
            inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = @linkId
            left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            left join item_link_async ila on @linkId = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @id
        )
        update @newIds
        set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
        from @newIds n inner join newItems ext on n.id = ext.id

        if @splitted = 0
        begin
            update content_data set data = @linkId
            from content_data cd
            inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
            where n.has_data = 1

            insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
            select n.id, n.attribute_id, @linkId
            from @newIds n
            where n.has_data = 0 and n.attribute_id is not null

            insert into item_link_async(link_id, item_id, linked_item_id)
            select @linkId, n.id, @id
            from @newIds n
            where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
        end
    end
END
GO

exec qp_drop_existing 'qp_update_m2m_values', 'IsProcedure'
GO


CREATE  PROCEDURE [dbo].[qp_update_m2m_values]
  @xmlParameter xml
AS
BEGIN
  DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
  INSERT INTO @fieldValues
  SELECT
    ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber
     ,doc.col.value('./@id', 'int') id
     ,doc.col.value('./@linkId', 'int') linkId
     ,doc.col.value('./@value', 'nvarchar(max)') value
     ,c.SPLITTED as splitted
    FROM @xmlParameter.nodes('/items/item') doc(col)
    INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')


  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @ids table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  insert into @newIds (id, link_id, linked_item_id, splitted)
  select a.id, a.linkId, b.nstr, a.splitted from @fieldValues a cross apply dbo.SplitNew(a.value, ',') b
  where b.nstr is not null and b.nstr <> '' and b.nstr <> '0'

  insert into @ids
  select item_id, link_id, linked_item_id, f.splitted
  from item_link_async ila inner join @fieldValues f
  on ila.link_id = f.linkId and ila.item_id = f.id
  where f.splitted = 1
  union all
  select item_id, link_id, linked_item_id, f.splitted
  from item_link il inner join @fieldValues f
  on il.link_id = f.linkId and il.item_id = f.id
  where f.splitted = 0

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @ids t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @ids from @ids t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_link_async from item_link_async il inner join @fieldValues f on il.item_id = f.id and il.link_id = f.linkId
  where f.splitted = 0

  delete item_link_united_full from item_link_united_full il
  where exists(
    select * from @ids i where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
    and i.splitted = 0
  )

  delete item_link_async from item_link_async il
  inner join @ids i on il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
  where i.splitted = 1

  insert into item_link_async (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 1

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 0

  delete from @newIds where dbo.qp_is_link_symmetric(link_id) = 0;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data, splitted, has_async) as
  (
  select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data,
    ci.splitted,
    case when ila.link_id is null then 0 else 1 end as has_async
  from @newIds n
    inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
    inner join content c on ci.content_id = c.content_id
    inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.id = ila.linked_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data, linked_splitted = ext.splitted, linked_has_async = ext.has_async
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.splitted = 0 and n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.splitted = 0 and n.linked_has_data = 0 and n.linked_attribute_id is not null

  insert into item_link_async(link_id, item_id, linked_item_id)
  select n.link_id, n.linked_item_id, n.id
  from @newIds n
  where n.splitted = 0 and n.linked_splitted = 1 and n.linked_has_async = 0 and n.linked_attribute_id is not null
END
GO


exec qp_drop_existing 'qp_merge_links_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_links_multiple]
@ids [Ids] READONLY,
@force_merge bit = 0
AS
BEGIN

  declare @idsWithLinks Table (id numeric, link_id numeric)

  insert into @idsWithLinks
  select i.id, ca.link_id from @ids i
  inner join content_item ci with(nolock) on ci.CONTENT_ITEM_ID = i.ID and (SPLITTED = 1 or @force_merge = 1)
  inner join content c on ci.CONTENT_ID = c.CONTENT_ID
  inner join CONTENT_ATTRIBUTE ca on ca.CONTENT_ID = c.CONTENT_ID and link_id is not null

  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @oldIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  declare @linkIds table (link_id numeric, primary key (link_id))

  insert into @newIds (id, link_id, linked_item_id)
  select ila.item_id, ila.link_id, ila.linked_item_id from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id

  insert into @oldIds (id, link_id, linked_item_id)
  select il.item_id, il.link_id, il.linked_item_id from item_link il inner join @idsWithLinks i on il.item_id = i.id and il.link_id = i.link_id

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @oldIds t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @oldIds from @oldIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_to_item from item_to_item il inner join @oldIds i on il.l_item_id = i.id and il.link_id = i.link_id and il.r_item_id = i.linked_item_id

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data) as
  (
    select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data
    from @newIds n
      inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
      inner join content c on ci.content_id = c.content_id
      inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.linked_has_data = 0 and n.linked_attribute_id is not null

  delete item_link_async from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id


END
;
GO

GO

