ALTER PROCEDURE [dbo].[qp_get_menu_status_list](@user_id int, @menu_code nvarchar(50), @entity_id numeric)
AS
	BEGIN
		if EXISTS (select * from user_group_bind where group_id = 1 and user_id = @user_Id) OR @user_id = 1 BEGIN
		SELECT ba.CODE, cast(1 as bit) as visible
		FROM CONTEXT_MENU_ITEM cmi
			INNER JOIN BACKEND_ACTION ba on ba.ID = cmi.ACTION_ID
		WHERE cmi.context_menu_id = dbo.qp_context_menu_id(@menu_code)
	END
	ELSE BEGIN
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
									FROM CONTEXT_MENU_ITEM cmi
										INNER JOIN BACKEND_ACTION ba on ba.ID = cmi.ACTION_ID
										JOIN ACTION_TYPE AS AT ON AT.ID = ba.[TYPE_ID]
										JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = AT.REQUIRED_PERMISSION_LEVEL_ID
										JOIN (<$_security_insert_$>) SEC ON SEC.BACKEND_ACTION_ID = ba.ID
									WHERE cmi.context_menu_id = dbo.qp_context_menu_id(@menu_code)' AS nvarchar(max)), N'<$_security_insert_$>', @seqQuery);
		EXEC sp_executesql @fullQuery,
			N'@menu_code nvarchar(50), @p2 int, @p3 nvarchar(50), @p4 int',
			@menu_code = @menu_code, @p2 = @user_id, @p3 = @menu_code, @p4 = @entity_id;
	END
END
GO