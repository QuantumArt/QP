SELECT setval('backend_action_seq', cast(COALESCE((SELECT MAX(id)+1 FROM backend_action), 1) as int), false) into tmp_val_tbl;
drop table tmp_val_tbl;

insert into backend_action(type_id, entity_type_id, name, short_name, code, controller_action_url, is_interface)
select (select id from action_type where code = 'read'), (select id from entity_type where code = 'article'),
       'Article Live Properties', 'Live Properties', 'view_live_article', '~/Article/LiveProperties/', true
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'view_live_article'), (select id from backend_action where code = 'refresh_article'),
       'Refresh', 'refresh.gif', 10
on conflict do nothing;

insert into backend_action(type_id, entity_type_id, name, short_name, code, controller_action_url, is_interface)
select (select id from action_type where code = 'read'), (select id from entity_type where code = 'article'),
       'Compare Article Live version with Current', 'Compare Live version with Current', 'compare_article_live_with_current', '~/ArticleVersion/CompareLiveWithCurrent/', true
on conflict do nothing;

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'compare_article_live_with_current'), (select id from backend_action where code = 'refresh_article'),
       'Refresh', 'refresh.gif', 10
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, short_name, code, controller_action_url, is_interface)
values ((select id from action_type where code = 'copy'), (select id from entity_type where code = 'custom_action'),
        'Create Like Custom Action', 'Create Like', N'copy_custom_action', '~/CustomAction/Copy/', false)
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, code, controller_action_url, is_window, window_width, window_height, is_multistep, has_settings)
values ((select id from action_type where code = 'export'), (select id from entity_type where code = 'archive_article'),
       'Export Archive Articles', 'export_archive_article', '~/ExportArchiveArticles/', true, 600, 400, true, true)
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, code, controller_action_url, is_window, window_width, window_height, is_multistep, has_settings)
values ((select id from action_type where code = 'multiple_export'), (select id from entity_type where code = 'archive_article'),
       'Multiple Export Archive Articles', 'multiple_export_archive_article', '~/ExportSelectedArchiveArticles/', true, 600, 400, true, true)
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, short_name, code, is_interface)
values ((select id from action_type where code = 'select'), (select id from entity_type where code = 'article'),
        'Select Child Articles', 'Select Child Articles', 'select_child_articles', false)
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, short_name, code, is_interface)
values ((select id from action_type where code = 'select'), (select id from entity_type where code = 'article'),
        'Unselect Child Articles', 'Unselect Child Articles', 'unselect_child_articles', false)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, controller_action_url, is_interface, default_view_type_id)
values('QP Plugins', 'list_plugin', (select id from action_type where code = 'list'), (select id from entity_type where code = 'plugin'),
       '~/QpPlugin/Index/',  true, (select id from view_type where code = 'list'))
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('New QP Plugin', 'new_plugin', (select id from action_type where code = 'new'), (select id from entity_type where code = 'plugin'),
       '~/QpPlugin/New/', true)
on conflict do nothing;

insert into backend_action(name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('QP Plugin Properties', 'Properties', 'edit_plugin', (select id from action_type where code = 'read'), (select id from entity_type where code = 'plugin'),
       '~/QpPlugin/Properties/', true)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, is_interface)
values('Update QP Plugin', 'update_plugin', (select id from action_type where code = 'update'), (select id from entity_type where code = 'plugin'), false)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id)
values('Refresh QP Plugin', 'refresh_plugin', (select id from action_type where code = 'refresh'), (select id from entity_type where code = 'plugin'))
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, confirm_phrase, controller_action_url, has_pre_action)
values('Remove QP Plugin', 'remove_plugin', (select id from action_type where code = 'remove'), (select id from entity_type where code = 'plugin'),
       'Do you really want to remove this QP plugin?', '~/QpPlugin/Remove/', false)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, is_interface)
