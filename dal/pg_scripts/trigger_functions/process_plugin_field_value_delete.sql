-- FUNCTION: public.process_plugin_field_value_delete()

-- DROP FUNCTION public.process_plugin_field_value_delete();

CREATE OR REPLACE FUNCTION public.process_plugin_field_value_delete()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
	DECLARE
	    plugin_ids int[];
	    processed int[];
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
    BEGIN

	    IF NOT EXISTS(SELECT * FROM information_schema.tables i where i.table_name = 'disable_td_plugin_field_value') THEN

            plugin_ids := COALESCE(array_agg(distinct(p.plugin_id)), ARRAY[]::int[]) from old_table d
                inner join plugin_field p on d.plugin_field_id = p.id;
            RAISE NOTICE 'plugin ids: %', plugin_ids;

            processed = ARRAY[]::int[];
            FOREACH pid in array plugin_ids
            LOOP
                plugin_field_ids := array_agg(distinct(p.id)) from plugin_field p where p.plugin_id = pid;
                RAISE NOTICE 'plugin field ids: %', plugin_field_ids;

                site_ids := array_agg(distinct(d.site_id)) from old_table d
                    where d.plugin_field_id = ANY(plugin_field_ids) and d.site_id is not null
                    and not exists(
                        select * from plugin_field_value v where d.site_id = v.site_id
                        and v.PLUGIN_FIELD_ID = ANY(plugin_field_ids)
                    );
                RAISE NOTICE 'site ids: %', site_ids;

                content_ids := array_agg(distinct(d.content_id)) from old_table d
                    where d.plugin_field_id = ANY(plugin_field_ids) and d.content_id is not null
                    and not exists(
                        select * from plugin_field_value v where d.content_id = v.content_id
                        and v.PLUGIN_FIELD_ID = ANY(plugin_field_ids)
                    );
                RAISE NOTICE 'content ids: %', content_ids;

                content_attribute_ids := array_agg(distinct(d.content_attribute_id)) from old_table d
                    where d.plugin_field_id = ANY(plugin_field_ids) and d.content_attribute_id is not null
                    and not exists(
                        select * from plugin_field_value v where d.content_attribute_id = v.content_attribute_id
                        and v.PLUGIN_FIELD_ID = ANY(plugin_field_ids)
                    );
                RAISE NOTICE 'field ids: %', content_attribute_ids;

                sql_template := 'delete from %s where id in (select id from unnest($1) i(id))';

                IF site_ids is not null THEN
                    table_name := 'plugin_site_' || pid;
                    sql := FORMAT(sql_template, table_name);
                    RAISE NOTICE '%', sql;
                    execute sql using site_ids;
                END IF;

                IF content_ids is not null THEN
                    table_name := 'plugin_content_' || pid;
                    sql := FORMAT(sql_template, table_name);
                    RAISE NOTICE '%', sql;
                    execute sql using content_ids;
                END IF;

                IF content_attribute_ids is not null THEN
                    table_name := 'plugin_content_attribute_' || pid;
                    sql := FORMAT(sql_template, table_name);
                    RAISE NOTICE '%', sql;
                    execute sql using content_attribute_ids;
                END IF;

                ids := array_agg(d.id) from old_table d where d.plugin_field_id = ANY(plugin_field_ids)
                and (d.site_id = ANY(site_ids) or d.content_id = ANY(content_ids)
                        or d.content_attribute_id = ANY(content_attribute_ids));
                processed := array_cat(processed, ids);
            END LOOP;

            Raise notice 'processed: %', processed;

            plugin_field_ids := COALESCE(array_agg(distinct(d.plugin_field_id)), ARRAY[]::int[])from old_table d
                                where NOT(d.id = ANY(processed));
            Raise notice 'plugin field ids: %', plugin_field_ids;

            FOREACH field_id in array plugin_field_ids
            LOOP
                ids := array_agg(coalesce(d.site_id, d.content_id, d.content_attribute_id)) from old_table d
                where d.plugin_field_id = field_id;
                RAISE NOTICE 'ids: %', ids;

                field := row(p.*) from plugin_field p where p.id = field_id;
                source := LOWER(field.relation_type);
                table_name := 'plugin_' || source || '_' || pid;
                sql_template := 'update %s p set "%s" = NULL from plugin_field_value v ' ||
                    'where v.id in (select id from unnest($1) i(id)) ';
                sql := FORMAT(sql_template, table_name, LOWER(field.name), source);
                RAISE NOTICE '%', sql;
                execute sql using ids;
            END LOOP;

	    END IF;

	    RETURN NULL;
	END;
$BODY$;



