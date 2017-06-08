exec qp_drop_existing 'Products', 'IsUserTable'
GO

exec qp_drop_existing 'ProductRegions', 'IsUserTable'
GO

exec qp_drop_existing 'ProductRelevance', 'IsUserTable'
GO

exec qp_drop_existing 'RegionUpdates', 'IsUserTable'
GO

exec qp_drop_existing 'ClearAllProducts', 'IsProcedure'
GO

exec qp_drop_existing 'DeleteProduct', 'IsProcedure'
GO

exec qp_drop_existing 'RegionUpdated', 'IsProcedure'
GO

if not exists(select * from sys.tables where name = 'Products')
BEGIN
    CREATE TABLE [dbo].[Products]
    (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [DpcId] [int] NOT NULL,
        [Slug] [nvarchar](50) NULL,
        [Version] [int] NOT NULL CONSTRAINT [DF_Products_Version] DEFAULT (1),
        [IsLive] [bit] NOT NULL CONSTRAINT [DF_Products_IsLive] DEFAULT (1),
        [Language] [nvarchar](10) NULL,
        [Format] [nvarchar](10) NOT NULL,
        [Data] [nvarchar](max) NOT NULL,
        [Alias] [nvarchar](250) NULL,
        [Created] [datetime] NOT NULL,
        [Updated] [datetime] NOT NULL,
        [Hash] [nvarchar](2000) NOT NULL,
        [MarketingProductId] [int] NULL,
        [Title] [nvarchar](500) NULL,
        [UserUpdated] [nvarchar](50) NULL,
        [UserUpdatedId] [int] NULL,
        [ProductType] [nvarchar](250) NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END

if not exists (select * from sys.indexes where name = 'IX_Products')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Products] ON [dbo].[Products]
    (
        [DpcId] ASC,
        [Slug] ASC,
        [Version] ASC,
        [IsLive] ASC,
        [Language] ASC,
        [Format] ASC
    )
END
GO


if not exists(select * from sys.tables where name = 'ProductRegions')
BEGIN
    CREATE TABLE [dbo].[ProductRegions]
    (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [RegionId] [int] NOT NULL,
        CONSTRAINT [PK_ProductRegions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ProductRegions_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id])
    )
END
GO
if not exists(select * from sys.tables where name = 'ProductRelevance')
BEGIN
    CREATE TABLE [dbo].[ProductRelevance]
    (
        [ProductId] [int] NOT NULL,
        [LastUpdateTime] [datetime] NOT NULL CONSTRAINT [DF_ProductRelevance_LastUpdateTime] DEFAULT (getdate()),
        [StatusID] [tinyint] NOT NULL,
        [IsLive] [bit] NOT NULL CONSTRAINT [DF_ProductRelevance_IsLive] DEFAULT (1),
        CONSTRAINT [PK_ProductRelevance] PRIMARY KEY NONCLUSTERED ([ProductID] ASC, [IsLive] ASC),
        CONSTRAINT [FK_ProductRelevance_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id])
    )
END
GO
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
CREATE PROCEDURE [dbo].[DeleteProduct]
    @id int
AS
BEGIN
    delete from ProductRegions with(rowlock) where ProductId = @id
    delete from Products with(rowlock) where Id = @id
END
GO
CREATE TABLE [dbo].[RegionUpdates](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Updated] [datetime] NOT NULL,
    [RegionId] [int] NULL,
 CONSTRAINT [PK_RegionUpdates] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO
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
