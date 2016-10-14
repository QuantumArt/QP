exec qp_drop_existing 'qp_update_links', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_links]
    @ids Ids READONLY, 
    @id numeric,
    @link_id numeric

AS
BEGIN

    declare @ids2 Ids
    declare @ids3 Ids

    insert into @ids2 
    select * from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3

        insert into item_to_item (l_item_id, r_item_id, link_id) 
        select id, @id, @link_id
        from @ids3 i where not exists(
            select * from item_link il where il.item_id = i.id and link_id = @link_id and il.linked_item_id = @id
        )
    end

END
GO