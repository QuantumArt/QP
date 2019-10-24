ALTER TRIGGER [dbo].[td_content_and_article_workflow_bind]
ON [dbo].[workflow]
FOR DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_content_and_article_workflow_bind') is null
    begin

        DELETE FROM content_workflow_bind WHERE workflow_id in (select d.workflow_id from deleted d)
        DELETE FROM article_workflow_bind WHERE workflow_id in (select d.workflow_id from deleted d)
        DELETE waiting_for_approval from waiting_for_approval wa inner join content_item_workflow ciw on wa.content_item_id = ciw.content_item_id
            WHERE ciw.workflow_id in (select d.workflow_id from deleted d)
    end
END
GO