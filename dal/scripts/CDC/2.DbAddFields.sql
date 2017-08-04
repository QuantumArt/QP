IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_CDC')
  ALTER TABLE [dbo].[DB] ADD [USE_CDC] [bit] NOT NULL DEFAULT ((0))
GO
