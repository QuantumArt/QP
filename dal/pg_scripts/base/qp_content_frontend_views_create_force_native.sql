CREATE OR REPLACE PROCEDURE qp_content_frontend_views_create_force_native(
    cid numeric,
    is_new boolean,
    use_native boolean
)
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    sql text;
    new text;
    cond text;
BEGIN
    new := case when is_new then '_new' else '' end;
    cond := case when is_new or use_native then 'visible and not archive' else 'visible = 1 and archive = 0' end;
    sql := 'create or replace view content_%s_live%s as
               select * from content_%s%s where %s
               and status_type_id in ( select status_type_id from status_type where status_type_name = ''Published'')';
    sql := format(sql, cid, new, cid, new, cond);
    raise notice '%', sql;

    execute sql;

    sql := 'create or replace view content_%s_stage%s as
               select * from content_%s_united%s where %s';
    sql := format(sql, cid, new, cid, new, cond);
    raise notice '%', sql;
    execute sql;
END;
$BODY$;


