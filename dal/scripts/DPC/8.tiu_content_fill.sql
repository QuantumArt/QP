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
