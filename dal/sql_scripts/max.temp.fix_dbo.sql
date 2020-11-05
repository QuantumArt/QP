USE [publishing]
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'DISABLE_CHANGING_ACTIONS' and TABLE_NAME = 'CONTENT')
ALTER TABLE CONTENT
  ADD DISABLE_CHANGING_ACTIONS BIT NOT NULL CONSTRAINT DF_CONTENT_DISABLE_CHANGING_ACTIONS DEFAULT (0)
GO