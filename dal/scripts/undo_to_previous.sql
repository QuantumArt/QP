ALTER PROCEDURE [dbo].[qp_merge_links] 
@content_item_id numeric
AS 
declare @splitted bit
BEGIN
	select @splitted = splitted from content_item with(nolock) where content_item_id = @content_item_id
	if @splitted = 1
	BEGIN
		
		declare @newIds table (link_id numeric, id numeric, attribute_id numeric null, has_data bit null, splitted bit null, has_async bit null, primary key (link_id, id))
		declare @oldIds table (link_id numeric, id numeric, primary key (link_id, id))
		declare @crossIds table (link_id numeric, id numeric, primary key (link_id, id))

		insert into @newIds (link_id, id) select link_id, linked_item_id from item_link_async where item_id = @content_item_id
		insert into @oldIds select link_id, linked_item_id from item_link where item_id = @content_item_id
		insert into @crossIds select t1.link_id, t1.id from @oldIds t1 inner join @newIds t2 on t1.id = t2.id and t1.link_id = t2.link_id
		
		delete @oldIds from @oldIds i inner join @crossIds ci on i.link_id = ci.link_id and i.id = ci.id 
		delete @newIds from @newIds i inner join @crossIds ci on i.link_id = ci.link_id and i.id = ci.id
		
		delete item_to_item from item_to_item ii inner join @oldIds i on i.link_id = ii.link_id and i.id = ii.r_item_id
		where ii.l_item_id = @content_item_id
		
		insert into item_to_item (link_id, l_item_id, r_item_id)
		select link_id, @content_item_id, id from @newIds;
		
		with newItems (link_id, id, attribute_id, has_data, splitted, has_async) as
		(
		select 
			n.link_id, n.id, ca.attribute_id, 
			case when cd.content_item_id is null then 0 else 1 end as has_data, 
			ci.splitted, 
			case when ila.link_id is null then 0 else 1 end as has_async
		from @newIds n
			inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
			inner join content c on ci.content_id = c.content_id
			inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
			inner join content_to_content c2c on c2c.link_id = n.link_id
			left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
			left join item_link_async ila on n.link_id = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @content_item_id
			where c2c.symmetric = 1
		)
		update @newIds 
		set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
		from @newIds n inner join newItems ext on n.link_id = ext.link_id and n.id = ext.id
		
		update content_data set data = n.link_id 
		from content_data cd 
		inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
		where n.has_data = 1
		
		insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
		select n.id, n.attribute_id, n.link_id
		from @newIds n where n.has_data = 0 and n.attribute_id is not null
		
		insert into item_link_async(link_id, item_id, linked_item_id)
		select n.link_id, n.id, @content_item_id
		from @newIds n 
		where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
		
		delete from item_link_async with(rowlock) where item_id = @content_item_id
	END
END

GO

if exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'APP_SETTINGS')
begin
	drop table dbo.[APP_SETTINGS] 
end
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_IN_CHILD_CONTENT_FILTER' and TABLE_NAME = 'CONTENT_ATTRIBUTE')
begin
	alter table [CONTENT_ATTRIBUTE] DROP CONSTRAINT [DF_CONTENT_ATTRIBUTE_USE_IN_CHILD_CONTENT_FILTER]
	alter table [CONTENT_ATTRIBUTE] DROP COLUMN [USE_IN_CHILD_CONTENT_FILTER]  
end
GO

exec qp_drop_existing 'qp_delete_single_link', 'IsProcedure'
GO

exec qp_drop_existing 'qp_insert_single_link', 'IsProcedure'
GO

if exists (select * From information_schema.columns where table_name = 'BACKEND_ACTION' and column_name = 'ADDITIONAL_CONTROLLER_ACTION_URL') 
begin
	ALTER TABLE BACKEND_ACTION DROP COLUMN ADDITIONAL_CONTROLLER_ACTION_URL
end
GO

if exists (select * From information_schema.columns where table_name = 'BACKEND_ACTION' and column_name = 'ENTITY_LIMIT') 
begin
	ALTER TABLE BACKEND_ACTION DROP COLUMN ENTITY_LIMIT 
end
GO

delete from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('multiple_publish_articles')
delete from BACKEND_ACTION where code = 'multiple_publish_articles'
delete from action_type where code = 'multiple_update'

update BACKEND_ACTION set allow_search = 0 where code = 'list_archive_article'
GO

ALTER PROCEDURE [dbo].[qp_update_m2m]
@id numeric,
@linkId numeric,
@value nvarchar(max),
@splitted bit = 0
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

	IF @splitted = 0
		DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id

	IF @splitted = 1
		DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)
	ELSE
		DELETE FROM item_link_united_full WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)

	IF @splitted = 1
		INSERT INTO item_link_async SELECT @linkId, @id, id from @newIds
	ELSE
		INSERT INTO item_to_item SELECT @linkId, @id, id from @newIds

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

ALTER PROCEDURE [dbo].[qp_update_m2o]
@id numeric,
@fieldId numeric,
@value nvarchar(max)
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
	from content_data where ATTRIBUTE_ID = @fieldId and DATA = @id

	insert into @new_ids select * from dbo.split(@value, ',');

	insert into @cross_ids select t1.id from @ids t1 inner join @new_ids t2 on t1.id = t2.id
	delete from @ids where id in (select id from @cross_ids);
	delete from @new_ids where id in (select id from @cross_ids);

	insert into #resultIds(id, attribute_id, to_remove)
	select id, @fieldId as attribute_id, 1 as to_remove from @ids
	union all
	select id, @fieldId as attribute_id, 0 as to_remove from @new_ids
END
GO

ALTER FUNCTION [dbo].[qp_get_display_fields] 
(	
  @content_id numeric(18,0), 
  @with_relation_field BIT = 0
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT  ATTRIBUTE_ID, attribute_name,
	  CASE attribute_type_id 
		WHEN 10 THEN 0 
		WHEN 9  THEN 0
		ELSE 1
	  END AS is_blob,
	  view_in_list,
	  attribute_order
	FROM content_attribute 
	WHERE content_id = @content_id 
	AND (attribute_type_id <> 11 AND @with_relation_field = 0 OR @with_relation_field = 1)	
)

GO

ALTER PROCEDURE [dbo].[qp_get_article_title] 
@content_item_id numeric, 
@content_id numeric, 
@title nvarchar(255) output
AS
BEGIN
	declare @titleName NVARCHAR(255), @sql nvarchar(2000)  
	SELECT @titleName = dbo.qp_get_display_field(@content_id, default)

	SET @sql = 'SELECT @title = CAST([' + @titleName + '] AS NVARCHAR (255)) FROM content_' + cast(@content_id as varchar) + '_united' +
		' WHERE content_item_id =' + cast(@content_item_id as varchar)

	exec sp_executesql @sql, N'@title nvarchar(255) out', @title out
END
GO

delete from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_virtual_article') and ACTION_ID = dbo.qp_action_id('multiple_export_virtual_article')
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('export_virtual_articles')
delete from BACKEND_ACTION where code = 'multiple_export_virtual_article'
delete from BACKEND_ACTION where code = 'export_virtual_articles'

exec qp_drop_existing 'qp_update_m2m_values', 'IsProcedure'
GO

