CREATE PROCEDURE [dbo].[RegionUpdated]
    -- Add the parameters for the stored procedure here
    @regionId int
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    declare @hasRecord int = 0

   set @hasRecord = (select COUNT(*) from RegionUpdates
    where RegionId = @regionId)

    if(@hasRecord > 0 )

    update RegionUpdates set Updated = getdate() where RegionId = @regionId

    else
     insert into RegionUpdates (RegionId, Updated) values (@regionId, getdate())

END
GO
