-- FUNCTION: public.qp_get_version_data(numeric, numeric)

-- DROP FUNCTION public.qp_get_version_data(numeric, numeric);

CREATE OR REPLACE FUNCTION public.qp_get_version_data(
	attribute_id numeric,
	version_id numeric)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$
declare result text;
begin
	result := coalesce(vcd.blob_data, vcd.data) from version_content_data vcd where vcd.attribute_id = $1 and vcd.content_item_version_id = $2;
	return result;
end;
$BODY$;


