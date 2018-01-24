IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_TO_ITEM]') AND name = 'IS_REV')
	ALTER TABLE [dbo].[ITEM_TO_ITEM] ADD [IS_REV] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_LINK_ASYNC]') AND name = 'IS_REV')
	ALTER TABLE [dbo].[ITEM_LINK_ASYNC] ADD [IS_REV] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_TO_ITEM]') AND name = 'IS_SELF')
	ALTER TABLE [dbo].[ITEM_TO_ITEM] ADD [IS_SELF] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_LINK_ASYNC]') AND name = 'IS_SELF')
	ALTER TABLE [dbo].[ITEM_LINK_ASYNC] ADD [IS_SELF] [bit] NOT NULL DEFAULT ((0))
GO

ALTER VIEW [dbo].[item_link] AS
SELECT ii.link_id AS link_id, ii.l_item_id AS item_id, ii.r_item_id AS linked_item_id, ii.is_rev, ii.is_self
FROM item_to_item AS ii
GO

ALTER VIEW [dbo].[item_link_united] AS
select link_id, item_id, linked_item_id, is_rev, is_self from item_link il where not exists (select * from content_item_splitted cis where il.item_id = cis.CONTENT_ITEM_ID)
union all
SELECT link_id, item_id, linked_item_id, is_rev, is_self from item_link_async ila
GO

ALTER VIEW [dbo].[site_item_link] AS
SELECT l.link_id, l.l_item_id, l.r_item_id, c.site_id
FROM item_to_item AS l
  LEFT OUTER JOIN content_item AS i ON i.content_item_id = l.l_item_id
  LEFT OUTER JOIN content AS c ON c.content_id = i.content_id
GO

exec sp_refreshview 'item_link_united_full'

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

  if @r_content_id <> @l_content_id
  BEGIN
    update item_link set is_rev = 1 from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID where il.link_id = @link_id and ci.CONTENT_ID <> @l_content_id and is_rev = 0
    update item_link_async set is_rev = 1 from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID where il.link_id = @link_id and ci.CONTENT_ID <> @l_content_id and il.is_rev = 0
  END

  if @r_content_id = @l_content_id
  BEGIN
    update item_link set is_self = 1 where link_id = @link_id and is_self = 0
    update item_link_async set is_self = 1 where link_id = @link_id and is_self = 0
  END

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

  declare @is_self BIT
  select @is_self = CASE WHEN l_content_id = r_content_id THEN 1 ELSE 0 END from content_to_content where link_id = @link_id

  declare @source_name nvarchar(20)
  set @source_name = CASE WHEN @is_async = 1 THEN 'item_link_async' ELSE 'item_link' END

  if @is_async = 1
    set @table_name = @table_name + '_async'

  if @use_reverse_table = 1
    set @table_name =  @table_name + '_rev'

  declare @sql nvarchar(max)

  declare @rev_fields nvarchar(50)
  select @rev_fields = case when @reverse_fields = 0 then 'il.id, il.linked_id' else 'il.linked_id, il.id' end

  declare @condition nvarchar(200)
  select @condition = case when @reverse_fields = 0 then 'il2.id = il.id and il2.linked_id = il.linked_id' else 'il2.id = il.linked_id and il2.linked_id = il.id' end

  declare @links2 LINKS

  insert into @links2 select il.* from @links il inner join content_item ci with(nolock) on il.id = ci.CONTENT_ITEM_ID where CONTENT_ID = @content_id

  set @sql = 'insert into ' + @table_name + ' select ' + @rev_fields + ' from @links2 il'
  + ' where not exists(select * from ' + @table_name + ' il2 where ' + @condition + ')'

  exec sp_executesql @sql, N'@link_id numeric, @links2 LINKS READONLY', @link_id = @link_id, @links2 = @links2

  if @is_self = 1
  BEGIN
    set @sql =
    'update ' + @source_name + ' set is_self = 1 from ' + @source_name + ' i with(rowlock) ' +
    'inner join @links2 i2 on i.link_id = @link_id and i.item_id = i2.id and i.linked_item_id = i2.linked_id '
  END

  if @use_reverse_table = 1 and @is_self = 0
  BEGIN
    set @sql =
    'update ' + @source_name + ' set is_rev = 1 from ' + @source_name + ' i with(rowlock) ' +
    'inner join @links2 i2 on i.link_id = @link_id and i.item_id = i2.id and i.linked_item_id = i2.linked_id '
  END

  exec sp_executesql @sql, N'@link_id numeric, @links2 LINKS READONLY', @link_id = @link_id, @links2 = @links2

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
