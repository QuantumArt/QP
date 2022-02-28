CREATE OR REPLACE PROCEDURE public.qp_before_content_item_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	    version_ids integer[];
	BEGIN
	    IF ids IS NOT NULL THEN
	        version_ids := array_agg(civ.content_item_version_id)
	            from content_item_version civ where content_item_id = ANY(ids);
	        call qp_before_content_item_version_delete(version_ids);

            delete from waiting_for_approval where content_item_id = ANY(ids);
            delete from child_delays where child_id = ANY(ids);
            delete from content_item_version where content_item_id = ANY(ids);
            delete from item_to_item_version where linked_item_id = ANY(ids);
            delete from item_link where item_id = ANY(ids) or linked_item_id = ANY(ids);
            delete from item_link_async where item_id = ANY(ids) or linked_item_id = ANY(ids);
            delete from field_article_bind where article_id = ANY(ids);
            delete from content_data where content_item_id = ANY(ids);
        END IF;
	END;
$BODY$;


