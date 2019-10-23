IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'START_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD START_DATE DATETIME NULL
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'END_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD END_DATE DATETIME NULL
GO
