CREATE OR REPLACE function public.qp_count_duplicates(content_id int, field_ids int[], ids int[] default null,
                                                       include_archive boolean default false) returns int
LANGUAGE 'plpgsql'

AS $BODY$
    DECLARE
        attr_names text[];
        attrs text;
        condition text;
        sql text;
        result int;
    BEGIN
        attr_names := array_agg('"' || lower(attribute_name) || '"') from content_attribute where attribute_id = ANY(field_ids);
        raise notice '%', attr_names;

        if ids is not null and array_length(ids, 1) > 0 then
            condition := 'and c.content_item_id = ANY($1)';
        else
            condition := '';
        end if;

        if not include_archive then
            condition := condition || ' and c.archive = 0';
        end if;

        if attr_names is not null then
            attrs := array_to_string(attr_names, ',');
            sql := 'select coalesce(sum(c0.cnt), 0) from (select COUNT(*) as cnt
                  from content_%s_united c where 1=1 %s group by %s having COUNT(*) > 1) as c0';
            sql := format(sql, content_id, condition, attrs);

        else
            sql := 'select 0 as cnt';
        end if;
        raise notice '%', sql;
        execute sql using ids into result;
        return result;

    END;
$BODY$;

