declare @level int
declare @sql nvarchar(max)
select @level = compatibility_level from sys.databases where name = db_name()
if @level < 110
begin
    print 'Changing compatibility level for ' + db_name() + ' from ' + cast(@level as nvarchar(3)) + ' to 110'
    set @sql = N'ALTER DATABASE ' + db_name() + ' SET compatibility_level = 110 '
    exec sp_executesql @sql
end
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XML_DB_UPDATE]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
  CREATE TABLE dbo.XML_DB_UPDATE (
    Id int IDENTITY,
    Applied datetime NOT NULL,
    Hash nvarchar(100) NOT NULL,
    FileName nvarchar(300) NULL,
    USER_ID int NOT NULL,
    Body nvarchar(max) NULL,
    Version nvarchar(10) NULL,
    CONSTRAINT PK_XML_DB_UPDATE PRIMARY KEY CLUSTERED (Id)
  )
  ON [PRIMARY]
  TEXTIMAGE_ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[XML_DB_UPDATE_ACTIONS]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
  CREATE TABLE dbo.XML_DB_UPDATE_ACTIONS (
    Id int IDENTITY,
    UpdateId int NULL,
    Ids nvarchar(max) NOT NULL,
    ParentId int NOT NULL,
    Hash nvarchar(100) NOT NULL,
    Applied datetime NOT NULL,
    UserId int NOT NULL,
    SourceXml nvarchar(max) NOT NULL,
    ResultXml nvarchar(max) NOT NULL,
    CONSTRAINT PK_XML_DB_UPDATE_ACTIONS_Id PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_XML_DB_UPDATE_ACTIONS_UpdateId FOREIGN KEY (UpdateId) REFERENCES dbo.XML_DB_UPDATE (Id)
  )
  ON [PRIMARY]
  TEXTIMAGE_ON [PRIMARY]
END

if not exists(select * from sys.tables where name = 'ACCESS_TOKEN')
BEGIN
	CREATE TABLE [dbo].[ACCESS_TOKEN](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[UserId] [int] NOT NULL,
		[Token] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
		[ExpirationDate] [datetime] NULL,
		[Application] [nvarchar](200) NOT NULL,
		[SessionId] [int] NOT NULL,
	CONSTRAINT [PK_ACCESS_TOKEN] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]	
END


if not exists (select * from sys.indexes where name = 'IX_ACCESS_TOKEN_Session')
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_ACCESS_TOKEN_Session] ON [dbo].[ACCESS_TOKEN]
	(
		[UserId] ASC,
		[Application] ASC,
		[SessionId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

if not exists (select * from sys.indexes where name = 'IX_ACCESS_TOKEN_Token')
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_ACCESS_TOKEN_Token] ON [dbo].[ACCESS_TOKEN]
	(
		[Token] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	ALTER TABLE [dbo].[ACCESS_TOKEN] ADD  CONSTRAINT [DF_ACCESS_TOKEN_Token]  DEFAULT (newid()) FOR [Token]
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CdcLastExecutedLsn]') AND name = 'ProviderName')
AND EXISTS (select * from information_schema.tables where table_name = 'CdcLastExecutedLsn')
BEGIN
  DROP TABLE [dbo].[CdcLastExecutedLsn];
  PRINT 'DROP [dbo].[CdcLastExecutedLsn]';
END
GO

IF NOT EXISTS (select * from information_schema.tables where table_name = 'CdcLastExecutedLsn')
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

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CONTENT]') AND name = 'FOR_REPLICATION')
  ALTER TABLE [dbo].[CONTENT] ADD [FOR_REPLICATION] [bit] NOT NULL DEFAULT ((1))
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT' AND COLUMN_NAME = 'TRACE_IMPORT_SCRIPT')
BEGIN
    ALTER TABLE CONTENT ADD TRACE_IMPORT_SCRIPT NVARCHAR(MAX) NULL
END
GO



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'OPTIMIZE_FOR_HIERARCHY')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD OPTIMIZE_FOR_HIERARCHY BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_OPTIMIZE_FOR_HIERARCHY DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'IS_LOCALIZATION')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD IS_LOCALIZATION BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_IS_LOCALIZATION DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_SEPARATE_REVERSE_VIEWS')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD USE_SEPARATE_REVERSE_VIEWS BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_USE_SEPARATE_REVERSE_VIEWS DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ATTRIBUTE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
    ALTER TABLE [dbo].[CONTENT_ATTRIBUTE]
    ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'TA_HIGHLIGHT_TYPE')
	ALTER TABLE dbo.CONTENT_ATTRIBUTE
	ADD TA_HIGHLIGHT_TYPE nvarchar(50) NULL
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'MAX_DATA_LIST_ITEM_COUNT')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD MAX_DATA_LIST_ITEM_COUNT numeric(18,0) NOT NULL
	CONSTRAINT DF_MAX_DATA_LIST_ITEM_COUNT DEFAULT 10
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'TRACE_IMPORT')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD TRACE_IMPORT BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_TRACE_IMPORT DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'DENY_PAST_DATES')
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD DENY_PAST_DATES BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_DENY_PAST_DATES DEFAULT 0
END
GO




IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CONTENT_DATA]') AND name = 'SPLITTED')
    ALTER TABLE [dbo].[CONTENT_DATA] ADD [SPLITTED] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_DATA' AND COLUMN_NAME = 'O2M_DATA')
BEGIN
    ALTER TABLE CONTENT_DATA ADD O2M_DATA NUMERIC(18, 0) NULL
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ITEM' AND COLUMN_NAME = 'UNIQUE_ID')
BEGIN
  ALTER TABLE dbo.CONTENT_ITEM ADD UNIQUE_ID uniqueidentifier NOT NULL CONSTRAINT DF_CONTENT_ITEM_UNIQUE_ID DEFAULT newid()
END

if not exists(select * from sys.indexes where name = 'IX_UNIQUE_ID' and [object_id] = object_id('CONTENT_ITEM'))
begin
  CREATE UNIQUE NONCLUSTERED INDEX [IX_UNIQUE_ID] ON dbo.CONTENT_ITEM ( UNIQUE_ID )
end

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'START_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD START_DATE DATETIME NULL
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'END_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD END_DATE DATETIME NULL
GO






IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CUSTOM_ACTION' AND COLUMN_NAME = 'ALIAS')
BEGIN
	ALTER TABLE CUSTOM_ACTION ADD ALIAS nvarchar(255) NULL
END
GO

if not exists(select * from sys.indexes where name = 'IX_CUSTOM_ACTION_ALIAS' and [object_id] = object_id('CUSTOM_ACTION'))
begin
  CREATE UNIQUE NONCLUSTERED INDEX [IX_CUSTOM_ACTION_ALIAS] ON [dbo].CUSTOM_ACTION ([ALIAS] ASC) WHERE [ALIAS] IS NOT NULL
end

GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_TOKENS')
	ALTER TABLE [dbo].[DB] ADD [USE_TOKENS] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_DPC')
	ALTER TABLE [dbo].[DB] ADD [USE_DPC] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DB]') AND name = 'USE_CDC')
  ALTER TABLE [dbo].[DB] ADD [USE_CDC] [bit] NOT NULL DEFAULT ((0))
GO


if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'MODIFIED' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
  ALTER TABLE dbo.EXTERNAL_NOTIFICATION_QUEUE ADD
  MODIFIED DATETIME NOT NULL CONSTRAINT DF_EXTERNAL_NOTIFICATION_QUEUE_MODIFIED DEFAULT(GETDATE())
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CONTENT_ID' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
    alter table [EXTERNAL_NOTIFICATION_QUEUE]
    add [CONTENT_ID] numeric NULL
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'SITE_ID' and TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
    alter table [EXTERNAL_NOTIFICATION_QUEUE]
    add [SITE_ID] numeric NULL
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_LINK_ASYNC]') AND name = 'IS_REV')
	ALTER TABLE [dbo].[ITEM_LINK_ASYNC] ADD [IS_REV] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_LINK_ASYNC]') AND name = 'IS_SELF')
	ALTER TABLE [dbo].[ITEM_LINK_ASYNC] ADD [IS_SELF] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_TO_ITEM]') AND name = 'IS_REV')
	ALTER TABLE [dbo].[ITEM_TO_ITEM] ADD [IS_REV] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (  SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[ITEM_TO_ITEM]') AND name = 'IS_SELF')
	ALTER TABLE [dbo].[ITEM_TO_ITEM] ADD [IS_SELF] [bit] NOT NULL DEFAULT ((0))
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SITE' AND COLUMN_NAME = 'EXTERNAL_DEVELOPMENT')
BEGIN
    ALTER TABLE SITE ADD EXTERNAL_DEVELOPMENT BIT NOT NULL CONSTRAINT DF_SITE_EXTERNAL_DEVELOPMENT DEFAULT 1
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SITE' AND COLUMN_NAME = 'DOWNLOAD_EF_SOURCE')
BEGIN
    ALTER TABLE SITE ADD DOWNLOAD_EF_SOURCE BIT NOT NULL CONSTRAINT DF_SITE_DOWNLOAD_EF_SOURCE DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[SITE]') AND name = 'DISABLE_LIST_AUTO_WRAP')
  ALTER TABLE [dbo].[SITE]
  ADD DISABLE_LIST_AUTO_WRAP BIT NOT NULL DEFAULT(0)
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[SITE]') AND name = 'REPLACE_URLS_IN_DB')
  ALTER TABLE [dbo].[SITE]
  ADD REPLACE_URLS_IN_DB BIT NOT NULL DEFAULT(0)
GO

  alter table SITE alter column live_directory nvarchar(255) null
  alter table SITE alter column live_virtual_root nvarchar(255) null
  alter table SITE alter column stage_directory nvarchar(255) null
  alter table SITE alter column stage_virtual_root nvarchar(255) null
GO



IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'STATUS_TYPE' AND COLUMN_NAME = 'ALIAS')
BEGIN
	ALTER TABLE STATUS_TYPE ADD ALIAS nvarchar(255) NULL
END
GO

IF EXISTS (select * from information_schema.tables where table_name = 'SYSTEM_NOTIFICATION_QUEUE')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SYSTEM_NOTIFICATION_QUEUE]') AND name = 'CdcLastExecutedLsnId')
    BEGIN
      DROP TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE];
      PRINT 'DROP [dbo].[SYSTEM_NOTIFICATION_QUEUE]';
    END
END
GO

IF NOT EXISTS (select * from information_schema.tables where table_name = 'SYSTEM_NOTIFICATION_QUEUE')
BEGIN
  CREATE TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] (
    [ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
    [CdcLastExecutedLsnId] [int] NOT NULL,
    [TRANSACTION_LSN] [varchar](22) NOT NULL,
    [TRANSACTION_DATE] [datetime] NOT NULL,
    [URL] [nvarchar](1024) NOT NULL,
    [TRIES] [numeric](18, 0) NOT NULL,
    [JSON] [nvarchar](max) NULL,
    [SENT] [bit] NOT NULL,
    [CREATED] [datetime] NOT NULL,
    [MODIFIED] [datetime] NOT NULL,
    PRIMARY KEY CLUSTERED([ID] ASC)
);

ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_TRIES]  DEFAULT ((0)) FOR [TRIES];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_SENT]  DEFAULT ((0)) FOR [SENT];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_CREATED]  DEFAULT (getdate()) FOR [CREATED];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD  CONSTRAINT [DF_SYSTEM_NOTIFICATION_QUEUE_MODIFIED]  DEFAULT (getdate()) FOR [MODIFIED];
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE]  WITH CHECK ADD  CONSTRAINT [FK_SYSTEM_NOTIFICATION_QUEUE_SYSTEM_NOTIFICATION_QUEUE] FOREIGN KEY([ID]) REFERENCES [dbo].[SYSTEM_NOTIFICATION_QUEUE] ([ID]);
ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] CHECK CONSTRAINT [FK_SYSTEM_NOTIFICATION_QUEUE_SYSTEM_NOTIFICATION_QUEUE];
PRINT 'CREATE [dbo].[SYSTEM_NOTIFICATION_QUEUE]';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SYSTEM_NOTIFICATION_QUEUE]') AND name = 'LastExceptionMessage')
  ALTER TABLE [dbo].[SYSTEM_NOTIFICATION_QUEUE] ADD [LastExceptionMessage] [nvarchar](max) NULL
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[USERS]') AND name = 'MUST_CHANGE_PASSWORD')
  ALTER TABLE [dbo].[USERS] ADD [MUST_CHANGE_PASSWORD] [bit] NOT NULL CONSTRAINT DF_MUST_CHANGE_PASSWORD DEFAULT 0 

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'VERSION_CONTENT_DATA' AND COLUMN_NAME = 'O2M_DATA')
BEGIN
    ALTER TABLE VERSION_CONTENT_DATA ADD O2M_DATA NUMERIC(18, 0) NULL
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WORKFLOW]') AND name = 'IS_DEFAULT')
  ALTER TABLE [dbo].[WORKFLOW] ADD [IS_DEFAULT] [bit] NOT NULL CONSTRAINT DF_WORKFLOW_IS_DEFAULT DEFAULT 0

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WORKFLOW]') AND name = 'USE_DIRECTION_CONTROLS')
  ALTER TABLE [dbo].[WORKFLOW] ADD [USE_DIRECTION_CONTROLS] [bit] NOT NULL CONSTRAINT DF_WORKFLOW_USE_DIRECTION_CONTROLS DEFAULT 0


exec qp_drop_existing 'STATUS_TYPE_NEW', 'IsView'
GO

create view STATUS_TYPE_NEW as
select cast(site_id as int) as site_id, cast(status_type_id as int) as id, status_type_name as name, cast([weight] as int) as weight from STATUS_TYPE
GO
exec qp_drop_existing 'USER_NEW', 'IsView'
GO

create view USER_NEW as
select cast([user_id] as int) as [id], [login], nt_login, l.iso_code, first_name, last_name, email from users u
inner join LANGUAGES l on l.language_id = u.language_id
GO

exec qp_drop_existing 'USER_GROUP_NEW', 'IsView'
GO

create view USER_GROUP_NEW as
select cast([group_id] as int) as [id], group_name as name from user_group
GO

exec qp_drop_existing 'USER_GROUP_BIND_NEW', 'IsView'
GO

create view USER_GROUP_BIND_NEW as
select cast([group_id] as int) as [group_id], cast([user_id] as int) as [user_id]
from USER_GROUP_BIND
GO

ALTER VIEW [dbo].[item_link] AS
SELECT ii.link_id AS link_id, ii.l_item_id AS item_id, ii.r_item_id AS linked_item_id, ii.is_rev, ii.is_self
FROM item_to_item AS ii
GO

ALTER VIEW [dbo].[item_link_united] AS
select link_id, item_id, linked_item_id, is_rev, is_self from item_link il where not exists (select * from content_item_splitted cis where il.item_id = cis.CONTENT_ITEM_ID)
union all
SELECT link_id, item_id, linked_item_id, is_rev, is_self from item_link_async ila
GO

ALTER VIEW [dbo].[site_item_link] AS
SELECT l.link_id, l.l_item_id, l.r_item_id, c.site_id
FROM item_to_item AS l
  LEFT OUTER JOIN content_item AS i ON i.content_item_id = l.l_item_id
  LEFT OUTER JOIN content AS c ON c.content_id = i.content_id
GO

exec sp_refreshview 'item_link_united_full'

exec qp_drop_existing 'USER_GROUP_BIND_RECURSIVE', 'IsView'
GO

CREATE VIEW dbo.USER_GROUP_BIND_RECURSIVE AS
	WITH UserGroupBind([USER_ID], GROUP_ID) AS   
	(  
		SELECT [USER_ID], GROUP_ID FROM dbo.USER_GROUP_BIND   
		UNION ALL  
		SELECT [USER_ID], PARENT_GROUP_ID FROM UserGroupBind 
		INNER JOIN Group_To_Group on GROUP_ID = Child_Group_Id
	)  
	SELECT [USER_ID], GROUP_ID 
	FROM UserGroupBind
GO  

exec qp_drop_existing 'get_schedule_date', 'IsScalarFunction'
GO

CREATE FUNCTION dbo.get_schedule_date(@dt int, @tm int) returns datetime
AS BEGIN
    declare @result datetime;
    declare @year int, @month int, @day int;
    declare @hour int, @minute int, @second int;

    set @year = @dt / 100;
    set @day = @dt % 100;
    set @month = @year % 100;
    set @year = @year / 100;

    set @hour = @tm / 100;
    set @second = @tm % 100;
    set @minute = @hour % 100;
    set @hour = @hour / 100;

    set @result = DATETIMEFROMPARTS(@year, @month, @day, @hour, @minute, @second, 0);

    return @result;
end
GO
exec qp_drop_existing 'qp_aggregated_and_self', 'IsTableFunction'
GO

CREATE function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as
begin

	declare @ids2 table (id numeric primary key, attribute_id numeric)
	insert into @ids2(id, attribute_id)
	select id, ca.ATTRIBUTE_ID from @itemIds i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID
	inner join CONTENT_ATTRIBUTE ca with(nolock) on ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1

	declare @attrIds Ids
	insert into @attrIds
	select distinct attribute_id from @ids2

	insert into @ids
	select id from @itemIds

	union

	select AGG_DATA.CONTENT_ITEM_ID
	from CONTENT_ATTRIBUTE AGG_ATT with(nolock)
	INNER JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
	where AGG_ATT.AGGREGATED = 1 and AGG_ATT.CLASSIFIER_ATTRIBUTE_ID in (select id from @attrIds)
	and AGG_DATA.O2M_DATA in (select id from @ids2)
	return
end
GO
exec qp_drop_existing 'qp_aggregates_to_remove', 'IsTableFunction'
GO

CREATE FUNCTION [dbo].[qp_aggregates_to_remove](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as
begin

    declare @ids2 Ids
    insert into @ids2
    select id from @itemIds i inner join content_item ci on i.ID = ci.CONTENT_ITEM_ID and ci.SPLITTED = 0
	where exists(select * from CONTENT_ATTRIBUTE ca where ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1)

    if exists (select * from @ids2)
    begin
        insert into @ids

        select AGG_DATA.CONTENT_ITEM_ID
        from CONTENT_ATTRIBUTE ATT
        JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
        JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
        JOIN CONTENT_DATA CLF_DATA with(nolock) ON CLF_DATA.ATTRIBUTE_ID = ATT.ATTRIBUTE_ID AND CLF_DATA.CONTENT_ITEM_ID = AGG_DATA.O2M_DATA
        where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1 AND (CLF_DATA.DATA IS NULL OR CLF_DATA.DATA <> cast(AGG_ATT.CONTENT_ID as nvarchar(8)))
        and ATT.CONTENT_ID in (
            select content_id from content_item with(nolock)
            where content_item_id in (select id from @itemIds)
        )
        AND AGG_DATA.O2M_DATA in (select id from @ids2)
    end

    return
end
GO

ALTER FUNCTION [dbo].[qp_correct_data] (
@data nvarchar(max),
@type_id numeric,
@length numeric,
@default_value nvarchar(255)
) RETURNS nvarchar(max)
AS
BEGIN
	declare @num numeric, @err numeric
	declare @return_data nvarchar(max)
	if @type_id in (1, 7, 8, 12) begin
		set @return_data = left(@data, @length)
	end
	else if @type_id in (2, 3, 11) begin
		if isnumeric(@data) = 1 or @data is null
			set @return_data = @data
		else if isnumeric(@default_value) = 1
			set @return_data = @default_value
		else
			set @return_data = null
	end
	else if @type_id in (4, 5, 6) begin
		if isdate(@data) = 1 or @data is null
			set @return_data = @data
		else if isdate(@default_value) = 1
			set @return_data = @default_value
		else
			set @return_data = null
	end
	else begin
		set @return_data = @data
	end
	RETURN @return_data
END
GO
ALTER function [dbo].[qp_get_version_data](@attribute_id numeric, @version_id numeric) returns nvarchar(max)
as
begin
	declare @result nvarchar(max)
	select @result = convert(nvarchar(max), coalesce(cd.BLOB_DATA, cd.DATA)) from version_content_data cd inner join CONTENT_ATTRIBUTE ca on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID where cd.attribute_id = @attribute_id and content_item_version_id = @version_id
	return @result
end
GO
exec qp_drop_existing 'qp_link_titles', 'IsScalarFunction'
GO

CREATE FUNCTION [dbo].[qp_link_titles](@link_id int, @id int, @display_attribute_id int, @maxlength int)
returns nvarchar(max)
AS
BEGIN

  declare @names table
  (
    name nvarchar(255)
  )
  declare @result nvarchar(max)

  insert into @names
  select coalesce(data, blob_data) from content_data where attribute_id = @display_attribute_id
  and content_item_id in (select linked_item_id from item_link where link_id = @link_id and item_id = @id)

  SELECT @result = COALESCE(@result + ', ', '') +  name  FROM @names

  if @result is null
    set @result = ''

  if (@maxlength > 0 and len(@result) > @maxlength)
    set @result = SUBSTRING(@result, 1, @maxlength) + '...'

  return @result

END
GO

exec qp_drop_existing 'qp_m2o_titles', 'IsScalarFunction'
GO

CREATE FUNCTION [dbo].[qp_m2o_titles](@id int, @field_related_id int, @related_attribute_id int, @maxlength int)
RETURNS nvarchar(max)
AS
BEGIN
	declare @names table
	(
		name nvarchar(255) 
	)
	declare @result nvarchar(max)

	insert into @names
	select coalesce(data, blob_data) from CONTENT_DATA where attribute_id = @field_related_id 
	and content_item_id in (select content_item_id from content_data where ATTRIBUTE_ID = @related_attribute_id and cast(coalesce(data, blob_data) as nvarchar(max)) = CAST(@id AS nvarchar(max)))
	
	SELECT @result = COALESCE(@result + ', ', '') +  name  FROM @names

	if @result is null
		set @result = ''

	if (@maxlength > 0 and len(@result) > @maxlength)
		set @result = SUBSTRING(@result, 1, @maxlength) + '...'
	
	return @result

END
GO


ALTER PROCEDURE [dbo].[create_content_item_version]
    @uid NUMERIC,
    @content_item_id NUMERIC
AS
BEGIN
    declare @ids [Ids]

    insert into @ids values (@content_item_id)

    exec qp_create_content_item_versions @ids, @uid
END
GO


exec qp_drop_existing 'qp_build_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_build_link_table]
@link_id NUMERIC
AS BEGIN

  declare @table_name nvarchar(255), @table_name_rev nvarchar(255), @table_name_async nvarchar(255), @table_name_async_rev nvarchar(255)
  declare @sql nvarchar(max), @rev_sql nvarchar(max), @async_sql nvarchar(max), @async_rev_sql nvarchar(max)
  set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR)
  set @table_name_rev = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_rev'
  set @table_name_async = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async'
  set @table_name_async_rev = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async_rev'

  set @sql = 'CREATE TABLE [dbo].[item_link_' + CAST(@link_id AS NVARCHAR) + ']([id] [int] NOT NULL, [linked_id] [int] NOT NULL, PRIMARY KEY CLUSTERED ([id],[linked_id]))'
  SET @rev_sql = REPLACE(@sql, @table_name, @table_name_rev)
  SET @async_sql = REPLACE(@sql, @table_name, @table_name_async)
  SET @async_rev_sql = REPLACE(@sql, @table_name, @table_name_async_rev)

  exec qp_drop_existing @table_name , 'IsUserTable'
  exec(@sql)

  exec qp_drop_existing @table_name_rev , 'IsUserTable'
  exec(@rev_sql)

  exec qp_drop_existing @table_name_async, 'IsUserTable'
  exec(@async_sql)

  exec qp_drop_existing @table_name_async_rev, 'IsUserTable'
  exec(@async_rev_sql)