ALTER PROCEDURE [dbo].[qp_get_paged_data]
	@select_block nvarchar(max),
	@from_block nvarchar(max),
	@where_block nvarchar(max) = '',
	@order_by_block nvarchar(max),
	@count_only bit = 0,
	@total_records int OUTPUT,
	@start_row int = 0,
	@page_size int = 0,
	
	@use_security bit = 0,
	@user_id numeric(18,0) = 0,
	@group_id numeric(18,0) = 0,
	@start_level int = 2,
	@end_level int = 4,
	@entity_name nvarchar(100),
	@parent_entity_name nvarchar(100) = '',
	@parent_entity_id numeric(18,0) = 0,
	
	@insert_key varchar(200) = '<$_security_insert_$>'
AS
BEGIN
	SET NOCOUNT ON
	
	-- Получаем фильтр по правам
	DECLARE @security_sql AS nvarchar(max)
	SET @security_sql = ''

	IF (@use_security = 1)
		BEGIN
			EXEC dbo.qp_GetPermittedItemsAsQuery
				@user_id = @user_id,
				@group_id = @group_id,
				@start_level = @start_level,
				@end_level = @end_level,
				@entity_name = @entity_name,
				@parent_entity_name = @parent_entity_name,
				@parent_entity_id = @parent_entity_id,				
				@SQLOut = @security_sql OUTPUT
				
			SET @from_block = REPLACE(@from_block, @insert_key, @security_sql)
		END
		
	-- Получаем общее количество записей
	DECLARE @sql_count AS nvarchar(max)
	
	if (@count_only = 1)
	BEGIN
		SET @sql_count = ''
		SET @sql_count = @sql_count + 'SELECT ' + CHAR(13)
		SET @sql_count = @sql_count + '		@record_count = COUNT(*) ' + CHAR(13)
		SET @sql_count = @sql_count + '	FROM' + CHAR(13)
		SET @sql_count = @sql_count + @from_block + CHAR(13)
		IF (LEN(@where_block) > 0)
			BEGIN
				SET @sql_count = @sql_count + 'WHERE ' + CHAR(13)
				SET @sql_count = @sql_count + @where_block + CHAR(13)
			END


		EXEC sp_executesql 
			@sql_count, 
			N'@record_count int OUTPUT', 
			@record_count = @total_records OUTPUT
	END
	
	-- Задаем номер начальной записи по умолчанию
	IF (@start_row <= 0)
		BEGIN
			SET @start_row = 1
		END
		
	-- Задаем номер конечной записи
	DECLARE @end_row AS int
	if (@page_size = 0)
		SET @end_row = 0			
	else
		SET @end_row = @start_row + @page_size - 1		
	
	IF (@count_only = 0)
		BEGIN
			-- Возвращаем результат
			DECLARE @sql_result AS nvarchar(max)
			
			SET @sql_result = ''		
			SET @sql_result = @sql_result + 'WITH PAGED_DATA_CTE' + CHAR(13)
			SET @sql_result = @sql_result + 'AS' + CHAR(13)
			SET @sql_result = @sql_result + '(' + CHAR(13)
			SET @sql_result = @sql_result + '	SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '		c.*, ' + CHAR(13)
			SET @sql_result = @sql_result + '		ROW_NUMBER() OVER (ORDER BY ' + @order_by_block + ') AS ROW_NUMBER, COUNT(*) OVER() AS ROWS_COUNT ' + CHAR(13)
			SET @sql_result = @sql_result + '	FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '	( ' + CHAR(13)
			SET @sql_result = @sql_result + '		SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '		' + @select_block + CHAR(13)
			SET @sql_result = @sql_result + '		FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '		' + @from_block + CHAR(13)
			IF (LEN(@where_block) > 0)
				BEGIN
					SET @sql_result = @sql_result + '		WHERE' + CHAR(13)
					SET @sql_result = @sql_result + '		' + @where_block + CHAR(13)
				END
			SET @sql_result = @sql_result + '	) AS c ' + CHAR(13)
			SET @sql_result = @sql_result + ')' + CHAR(13) + CHAR(13)
			
			SET @sql_result = @sql_result + 'SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '	* ' + CHAR(13)
			SET @sql_result = @sql_result + 'FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '	PAGED_DATA_CTE' + CHAR(13)
			IF (@end_row > 0 or @start_row > 1)
			BEGIN
				SET @sql_result = @sql_result + 'WHERE 1 = 1' + CHAR(13)
				IF @start_row > 1 
					SET @sql_result = @sql_result + ' AND ROW_NUMBER >= ' + CAST(@start_row AS nvarchar) + ' '
				IF @end_row > 0
					SET @sql_result = @sql_result + ' AND ROW_NUMBER <= ' + CAST(@end_row AS nvarchar) + ' ' + CHAR(13)
			END	
			SET @sql_result = @sql_result + 'ORDER BY ' + CHAR(13)
			SET @sql_result = @sql_result + '	ROW_NUMBER ASC ' + CHAR(13)
			
			print(@sql_result)
			EXEC(@sql_result)
		END
	
	SET NOCOUNT OFF
END
GO

ALTER procedure [dbo].[qp_update_with_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
declare @content_id numeric, @splitted bit
select @content_id = content_id, @splitted = SPLITTED from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
	
	set @table_name = 'content_' + CAST(@content_id as nvarchar)
	if (@splitted = 1)
		set @table_name = @table_name + '_async'
		
	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
	
	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
	
	SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
		
	set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
	case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as data 
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
	) as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt
	on pt.content_item_id = base.content_item_id
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER PROCEDURE [dbo].[qp_replicate_items] 
@ids nvarchar(max)
AS
BEGIN
	set nocount on
	
	declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
	declare @table_name nvarchar(50), @async_table_name nvarchar(50)

	declare @content_id numeric, @published_id numeric

	declare @articles table
	(
		id numeric primary key,
		splitted bit,
		status_type_id numeric,
		content_id numeric
	)
	
	insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')
	
	update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID 

	declare @contents table
	(
		id numeric primary key
	)
	
	insert into @contents
	select distinct content_id from @articles
	
	while exists (select id from @contents)
	begin
		select @content_id = id from @contents
		
		set @sync_ids_list = null
		select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 0
		set @async_ids_list = null
		select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 1
		
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
			
			exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0		
		end
		
		if @async_ids_list <> ''
		begin
			exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
			print @sql
			exec sp_executesql @sql
			
			exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
			print @sql
			exec sp_executesql @sql
			
			exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1							
		end
		
		select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
		if exists (select * from @articles where content_id = @content_id and status_type_id = @published_id and splitted = 0)
			update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
		else
			update content_modification set stage_modified = GETDATE() where content_id = @content_id	

		
		delete from @contents where id = @content_id
	end
	
	set @sql = 'update content_item  set not_for_replication = 0 where content_item_id in (' + @ids + ' )'
	print @sql
	exec sp_executesql @sql
END

GO

exec qp_drop_existing 'qp_aggregated_and_self', 'IsTableFunction'
GO

exec qp_drop_existing 'qp_link_titles', 'IsScalarFunction'
GO

update entity_type set CONTEXT_NAME = null where code = 'virtual_content'
update entity_type set CONTEXT_NAME = null where code = 'archive_article'
update entity_type set CONTEXT_NAME = null where code = 'virtual_article'
update entity_type set CONTEXT_NAME = null where code = 'virtual_field'
GO

ALTER VIEW [dbo].[USER_GROUP_TREE]
WITH SCHEMABINDING
AS
select ug.[GROUP_ID]
      ,ug.[GROUP_NAME]
      ,ug.[DESCRIPTION]
      ,ug.[CREATED]
      ,ug.[MODIFIED]
      ,ug.[LAST_MODIFIED_BY]
      ,U.[LOGIN] as LAST_MODIFIED_BY_LOGIN
      ,ug.[shared_content_items]
      ,ug.[nt_group]
      ,ug.[ad_sid]
      ,ug.[BUILT_IN]
      ,ug.[READONLY]
      ,ug.[use_parallel_workflow]
	  ,gtg.Parent_Group_Id AS PARENT_GROUP_ID 
from dbo.USER_GROUP ug 
left join dbo.Group_To_Group gtg on ug.GROUP_ID = gtg.Child_Group_Id
join dbo.USERS U ON U.[USER_ID] = ug.LAST_MODIFIED_BY
GO


if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CAN_UNLOCK_ITEMS' and TABLE_NAME = 'USER_GROUP')
begin
	alter table [USER_GROUP] DROP CONSTRAINT [DF_USER_GROUP_CAN_UNLOCK_ITEMS]
	alter table [USER_GROUP] DROP COLUMN [CAN_UNLOCK_ITEMS] 
end
GO


if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'API' and TABLE_NAME = 'BACKEND_ACTION_LOG')
begin
	alter table [BACKEND_ACTION_LOG] DROP CONSTRAINT [DF_BACKEND_ACTION_LOG_API]
	alter table [BACKEND_ACTION_LOG] DROP COLUMN [API] 
