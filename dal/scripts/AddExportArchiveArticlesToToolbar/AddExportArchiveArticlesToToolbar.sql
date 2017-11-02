if not exists (select * from BACKEND_ACTION where code = 'export_archive_article')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, code, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
VALUES(dbo.qp_action_type_id('multiple_export'), dbo.qp_entity_type_id('archive_article'), 'Export Archive Articles', 'export_archive_article', '~/ExportSelectedArticles/', 1, 600, 400, 1, 1)

if not exists (select * from ACTION_TOOLBAR_BUTTON where parent_action_id = dbo.qp_action_id('list_archive_article') and name = 'Export')
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], icon)
values (dbo.qp_action_id('list_archive_article'), dbo.qp_action_id('export_archive_article'), 'Export', 15, 'other/export.gif')
