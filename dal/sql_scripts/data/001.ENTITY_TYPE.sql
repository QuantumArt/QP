if not exists(select * from ENTITY_TYPE where CODE = 'plugin')
    insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, [ORDER], SOURCE, ID_FIELD, TITLE_FIELD, ORDER_FIELD)
    values ('QP Plugin', 'plugin', dbo.qp_entity_type_id('db'), 7, 'PLUGIN', 'ID', 'NAME', '[ORDER]')
GO

--delete from ENTITY_TYPE where code = 'plugin'

if not exists(select * from ENTITY_TYPE where CODE = 'plugin_version')
    insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, SOURCE, ID_FIELD, PARENT_ID_FIELD, TITLE_FIELD, DISABLED)
    values ('QP Plugin Version', 'plugin_version', dbo.qp_entity_type_id('plugin'), 'PLUGIN_VERSION', 'ID', 'PLUGIN_ID', 'ID', 1)
GO


update entity_type set TITLE_FIELD = 'ID', PARENT_ID_FIELD = 'PLUGIN_ID' where code = 'plugin_version'

--select * from ENTITY_TYPE

if not exists(select * from ENTITY_TYPE where CODE = 'article_external_workflow')
    insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, SOURCE, ID_FIELD, PARENT_ID_FIELD, TITLE_FIELD, DISABLED)
    values ('Article External Workflow', 'article_external_workflow', dbo.qp_entity_type_id('article'), 'EXTERNAL_WORKFLOW', 'ID', 'ARTICLE_ID', 'WORKFLOW_NAME', 1)
GO