end
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_SERVICE' and TABLE_NAME = 'NOTIFICATIONS')
begin
	alter table [NOTIFICATIONS] DROP CONSTRAINT [DF_NOTIFICATIONS_USE_SERVICE]
	alter table [NOTIFICATIONS] DROP COLUMN [USE_SERVICE] 
end
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_AD_SYNC_SERVICE' and TABLE_NAME = 'DB')
begin
	alter table [DB] DROP CONSTRAINT [DF_DB_USE_AD_SYNC_SERVICE]
	alter table [DB] DROP COLUMN [USE_AD_SYNC_SERVICE] 
end
GO

exec qp_drop_existing 'EXTERNAL_NOTIFICATION_QUEUE', 'IsUserTable'
GO


ALTER TRIGGER [dbo].[ti_statuses_and_default_notif] ON [dbo].[SITE] 
FOR INSERT
AS
 
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Created',  10, 'Article has been created' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Approved',  50, 'Article has been modified' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Published',  100, 'Article has been published' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'None',  0, 'No Status has been assigned' ,1 from inserted)
 
INSERT INTO page_template(site_id, template_name, net_template_name, template_picture, created, modified, last_modified_by, charset, codepage, locale, is_system, net_language_id)  
select site_id, 'Default Notification Template', 'Default_Notification_Template', '', getdate(), getdate(), 1, 'utf-8', 65001, 1049, 1, dbo.qp_default_net_language(script_language) from inserted 

insert into content_group (site_id, name)
select site_id, 'Default Group' from inserted 
GO


ALTER TRIGGER [dbo].[tbd_delete_content] ON [dbo].[CONTENT] INSTEAD OF DELETE
AS 
BEGIN
	create table #disable_td_delete_item(id numeric)

	UPDATE content_attribute SET related_attribute_id = NULL
	where related_attribute_id in (
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

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'FORM_SCRIPT' and TABLE_NAME = 'CONTENT')
	alter table [CONTENT] drop column [FORM_SCRIPT] 
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'HIDE' and TABLE_NAME = 'CONTENT_ACCESS')
begin
	alter table [CONTENT_ACCESS] drop constraint [DF_CONTENT_ACCESS_HIDE]
	alter table [CONTENT_ACCESS] drop column [HIDE]
end 
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'COLOR' and TABLE_NAME = 'STATUS_TYPE')
	alter table [STATUS_TYPE] drop column [COLOR]
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'ALT_COLOR' and TABLE_NAME = 'STATUS_TYPE')
	alter table [STATUS_TYPE] drop column [ALT_COLOR]
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'AUTO_OPEN_HOME' and TABLE_NAME = 'DB')
begin
	alter table [DB] drop constraint [DF_DB_AUTO_OPEN_HOME]
	alter table [DB] drop column [AUTO_OPEN_HOME] 
end
GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'EXCLUDED_BY_ACTION_ID' and TABLE_NAME = 'BACKEND_ACTION')
begin
	alter table [BACKEND_ACTION] drop constraint [FK_BACKEND_ACTION_EXCLUDED_BY_ACTION]
	alter table [BACKEND_ACTION] drop column [EXCLUDED_BY_ACTION_ID] 
end
GO

update BACKEND_ACTION set CONTROLLER_ACTION_URL = null where code = 'home'
update BACKEND_ACTION set CONTROLLER_ACTION_URL = null where code = 'about'

delete from ACTION_TOOLBAR_BUTTON where name = 'Refresh' and PARENT_ACTION_ID = dbo.qp_action_id('home')
delete from ACTION_TOOLBAR_BUTTON where name = 'Refresh' and PARENT_ACTION_ID = dbo.qp_action_id('about')
delete from BACKEND_ACTION where code = 'refresh_home'
delete from BACKEND_ACTION where code = 'refresh_about'
GO

ALTER PROCEDURE [dbo].[qp_all_article_search]
	@p_site_id int,
	@p_user_id int,
	@p_searchparam nvarchar(4000),
	@p_order_by nvarchar(max) = N'Rank DESC',
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
	
    -- Задаем номер начальной записи по умолчанию
	IF (@p_start_row <= 0)
		BEGIN
			SET @p_start_row = 1
		END
		
	-- Задаем номер конечной записи
	DECLARE @p_end_row AS int
	SET @p_end_row = @p_start_row + @p_page_size - 1
	
	-- свормировать запрос для подмножества контентов к которым есть доступ 
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
		
	-- посчитать общее кол-во записей					
	declare @paramdef nvarchar(4000);
	declare @query nvarchar(4000);
	
	create table #temp
	([rank] int, content_item_id numeric, attribute_id numeric, [priority] int)
	
	create table #temp2
	([rank] int, content_item_id numeric, attribute_id numeric, [priority] int)	
	
	declare @table_name nvarchar(10)
	if @is_admin = 0
		set @table_name = '#temp'
	else
		set @table_name = '#temp2'
	
	set @query = 'insert into ' + @table_name + CHAR(13)
		+ ' select ft.[rank], cd.content_item_id, cd.attribute_id, 0 ' + CHAR(13)
		+ ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
		+ ' inner join content_data cd on ft.[key] = cd.content_data_id ' + CHAR(13)
		
	IF @p_item_id is not null 	
		set @query = @query + ' union select 0, ' + cast(@p_item_id as varchar(20)) + ', 0, 1 ' + CHAR(13)		
	exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam	
	
	if @is_admin = 0
	begin
		set @query = 'insert into #temp2 ' + CHAR(13)
			+ ' select cd.* from #temp cd ' + CHAR(13)
			+ ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
			+ ' inner join (' + @security_sql + ') c on c.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)	
		exec sp_executesql @query
	end
	
	select @total_records = count(distinct content_item_id) from #temp2		
		
	-- главный запрос
	declare @query_template nvarchar(4000);
	set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
		+ ' (select wrapper.*, ' + CHAR(13) 
		+ ' 		ROW_NUMBER() OVER (ORDER BY wrapper.[priority] DESC, <$_order_by_$>) AS ROW ' + CHAR(13)
		+ '  from ' + CHAR(13)
		+ '  (select ' + CHAR(13) 
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
		+ ' 	ROW_NUMBER() OVER (PARTITION BY data.CONTENT_ITEM_ID ORDER BY data.[rank] DESC) AS SIMILAR_ITEM_ROW ' + CHAR(13)
		+ '   from #temp2 data ' + CHAR(13)
		+ '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
		+ '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
		+ '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)	
		+ '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
		+ '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
		+ '   ) as wrapper ' + CHAR(13)
		+ '   where wrapper.SIMILAR_ITEM_ROW = 1 ' + CHAR(13)
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
		+ ' 	Rank ' + CHAR(13)
		+ ' from PAGED_DATA_CTE pdc ' + CHAR(13)
		+ ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
		+ ' where ROW between @start_row and @end_row';
		
	
	declare @sortExp nvarchar(4000);
	set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;	
	set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);	
	set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
	EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;
	
	drop table #temp
	drop table #temp2
