create or replace function qp_link_titles(
    link_id integer, id integer, display_attribute_id integer, maxlength integer
) returns text
    immutable
    language plpgsql
as
$$
DECLARE
    result text[];
BEGIN

    	result := array_agg(
    	    case when char_length(d.title) > maxlength then left(d.title, maxlength) || '...'
    	    else d.title end
    	) from
        (select coalesce(data, blob_data, '') as title from content_data
    	where attribute_id = $3 and content_item_id in (
    	    select linked_item_id from item_link i where i.link_id = $1 and i.item_id = $2
    	)) d;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_link_titles(int, int, int, int) owner to postgres;
