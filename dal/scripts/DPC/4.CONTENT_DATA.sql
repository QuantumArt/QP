IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CONTENT_DATA]') AND name = 'SPLITTED')
    ALTER TABLE [dbo].[CONTENT_DATA] ADD [SPLITTED] [bit] NOT NULL DEFAULT ((0))
GO

