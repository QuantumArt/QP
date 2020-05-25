ALTER TRIGGER [dbo].[ti_add_to_everyone_group] ON [dbo].[USERS] 
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_add_to_everyone_group') is null
    BEGIN    
        INSERT INTO user_group_bind (user_id, group_id)
        SELECT i.user_id, ug.group_id FROM inserted i, user_group ug WHERE ug.built_in = 1 and ug.readonly = 1
    END
END
GO