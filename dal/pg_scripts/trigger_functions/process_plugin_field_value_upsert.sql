-- FUNCTION: process_plugin_field_value_upsert()

-- DROP FUNCTION process_plugin_field_value_upsert();

CREATE OR REPLACE FUNCTION process_plugin_field_value_upsert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
	DECLARE
	    plugin_ids int[];
	    plugin_field_ids int[];
	    site_ids int[];
	    content_ids int[];
	    content_attribute_ids int[];
	    ids int[];
        field plugin_field;
		pid int;
	    field_id int;
		source text;
		sql_template text;
	    sql text;
	    table_name text;
	    column_type text;
    BEGIN
		plugin_ids := COALESCE(array_agg(distinct(p.plugin_id)), ARRAY[]::int[]) from new_table i
		    inner join plugin_field p on i.plugin_field_id = p.id;
		RAISE NOTICE 'plugin ids: %', plugin_ids;

        FOREACH pid in array plugin_ids
		LOOP
            plugin_field_ids := array_agg(distinct(i.plugin_field_id)) from new_table i
                inner join plugin_field p on i.plugin_field_id = p.id
                where p.plugin_id = pid;

            site_ids := array_agg(distinct(i.site_id)) from new_table i
                where i.plugin_field_id = ANY(plugin_field_ids) and i.site_id is not null;
            RAISE NOTICE 'site ids: %', site_ids;

            content_ids := array_agg(distinct(i.content_id)) from new_table i
                where i.plugin_field_id = ANY(plugin_field_ids) and i.content_id is not null;
            RAISE NOTICE 'content ids: %', content_ids;

            content_attribute_ids := array_agg(distinct(i.content_attribute_id)) from new_table i
                where i.plugin_field_id = ANY(plugin_field_ids) and i.content_attribute_id is not null;
            RAISE NOTICE 'field ids: %', content_attribute_ids;

			sql_template := 'insert into %s(id) select id from unnest($1) i(id) ' ||
			       'where not exists(select * from %s i2 where i.id = i2.id)';

            IF site_ids is not null THEN
                table_name := 'plugin_site_' || pid;
                sql := FORMAT(sql_template, table_name, table_name);
                RAISE NOTICE '%', sql;
                execute sql using site_ids;
            END IF;

            IF content_ids is not null THEN
                table_name := 'plugin_content_' || pid;
                sql := FORMAT(sql_template, table_name, table_name);
			    RAISE NOTICE '%', sql;
                execute sql using content_ids;
            END IF;

            IF content_attribute_ids is not null THEN
                table_name := 'plugin_content_attribute_' || pid;
                sql := FORMAT(sql_template, table_name, table_name);
			    RAISE NOTICE '%', sql;
                execute sql using content_attribute_ids;
            END IF;

        END LOOP;

		plugin_field_ids := COALESCE(array_agg(distinct(plugin_field_id)), ARRAY[]::int[]) from new_table;
		Raise notice 'plugin field ids: %', plugin_field_ids;

		FOREACH field_id in array plugin_field_ids
		LOOP
		    ids := array_agg(i.id) from new_table i where i.plugin_field_id = plugin_field_id;
            RAISE NOTICE 'ids: %', ids;
		    field := row(p.*) from plugin_field p where p.id = field_id;
		    column_type := LOWER(field.value_type);
		    column_type := CASE
		        WHEN column_type = 'bool' THEN 'boolean'
		        WHEN column_type = 'string' THEN 'text'
		        WHEN column_type = 'datetime' THEN 'timestamp with time zone'
		        ELSE column_type
		    END;
		    source := LOWER(field.relation_type);
            table_name := 'plugin_' || source || '_' || pid;
            sql_template := 'update %s p set "%s" = v.value::%s from plugin_field_value v ' ||
                'where p.id = v.%s_id and v.id in (select id from unnest($1) i(id)) ';

            sql := FORMAT(sql_template, table_name, LOWER(field.name), column_type, source);
			RAISE NOTICE '%', sql;
            execute sql using ids;
        END LOOP;

		RETURN NULL;
	END
$BODY$;

