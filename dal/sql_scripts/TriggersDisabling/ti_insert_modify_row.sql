ALTER TRIGGER [dbo].[ti_insert_modify_row] ON [dbo].[CONTENT] FOR INSERT
AS
BEGIN
  	if object_id('tempdb..#disable_ti_insert_modify_row') is null
	begin
		INSERT INTO CONTENT_MODIFICATION
		SELECT CONTENT_ID, GETDATE(), GETDATE() from inserted
	end

END
GO