values('Save QP Plugin', 'save_plugin', (select id from action_type where code = 'save'), (select id from entity_type where code = 'plugin'), false)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id, is_interface)
values('Refresh QP Plugins', 'refresh_plugins', (select id from action_type where code = 'refresh'), (select id from entity_type where code = 'plugin'), true)
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, code, controller_action_url, is_interface)
VALUES((select id from action_type where code = 'read'), (select id from entity_type where code = 'db'), 'Scheduled Tasks', 'scheduled_tasks', '~/Home/ScheduledTasks/', true)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id)
values('Refresh Scheduled Tasks', 'refresh_scheduled_tasks', (select id from action_type where code = 'refresh'), (select id from entity_type where code = 'db'))
on conflict do nothing;

insert into backend_action(name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface, default_view_type_id)
values('QP Plugin versions', 'Versions', 'list_plugin_version', (select id from action_type where code = 'list'),
       (select id from entity_type where code = 'plugin_version'), '~/QpPluginVersion/Index/',  true, (select id from view_type where code = 'list'))
on conflict do nothing;

insert into backend_action(name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('Preview QP Plugin version', 'Preview', 'preview_plugin_version', (select id from action_type where code = 'read'),
       (select id from entity_type where code = 'plugin_version'), '~/QpPluginVersion/Properties/', true)
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('Compare QP Plugin version with Current', 'Compare with Current', 'compare_plugin_version_with_current', (select id from action_type where code = 'read'),
       (select id from entity_type where code = 'plugin_version'), '~/QpPluginVersion/CompareWithCurrent/', true)
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('Compare QP Plugin versions', 'Compare versions', 'compare_plugin_versions', (select id from action_type where code = 'compare'),
       (select id from entity_type where code = 'plugin_version'), '~/QpPluginVersion/Compare/', true)
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id)
values('Refresh Plugin Version', 'refresh_plugin_version', (select id from action_type where code = 'refresh'),
       (select id from entity_type where code = 'plugin_version'))
on conflict do nothing;

insert into backend_action(name, code, type_id, entity_type_id)
values('Refresh Plugin Versions', 'refresh_plugin_versions', (select id from action_type where code = 'refresh'),
       (select id from entity_type where code = 'plugin_version'))
on conflict do nothing;

insert into backend_action(type_id, entity_type_id, name, code)
    values ((select id from action_type where code = 'auto_resize'), (select id from entity_type where code = 'site_file'),
            'Auto Resize Site File', 'auto_resize_site_file')
on conflict do nothing;

insert into backend_action(type_id, entity_type_id, name, code)
    values ((select id from action_type where code = 'auto_resize'), (select id from entity_type where code = 'content_file'),
        'Auto Resize Content File', 'auto_resize_content_file')
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, code)
values ((select id from action_type where code = 'multiple_save'), (select id from entity_type where code = 'article'),
       'Multiple Save Articles', 'multiple_save_article')
on conflict do nothing;

insert into backend_action (type_id, entity_type_id, name, code)
values ((select id from action_type where code = 'multiple_update'), (select id from entity_type where code = 'article'),
       'Multiple Update Articles', 'multiple_update_article')
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface)
values('External Workflow User Tasks', 'User Tasks', 'list_article_external_workflow_tasks', (select id from action_type where code = 'list'),
       (select id from entity_type where code = 'db'), '~/Home/ExternalWorkflowUserTasks/', true)
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id)
values('Refresh External Workflow User Tasks', 'Refresh User Tasks', 'refresh_article_external_workflow_tasks', (select id from action_type where code = 'refresh'),
       (select id from entity_type where code = 'db'))
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id, controller_action_url)
values('Complete External Workflow User Task', 'Complete User Task', 'complete_article_external_workflow_task', (select id from action_type where code = 'complete'),
       (select id from entity_type where code = 'article_external_workflow'), '~/ExternalWorkflowUserActions/CompleteUserTask/')
on conflict do nothing;

insert into backend_action (name, short_name, code, type_id, entity_type_id, controller_action_url, is_interface, is_window, window_width, window_height)
values('Get External Workflow User Task', 'Get User Task', 'get_article_external_workflow_task', (select id from action_type where code = 'complete'),
       (select id from entity_type where code = 'article_external_workflow'), '~/ExternalWorkflowUserActions/GetUserTask/', true, true, 800, 600)
on conflict do nothing;


