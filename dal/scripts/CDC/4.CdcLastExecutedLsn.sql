IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CdcLastExecutedLsn]') AND name = 'ProviderName')
BEGIN
  DROP TABLE [dbo].[CdcLastExecutedLsn];
  PRINT 'DROP [dbo].[CdcLastExecutedLsn]';
END
GO

IF dbo.qp_check_existence('CdcLastExecutedLsn', 'IsUserTable') = 0
BEGIN
  CREATE TABLE [dbo].[CdcLastExecutedLsn] (
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [ProviderName] [nvarchar](512) NOT NULL,
    [ProviderUrl] [nvarchar](1024) NOT NULL,
    [TransactionLsn] [varchar](22) NULL,
    [TransactionDate] [datetime] NULL,
    [LastExecutedLsn] [varchar](22) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
  );

  PRINT 'CREATE [dbo].[CdcLastExecutedLsn]';
END
GO
