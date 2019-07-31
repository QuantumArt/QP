ALTER TRIGGER [dbo].[tiud_remove_empty_content_groups] ON [dbo].[CONTENT] FOR INSERT, UPDATE, DELETE
AS BEGIN
	if object_id('tempdb..#disable_tiud_remove_empty_content_groups') is null
	begin
      DELETE FROM content_group
      WHERE NAME <> 'Default Group'
      AND NOT EXISTS(SELECT * FROM content WHERE content.content_group_id = content_group.content_group_id)
    END
END
GO