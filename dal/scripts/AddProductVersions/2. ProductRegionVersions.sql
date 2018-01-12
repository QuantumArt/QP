if not exists(select * from sys.tables where name = 'ProductRegionVersions')
BEGIN
	CREATE TABLE [dbo].[ProductRegionVersions](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ProductVersionId] [int] NOT NULL,
		[RegionId] [int] NOT NULL
		CONSTRAINT [PK_ProductRegionVersions] PRIMARY KEY CLUSTERED ([Id] ASC)
		CONSTRAINT [FK_ProductRegionVersions_ProductVersions] FOREIGN KEY([ProductVersionId]) REFERENCES [dbo].[ProductVersions] ([Id])
	)
END
GO