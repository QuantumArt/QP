exec qp_drop_existing 'qp_fast_delete', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_fast_delete]
    @ids Ids READONLY
AS
BEGIN
    select 1 as A into #disable_td_delete_item_o2m_nullify

    declare @ids2 table (id numeric primary key)
    declare @ids3 table (id numeric primary key)

    insert into @ids2
    select id from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3
        delete content_item from content_item ci inner join @ids3 i on ci.content_item_id = i.id
    end

    drop table  #disable_td_delete_item_o2m_nullify
END
GO