END
GO

exec sp_refreshview 'CONTENT_ACCESS_PermLevel'
exec sp_refreshview 'CONTENT_ACCESS_PermLevel_site'

GO

exec qp_drop_existing 'ACTION_EXCLUSIONS', 'IsUserTable'
GO

delete from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('crop_content_file')

delete from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('crop_site_file')

delete from BACKEND_ACTION where code = 'crop_site_file'

delete from BACKEND_ACTION where code = 'crop_content_file'

delete from ACTION_TYPE where code = 'crop'

GO

if exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CONTENT_FORM_SCRIPT' and TABLE_NAME = 'SITE')
	alter table [SITE] drop column [CONTENT_FORM_SCRIPT]
GO

ALTER PROCEDURE [dbo].[qp_merge_article] 
@item_id numeric
AS
BEGIN
	if exists (select * from content_item where content_item_id = @item_id and SCHEDULE_NEW_VERSION_PUBLICATION = 1)
	begin
	exec qp_merge_links @item_id 
	UPDATE content_item with(rowlock) set not_for_replication = 1 WHERE content_item_id = @item_id
	UPDATE content_item with(rowlock) set SCHEDULE_NEW_VERSION_PUBLICATION = 0, MODIFIED = GETDATE(), LAST_MODIFIED_BY = 1, CANCEL_SPLIT = 0 where CONTENT_ITEM_ID = @item_id 
	exec qp_replicate @item_id
	UPDATE content_item_schedule with(rowlock) set delete_job = 0 WHERE content_item_id = @item_id
	DELETE FROM content_item_schedule with(rowlock) WHERE content_item_id = @item_id
	delete from CHILD_DELAYS with(rowlock) WHERE id = @item_id
	delete from CHILD_DELAYS with(rowlock) WHERE child_id = @item_id
	end
END
GO

delete from SYSTEM_INFO where field_value = '7.9.9.0'

PRINT '7.9.9.0 deleted'
GO

if exists (select * from sys.procedures where name = 'qp_clear_relations')
begin
	drop procedure dbo.qp_clear_relations
end
GO

delete from SYSTEM_INFO where field_value = '7.9.7.59'

PRINT '7.9.7.59 deleted'
GO

-- fixed in 7.9.7.16

delete from SYSTEM_INFO where field_value = '7.9.7.58'

PRINT '7.9.7.58 deleted'
GO

-- fixed in 7.9.7.29

delete from SYSTEM_INFO where field_value = '7.9.7.57'

PRINT '7.9.7.57 deleted'
GO


-- hot fixed in 7.9.7.38

delete from SYSTEM_INFO where field_value = '7.9.7.56'

PRINT '7.9.7.56 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.55'

PRINT '7.9.7.55 deleted'
GO

-- one way

delete from SYSTEM_INFO where field_value = '7.9.7.54'

PRINT '7.9.7.54 deleted'
GO


-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.53'

PRINT '7.9.7.53 deleted'
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'ORDER_BY_TITLE') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUTE_ORDER_BY_TITLE
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN ORDER_BY_TITLE   	 
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'FIELD_TITLE_COUNT') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUTE_FIELD_TITLE_COUNT
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN FIELD_TITLE_COUNT   
END
GO

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'INCLUDE_RELATIONS_IN_TITLE') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUTE_INCLUDE_RELATIONS_IN_TITLE
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN INCLUDE_RELATIONS_IN_TITLE   
END
GO


ALTER FUNCTION [dbo].[qp_get_display_field](@content_id NVARCHAR(255), @with_relation_field BIT = 0) RETURNS NVARCHAR(255)
AS BEGIN
	DECLARE @fld_name NVARCHAR(255)

	SELECT @fld_name = attribute_name FROM (
	SELECT  top 1 attribute_name from [dbo].[qp_get_display_fields](@content_id,  @with_relation_field)
	ORDER BY view_in_list desc, is_blob desc, attribute_order asc) AS a

	IF @fld_name is Null
		Set @fld_name = 'content_item_id'
	RETURN @fld_name
END
GO

delete from SYSTEM_INFO where field_value = '7.9.7.52'

PRINT '7.9.7.52 deleted'
GO

delete from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('new_adjacent_field') 
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_adjacent_field') 
delete from BACKEND_ACTION where ID = dbo.qp_action_id('new_adjacent_field')
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_virtual_content') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('content_group')  
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_content') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('content_group')  
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_template_object') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('template') 
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_page') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('template')  
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_page_object') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('page')
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_virtual_content') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('site')  
delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('new_content') and CONTEXT_MENU_ID = dbo.qp_context_menu_id('site')   
GO


delete from SYSTEM_INFO where field_value = '7.9.7.51'

PRINT '7.9.7.51 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.50'

PRINT '7.9.7.50 deleted'
GO

ALTER TRIGGER [dbo].[ti_item_to_item] ON [dbo].[item_to_item] AFTER INSERT
AS 
BEGIN

with items (link_id, item_id, linked_item_id)
AS
(
	select i1.link_id, i1.l_item_id, i1.r_item_id From inserted i1
	inner join content_to_content c2c on i1.link_id = c2c.link_id 
	where c2c.[symmetric] = 1 and not exists (select * from item_to_item i2 where i1.link_id = i2.link_id and i1.r_item_id = i2.l_item_id and i2.r_item_id = i1.l_item_id)
)
insert into item_to_item(link_id, l_item_id, r_item_id)
select link_id, linked_item_id, item_id from items

END
GO

delete from SYSTEM_INFO where field_value = '7.9.7.49'

PRINT '7.9.7.49 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.48'

PRINT '7.9.7.48 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.47'

PRINT '7.9.7.47 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.46'

PRINT '7.9.7.46 deleted'
GO



-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.45'

PRINT '7.9.7.45 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.44'

PRINT '7.9.7.44 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.43'

PRINT '7.9.7.43 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.42'

PRINT '7.9.7.42 deleted'
GO


-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.41'

PRINT '7.9.7.41 deleted'
GO


delete from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('multiple_export_article')
delete from BACKEND_ACTION where code = 'multiple_export_article'
delete from ACTION_TYPE where code = 'multiple_export'

update CONTEXT_MENU_ITEM set icon = null where name = 'Export Articles'
update CONTEXT_MENU_ITEM set icon = null where name = 'Import Articles'

delete from SYSTEM_INFO where field_value = '7.9.7.40'

PRINT '7.9.7.40 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.39'

PRINT '7.9.7.39 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.38'

PRINT '7.9.7.38 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.37'

PRINT '7.9.7.37 deleted'
GO

-- fixed in 7.9.7.33

delete from SYSTEM_INFO where field_value = '7.9.7.36'

PRINT '7.9.7.36 deleted'
GO


