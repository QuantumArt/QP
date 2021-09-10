update entity_type
set
    folder_default_action_id = (select id from backend_action where code = 'list_plugin'),
    default_action_id = (select id from backend_action where code = 'edit_plugin'),
    context_menu_id = (select id from context_menu where code = 'plugin'),
    folder_context_menu_id = (select id from context_menu where code = 'plugins')
where code = 'plugin';
