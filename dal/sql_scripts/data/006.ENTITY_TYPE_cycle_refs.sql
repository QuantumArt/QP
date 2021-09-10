update ENTITY_TYPE
set
    FOLDER_DEFAULT_ACTION_ID = dbo.qp_action_id('list_plugin'),
    DEFAULT_ACTION_ID = dbo.qp_action_id('edit_plugin'),
    CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugin'),
    FOLDER_CONTEXT_MENU_ID = dbo.qp_context_menu_id('plugins')
where CODE = 'plugin'
GO