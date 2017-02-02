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
