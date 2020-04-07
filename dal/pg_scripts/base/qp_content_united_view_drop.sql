
CREATE OR REPLACE PROCEDURE public.qp_content_united_view_drop(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  new text;
	  sql text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'drop view if exists content_%s_united%s';
	    sql := format(sql, cid, new);
	    execute sql;
	END;
$BODY$;

alter procedure qp_content_united_view_drop(numeric, boolean) owner to postgres;

