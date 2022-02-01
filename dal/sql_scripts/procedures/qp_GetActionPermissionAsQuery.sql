ALTER PROCEDURE [dbo].[qp_GetActionPermissionAsQuery]
	@user_id numeric(18,0),
	@result nvarchar(max) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @actionSecQuery nvarchar(max);
	declare @entitySecQuery nvarchar(max);

	EXEC [dbo].[qp_GetPermittedItemsAsQuery]
		@user_id = @user_id,
		@start_level = 0,
		@end_level = 100,
		@entity_name = N'BACKEND_ACTION',
		@SQLOut = @actionSecQuery OUTPUT

	EXEC [dbo].[qp_GetEntityPermissionAsQuery]
		@user_id = @user_id,
		@SQLOut = @entitySecQuery OUTPUT


	SELECT @result = REPLACE(
		REPLACE(CAST(N'select AP.BACKEND_ACTION_ID, COALESCE(AP.PERMISSION_LEVEL, EP.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL from
		(select L.PERMISSION_LEVEL AS PERMISSION_LEVEL, T.ID AS BACKEND_ACTION_ID, T.ENTITY_TYPE_ID FROM
			(<$_security_insert_$>) P1
			LEFT JOIN backend_action_access_PermLevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.[USER_ID] = <$_userid_$>
			RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
			LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
		) AP
		JOIN ' AS nvarchar(max)), N'<$_security_insert_$>', @actionSecQuery) +
		REPLACE(CAST(N'(<$_security_insert_$>) EP ON AP.ENTITY_TYPE_ID = EP.ENTITY_TYPE_ID' AS nvarchar(max)), N'<$_security_insert_$>', @entitySecQuery)
	, N'<$_userid_$>', @user_id)

END
GO