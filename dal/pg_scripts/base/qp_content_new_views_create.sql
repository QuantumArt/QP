
CREATE OR REPLACE PROCEDURE public.qp_content_new_views_create(
    cid numeric
)

LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    attrs content_attribute[];
	    attr content_attribute;
	    field text;
	    fields text[];
	    do_cast boolean;
	    field_template text;
	    type_name text;
	    is_user_query boolean;
	BEGIN
	    fields := ARRAY[
	        'coalesce(CONTENT_ITEM_ID::int, 0) as CONTENT_ITEM_ID',
	        'coalesce(STATUS_TYPE_ID::int, 0) as STATUS_TYPE_ID',
	        'coalesce(VISIBLE::int::boolean, false) as VISIBLE',
	        'coalesce(ARCHIVE::int::boolean, false) as ARCHIVE',
	        'CREATED',
	        'MODIFIED',
	        'coalesce(LAST_MODIFIED_BY::int, 0) as LAST_MODIFIED_BY'
	    ];
        is_user_query := virtual_type = 3 from content where content_id = cid;
        attrs := array_agg(ca.* order by attribute_order) from content_attribute ca where content_id = cid;
	    if attrs is not null THEN
            foreach attr in ARRAY attrs
            loop
                do_cast := true;
                field_template = 'cast(%s as %s) as %s';
                field := '"' || lower(attr.attribute_name) || '"';
                if attr.attribute_type_id = 2 and attr.attribute_size = 0 and attr.is_long then
                    type_name := 'bigint';
                elseif attr.attribute_type_id = 11 or attr.attribute_type_id = 13
                    or attr.attribute_type_id = 2 and attr.attribute_size = 0 and not attr.is_long then
                    type_name := 'int';
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and not attr.is_long then
                    type_name := 'float';
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and attr.is_long then
                    type_name := format('decimal(18, %s)', attr.attribute_size);
                elseif attr.attribute_type_id = 3 then
                    type_name := 'boolean';
                    field_template := 'cast(coalesce(%s::int, 0) as %s) as %s';
                elseif attr.attribute_type_id = 4 then
                    type_name := 'date';
                elseif attr.attribute_type_id = 5 then
                    type_name := 'time';
                elseif attr.attribute_type_id in (9,10) then
                    type_name := 'text';
                else
                    do_cast := false;
                end if;

                if do_cast then
                    field := format(field_template, field, type_name, field);
                end if;
                fields := array_append(fields, field);
            end loop;
        end if;


        sql := 'create view content_%s_new as select %s from content_%s';
	    sql := format(sql, cid, array_to_string(fields, ', '), cid);
	    raise notice '%', sql;
	    execute sql;

	    if is_user_query then
            sql := 'create view content_%s_united_new as select %s from content_%s_united';
            sql := format(sql, cid, array_to_string(fields, ', '), cid);
            raise notice '%', sql;
            execute sql;
        else
            sql := 'create view content_%s_async_new as select %s from content_%s_async';
            sql := format(sql, cid, array_to_string(fields, ', '), cid);
            raise notice '%', sql;
            execute sql;

            call qp_content_united_view_create(cid, true);
        end if;

	    call qp_content_frontend_views_create(cid, true);

	END;
$BODY$;

alter procedure qp_content_new_views_create(numeric) owner to postgres;

