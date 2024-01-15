
CREATE OR REPLACE PROCEDURE qp_content_user_query_view_recreate(
    cid integer
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    view_sql text;
	    alt_view_sql text;
	    sql text;

	BEGIN
	    select query, coalesce(alt_query, query)
	    into view_sql, alt_view_sql from content where content_id = cid;

	    sql := format('drop view if exists content_%s', cid);
	    execute sql;
	    sql := format('create view content_%s as %s', cid, view_sql);
	    execute sql;

	    sql := format('drop view if exists content_%s_united', cid);
	    execute sql;
	    sql := format('create view content_%s_united as %s', cid, alt_view_sql);
	    execute sql;
    END;
$BODY$;

