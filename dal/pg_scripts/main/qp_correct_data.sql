create or replace function qp_correct_data(value text, type_id numeric, length numeric, default_value text) returns text
    immutable
    called on null input
    language plpgsql
as
$$
DECLARE
	num numeric(18, 0);
BEGIN
	IF type_id in (1, 7, 8, 12) THEN
		RETURN left(value, length::int);
	ELSEIF type_id in (2, 3, 11) THEN
		IF qp_is_numeric(value) or value is null THEN
			RETURN value;
		ELSEIF qp_is_numeric(default_value) THEN
			RETURN default_value;
		ELSE
			RETURN NULL;
		END IF;			
	ELSEIF type_id in (4, 5, 6) THEN
		IF qp_is_date(value) or value is null THEN
			RETURN value;
		ELSEIF qp_is_date(default_value) THEN
			RETURN default_value;
		ELSE 
			RETURN NULL;
		END IF;			
	ELSE	
		RETURN value;
	END IF;
END;
$$;


alter function qp_correct_data(text, numeric, numeric, text) owner to postgres;

