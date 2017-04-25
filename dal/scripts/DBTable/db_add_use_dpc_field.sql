IF NOT EXISTS (  SELECT * FROM   sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_DPC')
	ALTER TABLE [dbo].[DB] ADD [USE_DPC] [bit] NOT NULL DEFAULT ((0))