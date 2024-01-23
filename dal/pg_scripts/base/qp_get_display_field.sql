-- FUNCTION: qp_get_display_field(numeric, boolean)

-- DROP FUNCTION qp_get_display_field(numeric, boolean);

CREATE OR REPLACE FUNCTION qp_get_display_field(
	content_id numeric,
	with_relation_field boolean DEFAULT false)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    STABLE
AS $BODY$
DECLARE
    result text;
BEGIN
    SELECT attribute_name into result
FROM (
         select attribute_name,
                CASE
                    WHEN attribute_type_id in (9, 10)
                        THEN CASE WHEN with_relation_field THEN 1 ELSE 0 END
                    WHEN attribute_type_id = 13 or IS_CLASSIFIER or
                         attribute_type_id = 11 AND NOT with_relation_field
                        THEN -1
                    ELSE 1
                    END AS attribute_priority
         from unnest(qp_get_display_fields(content_id, with_relation_field))
         order by view_in_list desc, attribute_priority desc, attribute_order asc
     ) a
LIMIT 1;

if result is null then
	return 'content_item_id';
else
	return result;
end if;

END;
$BODY$;


