
CREATE OR REPLACE PROCEDURE public.qp_content_united_view_create(
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
        sql := 'create or replace view content_%s_united%s as
	           select c1.* from content_%s%s c1
	           left join content_%s_async%s c2 on c1.content_item_id = c2.content_item_id
	           where c2.content_item_id is null
	           union all
	           select * from content_%s_async%s';
	    sql := format(sql, cid, new, cid, new, cid, new, cid, new);
	    execute sql;
	END;
$BODY$;


