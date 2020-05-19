
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
