create or replace function qp_get_m2o_ids(content_id integer, field_name text, id integer default NULL)
    returns int[]
    called on null input
    language plpgsql
as
$$
    DECLARE
        condition text;
        sql text;
        result int[];
    BEGIN
        condition := case when id is null THEN ' is null' ELSE ' = ' || id::text end;
        sql := 'select array_agg(content_item_id) from content_%s where "%s" %s';
        sql := format(sql, content_id, lower(field_name), condition);
        execute sql into result;
        return result;
    END;
$$;


