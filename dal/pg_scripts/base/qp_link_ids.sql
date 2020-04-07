create or replace function qp_link_ids(link_id integer, id integer, is_stage boolean) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result int[];
BEGIN
    if is_stage then
        result := array_agg(i.linked_item_id) from item_link_united i where i.link_id = $1 and i.item_id = $2;
    else
        result := array_agg(i.linked_item_id) from item_link i where i.link_id = $1 and i.item_id = $2;
    end if;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_link_ids(int, int, boolean) owner to postgres;
