IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CONTENT]') AND name = 'FOR_REPLICATION')
  ALTER TABLE [dbo].[CONTENT] ADD [FOR_REPLICATION] [bit] NOT NULL DEFAULT ((1))
GO
