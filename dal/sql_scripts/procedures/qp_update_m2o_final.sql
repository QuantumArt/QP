exec qp_drop_existing 'qp_update_m2o_final', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2o_final]
@id numeric
AS
BEGIN
    if exists(select * from #resultIds) or exists(select * from CHILD_DELAYS where id = @id)
    begin
        declare @statusId numeric
        declare @splitted bit
        declare @lastModifiedBy numeric
        declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, remove_delays bit not null default 0, primary key(id, attribute_id))
        declare @ids_to_split [Ids]
        declare @ids_to_process [Ids]
        declare @ids_to_merge [Ids]
        declare @idsstr nvarchar(max)
        declare @maxStatus numeric

        select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
        select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

        if @statusId = @maxStatus and @splitted = 0 begin
            insert @ids_to_merge
            select @id

            exec qp_merge_delays @ids_to_merge, @lastModifiedBy
        end

        if not exists(select * from #resultIds)
            return

        insert into @ids(id, attribute_id, to_remove, remove_delays)
        select r.*, case when cd.id is null then 0 else 1 end as remove_delays
		from #resultIds r left join CHILD_DELAYS cd on cd.id = @id and r.id = cd.child_id

        update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1
        where content_item_id in (select id from @ids)

        update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate()
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 0

        insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
        select r.id, r.attribute_id, @id, NULL, getdate()
        from @ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 0 and cd.CONTENT_DATA_ID is null

        update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate()
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
        where r.to_remove = 1

		delete from CHILD_DELAYS where id = @id and child_id in (select id from @ids where remove_delays = 1)

        declare @resultId numeric

        if (@statusId <> @maxStatus and @maxStatus is not null or @splitted = 1) begin
            insert into child_delays (id, child_id) select @id, r.id from @ids r
            inner join content_item ci on r.id = ci.content_item_id
            left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
            left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
            left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
            where ex.child_id is null and ci.status_type_id = wms.max_status_type_id
                and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
				and r.remove_delays = 0

            insert into @ids_to_split
            select content_item_id from content_item with(nolock) where content_item_id in (select child_id from child_delays where id = @id) and splitted = 0

            exec qp_split_articles @ids_to_split

            update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
        end

        select @idsstr = COALESCE(@idsstr + ', ', '') + CAST(id as nvarchar) from @ids

        exec qp_replicate_items @idsstr
    end
END
GO
