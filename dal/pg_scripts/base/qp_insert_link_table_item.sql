-- PROCEDURE: qp_insert_link_table_item(numeric, numeric, link[], boolean, boolean, boolean)

-- DROP PROCEDURE qp_insert_link_table_item(numeric, numeric, link[], boolean, boolean, boolean);

CREATE OR REPLACE PROCEDURE qp_insert_link_table_item(
	link_id numeric,
	content_id numeric,
	links link[],
	is_async boolean,
	use_reverse_table boolean,
	reverse_fields boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  link_table_name text;
	  source_name text;
	  is_self boolean;
	  rev_fields text;
	  condition text;
	  sql text;
	  fsql text;
	  links2 link[];
	  t link;
	BEGIN
		link_table_name := 'item_link_' || $1::text;
		is_self := l_content_id = r_content_id from content_to_content cc where cc.link_id = $1;
		source_name := CASE WHEN is_async THEN 'item_link_async' ELSE 'item_link' END;
		rev_fields := CASE WHEN reverse_fields THEN 'il.linked_id, il.id' ELSE 'il.id, il.linked_id' END;
		condition := CASE WHEN reverse_fields THEN 'il2.id = il.linked_id and il2.linked_id = il.id' ELSE 'il2.id = il.id and il2.linked_id = il.linked_id' END;

		IF is_async THEN
			link_table_name := link_table_name || '_async';
		END IF;

		IF use_reverse_table THEN
			link_table_name := link_table_name || '_rev';
		END IF;

		links2 := array(
			select il from unnest(links) il inner join content_item ci on il.id = ci.CONTENT_ITEM_ID where ci.CONTENT_ID = $2
		);

		foreach t in array links2
		loop
			raise notice '%', t;
		end loop;

		sql := 'insert into %s select %s from unnest($1) il where not exists(select * from %s il2 where %s)';
		fsql := format(sql, link_table_name, rev_fields, link_table_name, condition);
		EXECUTE fsql USING links2;
		RAISE NOTICE 'Query: %', fsql;


		IF is_self THEN
	    	sql := 'update %s i set is_self = true from unnest($1) i2 where i.link_id = $2 and i.item_id = i2.id and i.linked_item_id = i2.linked_id';
			fsql := format(sql, source_name, source_name);
			EXECUTE fsql USING links2, link_id;
			RAISE NOTICE 'Query: %', fsql;
		END IF;

		IF use_reverse_table and not is_self THEN
	    	sql := 'update %s i set is_rev = true from unnest($1) i2 where i.link_id = $2 and i.item_id = i2.id and i.linked_item_id = i2.linked_id';
			fsql := format(sql, source_name, source_name);
			EXECUTE fsql USING links2, link_id;
			RAISE NOTICE 'Query: %', fsql;
		END IF;

	END;
$BODY$;
