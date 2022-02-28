create or replace procedure qp_upsert_items(content_id integer, ids integer[], delayed_ids integer[], none_id integer, is_async boolean)
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

	    sql := 'update %s base set visible = ci.visible, archive = ci.archive,
    			modified = ci.modified, last_modified_by = ci.last_modified_by, status_type_id = ci.status_type_id
			 	from content_item ci
		 		where base.content_item_id = ci.content_item_id and ci.content_item_id = ANY($1)';

		sql := FORMAT(sql, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;

		sql := 'insert into %s (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)
    			select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by,
    			case when i2.id is not null then $3 else ci.status_type_id end as status_type_id,
    			ci.visible, ci.archive
				from content_item ci left join %s base on ci.content_item_id = base.content_item_id
    			inner join unnest($1) i(id) on ci.content_item_id = i.id
				left join unnest($2) i2(id) on ci.content_item_id = i2.id
    			where base.content_item_id is null';
		sql := FORMAT(sql, table_name, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids, delayed_ids, none_id;
	END;
$$;
