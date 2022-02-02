ALTER PROCEDURE [dbo].[qp_get_toolbar_buttons_list_by_action_code](@user_id int, @action_code nvarchar(50), @entity_id int)
AS
BEGIN
	DECLARE @action_id int
	SET @action_id = dbo.qp_action_id(@action_code)

	DECLARE @language_id numeric(18, 0)
	SET @language_id = dbo.qp_language(@user_id)

	if EXISTS (select * from user_group_bind where group_id = 1 and user_id = @user_Id) OR @user_id = 1 BEGIN
		SELECT
		ba.ID AS ACTION_ID,
		ba.CODE AS ACTION_CODE,
		bat.CODE AS ACTION_TYPE_CODE,
		ba2.ID AS PARENT_ACTION_ID,
		ba2.CODE AS PARENT_ACTION_CODE,
		dbo.qp_translate(atb.NAME, @language_id) AS NAME,
		bat.ITEMS_AFFECTED,
		atb.[ORDER],
		ISNULL(ca.ICON_URL, atb.ICON) AS ICON,
		atb.ICON_DISABLED,
		atb.IS_COMMAND
	FROM
		ACTION_TOOLBAR_BUTTON AS atb
		INNER JOIN BACKEND_ACTION AS ba ON atb.ACTION_ID = ba.ID
		LEFT OUTER JOIN CUSTOM_ACTION AS ca ON ca.ACTION_ID = ba.ID
		INNER JOIN ACTION_TYPE AS bat ON bat.ID = ba.TYPE_ID
		INNER JOIN BACKEND_ACTION AS ba2 ON atb.PARENT_ACTION_ID = ba2.ID
	WHERE
		atb.PARENT_ACTION_ID = @action_id
	ORDER BY
		[ORDER]
	END
	ELSE BEGIN
		DECLARE @entity_code nvarchar(50)
		select @entity_code = dbo.qp_entity_type_code(entity_type_id) from backend_action where code = @action_code

		declare @seqQuery nvarchar(max);
		EXEC [dbo].[qp_GetActionPermissionAsQuery]
			@user_id = @user_id,
			@result = @seqQuery OUTPUT

		declare @fullQuery nvarchar(max);

		set @fullQuery = REPLACE(CAST(N'SELECT
			ba.ID AS ACTION_ID,
			ba.CODE AS ACTION_CODE,
			bat.CODE AS ACTION_TYPE_CODE,
			ba2.ID AS PARENT_ACTION_ID,
			ba2.CODE AS PARENT_ACTION_CODE,
			dbo.qp_translate(atb.NAME, @p0) AS NAME,
			bat.ITEMS_AFFECTED,
			atb.[ORDER],
			ISNULL(ca.ICON_URL, atb.ICON) AS ICON,
			atb.ICON_DISABLED,
			atb.IS_COMMAND
		FROM
			ACTION_TOOLBAR_BUTTON AS atb
			INNER JOIN BACKEND_ACTION AS ba ON atb.ACTION_ID = ba.ID
			LEFT OUTER JOIN CUSTOM_ACTION AS ca ON ca.ACTION_ID = ba.ID
			INNER JOIN ACTION_TYPE AS bat ON bat.ID = ba.TYPE_ID
			INNER JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = bat.REQUIRED_PERMISSION_LEVEL_ID
			INNER JOIN BACKEND_ACTION AS ba2 ON atb.PARENT_ACTION_ID = ba2.ID
			INNER JOIN
			(<$_security_insert_$>) SEC ON SEC.BACKEND_ACTION_ID = ba.ID
		WHERE
			atb.PARENT_ACTION_ID = @p1
			AND (SEC.PERMISSION_LEVEL >= PL.PERMISSION_LEVEL or bat.CODE = ''refresh'')
			AND dbo.qp_action_visible(@p2, @p3, @p4, ba.CODE) = 1
		ORDER BY
			[ORDER]' AS nvarchar(max)), N'<$_security_insert_$>', @seqQuery)

		EXEC sp_executesql @fullQuery,
			N'@p0 numeric(18, 0), @p1 int, @p2 int, @p3 nvarchar(50), @p4 int',
			@p0 = @language_id, @p1 = @action_id, @p2 = @user_id, @p3 = @entity_code, @p4 = @entity_id;
	END
END
GO