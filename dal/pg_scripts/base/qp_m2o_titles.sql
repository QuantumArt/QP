create or replace function qp_m2o_titles(
    id integer, field_related_id integer, related_attribute_id integer, maxlength integer
) returns text
    stable
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
        (
            select coalesce(data, blob_data, '') as title from CONTENT_DATA where attribute_id = $2
	        and content_item_id in (select content_item_id from content_data where ATTRIBUTE_ID = $3 and o2m_data = $1)
    	) d;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_m2o_titles(int, int, int, int) owner to postgres;
