create or replace function process_before_content_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.CONTENT_ID]::int[];
        call qp_before_content_delete(ids);
		RETURN OLD;
	END;
$$;

alter function process_before_content_delete() owner to postgres;
