GO
if not exists (select * From BACKEND_ACTION where code = 'copy_custom_action')
begin

  INSERT INTO [dbo].[BACKEND_ACTION] ([TYPE_ID], [ENTITY_TYPE_ID], [NAME], [SHORT_NAME], [CODE], [CONTROLLER_ACTION_URL],[IS_INTERFACE])
  VALUES (dbo.qp_action_type_id('copy'), dbo.qp_entity_type_id('custom_action'), N'Create Like Custom Action', 'Create Like', N'copy_custom_action', '~/CustomAction/Copy/', 0)

  INSERT INTO [dbo].[CONTEXT_MENU_ITEM] ([CONTEXT_MENU_ID], [ACTION_ID], [Name], [ORDER], [ICON],  [BOTTOM_SEPARATOR])
  VALUES (dbo.qp_context_menu_id('custom_action'), dbo.qp_action_id('copy_custom_action'), N'Create Like', 5, 'create_like.gif', 1)

end
