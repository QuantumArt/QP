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