
CREATE OR REPLACE PROCEDURE qp_content_new_views_drop(
    cid numeric
)

LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	BEGIN
	    call qp_content_frontend_views_drop(cid, true);
	    call qp_content_united_view_drop(cid, true);

        sql := format('drop view if exists content_%s_new', cid);
	    raise notice '%', sql;
	    execute sql;

        sql := format('drop view if exists content_%s_async_new', cid);
	    raise notice '%', sql;
	    execute sql;

	END;
$BODY$;


