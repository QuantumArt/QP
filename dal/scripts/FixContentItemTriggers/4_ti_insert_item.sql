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
