create or replace function qp_get_aggregated_ids(id integer, classifier_ids integer[], content_ids integer[], is_live boolean)
returns integer[]
    immutable
    language plpgsql
as
$$
DECLARE
    result int[];
    agg_attrs content_attribute[];
    attr content_attribute;
    queries text[];
    sql text;
    live_suffix text;
BEGIN

    sql := '';
    live_suffix := case when is_live then '' else '_united' end;

    agg_attrs := array_agg(ca.*) from content_attribute ca
    where ca.classifier_attribute_id = ANY($2) and ca.content_id = ANY($3);

    if agg_attrs is not null then
        foreach attr in array agg_attrs
        loop
          sql := 'select content_item_id from content_%s%s where "%s" = $1';
          sql := format(sql, attr.content_id, live_suffix, lower(attr.attribute_name));
          queries := array_append(queries, sql);
        end loop;
        sql := array_to_string(queries, ' union all ');
        sql := 'select array_agg(u.content_item_id) from (' || sql || ') u';
        raise notice '%', sql;
        execute sql using $1 into result;
    end if;

    return coalesce(result, ARRAY[]::int[]);
END;
$$;

alter function qp_get_aggregated_ids(int, int[], int[], boolean) owner to postgres;