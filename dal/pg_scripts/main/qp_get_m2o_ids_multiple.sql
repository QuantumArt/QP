create or replace function qp_get_m2o_ids_multiple(content_id integer, field_name text, ids integer[])
    returns link[]
    language plpgsql
as
$$
    DECLARE
        sql text;
        result link[];
    BEGIN
        sql := 'select array_agg(row("%s", content_item_id)) from content_%s where "%s" = ANY($1)';
        sql := format(sql, lower(field_name), content_id, lower(field_name));
        execute sql into result using ids;
        return result;
    END;
$$;


alter function qp_get_m2o_ids_multiple(integer, text, integer[]) owner to postgres;

