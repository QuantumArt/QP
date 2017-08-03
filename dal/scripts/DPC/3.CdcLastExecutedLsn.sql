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
  )
END
GO
