DROP FUNCTION IF EXISTS qp_count_duplicates(int, int[], int[], bool);

CREATE OR REPLACE function qp_count_duplicates(
    content_id int,
    field_ids int[],
    ids int[] default null,
    include_archive boolean default false,
    use_native_bool boolean default false
) returns int
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
        if use_native_bool then
            condition := condition || ' and not c.archive';
        else
            condition := condition || ' and c.archive = 0';
        end if;
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

