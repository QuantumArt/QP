IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USER_IP' and TABLE_NAME = 'BACKEND_ACTION_LOG')
    ALTER TABLE BACKEND_ACTION_LOG
    ADD USER_IP VARCHAR(50)
