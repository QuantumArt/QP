if not exists(select * from sys.tables where name = 'ProductVersions')
BEGIN
	CREATE TABLE [dbo].[ProductVersions](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Deleted] [bit] NOT NULL,
		[Modification] [datetime] NOT NULL,
		[DpcId] [int] NOT NULL,
		[Slug] [nvarchar](50) NULL,
		[Version] [int] NOT NULL,
		[IsLive] [bit] NOT NULL,
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
		[ProductType] [nvarchar](250) NULL
		CONSTRAINT [PK_ProductVersions] PRIMARY KEY CLUSTERED ([Id] ASC )	
	)
END
GO

if exists (select * from sys.indexes where name = 'IX_ProductVersions')
	DROP INDEX [dbo].[ProductVersions].[IX_ProductVersions]

CREATE NONCLUSTERED INDEX [IX_ProductVersions] ON [dbo].[ProductVersions]
(
    [DpcId] ASC,
    [IsLive] ASC,
    [Language] ASC,
    [Format] ASC,
	[Modification] ASC
)
GO