CREATE PROCEDURE [dbo].[DeleteProduct]
    @id int
AS
BEGIN
    delete from ProductRegions with(rowlock) where ProductId = @id
    delete from Products with(rowlock) where Id = @id
END
GO
