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

