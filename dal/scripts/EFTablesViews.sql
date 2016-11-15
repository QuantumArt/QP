exec qp_drop_existing 'qp_content_new_views_create', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_content_new_views_create] 
  @content_id NUMERIC
AS
BEGIN 

    if object_id('tempdb..#disable_create_new_views') is null
    begin 		
        declare @ca table (
			id numeric identity(1,1) primary key,
			attribute_id numeric, 
			attribute_name nvarchar(255), 
			attribute_size numeric,
			attribute_type_id numeric,
			is_long bit, 
			link_id numeric
		)

		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric
		declare @attribute_type_id numeric, @link_id numeric, @is_long bit
		declare @sql nvarchar(max), @result_sql nvarchar(max), @field_sql nvarchar(max)
		declare @field_template nvarchar(max), @type_name nvarchar(20), @cast bit
		declare @i numeric, @count numeric, @preserve_index bit

		set @sql = '
	isnull(cast([CONTENT_ITEM_ID] as int), 0) as [CONTENT_ITEM_ID]
	,isnull(cast([STATUS_TYPE_ID] as int), 0) as [STATUS_TYPE_ID]
	,isnull(cast([VISIBLE] as bit), 0) as [VISIBLE]
	,isnull(cast([ARCHIVE] as bit), 0) as [ARCHIVE]
	,[CREATED]
	,[MODIFIED]
	,isnull(cast([LAST_MODIFIED_BY] as int), 0) as [LAST_MODIFIED_BY]
	'
		insert into @ca (attribute_id, attribute_name, attribute_size, attribute_type_id, link_id, is_long)

		select ca.attribute_id, ca.attribute_name, ca.attribute_size, ca.attribute_type_id, isnull(ca.link_id, 0), ca.IS_LONG
		from CONTENT_ATTRIBUTE ca 
		where ca.content_id = @content_id
		order by ATTRIBUTE_ORDER

		set @i = 1
		select @count = count(id) from @ca

		while @i < @count + 1
		begin

			select @attribute_id = attribute_id, @attribute_name = '[' + attribute_name + ']', @attribute_size = attribute_size,
			@attribute_type_id = attribute_type_id, @link_id = link_id, @is_long = is_long
			from @ca where id = @i

			set @cast = 0
			set @field_template = 'cast({0} as {1}) as {0}'
			 
			if @attribute_type_id = 2 and @attribute_size = 0 and @is_long = 1
			begin
				set @type_name = 'bigint'
				set @cast = 1
			end

			else if @attribute_type_id = 11
			or @attribute_type_id = 13
			or @attribute_type_id = 2 and @attribute_size = 0 and @is_long = 0
			begin
				set @type_name = 'int'
				set @cast = 1
			end
			
			else if @attribute_type_id = 2 and @attribute_size <> 0 and @is_long = 0
			begin
				set @type_name = 'float'
				set @cast = 1
			end

			else if @attribute_type_id = 2 and @attribute_size <> 0 and @is_long = 1
			begin
				set @type_name = 'decimal'
				set @cast = 1
			end

			else if @attribute_type_id = 3 
			begin
				set @type_name = 'bit'
				set @cast = 1
				set @field_template = 'cast(isnull({0}, 0) as {1}) as {0}'
			end

			else if @attribute_type_id = 4 
			begin
				set @type_name = 'date'
				set @cast = 1
			end

			else if @attribute_type_id = 5 
			begin
				set @type_name = 'time'
				set @cast = 1
			end

			else if @attribute_type_id in (9,10) 
			begin
				set @type_name = 'nvarchar(max)'
				set @cast = 1
			end

			if @cast = 1
			begin
				set @field_sql = replace(@field_template, '{0}', @attribute_name)
				set @field_sql = replace(@field_sql, '{1}', @type_name)
			end
			else
			begin
				set @field_sql = @attribute_name
			end

			set @sql = @sql + CHAR(13) + CHAR(9) + ',' + @field_sql

			set @i = @i + 1
		end



        SET @result_sql =  ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_new as (select ' + char(13) + @sql
            + char(13) + ' from content_' + cast(@content_id as varchar(20)) + ')'
        print(@result_sql)
        exec(@result_sql)

      
        declare @virtual_type int
        select @virtual_type = virtual_type from content where content_id = @content_id

        if (@virtual_type <> 3)
        begin
    	   set @result_sql = ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_async_new as (select ' + char(13) + @sql
    		  + char(13) + ' from content_' + cast(@content_id as varchar(20)) + '_async )'
    	   print(@result_sql)
    	   exec(@result_sql)

    	   exec qp_content_united_view_create @content_id, 1

        end
         else begin
    	   set @result_sql = ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_united_new as (select ' + char(13) + @sql
    		+ char(13) + ' from content_' + cast(@content_id as varchar(20)) + '_united )'
    	   print(@result_sql)
    	   exec(@result_sql)
        end
      
        exec qp_content_frontend_views_create @content_id, 1
    end	
END
GO

exec qp_drop_existing 'qp_content_new_views_recreate', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_content_new_views_recreate] 
  @content_id NUMERIC
AS
BEGIN 

	declare @name nvarchar(50), @base_name nvarchar(20)
	set @base_name = 'dbo.content_' + cast(@content_id as varchar(20))

	set @name = @base_name + '_live_new'
	exec qp_drop_existing @name, 'IsView'

	set @name = @base_name + '_stage_new'
	exec qp_drop_existing @name, 'IsView'

	set @name = @base_name + '_united_new'
	exec qp_drop_existing @name, 'IsView'

	set @name = @base_name + '_async_new'
	exec qp_drop_existing @name, 'IsView'

	set @name = @base_name + '_new'
	exec qp_drop_existing @name, 'IsView'
	
	exec qp_content_new_views_create @content_id
	 
END
GO

ALTER PROCEDURE [dbo].[qp_content_united_view_create] 
  @content_id NUMERIC,
  @is_new bit = 0
AS 

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
	set @new_str = '_new'

  DECLARE @str_sql VARCHAR(4000), @char_content_id varchar(20)
  SET @char_content_id = CONVERT(varchar,@content_id)

  SET @str_sql =  ' create view dbo.content_' + @char_content_id + '_united' + @new_str + ' as ' +
      ' select c1.* from content_' + @char_content_id + @new_str + ' c1 ' + 
      ' left join content_' + @char_content_id + '_async' + @new_str + ' c2 ' +
      ' on c1.content_item_id = c2.content_item_id ' +
      ' where c2.content_item_id is null ' +
      ' union all ' +
      ' select * from content_' + @char_content_id + '_async' + @new_str 
  EXEC(@str_sql)
GO

ALTER PROCEDURE [dbo].[qp_content_united_view_recreate] 
  @content_id NUMERIC,
  @is_new bit = 0
AS 

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
	set @new_str = '_new'

  DECLARE @str_sql VARCHAR(4000), @view_name varchar(40)
  
  SET @view_name = 'content_' + CONVERT(varchar,@content_id) + '_united' + @new_str 
  EXEC qp_drop_existing @view_name, 'IsView'
  
  EXEC qp_content_united_view_create @content_id, @is_new
GO

ALTER PROCEDURE [dbo].[qp_content_frontend_views_create] 
  @content_id NUMERIC,
  @is_new bit = 0,
  @extra_call bit = 1
AS
  
  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
	set @new_str = '_new'
   
  DECLARE @str_sql nvarchar(max), @char_content_id nvarchar(20)
  SET @char_content_id = CONVERT(nvarchar,@content_id)

  SET @str_sql =  ' create view dbo.content_' + @char_content_id + '_live' + @new_str + ' as ' +
      ' select * from dbo.content_' + @char_content_id + @new_str + ' where visible = 1 and archive = 0  and status_type_id in (' +
      ' select status_type_id from status_type where status_type_name = ''' + dbo.qp_content_max_status_name(@content_id) + ''')'
  EXEC(@str_sql)
  
  SET @str_sql = ' create view dbo.content_' + @char_content_id + '_stage' + @new_str + ' as ' +
      ' select * from dbo.content_' + @char_content_id + '_united' + @new_str + ' where visible = 1 and archive = 0  '
  EXEC(@str_sql)

  if (@is_new = 0 and @extra_call = 1)
	exec qp_content_new_views_create @content_id

GO


ALTER PROCEDURE [dbo].[qp_content_frontend_views_recreate] 
  @content_id NUMERIC,
  @is_new bit = 0
AS

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
	set @new_str = '_new'

  declare @view_name nvarchar(50)
  SET @view_name = 'dbo.content_' + CONVERT(varchar,@content_id) + '_live' + @new_str 
  EXEC qp_drop_existing @view_name, 'IsView'
  SET @view_name = 'dbo.content_' + CONVERT(varchar,@content_id) + '_stage' + @new_str 
  EXEC qp_drop_existing @view_name, 'IsView'
  
  exec qp_content_frontend_views_create @content_id, @is_new, 0

  if (@is_new = 0)
	exec qp_content_new_views_recreate @content_id

GO

ALTER PROCEDURE [dbo].[qp_drop_link_view]
@link_id NUMERIC
AS 
BEGIN
	DECLARE @view_name nvarchar(50)
	set @view_name = 'link_' + CAST(@link_id AS NVARCHAR)
	exec qp_drop_existing @view_name , 'IsView'

	set @view_name = 'link_' + CAST(@link_id AS NVARCHAR) + '_united'
	exec qp_drop_existing @view_name , 'IsView'

	set @view_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united'
	exec qp_drop_existing @view_name , 'IsView'

	set @view_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united_rev'
	exec qp_drop_existing @view_name , 'IsView'

	exec qp_drop_link_table @link_id
	
END
GO

ALTER PROCEDURE [dbo].[qp_build_link_view] 
@link_id NUMERIC
AS BEGIN
	DECLARE @content_id numeric, @rev_content_id numeric, @link_str nvarchar(20)
	DECLARE @sql nvarchar(max), @sql2 nvarchar(max), @sql9 nvarchar(max), @sql10 nvarchar(max)
	DECLARE @view_name nvarchar(50), @view_name2 nvarchar(50), @view_name9 nvarchar(50), @view_name10 nvarchar(50)

	select @link_str = cast(@link_id as nvarchar(20))
	select @content_id = l_content_id, @rev_content_id = r_content_id from content_to_content where link_id = @link_id

	set @view_name = 'link_' + CAST(@link_id AS NVARCHAR)
	set @view_name2 = 'link_' + CAST(@link_id AS NVARCHAR) + '_united'
	set @view_name9 = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united'
	set @view_name10 = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united_rev'

	SET @sql= 'CREATE VIEW dbo.' + @view_name + ' AS select il.item_id, il.linked_item_id from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = ' + CAST(@content_id AS NVARCHAR) + ' and link_id = ' + CAST(@link_id AS NVARCHAR)

	SET @sql2 = REPLACE(@sql, @view_name, @view_name2)
	SET @sql2 = REPLACE(@sql2, 'item_link il', 'item_link_united il')

	SET @sql9 = 'CREATE VIEW dbo.' + @view_name9 + ' AS select id, linked_id from item_link_' + @link_str + ' il '
	+ ' where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID) '
	+ ' union all SELECT id, linked_id from item_link_' + @link_str + '_async ila'
	
	SET @sql10 = 'CREATE VIEW dbo.' + @view_name10 + ' AS select id, linked_id from item_link_' + @link_str + '_rev il '
	+ ' where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID) '
	+ ' union all SELECT id, linked_id from item_link_' + @link_str + '_async_rev ila'

	exec qp_drop_existing @view_name , 'IsView'
	exec(@sql)

	exec qp_drop_existing @view_name2 , 'IsView'
	exec(@sql2)

	exec qp_build_link_table @link_id

	exec qp_drop_existing @view_name9, 'IsView'
	exec(@sql9)

	exec qp_drop_existing @view_name10, 'IsView'
	exec(@sql10)

END
GO

ALTER PROCEDURE [dbo].[qp_content_table_drop]  
  @content_id numeric
AS BEGIN
  DECLARE @base_table_name VARCHAR(100), @table_name VARCHAR(100)

  SET @base_table_name = 'dbo.content_' + CONVERT(VARCHAR, @content_id)
  exec qp_drop_existing @base_table_name, 'IsView'
  exec qp_drop_existing @base_table_name, 'IsUserTable'
	
  set @table_name = @base_table_name + '_ASYNC'
  exec qp_drop_existing @table_name, 'IsView'
  exec qp_drop_existing @table_name, 'IsUserTable'

  set @table_name = @base_table_name + '_UNITED'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_live'
  exec qp_drop_existing @table_name, 'IsView'
  
  set @table_name = @base_table_name + '_stage'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_new'
  exec qp_drop_existing @table_name, 'IsView'
  
  set @table_name = @base_table_name + '_async_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_united_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_live_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_stage_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_item_versions'
  exec qp_drop_existing @table_name, 'IsView'
END
GO


exec qp_drop_existing 'qp_rebuild_all_new_views', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_rebuild_all_new_views]  
AS
begin

declare @content_id numeric
declare contents CURSOR FOR select content_id from content

open contents
fetch next from contents into @content_id
while @@fetch_status = 0
begin
	exec qp_content_new_views_recreate @content_id
    fetch next from contents into @content_id
end
close contents
deallocate contents

end
GO

exec qp_drop_existing 'STATUS_TYPE_NEW', 'IsView'
GO

create view STATUS_TYPE_NEW as
select cast(site_id as int) as site_id, cast(status_type_id as int) as id, status_type_name as name, cast([weight] as int) as weight from STATUS_TYPE
GO
exec qp_drop_existing 'USER_NEW', 'IsView'
GO

create view USER_NEW as
select cast([user_id] as int) as [id], [login], nt_login, l.iso_code, first_name, last_name, email from users u
inner join LANGUAGES l on l.language_id = u.language_id
GO

exec qp_drop_existing 'USER_GROUP_NEW', 'IsView'
GO

create view USER_GROUP_NEW as
select cast([group_id] as int) as [id], group_name as name from user_group
GO

exec qp_drop_existing 'USER_GROUP_BIND_NEW', 'IsView'
GO

create view USER_GROUP_BIND_NEW as
select cast([group_id] as int) as [group_id], cast([user_id] as int) as [user_id]
from USER_GROUP_BIND
GO


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


ALTER TRIGGER [dbo].[ti_item_to_item] ON [dbo].[item_to_item] AFTER INSERT
AS 
BEGIN

if object_id('tempdb..#disable_ti_item_to_item') is null
	begin
		with items (link_id, item_id, linked_item_id)
		AS
		(
			select i1.link_id, i1.l_item_id, i1.r_item_id From inserted i1
			inner join content_to_content c2c on i1.link_id = c2c.link_id 
			where c2c.[symmetric] = 1 and not exists (select * from item_to_item i2 where i1.link_id = i2.link_id and i1.r_item_id = i2.l_item_id and i2.r_item_id = i1.l_item_id)
		)
		insert into item_to_item(link_id, l_item_id, r_item_id)
		select link_id, linked_item_id, item_id from items
	end

	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct i.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from inserted i inner join content_to_content c2c on i.link_id = c2c.link_id 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct l_item_id, r_item_id from inserted where link_id = @link_id

		if @is_symmetric = 1
		begin
			insert into @link_items
			select r_item_id, l_item_id from inserted i where link_id = @link_id
			and not exists (select * from @link_items where id = i.r_item_id and linked_id = i.l_item_id)
		end

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 


		exec qp_insert_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
		exec qp_insert_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO


ALTER TRIGGER [dbo].[td_item_to_item] ON [dbo].[item_to_item] AFTER DELETE
AS 
BEGIN
	if object_id('tempdb..#disable_td_item_to_item') is null
	begin
		delete item_to_item from item_to_item ii 
			inner join deleted d on ii.link_id = d.link_id and ii.l_item_id = d.r_item_id and ii.r_item_id = d.l_item_id
			inner join content_to_content c2c on d.link_id = c2c.link_id
			where c2c.[symmetric] = 1
	end

	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct d.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from deleted d inner join content_to_content c2c on d.link_id = c2c.link_id
	
		declare @count numeric
		select @count = count(*) from @links
		print (@count) 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct l_item_id, r_item_id from deleted where link_id = @link_id

		if @is_symmetric = 1
		begin
			insert into @link_items
			select distinct r_item_id, l_item_id from deleted d where link_id = @link_id
			and not exists (select * from @link_items where id = d.r_item_id and linked_id = d.l_item_id)
		end

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 

		exec qp_delete_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
		exec qp_delete_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO

exec qp_drop_existing 'ti_item_link_async', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[ti_item_link_async] ON [dbo].[item_link_async] AFTER INSERT
AS 
BEGIN

	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct i.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from inserted i inner join content_to_content c2c on i.link_id = c2c.link_id 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct item_id, linked_item_id from inserted where link_id = @link_id

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 

		exec qp_insert_link_table_item @link_id, @l_content_id, @link_items, 1, 0, 0
		exec qp_insert_link_table_item @link_id, @r_content_id, @link_items, 1, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO

exec qp_drop_existing 'td_item_link_async', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[td_item_link_async] ON [dbo].[item_link_async] AFTER DELETE
AS 
BEGIN
	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct d.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from deleted d inner join content_to_content c2c on d.link_id = c2c.link_id
	
		declare @count numeric
		select @count = count(id) from @links
		print (@count) 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct item_id, linked_item_id from deleted where link_id = @link_id

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 

		exec qp_delete_link_table_item @link_id, @l_content_id, @link_items, 1, 0, 0
		exec qp_delete_link_table_item @link_id, @r_content_id, @link_items, 1, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO
