CREATE OR REPLACE PROCEDURE public.qp_before_content_item_version_delete(
	version_ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
	    IF version_ids IS NOT NULL THEN
			delete from content_item_status_history where content_item_version_id = ANY(version_ids)
			and system_status_type_id = 2;
			delete from item_to_item_version where content_item_version_id = ANY(version_ids);
			delete from version_content_data where content_item_version_id = ANY(version_ids);
        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_item_version_delete(integer[])
    OWNER TO postgres;

