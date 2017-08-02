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


