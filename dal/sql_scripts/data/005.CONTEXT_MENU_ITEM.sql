if not exists (SELECT * FROM CONTEXT_MENU_ITEM WHERE NAME = 'Unselect Child Articles' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_article'))
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER],  ICON)
    values (dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('unselect_child_articles'), 'Unselect Child Articles', 90, 'deselect_all.gif')

if not exists (SELECT * FROM  CONTEXT_MENU_ITEM WHERE NAME = 'Select Child Articles' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_article'))
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values (dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('select_child_articles'), 'Select Child Articles', 80, 'select_all.gif')

if not exists (SELECT * FROM  CONTEXT_MENU_ITEM WHERE NAME = 'Create Like' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('custom_action'))
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
    values (dbo.qp_context_menu_id('custom_action'), dbo.qp_action_id('copy_custom_action'), 'Create Like', 5, 'create_like.gif', 1)

if not exists (select * from CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Live Properties')
	insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
	values (dbo.qp_context_menu_id('article'), dbo.qp_action_id('view_live_article'), 'Live Properties', 52, 'properties.gif')

if not exists (select * from CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Compare Live version with Current')
	insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
	values (dbo.qp_context_menu_id('article'), dbo.qp_action_id('compare_article_live_with_current'), 'Compare Live version with Current', 53, 'compare.gif')

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Select Child Articles')
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
    values (dbo.qp_context_menu_id('article'), dbo.qp_action_id('select_child_articles'), 'Select Child Articles', 80, 'select_all.gif', 0)

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Unselect Child Articles')
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
    values (dbo.qp_context_menu_id('article'), dbo.qp_action_id('unselect_child_articles'), 'Unselect Child Articles', 90, 'deselect_all.gif', 0)

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugins') and name = 'Refresh')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
    values(dbo.qp_context_menu_id('plugins'), dbo.qp_action_id('refresh_plugins'), 'Refresh', 1, 'refresh.gif', 1)

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugins') and name = 'New QP Plugin')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('plugins'), dbo.qp_action_id('new_plugin'), 'New QP Plugin', 2, 'add.gif')

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugin') and name = 'Remove')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
    values(dbo.qp_context_menu_id('plugin'), dbo.qp_action_id('remove_plugin'), 'Remove', 2, 'delete.gif', 1)

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugin') and name = 'Properties')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('plugin'), dbo.qp_action_id('edit_plugin'), 'Properties', 3, 'properties.gif')

if not exists (select * From CONTEXT_MENU_ITEM WHERE NAME = 'Scheduled Tasks' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('db'))
    INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER])
    VALUES(dbo.qp_context_menu_id('db'), dbo.qp_action_id('scheduled_tasks'), 'Scheduled Tasks', 90)

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugin') and name = 'Versions')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('plugin'), dbo.qp_action_id('list_plugin_version'), 'Versions', 4, 'version.gif')

if not exists (select * From CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugin_version') and name = 'Preview')
    insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('plugin_version'), dbo.qp_action_id('preview_plugin_version'), 'Preview', 1, 'properties.gif')

if not exists(select * from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('auto_resize_content_file'))
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('content_file'), dbo.qp_action_id('auto_resize_content_file'), 'Auto Resize', 51, 'crop.gif')

if not exists(select * from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('auto_resize_site_file'))
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
    values(dbo.qp_context_menu_id('site_file'), dbo.qp_action_id('auto_resize_site_file'), 'Auto Resize', 51, 'crop.gif')

if not exists(select * from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('list_article_external_workflow_tasks'))
    insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER])
    values(dbo.qp_context_menu_id('db'), dbo.qp_action_id('list_article_external_workflow_tasks'), 'External Workflow User Tasks', 65)
