if not exists(select * from sys.tables where name = 'PLUGIN_FIELD')
BEGIN
	CREATE TABLE dbo.PLUGIN_FIELD(
		ID numeric(18,0) IDENTITY(1,1) PRIMARY KEY,
		PLUGIN_ID numeric(18,0) NOT NULL
		    CONSTRAINT FK_PLUGIN_FIELD_PLUGIN_ID FOREIGN KEY REFERENCES dbo.PLUGIN (ID) ON DELETE CASCADE,
        NAME nvarchar(255) NOT NULL,
        DESCRIPTION nvarchar(max) NULL,
	    VALUE_TYPE nvarchar(50) NOT NULL,
	    RELATION_TYPE nvarchar(50) NOT NULL,
	    [ORDER] int NOT NULL DEFAULT (0),
	) ON [PRIMARY]

END
GO

if not exists(select * from sys.indexes where name = 'IX_PLUGIN_FIELD_NAME' and [object_id] = object_id('PLUGIN_FIELD'))
begin
    CREATE UNIQUE INDEX IX_PLUGIN_FIELD_NAME ON PLUGIN_FIELD(PLUGIN_ID, NAME)
end
GO
--drop table plugin_field