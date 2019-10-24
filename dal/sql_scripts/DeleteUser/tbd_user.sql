ALTER TRIGGER [dbo].[tbd_user] ON [dbo].[USERS]
INSTEAD OF DELETE
AS
BEGIN

  DELETE USER_GROUP_BIND FROM USER_GROUP_BIND c inner join deleted d on c.user_id = d.user_id
  DELETE USER_DEFAULT_FILTER FROM USER_DEFAULT_FILTER f inner join deleted d on f.user_id = d.user_id

  UPDATE CONTAINER SET locked = NULL, locked_by = NULL FROM CONTAINER c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_FORM SET locked = NULL, locked_by = NULL FROM CONTENT_FORM c inner join deleted d on c.locked_by = d.user_id
  UPDATE CONTENT_ITEM SET locked = NULL, locked_by = NULL FROM CONTENT_ITEM c inner join deleted d on c.locked_by = d.user_id
  UPDATE [OBJECT] SET locked = NULL, locked_by = NULL FROM [OBJECT] c inner join deleted d on c.locked_by = d.user_id
  UPDATE OBJECT_FORMAT SET locked = NULL, locked_by = NULL FROM OBJECT_FORMAT c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE SET locked = NULL, locked_by = NULL FROM PAGE c inner join deleted d on c.locked_by = d.user_id
  UPDATE PAGE_TEMPLATE SET locked = NULL, locked_by = NULL FROM PAGE_TEMPLATE c inner join deleted d on c.locked_by = d.user_id
  UPDATE [SITE] SET locked = NULL, locked_by = NULL FROM [SITE] c inner join deleted d on c.locked_by = d.user_id

  UPDATE [SITE] SET last_modified_by = 1 FROM [SITE] c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT SET last_modified_by = 1 FROM CONTENT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM SET last_modified_by = 1 FROM CONTENT_ITEM c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_SCHEDULE SET last_modified_by = 1 FROM CONTENT_ITEM_SCHEDULE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET created_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.created_by = d.user_id
  UPDATE CONTENT_ITEM_VERSION SET last_modified_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ATTRIBUTE SET last_modified_by = 1 FROM CONTENT_ATTRIBUTE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE PAGE_TEMPLATE SET last_modified_by = 1 FROM PAGE_TEMPLATE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_modified_by = 1 FROM PAGE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE PAGE SET last_assembled_by = 1 FROM PAGE c inner join deleted d on c.last_assembled_by  = d.user_id
  UPDATE OBJECT SET last_modified_by = 1 FROM OBJECT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT SET last_modified_by = 1 FROM OBJECT_FORMAT c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE OBJECT_FORMAT_VERSION SET last_modified_by = 1 FROM OBJECT_FORMAT_VERSION c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE FOLDER SET last_modified_by = 1 FROM FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE FOLDER_ACCESS SET last_modified_by = 1 FROM FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER SET last_modified_by = 1 FROM CONTENT_FOLDER c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_FOLDER_ACCESS SET last_modified_by = 1 FROM CONTENT_FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CODE_SNIPPET SET last_modified_by = 1 FROM CODE_SNIPPET c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE STYLE SET last_modified_by = 1 FROM STYLE c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE STATUS_TYPE SET last_modified_by = 1 FROM STATUS_TYPE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW SET last_modified_by = 1 FROM WORKFLOW c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE SITE_ACCESS SET last_modified_by = 1 FROM SITE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ACCESS SET last_modified_by = 1 FROM CONTENT_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE CONTENT_ITEM_ACCESS SET last_modified_by = 1 FROM CONTENT_ITEM_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE WORKFLOW_ACCESS SET last_modified_by = 1 FROM WORKFLOW_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE USER_GROUP SET last_modified_by = 1 FROM USER_GROUP c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE USERS SET last_modified_by = 1 FROM USERS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE NOTIFICATIONS SET last_modified_by = 1 FROM NOTIFICATIONS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE CONTENT_ITEM_STATUS_HISTORY SET user_id = 1 WHERE user_id in (select user_id from deleted)

  UPDATE CUSTOM_ACTION SET LAST_MODIFIED_BY = 1 FROM CUSTOM_ACTION c INNER JOIN deleted d on c.LAST_MODIFIED_BY = d.[USER_ID]

  UPDATE NOTIFICATIONS SET FROM_BACKENDUSER_ID = 1 FROM NOTIFICATIONS c inner join deleted d on c.FROM_BACKENDUSER_ID = d.user_id

  UPDATE ENTITY_TYPE_ACCESS SET last_modified_by = 1 FROM ENTITY_TYPE_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE ACTION_ACCESS SET last_modified_by = 1 FROM ACTION_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

  UPDATE DB SET last_modified_by = 1 FROM DB c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE DB SET single_user_id = NULL FROM DB c inner join deleted d on c.single_user_id = d.user_id
  UPDATE VE_PLUGIN SET last_modified_by = 1 FROM VE_PLUGIN c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_STYLE SET last_modified_by = 1 FROM VE_STYLE c inner join deleted d on c.last_modified_by = d.user_id
  UPDATE VE_COMMAND SET last_modified_by = 1 FROM VE_COMMAND c inner join deleted d on c.last_modified_by = d.user_id

  delete users from users c inner join deleted d on c.user_id = d.user_id
END