END

GO

exec qp_drop_existing 'qp_drop_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_drop_link_table]
@link_id NUMERIC
AS BEGIN

  declare @table_name nvarchar(255)

  set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR)
  exec qp_drop_existing @table_name , 'IsUserTable'

  set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_rev'
  exec qp_drop_existing @table_name , 'IsUserTable'

  set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async'
  exec qp_drop_existing @table_name , 'IsUserTable'

  set @table_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_async_rev'
  exec qp_drop_existing @table_name , 'IsUserTable'
END

GO

exec qp_drop_existing 'qp_fill_link_table', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_fill_link_table]
@link_id numeric
AS
BEGIN
  declare @l_content_id numeric, @r_content_id numeric
  select @l_content_id = l_content_id, @r_content_id = r_content_id from content_to_content where link_id = @link_id


  declare @rev_fields nvarchar(100)
  if @l_content_id <> @r_content_id
  begin
    set @rev_fields = 'il.item_id, il.linked_item_id'
  end
  else
  begin
    set @rev_fields = 'il.linked_item_id as item_id, il.item_id as linked_item_id'
  end

  declare @sql nvarchar(max)
  set @sql = 'insert into item_link_' + cast(@link_id as varchar) + ' select il.item_id, il.linked_item_id from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@l_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar)
  exec(@sql)

  set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_rev select ' + @rev_fields + ' from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@r_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar)
  exec(@sql)

  set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_async select il.item_id, il.linked_item_id from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@l_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar)
  exec(@sql)

  set @sql = 'insert into item_link_' + cast(@link_id as varchar) + '_async_rev select ' + @rev_fields + ' from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = '+ cast(@r_content_id as varchar) + ' and link_id = ' + cast(@link_id as varchar)
  exec(@sql)

  if @r_content_id <> @l_content_id
  BEGIN
    update item_link set is_rev = 1 from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID where il.link_id = @link_id and ci.CONTENT_ID <> @l_content_id and is_rev = 0
    update item_link_async set is_rev = 1 from item_link_async il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID where il.link_id = @link_id and ci.CONTENT_ID <> @l_content_id and il.is_rev = 0
  END

  if @r_content_id = @l_content_id
  BEGIN
    update item_link set is_self = 1 where link_id = @link_id and is_self = 0
    update item_link_async set is_self = 1 where link_id = @link_id and is_self = 0
  END

END
GO

exec qp_drop_existing 'qp_recreate_link_tables', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_recreate_link_tables]
AS

BEGIN
  declare @c table (
    id numeric identity(1,1) primary key,
    link_id numeric
  )

  insert into @c(link_id) select link_id from content_to_content where link_id in
  (select link_id from CONTENT_ATTRIBUTE where link_id is not null)

  declare @i int, @count int, @link_id numeric

  set @i = 1
  select @count = count(id) from @c

  while @i < @count + 1
  begin
    select @link_id = link_id from @c where id = @i
    exec qp_build_link_table @link_id
    exec qp_fill_link_table @link_id

    set @i = @i + 1
  end
END
GO


exec qp_drop_existing 'qp_insert_link_table_item', 'IsProcedure'
GO

exec qp_drop_existing 'qp_delete_link_table_item', 'IsProcedure'
GO

if exists (SELECT * FROM sys.Types WHERE is_user_defined = 1 and name = 'Links')
  exec('DROP TYPE Links')

