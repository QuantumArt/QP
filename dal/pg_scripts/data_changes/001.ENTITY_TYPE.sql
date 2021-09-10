insert into entity_type(name, code, parent_id, "order", source, id_field, title_field, order_field)
values ('QP Plugin', 'plugin', (select id from entity_type where code = 'db'), 7, 'PLUGIN', 'ID', 'NAME', '[ORDER]')
on conflict do nothing;

insert into entity_type(name, code, parent_id, source, id_field, parent_id_field, title_field, disabled)
values ('QP Plugin Version', 'plugin_version', (select id from entity_type where code = 'plugin'), 'PLUGIN_VERSION', 'ID', 'PLUGIN_ID', 'ID', true)
on conflict do nothing;

update entity_type set TITLE_FIELD = 'ID', PARENT_ID_FIELD = 'PLUGIN_ID' where code = 'plugin_version';

--select * from ENTITY_TYPE