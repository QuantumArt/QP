
CREATE OR REPLACE FUNCTION public.qp_get_article_tsvector(id int)
RETURNS tsvector
    STABLE
    LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    result tsvector;
	    data content_data[];
	    data_item content_data;
    BEGIN
	    result := '' as tsvector;
	    data = array_agg(cd.* order by attribute_id asc) from content_data cd
	    where cd.content_item_id = $1 and cd.ft_data is not null;

	    data = coalesce(data, ARRAY[]::content_data[]);

	    foreach data_item in array data
	    loop
            result := result || data_item.ft_data;
        end loop;

	    return result;

	END;
$BODY$;







