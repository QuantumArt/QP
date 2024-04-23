IF NOT EXISTS(select * from sys.tables where name = 'BACKEND_ACTION_LOG_USER_GROUPS')
BEGIN
    CREATE TABLE dbo.BACKEND_ACTION_LOG_USER_GROUPS
    (
        ID int NOT NULL IDENTITY(1,1) PRIMARY KEY,
        BACKEND_ACTION_LOG_ID int NOT NULL
            CONSTRAINT FK_BACKEND_ACTION_LOG_USER_GROUPS_BACKEND_ACTION_LOG_ID FOREIGN KEY REFERENCES dbo.BACKEND_ACTION_LOG(ID),
        GROUP_ID NUMERIC(18,0) NOT NULL
    ) ON [PRIMARY]
END