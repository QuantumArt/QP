exec qp_drop_existing 'qp_split_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_split_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1
AS
BEGIN
  declare @contentIds [Ids], @content_id numeric
  declare @ids2 table (content_id numeric, id numeric)
  insert into @ids2 select content_id, id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID

  insert into @contentIds
  select distinct content_id from @ids2

  while exists(select * From @contentIds)
  begin
    select @content_id = id from @contentIds

    declare @ids3 [Ids]
    declare @sql nvarchar(max)
    declare @cstr nvarchar(20)

    insert into @ids3
    select id from @ids2 where content_id = @content_id

    set @cstr = cast(@content_id as nvarchar(max))

    set @sql = 'insert into content_' + @cstr + '_async select * from content_' + @cstr + ' c where content_item_id in (select id from @ids) and not exists(select * from content_' + @cstr + '_async a where a.content_item_id = c.content_item_id)'
    exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids3

    delete from @contentIds where id = @content_id
  end

  insert into item_link_async select * from item_to_item ii where l_item_id in (select id from @ids)
  and link_id in (select link_id from content_attribute where content_id in (select id from @contentIds))
  and not exists (select * from item_link_async ila where ila.item_id = ii.l_item_id)

END
GO
