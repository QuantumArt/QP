if not exists (select * from BACKEND_ACTION where code = 'scheduled_tasks')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, code, CONTROLLER_ACTION_URL, IS_INTERFACE)
VALUES(dbo.qp_action_type_id('read'), dbo.qp_entity_type_id('db'), 'Scheduled Tasks', 'scheduled_tasks', '~/Home/ScheduledTasks/', 1)

IF NOT EXISTS(SELECT * FROM  CONTEXT_MENU_ITEM WHERE NAME = 'Scheduled Tasks' AND CONTEXT_MENU_ID = dbo.qp_context_menu_id('db'))
INSERT INTO CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER])
VALUES(dbo.qp_context_menu_id('db'), dbo.qp_action_id('scheduled_tasks'), 'Scheduled Tasks', 90)

if not exists (select * from BACKEND_ACTION where code = 'refresh_scheduled_tasks')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
values('Refresh Scheduled Tasks', 'refresh_scheduled_tasks', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('db'))

if not exists (select * from ACTION_TOOLBAR_BUTTON where name = 'Refresh' and PARENT_ACTION_ID = dbo.qp_action_id('scheduled_tasks'))
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, ICON, [ORDER], NAME)
values (dbo.qp_action_id('scheduled_tasks'), dbo.qp_action_id('refresh_scheduled_tasks'), 'refresh.gif', 100, 'Refresh')

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CAN_MANAGE_SCHEDULED_TASKS' and TABLE_NAME = 'USER_GROUP')
    alter table [USER_GROUP]
    add [CAN_MANAGE_SCHEDULED_TASKS] bit NOT NULL CONSTRAINT [DF_USER_GROUP_CAN_MANAGE_SCHEDULED_TASKS] DEFAULT ((0))
GO

ALTER VIEW [dbo].[USER_GROUP_TREE]
WITH SCHEMABINDING
AS
select ug.[GROUP_ID]
      ,ug.[GROUP_NAME]
      ,ug.[DESCRIPTION]
      ,ug.[CREATED]
      ,ug.[MODIFIED]
      ,ug.[LAST_MODIFIED_BY]
      ,U.[LOGIN] as LAST_MODIFIED_BY_LOGIN
      ,ug.[shared_content_items]
      ,ug.[nt_group]
      ,ug.[ad_sid]
      ,ug.[BUILT_IN]
      ,ug.[READONLY]
      ,ug.[use_parallel_workflow]
      ,ug.[CAN_UNLOCK_ITEMS]
      ,ug.[CAN_MANAGE_SCHEDULED_TASKS]
      ,gtg.Parent_Group_Id AS PARENT_GROUP_ID
from dbo.USER_GROUP ug
left join dbo.Group_To_Group gtg on ug.GROUP_ID = gtg.Child_Group_Id
join dbo.USERS U ON U.[USER_ID] = ug.LAST_MODIFIED_BY

GO