CREATE OR REPLACE PROCEDURE public.qp_before_content_to_content_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
	    IF ids IS NOT NULL THEN
            delete from content_attribute ca where link_id = ANY(ids);
            delete from item_to_item ii where link_id = ANY(ids);
            delete from item_link_async ila where link_id = ANY(ids);
        END IF;
	END;
$BODY$;


