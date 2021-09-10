EXEC qp_drop_existing '[dbo].[td_plugin_field_value]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[td_plugin_field_value] ON [dbo].[PLUGIN_FIELD_VALUE] AFTER DELETE
AS
BEGIN

		declare @i int, @count int
		declare @plugin_id numeric
		declare @sql nvarchar(max)
		declare @field_id numeric, @field_name nvarchar(255)
		declare @relation_type nvarchar(50), @value_type nvarchar(50)
		declare @table_name nvarchar(255), @plugin_table_name nvarchar(255)

		declare @plugins table(
			id numeric identity(1,1) primary key,
		    plugin_id numeric
        )

        declare @fields table (
			id numeric identity(1,1) primary key,
			field_id numeric
        )

		declare @site_ids [Ids], @content_ids [Ids], @content_attribute_ids [Ids], @ids [Ids]
		declare @field_ids [Ids], @processed [Ids]

        insert into @plugins(plugin_id)
        select distinct plugin_id from deleted i inner join PLUGIN_FIELD p on i.PLUGIN_FIELD_ID = p.ID

		select @count = count(*) from @plugins
		set @i = 1
		while @i <= @count
		begin
            select @plugin_id = plugin_id from @plugins where id = @i
            insert into @field_ids select id from PLUGIN_FIELD where plugin_id = @plugin_id

            insert into @site_ids
            select distinct site_id from deleted d
            where d.PLUGIN_FIELD_ID in (select id from @field_ids) and d.site_id is not null
            and not exists(
                select * from plugin_field_value v where d.site_id = v.site_id
                    and v.PLUGIN_FIELD_ID in (select id from @field_ids)
            )

            insert into @content_ids
            select distinct content_id from deleted d
            where d.PLUGIN_FIELD_ID in (select id from @field_ids) and d.content_id is not null
            and not exists(
                select * from plugin_field_value v where d.content_id = v.content_id
                    and v.PLUGIN_FIELD_ID in (select id from @field_ids)
            )

            insert into @content_attribute_ids
            select distinct content_attribute_id from deleted d
            where d.PLUGIN_FIELD_ID in (select id from @field_ids) and d.content_attribute_id is not null
            and not exists(
                select * from plugin_field_value v where d.content_attribute_id = v.content_attribute_id
                    and v.PLUGIN_FIELD_ID in (select id from @field_ids)
            )

            if exists (select * from @site_ids)
            begin
                set @sql = 'delete from plugin_site_' + cast(@plugin_id as nvarchar) +
                           ' where id in (select id from @site_ids)'
                exec sp_executesql @sql, N'@site_ids [Ids] READONLY', @site_ids = @site_ids
            end

            if exists (select * from @content_ids)
            begin
                set @sql = 'delete from plugin_content_' + cast(@plugin_id as nvarchar) +
                           ' where id in (select id from @content_ids)'
                exec sp_executesql @sql, N'@content_ids [Ids] READONLY', @content_ids = @content_ids
            end

            if exists (select * from @content_attribute_ids)
            begin
                set @sql = 'delete from plugin_content_attribute_' + cast(@plugin_id as nvarchar) +
                           ' where id in (select id from @content_attribute_ids)'
                exec sp_executesql @sql, N'@content_attribute_ids [Ids] READONLY', @content_attribute_ids = @content_attribute_ids
            end

            insert into @processed
            select id from deleted
            where plugin_field_id in (select id from @field_ids)
              and (
                     site_id in (select id from @site_ids) or
                     content_id in (select id from @content_ids) or
                     content_attribute_id in (select id from @content_attribute_ids)
              )

            delete from @field_ids
            delete from @site_ids
            delete from @content_ids
            delete from @content_attribute_ids

            set @i = @i + 1
		end


        insert into @fields (field_id) select distinct PLUGIN_FIELD_ID from deleted
		where id not in (select id from @processed)

		select @count = count(*) from @fields
		set @i = 1
		while @i <= @count
		begin
            select @field_id = field_id, @field_name = name,
                   @relation_type = RELATION_TYPE, @value_type = VALUE_TYPE, @plugin_id = PLUGIN_ID
            from @fields f inner join PLUGIN_FIELD p on p.ID = f.field_id where f.id = @i
			set @table_name = IIF(@relation_type = 'ContentAttribute', 'CONTENT_ATTRIBUTE', UPPER(@relation_type))
			set @plugin_table_name = 'PLUGIN_' + @table_name + '_' + cast(@plugin_id as nvarchar)

            insert into @ids
            select coalesce(site_id, content_id, content_attribute_id) from deleted
            where PLUGIN_FIELD_ID = @field_id

        set @sql = 'update ' + @plugin_table_name + ' set [' + @field_name + '] = NULL from ' + @plugin_table_name + ' p ' +
                'where id in (select id from @ids)'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids

            delete from @ids
            set @i = @i + 1
        end
end
GO
