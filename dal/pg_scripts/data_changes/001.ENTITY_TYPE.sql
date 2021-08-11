insert into entity_type(name, code, parent_id, "order", source, id_field, title_field, order_field)
values ('QP Plugin', 'plugin', (select id from entity_type where code = 'db'), 7, 'PLUGIN', 'ID', 'NAME', '[ORDER]')
on conflict do nothing;

insert into entity_type(name, code, parent_id, source, id_field, parent_id_field, disabled)
values ('QP Plugin Version', 'plugin_version', (select id from entity_type where code = 'plugin'), 'PLUGIN_VERSION', 'ID', 'PARENT_ID', true)
on conflict do nothing;


--select * from ENTITY_TYPE