exec qp_drop_existing 'CdcLastExecutedLsn', 'IsUserTable'
GO

CREATE TABLE [dbo].[CdcLastExecutedLsn] (
  [Id] [int] IDENTITY(1,1) NOT NULL,
  [ProviderUrl] [nvarchar](1024) NOT NULL,
  [TransactionLsn] [varchar](22) NULL,
  [TransactionDate] [datetime] NULL,
  [LastExecutedLsn] [varchar](22) NOT NULL,
  PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO
