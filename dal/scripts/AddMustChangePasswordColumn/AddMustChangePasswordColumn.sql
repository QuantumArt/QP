IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[USERS]') AND name = 'MUST_CHANGE_PASSWORD')
  ALTER TABLE [dbo].[USERS] ADD [MUST_CHANGE_PASSWORD] [bit] NOT NULL CONSTRAINT DF_MUST_CHANGE_PASSWORD DEFAULT 0 