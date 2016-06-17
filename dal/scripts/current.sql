IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'OPTIMIZE_FOR_HIERARCHY') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD OPTIMIZE_FOR_HIERARCHY BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_OPTIMIZE_FOR_HIERARCHY DEFAULT 0  
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'IS_LOCALIZATION') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD IS_LOCALIZATION BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_IS_LOCALIZATION DEFAULT 0  
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_SEPARATE_REVERSE_VIEWS') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD USE_SEPARATE_REVERSE_VIEWS BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_USE_SEPARATE_REVERSE_VIEWS DEFAULT 0  
END



GO

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

GO

