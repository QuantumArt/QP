-- PROCEDURE: public.qp_delete_link_table_item(numeric, numeric, link[], boolean, boolean, boolean)

-- DROP PROCEDURE public.qp_delete_link_table_item(numeric, numeric, link[], boolean, boolean, boolean);

CREATE OR REPLACE PROCEDURE public.qp_delete_link_table_item(
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
	  condition text;
	  sql text;
	  fsql text;
	BEGIN
		link_table_name := 'item_link_' || $1::text;
		condition := CASE WHEN reverse_fields THEN 'src.id = il.linked_id and src.linked_id = il.id' ELSE 'src.id = il.id and src.linked_id = il.linked_id' END;

		IF is_async THEN
			link_table_name := link_table_name || '_async';
		END IF;

		IF use_reverse_table THEN
			link_table_name := link_table_name || '_rev';
		END IF;

		sql := 'delete from %s src using unnest($1) il where %s';
		fsql := format(sql, link_table_name, condition);
		EXECUTE fsql USING links;
		RAISE NOTICE 'Query: %', fsql;

	END;
$BODY$;
