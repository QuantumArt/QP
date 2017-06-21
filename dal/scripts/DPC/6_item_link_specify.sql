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

exec qp_drop_existing 'tu_item_to_item', 'IsTrigger'
go

CREATE TRIGGER [dbo].[tu_item_to_item] ON [dbo].[item_to_item] AFTER UPDATE
AS
BEGIN
	if update(l_item_id) or update(r_item_id)
	begin

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
END
;
GO
