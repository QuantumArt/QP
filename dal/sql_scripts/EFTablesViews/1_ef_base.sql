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
				set @type_name = 'decimal(18, ' + cast (@attribute_size as NVARCHAR(2)) + ')'
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