ALTER PROCEDURE [dbo].[qp_update_m2o_final] 
@id numeric
AS
BEGIN
	declare @statusId numeric
	declare @splitted bit
	declare @lastModifiedBy numeric
	declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, processed bit not null default 0, primary key(id, attribute_id))
	
	insert into @ids(id, attribute_id, to_remove)
	select * from #resultIds
	
	select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
	
	update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1 
	where content_item_id in (select id from @ids)
	
	update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate() 
	from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
	where r.to_remove = 0
	
	update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate() 
	from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
	where r.to_remove = 1
	
	declare @maxStatus numeric
	declare @resultId numeric
	
	select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

	if @statusId = @maxStatus and @splitted = 0 begin 
	while exists (select * from child_delays where id = @id)
	begin
		select @resultId = child_id from child_delays where id = @id
		print @resultId
		delete from child_delays where id = @id and child_id = @resultId
		if not exists(select * from child_delays where child_id = @resultId)
		begin
			exec qp_merge_article @resultId
		end
	end
	end else if @maxStatus is not null begin
		insert into child_delays (id, child_id) select @id, r.id from @ids r 
		inner join content_item ci on r.id = ci.content_item_id 
		left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
		left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id 
		left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
		where ex.child_id is null and ci.status_type_id = wms.max_status_type_id 
			and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
		
		update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
	end
	
	while exists (select id from @ids where processed = 0)
	begin
		select @resultId = id from @ids where processed = 0
		exec qp_replicate @resultId
		update @ids set processed = 1 where id = @resultId
	end
END
GO

delete from SYSTEM_INFO where field_value = '7.9.7.35'

PRINT '7.9.7.35 deleted'
GO

-- one way

delete from SYSTEM_INFO where field_value = '7.9.7.34'

PRINT '7.9.7.34 deleted'
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_articles') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_articles
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_contents') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_contents
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_contents_update') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_contents_update
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_copy_contents_attributes') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_copy_contents_attributes
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_settings') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_settings
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_templates') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_templates
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_update_attributes') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_update_attributes
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_update_links') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_update_links
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_virtual_contents') AND type in (N'P', N'PC'))
	DROP PROCEDURE dbo.qp_copy_site_virtual_contents
GO

IF EXISTS (SELECT * FROM   sys.objects WHERE  object_id = OBJECT_ID(N'dbo.GetRelationsBetweenContentLinks') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
	DROP FUNCTION dbo.GetRelationsBetweenContentLinks
GO

IF EXISTS (SELECT * FROM   sys.objects WHERE  object_id = OBJECT_ID(N'dbo.GetRelationsBetweenContents') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
	DROP FUNCTION dbo.GetRelationsBetweenContents
GO

delete from SYSTEM_INFO where field_value = '7.9.7.33'

PRINT '7.9.7.33 deleted'
GO

ALTER procedure [dbo].[qp_update_items_with_content_data_pivot]
@content_id numeric,
@ids nvarchar(max),
@is_async bit
as
begin

	declare @sql nvarchar(max), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
	 
	set @table_name = 'content_' + CAST(@content_id as nvarchar)
	if (@is_async = 1)
	set @table_name = @table_name + '_async'
		
	declare @attributes table
	(
		name nvarchar(255) primary key
	)
	
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id

	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

	SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
		
	set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
	case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as data 
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID in (' + @ids + ') 
	) as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt
	on pt.content_item_id = base.content_item_id
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
end
GO

ALTER procedure [dbo].[qp_update_with_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
declare @content_id numeric, @splitted bit
select @content_id = content_id, @splitted = SPLITTED from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
	
	set @table_name = 'content_' + CAST(@content_id as nvarchar)
	if (@splitted = 1)
		set @table_name = @table_name + '_async'
		
	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
	
	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
	
	SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
		
	set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
	case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as data 
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
	) as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt
	on pt.content_item_id = base.content_item_id
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER procedure [dbo].[qp_get_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
	
	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

	set @sql = N'select * from
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
	case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as data 
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
	) as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt order by pt.content_item_id desc
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER procedure [dbo].[qp_get_versions]
@item_id numeric,
@version_id numeric = 0
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
	
	declare @attributes table
	(
		name nvarchar(255)
	)
	
	declare @main_ids table
	(
		id numeric
	)
	
	insert into @main_ids
	select content_id from CONTENT_ATTRIBUTE where AGGREGATED = 1 and RELATED_ATTRIBUTE_ID in (select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where CONTENT_ID  = @content_id)
	
	insert into @main_ids
	values(@content_id)
	
	
	insert into @attributes(name) 
	select CASE c.CONTENT_ID WHEN @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + '.' + CA.ATTRIBUTE_NAME END 
	from content_attribute ca 
	inner join content c on ca.CONTENT_ID = c.CONTENT_ID 
	where ca.CONTENT_ID in (select id from @main_ids)
	order by C.CONTENT_ID, CA.attribute_order
	
	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
	SELECT @prefixed_fields = COALESCE(@prefixed_fields + ', ', '') + 'pt.[' + name + ']' FROM @attributes
	
	if @version_id = 0
		set @version_sql = ''
	else
		set @version_sql = ' and vcd.CONTENT_ITEM_VERSION_ID= @version_id'
		
		
	declare @ids nvarchar(max)
	select @ids = coalesce(@ids + ', ', '') + cast(id as nvarchar(10)) from @main_ids
	
	set @sql = N'select pt.content_item_id, pt.version_id, pt.created, pt.created_by, pt.modified, pt.last_modified_by, ' + @prefixed_fields  + N' from
	(
	select civ.CONTENT_ITEM_ID, civ.CREATED, civ.CREATED_BY, civ.MODIFIED, civ.LAST_MODIFIED_BY, vcd.CONTENT_ITEM_VERSION_ID as version_id, 
	case ca.CONTENT_ID when @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + ''.'' + ca.ATTRIBUTE_NAME END AS ATTRIBUTE_NAME,
	dbo.qp_get_version_data(vcd.ATTRIBUTE_ID, vcd.CONTENT_ITEM_VERSION_ID) as data 
	from CONTENT_ATTRIBUTE ca
	INNER JOIN CONTENT c on ca.CONTENT_ID = c.CONTENT_ID
	left outer join VERSION_CONTENT_DATA vcd on ca.ATTRIBUTE_ID = vcd.ATTRIBUTE_ID
	inner join CONTENT_ITEM_VERSION civ on vcd.CONTENT_ITEM_VERSION_ID = civ.CONTENT_ITEM_VERSION_ID
	where ca.CONTENT_ID in (' + @ids + ') and civ.CONTENT_ITEM_ID = @item_id ' + @version_sql + ') as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt order by pt.version_id desc

	'

	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric, @version_id numeric', @content_id = @content_id, @item_id = @item_id, @version_id = @version_id
end
end
GO

ALTER procedure [dbo].[qp_get_default_article]
@content_id numeric
as
begin

declare @sql nvarchar(max), @fields nvarchar(max), @prefixed_fields nvarchar(max)
 
if @content_id is not null
begin
	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
	
	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
	SELECT @prefixed_fields = COALESCE(@prefixed_fields + ', ', '') + 'pt.[' + name + ']' FROM @attributes
	
	set @sql = N'select ' + @prefixed_fields  + N' from
	(
	select ca.ATTRIBUTE_NAME, CASE WHEN ca.attribute_type_id in (9, 10) THEN convert(nvarchar(max), ca.DEFAULT_BLOB_VALUE) ELSE ca.DEFAULT_VALUE END as data from CONTENT_ATTRIBUTE ca
	where ca.CONTENT_ID = @content_id) as src
	PIVOT
	(
	MAX(src.data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt 
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
end
end
GO

delete from SYSTEM_INFO where field_value = '7.9.7.32'

PRINT '7.9.7.32 deleted'
GO
-- one way

delete from SYSTEM_INFO where field_value = '7.9.7.31'

PRINT '7.9.7.31 deleted'
GO

delete from ACTION_TOOLBAR_BUTTON where action_id = dbo.qp_action_id('update_content_and_up')
delete from ACTION_TOOLBAR_BUTTON where action_id = dbo.qp_action_id('save_content_and_up')
delete from ACTION_TOOLBAR_BUTTON where action_id = dbo.qp_action_id('update_field_and_up')
delete from ACTION_TOOLBAR_BUTTON where action_id = dbo.qp_action_id('save_field_and_up')

delete from BACKEND_ACTION where code = 'update_content_and_up'
delete from BACKEND_ACTION where code = 'save_content_and_up'
delete from BACKEND_ACTION where code = 'update_field_and_up'
delete from BACKEND_ACTION where code = 'save_field_and_up'

GO

delete from SYSTEM_INFO where field_value = '7.9.7.30'

PRINT '7.9.7.30 deleted'
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT' AND COLUMN_NAME = 'PARENT_CONTENT_ID') 
BEGIN
	ALTER TABLE CONTENT DROP CONSTRAINT FK_CONTENT_PARENT_CONTENT_ID 
	ALTER TABLE CONTENT DROP COLUMN PARENT_CONTENT_ID 
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'PARENT_ATTRIBUTE_ID') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT FK_CONTENT_ATTRIBUTE_PARENT_ATTRIBUTE_ID 
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN PARENT_ATTRIBUTE_ID 
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'HIDE') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUITE_HIDE
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN HIDE
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'OVERRIDE') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUITE_OVERRIDE
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN OVERRIDE
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT' AND COLUMN_NAME = 'USE_FOR_CONTEXT') 
BEGIN
	ALTER TABLE CONTENT DROP CONSTRAINT DF_CONTENT_USE_FOR_CONTEXT
	ALTER TABLE CONTENT DROP COLUMN USE_FOR_CONTEXT
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_FOR_CONTEXT') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUTE_USE_FOR_CONTEXT
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN USE_FOR_CONTEXT
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_FOR_VARIATIONS') 
BEGIN
	ALTER TABLE CONTENT_ATTRIBUTE DROP CONSTRAINT DF_CONTENT_ATTRIBUTE_USE_FOR_VARIATIONS
	ALTER TABLE CONTENT_ATTRIBUTE DROP COLUMN USE_FOR_VARIATIONS
