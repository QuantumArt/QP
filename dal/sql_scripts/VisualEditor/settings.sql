IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[SITE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
  ALTER TABLE [dbo].[SITE]
  ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ATTRIBUTE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
  ALTER TABLE [dbo].[CONTENT_ATTRIBUTE]
  ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

EXEC qp_update_translations 'Disable list auto wrapping (ul, ol, dl)', 'Отключить автоматическое оборачивание списков (ul, ol, dl)'
GO
