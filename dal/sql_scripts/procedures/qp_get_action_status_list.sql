ALTER PROCEDURE [dbo].[qp_get_action_status_list](@user_id int, @action_code nvarchar(50), @entity_id numeric)
AS
BEGIN
	if EXISTS (select * from user_group_bind where group_id = 1 and user_id = @user_Id) OR @user_id = 1 BEGIN
		SELECT ba.CODE, cast(1 as bit) as visible
		FROM ACTION_TOOLBAR_BUTTON atb
		INNER JOIN BACKEND_ACTION ba on ba.ID = atb.ACTION_ID
		INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
		WHERE atb.PARENT_ACTION_ID = dbo.qp_action_id(@action_code) AND at.items_affected = 1
	END
	ELSE BEGIN
		DECLARE @entity_code nvarchar(50)
		select @entity_code = dbo.qp_entity_type_code(entity_type_id) from backend_action where code = @action_code

		declare @seqQuery nvarchar(max);
		EXEC [dbo].[qp_GetActionPermissionAsQuery]
			@user_id = @user_id,
			@result = @seqQuery OUTPUT

		declare @fullQuery nvarchar(max);
		select @fullQuery = REPLACE(CAST(N'SELECT ba.CODE,
										CAST(
												(
													CASE
														WHEN
															sec.PERMISSION_LEVEL >= PL.PERMISSION_LEVEL AND dbo.qp_action_visible(@p2, @p3, @p4, ba.CODE) = 1
														THEN 1
														ELSE 0
													END
												) AS BIT
										) as visible
										FROM ACTION_TOOLBAR_BUTTON atb
											INNER JOIN BACKEND_ACTION ba on ba.ID = atb.ACTION_ID
											INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
											JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = AT.REQUIRED_PERMISSION_LEVEL_ID
											JOIN (<$_security_insert_$>) SEC ON SEC.BACKEND_ACTION_ID = ba.ID
										WHERE atb.PARENT_ACTION_ID = dbo.qp_action_id(@p1) AND at.items_affected = 1' AS nvarchar(max)), N'<$_security_insert_$>', @seqQuery);
			EXEC sp_executesql @fullQuery,
				N'@p1 nvarchar(50), @p2 int, @p3 nvarchar(50), @p4 int',
				@p1 = @action_code, @p2 = @user_id, @p3 = @entity_code, @p4 = @entity_id;
	END
END
GO