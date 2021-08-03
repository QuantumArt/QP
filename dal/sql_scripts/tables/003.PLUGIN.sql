if not exists(select * from sys.tables where name = 'PLUGIN')
BEGIN
	CREATE TABLE dbo.PLUGIN(
		ID numeric(18,0) IDENTITY(1,1) PRIMARY KEY,
		NAME nvarchar(255) NOT NULL,
		DESCRIPTION nvarchar(max) NULL,
		CODE nvarchar(50) NULL,
        CONTRACT nvarchar(max) NULL,
		VERSION char(10) NULL,
	    [ORDER] int NOT NULL DEFAULT (0),
		SERVICE_URL nvarchar(512) NULL,
		INSTANCE_KEY nvarchar(50) NULL,
	    CREATED datetime NOT NULL  DEFAULT (getdate()),
	    MODIFIED datetime NOT NULL DEFAULT (getdate()),
	    LAST_MODIFIED_BY numeric(18, 0) NOT NULL
	        CONSTRAINT FK_PLUGIN_LAST_MODIFIED_BY FOREIGN KEY REFERENCES dbo.USERS (USER_ID)
	) ON [PRIMARY]
END

--drop table plugin



