create or replace function qp_is_numeric(text) returns boolean
    immutable
    strict
    language plpgsql
as
$$
DECLARE x NUMERIC;
BEGIN
    x = $1::NUMERIC;
    RETURN TRUE;
EXCEPTION WHEN others THEN
    RETURN FALSE;
END;
$$;


