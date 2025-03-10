if not exists (select * from ACTION_TOOLBAR_BUTTON where parent_action_id = dbo.qp_action_id('list_archive_article') and name = 'Export')
    insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], icon)
    values (dbo.qp_action_id('list_archive_article'), dbo.qp_action_id('multiple_export_archive_article'), 'Export', 15, 'other/export.gif')

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('view_live_article'))
	insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, [ORDER])
	values (dbo.qp_action_id('view_live_article'), dbo.qp_action_id('refresh_article'), 'Refresh', 'refresh.gif', 10)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('compare_article_live_with_current'))
	insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, [ORDER])
	values (dbo.qp_action_id('compare_article_live_with_current'), dbo.qp_action_id('refresh_article'), 'Refresh', 'refresh.gif', 10)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('edit_plugin') and NAME = 'Save')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('edit_plugin'), dbo.qp_action_id('update_plugin'), 'Save', 'save.gif', NULL, 1, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('edit_plugin') and NAME = 'Remove')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('edit_plugin'), dbo.qp_action_id('remove_plugin'), 'Remove', 'delete.gif', NULL, 2, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('edit_plugin') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('edit_plugin'), dbo.qp_action_id('refresh_plugin'), 'Refresh', 'refresh.gif', NULL, 3, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('new_plugin') and NAME = 'Save')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('new_plugin'), dbo.qp_action_id('save_plugin'), 'Save', 'save.gif', NULL, 1, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('new_plugin') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('new_plugin'), dbo.qp_action_id('refresh_plugin'), 'Refresh', 'refresh.gif', NULL, 2, 1)


if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin') and NAME = 'Properties')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('list_plugin'), dbo.qp_action_id('edit_plugin'), 'Properties', 'properties.gif', NULL, 1, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin') and NAME = 'Remove')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('list_plugin'), dbo.qp_action_id('remove_plugin'), 'Remove', 'delete.gif', NULL, 2, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
    values (dbo.qp_action_id('list_plugin'), dbo.qp_action_id('refresh_plugin'), 'Refresh', 'refresh.gif', NULL, 3, 0)

if not exists (select * from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('scheduled_tasks') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, ICON, [ORDER])
    values (dbo.qp_action_id('scheduled_tasks'), dbo.qp_action_id('refresh_scheduled_tasks'), 'Refresh', 'refresh.gif', 100)
GO

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin') and NAME = 'Versions')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_plugin'), dbo.qp_action_id('list_plugin_version'), 'Versions', 'version.gif', NULL, 2)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin_version') and NAME = 'Preview')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_plugin_version'), dbo.qp_action_id('preview_plugin_version'), 'Preview', 'properties.gif', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin_version') and NAME = 'Compare Versions')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_plugin_version'), dbo.qp_action_id('compare_plugin_versions'), 'Compare Versions', 'compare.gif', NULL, 2)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin_version') and NAME = 'Compare with current')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_plugin_version'), dbo.qp_action_id('compare_plugin_version_with_current'), 'Compare with current', 'compare.gif', NULL, 3)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_plugin_version') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_plugin_version'), dbo.qp_action_id('refresh_plugin_versions'), 'Refresh', 'refresh.gif', NULL, 4)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('preview_plugin_version') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('preview_plugin_version'), dbo.qp_action_id('refresh_plugin_version'), 'Refresh', 'refresh.gif', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('compare_plugin_versions') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('compare_plugin_versions'), dbo.qp_action_id('refresh_plugin_versions'), 'Refresh', 'refresh.gif', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('compare_plugin_version_with_current') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('compare_plugin_version_with_current'), dbo.qp_action_id('refresh_plugin_version'), 'Refresh', 'refresh.gif', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('get_article_external_workflow_task') and NAME = 'Complete')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('get_article_external_workflow_task'), dbo.qp_action_id('complete_article_external_workflow_task'), 'Complete', 'check.png', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_article_external_workflow_tasks') and NAME = 'Refresh')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_article_external_workflow_tasks'), dbo.qp_action_id('refresh_article_external_workflow_tasks'), 'Refresh', 'refresh.gif', NULL, 1)

if not exists (select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_article_external_workflow_tasks') and NAME = 'Complete')
    insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER])
    values (dbo.qp_action_id('list_article_external_workflow_tasks'), dbo.qp_action_id('get_article_external_workflow_task'), 'Complete', 'check.png', NULL, 2)
