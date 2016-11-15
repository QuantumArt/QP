ALTER PROCEDURE [dbo].[qp_replicate] 
@content_item_id numeric
AS
BEGIN
	declare @list nvarchar(30)
	set @list = convert(nvarchar, @content_item_id)
	exec qp_replicate_items @list, '', 0
END
GO
