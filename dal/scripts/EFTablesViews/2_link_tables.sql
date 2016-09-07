
exec qp_drop_existing 'qp_build_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_build_link_table] 
@link_id NUMERIC
AS BEGIN

	declare @table_name nvarchar(255), @table_name_rev nvarchar(255), @table_name_async nvarchar(255), @table_name_async_rev nvarchar(255)
	declare @sql nvarchar(max), @rev_sql nvarchar(max), @async_sql nvarchar(max), @async_rev_sql nvarchar(max)
	set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR)
	set @table_name_rev = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_rev'
	set @table_name_async = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async'
	set @table_name_async_rev = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async_rev'
	
	set @sql = 'CREATE TABLE [dbo].[item_link_' + CAST(@link_id AS NVARCHAR) + ']([id] [int] NOT NULL, [linked_id] [int] NOT NULL, PRIMARY KEY CLUSTERED ([id],[linked_id]))'
	SET @rev_sql = REPLACE(@sql, @table_name, @table_name_rev)
	SET @async_sql = REPLACE(@sql, @table_name, @table_name_async)
	SET @async_rev_sql = REPLACE(@sql, @table_name, @table_name_async_rev)

	exec qp_drop_existing @table_name , 'IsUserTable'
	exec(@sql)

	exec qp_drop_existing @table_name_rev , 'IsUserTable'
	exec(@rev_sql)

	exec qp_drop_existing @table_name_async, 'IsUserTable'
	exec(@async_sql)

	exec qp_drop_existing @table_name_async_rev, 'IsUserTable'
	exec(@async_rev_sql)

END

GO

exec qp_drop_existing 'qp_drop_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_drop_link_table] 
@link_id NUMERIC
AS BEGIN

	declare @table_name nvarchar(255)

	set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR)
	exec qp_drop_existing @table_name , 'IsUserTable'

	set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_rev'
	exec qp_drop_existing @table_name , 'IsUserTable'

	set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async'
	exec qp_drop_existing @table_name , 'IsUserTable'

	set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async_rev'
	exec qp_drop_existing @table_name , 'IsUserTable'
END

GO

exec qp_drop_existing 'qp_fill_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_fill_link_table] 
@link_id numeric
AS
BEGIN
	declare @l_content_id numeric, @r_content_id numeric
	select @l_content_id = l_content_id, @r_content_id = r_content_id from content_to_content where link_id = @link_id
	
	
	declare @rev_fields nvarchar(100)
	if @l_content_id <> @r_content_id
	begin
		set @rev_fields = 'il.item_id, il.linked_item_id'
	end
	else
	begin
		set @rev_fields = 'il.linked_item_id as item_id, il.item_id as linked_item_id'
	end
	
	declare @sql nvarchar(max)
	set @sql = 'insert into item_link_' + cast(@link_id as varchar) + ' select il.item_id, il.linked_item_id from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@l_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar) 
	exec(@sql)

	set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_rev select ' + @rev_fields + ' from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@r_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar) 
	exec(@sql)

	set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_async select il.item_id, il.linked_item_id from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@l_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar) 
	exec(@sql)

	set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_async_rev select ' + @rev_fields + ' from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@r_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar) 
	exec(@sql)
	
END
GO

exec qp_drop_existing 'qp_recreate_link_tables', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_recreate_link_tables] 
AS

BEGIN
	declare @c table (
		id numeric identity(1,1) primary key,
		link_id numeric
	)
	
	insert into @c(link_id) select link_id from content_to_content where link_id in
	(select link_id from CONTENT_ATTRIBUTE where link_id is not null)
	
	declare @i int, @count int, @link_id numeric
	
	set @i = 1
	select @count = count(id) from @c

	while @i < @count + 1
	begin
		select @link_id = link_id from @c where id = @i
		exec qp_build_link_table @link_id
		exec qp_fill_link_table @link_id

		set @i = @i + 1
	end
END
GO


exec qp_drop_existing 'qp_insert_link_table_item', 'IsProcedure'
GO

exec qp_drop_existing 'qp_delete_link_table_item', 'IsProcedure'
GO

if exists (SELECT * FROM sys.Types WHERE is_user_defined = 1 and name = 'Links')
	exec('DROP TYPE Links')

CREATE TYPE [dbo].[Links] AS TABLE(
	[ID] [numeric](18, 0) NOT NULL,
	[LINKED_ID] [numeric](18, 0) NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[ID] ASC, [LINKED_ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO


CREATE PROCEDURE [dbo].[qp_insert_link_table_item] 
@link_id numeric, @content_id numeric, @links LINKS READONLY, @is_async bit, @use_reverse_table bit, @reverse_fields bit
AS
BEGIN

	declare @table_name nvarchar(50)
	set @table_name = 'item_link_' + cast(@link_id as varchar)

	if @is_async = 1
		set @table_name = @table_name + '_async'

	if @use_reverse_table = 1
		set @table_name =  @table_name + '_rev'

	declare @sql nvarchar(max)

	declare @rev_fields nvarchar(50)
	select @rev_fields = case when @reverse_fields = 0 then 'il.id, il.linked_id' else 'il.linked_id, il.id' end
	
	declare @condition nvarchar(200)
	select @condition = case when @reverse_fields = 0 then 'il2.id = il.id and il2.linked_id = il.linked_id' else 'il2.id = il.linked_id and il2.linked_id = il.id' end

	
	set @sql = 'insert into ' + @table_name + ' select ' + @rev_fields + ' from @links il inner join content_item ci with(nolock) on il.id = ci.CONTENT_ITEM_ID where CONTENT_ID = @content_id '
	+ ' and not exists(select * from ' + @table_name + ' il2 where ' + @condition + ')'

	exec sp_executesql @sql, N'@link_id numeric, @content_id numeric, @links LINKS READONLY', @link_id = @link_id , @content_id = @content_id, @links = @links


END
GO

CREATE PROCEDURE [dbo].[qp_delete_link_table_item] 
@link_id numeric, @content_id numeric, @links LINKS READONLY, @is_async bit, @use_reverse_table bit, @reverse_fields bit
AS
BEGIN

	declare @table_name nvarchar(50)
	set @table_name = 'item_link_' + cast(@link_id as varchar)

	if @is_async = 1
		set @table_name = @table_name + '_async'

	if @use_reverse_table = 1
		set @table_name =  @table_name + '_rev'

	declare @sql nvarchar(max)

	declare @condition nvarchar(200)
	select @condition = case when @reverse_fields = 0 then 'src.id = il.id and src.linked_id = il.linked_id' else 'src.id = il.linked_id and src.linked_id = il.id' end

	set @sql = 'delete ' + @table_name + ' from ' + @table_name + ' src inner join @links il on ' + @condition

	print @sql

	exec sp_executesql @sql, N'@link_id numeric, @content_id numeric, @links LINKS READONLY', @link_id = @link_id , @content_id = @content_id, @links = @links


END
GO

GO

exec qp_rebuild_all_new_views
GO
exec qp_rebuild_all_link_views
GO
exec qp_recreate_link_tables
GO

