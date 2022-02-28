-- FUNCTION: public.qp_get_display_fields(numeric, boolean)

-- DROP FUNCTION public.qp_get_display_fields(numeric, boolean);

CREATE OR REPLACE FUNCTION public.qp_get_display_fields(
	content_id numeric,
	with_relation_field boolean DEFAULT false)
    RETURNS content_attribute[]
    LANGUAGE 'plpgsql'

    COST 100
    STABLE
AS $BODY$
DECLARE
    result content_attribute[];
BEGIN
    result := ARRAY(
            SELECT ca
            FROM (
                     select ca,
                            CASE
                                WHEN attribute_type_id in (9, 10)
                                    THEN CASE WHEN with_relation_field THEN 1 ELSE 0 END
                                WHEN attribute_type_id = 13 or IS_CLASSIFIER or
                                     attribute_type_id = 11 AND NOT with_relation_field
                                    THEN -1
                                ELSE 1
                                END AS attribute_priority
                     from content_attribute ca
                     where ca.content_id = $1
                 ) i
            where attribute_priority >= 0
        );
    return result;
END;
$BODY$;


