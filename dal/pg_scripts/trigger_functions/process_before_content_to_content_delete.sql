create or replace function process_before_content_to_content_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.LINK_ID]::int[];
        call qp_before_content_to_content_delete(ids);
		RETURN OLD;
	END;
$$;


