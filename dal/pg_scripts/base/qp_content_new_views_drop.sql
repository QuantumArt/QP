
CREATE OR REPLACE PROCEDURE public.qp_content_new_views_drop(
    cid numeric
)

LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    is_user_query boolean;
	BEGIN
	    call qp_content_frontend_views_drop(cid, true);

        is_user_query := virtual_type = 3 from content where content_id = cid;
	    if is_user_query then
            sql := format('drop view if exists content_%s_united_new', cid);
            raise notice '%', sql;
            execute sql;
        else
            call qp_content_united_view_drop(cid, true);

            sql := format('drop view if exists content_%s_async_new', cid);
            raise notice '%', sql;
            execute sql;
        end if;

	    sql := format('drop view if exists content_%s_new', cid);
	    raise notice '%', sql;
	    execute sql;


	END;
$BODY$;



