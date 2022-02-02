ALTER PROCEDURE [dbo].[qp_GetEntityPermissionAsQuery]
	@user_id numeric(18,0),
	@SQLOut nvarchar(max) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @entitySecQuery nvarchar(max);

	EXEC	[dbo].[qp_GetPermittedItemsAsQuery]
			@user_id = @user_id,
			@start_level = 0,
			@end_level = 100,
			@entity_name = N'ENTITY_TYPE',
			@SQLOut = @entitySecQuery OUTPUT


	SELECT @SQLOut = REPLACE(
		REPLACE(CAST(N'select COALESCE(L.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL, T.ID AS ENTITY_TYPE_ID, HIDE FROM
			(<$_security_insert_$>) P1
			LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.[USER_ID] = <$_userid_$>
			RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
			LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL' AS nvarchar(max)), N'<$_security_insert_$>', @entitySecQuery)
	, N'<$_userid_$>', @user_id)

END
GO