END

delete from SYSTEM_INFO where field_value = '7.9.7.29'

PRINT '7.9.7.29 deleted'
GO

-- fixed in 7.9.7.25

delete from SYSTEM_INFO where field_value = '7.9.7.28'

PRINT '7.9.7.28 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.27'

PRINT '7.9.7.27 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.26'

PRINT '7.9.7.26 deleted'
GO

delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('search_in_archive_articles')
delete from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('refresh_search_in_archive_articles')
delete from BACKEND_ACTION where code = 'search_in_archive_articles'
delete from BACKEND_ACTION where code = 'refresh_search_in_archive_articles'
GO
delete from SYSTEM_INFO where field_value = '7.9.7.25'

PRINT '7.9.7.25 deleted'
GO

update BACKEND_ACTION set TAB_ID = NULL, ENTITY_TYPE_ID = dbo.qp_entity_type_id('article') where CODE = 'list_status_history'
delete from entity_type where code = 'article_status'
GO
delete from SYSTEM_INFO where field_value = '7.9.7.24'

PRINT '7.9.7.24 deleted'
GO

-- fixed in 7.9.7.24

delete from SYSTEM_INFO where field_value = '7.9.7.23'

PRINT '7.9.7.23 deleted'
GO

-- fixed in 7.9.7.21

delete from SYSTEM_INFO where field_value = '7.9.7.22'

PRINT '7.9.7.22 deleted'
GO


ALTER PROCEDURE [dbo].[qp_update_acrticle_modification_date]
	@xmlParameter xml
AS
BEGIN
	DECLARE @idoc int
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlParameter;
	
	DECLARE @ModifiedArticles TABLE (CONTENT_ITEM_ID int, MODIFIED datetime, LAST_MODIFIED_BY int)	
	
INSERT INTO @ModifiedArticles
		SELECT * FROM OPENXML(@idoc, '/items/item')
		WITH(
				CONTENT_ITEM_ID int '@id'
				,MODIFIED datetime '@modified'
				,LAST_MODIFIED_BY int '@modifiedBy')

		BEGIN  
		   UPDATE
			[dbo].[CONTENT_ITEM]
		SET
			[dbo].[CONTENT_ITEM].[MODIFIED] = [@ModifiedArticles].[MODIFIED]
			,[dbo].[CONTENT_ITEM].[LAST_MODIFIED_BY] = [@ModifiedArticles].[LAST_MODIFIED_BY]
		FROM
			[dbo].[CONTENT_ITEM]
		INNER JOIN
			@ModifiedArticles
		ON
			[dbo].[CONTENT_ITEM].CONTENT_ITEM_ID = [@ModifiedArticles].[CONTENT_ITEM_ID]

		END
END
GO
ALTER PROCEDURE [dbo].[qp_insert_m2m_field_Values]
	@xmlParameter xml
AS
BEGIN
	DECLARE @idoc int
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlParameter;
	
INSERT INTO [dbo].[item_to_item] (link_id, l_item_id, r_item_id)
SELECT * FROM OPENXML(@idoc, '/items/item', 1)
		WITH(
				linkId int
				,id int
				,linkedId int) 
END
GO

ALTER PROCEDURE [dbo].[qp_insertArticleValues]
	@xmlParameter xml
AS
BEGIN
	DECLARE @idoc int
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlParameter;
	
	DECLARE @NewArticles TABLE (CONTENT_ITEM_ID int, ATTRIBUTE_ID int, DATA nvarchar(3500), BLOB_DATA nvarchar(max))	
	
	INSERT INTO @NewArticles
		SELECT * FROM OPENXML(@idoc, '/PARAMETERS/FIELDVALUE')
		WITH(
				CONTENT_ITEM_ID int './CONTENT_ITEM_ID'
				,ATTRIBUTE_ID int './ATTRIBUTE_ID'
				,DATA nvarchar(3500) './DATA'
				,BLOB_DATA nvarchar(max) './BLOB_DATA') 

		BEGIN  
		   UPDATE
			[dbo].[CONTENT_DATA]
		SET
			[dbo].[CONTENT_DATA].[DATA] = CASE WHEN ([@NewArticles].[DATA] IS NULL OR [@NewArticles].[DATA] = '') THEN NULL ELSE [@NewArticles].[DATA] END,
			[dbo].[CONTENT_DATA].[BLOB_DATA] = CASE WHEN ([@NewArticles].[BLOB_DATA] IS NULL OR [@NewArticles].[BLOB_DATA] = '') THEN NULL ELSE [@NewArticles].[BLOB_DATA] END
		FROM
			[dbo].[CONTENT_DATA]
		INNER JOIN
			@NewArticles
		ON
			[dbo].[CONTENT_DATA].CONTENT_ITEM_ID = [@NewArticles].CONTENT_ITEM_ID AND
			[dbo].[CONTENT_DATA].ATTRIBUTE_ID = [@NewArticles].ATTRIBUTE_ID

		END
END
GO


delete from SYSTEM_INFO where field_value = '7.9.7.21'

PRINT '7.9.7.21 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.20'

PRINT '7.9.7.20 deleted'
GO


IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'Ids' AND ss.name = N'dbo')
DROP TYPE [dbo].[Ids]
GO

