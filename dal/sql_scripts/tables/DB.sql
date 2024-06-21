IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_TOKENS')
	ALTER TABLE [dbo].[DB] ADD [USE_TOKENS] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_DPC')
	ALTER TABLE [dbo].[DB] ADD [USE_DPC] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_CDC')
  ALTER TABLE [dbo].[DB] ADD [USE_CDC] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_S3')
    ALTER TABLE [dbo].[DB] ADD [USE_S3] [bit] NOT NULL DEFAULT ((0))
GO
