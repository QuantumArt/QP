IF NOT EXISTS(SELECT * FROM CONTEXT_MENU_ITEM WHERE NAME = 'Unselect Child Articles')
INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER],  ICON)
VALUES(dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('unselect_child_articles'), 'Unselect Child Articles', 90, 'deselect_all.gif')

IF NOT EXISTS(SELECT * FROM  CONTEXT_MENU_ITEM WHERE NAME = 'Select Child Articles')
INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
VALUES(dbo.qp_context_menu_id('virtual_article'), dbo.qp_action_id('select_child_articles'), 'Select Child Articles', 80, 'select_all.gif')
