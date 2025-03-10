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

if not exists (select * From BACKEND_ACTION where code = 'list_plugin_version')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE, DEFAULT_VIEW_TYPE_ID)
    values('QP Plugin versions', 'list_plugin_version', dbo.qp_action_type_id('list'), dbo.qp_entity_type_id('plugin_version'), '~/QpPluginVersion/Index/',  1, dbo.qp_view_id('list'))

if not exists (select * from BACKEND_ACTION where code = 'preview_plugin_version')
	INSERT INTO BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES('Preview QP Plugin version', 'preview_plugin_version', dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('plugin_version'), '~/QpPluginVersion/Properties/', 1)

if not exists (select * from BACKEND_ACTION where code = 'compare_plugin_version_with_current')
	INSERT INTO BACKEND_ACTION (NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES('Compare QP Plugin version with Current', 'compare_plugin_version_with_current', dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('plugin_version'), '~/QpPluginVersion/CompareWithCurrent/', 1)

if not exists (select * from BACKEND_ACTION where code = 'compare_plugin_versions')
	INSERT INTO BACKEND_ACTION (NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES('Compare QP Plugin versions', 'compare_plugin_versions', dbo.qp_action_type_id('compare'), dbo.qp_entity_type_id('plugin_version'), '~/QpPluginVersion/Compare/', 1)

if not exists (select * from BACKEND_ACTION where code = 'refresh_plugin_version')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
    values('Refresh Plugin Version', 'refresh_plugin_version', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('plugin_version'))

if not exists (select * from BACKEND_ACTION where code = 'refresh_plugin_versions')
    insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
    values('Refresh Plugin Versions', 'refresh_plugin_versions', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('plugin_version'))

update BACKEND_ACTION set SHORT_NAME = 'Compare versions' where NAME = 'Compare QP Plugin versions'
update BACKEND_ACTION set SHORT_NAME = 'Compare with Current' where NAME = 'Compare QP Plugin version with Current'
update BACKEND_ACTION set SHORT_NAME = 'Preview' where NAME = 'Preview QP Plugin version'
update BACKEND_ACTION set SHORT_NAME = 'Versions' where NAME = 'QP Plugin versions'

if not exists(select * from BACKEND_ACTION where code = 'auto_resize_site_file')
    insert into [BACKEND_ACTION] (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
    values (dbo.qp_action_type_id('auto_resize'), dbo.qp_entity_type_id('site_file'), 'Auto Resize Site File', 'auto_resize_site_file')

if not exists(select * from BACKEND_ACTION where code = 'auto_resize_content_file')
    insert into [BACKEND_ACTION] (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
    values (dbo.qp_action_type_id('auto_resize'), dbo.qp_entity_type_id('content_file'), 'Auto Resize Content File', 'auto_resize_content_file')

if not exists (select * from BACKEND_ACTION where code = 'multiple_update_article')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
    VALUES(dbo.qp_action_type_id('multiple_update'), dbo.qp_entity_type_id('article'), 'Multiple Update Articles', 'multiple_update_article')

if not exists (select * from BACKEND_ACTION where code = 'multiple_save_article')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
    VALUES(dbo.qp_action_type_id('multiple_save'), dbo.qp_entity_type_id('article'), 'Multiple Save Articles', 'multiple_save_article')

if not exists (select * from BACKEND_ACTION where code = 'list_article_external_workflow_tasks')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, SHORT_NAME, CONTROLLER_ACTION_URL, IS_INTERFACE)
    VALUES(dbo.qp_action_type_id('list'), dbo.qp_entity_type_id('db'), 'External Workflow User Tasks', 'list_article_external_workflow_tasks', 'User Tasks', '~/Home/ExternalWorkflowUserTasks/', 1)

if not exists (select * from BACKEND_ACTION where code = 'refresh_article_external_workflow_tasks')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, SHORT_NAME)
    VALUES(dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('db'), 'Refresh External Workflow User Tasks', 'refresh_article_external_workflow_tasks', 'Refresh User Tasks')

if not exists (select * from BACKEND_ACTION where code = 'complete_article_external_workflow_task')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, SHORT_NAME, CONTROLLER_ACTION_URL)
    VALUES(dbo.qp_action_type_id('complete'), dbo.qp_entity_type_id('article_external_workflow'), 'Complete External Workflow User Task', 'complete_article_external_workflow_task', 'Complete User Task', '~/ExternalWorkflowUserActions/CompleteUserTask/')

if not exists (select * from BACKEND_ACTION where code = 'get_article_external_workflow_task')
    INSERT INTO BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, SHORT_NAME, CONTROLLER_ACTION_URL, IS_INTERFACE, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT)
    VALUES(dbo.qp_action_type_id('complete'), dbo.qp_entity_type_id('article_external_workflow'), 'Get External Workflow User Task', 'get_article_external_workflow_task', 'Get User Task', '~/Home/ExternalWorkflowUserTasks/', 1, 1, 800, 600)

GO
