EXEC qp_drop_existing '[dbo].[td_plugin]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[td_plugin] ON [dbo].[PLUGIN] AFTER DELETE
AS
BEGIN
		declare @p table (
			id numeric identity(1,1) primary key,
            plugin_id numeric
		)

        declare @plugin_id numeric
		declare @plugin nvarchar(10), @table_name nvarchar(max)
		declare @i int, @count int

		insert into @p(plugin_id) select id from deleted
		select @count = count(*) from @p
		set @i = 1
		while @i <= @count
			begin
				select @plugin_id = plugin_id from @p where id = @i
				set @plugin = cast(@plugin_id as nvarchar(10))
				set @table_name = 'PLUGIN_SITE_' + @plugin
				exec qp_drop_existing @table_name, 'IsUserTable'
				set @table_name = 'PLUGIN_CONTENT_' + @plugin
				exec qp_drop_existing @table_name, 'IsUserTable'
				set @table_name = 'PLUGIN_CONTENT_ATTRIBUTE_' + @plugin
				exec qp_drop_existing @table_name, 'IsUserTable'

				set @i = @i + 1
			end
		end
	end
END