CREATE TYPE [dbo].[Links] AS TABLE(
  [ID] [numeric](18, 0) NOT NULL,
  [LINKED_ID] [numeric](18, 0) NOT NULL,
  PRIMARY KEY CLUSTERED
(
  [ID] ASC, [LINKED_ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO


CREATE PROCEDURE [dbo].[qp_insert_link_table_item]
@link_id numeric, @content_id numeric, @links LINKS READONLY, @is_async bit, @use_reverse_table bit, @reverse_fields bit
AS
BEGIN

  declare @table_name nvarchar(50)
  set @table_name = 'item_link_' + cast(@link_id as varchar)

  declare @is_self BIT
  select @is_self = CASE WHEN l_content_id = r_content_id THEN 1 ELSE 0 END from content_to_content where link_id = @link_id

  declare @source_name nvarchar(20)
  set @source_name = CASE WHEN @is_async = 1 THEN 'item_link_async' ELSE 'item_link' END

  if @is_async = 1
    set @table_name = @table_name + '_async'

  if @use_reverse_table = 1
    set @table_name =  @table_name + '_rev'

  declare @sql nvarchar(max)

  declare @rev_fields nvarchar(50)
  select @rev_fields = case when @reverse_fields = 0 then 'il.id, il.linked_id' else 'il.linked_id, il.id' end

  declare @condition nvarchar(200)
  select @condition = case when @reverse_fields = 0 then 'il2.id = il.id and il2.linked_id = il.linked_id' else 'il2.id = il.linked_id and il2.linked_id = il.id' end

  declare @links2 LINKS

  insert into @links2 select il.* from @links il inner join content_item ci with(nolock) on il.id = ci.CONTENT_ITEM_ID where CONTENT_ID = @content_id

  set @sql = 'insert into ' + @table_name + ' select ' + @rev_fields + ' from @links2 il'
  + ' where not exists(select * from ' + @table_name + ' il2 where ' + @condition + ')'

  exec sp_executesql @sql, N'@link_id numeric, @links2 LINKS READONLY', @link_id = @link_id, @links2 = @links2

  if @is_self = 1
  BEGIN
    set @sql =
    'update ' + @source_name + ' set is_self = 1 from ' + @source_name + ' i with(rowlock) ' +
    'inner join @links2 i2 on i.link_id = @link_id and i.item_id = i2.id and i.linked_item_id = i2.linked_id '
  END

  if @use_reverse_table = 1 and @is_self = 0
  BEGIN
    set @sql =
    'update ' + @source_name + ' set is_rev = 1 from ' + @source_name + ' i with(rowlock) ' +
    'inner join @links2 i2 on i.link_id = @link_id and i.item_id = i2.id and i.linked_item_id = i2.linked_id '
  END

  exec sp_executesql @sql, N'@link_id numeric, @links2 LINKS READONLY', @link_id = @link_id, @links2 = @links2

END
GO

CREATE PROCEDURE [dbo].[qp_delete_link_table_item]
@link_id numeric, @content_id numeric, @links LINKS READONLY, @is_async bit, @use_reverse_table bit, @reverse_fields bit
AS
BEGIN

  declare @table_name nvarchar(50)
  set @table_name = 'item_link_' + cast(@link_id as varchar)

  if @is_async = 1
    set @table_name = @table_name + '_async'

  if @use_reverse_table = 1
    set @table_name =  @table_name + '_rev'

  declare @sql nvarchar(max)

  declare @condition nvarchar(200)
  select @condition = case when @reverse_fields = 0 then 'src.id = il.id and src.linked_id = il.linked_id' else 'src.id = il.linked_id and src.linked_id = il.id' end

  set @sql = 'delete ' + @table_name + ' from ' + @table_name + ' src inner join @links il on ' + @condition

  print @sql

  exec sp_executesql @sql, N'@link_id numeric, @content_id numeric, @links LINKS READONLY', @link_id = @link_id , @content_id = @content_id, @links = @links

END
GO


ALTER PROCEDURE [dbo].[qp_content_table_drop]
  @content_id numeric
AS BEGIN
  DECLARE @base_table_name VARCHAR(100), @table_name VARCHAR(100)

  SET @base_table_name = 'dbo.content_' + CONVERT(VARCHAR, @content_id)
  exec qp_drop_existing @base_table_name, 'IsView'
  exec qp_drop_existing @base_table_name, 'IsUserTable'

  set @table_name = @base_table_name + '_ASYNC'
  exec qp_drop_existing @table_name, 'IsView'
  exec qp_drop_existing @table_name, 'IsUserTable'

  set @table_name = @base_table_name + '_UNITED'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_live'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_stage'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_async_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_united_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_live_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_stage_new'
  exec qp_drop_existing @table_name, 'IsView'

  set @table_name = @base_table_name + '_item_versions'
  exec qp_drop_existing @table_name, 'IsView'
END
GO

exec qp_drop_existing 'qp_content_new_views_create', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_content_new_views_create]
  @content_id NUMERIC
AS
BEGIN

    if object_id('tempdb..#disable_create_new_views') is null
    begin
        declare @ca table (
			id numeric identity(1,1) primary key,
			attribute_id numeric,
			attribute_name nvarchar(255),
			attribute_size numeric,
			attribute_type_id numeric,
			is_long bit,
			link_id numeric
		)

		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric
		declare @attribute_type_id numeric, @link_id numeric, @is_long bit
		declare @sql nvarchar(max), @result_sql nvarchar(max), @field_sql nvarchar(max)
		declare @field_template nvarchar(max), @type_name nvarchar(20), @cast bit
		declare @i numeric, @count numeric, @preserve_index bit

		set @sql = '
	isnull(cast([CONTENT_ITEM_ID] as int), 0) as [CONTENT_ITEM_ID]
	,isnull(cast([STATUS_TYPE_ID] as int), 0) as [STATUS_TYPE_ID]
	,isnull(cast([VISIBLE] as bit), 0) as [VISIBLE]
	,isnull(cast([ARCHIVE] as bit), 0) as [ARCHIVE]
	,[CREATED]
	,[MODIFIED]
	,isnull(cast([LAST_MODIFIED_BY] as int), 0) as [LAST_MODIFIED_BY]
	'
		insert into @ca (attribute_id, attribute_name, attribute_size, attribute_type_id, link_id, is_long)

		select ca.attribute_id, ca.attribute_name, ca.attribute_size, ca.attribute_type_id, isnull(ca.link_id, 0), ca.IS_LONG
		from CONTENT_ATTRIBUTE ca
		where ca.content_id = @content_id
		order by ATTRIBUTE_ORDER

		set @i = 1
		select @count = count(id) from @ca

		while @i < @count + 1
		begin

			select @attribute_id = attribute_id, @attribute_name = '[' + attribute_name + ']', @attribute_size = attribute_size,
			@attribute_type_id = attribute_type_id, @link_id = link_id, @is_long = is_long
			from @ca where id = @i

			set @cast = 0
			set @field_template = 'cast({0} as {1}) as {0}'

			if @attribute_type_id = 2 and @attribute_size = 0 and @is_long = 1
			begin
				set @type_name = 'bigint'
				set @cast = 1
			end

			else if @attribute_type_id = 11
			or @attribute_type_id = 13
			or @attribute_type_id = 2 and @attribute_size = 0 and @is_long = 0
			begin
				set @type_name = 'int'
				set @cast = 1
			end

			else if @attribute_type_id = 2 and @attribute_size <> 0 and @is_long = 0
			begin
				set @type_name = 'float'
				set @cast = 1
			end

			else if @attribute_type_id = 2 and @attribute_size <> 0 and @is_long = 1
			begin
				set @type_name = 'decimal(18, ' + cast (@attribute_size as NVARCHAR(2)) + ')'
				set @cast = 1
			end

			else if @attribute_type_id = 3
			begin
				set @type_name = 'bit'
				set @cast = 1
				set @field_template = 'cast(isnull({0}, 0) as {1}) as {0}'
			end

			else if @attribute_type_id = 4
			begin
				set @type_name = 'date'
				set @cast = 1
			end

			else if @attribute_type_id = 5
			begin
				set @type_name = 'time'
				set @cast = 1
			end

			else if @attribute_type_id in (9,10)
			begin
				set @type_name = 'nvarchar(max)'
				set @cast = 1
			end

			if @cast = 1
			begin
				set @field_sql = replace(@field_template, '{0}', @attribute_name)
				set @field_sql = replace(@field_sql, '{1}', @type_name)
			end
			else
			begin
				set @field_sql = @attribute_name
			end

			set @sql = @sql + CHAR(13) + CHAR(9) + ',' + @field_sql

			set @i = @i + 1
		end


        SET @result_sql =  ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_new as (select ' + char(13) + @sql
            + char(13) + ' from content_' + cast(@content_id as varchar(20)) + ')'
        print(@result_sql)
        exec(@result_sql)


        declare @virtual_type int
        select @virtual_type = virtual_type from content where content_id = @content_id

        if (@virtual_type <> 3)
        begin
         set @result_sql = ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_async_new as (select ' + char(13) + @sql
          + char(13) + ' from content_' + cast(@content_id as varchar(20)) + '_async )'
         print(@result_sql)
         exec(@result_sql)

         exec qp_content_united_view_create @content_id, 1

        end
         else begin
         set @result_sql = ' create view dbo.content_' + cast(@content_id as varchar(20)) + '_united_new as (select ' + char(13) + @sql
        + char(13) + ' from content_' + cast(@content_id as varchar(20)) + '_united )'
         print(@result_sql)
         exec(@result_sql)
        end

        exec qp_content_frontend_views_create @content_id, 1
    end
END
GO

exec qp_drop_existing 'qp_content_new_views_recreate', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_content_new_views_recreate]
  @content_id NUMERIC
AS
BEGIN

  declare @name nvarchar(50), @base_name nvarchar(20)
  set @base_name = 'dbo.content_' + cast(@content_id as varchar(20))

  set @name = @base_name + '_live_new'
  exec qp_drop_existing @name, 'IsView'

  set @name = @base_name + '_stage_new'
  exec qp_drop_existing @name, 'IsView'

  set @name = @base_name + '_united_new'
  exec qp_drop_existing @name, 'IsView'

  set @name = @base_name + '_async_new'
  exec qp_drop_existing @name, 'IsView'

  set @name = @base_name + '_new'
  exec qp_drop_existing @name, 'IsView'

  exec qp_content_new_views_create @content_id

END
GO

ALTER PROCEDURE [dbo].[qp_content_united_view_create]
  @content_id NUMERIC,
  @is_new bit = 0
AS

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
  set @new_str = '_new'

  DECLARE @str_sql VARCHAR(4000), @char_content_id varchar(20)
  SET @char_content_id = CONVERT(varchar,@content_id)

  SET @str_sql =  ' create view dbo.content_' + @char_content_id + '_united' + @new_str + ' as ' +
      ' select c1.* from content_' + @char_content_id + @new_str + ' c1 ' +
      ' left join content_' + @char_content_id + '_async' + @new_str + ' c2 ' +
      ' on c1.content_item_id = c2.content_item_id ' +
      ' where c2.content_item_id is null ' +
      ' union all ' +
      ' select * from content_' + @char_content_id + '_async' + @new_str
  EXEC(@str_sql)
GO

ALTER PROCEDURE [dbo].[qp_content_united_view_recreate]
  @content_id NUMERIC,
  @is_new bit = 0
AS

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
  set @new_str = '_new'

  DECLARE @str_sql VARCHAR(4000), @view_name varchar(40)

  SET @view_name = 'content_' + CONVERT(varchar,@content_id) + '_united' + @new_str
  EXEC qp_drop_existing @view_name, 'IsView'

  EXEC qp_content_united_view_create @content_id, @is_new
GO

ALTER PROCEDURE [dbo].[qp_content_frontend_views_create]
  @content_id NUMERIC,
  @is_new bit = 0,
  @extra_call bit = 1
AS

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
  set @new_str = '_new'

  DECLARE @str_sql nvarchar(max), @char_content_id nvarchar(20)
  SET @char_content_id = CONVERT(nvarchar,@content_id)

  SET @str_sql =  ' create view dbo.content_' + @char_content_id + '_live' + @new_str + ' as ' +
      ' select * from dbo.content_' + @char_content_id + @new_str + ' where visible = 1 and archive = 0  and status_type_id in (' +
      ' select status_type_id from status_type where status_type_name = ''' + dbo.qp_content_max_status_name(@content_id) + ''')'
  EXEC(@str_sql)

  SET @str_sql = ' create view dbo.content_' + @char_content_id + '_stage' + @new_str + ' as ' +
      ' select * from dbo.content_' + @char_content_id + '_united' + @new_str + ' where visible = 1 and archive = 0  '
  EXEC(@str_sql)

  if (@is_new = 0 and @extra_call = 1)
  exec qp_content_new_views_create @content_id

GO


ALTER PROCEDURE [dbo].[qp_content_frontend_views_recreate]
  @content_id NUMERIC,
  @is_new bit = 0
AS

  declare @new_str nvarchar(5)
  set @new_str = ''
  if @is_new = 1
  set @new_str = '_new'

  declare @view_name nvarchar(50)
  SET @view_name = 'dbo.content_' + CONVERT(varchar,@content_id) + '_live' + @new_str
  EXEC qp_drop_existing @view_name, 'IsView'
  SET @view_name = 'dbo.content_' + CONVERT(varchar,@content_id) + '_stage' + @new_str
  EXEC qp_drop_existing @view_name, 'IsView'

  exec qp_content_frontend_views_create @content_id, @is_new, 0

  if (@is_new = 0)
  exec qp_content_new_views_recreate @content_id

GO

ALTER PROCEDURE [dbo].[qp_drop_link_view]
@link_id NUMERIC
AS
BEGIN
  DECLARE @view_name nvarchar(50)
  set @view_name = 'link_' + CAST(@link_id AS NVARCHAR)
  exec qp_drop_existing @view_name , 'IsView'

  set @view_name = 'link_' + CAST(@link_id AS NVARCHAR) + '_united'
  exec qp_drop_existing @view_name , 'IsView'

  set @view_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united'
  exec qp_drop_existing @view_name , 'IsView'

  set @view_name = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united_rev'
  exec qp_drop_existing @view_name , 'IsView'

  exec qp_drop_link_table @link_id

END
GO

ALTER PROCEDURE [dbo].[qp_build_link_view]
@link_id NUMERIC
AS BEGIN
  DECLARE @content_id numeric, @rev_content_id numeric, @link_str nvarchar(20)
  DECLARE @sql nvarchar(max), @sql2 nvarchar(max), @sql9 nvarchar(max), @sql10 nvarchar(max)
  DECLARE @view_name nvarchar(50), @view_name2 nvarchar(50), @view_name9 nvarchar(50), @view_name10 nvarchar(50)

  select @link_str = cast(@link_id as nvarchar(20))
  select @content_id = l_content_id, @rev_content_id = r_content_id from content_to_content where link_id = @link_id

  set @view_name = 'link_' + CAST(@link_id AS NVARCHAR)
  set @view_name2 = 'link_' + CAST(@link_id AS NVARCHAR) + '_united'
  set @view_name9 = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united'
  set @view_name10 = 'item_link_' + CAST(@link_id AS NVARCHAR) + '_united_rev'

  SET @sql= 'CREATE VIEW dbo.' + @view_name + ' AS select il.item_id, il.linked_item_id from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID  where CONTENT_ID = ' + CAST(@content_id AS NVARCHAR) + ' and link_id = ' + CAST(@link_id AS NVARCHAR)

  SET @sql2 = REPLACE(@sql, @view_name, @view_name2)
  SET @sql2 = REPLACE(@sql2, 'item_link il', 'item_link_united il')

  SET @sql9 = 'CREATE VIEW dbo.' + @view_name9 + ' AS select id, linked_id from item_link_' + @link_str + ' il '
  + ' where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID) '
  + ' union all SELECT id, linked_id from item_link_' + @link_str + '_async ila'

  SET @sql10 = 'CREATE VIEW dbo.' + @view_name10 + ' AS select id, linked_id from item_link_' + @link_str + '_rev il '
  + ' where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID) '
  + ' union all SELECT id, linked_id from item_link_' + @link_str + '_async_rev ila'

  exec qp_drop_existing @view_name , 'IsView'
  exec(@sql)

  exec qp_drop_existing @view_name2 , 'IsView'
  exec(@sql2)

  exec qp_build_link_table @link_id

  exec qp_drop_existing @view_name9, 'IsView'
  exec(@sql9)

  exec qp_drop_existing @view_name10, 'IsView'
  exec(@sql10)

END
GO

exec qp_drop_existing 'qp_rebuild_all_new_views', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_rebuild_all_new_views]
AS
begin

declare @content_id numeric
declare contents CURSOR FOR select content_id from content

open contents
fetch next from contents into @content_id
while @@fetch_status = 0
begin
  exec qp_content_new_views_recreate @content_id
    fetch next from contents into @content_id
end
close contents
deallocate contents

end
GO



ALTER PROCEDURE [dbo].[qp_all_article_search]
    @p_site_id int,
    @p_user_id int,
    @p_searchparam nvarchar(4000),
    @p_order_by nvarchar(max) = N'data.[rank] DESC',
    @p_start_row int = 0,
    @p_page_size int = 0,
    @p_item_id int = null,

    @total_records int OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    declare @is_admin bit
    select @is_admin = dbo.qp_is_user_admin(@p_user_id)

    -- set number of start record by default
    IF (@p_start_row <= 0)
        BEGIN
            SET @p_start_row = 1
        END

    -- set number of finish record
    DECLARE @p_end_row AS int
    SET @p_end_row = @p_start_row + @p_page_size - 1

    -- make a query for subset of contents which enabled to access
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    if @is_admin = 0
    begin
        EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @p_user_id,
                @group_id = 0,
                @start_level = 1,
                @end_level = 4,
                @entity_name = 'content',
                @parent_entity_name = 'site',
                @parent_entity_id = @p_site_id,
                @SQLOut = @security_sql OUTPUT
    end

    -- count all records
    declare @paramdef nvarchar(4000);
    declare @query nvarchar(4000);

    create table #temp
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    create table #temp2
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    set @query = 'insert into #temp' + CHAR(13)
        + ' select content_item_id, weight, attribute_id, priority from ' + CHAR(13)
        + ' (select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority, ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number ' + CHAR(13)
        + ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
        + ' inner join content_data cd on ft.[key] = cd.content_data_id) as c where c.number = 1 order by weight desc ' + CHAR(13)
    print @query

    exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam

    IF @p_item_id is not null
    begin
        set @query = 'if not exists (select * from #temp where content_item_id = ' + cast(@p_item_id as varchar(20)) + ') insert into #temp' + CHAR(13)
        set @query = @query + ' select ' + cast(@p_item_id as varchar(20)) + ', 0, 0, 1 ' + CHAR(13)
		print @query
        exec sp_executesql @query
    end

    set @paramdef = '@site_id int';
    if @is_admin = 0
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c0 on c0.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)
            + ' inner join (' + @security_sql + ') c on c.CONTENT_ID = c0.CONTENT_ID where c0.site_id = @site_id' + CHAR(13)

        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end
    else
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c on c.CONTENT_ID = ci.CONTENT_ID where c.site_id = @site_id' + CHAR(13)
        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end

    select @total_records = count(distinct content_item_id) from #temp2

    -- main query
    declare @query_template nvarchar(4000);
    set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
        + ' (select ROW_NUMBER() OVER (ORDER BY [priority] DESC, <$_order_by_$>) AS ROW, ' + CHAR(13)
        + ' 	ci.CONTENT_ID as ParentId, ' + CHAR(13)
        + ' 	data.CONTENT_ITEM_ID as Id, ' + CHAR(13)
        + ' 	data.ATTRIBUTE_ID as FieldId, ' + CHAR(13)
        + ' 	attr.ATTRIBUTE_TYPE_ID as FieldTypeId, ' + CHAR(13)
        + ' 	c.CONTENT_NAME as ParentName, ' + CHAR(13)
        + ' 	st.STATUS_TYPE_NAME as StatusName, ' + CHAR(13)
        + ' 	ci.CREATED as Created, ' + CHAR(13)
        + ' 	ci.MODIFIED as Modified, ' + CHAR(13)
        + ' 	usr.[LOGIN] as LastModifiedByUser, ' + CHAR(13)
        + ' 	data.[rank] as Rank, ' + CHAR(13)
        + ' 	data.[priority] as [priority], ' + CHAR(13)
		+ '		ci.ARCHIVE as Archive' + CHAR(13)
        + '   from #temp2 data ' + CHAR(13)
        + '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
        + '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
        + '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID and c.site_id = @site_id ' + CHAR(13)
        + '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
        + '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
        + ' ) ' + CHAR(13)
        + ' select ' + CHAR(13)
        + ' 	ParentId, ' + CHAR(13)
        + ' 	ParentName, ' + CHAR(13)
        + ' 	Id, ' + CHAR(13)
        + ' 	FieldId, ' + CHAR(13)
        + '		cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)) as Text, ' + CHAR(13)
        + ' 	dbo.qp_get_article_title_func(Id, ParentId) as Name, ' + CHAR(13)
        + ' 	StatusName, ' + CHAR(13)
        + ' 	pdc.Created, ' + CHAR(13)
        + ' 	pdc.Modified, ' + CHAR(13)
        + ' 	LastModifiedByUser, ' + CHAR(13)
        + ' 	Rank, ' + CHAR(13)
		+ '		pdc.Archive ' + CHAR(13)
        + ' from PAGED_DATA_CTE pdc ' + CHAR(13)
        + ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
        + ' where ROW between @start_row and @end_row order by row asc';


    declare @sortExp nvarchar(4000);
    set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;
    set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);
    set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
    print @query
    EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;

    drop table #temp
    drop table #temp2
END
GO
exec qp_drop_existing 'qp_assert_num_equal', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_assert_num_equal]
@id1 int,
@id2 int,
@msg nvarchar(50)
AS
BEGIN
  declare @text nvarchar(max)
  set @text = @msg + ': '
  if @id1 = @id2
  begin
    set @text = @text + 'OK'
    print @text
  end
  else
  begin
    set @text = @text + 'Failed - %d/%d'
    raiserror(@text, 11, 1, @id1, @id2)
  end
END
GO

exec qp_drop_existing 'qp_count_duplicates', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_count_duplicates]
@content_id numeric, 
@field_ids nvarchar(max),
@ids nvarchar(max) = null,
@includeArchive bit = 0
AS
BEGIN
	declare @field_names_table table (name nvarchar(255))
	insert @field_names_table select ca.ATTRIBUTE_NAME as name from dbo.Split(@field_ids, ',') f inner join CONTENT_ATTRIBUTE ca on f.Items = ca.ATTRIBUTE_ID
	
	declare @currentName nvarchar(255)
	declare @fieldList nvarchar(max)
	set @fieldList = ''
	
	while exists(select * from @field_names_table)
	begin
		select @currentName = name from @field_names_table
		if @fieldList <> ''
			set @fieldList = @fieldList + ', '
		set @fieldList = @fieldList + '[' + @currentName + ']'
		delete from @field_names_table where name = @currentName
	end
	
	declare @where nvarchar(max)
	if @ids is null or @ids = ''
		set @where = ''
	else
		set @where = ' where content_item_id in (' + @ids + ')' 	

	if @includeArchive = 0
		set @where = @where + case when @where ='' then 'where archive = 0' else ' and archive = 0' end

	declare @sql nvarchar(max)
	if @fieldList = ''
		set @sql = 'select 0 as cnt'
	else
		set @sql = 'select coalesce(sum(c.cnt), 0) from (select COUNT(*) as cnt from content_' + CAST(@content_id as nvarchar(20)) + '_united ' + @where + ' group by ' + @fieldList + ' having COUNT(*) > 1) as c'
		
	exec sp_executesql @sql

END
GO
ALTER PROCEDURE [dbo].[qp_create_content_item_versions]
  @ids [Ids] READONLY,
  @last_modified_by NUMERIC
AS
BEGIN
    declare @tm datetime
    select @tm = getdate()

    declare @items table (id numeric, cnt int, last_version_id int, new_version_id int, content_id numeric, max_num int)

    insert into @items (id, cnt)
    select i.ID, count(civ.content_item_version_id) from @ids i
    left join content_item_version civ on civ.content_item_id = i.id
    group by i.ID

    --print 'init completed'


    update @items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
    from @items items
    inner join content_item ci with(nolock) on items.id = ci.CONTENT_ITEM_ID
    inner join content c on c.CONTENT_ID = ci.CONTENT_ID

    --print 'max_num updated'

    declare @delete_ids [Ids]

    insert into @delete_ids
    select content_item_version_id from
    (
        select content_item_id, content_item_version_id,
        row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
        from content_item_version civ
        where content_item_id in (select id from @ids)
    ) c
    inner join @items items
    on items.id = c.content_item_id and c.num >= items.max_num

    DELETE item_to_item_version WHERE content_item_version_id in (select id from @delete_ids)
    DELETE content_item_version WHERE content_item_version_id in (select id from @delete_ids)

    --print 'exceeded deleted'

    DECLARE @NewVersions TABLE(ID INT)

    INSERT INTO content_item_version (version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by)
    output inserted.[CONTENT_ITEM_VERSION_ID] INTO @NewVersions
    SELECT @tm, 'backup', NULL, content_item_id, @last_modified_by, modified, last_modified_by
    from content_item where CONTENT_ITEM_ID in (select id from @ids);

    --print 'versions inserted'

    update @items set new_version_id = zip.version_id
    FROM @items items
    INNER JOIN
    (
        select x.item_id, y.version_id from
        (
        select id as item_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @items
        ) x
        inner join
        (
          select id as version_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @NewVersions
        ) y
        on x.num = y.num
    ) zip
    on zip.item_id = items.id

    --print 'new versions updated'


    -- -- Get Extensions info
    declare @contentIds TABLE
    (
        id numeric
    )

    insert into @contentIds
    select convert(numeric, DATA) as ids
    from
    (
        select distinct DATA from content_data
        where CONTENT_ITEM_ID in (select id from @ids)
        and DATA is not null
        and ATTRIBUTE_ID in (
        select attribute_id from CONTENT_ATTRIBUTE where content_id in (select distinct content_id from @items) and IS_CLASSIFIER = 1)
    ) as p

    --print 'contents defined'

    declare @aggregates TABLE (content_id numeric, attribute_name nvarchar(255), attribute_id numeric)
    insert into @aggregates
    select ca.content_id, ca.ATTRIBUTE_NAME, ca.ATTRIBUTE_ID
    from CONTENT_ATTRIBUTE ca where ca.aggregated = 1 and ca.CONTENT_ID in (select * from @contentIds)

    declare @extensions TABLE (id numeric, agg_id numeric, content_id numeric)
    insert into @extensions
    select O2M_DATA, CONTENT_ITEM_ID, a.content_id from content_data cd
    inner join @aggregates a on cd.ATTRIBUTE_ID = a.attribute_id
    where O2M_DATA in (select id from @ids)

    --print 'extensions received'

    declare @main_ids TABLE
    (
        version_id numeric,
        id numeric,
        content_id numeric
    )

    insert into @main_ids
    select i.new_version_id, e.agg_id, e.content_id from @extensions e inner join @items i on e.id = i.id

    insert into @main_ids
    select i.new_version_id, i.id, i.content_id from @items i

    declare @total numeric
    select @total = count(*) from @main_ids
    print @total

    --print 'main defined'

    -- Store content item data
    INSERT INTO version_content_data (attribute_id, content_item_version_id, data, blob_data, o2m_data, created)
    SELECT attribute_id, m.version_id, data, blob_data, o2m_data, @tm
    FROM content_data cd inner join @main_ids m on cd.CONTENT_ITEM_ID = m.id

    --print 'content_data saved'


    -- Store Many-to-Many slice
    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
    SELECT m.version_id, ca.attribute_id, linked_item_id
    FROM item_link_united AS il
    INNER JOIN content_attribute AS ca ON ca.link_id = il.link_id
    INNER JOIN content_item AS ci ON ci.content_id =  ca.content_id AND ci.content_item_id = il.item_id
    inner join @main_ids m on il.item_id = m.id

    --print 'm2m saved'

    -- Store Many-to-One slice
    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
    SELECT m.version_id, ca.attribute_id, cd.content_item_id
    FROM content_data AS cd
    INNER JOIN content_attribute AS ca ON ca.BACK_RELATED_ATTRIBUTE_ID = cd.ATTRIBUTE_ID
    inner join @main_ids m on cd.O2M_DATA = m.id and ca.CONTENT_ID = m.content_id

    --print 'm2o saved'

    -- Write status history log
    INSERT INTO content_item_status_history
    (content_item_id, user_id, description, created, content_item_version_id, system_status_type_id)
    select id, @last_modified_by, 'Record version backup has been created', @tm, new_version_id, 2
    from @items
END
GO

exec qp_drop_existing 'qp_default_link_ids', 'IsScalarFunction'
GO

CREATE function [dbo].[qp_default_link_ids](@field_id numeric)
returns nvarchar(max)
AS
BEGIN
  declare @result nvarchar(max)
  SELECT @result = COALESCE(@result + ', ', '') +  cast(ARTICLE_ID as nvarchar(20))  FROM FIELD_ARTICLE_BIND where FIELD_ID = @field_id
  return @result
END
GO

exec qp_drop_existing 'qp_fast_delete', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_fast_delete]
    @ids Ids READONLY
AS
BEGIN
    select 1 as A into #disable_td_delete_item_o2m_nullify

    declare @ids2 table (id numeric primary key)
    declare @ids3 table (id numeric primary key)

    insert into @ids2
    select id from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3
        delete content_item from content_item ci inner join @ids3 i on ci.content_item_id = i.id
    end

    drop table  #disable_td_delete_item_o2m_nullify
END
GO

ALTER procedure [dbo].[qp_get_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id

if @content_id is not null
begin
	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id

	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

	set @sql = N'select * from
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME,
	dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ca.attribute_type_id, ca.attribute_size, ca.default_value) as pivot_data
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
	) as src
	PIVOT
	(
	MAX(src.pivot_data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt order by pt.content_item_id desc
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO
exec qp_drop_existing 'qp_get_m2o_ids_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_m2o_ids_multiple]
@contentId numeric,
@fieldName nvarchar(255),
@ids [Ids] READONLY
AS
BEGIN
  declare @sql nvarchar(max)
  set @sql = 'select [' + @fieldName + '], content_item_id from content_' + CAST(@contentId as nvarchar(255)) + '_united where [' + @fieldName + '] in (select id from @ids)'
  exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids
END

GO

exec qp_drop_existing 'qp_GetPermittedItemsAsQuery', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_GetPermittedItemsAsQuery]
(
  @user_id numeric(18,0)=0,
  @group_id numeric(18,0)=0,
  @start_level int=2,
  @end_level int=4,
  @entity_name varchar(100)='content_item',
  @parent_entity_name varchar(100)='',
  @parent_entity_id numeric(18,0)=0,
  @SQLOut varchar(8000) OUTPUT
)
AS

SET NOCOUNT ON

Declare @sPermissionTable varchar(200)
Declare @sHide varchar(50)
Declare @NewLine char(2)
Declare @sUnion varchar(20)
Declare @sSelectUser varchar(200)
Declare @sSelectGroup varchar(8000)
Declare @sSQL varchar(8000)
Declare @srGroupInList varchar (30)
Declare @srLevelIncrement varchar (30)
Declare @sTemp varchar(8000)
Declare @sWhereParentEntity varchar (8000)
Declare @sDefaultSQL varchar (8000)
Declare @sGroupBy varchar (200)
Declare @intIncrement int
Declare @CurrentLevelAddition int
Declare @sSQLStart varchar(300)
Declare @sSQLEnd varchar (600)

/***********************************/
/**** Declare Table Variables   ****/
/***********************************/
declare @ChildGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @ParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)

declare @UsedGroups table
(
	group_id numeric(18,0)
)

declare @TempParentGroups table
(
	group_id numeric(18,0) PRIMARY KEY
)
/***********************************/

select @NewLine = CHAR(13) + CHAR(10)
Select @intIncrement = 10
Select @CurrentLevelAddition = 0
Select @sSQLStart = ' select ' + @entity_name + '_id, cast(min(pl) as int)%10 as permission_level, max(hide) as hide from ('
Select @sSQLEnd = ') as qp_zzz group by qp_zzz.' + @entity_name + '_id HAVING cast(min(pl) as int)%10 >= ' + Cast(@start_level AS varchar) + ' AND cast(min(pl) as int)%10 <= ' + Cast(@end_level AS varchar)

Select @sGroupBy =  ' group by ' + @entity_name + '_id '
Select @sWhereParentEntity = ''
select @sPermissionTable = @entity_name + '_access_PermLevel'

if @parent_entity_name != '' and @parent_entity_id != 0
Begin
   Select @sPermissionTable = @sPermissionTable + '_' + @parent_entity_name
   Select @sWhereParentEntity = ' and ' + @parent_entity_name+ '_id=' + Cast(@parent_entity_id As varchar) + ' '
End

if @entity_name = 'content'
	set @sHide = ', MAX(CONVERT(int, hide)) as hide'
else
	set @sHide = ', 0 as hide'

select @sSQL = ''
select @sTemp = null
Select @srGroupInList = '<@_group_in_list_@>'
Select @srLevelIncrement = '<@_increment_level_@>'
select @sUnion = @NewLine + ' Union All ' + @NewLine
select @sSelectUser = ' select ' + @entity_name + '_id, max(permission_level) as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where user_id=' + Cast(@user_id AS varchar) + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sSelectGroup = ' select ' + @entity_name + '_id, max(permission_level) + ' + @srLevelIncrement + ' as pl' + @sHide + ' from ' + @sPermissionTable +  ' with(nolock) where group_id in (' + @srGroupInList + ')' + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sDefaultSQL = ' select 0 as ' + @entity_name + '_id, 0 as permission_level' + @sHide + ' from ' + @sPermissionTable


if @user_id > 0
Begin
   Select @sSQL = @sSelectUser + @sGroupBy
   insert into @ChildGroups (group_id) select distinct group_id from user_group_bind where user_id = @user_id
   Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
End

if @group_id > 0 AND @user_id <= 0
Begin
   insert into @ChildGroups(group_id) values (@group_id)
End

if (select count(*) from @ChildGroups) = 0
Begin
   if @sSQL != '' Select @SQLOut = @sSQL
   else Select @SQLOut = @sDefaultSQL
   return
End

SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @ChildGroups
if @sSQL != '' Select @sSQL = @sSQL + @sUnion
Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
Select @sSQL = @sSQL + @sGroupBy

insert into @UsedGroups(group_id) select group_id from @ChildGroups

WHILE 1=1
BEGIN
    Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
    select @sTemp = null
	insert into @ParentGroups (group_id) select distinct gtg.parent_group_id from group_to_group gtg inner join @ChildGroups cg on gtg.child_group_id = cg.group_id
    if (select count(*) from @ParentGroups) = 0 BREAK

    /* need to check that parent groups are not appearing in child groups */
    insert into @TempParentGroups (group_id) select pg.group_id from @ParentGroups pg where pg.group_id not in(select cg.group_id from @ChildGroups cg) and pg.group_id not in (select group_id from @UsedGroups)
    if (select count(*) from @TempParentGroups) != 0
    Begin
		SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @TempParentGroups
		if @sSQL != '' Select @sSQL = @sSQL + @sUnion
		Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
		Select @sSQL = @sSQL + @sGroupBy
        insert into @UsedGroups (group_id) select group_id from @TempParentGroups
    End

    delete @ChildGroups
    delete @TempParentGroups
    insert into @ChildGroups (group_id) select (group_id) from @ParentGroups
    delete @ParentGroups
    CONTINUE
END

Select @SQLOut = @sSQLStart + @sSQL + @sSQLEnd
return

GO

ALTER PROCEDURE [dbo].[qp_get_update_cell_sql]
@table_name nvarchar(255),
@content_item_id numeric,
@attribute_id numeric,
@attribute_type_id numeric,
@attribute_size numeric,
@default_value nvarchar(255),
@attribute_name nvarchar(255),
@sql nvarchar(2048) output
AS
BEGIN
	declare @source_function nvarchar(512)
	set @source_function = 'dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ' + convert(nvarchar, @attribute_type_id) + ', ' + convert(nvarchar, @attribute_size) + ', N''' + isnull(@default_value, '') + ''')'

	set @sql = ' update ' + @table_name + ' set [' + @attribute_name + '] = ' + @source_function + ' from content_data cd '
	set @sql = @sql + ' where ' + @table_name + '.content_item_id = ' + convert(nvarchar, @content_item_id)
	set @sql = @sql + ' and cd.attribute_id = ' + convert(nvarchar, @attribute_id) + ' and cd.content_item_id = ' + @table_name + '.content_item_id '
END
GO
ALTER PROCEDURE [dbo].[qp_get_update_column_sql]
@table_name nvarchar(255),
@content_item_ids nvarchar(max),
@attribute_id numeric,
@attribute_type_id numeric,
@attribute_size numeric,
@default_value nvarchar(255),
@attribute_name nvarchar(255),
@sql nvarchar(max) output
AS
BEGIN
	declare @source_function nvarchar(512)
	set @source_function = 'dbo.qp_correct_data( cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ' + convert(nvarchar, @attribute_type_id) + ', ' + convert(nvarchar, @attribute_size) + ', N''' + isnull(@default_value, '') + ''')'

	set @sql = ' update ' + @table_name + ' set [' + @attribute_name + '] = ' + @source_function + ' from ' + @table_name + ' c '
	set @sql = @sql + ' inner join content_data cd on cd.content_item_id = c.content_item_id'
	set @sql = @sql + ' where cd.content_item_id in (' + @content_item_ids + ')'
	set @sql = @sql + ' and cd.attribute_id = ' + convert(nvarchar, @attribute_id)
END
GO

exec qp_drop_existing 'qp_get_upsert_items_sql_new', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_upsert_items_sql_new]
@table_name nvarchar(25),
@sql nvarchar(max) output
as
BEGIN
    set @sql = 'update base set '
    set @sql = @sql + ' base.modified = ci.modified, base.last_modified_by = ci.last_modified_by, base.status_type_id = ci.status_type_id, '
    set @sql = @sql + ' base.visible = ci.visible, base.archive = ci.archive '
    set @sql = @sql + ' from ' + @table_name + ' base with(rowlock) '
    set @sql = @sql + ' inner join content_item ci with(rowlock) on base.content_item_id = ci.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id'
    set @sql = @sql + ';' + CHAR(13) + CHAR(10)

    set @sql = @sql + 'insert into ' + @table_name + ' (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)'
    set @sql = @sql + ' select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by, '
    set @sql = @sql + ' case when i2.id is not null then @noneId else ci.status_type_id end as status_type_id, '
    set @sql = @sql + ' ci.visible, ci.archive '
    set @sql = @sql + ' from content_item ci left join ' + @table_name + ' base on ci.content_item_id = base.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id '
    set @sql = @sql + ' left join @ids2 i2 on ci.content_item_id = i2.id '
    set @sql = @sql + ' where base.content_item_id is null'
END
GO

ALTER procedure [dbo].[qp_get_versions]
@item_id numeric,
@version_id numeric = 0
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id

if @content_id is not null
begin

	declare @attributes table
	(
		name nvarchar(255)
	)

	declare @main_ids table
	(
		id numeric
	)

	insert into @main_ids
	select content_id from CONTENT_ATTRIBUTE where AGGREGATED = 1 and RELATED_ATTRIBUTE_ID in (select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where CONTENT_ID  = @content_id)

	insert into @main_ids
	values(@content_id)


	insert into @attributes(name)
	select CASE c.CONTENT_ID WHEN @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + '.' + CA.ATTRIBUTE_NAME END
	from content_attribute ca
	inner join content c on ca.CONTENT_ID = c.CONTENT_ID
	where ca.CONTENT_ID in (select id from @main_ids)
	order by C.CONTENT_ID, CA.attribute_order

	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
	SELECT @prefixed_fields = COALESCE(@prefixed_fields + ', ', '') + 'pt.[' + name + ']' FROM @attributes

	if @version_id = 0
		set @version_sql = ''
	else
		set @version_sql = ' and vcd.CONTENT_ITEM_VERSION_ID= @version_id'


	declare @ids nvarchar(max)
	select @ids = coalesce(@ids + ', ', '') + cast(id as nvarchar(10)) from @main_ids

	set @sql = N'select pt.content_item_id, pt.version_id, pt.created, pt.created_by, pt.modified, pt.last_modified_by, ' + @prefixed_fields  + N' from
	(
	select civ.CONTENT_ITEM_ID, civ.CREATED, civ.CREATED_BY, civ.MODIFIED, civ.LAST_MODIFIED_BY, vcd.CONTENT_ITEM_VERSION_ID as version_id,
	case ca.CONTENT_ID when @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + ''.'' + ca.ATTRIBUTE_NAME END AS ATTRIBUTE_NAME,
	cast(coalesce(vcd.blob_data, vcd.data) as nvarchar(max)) as pivot_data
	from CONTENT_ATTRIBUTE ca
	INNER JOIN CONTENT c on ca.CONTENT_ID = c.CONTENT_ID
	left outer join VERSION_CONTENT_DATA vcd on ca.ATTRIBUTE_ID = vcd.ATTRIBUTE_ID
	inner join CONTENT_ITEM_VERSION civ on vcd.CONTENT_ITEM_VERSION_ID = civ.CONTENT_ITEM_VERSION_ID
	where ca.CONTENT_ID in (' + @ids + ') and civ.CONTENT_ITEM_ID = @item_id ' + @version_sql + ') as src
	PIVOT
	(
	MAX(src.pivot_data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt order by pt.version_id desc

	'

	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric, @version_id numeric', @content_id = @content_id, @item_id = @item_id, @version_id = @version_id
end
end
GO

ALTER PROCEDURE [dbo].[qp_merge_article]
@item_id numeric,
@last_modified_by numeric = 1
AS
BEGIN
	DECLARE @ids [Ids]
	insert into @ids
	select @item_id
	EXEC qp_merge_articles @ids, @last_modified_by, 0
END
GO

exec qp_drop_existing 'qp_merge_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1,
@force_merge bit = 0
AS
BEGIN
  declare @ids2 [Ids], @ids2str nvarchar(max)

  insert into @ids2 select id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID and (SCHEDULE_NEW_VERSION_PUBLICATION = 1 or @force_merge = 1)

  if exists(select * From @ids2)
  begin
    exec qp_merge_links_multiple @ids2, @force_merge
    UPDATE content_item with(rowlock) set not_for_replication = 1 WHERE content_item_id in (select id from @ids2)
    UPDATE content_item with(rowlock) set SCHEDULE_NEW_VERSION_PUBLICATION = 0, MODIFIED = GETDATE(), LAST_MODIFIED_BY = @last_modified_by, CANCEL_SPLIT = 0 where CONTENT_ITEM_ID in (select id from @ids2)
    SELECT @ids2str = COALESCE(@ids2str + ',', '') + cast(id as nvarchar(20)) from @ids2
    exec qp_replicate_items @ids2str
    UPDATE content_item_schedule with(rowlock) set delete_job = 0 WHERE content_item_id in (select id from @ids2)
    DELETE FROM content_item_schedule with(rowlock) WHERE content_item_id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE child_id in (select id from @ids2)

    delete from content_item with(rowlock) where content_item_id in (select id from dbo.qp_aggregates_to_remove(@ids2))

  end
END
GO

EXEC qp_drop_existing 'qp_merge_delays', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_delays]
@ids [Ids] READONLY,
@lastModifiedBy int
AS
BEGIN
    if exists(select * from CHILD_DELAYS cd inner join @ids i on cd.id = i.ID)
    BEGIN
        declare @ids_to_merge [Ids]

        insert into @ids_to_merge
        select child_id from CHILD_DELAYS cd1 where id in (select id from @ids)
        and not exists(select * from CHILD_DELAYS cd2 where cd2.child_id = cd1.child_id and cd2.id <> cd1.id)

        exec qp_merge_articles @ids_to_merge, @lastModifiedBy

        delete from child_delays where id in (select id from @ids)
    END
END
GO

exec qp_drop_existing 'qp_merge_links_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_links_multiple]
@ids [Ids] READONLY,
@force_merge bit = 0
AS
BEGIN

  declare @idsWithLinks Table (id numeric, link_id numeric)

  insert into @idsWithLinks
  select i.id, ca.link_id from @ids i
  inner join content_item ci with(nolock) on ci.CONTENT_ITEM_ID = i.ID and (SPLITTED = 1 or @force_merge = 1)
  inner join content c on ci.CONTENT_ID = c.CONTENT_ID
  inner join CONTENT_ATTRIBUTE ca on ca.CONTENT_ID = c.CONTENT_ID and link_id is not null

  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @oldIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  declare @linkIds table (link_id numeric, primary key (link_id))

  insert into @newIds (id, link_id, linked_item_id)
  select ila.item_id, ila.link_id, ila.linked_item_id from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id

  insert into @oldIds (id, link_id, linked_item_id)
  select il.item_id, il.link_id, il.linked_item_id from item_link il inner join @idsWithLinks i on il.item_id = i.id and il.link_id = i.link_id

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @oldIds t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @oldIds from @oldIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_to_item from item_to_item il inner join @oldIds i on il.l_item_id = i.id and il.link_id = i.link_id and il.r_item_id = i.linked_item_id

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data) as
  (
    select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data
    from @newIds n
      inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
      inner join content c on ci.content_id = c.content_id
      inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.linked_has_data = 0 and n.linked_attribute_id is not null

  delete item_link_async from item_link_async ila inner join @idsWithLinks i on ila.item_id = i.id and ila.link_id = i.link_id


END
GO

exec qp_drop_existing 'qp_remove_old_aggregates', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_remove_old_aggregates](@id numeric)
as
begin
    DECLARE @ids [Ids]
    INSERT INTO @ids VALUES(@id)
    DELETE FROM content_item WITH(ROWLOCK) WHERE content_item_id IN (SELECT id FROM dbo.qp_aggregates_to_remove(@ids))
end
GO

ALTER PROCEDURE [dbo].[qp_replicate]
@content_item_id numeric
AS
BEGIN
  declare @list nvarchar(30)
  set @list = convert(nvarchar, @content_item_id)
  exec qp_replicate_items @list, '', 0
END
GO

ALTER PROCEDURE [dbo].[qp_replicate_items]
@ids nvarchar(max),
@attr_ids nvarchar(max) = '',
@modification_update_interval numeric = -1
-- <0   throttling with default value (setting or constant)
-- 0    no throttling
-- >0   throttling with specified value

AS
BEGIN
    set nocount on
    declare @setting_name nvarchar(255) = 'CONTENT_MODIFICATION_UPDATE_INTERVAL'
    declare @setting_value nvarchar(255)
    declare @default_modification_update_interval numeric = 30
    select @setting_value = VALUE from APP_SETTINGS where [KEY] = @setting_name
    set @default_modification_update_interval = case when @setting_value is not null and ISNUMERIC(@setting_value) = 1 then convert(numeric, @setting_value) else @default_modification_update_interval end
    set @modification_update_interval = case when @modification_update_interval < 0 then @default_modification_update_interval else @modification_update_interval end

    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric
    declare @live_modified datetime, @stage_modified datetime
    declare @live_expired bit, @stage_expired bit

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        cancel_split bit,
        delayed bit,
        status_type_id numeric,
        content_id numeric
    )

    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')

    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.cancel_split = ci.cancel_split, base.delayed = ci.schedule_new_version_publication, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID

    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )

    insert into @contents
    select distinct a.content_id, st.STATUS_TYPE_ID from @articles a
    inner join content c on a.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID


    declare @articleIds [Ids], @syncIds [Ids], @syncIds2 [Ids], @asyncIds [Ids], @asyncIds2 [Ids]
    declare @noneId numeric

    insert into @articleIds select id from @articles

    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents

        if @modification_update_interval <= 0
        begin
            set @live_expired = 1
            set @stage_expired = 1
        end
        else
        begin
            select @live_modified = live_modified, @stage_modified = stage_modified from CONTENT_MODIFICATION with(nolock) where CONTENT_ID = @content_id
            set @live_expired = case when datediff(s, @live_modified, GETDATE()) >= @modification_update_interval then 1 else 0 end
            set @stage_expired = case when datediff(s, @stage_modified, GETDATE()) >= @modification_update_interval then 1 else 0 end
        end

        insert into @syncIds select id from @articles where content_id = @content_id and splitted = 0
        insert into @asyncIds select id from @articles where content_id = @content_id and splitted = 1
        insert into @syncIds2 select id from @articles where content_id = @content_id and splitted = 0 and delayed = 1

        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @syncIds
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @asyncIds

        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'

        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @syncIds, @ids2 = @syncIds2, @noneId = @noneId

            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql

            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0, @attr_ids
        end

        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @async_table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @asyncIds, @ids2 = @asyncIds2, @noneId = @noneId

            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql

            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1, @attr_ids
        end

        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and (cancel_split = 1 or (splitted = 0 and status_type_id = @published_id)))
        begin
            if (@live_expired = 1 or @stage_expired = 1)
                update content_modification with(rowlock) set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        end
        else
        begin
            if (@stage_expired = 1)
                update content_modification with(rowlock) set stage_modified = GETDATE() where content_id = @content_id
        end


        delete from @contents where id = @content_id

        delete from @syncIds
        delete from @syncIds2
        delete from @asyncIds
    end

    update content_data set O2M_DATA = data from content_data cd
    inner join content_attribute ca on ca.attribute_id = cd.attribute_id
    where cd.CONTENT_ITEM_ID in (select id from @articleIds)
    and ca.attribute_type_id = 11 and ca.link_id is null
    and (isnumeric(data) = 1 or data is null)

    update content_item set not_for_replication = 0, CANCEL_SPLIT = 0 where content_item_id in (select id from @articleIds)
END
GO

exec qp_drop_existing 'qp_split_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_split_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1
AS
BEGIN
  declare @contentIds [Ids], @content_id numeric
  declare @ids2 table (content_id numeric, id numeric)
  insert into @ids2 select content_id, id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID

  insert into @contentIds
  select distinct content_id from @ids2

  while exists(select * From @contentIds)
  begin
    select @content_id = id from @contentIds

    declare @ids3 [Ids]
    declare @sql nvarchar(max)
    declare @cstr nvarchar(20)

    insert into @ids3
    select id from @ids2 where content_id = @content_id

    set @cstr = cast(@content_id as nvarchar(max))

    set @sql = 'insert into content_' + @cstr + '_async select * from content_' + @cstr + ' c where content_item_id in (select id from @ids) and not exists(select * from content_' + @cstr + '_async a where a.content_item_id = c.content_item_id)'
    exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids3

    delete from @contentIds where id = @content_id
  end

  insert into item_link_async select * from item_to_item ii where l_item_id in (select id from @ids)
  and link_id in (select link_id from content_attribute where content_id in (select content_id from @ids2))
  and not exists (select * from item_link_async ila where ila.item_id = ii.l_item_id)

END
GO


ALTER procedure [dbo].[qp_update_items_with_content_data_pivot]
@content_id numeric,
@ids nvarchar(max),
@is_async bit,
@attr_ids nvarchar(max) = ''
as
begin

	declare @sql nvarchar(max), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)

	set @table_name = 'content_' + CAST(@content_id as nvarchar)
	if (@is_async = 1)
	set @table_name = @table_name + '_async'

	declare @attributes table
	(
		name nvarchar(255) primary key
	)

	if @attr_ids = ''
		insert into @attributes
		select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
	else
		insert into @attributes
		select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id and attribute_id in (select nstr from dbo.SplitNew(@attr_ids, ','))

	if exists (select * from @attributes)
	begin
		SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

		SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes

		set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
		(
		select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME,
	    dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ca.attribute_type_id, ca.attribute_size, ca.default_value) as pivot_data
		from CONTENT_ATTRIBUTE ca
		left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
		inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
		where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID in (' + @ids + ')
		) as src
		PIVOT
		(
		MAX(src.pivot_data)
		FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
		) AS pt
		on pt.content_item_id = base.content_item_id
		'
		print @sql
		exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
	end
end
GO

exec qp_drop_existing 'qp_update_links', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_links]
    @ids Ids READONLY,
    @id numeric,
    @link_id numeric

AS
BEGIN

    declare @ids2 Ids
    declare @ids3 Ids

    insert into @ids2
    select * from @ids

    while exists(select * from @ids2)
    begin
        delete from @ids3
        delete top(100) from @ids2 output DELETED.* into @ids3

        insert into item_to_item (l_item_id, r_item_id, link_id)
        select id, @id, @link_id
        from @ids3 i where not exists(
            select * from item_link il where il.item_id = i.id and link_id = @link_id and il.linked_item_id = @id
        )
    end

END
GO

exec qp_drop_existing 'qp_update_m2m', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2m]
@id numeric,
@linkId numeric,
@value nvarchar(max),
@splitted bit = 0,
@update_archive bit = 1
AS
BEGIN
    declare @newIds table (id numeric primary key, attribute_id numeric null, has_data bit, splitted bit, has_async bit null)
    declare @ids table (id numeric primary key)
    declare @crossIds table (id numeric primary key)

    insert into @newIds (id) select * from dbo.split(@value, ',')

    IF @splitted = 1
        insert into @ids select linked_item_id from item_link_async where link_id = @linkId and item_id = @id
    ELSE
        insert into @ids select linked_item_id from item_link where link_id = @linkId and item_id = @id

    insert into @crossIds select t1.id from @ids t1 inner join @newIds t2 on t1.id = t2.id
    delete from @ids where id in (select id from @crossIds)
    delete from @newIds where id in (select id from @crossIds)

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    IF @splitted = 0
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id

    IF @splitted = 1
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)
    ELSE
        DELETE FROM item_link_united_full WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)

    IF @splitted = 1
        INSERT INTO item_link_async (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds
    ELSE
        INSERT INTO item_link (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds

    if dbo.qp_is_link_symmetric(@linkId) = 1
    begin

        with newItems (id, attribute_id, has_data, splitted, has_async) as
        (
        select
            n.id, ca.attribute_id,
            case when cd.content_item_id is null then 0 else 1 end as has_data,
            ci.splitted,
            case when ila.link_id is null then 0 else 1 end as has_async
        from @newIds n
            inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            inner join content c on ci.content_id = c.content_id
            inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = @linkId
            left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            left join item_link_async ila on @linkId = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @id
        )
        update @newIds
        set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
        from @newIds n inner join newItems ext on n.id = ext.id

        if @splitted = 0
        begin
            update content_data set data = @linkId
            from content_data cd
            inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
            where n.has_data = 1

            insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
            select n.id, n.attribute_id, @linkId
            from @newIds n
            where n.has_data = 0 and n.attribute_id is not null

            insert into item_link_async(link_id, item_id, linked_item_id)
            select @linkId, n.id, @id
            from @newIds n
            where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
        end
    end
END
GO
exec qp_drop_existing 'qp_update_m2m_values', 'IsProcedure'
GO


CREATE  PROCEDURE [dbo].[qp_update_m2m_values]
  @xmlParameter xml
AS
BEGIN
  DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
  INSERT INTO @fieldValues
  SELECT
    ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber
     ,doc.col.value('./@id', 'int') id
     ,doc.col.value('./@linkId', 'int') linkId
     ,doc.col.value('./@value', 'nvarchar(max)') value
     ,c.SPLITTED as splitted
    FROM @xmlParameter.nodes('/items/item') doc(col)
    INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')


  declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
  declare @ids table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
  declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

  insert into @newIds (id, link_id, linked_item_id, splitted)
  select a.id, a.linkId, b.nstr, a.splitted from @fieldValues a cross apply dbo.SplitNew(a.value, ',') b
  where b.nstr is not null and b.nstr <> '' and b.nstr <> '0'

  insert into @ids
  select item_id, link_id, linked_item_id, f.splitted
  from item_link_async ila inner join @fieldValues f
  on ila.link_id = f.linkId and ila.item_id = f.id
  where f.splitted = 1
  union all
  select item_id, link_id, linked_item_id, f.splitted
  from item_link il inner join @fieldValues f
  on il.link_id = f.linkId and il.item_id = f.id
  where f.splitted = 0

  insert into @crossIds
  select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @ids t1 inner join @newIds t2
  on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete @ids from @ids t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
  delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

  delete item_link_async from item_link_async il inner join @fieldValues f on il.item_id = f.id and il.link_id = f.linkId
  where f.splitted = 0

  delete item_link_united_full from item_link_united_full il
  where exists(
    select * from @ids i where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
    and i.splitted = 0
  )

  delete item_link_async from item_link_async il
  inner join @ids i on il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
  where i.splitted = 1

  insert into item_link_async (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 1

  insert into item_link (link_id, item_id, linked_item_id)
  select link_id, id, linked_item_id from @newIds
  where splitted = 0

  delete from @newIds where dbo.qp_is_link_symmetric(link_id) = 0;

  with newItems (id, link_id, linked_item_id, attribute_id, has_data, splitted, has_async) as
  (
  select
    n.id, n.link_id, n.linked_item_id, ca.attribute_id,
    case when cd.content_item_id is null then 0 else 1 end as has_data,
    ci.splitted,
    case when ila.link_id is null then 0 else 1 end as has_async
  from @newIds n
    inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
    inner join content c on ci.content_id = c.content_id
    inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.id = ila.linked_item_id
  )
  update @newIds
  set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data, linked_splitted = ext.splitted, linked_has_async = ext.has_async
  from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

  update content_data set data = n.link_id
  from content_data cd
  inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  where n.splitted = 0 and n.linked_has_data = 1

  insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
  from @newIds n
  where n.splitted = 0 and n.linked_has_data = 0 and n.linked_attribute_id is not null

  insert into item_link_async(link_id, item_id, linked_item_id)
  select n.link_id, n.linked_item_id, n.id
  from @newIds n
  where n.splitted = 0 and n.linked_splitted = 1 and n.linked_has_async = 0 and n.linked_attribute_id is not null
END
GO



ALTER PROCEDURE [dbo].[qp_update_m2o]
@id numeric,
@fieldId numeric,
@value nvarchar(max),
@update_archive bit = 1
AS
BEGIN
    declare @ids table (id numeric primary key)
    declare @new_ids table (id numeric primary key);
    declare @cross_ids table (id numeric primary key);

    declare @contentId numeric, @fieldName nvarchar(255)
    select @contentId = content_id, @fieldName = attribute_name from CONTENT_ATTRIBUTE where ATTRIBUTE_ID = @fieldId

    insert into @ids
    select content_item_id from content_data where O2M_DATA = @id and ATTRIBUTE_ID = @fieldId

    insert into @new_ids select * from dbo.split(@value, ',');

    insert into @cross_ids select t1.id from @ids t1 inner join @new_ids t2 on t1.id = t2.id
    delete from @ids where id in (select id from @cross_ids);
    delete from @new_ids where id in (select id from @cross_ids);

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    insert into #resultIds(id, attribute_id, to_remove)
    select id, @fieldId as attribute_id, 1 as to_remove from @ids
    union all
    select id, @fieldId as attribute_id, 0 as to_remove from @new_ids
END
GO

exec qp_drop_existing 'qp_update_m2o_final', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2o_final]
@id numeric
AS
BEGIN
    if exists(select * from #resultIds) or exists(select * from CHILD_DELAYS where id = @id)
    begin
        declare @statusId numeric
        declare @splitted bit
        declare @lastModifiedBy numeric
        declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, remove_delays bit not null default 0, primary key(id, attribute_id))
        declare @ids_to_split [Ids]
        declare @ids_to_process [Ids]
        declare @ids_to_merge [Ids]
        declare @idsstr nvarchar(max)
        declare @maxStatus numeric

        select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
        select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

        if @statusId = @maxStatus and @splitted = 0 begin
            insert @ids_to_merge
            select @id

            exec qp_merge_delays @ids_to_merge, @lastModifiedBy
        end

        if not exists(select * from #resultIds)
            return

        insert into @ids(id, attribute_id, to_remove, remove_delays)
        select r.*, case when cd.id is null then 0 else 1 end as remove_delays
		from #resultIds r left join CHILD_DELAYS cd on cd.id = @id and r.id = cd.child_id

        update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1
        where content_item_id in (select id from @ids)

        update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate()
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 0

        insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
        select r.id, r.attribute_id, @id, NULL, getdate()
        from @ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 0 and cd.CONTENT_DATA_ID is null

        update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate()
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 1

		delete from CHILD_DELAYS where id = @id and child_id in (select id from @ids where remove_delays = 1)

        declare @resultId numeric

        if (@statusId <> @maxStatus and @maxStatus is not null or @splitted = 1) begin
            insert into child_delays (id, child_id) select @id, r.id from @ids r
            inner join content_item ci on r.id = ci.content_item_id
            left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
            left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
            left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
            where ex.child_id is null and ci.status_type_id = wms.max_status_type_id
                and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
				and r.remove_delays = 0

            insert into @ids_to_split
            select content_item_id from content_item with(nolock) where content_item_id in (select child_id from child_delays where id = @id) and splitted = 0

            exec qp_split_articles @ids_to_split

            update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
        end

        select @idsstr = COALESCE(@idsstr + ', ', '') + CAST(id as nvarchar) from @ids

        exec qp_replicate_items @idsstr
    end
END
GO

exec qp_drop_existing 'qp_update_values', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_values]
    @values [Values] READONLY
AS
BEGIN

    declare @values2 [Values]
    declare @values3 [Values]

    insert into @values2
    select * from @values

    while exists(select * from @values2)
    begin
        delete from @values3

        delete top(100) from @values2 output DELETED.* into @values3

        update content_data set
        data = case when ca.attribute_type_id in (9,10) then null else v.Value end,
        blob_data = case when ca.attribute_type_id in (9,10) then v.Value else null end
        from content_data cd
        inner join @values3 v on v.ArticleId = cd.content_item_id and v.FieldId = cd.attribute_id
        inner join content_attribute ca on cd.attribute_id = ca.attribute_id

    end

END
GO

ALTER procedure [dbo].[qp_update_with_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
declare @content_id numeric, @splitted bit
select @content_id = content_id, @splitted = SPLITTED from content_item ci where ci.CONTENT_ITEM_ID = @item_id

if @content_id is not null
begin

	set @table_name = 'content_' + CAST(@content_id as nvarchar)
	if (@splitted = 1)
		set @table_name = @table_name + '_async'

	declare @attributes table
	(
		name nvarchar(255)
	)
	insert into @attributes
	select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id

	SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

	SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes

	set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
	(
	select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME,
	dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ca.attribute_type_id, ca.attribute_size, ca.default_value) as pivot_data
	from CONTENT_ATTRIBUTE ca
	left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
	inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
	where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
	) as src
	PIVOT
	(
	MAX(src.pivot_data)
	FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
	) AS pt
	on pt.content_item_id = base.content_item_id
	'
	print @sql
	exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER  PROCEDURE [dbo].[restore_content_item_version]
  @uid NUMERIC,
  @version_id NUMERIC
AS
  DECLARE @id NUMERIC, @tm DATETIME
  DECLARE @content_id numeric, @splitted bit
  SET @tm = GETDATE()
  SELECT @id = content_item_id FROM content_item_version WHERE content_item_version_id = @version_id
  IF @id IS NOT NULL BEGIN
    select @content_id = content_id, @splitted = splitted from content_item where content_item_id = @id

    -- Restore common data
    DELETE FROM content_data WHERE content_item_id = @id
    INSERT INTO content_data (attribute_id, content_item_id, data, blob_data)
    SELECT attribute_id, @id, data, blob_data
    FROM version_content_data
    WHERE content_item_version_id = @version_id

    -- Restore many-to-many data
    IF @splitted = 1
    begin
		DELETE FROM item_link_async where item_id = @id and link_id in (select link_id from content_attribute where content_id = @content_id)

		INSERT INTO item_link_async (link_id, item_id, linked_item_id)
		SELECT link_id, @id, linked_item_id FROM item_to_item_version AS iv
		INNER JOIN content_attribute ca on iv.attribute_id = ca.attribute_id
		WHERE iv.content_item_version_id = @version_id and link_id is not null
    end else
    begin
		DELETE FROM item_link_united_full where item_id = @id and link_id in (select link_id from content_attribute where content_id = @content_id)

		INSERT INTO item_to_item (link_id, l_item_id, r_item_id)
		SELECT link_id, @id, linked_item_id FROM item_to_item_version AS iv
		INNER JOIN content_attribute ca on iv.attribute_id = ca.attribute_id
		WHERE iv.content_item_version_id = @version_id and link_id is not null
    end

    -- Restore many-to-one data
    create table #resultIds (id numeric, attribute_id numeric not null, to_remove bit not null default 0)

    declare @fieldIds table (id numeric, back_id numeric)

    insert into @fieldIds
    select ATTRIBUTE_ID, BACK_RELATED_ATTRIBUTE_ID From CONTENT_ATTRIBUTE where BACK_RELATED_ATTRIBUTE_ID is not null and CONTENT_ID = @content_id

    while exists(select * from @fieldIds)
    begin
    	declare @currentFieldId numeric, @currentBackFieldId numeric
    	select @currentFieldId = id, @currentBackFieldId = back_id from @fieldIds

    	declare @ids table (id numeric)
    	insert into @ids
    	select linked_item_id from item_to_item_version where attribute_id = @currentFieldId and content_item_version_id = @version_id

    	declare @value nvarchar(max)
    	set @value = ''
    	while exists(select * from @ids)
    	begin
    		declare @currentId numeric
    		select @currentId = id from @ids
    		if @value <> ''
    			set @value = @value + ','
    		set @value = @value + CAST(@currentId as nvarchar)

    		delete from @ids where id = @currentId

    	end

    	exec qp_update_m2o @id, @currentBackFieldId, @value


		delete from @fieldIds where id = @currentFieldId
    end

    exec qp_update_m2o_final @id

    drop table #resultIds

    update content_item set MODIFIED = GETDATE(), LAST_MODIFIED_BY = @uid where CONTENT_ITEM_ID = @id

    -- Write status history log
    INSERT INTO content_item_status_history
      (content_item_id, user_id, description, created,
      system_status_type_id, content_item_version_id)
    VALUES
      (@id, @uid, 'Record has been restored from version backup', @tm,
      4, @version_id)
  END
GO

exec qp_drop_existing 'tbd_delete_content', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tbd_delete_content] ON [dbo].[CONTENT] INSTEAD OF DELETE
AS
BEGIN
	create table #disable_td_delete_item(id numeric)

	UPDATE content_attribute SET related_attribute_id = NULL
	where related_attribute_id in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)

	UPDATE content_attribute SET CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = 0
	where CLASSIFIER_ATTRIBUTE_ID in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	UPDATE content_attribute SET TREE_ORDER_FIELD = NULL
	where TREE_ORDER_FIELD in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	update content_attribute set link_id = null where link_id in (select link_id from content_link cl
	inner join deleted d on cl.content_id = d.content_id)

	delete USER_DEFAULT_FILTER from USER_DEFAULT_FILTER f
	inner join deleted d on d.content_id = f.CONTENT_ID

	delete content_to_content from content_to_content cc
	inner join deleted d on d.content_id = cc.r_content_id or d.content_id = cc.l_content_id

	delete container from container c
	inner join deleted d on d.content_id = c.content_id

	delete content_form from content_form cf
	inner join deleted d on d.content_id = cf.content_id

	delete content_item from content_item ci
	inner join deleted d on d.content_id = ci.content_id

	delete content_tab_bind from content_tab_bind ctb
	inner join deleted d on d.content_id = ctb.content_id

	delete [ACTION_CONTENT_BIND] from [ACTION_CONTENT_BIND] acb
	inner join deleted d on d.content_id = acb.content_id

	delete ca from CONTENT_ATTRIBUTE ca
	inner join CONTENT_ATTRIBUTE cad on ca.BACK_RELATED_ATTRIBUTE_ID = cad.ATTRIBUTE_ID
	inner join deleted c on cad.CONTENT_ID = c.CONTENT_ID

	delete content from content c inner join deleted d on c.content_id = d.content_id

	drop table #disable_td_delete_item
END

GO

ALTER TRIGGER [dbo].[tbd_delete_content_item] ON [dbo].[CONTENT_ITEM] INSTEAD OF DELETE
AS
BEGIN

delete waiting_for_approval from waiting_for_approval wa inner join deleted d on wa.content_item_id = d.content_item_id

delete child_delays from child_delays cd inner join deleted d on cd.child_id = d.content_item_id

IF dbo.qp_get_version_control() IS NOT NULL BEGIN
  delete content_item_version from content_item_version civ inner join deleted d on civ.content_item_id = d.content_item_id

  delete item_to_item_version from item_to_item_version iiv
  inner join content_item_version civ on civ.content_item_version_id = iiv.content_item_version_id
  inner join deleted d on d.content_item_id = civ.content_item_id

  delete item_to_item_version from item_to_item_version iiv
  inner join deleted d on d.content_item_id = iiv.linked_item_id
END

delete item_link_united_full from item_link_united_full ii where ii.item_id in (select content_item_id from deleted)

-- delete asymettric links
delete item_link_united_full from item_link_united_full ii where ii.linked_item_id in (select content_item_id from deleted)

DELETE from FIELD_ARTICLE_BIND where [ARTICLE_ID] in (select content_item_id from deleted)

delete content_data from content_data cd inner join deleted d on cd.content_item_id = d.content_item_id

delete content_item from content_item ci inner join deleted d on ci.content_item_id = d.content_item_id

END
GO

ALTER TRIGGER [dbo].[tbd_user] ON [dbo].[USERS]
INSTEAD OF DELETE
AS
BEGIN

  DELETE USER_GROUP_BIND FROM USER_GROUP_BIND c inner join deleted d on c.user_id = d.user_id
  DELETE USER_DEFAULT_FILTER FROM USER_DEFAULT_FILTER f inner join deleted d on f.user_id = d.user_id

  UPDATE CONTAINER SET locked = NULL, locked_by = NULL FROM CONTAINER c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_FORM SET locked = NULL, locked_by = NULL FROM CONTENT_FORM c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_ITEM SET locked = NULL, locked_by = NULL FROM CONTENT_ITEM c inner join deleted d on c.locked_by = d.user_id
  UPDATE [OBJECT] SET locked = NULL, locked_by = NULL FROM [OBJECT] c inner join deleted d on c.locked_by = d.user_id
  UPDATE OBJECT_FORMAT SET locked = NULL, locked_by = NULL FROM OBJECT_FORMAT c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE SET locked = NULL, locked_by = NULL FROM PAGE c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE_TEMPLATE SET locked = NULL, locked_by = NULL FROM PAGE_TEMPLATE c inner join deleted d on c.locked_by = d.user_id
  UPDATE [SITE] SET locked = NULL, locked_by = NULL FROM [SITE] c inner join deleted d on c.locked_by = d.user_id

  UPDATE [SITE] SET last_modified_by = 1 FROM [SITE] c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT SET last_modified_by = 1 FROM CONTENT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM SET last_modified_by = 1 FROM CONTENT_ITEM c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_SCHEDULE SET last_modified_by = 1 FROM CONTENT_ITEM_SCHEDULE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET created_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.created_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET last_modified_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ATTRIBUTE SET last_modified_by = 1 FROM CONTENT_ATTRIBUTE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE PAGE_TEMPLATE SET last_modified_by = 1 FROM PAGE_TEMPLATE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_modified_by = 1 FROM PAGE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_assembled_by = 1 FROM PAGE c inner join deleted d on c.last_assembled_by  = d.user_id
  UPDATE OBJECT SET last_modified_by = 1 FROM OBJECT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT SET last_modified_by = 1 FROM OBJECT_FORMAT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT_VERSION SET last_modified_by = 1 FROM OBJECT_FORMAT_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE FOLDER SET last_modified_by = 1 FROM FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE FOLDER_ACCESS SET last_modified_by = 1 FROM FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER SET last_modified_by = 1 FROM CONTENT_FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER_ACCESS SET last_modified_by = 1 FROM CONTENT_FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CODE_SNIPPET SET last_modified_by = 1 FROM CODE_SNIPPET c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE STYLE SET last_modified_by = 1 FROM STYLE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE STATUS_TYPE SET last_modified_by = 1 FROM STATUS_TYPE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW SET last_modified_by = 1 FROM WORKFLOW c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE SITE_ACCESS SET last_modified_by = 1 FROM SITE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ACCESS SET last_modified_by = 1 FROM CONTENT_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_ACCESS SET last_modified_by = 1 FROM CONTENT_ITEM_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW_ACCESS SET last_modified_by = 1 FROM WORKFLOW_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE USER_GROUP SET last_modified_by = 1 FROM USER_GROUP c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE USERS SET last_modified_by = 1 FROM USERS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE NOTIFICATIONS SET last_modified_by = 1 FROM NOTIFICATIONS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ITEM_STATUS_HISTORY SET user_id = 1 WHERE user_id in (select user_id from deleted)

  UPDATE CUSTOM_ACTION SET LAST_MODIFIED_BY = 1 FROM CUSTOM_ACTION c INNER JOIN deleted d on c.LAST_MODIFIED_BY = d.[USER_ID]

  UPDATE NOTIFICATIONS SET FROM_BACKENDUSER_ID = 1 FROM NOTIFICATIONS c inner join deleted d on c.FROM_BACKENDUSER_ID = d.user_id

  UPDATE ENTITY_TYPE_ACCESS SET last_modified_by = 1 FROM ENTITY_TYPE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE ACTION_ACCESS SET last_modified_by = 1 FROM ACTION_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE DB SET last_modified_by = 1 FROM DB c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE DB SET single_user_id = NULL FROM DB c inner join deleted d on c.single_user_id = d.user_id
  UPDATE VE_PLUGIN SET last_modified_by = 1 FROM VE_PLUGIN c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_STYLE SET last_modified_by = 1 FROM VE_STYLE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_COMMAND SET last_modified_by = 1 FROM VE_COMMAND c inner join deleted d on c.last_modified_by = d.user_id

  delete users from users c inner join deleted d on c.user_id = d.user_id
END
GO

ALTER TRIGGER [dbo].[td_content_and_article_workflow_bind]
ON [dbo].[workflow]
FOR DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_content_and_article_workflow_bind') is null
    begin

        DELETE FROM content_workflow_bind WHERE workflow_id in (select d.workflow_id from deleted d)
        DELETE FROM article_workflow_bind WHERE workflow_id in (select d.workflow_id from deleted d)
        DELETE waiting_for_approval from waiting_for_approval wa inner join content_item_workflow ciw on wa.content_item_id = ciw.content_item_id
            WHERE ciw.workflow_id in (select d.workflow_id from deleted d)
    end
END
GO
ALTER TRIGGER [dbo].[td_content_to_content] ON [dbo].[content_to_content] AFTER DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_content_to_content') is null
    begin 
        declare @link_id numeric, @i numeric, @count numeric
      
        declare @cc table (
          id numeric identity(1,1) primary key,
          link_id numeric
        )
      
        insert into @cc (link_id) select d.link_id from deleted d
      
        set @i = 1
        select @count = count(id) from @cc
      
        while @i < @count + 1
        begin
          select @link_id = link_id from @cc where id = @i
          exec qp_drop_link_view @link_id
          set @i = @i + 1
        end
    end
END
GO
ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN

    if object_id('tempdb..#disable_td_delete_item') is null
    begin

        declare @content_id numeric, @virtual_type numeric, @published_id numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )

        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c
        on d.content_id = c.content_id

        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30),
            status_type_id numeric,
            splitted bit
        )


        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin

            select @content_id = id, @virtual_type = virtual_type from @c

            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id), status_type_id, splitted from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID
            where ca2.CONTENT_ID = @content_id

            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids

            select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
            if exists (select * from @ids where status_type_id = @published_id and splitted = 0)
                update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
            else
                update content_modification set stage_modified = GETDATE() where content_id = @content_id

            /* Drop relations to current item */
            if exists(select id from @attr_ids) and object_id('tempdb..#disable_td_delete_item_o2m_nullify') is null
            begin
                UPDATE content_attribute SET default_value = null
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND default_value IN (select char_id from @ids)

                UPDATE content_data SET data = NULL, blob_data = NULL
                    WHERE O2M_DATA in (select id from @ids)
                    AND attribute_id IN (select id from @attr_ids)

                DELETE from VERSION_CONTENT_DATA
                    WHERE O2M_DATA in (select id from @ids)
                    AND attribute_id IN (select id from @attr_ids)

            end

            if @virtual_type = 0
            begin
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql

                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id

            delete from @ids

            delete from @attr_ids
        end
    end
END
GO


ALTER TRIGGER [dbo].[td_drop_table] ON [dbo].[CONTENT]
FOR  DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_drop_table') is null
    begin

		declare @content_id numeric

		declare del_content CURSOR FOR
		select content_id from deleted

		open del_content
		FETCH NEXT FROM del_content into @content_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC qp_content_table_drop @content_id
			FETCH NEXT FROM del_content into @content_id
		END

		close del_content
		deallocate del_content
    END
END
GO




exec qp_drop_existing 'td_item_link_async', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[td_item_link_async] ON [dbo].[item_link_async] AFTER DELETE
AS
BEGIN
  declare @links table
  (
    id numeric primary key,
    is_symmetric bit,
    l_content_id numeric,
    r_content_id numeric
  )

  insert into @links
  select distinct d.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from deleted d inner join content_to_content c2c on d.link_id = c2c.link_id

    declare @count numeric
    select @count = count(id) from @links
    print (@count)

  declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

  while exists(select id from @links)
  begin

    declare @link_items [Links]

    select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

    insert into @link_items
    select distinct item_id, linked_item_id from deleted where link_id = @link_id

    declare @self_related bit
    select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end

    exec qp_delete_link_table_item @link_id, @l_content_id, @link_items, 1, 0, 0
    exec qp_delete_link_table_item @link_id, @r_content_id, @link_items, 1, 1, @self_related

    delete from @link_items

    delete from @links where id = @link_id

  end
END
GO


ALTER TRIGGER [dbo].[td_item_to_item] ON [dbo].[item_to_item] AFTER DELETE
AS
BEGIN
  if object_id('tempdb..#disable_td_item_to_item') is null
  begin
    delete item_to_item from item_to_item ii
      inner join deleted d on ii.link_id = d.link_id and ii.l_item_id = d.r_item_id and ii.r_item_id = d.l_item_id
      inner join content_to_content c2c on d.link_id = c2c.link_id
      where c2c.[symmetric] = 1
  end

  declare @links table
  (
    id numeric primary key,
    is_symmetric bit,
    l_content_id numeric,
    r_content_id numeric
  )

  insert into @links
  select distinct d.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from deleted d inner join content_to_content c2c on d.link_id = c2c.link_id

    declare @count numeric
    select @count = count(*) from @links
    print (@count)

  declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

  while exists(select id from @links)
  begin

    declare @link_items [Links]

    select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

    insert into @link_items
    select distinct l_item_id, r_item_id from deleted where link_id = @link_id

    if @is_symmetric = 1
    begin
      insert into @link_items
      select distinct r_item_id, l_item_id from deleted d where link_id = @link_id
      and not exists (select * from @link_items where id = d.r_item_id and linked_id = d.l_item_id)
    end

    declare @self_related bit
    select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end

    exec qp_delete_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
    exec qp_delete_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

    delete from @link_items

    delete from @links where id = @link_id

  end
END
GO

ALTER TRIGGER [dbo].[tiud_remove_empty_content_groups] ON [dbo].[CONTENT] FOR INSERT, UPDATE, DELETE
AS BEGIN
	if object_id('tempdb..#disable_tiud_remove_empty_content_groups') is null
	begin
      DELETE FROM content_group
      WHERE NAME <> 'Default Group'
      AND NOT EXISTS(SELECT * FROM content WHERE content.content_group_id = content_group.content_group_id)
    END
END
GO


ALTER TRIGGER [dbo].[tiu_content_fill] ON [dbo].[CONTENT_DATA] FOR INSERT, UPDATE AS
BEGIN
  set nocount on
  IF EXISTS(select content_data_id from inserted where not_for_replication = 0)
    BEGIN
        IF NOT EXISTS(select content_data_id from deleted) -- insert or update without special columns
               OR (NOT(UPDATE(SPLITTED)) AND NOT (UPDATE(not_for_replication)) AND NOT (UPDATE(O2M_DATA)))
        BEGIN

            update content_item set modified = getdate() where content_item_id in (select content_item_id from deleted where not_for_replication = 0)

            DECLARE @attribute_id NUMERIC, @attribute_type_id NUMERIC, @attribute_size NUMERIC
            DECLARE @default_value NVARCHAR(255), @attribute_name NVARCHAR(255), @content_id NUMERIC, @link_id NUMERIC
            DECLARE @table_name nvarchar(50), @sql NVARCHAR(max), @ids_list nvarchar(max), @async_ids_list nvarchar(max)

            declare @ca table
            (
                id numeric primary key
            )

            insert into @ca
            select distinct attribute_id from inserted


            declare @ids table
            (
                id numeric primary key,
                splitted bit
            )

            while exists(select id from @ca)
            begin

                select @attribute_id = id from @ca

                select @attribute_name = attribute_name, @attribute_type_id = attribute_type_id,
                       @attribute_size = attribute_size, @default_value = default_value,
                       @content_id = content_id, @link_id = link_id
                from content_attribute
                where ATTRIBUTE_ID = @attribute_id

                insert into @ids
                select i.content_item_id, ci.SPLITTED from inserted i
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = @attribute_id and ci.not_for_replication = 0 and c.virtual_type = 0

                if @attribute_type_id = 11 and @link_id is null
                begin
                    update content_data set O2M_DATA = DATA
                    where CONTENT_ITEM_ID in (select id from @ids) and ATTRIBUTE_ID = @attribute_id
                    and (isnumeric(data) = 1 or data is null)
                end

                set @ids_list = null
                select @ids_list = coalesce(@ids_list + ', ', '') + CONVERT(nvarchar, id) from @ids where splitted = 0
                set @async_ids_list = null
                select @async_ids_list = coalesce(@async_ids_list + ', ', '') + CONVERT(nvarchar, id) from @ids where splitted = 1

                set @table_name = 'content_' + CONVERT(nvarchar, @content_id)

                if @ids_list <> ''
                begin
                    exec qp_get_update_column_sql @table_name, @ids_list, @attribute_id, @attribute_type_id, @attribute_size, @default_value, @attribute_name, @sql = @sql out
                    print @sql
                    exec sp_executesql @sql
                end

                if @async_ids_list <> ''
                begin
                    set @table_name = @table_name + '_async'
                    exec qp_get_update_column_sql @table_name, @async_ids_list, @attribute_id, @attribute_type_id, @attribute_size, @default_value, @attribute_name, @sql = @sql out
                    print @sql
                    exec sp_executesql @sql
                end

                delete from @ca where id = @attribute_id

                delete from @ids
            end --while
        end --if
    end --if
END
go


ALTER TRIGGER [dbo].[ti_access_content] ON [dbo].[CONTENT] FOR INSERT
AS
  if object_id('tempdb..#disable_ti_access_content') is null
  begin
	  INSERT INTO content_access
		(content_id, user_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		content_id, last_modified_by, 1, 1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM inserted

	  INSERT INTO content_access
		(content_id, user_id, group_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		i.content_id,ca.user_id, ca.group_id, ca.permission_level_id , 1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM site_access ca, inserted i
	  WHERE ca.site_id = i.site_id
		AND (ca.user_id <> i.last_modified_by OR ca.user_id IS NULL)
		AND ca.propagate_to_contents = 1

	  INSERT INTO content_access
		(content_id, group_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		c.content_id, 1, 1,	1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM inserted AS c
	  WHERE c.content_id NOT IN (
		SELECT ca.content_id FROM content_access AS ca
		WHERE ca.content_id = c.content_id AND ca.group_id = 1
	  )
  end
GO
ALTER TRIGGER ti_access_site ON SITE 
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_access_site') is null
    begin
        insert into site_access (site_id, user_id, permission_level_id, last_modified_by)
        (select site_id, last_modified_by, 1, 1 from inserted i where i.last_modified_by > 1 )
        insert into site_access (site_id, group_id, permission_level_id, last_modified_by)
        (select site_id, 1, 1, 1 from inserted)
    end
END
go


ALTER TRIGGER [dbo].[ti_access_workflow] ON [dbo].[workflow] FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_access_workflow') is null
    begin
		INSERT INTO workflow_access (workflow_id, user_id, permission_level_id, last_modified_by)
		SELECT workflow_id, last_modified_by, 1, 1 FROM inserted

		INSERT INTO workflow_access (workflow_id, group_id, permission_level_id, last_modified_by)
		SELECT c.workflow_id, 1, 1, 1
		FROM inserted AS c WHERE c.workflow_id NOT IN (
			SELECT ca.workflow_id FROM workflow_access AS ca
			WHERE ca.workflow_id = c.workflow_id AND ca.group_id = 1
		)
	END
END
GO
ALTER TRIGGER [dbo].[ti_add_to_everyone_group] ON [dbo].[USERS] 
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_add_to_everyone_group') is null
    BEGIN    
        INSERT INTO user_group_bind (user_id, group_id)
        SELECT i.user_id, ug.group_id FROM inserted i, user_group ug WHERE ug.built_in = 1 and ug.readonly = 1
    END
END
GO
ALTER TRIGGER [dbo].[ti_content_item_schedule_add_job] ON [dbo].[CONTENT_ITEM_SCHEDULE] FOR INSERT AS BEGIN
  DECLARE @current_db SYSNAME, @item_id NUMERIC, @qp_job_name SYSNAME, @sql NVARCHAR(1024)
  DECLARE @freq_type INT, @freq_interval INT, @freq_relative_interval INT, @freq_recurrence_factor INT
  DECLARE @active_start_date INT, @active_end_date INT, @active_start_time INT, @active_end_time INT
  DECLARE @use_duration INT, @deactivate BIT
  DECLARE @pre_sql NVARCHAR(1024)
  declare @str_set_params nvarchar(255)
  SELECT @current_db = DB_NAME()

  UPDATE CONTENT_ITEM_SCHEDULE
  set
    START_DATE = COALESCE(START_DATE, dbo.get_schedule_date(active_start_date, active_start_time)),
    END_DATE = COALESCE(END_DATE, dbo.get_schedule_date(active_end_date, active_end_time))
  where CONTENT_ITEM_ID in (select CONTENT_ITEM_ID from inserted)

  DECLARE items CURSOR FOR
    SELECT content_item_id, freq_type, freq_interval, freq_relative_interval, freq_recurrence_factor,
      active_start_date, active_end_date, active_start_time, active_end_time, use_duration, deactivate
    FROM inserted where use_service = 0
  OPEN items

  FETCH NEXT FROM items
  INTO @item_id, @freq_type, @freq_interval, @freq_relative_interval, @freq_recurrence_factor,
    @active_start_date, @active_end_date, @active_start_time, @active_end_time, @use_duration, @deactivate
  WHILE @@FETCH_STATUS = 0 BEGIN

    DECLARE @delete_level INT

    IF @freq_type = 1 OR @freq_type = 2 BEGIN
      DECLARE @now_date DATETIME
      DECLARE @now_date_int BIGINT, @start_date_int BIGINT, @end_date_int BIGINT

      SET @now_date = DATEADD(mi, 1, GETDATE())
      SET @now_date_int =  DATEPART(ss, @now_date) + (100 * DATEPART(mi, @now_date)) + (10000 * DATEPART(hh, @now_date)) + (1000000 * DAY(@now_date)) + (100000000 * MONTH(@now_date)) + (10000000000 * YEAR(@now_date))
      SET @start_date_int = CAST(@active_start_time AS BIGINT) + CAST(@active_start_date AS BIGINT) * 1000000
      SET @end_date_int   = CAST(@active_end_time AS BIGINT) + CAST(@active_end_date AS BIGINT) * 1000000

      IF @now_date_int > @start_date_int BEGIN
        SET @active_start_date = @now_date_int / 1000000
        SET @active_start_time = @now_date_int % 1000000

        UPDATE content_item_schedule
        SET active_start_date = @active_start_date, active_start_time = @active_start_time
        WHERE CONTENT_ITEM_ID = @item_id
      END

      SET @now_date = DATEADD(ss, 10, @now_date)
      SET @now_date_int =  DATEPART(ss, @now_date) + (100 * DATEPART(mi, @now_date)) + (10000 * DATEPART(hh, @now_date)) + (1000000 * DAY(@now_date)) + (100000000 * MONTH(@now_date)) + (10000000000 * YEAR(@now_date))

      IF @now_date_int > @end_date_int BEGIN
        SET @active_end_date = @now_date_int / 1000000
        SET @active_end_time = @now_date_int % 1000000

        UPDATE content_item_schedule
        SET active_end_date = @active_end_date, active_end_time = @active_end_time
        WHERE CONTENT_ITEM_ID = @item_id
      END

      SET @delete_level  = 1

    END ELSE BEGIN
      SET @delete_level  = 0
    END

    SET @qp_job_name = 'Q-Publishing Schedule for ' + @current_db + ' item '
      + CAST(@item_id AS NVARCHAR) + ' on'
    IF EXISTS(SELECT * FROM msdb.dbo.sysjobs_view WHERE name = @qp_job_name) BEGIN
      EXEC msdb.dbo.sp_delete_job @job_name = @qp_job_name
    END

	IF @deactivate = 0 BEGIN	--if schedule is deactivated then don't create job
		if dbo.qp_is_sql_2000() = 1
			set @str_set_params =  '@activation_start_dt=[STRTDT], @activation_start_tm=[STRTTM]'
		else if dbo.qp_is_early_sql_2005() = 1
			set @str_set_params =  '@activation_start_dt=$' + '(STRTDT), @activation_start_tm=$' + '(STRTTM)'
		else
			set @str_set_params =  '@activation_start_dt=$' + '(ESCAPE_NONE(STRTDT)), @activation_start_tm=$' + '(ESCAPE_NONE(STRTTM))'
		if @freq_type <> 2
			SET @sql = 'UPDATE content_item with(rowlock) SET visible = 1 WHERE content_item_id = '
			  + CAST(@item_id AS NVARCHAR)
			  + '
				EXECUTE qp_create_deactivation_job @item_id=' + CAST(@item_id AS NVARCHAR) + ', ' + @str_set_params
		else begin	--scheduleNewVersionPublication
			set @sql  = 'exec qp_merge_article ' + CAST(@item_id AS NVARCHAR)
			set @freq_type = 1
		end

		SET @pre_sql = 'Q-Publishing Schedule for ' + @current_db + ' item ' + CAST(@item_id AS NVARCHAR) + ' off'
		SET @pre_sql = 'IF EXISTS(SELECT * FROM msdb.dbo.sysjobs_view WHERE name = ''' + @pre_sql + ''') EXEC msdb.dbo.sp_delete_job @job_name = ''' + @pre_sql + ''' '

		EXEC msdb.dbo.sp_add_job @job_name = @qp_job_name, @delete_level  =  @delete_level
		EXEC msdb.dbo.sp_add_jobstep @job_name = @qp_job_name, @step_name = 'Remove old deactivation job',
		  @command = @pre_sql, @database_name = @current_db,
		  @retry_attempts = 1,
		  @on_success_action  = 3, @on_fail_action = 3
		EXEC msdb.dbo.sp_add_jobstep @job_name = @qp_job_name, @step_name = 'Activate article',
		  @command = @sql, @database_name = @current_db,
		  @retry_attempts = 1
		EXEC msdb.dbo.sp_add_jobschedule @job_name = @qp_job_name, @name = 'Activate Schedule',
		  @enabled = 1, @freq_type = @freq_type, @freq_interval = @freq_interval,
		  @freq_relative_interval = @freq_relative_interval,
		  @freq_recurrence_factor = @freq_recurrence_factor,
		  @freq_subday_type = 0x1, @freq_subday_interval = 0,
		  @active_start_date = @active_start_date, @active_end_date = @active_end_date,
		  @active_start_time = @active_start_time, @active_end_time = @active_end_time
		EXEC msdb.dbo.sp_add_jobserver @job_name = @qp_job_name, @server_name = '(LOCAL)'
	END

    FETCH NEXT FROM items
    INTO @item_id, @freq_type, @freq_interval, @freq_relative_interval, @freq_recurrence_factor,
      @active_start_date, @active_end_date, @active_start_time, @active_end_time, @use_duration, @deactivate
  END
  CLOSE items
  DEALLOCATE items
END
go


ALTER TRIGGER [dbo].[ti_content_to_content] ON [dbo].[content_to_content] AFTER INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_content_to_content') is null
    begin
		declare @link_id numeric, @i numeric, @count numeric, @inscount numeric

		declare @cc table (
			id numeric identity(1,1) primary key,
			link_id numeric
		)

		select @count = count(link_id) from inserted

		if (@count = 1) -- prevent @@identity change (for restore site)
		begin
			select @link_id = link_id from inserted
			exec qp_build_link_view @link_id
		end
		else if (@count > 1)
		begin
			insert into @cc (link_id) select i.link_id from inserted i

			set @i = 1

			while @i < @count + 1
			begin
				select @link_id = link_id from @cc where id = @i
				exec qp_build_link_view @link_id
				set @i = @i + 1
			end
		end
	end
END
GO
ALTER TRIGGER [dbo].[ti_insert_field] ON [dbo].[CONTENT_ATTRIBUTE] FOR INSERT
AS
BEGIN
	if object_id('tempdb..#disable_ti_insert_field') is null
	begin
		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric, @content_id numeric
		declare @indexed numeric, @required numeric
		declare @attribute_type_id numeric, @type_name nvarchar(255), @database_type nvarchar(255)

		declare @base_table_name nvarchar(30), @table_name nvarchar(30)

		declare @i numeric, @count numeric, @max numeric

		declare @ca table (
			id numeric identity(1,1) primary key,
			attribute_id numeric,
			attribute_name nvarchar(255),
			attribute_size numeric,
			indexed numeric,
			required numeric,
			attribute_type_id numeric,
			type_name nvarchar(255),
			database_type nvarchar(255),
			content_id numeric
		)

		/* Collect affected items */
		insert into @ca (attribute_id, attribute_name, attribute_size, indexed, required, attribute_type_id, type_name, database_type, content_id)
			select i.attribute_id, i.attribute_name, i.attribute_size, i.index_flag, i.required, i.attribute_type_id, at.type_name, at.database_type, i.content_id
			from inserted i
			inner join attribute_type at on i.attribute_type_id = at.attribute_type_id
			inner join content c on i.content_id = c.content_id
			where c.virtual_type = 0

		set @i = 1
		select @count = count(id) from @ca

		while @i < @count + 1
		begin
			select @attribute_id = attribute_id, @attribute_name = attribute_name, @attribute_size = attribute_size,
				@indexed = indexed, @required = required, @attribute_type_id = attribute_type_id,
				@type_name = type_name, @database_type = database_type, @content_id = content_id
				from @ca where id = @i

				set @base_table_name = 'content_' + convert(nvarchar, @content_id)

				IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = OBJECT_ID(@base_table_name) AND OBJECTPROPERTY(id, 'IsUserTable') = 1)
				begin
					exec qp_rebuild_content @content_id
				end
				else begin

					/* Add column in common and async tables */
					set @table_name = @base_table_name + '_ASYNC'
					exec qp_add_column @base_table_name, @attribute_name, @type_name, @database_type, @attribute_size
					exec qp_add_column @table_name, @attribute_name, @type_name, @database_type, @attribute_size

					/* Create indexes on new fields if required */
					if @indexed = 1
					begin
						exec qp_add_index @base_table_name, @attribute_name
						exec qp_add_index @table_name, @attribute_name
					end

					/* Recreate United View */
					exec qp_content_united_view_recreate @content_id
					exec qp_content_frontend_views_recreate @content_id
				end
			set @i = @i + 1
		end
	END
END
GO

ALTER TRIGGER [dbo].[ti_insert_item] ON [dbo].[CONTENT_ITEM] FOR INSERT AS
BEGIN
    declare @content_id numeric
    declare @ids_list nvarchar(max)

    declare @table_name varchar(50), @sql nvarchar(max)

    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )

    insert into @contents
    select distinct i.content_id, st.STATUS_TYPE_ID from inserted i
    inner join content c on i.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID

    declare @ids [Ids]
    declare @ids2 [Ids]
    declare @noneId numeric

    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents

        insert into @ids
        select i.content_item_id from inserted i
        where i.CONTENT_ID = @content_id and i.not_for_replication = 0

        if exists (select id from @ids)
        begin

            insert into @ids2
            select i.content_item_id from inserted i
            where i.CONTENT_ID = @content_id and i.not_for_replication = 0 and i.SCHEDULE_NEW_VERSION_PUBLICATION = 1

            set @table_name = 'content_' + convert(nvarchar, @content_id)

            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @ids, @ids2 = @ids2, @noneId = @noneId

            delete from @ids2
            delete from @ids

        end

        delete from @contents where id = @content_id

    end

END
GO

ALTER TRIGGER [dbo].[ti_insert_modify_row] ON [dbo].[CONTENT] FOR INSERT
AS
BEGIN
  	if object_id('tempdb..#disable_ti_insert_modify_row') is null
	begin
		INSERT INTO CONTENT_MODIFICATION
		SELECT CONTENT_ID, GETDATE(), GETDATE() from inserted
	end

END
GO

exec qp_drop_existing 'ti_item_link_async', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[ti_item_link_async] ON [dbo].[item_link_async] AFTER INSERT
AS
BEGIN

  declare @links table
  (
    id numeric primary key,
    is_symmetric bit,
    l_content_id numeric,
    r_content_id numeric
  )

  insert into @links
  select distinct i.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from inserted i inner join content_to_content c2c on i.link_id = c2c.link_id

  declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

  while exists(select id from @links)
  begin

    declare @link_items [Links]

    select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

    insert into @link_items
    select distinct item_id, linked_item_id from inserted where link_id = @link_id

    declare @self_related bit
    select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end

    exec qp_insert_link_table_item @link_id, @l_content_id, @link_items, 1, 0, 0
    exec qp_insert_link_table_item @link_id, @r_content_id, @link_items, 1, 1, @self_related

    delete from @link_items

    delete from @links where id = @link_id

  end
END
GO


ALTER TRIGGER [dbo].[ti_item_to_item] ON [dbo].[item_to_item] AFTER INSERT
AS
BEGIN

if object_id('tempdb..#disable_ti_item_to_item') is null
  begin
    with items (link_id, item_id, linked_item_id)
    AS
    (
      select i1.link_id, i1.l_item_id, i1.r_item_id From inserted i1
      inner join content_to_content c2c on i1.link_id = c2c.link_id
      where c2c.[symmetric] = 1 and not exists (select * from item_to_item i2 where i1.link_id = i2.link_id and i1.r_item_id = i2.l_item_id and i2.r_item_id = i1.l_item_id)
    )
    insert into item_to_item(link_id, l_item_id, r_item_id)
    select link_id, linked_item_id, item_id from items
  end

  declare @links table
  (
    id numeric primary key,
    is_symmetric bit,
    l_content_id numeric,
    r_content_id numeric
  )

  insert into @links
  select distinct i.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from inserted i inner join content_to_content c2c on i.link_id = c2c.link_id

  declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

  while exists(select id from @links)
  begin

    declare @link_items [Links]

    select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

    insert into @link_items
    select distinct l_item_id, r_item_id from inserted where link_id = @link_id

    if @is_symmetric = 1
    begin
      insert into @link_items
      select r_item_id, l_item_id from inserted i where link_id = @link_id
      and not exists (select * from @link_items where id = i.r_item_id and linked_id = i.l_item_id)
    end

    declare @self_related bit
    select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end


    exec qp_insert_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
    exec qp_insert_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

    delete from @link_items

    delete from @links where id = @link_id

  end
END
GO

ALTER TRIGGER [dbo].[ti_statuses_and_default_notif] ON [dbo].[SITE]
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_statuses_and_default_notif') is null
    begin
        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Created',  10, 'Article has been created' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Approved',  50, 'Article has been modified' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Published',  100, 'Article has been published' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'None',  0, 'No Status has been assigned' ,1, 1 from inserted)
 
        INSERT INTO page_template(site_id, template_name, net_template_name, template_picture, created, modified, last_modified_by, charset, codepage, locale, is_system, net_language_id)
        select site_id, 'Default Notification Template', 'Default_Notification_Template', '', getdate(), getdate(), 1, 'utf-8', 65001, 1049, 1, dbo.qp_default_net_language(script_language) from inserted

        insert into content_group (site_id, name)
        select site_id, 'Default Group' from inserted
    end
END
go


ALTER TRIGGER [dbo].[tu_content_attribute_clean_empty_links] ON [dbo].[CONTENT_ATTRIBUTE] FOR UPDATE
AS
BEGIN
	if update(link_id) and object_id('tempdb..#disable_tu_content_attribute_clean_empty_links') is null
	begin
		declare @link_id numeric, @attribute_id numeric, @version numeric
		declare @i numeric, @count numeric
		declare @links table (
			id numeric identity(1,1) primary key,
			link_id numeric,
			attribute_id numeric
		)

		insert into @links (link_id, attribute_id)
		select d.link_id, d.attribute_id from deleted d inner join inserted i on d.attribute_id = i.attribute_id where d.link_id IS NOT NULL AND (i.link_id IS NULL OR i.link_id <> d.link_id) 

		set @i = 1
		select @count = count(id) from @links
		set @version = dbo.qp_get_version_control()		

		while @i < @count + 1
		begin
			select @link_id = link_id, @attribute_id = attribute_id from @links where id = @i
			
			exec qp_drop_link_with_check @link_id
			
			if @version is not null
			   DELETE FROM item_to_item_version WHERE attribute_id = @attribute_id
			
			set @i = @i + 1
		end
	end
END
GO
ALTER TRIGGER [dbo].[tu_content_indexes] ON [dbo].[content_constraint] 
FOR UPDATE 
AS
BEGIN
	if object_id('tempdb..#disable_tu_content_indexes') is null
	BEGIN
        DECLARE @constraint_id numeric
        DECLARE @content_id numeric
        DECLARE @attribute_id numeric
        DECLARE @count numeric
        SELECT @constraint_id = COUNT(constraint_id) FROM deleted
        IF @constraint_id <> 0 
        BEGIN
            DECLARE Constraints CURSOR FOR SELECT constraint_id, content_id FROM deleted
            OPEN Constraints
            FETCH NEXT FROM Constraints INTO @constraint_id, @content_id
            WHILE @@fetch_status = 0 BEGIN
                print @constraint_id
                print @content_id
                exec qp_drop_complex_index @constraint_id, 1, @content_id
                exec qp_drop_complex_index @constraint_id, 0, @content_id
                FETCH NEXT FROM Constraints INTO @constraint_id, @content_id
            END
            CLOSE Constraints
            DEALLOCATE Constraints
        END
        DECLARE Constraints CURSOR FOR SELECT constraint_id FROM inserted
        OPEN Constraints
        FETCH NEXT FROM Constraints INTO @constraint_id
        WHILE @@fetch_status = 0 BEGIN
            SELECT @count = count(constraint_id),@attribute_id = min(attribute_id) FROM content_constraint_rule WHERE constraint_id = @constraint_id
            IF (@count = 1)
                UPDATE content_attribute SET index_flag = 1 WHERE attribute_id = @attribute_id
            ElSE
                IF (@count > 1)
                BEGIN
                    exec qp_create_complex_index @constraint_id, 1
                    exec qp_create_complex_index @constraint_id, 0
                END
            FETCH NEXT FROM Constraints INTO @constraint_id
        END
        CLOSE Constraints
        DEALLOCATE Constraints
    END
END
GO
EXEC qp_drop_existing '[dbo].[tu_db]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tu_db]
   ON [dbo].[DB]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;

    DECLARE @CdcChangeBit bit;
    SELECT @CdcChangeBit = i.USE_CDC FROM Inserted i INNER JOIN Deleted d ON i.ID = d.ID AND i.USE_CDC != d.USE_CDC;

    IF @CdcChangeBit = 1 BEGIN
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_attribute',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_data',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_item',
           @captured_column_list = 'content_item_id,visible,status_type_id,created,modified,content_id,last_modified_by,archive,not_for_replication,schedule_new_version_publication,splitted,permanent_lock,cancel_split,unique_id',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_to_content',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'item_link_async',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'item_to_item',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'status_type',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
    END ELSE IF @CdcChangeBit = 0 BEGIN
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_attribute',
           @capture_instance = N'dbo_content_attribute';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content',
           @capture_instance = N'dbo_content';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_data',
           @capture_instance = N'dbo_content_data';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_item',
           @capture_instance = N'dbo_content_item';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_to_content',
           @capture_instance = N'dbo_content_to_content';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'item_link_async',
           @capture_instance = N'dbo_item_link_async';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'item_to_item',
           @capture_instance = N'dbo_item_to_item';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'status_type',
           @capture_instance = N'dbo_status_type';
    END
END
GO

exec qp_drop_existing 'tu_item_to_item', 'IsTrigger'
go

CREATE TRIGGER [dbo].[tu_item_to_item] ON [dbo].[item_to_item] AFTER UPDATE
AS
BEGIN

    if object_id('tempdb..#disable_tu_item_to_item') is null
    BEGIN

	    if update(l_item_id) or update(r_item_id)
	    BEGIN

            declare @links table
	        (
	    	    id numeric primary key,
	    	    item_id numeric
	        )

	    	insert into @links
	    	select distinct link_id, l_item_id from inserted

	    	declare @link_id numeric, @item_id numeric , @query nvarchar(max)

	    	while exists(select id from @links)
	    	BEGIN

	    		select @link_id = id from @links
	    		select 	@item_id = item_id from @links

	    		declare @table_name nvarchar(50), @table_name_rev nvarchar(50)
	    		set @table_name = 'item_link_' + cast(@link_id as varchar)
	    		set @table_name_rev = 'item_link_' + cast(@link_id as varchar) + '_rev'

	    		declare @linked_item numeric
	    		select @linked_item = l_item_id from inserted

	    		set @query = 'update ' + @table_name + ' set linked_id = @linked_item where id = @item_id'
	    		print @query
	    		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

	    		set @query = 'update ' + @table_name_rev + ' set linked_id = @linked_item where id = @item_id'
	    		print @query
	    		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

	    		delete from @links where id = @link_id
	    	END
	    END
    END
END
GO

ALTER TRIGGER [dbo].[tu_not_for_replication] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
BEGIN
	if update(not_for_replication)
	begin
		update base set base.not_for_replication = i.not_for_replication
		from content_data base inner join inserted i on i.content_item_id = base.CONTENT_ITEM_ID
	end

    if update(splitted)
	begin
		update base set base.splitted = i.splitted
		from content_data base inner join inserted i on i.content_item_id = base.CONTENT_ITEM_ID
	end
END
GO

ALTER  TRIGGER [dbo].[tu_update_field] ON [dbo].[CONTENT_ATTRIBUTE] FOR UPDATE
AS
BEGIN
if not update(attribute_order) and object_id('tempdb..#disable_tu_update_field') is null and
		(
			update(attribute_name) or update(attribute_type_id)
			or update(attribute_size) or update(index_flag) or update(is_long)
		)
	begin
		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric, @content_id numeric
		declare @indexed numeric, @required numeric, @is_long bit
		declare @attribute_type_id numeric, @type_name nvarchar(255), @database_type nvarchar(255)

		declare @new_attribute_name nvarchar(255), @new_attribute_size numeric
		declare @new_indexed numeric, @new_required numeric, @new_is_long bit
		declare @new_attribute_type_id numeric, @new_type_name nvarchar(255), @new_database_type nvarchar(255)
		declare @related_content_id numeric, @new_related_content_id numeric
		declare @link_id numeric, @new_link_id numeric

		declare @base_table_name nvarchar(30), @table_name nvarchar(30)

		declare @i numeric, @count numeric, @preserve_index bit

		declare @ca table (
			id numeric identity(1,1) primary key,
			attribute_id numeric,
			attribute_name nvarchar(255),
			attribute_size numeric,
			indexed numeric,
			required numeric,
			attribute_type_id numeric,
			type_name nvarchar(255),
			database_type nvarchar(255),
			content_id numeric,
			related_content_id numeric,
			link_id numeric,
			is_long bit
		)

	/* Collect affected items */
		insert into @ca (attribute_id, attribute_name, attribute_size, indexed, required, attribute_type_id, type_name, database_type, content_id, related_content_id, link_id, is_long)
			select d.attribute_id, d.attribute_name, d.attribute_size, d.index_flag, d.required, d.attribute_type_id, at.type_name, at.database_type, d.content_id,
			isnull(ca1.content_id, 0), isnull(d.link_id, 0), d.is_long
			from deleted d
			inner join attribute_type at on d.attribute_type_id = at.attribute_type_id
			inner join content c on d.content_id = c.content_id
			left join CONTENT_ATTRIBUTE ca1 on d.RELATED_ATTRIBUTE_ID = ca1.ATTRIBUTE_ID
			where c.virtual_type = 0

		set @i = 1
		select @count = count(id) from @ca

		while @i < @count + 1
		begin
			select @attribute_id = attribute_id, @attribute_name = attribute_name, @attribute_size = attribute_size,
				@indexed = indexed, @required = required, @attribute_type_id = attribute_type_id,
				@type_name = type_name, @database_type = database_type, @content_id = content_id,
				@related_content_id = related_content_id, @link_id = link_id, @is_long = is_long
				from @ca where id = @i

			select @new_attribute_name = ca.attribute_name, @new_attribute_size = ca.attribute_size,
				@new_indexed = ca.index_flag, @new_required = ca.required, @new_attribute_type_id = ca.attribute_type_id,
				@new_type_name = at.type_name, @new_database_type = at.database_type,
				@new_related_content_id = isnull(ca1.content_id, 0), @new_link_id = isnull(ca.link_id, 0), @new_is_long = ca.IS_LONG
				from content_attribute ca
				inner join attribute_type at on ca.attribute_type_id = at.attribute_type_id
				left join CONTENT_ATTRIBUTE ca1 on ca.RELATED_ATTRIBUTE_ID = ca1.ATTRIBUTE_ID
				where ca.attribute_id = @attribute_id

				set @base_table_name = 'content_' + convert(nvarchar, @content_id)
				set @table_name = @base_table_name + '_ASYNC'

				if @indexed = 1 and @new_indexed = 1
					set @preserve_index = 1
				else
					set @preserve_index = 0

				if @attribute_type_id <> @new_attribute_type_id
					or @link_id <> @new_link_id
					or @related_content_id <> @new_related_content_id
					or (@attribute_size > @new_attribute_size and @attribute_type_id = 1)
				begin
					exec qp_clear_versions_for_field @attribute_id
				end

				if @indexed = 1 and @new_indexed = 0
				begin
					exec qp_drop_index @base_table_name, @attribute_name
					exec qp_drop_index @table_name, @attribute_name
				end

				if @database_type <> @new_database_type or (@attribute_size <> @new_attribute_size and @new_database_type <> 'ntext')
				begin
					exec qp_recreate_column @base_table_name, @attribute_id, @attribute_name, @new_attribute_name, @type_name, @new_type_name, @new_database_type, @new_attribute_size, @preserve_index
					exec qp_recreate_column @table_name, @attribute_id, @attribute_name, @new_attribute_name, @type_name, @new_type_name, @new_database_type, @new_attribute_size, @preserve_index
					exec qp_content_united_view_recreate @content_id
					exec qp_content_frontend_views_recreate @content_id
				end
				else if @attribute_name <> @new_attribute_name
				begin
					exec qp_rename_column @base_table_name, @attribute_name, @new_attribute_name, @preserve_index
					exec qp_rename_column @table_name, @attribute_name, @new_attribute_name, @preserve_index
					exec qp_content_united_view_recreate @content_id
					exec qp_content_frontend_views_recreate @content_id
				end
				else if @is_long <> @new_is_long
				begin
					exec qp_content_frontend_views_recreate @content_id
				end

				if @attribute_name <> @new_attribute_name
					UPDATE container Set order_static = REPLACE(order_static, @attribute_name, @new_attribute_name) WHERE content_id = @content_id AND order_static LIKE '%'+ @attribute_name +'%'

				if @indexed = 0 and @new_indexed = 1
				begin
					exec qp_add_index @base_table_name, @new_attribute_name
					exec qp_add_index @table_name, @new_attribute_name
				end
			set @i = @i + 1
		end
	end
END
GO

ALTER TRIGGER [dbo].[tu_update_item] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
begin
    if not update(locked_by) and not update(splitted) and not UPDATE(not_for_replication)
    begin
        declare @content_id numeric
        declare @sql nvarchar(max), @table_name varchar(50), @async_table_name varchar(50)
        declare @items_list nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)

        declare @contents table
        (
            id numeric primary key
        )

        insert into @contents
        select distinct content_id from inserted
        where CONTENT_ID in (select CONTENT_ID from content where virtual_type = 0)

        create table #ids_with_splitted
        (
            id numeric primary key,
            new_splitted bit
        )

        declare @items table
        (
            id numeric primary key,
            splitted bit,
            not_for_replication bit,
            cancel_split bit
        )

        declare @ids [Ids], @ids2 [Ids]

        while exists (select id from @contents)
        begin
            select @content_id = id from @contents

            insert into @items
            select i.content_item_id, i.SPLITTED, i.not_for_replication, i.cancel_split from inserted i
            inner join content_item ci on i.content_item_id = ci.content_item_id
            where ci.CONTENT_ID = @content_id

            insert into @ids
            select id from @items

            insert into @ids2
            select id from @items where cancel_split = 1

            set @items_list = null
            select @items_list = coalesce(@items_list + ',', '') + convert(nvarchar, id) from @items

            set @sql = 'insert into #ids_with_splitted '
            set @sql = @sql + ' select content_item_id,'
            set @sql = @sql + ' case'
            set @sql = @sql + ' when curr_weight < front_weight and is_workflow_async = 1 then 1'
            set @sql = @sql + ' when curr_weight = workflow_max_weight and delayed = 1 then 1'
            set @sql = @sql + ' else 0'
            set @sql = @sql + ' end'
            set @sql = @sql + ' as new_splitted from ('
            set @sql = @sql + ' select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, '
            set @sql = @sql + ' max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, case when i2.id is not null then 0 else ciw.is_async end as is_workflow_async, '
            set @sql = @sql + ' ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed '
            set @sql = @sql + ' from content_item ci'
            set @sql = @sql + ' inner join @ids i on i.id = ci.content_item_id'
            set @sql = @sql + ' left join @ids2 i2 on i.id = ci.content_item_id'
            set @sql = @sql + ' inner join content_' + CONVERT(nvarchar, @content_id) + ' c WITH(UPDLOCK, ROWLOCK) on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID'
            set @sql = @sql + ' left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id'
            set @sql = @sql + ' left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID'
            set @sql = @sql + ' left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID'
            set @sql = @sql + ' ) as main'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY', @ids = @ids, @ids2 = @ids2

            update base set base.splitted = i.new_splitted from @items base inner join #ids_with_splitted i on base.id = i.id
            update base set base.splitted = i.splitted from content_item base inner join @items i on base.CONTENT_ITEM_ID = i.id

            insert into content_item_splitted(content_item_id)
            select id from @items base where splitted = 1 and not exists (select * from content_item_splitted cis where cis.content_item_id = base.id)

            delete from content_item_splitted where content_item_id in (
                select id from @items base where splitted = 0
            )

            set @sync_ids_list = null
            select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 0 and not_for_replication = 0
            set @async_ids_list = null
            select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 1 and not_for_replication = 0

            set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
            set @async_table_name = @table_name + '_async'

            if @sync_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            if @async_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            delete from #ids_with_splitted

            delete from @contents where id = @content_id

            delete from @items
            delete from @ids
            delete from @ids2
        end

        drop table #ids_with_splitted

    end
end
GO

if not exists (select * from BACKEND_ACTION where code = 'export_archive_article')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, code, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
VALUES(dbo.qp_action_type_id('export'), dbo.qp_entity_type_id('archive_article'), 'Export Archive Articles', 'export_archive_article', '~/ExportArchiveArticles/', 1, 600, 400, 1, 1)

if not exists (select * from BACKEND_ACTION where code = 'multiple_export_archive_article')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, code, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
VALUES(dbo.qp_action_type_id('multiple_export'), dbo.qp_entity_type_id('archive_article'), 'Multiple Export Archive Articles', 'multiple_export_archive_article', '~/ExportSelectedArchiveArticles/', 1, 600, 400, 1, 1)

if not exists (select * from ACTION_TOOLBAR_BUTTON where parent_action_id = dbo.qp_action_id('list_archive_article') and name = 'Export')
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], icon)
values (dbo.qp_action_id('list_archive_article'), dbo.qp_action_id('multiple_export_archive_article'), 'Export', 15, 'other/export.gif')

IF NOT EXISTS(SELECT * FROM CONTEXT_MENU_ITEM WHERE NAME = 'Unselect Child Articles' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_article'))
INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER],  ICON)
VALUES(dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('unselect_child_articles'), 'Unselect Child Articles', 90, 'deselect_all.gif')

IF NOT EXISTS(SELECT * FROM  CONTEXT_MENU_ITEM WHERE NAME = 'Select Child Articles' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_article'))
INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
VALUES(dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('select_child_articles'), 'Select Child Articles', 80, 'select_all.gif')

GO
if not exists (select * From BACKEND_ACTION where code = 'copy_custom_action')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [CONTROLLER_ACTION_URL],[IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('copy'), dbo.qp_entity_type_id('custom_action'), N'Create Like Custom Action', 'Create Like', N'copy_custom_action', '~/CustomAction/Copy/', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [ICON],  [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('custom_action'), dbo.qp_action_id('copy_custom_action'), N'Create Like', 5, 'create_like.gif', 1)

end

declare @cnt numeric

;with numbers(num)  as
(
	select top (1000) ROW_NUMBER() OVER (ORDER BY content_data_id) as num from content_data cd where cd.O2M_DATA is not null
)
select @cnt = max(num) from numbers


if @cnt < 1000
begin
    update content_data set O2M_DATA = try_convert(numeric, cd.data) from content_data cd
    inner join content_attribute ca on ca.attribute_id = cd.attribute_id
    where ca.attribute_type_id = 11 and ca.link_id is null
    and cd.data is not null and isnumeric(cd.data) = 1 and cd.data <> '0' and cd.O2M_DATA is null

    update version_content_data set O2M_DATA = try_convert(numeric, vcd.data) from version_content_data vcd
    inner join content_attribute ca on ca.attribute_id = vcd.attribute_id
    where ca.attribute_type_id = 11 and ca.link_id is null
    and vcd.data is not null and isnumeric(vcd.data) = 1 and vcd.data <> '0' and vcd.O2M_DATA is null
end
GO


update CONTENT_DATA set SPLITTED = i.splitted
from content_data cd inner join CONTENT_ITEM i on i.content_item_id = cd.CONTENT_ITEM_ID
WHERE i.SPLITTED = 1
GO
if not exists (select * from APP_SETTINGS where [key] = 'CONTENT_MODIFICATION_UPDATE_INTERVAL')
  insert into APP_SETTINGS
  values ('CONTENT_MODIFICATION_UPDATE_INTERVAL', '30')
GO

update CONTENT_ITEM_SCHEDULE
SET
    START_DATE = dbo.get_schedule_date(isnull(active_start_date, 17530101), active_start_time),
    END_DATE = dbo.get_schedule_date(active_end_date, active_end_time)

GO


declare @cnt numeric
select @cnt = count(*) from item_link where is_rev = 1
if @cnt < 1000
begin
	update item_link set is_rev = 1
	from item_link il
	inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
	inner join content_to_content cc on il.link_id = cc.link_id and ci.CONTENT_ID <> cc.l_content_id
	and is_rev = 0
end
GO

update item_link_async set is_rev = 1
from item_link_async il
inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
inner join content_to_content cc on il.link_id = cc.link_id and ci.CONTENT_ID <> cc.l_content_id
and is_rev = 0
GO

update item_link set is_self = 1
from item_link il inner join content_to_content cc on il.link_id = cc.link_id where cc.l_content_id = cc.r_content_id
GO


update item_link_async set is_self = 1
from item_link_async il inner join content_to_content cc on il.link_id = cc.link_id where cc.l_content_id = cc.r_content_id
GO


if not exists (select * from BACKEND_ACTION where ENTITY_TYPE_ID = dbo.qp_entity_type_id('article') and code = 'view_live_article')
begin
	insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES(dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('article'), 'Article Live Properties', 'Live Properties', 'view_live_article', '~/Article/LiveProperties/', 1)
end
GO

if not exists(select * from CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Live Properties')
begin
	insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
	VALUES(dbo.qp_context_menu_id('article'), dbo.qp_action_id('view_live_article'), 'Live Properties', 52, 'properties.gif')
end
GO

if not exists(select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('view_live_article'))
begin
	insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, [ORDER])
	values(dbo.qp_action_id('view_live_article'), dbo.qp_action_id('refresh_article'), 'Refresh', 'refresh.gif', 10)

end
GO

exec qp_update_translations 'Live Properties', 'Свойства Live'
GO


if not exists (select * From BACKEND_ACTION where code = 'select_child_articles')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Select Child Articles', 'Select Child Articles', N'select_child_articles', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('article'), dbo.qp_action_id('select_child_articles'), N'Select Child Articles', 80, 0)

end

if not exists (select * From BACKEND_ACTION where code = 'unselect_child_articles')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Unselect Child Articles', 'Unselect Child Articles', N'unselect_child_articles', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('article'), dbo.qp_action_id('unselect_child_articles'), N'Unselect Child Articles', 90, 0)

end

update CONTEXT_MENU_ITEM set icon = 'deselect_all.gif' where ACTION_ID = dbo.qp_action_id('unselect_child_articles')
update CONTEXT_MENU_ITEM set icon = 'select_all.gif' where ACTION_ID = dbo.qp_action_id('select_child_articles')

exec qp_update_translations 'Select Child Articles', 'Выбрать дочерние статьи'
exec qp_update_translations 'Unselect Child Articles', 'Отменить выбор дочерних статей'

if not exists(
	select * From INFORMATION_SCHEMA.VIEWS where table_name like 'content%' and TABLE_NAME like '%new'
) or object_id('tempdb..#qp_rebuild_all_new_views') is not null
begin
	exec qp_rebuild_all_new_views
end
GO

if not exists(
	select * From INFORMATION_SCHEMA.VIEWS where table_name like 'item_link%' and TABLE_NAME like '%rev'
) or object_id('tempdb..#qp_rebuild_all_link_views') is not null
begin
	exec qp_rebuild_all_link_views
end
GO

if not exists(
	select * From INFORMATION_SCHEMA.TABLES where table_name like 'item_link%' and TABLE_NAME like '%rev' and TABLE_TYPE = 'BASE TABLE'
) or object_id('tempdb..#qp_recreate_link_tables') is not null
begin
	exec qp_recreate_link_tables
end
GO

DECLARE @articles_with_wrong_statuses TABLE (
Site_ID int,
CONTENT_ID int,
STATUS_TYPE_ID int,
CONTENT_ITEM_ID int
)

INSERT INTO @articles_with_wrong_statuses
	SELECT c.Site_ID, c.CONTENT_ID, ci.STATUS_TYPE_ID, ci.CONTENT_ITEM_ID FROM [dbo].[CONTENT_ITEM] ci
	INNER JOIN [dbo].[CONTENT] c ON ci.CONTENT_ID = c.CONTENT_ID
	INNER JOIN [dbo].[STATUS_TYPE] st ON ci.STATUS_TYPE_ID = st.STATUS_TYPE_ID
	WHERE st.SITE_ID <> c.SITE_ID

IF EXISTS (SELECT * FROM @articles_with_wrong_statuses)
BEGIN
DECLARE @statuses_names TABLE (
	STATUS_TYPE_NAME nvarchar(255),
	STATUS_TYPE_ID int,
	NEW_SITE int
)

INSERT INTO @statuses_names
	SELECT st1.STATUS_TYPE_NAME, st1.STATUS_TYPE_ID as old_status, st1.SITE_ID as old_site from [dbo].[STATUS_TYPE] st1
	WHERE st1.STATUS_TYPE_ID IN (SELECT STATUS_TYPE_ID FROM @articles_with_wrong_statuses)

;WITH rel_betw_statuses AS (
	SELECT new_status_id, new_site_id, old_status_id FROM (
	SELECT st.SITE_ID AS new_site_id, st.STATUS_TYPE_ID AS new_status_id, stn.STATUS_TYPE_NAME, stn.STATUS_TYPE_ID AS old_status_id
		FROM [dbo].[STATUS_TYPE] st
		INNER JOIN @statuses_names stn ON st.STATUS_TYPE_NAME = stn.STATUS_TYPE_NAME
	) AS nsi
	WHERE nsi.NEW_SITE_ID IN (SELECT SITE_ID FROM @articles_with_wrong_statuses)
)

UPDATE CONTENT_ITEM
	SET STATUS_TYPE_ID = (
		SELECT NEW_STATUS_ID FROM CONTENT_ITEM AS ci
		INNER JOIN @articles_with_wrong_statuses AS t ON t.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
		INNER JOIN rel_betw_statuses AS rbs ON t.SITE_ID = rbs.new_site_id AND ci.STATUS_TYPE_ID = rbs.old_status_id
		WHERE  [dbo].[CONTENT_ITEM].CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
)
WHERE CONTENT_ITEM_ID IN (SELECT CONTENT_ITEM_ID FROM @articles_with_wrong_statuses)
END



DECLARE @workflow_rules_ids TABLE (
WORKFLOW_RULE_ID int
)

INSERT INTO @workflow_rules_ids
	SELECT workflow_rule_id FROM [dbo].[workflow_rules] wr
		WHERE SUCCESSOR_STATUS_ID NOT IN (
			SELECT STATUS_TYPE_ID FROM [dbo].[STATUS_TYPE] st
			INNER JOIN [dbo].[workflow] w ON st.SITE_ID = w.SITE_ID
			WHERE wr.WORKFLOW_ID = w.WORKFLOW_ID
	)

IF EXISTS (SELECT * FROM @workflow_rules_ids)
BEGIN
UPDATE [dbo].[WORKFLOW_RULES]
SET SUCCESSOR_STATUS_ID = (
	SELECT st2.STATUS_TYPE_ID
		FROM [dbo].[STATUS_TYPE] st1
			INNER JOIN [dbo].[STATUS_TYPE] st2 on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME
			INNER JOIN [dbo].[workflow] w on st2.SITE_ID = w.SITE_ID
		WHERE w.WORKFLOW_ID = [dbo].[workflow_rules].WORKFLOW_ID AND [dbo].[WORKFLOW_RULES].SUCCESSOR_STATUS_ID = st1.STATUS_TYPE_ID
	)
WHERE WORKFLOW_RULE_ID IN (SELECT WORKFLOW_RULE_ID FROM @workflow_rules_ids)
END

update VE_COMMAND set NAME = 'Spellchecker' where NAME = 'SpellCheck'
update VE_COMMAND set NAME = 'SpecialChar' where NAME = 'QSpecChar'
update VE_COMMAND set NAME = 'ShowBlocks', COMMAND_IN_GROUP_ORDER = 5 where NAME = 'LineBreak'
update VE_COMMAND set ALIAS = 'Show Blocks' where NAME = 'ShowBlocks'

GO


if not exists (select * From VE_COMMAND where name = 'autoFormat')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('autoFormat', 'Format Selection', 0, 3, 0, 0, 1, 1)

if not exists (select * From VE_COMMAND where name = 'CommentSelectedRange')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('CommentSelectedRange', 'Comment Selection', 0, 3, 0, 1, 1, 1)

if not exists (select * From VE_COMMAND where name = 'UncommentSelectedRange')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('UncommentSelectedRange', 'Uncomment Selection', 0, 3, 0, 2, 1, 1)

if not exists (select * From VE_COMMAND where name = 'AutoComplete')
  insert into VE_COMMAND (NAME, ALIAS, ROW_ORDER, TOOLBAR_IN_ROW_ORDER, GROUP_IN_TOOLBAR_ORDER, COMMAND_IN_GROUP_ORDER, [ON], LAST_MODIFIED_BY)
  values ('AutoComplete', 'Enable\Disable HTML Tag Autocomplete', 0, 3, 0, 3, 1, 1)

GO

exec qp_update_translations 'Show Blocks', 'Отображать блоки'
exec qp_update_translations 'Format Selection', 'Форматировать выбранное'
exec qp_update_translations 'Comment Selection', 'Комментировать выбранное'
exec qp_update_translations 'Uncomment Selection', 'Раскомментировать выбранное'
exec qp_update_translations 'Enable\Disable HTML Tag Autocomplete', 'Включить/Выключить автозавершение HTML-тегов'

GO

EXEC qp_update_translations 'Disable list auto wrapping (ul, ol, dl)', 'Отключить автоматическое оборачивание списков (ul, ol, dl)'
GO


update workflow set is_default = 1 where workflow_name = 'general' and not exists (select * from workflow where is_default = 1)
GO
if not exists(select * from sys.indexes where name = 'IX_O2M_DATA' and [object_id] = object_id('CONTENT_DATA'))
begin
    create index IX_O2M_DATA on CONTENT_DATA(O2M_DATA) WHERE O2M_DATA IS NOT NULL
end

if not exists(select * from sys.indexes where name = 'IX_O2M_DATA' and [object_id] = object_id('VERSION_CONTENT_DATA'))
begin
    create index IX_O2M_DATA on VERSION_CONTENT_DATA(O2M_DATA) WHERE O2M_DATA IS NOT NULL
end
go

exec qp_drop_existing 'RegionUpdates', 'IsUserTable'
GO

exec qp_drop_existing 'ClearAllProducts', 'IsProcedure'
GO

exec qp_drop_existing 'DeleteProduct', 'IsProcedure'
GO

exec qp_drop_existing 'RegionUpdated', 'IsProcedure'
GO


if not exists(select * from sys.tables where name = 'Products')
BEGIN
    CREATE TABLE [dbo].[Products]
    (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [DpcId] [int] NOT NULL,
        [Slug] [nvarchar](50) NULL,
        [Version] [int] NOT NULL CONSTRAINT [DF_Products_Version] DEFAULT (1),
        [IsLive] [bit] NOT NULL CONSTRAINT [DF_Products_IsLive] DEFAULT (1),
        [Language] [nvarchar](10) NULL,
        [Format] [nvarchar](10) NOT NULL,
        [Data] [nvarchar](max) NOT NULL,
        [Alias] [nvarchar](250) NULL,
        [Created] [datetime] NOT NULL,
        [Updated] [datetime] NOT NULL,
        [Hash] [nvarchar](2000) NOT NULL,
        [MarketingProductId] [int] NULL,
        [Title] [nvarchar](500) NULL,
        [UserUpdated] [nvarchar](50) NULL,
        [UserUpdatedId] [int] NULL,
        [ProductType] [nvarchar](250) NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

IF EXISTS (select index_id from sys.indexes where name = 'IX_Products' AND object_id = OBJECT_ID('dbo.Products'))
AND NOT EXISTS (select * from sys.index_columns where object_id = OBJECT_ID('IX_Products')
and index_id in (select index_id from sys.indexes where name = 'IX_Products')
and column_id in (select column_id from sys.columns where object_id = OBJECT_ID('dbo.Products') and name = 'Updated'))
BEGIN
    DROP INDEX [IX_Products] ON [dbo].[Products];
END
GO

IF NOT EXISTS (select index_id from sys.indexes where name = 'IX_Products' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Products] ON [dbo].[Products]
    (
        [DpcId] ASC,
        [Slug] ASC,
        [Version] ASC,
        [IsLive] ASC,
        [Language] ASC,
        [Format] ASC
    )
    INCLUDE
    (
    [Updated]
    );
END
GO

IF NOT EXISTS (SELECT index_id FROM sys.indexes WHERE name='IX_Updated' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Updated] ON [dbo].[Products]
    (
    [Updated] DESC
    )
    INCLUDE
    (
    [DpcId],
    [Slug],
    [IsLive],
    [Version],
    [Language],
    [Format]
    );
END
GO



if not exists(select * from sys.tables where name = 'ProductRegions')
BEGIN
    CREATE TABLE [dbo].[ProductRegions]
    (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [RegionId] [int] NOT NULL,
        CONSTRAINT [PK_ProductRegions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ProductRegions_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id])
    )
END
GO

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

CREATE PROCEDURE [dbo].[ClearAllProducts]
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    delete from  ProductRelevance
    delete from  ProductRegions
    delete from  Products
END
GO

CREATE PROCEDURE [dbo].[DeleteProduct]
    @id int
AS
BEGIN
    delete from ProductRegions with(rowlock) where ProductId = @id
    delete from Products with(rowlock) where Id = @id
END
GO

CREATE TABLE [dbo].[RegionUpdates](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Updated] [datetime] NOT NULL,
    [RegionId] [int] NULL,
 CONSTRAINT [PK_RegionUpdates] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE PROCEDURE [dbo].[RegionUpdated]
    -- Add the parameters for the stored procedure here
    @regionId int
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    declare @hasRecord int = 0

   set @hasRecord = (select COUNT(*) from RegionUpdates
    where RegionId = @regionId)

    if(@hasRecord > 0 )

    update RegionUpdates set Updated = getdate() where RegionId = @regionId

    else
     insert into RegionUpdates (RegionId, Updated) values (@regionId, getdate())

END
GO


exec qp_drop_existing  'GetProducts', 'IsTableFunction'
exec qp_drop_existing  'SyncProductVersions', 'IsProcedure'
GO
if not exists(select * from sys.tables where name = 'ProductVersions')
BEGIN
	CREATE TABLE [dbo].[ProductVersions](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Deleted] [bit] NOT NULL,
		[Modification] [datetime] NOT NULL,
		[DpcId] [int] NOT NULL,
		[Slug] [nvarchar](50) NULL,
		[Version] [int] NOT NULL,
		[IsLive] [bit] NOT NULL,
		[Language] [nvarchar](10) NULL,
		[Format] [nvarchar](10) NOT NULL,
		[Data] [nvarchar](max) NOT NULL,
		[Alias] [nvarchar](250) NULL,
		[Created] [datetime] NOT NULL,
		[Updated] [datetime] NOT NULL,
		[Hash] [nvarchar](2000) NOT NULL,
		[MarketingProductId] [int] NULL,
		[Title] [nvarchar](500) NULL,
		[UserUpdated] [nvarchar](50) NULL,
		[UserUpdatedId] [int] NULL,
		[ProductType] [nvarchar](250) NULL
		CONSTRAINT [PK_ProductVersions] PRIMARY KEY CLUSTERED ([Id] ASC )	
	)
END
GO

if not exists (select * from sys.indexes where name = 'IX_ProductVersions')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_ProductVersions] ON [dbo].[ProductVersions]
	(
		[DpcId] ASC,
		[IsLive] ASC,
		[Language] ASC,
		[Format] ASC,
		[Modification] ASC
	)
END
GO
if not exists(select * from sys.tables where name = 'ProductRegionVersions')
BEGIN
	CREATE TABLE [dbo].[ProductRegionVersions](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ProductVersionId] [int] NOT NULL,
		[RegionId] [int] NOT NULL
		CONSTRAINT [PK_ProductRegionVersions] PRIMARY KEY CLUSTERED ([Id] ASC)
		CONSTRAINT [FK_ProductRegionVersions_ProductVersions] FOREIGN KEY([ProductVersionId]) REFERENCES [dbo].[ProductVersions] ([Id])
	)
END
GO
CREATE FUNCTION GetProducts(@date datetime)
RETURNS TABLE
AS
RETURN
(
	select *
	from
		ProductVersions v with (nolock)
	where
		not exists (
			select null
			from ProductVersions v2 with (nolock)
			where
				v2.[Id] > v.[Id]
				and v2.[DpcId] = v.[DpcId]
				and v2.[IsLive] = v.[IsLive]
				and v2.[Language] = v.[Language]
				and v2.[Format] = v.[Format]
				and v2.[Modification] <= @date)
		and v.[Deleted] = 0
		and v.[Modification] <= @date
)
GO
CREATE PROCEDURE SyncProductVersions
AS
BEGIN
	print('Start update ProductVersions...')

	WHILE exists(
		select null
		from Products p
		where not exists (
			select null
			from ProductVersions v with (nolock)
			where
				p.[Updated] = v.[Modification] and
				p.[DpcId] = v.[DpcId] and
				p.[IsLive] = v.[IsLive] and
				p.[Language] = v.[Language]
				and p.[Format] = v.[Format]
			)
	)
	BEGIN
		insert into ProductVersions(Deleted, Modification, DpcId, Version, IsLive, Language, Format, Data, Alias, Created, Updated, Hash, MarketingProductId, Title, UserUpdated, UserUpdatedId, ProductType)
		select top 1000
			0 Deleted,
			p.Updated Modification,
			p.DpcId,
			p.Version,
			p.IsLive,
			p.Language,
			p.Format,
			p.Data,
			p.Alias,
			p.Created,
			p.Updated,
			p.Hash,
			p.MarketingProductId,
			p.Title,
			p.UserUpdated,
			p.UserUpdatedId,
			p.ProductType
		from Products p
		where not exists (
			select null
			from ProductVersions v with (nolock)
			where
				p.[Updated] = v.[Modification] and
				p.[DpcId] = v.[DpcId] and
				p.[IsLive] = v.[IsLive] and
				p.[Language] = v.[Language]
				and p.[Format] = v.[Format]
			)	
	END

	print('End update ProductVersions')
END
GO
