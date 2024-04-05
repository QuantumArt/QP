if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'LAST_UPDATE' and TABLE_NAME = 'SESSIONS_LOG')
	alter table [SESSIONS_LOG]
	add [LAST_UPDATE] datetime NULL