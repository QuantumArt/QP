
CREATE OR REPLACE PROCEDURE qp_content_united_views_recreate()
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  content_ids int[];
	  cid int;
	BEGIN
	    content_ids := array_agg(content_id) from content where virtual_type = 0;
	    foreach cid in array content_ids
	    loop
            call qp_content_united_view_drop(cid);
            call qp_content_united_view_create(cid);
        end loop;
    END;
$BODY$;



