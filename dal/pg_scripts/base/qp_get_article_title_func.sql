-- FUNCTION: public.qp_get_article_title_func(numeric, numeric)

-- DROP FUNCTION public.qp_get_article_title_func(numeric, numeric);

CREATE OR REPLACE FUNCTION public.qp_get_article_title_func(
	content_item_id numeric,
	content_id numeric)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    STABLE
AS $BODY$
DECLARE
    result text;
BEGIN
    result := null;

	select data into result
	from content_data cd
	where
	cd.content_item_id = $1
	and attribute_id in (
		select ca.attribute_id from content_attribute ca
		where ca.content_id = $2 and attribute_name = public.qp_get_display_field($2, true)
	);

	return result;
END;
$BODY$;

ALTER FUNCTION public.qp_get_article_title_func(numeric, numeric)
    OWNER TO postgres;
