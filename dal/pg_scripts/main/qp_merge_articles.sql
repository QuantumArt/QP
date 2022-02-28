create or replace procedure qp_merge_articles(ids integer[], last_modified_by integer DEFAULT 1, force_merge boolean DEFAULT false)
    language plpgsql
as
$$
DECLARE
		ids2 int[];
    BEGIN
		ids2 := array_agg(ci.content_item_id) from content_item ci where ci.content_item_id = ANY(ids)
			and (ci.SCHEDULE_NEW_VERSION_PUBLICATION or force_merge);

		IF ids2 is not null THEN
			call qp_merge_links_multiple(ids2, force_merge);

    		UPDATE content_item set not_for_replication = true WHERE content_item_id = ANY(ids2);

    		UPDATE content_item set SCHEDULE_NEW_VERSION_PUBLICATION = false, MODIFIED = now(),
			LAST_MODIFIED_BY = $2, CANCEL_SPLIT = false
			where CONTENT_ITEM_ID = ANY(ids2);

			call qp_replicate_items(ids2);

    		UPDATE content_item_schedule set delete_job = false WHERE content_item_id = ANY(ids2);
			DELETE FROM content_item_schedule WHERE content_item_id = ANY(ids2);
    		DELETE FROM CHILD_DELAYS WHERE id = ANY(ids2);
    		DELETE FROM CHILD_DELAYS WHERE child_id = ANY(ids2);

		END IF;
	END;
$$;


