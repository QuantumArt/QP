ALTER PROCEDURE [dbo].[qp_merge_article]
@item_id numeric,
@last_modified_by numeric = 1
AS
BEGIN
	DECLARE @ids [Ids]
	insert into @ids
	select @item_id
	EXEC qp_merge_articles @ids, @last_modified_by, 0
END
GO
