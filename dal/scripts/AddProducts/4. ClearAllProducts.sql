CREATE PROCEDURE [dbo].[ClearAllProducts]
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    delete from  ProductRelevance
    delete from  ProductRegions
    delete from  Products
END
GO
