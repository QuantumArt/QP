exec qp_drop_existing 'qp_remove_old_aggregates', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_remove_old_aggregates](@id numeric)
as
begin
    DECLARE @ids [Ids]
    INSERT INTO @ids VALUES(@id)
    DELETE FROM content_item WITH(ROWLOCK) WHERE content_item_id IN (SELECT id FROM dbo.qp_aggregates_to_remove(@ids))
end
GO
