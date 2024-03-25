insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
select (select id from context_menu where code = 'article'), (select id from backend_action where code = 'view_live_article'),
       'Live Properties', 52, 'properties.gif'
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
select (select id from context_menu where code = 'article'), (select id from backend_action where code = 'compare_article_live_with_current'),
       'Compare Live version with Current', 53, 'compare.gif'
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order",  icon)
values ((select id from context_menu where code = 'virtual_article'), (select id from backend_action where code = 'unselect_child_articles'),
        'Unselect Child Articles', 90, 'deselect_all.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values ((select id from context_menu where code = 'virtual_article'), (select id from backend_action where code = 'select_child_articles'),
        'Select Child Articles', 80, 'select_all.gif')
on conflict do nothing;

insert into context_menu_item (context_menu_id, action_id, name, "order", icon, bottom_separator)
values ((select id from context_menu where code = 'custom_action'), (select id from backend_action where code = 'copy_custom_action'),
        'Create Like', 5, 'create_like.gif', true)
on conflict do nothing;

insert into context_menu_item (context_menu_id, action_id, name, "order", icon, bottom_separator)
values ((select id from context_menu where code = 'article'), (select id from backend_action where code = 'select_child_articles'),
        'Select child articles', 80, 'select_all.gif', false)
on conflict do nothing;

insert into context_menu_item (context_menu_id, action_id, name, "order", icon, bottom_separator)
values ((select id from context_menu where code = 'article'), (select id from backend_action where code = 'unselect_child_articles'),
        'Unselect child articles', 90, 'deselect_all.gif', false)
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon, bottom_separator)
values((select id from context_menu where code = 'plugins'), (select id from backend_action where code = 'refresh_plugins'),
       'Refresh', 1, 'refresh.gif', true)
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'plugins'), (select id from backend_action where code = 'new_plugin'),
       'New QP Plugin', 2, 'add.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon, bottom_separator)
values((select id from context_menu where code = 'plugin'), (select id from backend_action where code = 'remove_plugin'),
       'Remove', 2, 'delete.gif', true)
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'plugin'), (select id from backend_action where code = 'edit_plugin'),
       'Properties', 3, 'properties.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order")
values((select id from context_menu where code = 'db'), (select id from backend_action where code = 'scheduled_tasks'),
       'Scheduled Tasks', 90)
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'plugin'), (select id from backend_action where code = 'list_plugin_version'),
       'Versions', 4, 'version.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'plugin_version'), (select id from backend_action where code = 'preview_plugin_version'),
       'Preview', 1, 'properties.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'content_file'), (select id from backend_action where code = 'auto_resize_content_file'),
       'Auto Resize', 51, 'crop.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
values((select id from context_menu where code = 'site_file'), (select id from backend_action where code = 'auto_resize_site_file'),
       'Auto Resize', 51, 'crop.gif')
on conflict do nothing;

insert into context_menu_item(context_menu_id, action_id, name, "order")
values((select id from context_menu where code = 'db'), (select id from backend_action where code = 'list_article_external_workflow_tasks'),
       'External Workflow User Tasks', 65)
on conflict do nothing;
