CREATE OR REPLACE PROCEDURE qp_content_frontend_views_drop(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    new text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'drop view if exists content_%s_live%s';
	    sql := format(sql, cid, new);
	    execute sql;

        sql := 'drop view if exists content_%s_stage%s';
	    sql := format(sql, cid, new);
	    execute sql;
	END;
$BODY$;