delete from SYSTEM_INFO where field_value = '7.9.7.19'

PRINT '7.9.7.19 deleted'
GO

if exists (select * from sys.foreign_keys where name = 'FK_CONTENT_ATTRIBUTE_TREE_ORDER_FIELD')
begin
	ALTER TABLE [dbo].[CONTENT_ATTRIBUTE] drop constraint FK_CONTENT_ATTRIBUTE_TREE_ORDER_FIELD
end

delete from SYSTEM_INFO where field_value = '7.9.7.18'

PRINT '7.9.7.18 deleted'
GO

if exists (select * from sys.procedures where name = 'qp_add_contents_new_site')
begin
	drop procedure dbo.qp_add_contents_new_site
end


update BACKEND_ACTION set CONTROLLER_ACTION_URL = null where code = 'copy_site'
delete from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('copy_site')

delete from SYSTEM_INFO where field_value = '7.9.7.17'

PRINT '7.9.7.17 deleted'
GO

ALTER TRIGGER [dbo].[tbd_delete_content_item] ON [dbo].[CONTENT_ITEM] INSTEAD OF DELETE
AS 
BEGIN

delete waiting_for_approval from waiting_for_approval wa inner join deleted d on wa.content_item_id = d.content_item_id

delete child_delays from child_delays cd inner join deleted d on cd.child_id = d.content_item_id

IF dbo.qp_get_version_control() IS NOT NULL BEGIN
	delete content_item_version from content_item_version civ inner join deleted d on civ.content_item_id = d.content_item_id
	
	delete item_to_item_version from item_to_item_version iiv 
	inner join content_item_version civ on civ.content_item_version_id = iiv.content_item_version_id
	inner join deleted d on d.content_item_id = civ.content_item_id 

	delete item_to_item_version from item_to_item_version iiv 
	inner join deleted d on d.content_item_id = iiv.linked_item_id 
END

delete item_link_united_full from item_link_united_full ii where ii.item_id in (select content_item_id from deleted) 

delete content_data from content_data cd inner join deleted d on cd.content_item_id = d.content_item_id

delete content_item from content_item ci inner join deleted d on ci.content_item_id = d.content_item_id

END
GO

delete from SYSTEM_INFO where field_value = '7.9.7.16'
PRINT '7.9.7.16 deleted'
GO

update ENTITY_TYPE set NAME = 'Format' where CODE = 'page_object_format'
update ENTITY_TYPE set NAME = 'Format' where CODE = 'template_object_format'
update ENTITY_TYPE set ACTION_PERMISSION_ENABLE = 0 where CODE in ('template', 'page', 'template_object', 'page_object', 'page_object_format', 'template_object_format', 'page_object_format_version', 'template_object_format_version' )

delete from SYSTEM_INFO where field_value = '7.9.7.15'

PRINT '7.9.7.15 deleted'
GO

ALTER PROCEDURE [dbo].[qp_get_paged_data]
	@select_block nvarchar(max),
	@from_block nvarchar(max),
	@where_block nvarchar(max) = '',
	@order_by_block nvarchar(max),
	@count_only bit = 0,
	@total_records int OUTPUT,
	@start_row int = 0,
	@page_size int = 0,
	
	@use_security bit = 0,
	@user_id numeric(18,0) = 0,
	@group_id numeric(18,0) = 0,
	@start_level int = 2,
	@end_level int = 4,
	@entity_name nvarchar(100),
	@parent_entity_name nvarchar(100) = '',
	@parent_entity_id numeric(18,0) = 0,
	
	@insert_key varchar(200) = '<$_security_insert_$>'
