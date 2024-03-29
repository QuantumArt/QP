-- FUNCTION: qp_get_hash(text, bigint)

-- DROP FUNCTION qp_get_hash(text, bigint);

CREATE OR REPLACE FUNCTION qp_get_hash(
	text,
	bigint)
    RETURNS bytea
    LANGUAGE 'sql'

    COST 100
    IMMUTABLE STRICT
AS $BODY$
    SELECT digest($1 || $2::text, 'sha1')
$BODY$;


