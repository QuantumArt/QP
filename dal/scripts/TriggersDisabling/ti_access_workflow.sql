ALTER TRIGGER [dbo].[ti_access_workflow] ON [dbo].[workflow] FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_access_workflow') is null
    begin
		INSERT INTO workflow_access (workflow_id, user_id, permission_level_id, last_modified_by)
		SELECT workflow_id, last_modified_by, 1, 1 FROM inserted

		INSERT INTO workflow_access (workflow_id, group_id, permission_level_id, last_modified_by)
		SELECT c.workflow_id, 1, 1, 1
		FROM inserted AS c WHERE c.workflow_id NOT IN (
			SELECT ca.workflow_id FROM workflow_access AS ca
			WHERE ca.workflow_id = c.workflow_id AND ca.group_id = 1
		)
	END
END
GO