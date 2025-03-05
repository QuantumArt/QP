insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'view_live_article'), (select id from backend_action where code = 'refresh_article'),
       'Refresh', 'refresh.gif', 10
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'compare_article_live_with_current'), (select id from backend_action where code = 'refresh_article'),
       'Refresh', 'refresh.gif', 10
on conflict do nothing;

insert into action_toolbar_button (parent_action_id, action_id, name, "order", icon)
values ((select id from backend_action where code = 'list_archive_article'), (select id from backend_action where code = 'multiple_export_archive_article'),
        'Export', 15, 'other/export.gif')
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
values ((select id from backend_action where code = 'view_live_article'), (select id from backend_action where code = 'refresh_article'),
        'Refresh', 'refresh.gif', 10)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
values ((select id from backend_action where code = 'compare_article_live_with_current'), (select id from backend_action where code = 'refresh_article'),
        'Refresh', 'refresh.gif', 10)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'edit_plugin'), (select id from backend_action where code = 'update_plugin'),
        'Save', 'save.gif', NULL, 1, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'edit_plugin'), (select id from backend_action where code = 'remove_plugin'),
        'Remove', 'delete.gif', NULL, 2, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'edit_plugin'), (select id from backend_action where code = 'refresh_plugin'),
        'Refresh', 'refresh.gif', NULL, 3, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'new_plugin'), (select id from backend_action where code = 'save_plugin'),
        'Save', 'save.gif', NULL, 1, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'new_plugin'), (select id from backend_action where code = 'refresh_plugin'),
        'Refresh', 'refresh.gif', NULL, 2, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'list_plugin'), (select id from backend_action where code = 'edit_plugin'),
        'Properties', 'properties.gif', NULL, 1, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'list_plugin'), (select id from backend_action where code = 'remove_plugin'),
        'Remove', 'delete.gif', NULL, 2, true)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order", is_command)
values ((select id from backend_action where code = 'list_plugin'), (select id from backend_action where code = 'refresh_plugin'),
        'Refresh', 'refresh.gif', NULL, 3, false)
on conflict do nothing;

insert into action_toolbar_button (parent_action_id, action_id, icon, "order", name)
values ((select id from backend_action where code = 'scheduled_tasks'), (select id from backend_action where code = 'refresh_scheduled_tasks'),
        'refresh.gif', 100, 'Refresh')
on conflict do nothing;


insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_plugin'), (select id from backend_action where code = 'list_plugin_version'),
        'Versions', 'version.gif', NULL, 2)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_plugin_version'), (select id from backend_action where code = 'preview_plugin_version'),
        'Preview', 'properties.gif', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_plugin_version'), (select id from backend_action where code = 'compare_plugin_versions'),
        'Compare Versions', 'compare.gif', NULL, 2)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_plugin_version'), (select id from backend_action where code = 'compare_plugin_version_with_current'),
        'Compare with current', 'compare.gif', NULL, 3)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_plugin_version'), (select id from backend_action where code = 'refresh_plugin_versions'),
        'Refresh', 'refresh.gif', NULL, 4)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'preview_plugin_version'), (select id from backend_action where code = 'refresh_plugin_version'),
        'Refresh', 'refresh.gif', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'compare_plugin_versions'), (select id from backend_action where code = 'refresh_plugin_versions'),
        'Refresh', 'refresh.gif', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'compare_plugin_version_with_current'), (select id from backend_action where code = 'refresh_plugin_version'),
        'Refresh', 'refresh.gif', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'get_article_external_workflow_task'), (select id from backend_action where code = 'complete_article_external_workflow_task'),
        'Complete', 'check.png', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_article_external_workflow_tasks'), (select id from backend_action where code = 'refresh_article_external_workflow_tasks'),
        'Refresh', 'refresh.gif', NULL, 1)
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, icon_disabled, "order")
values ((select id from backend_action where code = 'list_article_external_workflow_tasks'), (select id from backend_action where code = 'get_article_external_workflow_task'),
        'Complete', 'check.png', NULL, 2)
on conflict do nothing;

