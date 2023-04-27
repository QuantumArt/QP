if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'TEMPLATE_ID' and TABLE_NAME = 'NOTIFICATIONS')
    alter table [NOTIFICATIONS]
    add [TEMPLATE_ID] numeric(18,0) NULL
