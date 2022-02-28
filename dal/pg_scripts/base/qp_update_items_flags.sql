-- PROCEDURE: public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[])

-- DROP PROCEDURE public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]);

CREATE OR REPLACE PROCEDURE public.qp_update_items_flags(
	content_id integer,
	ids integer[],
	is_async boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  	table_name text;
		sql text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;

	    sql := 'update %s base set visible = ci.visible, archive = ci.archive from content_item ci
		 where base.content_item_id = ci.content_item_id and ci.content_item_id = ANY($1)';

		sql := FORMAT(sql, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;
	END;
$BODY$;

