
ALTER TRIGGER [dbo].[td_drop_table] ON [dbo].[CONTENT]
FOR  DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_drop_table') is null
    begin

		declare @content_id numeric

		declare del_content CURSOR FOR
		select content_id from deleted

		open del_content
		FETCH NEXT FROM del_content into @content_id
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC qp_content_table_drop @content_id
			FETCH NEXT FROM del_content into @content_id
		END

		close del_content
		deallocate del_content
    END
END
GO


