DECLARE @articles_with_wrong_statuses TABLE (
Site_ID int,
CONTENT_ID int,
STATUS_TYPE_ID int,
CONTENT_ITEM_ID int
)

INSERT INTO @articles_with_wrong_statuses
	SELECT c.Site_ID, c.CONTENT_ID, ci.STATUS_TYPE_ID, ci.CONTENT_ITEM_ID FROM [dbo].[CONTENT_ITEM] ci
	INNER JOIN [dbo].[CONTENT] c ON ci.CONTENT_ID = c.CONTENT_ID
	WHERE  ci.STATUS_TYPE_ID NOT IN (
					SELECT STATUS_TYPE_ID
					FROM [dbo].[STATUS_TYPE] i_st
					WHERE i_st.SITE_ID = c.SITE_ID
					)
IF EXISTS (SELECT * FROM @articles_with_wrong_statuses)
BEGIN
DECLARE @statuses_names TABLE (
	STATUS_TYPE_NAME nvarchar(255),
	STATUS_TYPE_ID int,
	NEW_SITE int
)

INSERT INTO @statuses_names
	SELECT st1.STATUS_TYPE_NAME, st1.STATUS_TYPE_ID as old_status, st1.SITE_ID as old_site from [dbo].[STATUS_TYPE] st1
	WHERE st1.STATUS_TYPE_ID IN (SELECT STATUS_TYPE_ID FROM @articles_with_wrong_statuses)

;WITH rel_betw_statuses AS (
	SELECT new_status_id, new_site_id, old_status_id FROM (
	SELECT st.SITE_ID AS new_site_id, st.STATUS_TYPE_ID AS new_status_id, stn.STATUS_TYPE_NAME, stn.STATUS_TYPE_ID AS old_status_id
		FROM [dbo].[STATUS_TYPE] st
		INNER JOIN @statuses_names stn ON st.STATUS_TYPE_NAME = stn.STATUS_TYPE_NAME
	) AS nsi
	WHERE nsi.NEW_SITE_ID IN (SELECT SITE_ID FROM @wrong_statuses)
)

UPDATE CONTENT_ITEM
	SET STATUS_TYPE_ID = (
		SELECT NEW_STATUS_ID FROM CONTENT_ITEM AS ci
		INNER JOIN @temp AS t ON t.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
		INNER JOIN rel_betw_statuses AS rbs ON t.SITE_ID = rbs.new_site_id AND ci.STATUS_TYPE_ID = rbs.old_status_id
		WHERE  [dbo].[CONTENT_ITEM].CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
)
WHERE CONTENT_ITEM_ID IN (SELECT CONTENT_ITEM_ID FROM @temp)
END



DECLARE @workflow_rules_ids TABLE (
WORKFLOW_RULE_ID int
)

INSERT INTO @workflow_rules_ids
	SELECT workflow_rule_id FROM [dbo].[workflow_rules] wr
		WHERE SUCCESSOR_STATUS_ID NOT IN (
			SELECT STATUS_TYPE_ID FROM [dbo].[STATUS_TYPE] st
			INNER JOIN [dbo].[workflow] w ON st.SITE_ID = w.SITE_ID
			WHERE wr.WORKFLOW_ID = w.WORKFLOW_ID
	)

IF EXISTS (SELECT * FROM @workflow_rules_ids)
BEGIN
UPDATE [dbo].[WORKFLOW_RULES]
SET SUCCESSOR_STATUS_ID = (
	SELECT st2.STATUS_TYPE_ID
		FROM [dbo].[STATUS_TYPE] st1
			INNER JOIN [dbo].[STATUS_TYPE] st2 on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME
			INNER JOIN [dbo].[workflow] w on st2.SITE_ID = w.SITE_ID
		WHERE w.WORKFLOW_ID = [dbo].[workflow_rules].WORKFLOW_ID AND [dbo].[WORKFLOW_RULES].SUCCESSOR_STATUS_ID = st1.STATUS_TYPE_ID
	)
WHERE WORKFLOW_RULE_ID IN (SELECT WORKFLOW_RULE_ID FROM @workflow_rules_ids)
END
