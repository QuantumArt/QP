EXEC qp_drop_existing '[dbo].[ti_plugin_field]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[ti_plugin_field] ON [dbo].[PLUGIN_FIELD] AFTER INSERT
AS
BEGIN
		declare @p table (
			id numeric identity(1,1) primary key,
            plugin_id numeric,
            name nvarchar(255),
            value_type nvarchar(50),
            relation_type nvarchar(50)
		)

        declare @plugin_id numeric
		declare @plugin nvarchar(10), @sql nvarchar(max), @table_name nvarchar(50), @type_name nvarchar(50)
		declare @name nvarchar(255), @value_type nvarchar(50), @relation_type nvarchar(50)
		declare @i int, @count int

		insert into @p(plugin_id, name, value_type, relation_type)
		select plugin_id, name, value_type, relation_type from inserted

		select @count = count(*) from @p
		set @i = 1
		while @i <= @count
			begin
				select @plugin_id = plugin_id, @name = name, @value_type = value_type, @relation_type = relation_type from @p where id = @i
				set @plugin = cast(@plugin_id as nvarchar(10))
				set @table_name = IIF(@relation_type = 'ContentAttribute', 'CONTENT_ATTRIBUTE', UPPER(@relation_type))
				set @table_name = 'PLUGIN_' + @table_name + '_' + @plugin
				select @type_name = CASE
				    WHEN @value_type = 'Bool' then 'bit'
				    WHEN @value_type = 'String' then 'nvarchar(255)'
				    else @value_type
				END

				if not exists (SELECT * from sys.tables where name = @table_name)
				begin
                    set @sql = 'CREATE TABLE dbo.[' + @table_name + '] (ID NUMERIC PRIMARY KEY)'
				    EXEC sp_executesql @sql
                end

                set @sql = 'ALTER TABLE ' + @table_name + ' ADD [' + @name + '] ' + @type_name + ' NULL'
				EXEC sp_executesql @sql

				set @i = @i + 1
			end
		end
	end
END
