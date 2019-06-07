
CREATE OR REPLACE PROCEDURE public.qp_content_frontend_views_create(
	cid numeric,
	is_new boolean
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    new text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'create view content_%s_live%s as
               select * from content_%s%s where visible and not archive
               and status_type_id in ( select status_type_id from status_type where status_type_name = ''Published'')';
	     sql := format(sql, cid, new, cid, new);
	    execute sql;

         sql := 'create view content_%s_stage%s as
               select * from content_%s_united%s where visible and not archive';
	     sql := format(sql, cid, new, cid, new);
	     execute sql;
	END;
$BODY$;

alter procedure qp_content_frontend_views_create(numeric, boolean) owner to postgres;

