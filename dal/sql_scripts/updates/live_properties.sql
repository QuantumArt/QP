if not exists (select * from BACKEND_ACTION where ENTITY_TYPE_ID = dbo.qp_entity_type_id('article') and code = 'view_live_article')
begin
	insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_INTERFACE)
	VALUES(dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('article'), 'Article Live Properties', 'Live Properties', 'view_live_article', '~/Article/LiveProperties/', 1)
end
GO

if not exists(select * from CONTEXT_MENU_ITEM where CONTEXT_MENU_ID = dbo.qp_context_menu_id('article') and name = 'Live Properties')
begin
	insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
	VALUES(dbo.qp_context_menu_id('article'), dbo.qp_action_id('view_live_article'), 'Live Properties', 52, 'properties.gif')
end
GO

if not exists(select * From ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('view_live_article'))
begin
	insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, [ORDER])
	values(dbo.qp_action_id('view_live_article'), dbo.qp_action_id('refresh_article'), 'Refresh', 'refresh.gif', 10)

end
GO

exec qp_update_translations 'Live Properties', 'Свойства Live'
GO

