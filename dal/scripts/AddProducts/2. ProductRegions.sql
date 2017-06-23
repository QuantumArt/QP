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
