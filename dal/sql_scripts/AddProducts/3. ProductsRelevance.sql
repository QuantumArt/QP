if not exists(select * from sys.tables where name = 'ProductRelevance')
BEGIN
    CREATE TABLE [dbo].[ProductRelevance]
    (
        [ProductId] [int] NOT NULL,
        [LastUpdateTime] [datetime] NOT NULL CONSTRAINT [DF_ProductRelevance_LastUpdateTime] DEFAULT (getdate()),
        [StatusID] [tinyint] NOT NULL,
        [IsLive] [bit] NOT NULL CONSTRAINT [DF_ProductRelevance_IsLive] DEFAULT (1),
        CONSTRAINT [PK_ProductRelevance] PRIMARY KEY NONCLUSTERED ([ProductID] ASC, [IsLive] ASC),
        CONSTRAINT [FK_ProductRelevance_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id])
    )
END
GO
