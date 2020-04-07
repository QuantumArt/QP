create or replace procedure qp_delete_items(content_id integer, ids integer[], is_async boolean)
    language plpgsql
as
$$
DECLARE
	  	table_name text;	  
		sql text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;
		
		sql := FORMAT('delete from %s where content_item_id = ANY($1)', table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;
	END;
$$;

alter procedure qp_delete_items(integer, integer[], boolean) owner to postgres;

