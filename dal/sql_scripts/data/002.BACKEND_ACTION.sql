if not exists (select * From BACKEND_ACTION where code = 'copy_custom_action')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_INTERFACE)
    VALUES (dbo.qp_action_type_id('copy'), dbo.qp_entity_type_id('custom_action'), N'Create Like Custom Action', 'Create Like', N'copy_custom_action', '~/CustomAction/Copy/', 0)

if not exists (select * from BACKEND_ACTION where code = 'export_archive_article')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
    VALUES(dbo.qp_action_type_id('export'), dbo.qp_entity_type_id('archive_article'), 'Export Archive Articles', 'export_archive_article', '~/ExportArchiveArticles/', 1, 600, 400, 1, 1)

if not exists (select * from BACKEND_ACTION where code = 'multiple_export_archive_article')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
    VALUES(dbo.qp_action_type_id('multiple_export'), dbo.qp_entity_type_id('archive_article'), 'Multiple Export Archive Articles', 'multiple_export_archive_article', '~/ExportSelectedArchiveArticles/', 1, 600, 400, 1, 1)

if not exists (select * from BACKEND_ACTION where code = 'view_live_article')
	INSERT INTO BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES(dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('article'), 'Article Live Properties', 'Live Properties', 'view_live_article', '~/Article/LiveProperties/', 1)

if not exists (select * from BACKEND_ACTION where code = 'compare_article_live_with_current')
	INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES(dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('article'), 'Compare Article Live version with Current', 'Compare Live version with Current', 'compare_article_live_with_current', '~/ArticleVersion/CompareLiveWithCurrent/', 1)

if not exists (select * From BACKEND_ACTION where code = 'select_child_articles')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, IS_INTERFACE)
    VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Select Child Articles', 'Select Child Articles', N'select_child_articles', 0)

if not exists (select * From BACKEND_ACTION where code = 'unselect_child_articles')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, IS_INTERFACE)
    VALUES (dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('article'), N'Unselect Child Articles', 'Unselect Child Articles', N'unselect_child_articles', 0)

if not exists (select * From BACKEND_ACTION where code = 'list_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE, DEFAULT_VIEW_TYPE_ID)
    values('QP Plugins', 'list_plugin', dbo.qp_action_type_id('list'), dbo.qp_entity_type_id('plugin'), '~/QpPlugin/Index/',  1, dbo.qp_view_id('list'))

if not exists (select * From BACKEND_ACTION where code = 'new_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
    values('New QP Plugin', 'new_plugin', dbo.qp_action_type_id('new'), dbo.qp_entity_type_id('plugin'), '~/QpPlugin/New/', 1)

if not exists (select * From BACKEND_ACTION where code = 'edit_plugin')
    insert into BACKEND_ACTION(NAME, SHORT_NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
    values('QP Plugin Properties', 'Properties', 'edit_plugin', dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('plugin'), '~/QpPlugin/Properties/', 1)

if not exists (select * From BACKEND_ACTION where code = 'update_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE)
    values('Update QP Plugin', 'update_plugin', dbo.qp_action_type_id('update'), dbo.qp_entity_type_id('plugin'),0)

if not exists (select * From BACKEND_ACTION where code = 'refresh_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
    values('Refresh QP Plugin', 'refresh_plugin', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('plugin'))

if not exists (select * From BACKEND_ACTION where code = 'remove_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONFIRM_PHRASE, CONTROLLER_ACTION_URL, HAS_PRE_ACTION)
    values('Remove QP Plugin', 'remove_plugin', dbo.qp_action_type_id('remove'), dbo.qp_entity_type_id('plugin'), 'Do you really want to remove this QP plugin?', '~/QpPlugin/Remove/', 0)

if not exists (select * From BACKEND_ACTION where code = 'save_plugin')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE)
    values('Save QP Plugin', 'save_plugin', dbo.qp_action_type_id('save'), dbo.qp_entity_type_id('plugin'), 0)

if not exists (select * From BACKEND_ACTION where code = 'refresh_plugins')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE)
    values('Refresh QP Plugins', 'refresh_plugins', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('plugin'), 1)

if not exists (select * from BACKEND_ACTION where code = 'scheduled_tasks')
    insert into BACKEND_ACTION (NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
    values('Scheduled Tasks', 'scheduled_tasks', dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('db'),  '~/Home/ScheduledTasks/', 1)

if not exists (select * from BACKEND_ACTION where code = 'refresh_scheduled_tasks')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
    values('Refresh Scheduled Tasks', 'refresh_scheduled_tasks', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('db'))
GO