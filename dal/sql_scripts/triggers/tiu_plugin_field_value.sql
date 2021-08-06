EXEC qp_drop_existing '[dbo].[tiu_plugin_field_value]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tiu_plugin_field_value] ON [dbo].[PLUGIN_FIELD_VALUE] AFTER INSERT, UPDATE
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

        insert into @plugins(plugin_id)
        select distinct plugin_id from inserted i inner join PLUGIN_FIELD p on i.PLUGIN_FIELD_ID = p.ID
		where not exists (select * from deleted d where d.ID = i.ID)

		select @count = count(*) from @plugins
		set @i = 1
		while @i <= @count
		begin
            select @plugin_id = plugin_id from @plugins where id = @i

            insert into @site_ids
            select distinct site_id from inserted i inner join PLUGIN_FIELD p on i.PLUGIN_FIELD_ID = p.ID
            where p.PLUGIN_ID = @plugin_id and site_id is not null

            insert into @content_ids
            select distinct content_id from inserted i inner join PLUGIN_FIELD p on i.PLUGIN_FIELD_ID = p.ID
            where p.PLUGIN_ID = @plugin_id and content_id is not null

            insert into @content_attribute_ids
            select distinct content_attribute_id from inserted i inner join PLUGIN_FIELD p on i.PLUGIN_FIELD_ID = p.ID
            where p.PLUGIN_ID = @plugin_id and content_attribute_id is not null

            if exists (select * from @site_ids)
            begin
                set @sql = 'insert into plugin_site_' + cast(@plugin_id as nvarchar) + '(id) select id from @site_ids' +
                           ' where id not in (select id from plugin_site_' + cast(@plugin_id as nvarchar) + ')'
                exec sp_executesql @sql, N'@site_ids [Ids] READONLY', @site_ids = @site_ids
            end

            if exists (select * from @content_ids)
            begin
                set @sql = 'insert into plugin_content_' + cast(@plugin_id as nvarchar) + '(id) select id from @content_ids' +
                           ' where id not in (select id from plugin_content_' + cast(@plugin_id as nvarchar)  + ')'
                exec sp_executesql @sql, N'@content_ids [Ids] READONLY', @content_ids = @content_ids
            end

            if exists (select * from @content_attribute_ids)
            begin
                set @sql = 'insert into plugin_content_attribute_' + cast(@plugin_id as nvarchar) + '(id) select id from @content_attribute_ids' +
                           ' where id not in (select id from plugin_content_attribute_' + cast(@plugin_id as nvarchar) + ')'
                exec sp_executesql @sql, N'@content_attribute_ids [Ids] READONLY', @content_attribute_ids = @content_attribute_ids
            end

            delete from @site_ids
            delete from @content_ids
            delete from @content_attribute_ids

            set @i = @i + 1
		end


        insert into @fields (field_id) select distinct PLUGIN_FIELD_ID from inserted

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
            select id from inserted
            where PLUGIN_FIELD_ID = @field_id

            set @sql = 'update ' + @plugin_table_name + ' set [' + @field_name + '] = v.value from ' + @plugin_table_name + ' p ' +
                'inner join plugin_field_value v on p.id = v.' + @table_name + '_ID ' +
                'where v.id in (select id from @ids)'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids

            delete from @ids
            set @i = @i + 1
        end
	end
