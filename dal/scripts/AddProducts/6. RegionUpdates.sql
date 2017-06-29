CREATE TABLE [dbo].[RegionUpdates](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Updated] [datetime] NOT NULL,
    [RegionId] [int] NULL,
 CONSTRAINT [PK_RegionUpdates] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO
