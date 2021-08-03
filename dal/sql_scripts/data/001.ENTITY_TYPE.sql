if not exists(select * from ENTITY_TYPE where CODE = 'plugin')
    insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, [ORDER], SOURCE, ID_FIELD, TITLE_FIELD, ORDER_FIELD)
    values ('QP Plugin', 'plugin', dbo.qp_entity_type_id('db'), 7, 'PLUGIN', 'ID', 'NAME', '[ORDER]')
GO

--delete from ENTITY_TYPE where code = 'plugin'

if not exists(select * from ENTITY_TYPE where CODE = 'plugin_version')
    insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, SOURCE, ID_FIELD, PARENT_ID_FIELD, DISABLED)
    values ('QP Plugin Version', 'plugin_version', dbo.qp_entity_type_id('plugin'), 'PLUGIN_VERSION', 'ID', 'PARENT_ID', 1)
GO

--select * from ENTITY_TYPE