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
	
	insert into item_to_item
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

	insert into item_link_async
	select link_id, id, linked_item_id from @newIds 
	where splitted = 1

	insert into item_to_item
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

