exec qp_drop_existing 'qp_merge_articles', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_articles]
@ids [Ids] READONLY,
@last_modified_by numeric = 1,
@force_merge bit = 0
AS
BEGIN
  declare @ids2 [Ids], @ids2str nvarchar(max)

  insert into @ids2 select id from @ids i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID and (SCHEDULE_NEW_VERSION_PUBLICATION = 1 or @force_merge = 1)

  if exists(select * From @ids2)
  begin
    exec qp_merge_links_multiple @ids2, @force_merge
    UPDATE content_item with(rowlock) set not_for_replication = 1 WHERE content_item_id in (select id from @ids2)
    UPDATE content_item with(rowlock) set SCHEDULE_NEW_VERSION_PUBLICATION = 0, MODIFIED = GETDATE(), LAST_MODIFIED_BY = @last_modified_by, CANCEL_SPLIT = 0 where CONTENT_ITEM_ID in (select id from @ids2)
    SELECT @ids2str = COALESCE(@ids2str + ',', '') + cast(id as nvarchar(20)) from @ids2
    exec qp_replicate_items @ids2str
    UPDATE content_item_schedule with(rowlock) set delete_job = 0 WHERE content_item_id in (select id from @ids2)
    DELETE FROM content_item_schedule with(rowlock) WHERE content_item_id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE id in (select id from @ids2)
    delete from CHILD_DELAYS with(rowlock) WHERE child_id in (select id from @ids2)

  end
END
GO