AS
BEGIN
	SET NOCOUNT ON
	
	-- Получаем фильтр по правам
	DECLARE @security_sql AS nvarchar(max)
	SET @security_sql = ''

	IF (@use_security = 1)
		BEGIN
			EXEC dbo.qp_GetPermittedItemsAsQuery
				@user_id = @user_id,
				@group_id = @group_id,
				@start_level = @start_level,
				@end_level = @end_level,
				@entity_name = @entity_name,
				@parent_entity_name = @parent_entity_name,
				@parent_entity_id = @parent_entity_id,				
				@SQLOut = @security_sql OUTPUT
				
			SET @from_block = REPLACE(@from_block, @insert_key, @security_sql)
		END
		
	-- Получаем общее количество записей
	DECLARE @sql_count AS nvarchar(max)
	
	if (@count_only = 1)
	BEGIN
		SET @sql_count = ''
		SET @sql_count = @sql_count + 'SELECT ' + CHAR(13)
		SET @sql_count = @sql_count + '		@record_count = COUNT(*) ' + CHAR(13)
		SET @sql_count = @sql_count + '	FROM' + CHAR(13)
		SET @sql_count = @sql_count + @from_block + CHAR(13)
		IF (LEN(@where_block) > 0)
			BEGIN
				SET @sql_count = @sql_count + 'WHERE ' + CHAR(13)
				SET @sql_count = @sql_count + @where_block + CHAR(13)
			END


		EXEC sp_executesql 
			@sql_count, 
			N'@record_count int OUTPUT', 
			@record_count = @total_records OUTPUT
	END
	
	-- Задаем номер начальной записи по умолчанию
	IF (@start_row <= 0)
		BEGIN
			SET @start_row = 1
		END
		
	-- Задаем номер конечной записи
	DECLARE @end_row AS int
	if (@page_size = 0)
		SET @end_row = 0			
	else
		SET @end_row = @start_row + @page_size - 1		
	
	IF (@count_only = 0)
		BEGIN
			-- Возвращаем результат
			DECLARE @sql_result AS nvarchar(max)
			
			SET @sql_result = ''		
			SET @sql_result = @sql_result + 'WITH PAGED_DATA_CTE' + CHAR(13)
			SET @sql_result = @sql_result + 'AS' + CHAR(13)
			SET @sql_result = @sql_result + '(' + CHAR(13)
			SET @sql_result = @sql_result + '	SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '		c.*, ' + CHAR(13)
			SET @sql_result = @sql_result + '		ROW_NUMBER() OVER (ORDER BY ' + @order_by_block + ') AS ROW_NUMBER, COUNT(*) OVER() AS ROWS_COUNT ' + CHAR(13)
			SET @sql_result = @sql_result + '	FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '	( ' + CHAR(13)
			SET @sql_result = @sql_result + '		SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '		' + @select_block + CHAR(13)
			SET @sql_result = @sql_result + '		FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '		' + @from_block + CHAR(13)
			IF (LEN(@where_block) > 0)
				BEGIN
					SET @sql_result = @sql_result + '		WHERE' + CHAR(13)
					SET @sql_result = @sql_result + '		' + @where_block + CHAR(13)
				END
			SET @sql_result = @sql_result + '	) AS c ' + CHAR(13)
			SET @sql_result = @sql_result + ')' + CHAR(13) + CHAR(13)
			
			SET @sql_result = @sql_result + 'SELECT ' + CHAR(13)
			SET @sql_result = @sql_result + '	* ' + CHAR(13)
			SET @sql_result = @sql_result + 'FROM ' + CHAR(13)
			SET @sql_result = @sql_result + '	PAGED_DATA_CTE' + CHAR(13)
			IF (@end_row > 0 or @start_row > 1)
			BEGIN
				SET @sql_result = @sql_result + 'WHERE 1 = 1' + CHAR(13)
				IF @start_row > 1 
					SET @sql_result = @sql_result + ' AND ROW_NUMBER >= ' + CAST(@start_row AS nvarchar) + ' '
				IF @end_row > 0
					SET @sql_result = @sql_result + ' AND ROW_NUMBER <= ' + CAST(@end_row AS nvarchar) + ' ' + CHAR(13)
			END	
			SET @sql_result = @sql_result + 'ORDER BY ' + CHAR(13)
			SET @sql_result = @sql_result + '	ROW_NUMBER ASC ' + CHAR(13)

			print(@sql_result)
			EXEC(@sql_result)
		END
	
	SET NOCOUNT OFF
END
GO

delete from SYSTEM_INFO where field_value = '7.9.7.14'

PRINT '7.9.7.14 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.13'

PRINT '7.9.7.13 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.12'

PRINT '7.9.7.12 deleted'
GO

if exists (select * from sys.procedures where name = 'qp_update_o2mfieldvalues')
begin
	drop procedure dbo.qp_update_o2mfieldvalues
end

delete from SYSTEM_INFO where field_value = '7.9.7.11'

PRINT '7.9.7.11 deleted'
GO

-- is to be fixed in 7.9.7.9

delete from SYSTEM_INFO where field_value = '7.9.7.10'

PRINT '7.9.7.10 deleted'
GO


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
			if exists(select id from @attr_ids)
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

if exists (select * from sys.tables where name = 'FIELD_ARTICLE_BIND')
begin
	DROP TABLE [dbo].[FIELD_ARTICLE_BIND] 
end

GO

delete from SYSTEM_INFO where field_value = '7.9.7.9'

PRINT '7.9.7.9 deleted'
GO


delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('copy_field')
delete from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('copy_field')
delete from BACKEND_ACTION where code = 'copy_field'

delete from SYSTEM_INFO where field_value = '7.9.7.8'

PRINT '7.9.7.8 deleted'
GO

-- fixed in 7.9.7.18

delete from SYSTEM_INFO where field_value = '7.9.7.7'

PRINT '7.9.7.7 deleted'
GO

-- hot fix

delete from SYSTEM_INFO where field_value = '7.9.7.6'

PRINT '7.9.7.6 deleted'
GO

exec qp_delete_constraint 'CONTENT_ATTRIBUTE', 'TREE_ORDER_FIELD'
GO

if exists (select * from sys.default_constraints where name = 'DF_TREE_ORDER_FILED')
begin
	ALTER TABLE [dbo].[CONTENT_ATTRIBUTE] drop constraint DF_TREE_ORDER_FILED
end

if exists (select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'TREE_ORDER_FIELD' and TABLE_NAME = 'CONTENT_ATTRIBUTE')
begin
	ALTER TABLE [dbo].[CONTENT_ATTRIBUTE] drop column TREE_ORDER_FIELD
end

exec sp_refreshview 'CONTENT_ATTRIBUTE_TYPE'

delete from SYSTEM_INFO where field_value = '7.9.7.5'

PRINT '7.9.7.5 deleted'
GO


delete from CONTEXT_MENU_ITEM where ACTION_ID = dbo.qp_action_id('copy_page')
delete from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('copy_page')
delete from BACKEND_ACTION where code = 'copy_page'

delete from SYSTEM_INFO where field_value = '7.9.7.4'

PRINT '7.9.7.4 deleted'
GO

delete from SYSTEM_INFO where field_value = '7.9.7.3'

PRINT '7.9.7.3 deleted'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RestorePageObjectFormatVersion/'
WHERE [code] = 'restore_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RestoreTemplateObjectFormatVersion/'
WHERE [code] = 'restore_template_object_format_version'
GO

delete from SYSTEM_INFO where field_value = '7.9.7.2'

PRINT '7.9.7.2 deleted'
GO


UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexPages/'
WHERE [code] = 'list_page' 
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/NewPage/'
WHERE [code] = 'new_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/PageProperties/'
WHERE [code] = 'edit_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RemovePage/'
WHERE [code] = 'remove_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleRemovePage/'
WHERE [code] = 'multiple_remove_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CancelPage/'
WHERE [code] = 'cancel_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/SelectPages/'
WHERE [code] = 'select_page_for_object_form'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexTemplateObjects/'
WHERE [code] = 'list_template_object' 
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexPageObjects/'
WHERE [code] = 'list_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/NewPageObject/'
WHERE [code] = 'new_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/NewTemplateObject/'
WHERE [code] = 'new_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/PromotePageObject/'
WHERE [code] = 'promote_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/PageObjectProperties/'
WHERE [code] = 'edit_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/TemplateObjectProperties/'
WHERE [code] = 'edit_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RemovePageObject/'
WHERE [code] = 'remove_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RemoveTemplateObject/'
WHERE [code] = 'remove_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleRemovePageObject/'
WHERE [code] = 'multiple_remove_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleRemoveTemplateObject/'
WHERE [code] = 'multiple_remove_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CancelPageObject/'
WHERE [code] = 'cancel_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CancelTemplateObject/'
WHERE [code] = 'cancel_template_object'
GO
--

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexPageObjectFormats/'
WHERE [code] = 'list_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/IndexTemplateObjectFormats/'
WHERE [code] = 'list_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexPageObjectFormatVersions/'
WHERE [code] = 'list_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/IndexTemplateObjectFormatVersions/'
WHERE [code] = 'list_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CancelTemplateObjectFormat/'
WHERE [code] = 'cancel_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CancelPageObjectFormat/'
WHERE [code] = 'cancel_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/NewPageObjectFormat/'
WHERE [code] = 'new_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/NewTemplateObjectFormat/'
WHERE [code] = 'new_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/PageObjectFormatProperties/'
WHERE [code] = 'edit_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/TemplateObjectFormatProperties/'
WHERE [code] = 'edit_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/TemplateObjectFormatVersionProperties/'
WHERE [code] = 'edit_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/PageObjectFormatVersionProperties/'
WHERE [code] = 'edit_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CompareWithCurrentTemplateObjectFormatVersion/'
WHERE [code] = 'compare_with_cur_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CompareWithCurrentPageObjectFormatVersion/'
WHERE [code] = 'compare_with_cur_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CompareTemplateObjectFormatVersions/'
WHERE [code] = 'compare_template_object_format_versions'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/ComparePageObjectFormatVersions/'
WHERE [code] = 'compare_page_object_format_versions'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RemoveTemplateObjectFormat/'
WHERE [code] = 'remove_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/RemovePageObjectFormat/'
WHERE [code] = 'remove_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleRemoveTemplateObjectFormatVersion/'
WHERE [code] = 'multiple_remove_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleRemovePageObjectFormatVersion/'
WHERE [code] = 'multiple_remove_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/AssemblePage/'
WHERE [code] = 'assemble_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleAssemblePage/'
WHERE [code] = 'multiple_assemble_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/AssemblePageObject/'
WHERE [code] = 'assemble_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/AssembleTemplateObject/'
WHERE [code] = 'assemble_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleAssemblePageObject/'
WHERE [code] = 'multiple_assemble_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/MultipleAssembleTemplateObject/'
WHERE [code] = 'multiple_assemble_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CaptureLockPage/'
WHERE [code] = 'capture_lock_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CaptureLockPageObject/'
WHERE [code] = 'capture_lock_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CaptureLockTemplateObject/'
WHERE [code] = 'capture_lock_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CaptureLockPageObjectFormat/'
WHERE [code] = 'capture_lock_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/PageTemplate/CaptureLockTemplateObjectFormat/'
WHERE [code] = 'capture_lock_template_object_format'
GO

delete from SYSTEM_INFO where field_value = '7.9.7.1'

PRINT '7.9.7.1 deleted'

