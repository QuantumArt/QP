create or replace function qp_default_link_ids(field_id numeric) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result int[];
BEGIN
    result := array_agg(a.article_id) from field_article_bind a where a.field_id = $1;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_default_link_ids(numeric) owner to postgres;