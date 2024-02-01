CREATE OR REPLACE PROCEDURE qp_content_table_change_column_types(
	cid numeric, is_async boolean, use_native boolean)
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	    attrs content_attribute[];
	    attr content_attribute;
	    field text;
	    change boolean;
		target_table text;
		sql_template text;
		queries text[];
		sql_query text;
	    type_name text;
	    int_type_name text;
	    bool_type_name text;
BEGIN
		target_table := case is_async when true then 'content_%s_async' else 'content_%s' end;
		target_table := format(target_table, cid);
		sql_template := 'alter table %s alter column %s type %s';

		int_type_name := case use_native when true then 'int' else 'numeric(18,0)' end;
		bool_type_name := case use_native when true then 'boolean using %s::int::boolean' else 'numeric(18,0) using %s::int::numeric' end;
		queries := array_append(queries, format(sql_template, target_table, 'content_item_id', int_type_name));
		queries := array_append(queries, format(sql_template, target_table, 'status_type_id', int_type_name));
		queries := array_append(queries, format(sql_template, target_table, 'last_modified_by', int_type_name));
		queries := array_append(queries, format(sql_template, target_table, 'archive', format(bool_type_name, 'archive')));
		queries := array_append(queries, format(sql_template, target_table, 'visible', format(bool_type_name, 'visible')));

		foreach sql_query in ARRAY queries
		loop
    	    raise notice '%', sql_query;
            execute sql_query;
        end loop;

        attrs := array_agg(ca.* order by attribute_order) from content_attribute ca where content_id = cid;
	    if attrs is not null THEN
            foreach attr in ARRAY attrs
            loop
                change := true;
                field := '"' || lower(attr.attribute_name) || '"';
                if attr.attribute_type_id = 2 and attr.attribute_size = 0 and attr.is_long then
                    type_name := case use_native when true then 'bigint' else 'numeric(18,0)'  end;
                elseif attr.attribute_type_id = 11 or attr.attribute_type_id = 13
                    or attr.attribute_type_id = 2 and attr.attribute_size = 0 and not attr.is_long then
                    type_name := case use_native when true then 'int' else 'numeric(18,0)' end;
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and not attr.is_long then
                    type_name := case use_native when true then 'float' else format('numeric(18, %s)', attr.attribute_size) end;
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and attr.is_long then
                    type_name := format('numeric(18, %s)', attr.attribute_size);
                elseif attr.attribute_type_id = 3 then
                    type_name := case use_native when true then 'boolean using %s::int::boolean' else 'numeric(18,0) using %s::int::numeric' end;
                    type_name := format(type_name, field);
                elseif attr.attribute_type_id = 4 then
                    type_name := case use_native when true then 'date' else 'timestamptz' end;
                elseif attr.attribute_type_id = 5 then
                    type_name := case use_native when true then 'timetz' else 'timestamptz' end;
                else
                    change := false;
                end if;

                if change then
                    sql_query := format(sql_template, target_table, field, type_name);
	    			raise notice '%', sql_query;
                    execute sql_query;
                end if;
            end loop;
        end if;
    END;
$BODY$;