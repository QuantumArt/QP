create or replace function process_before_content_item_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.CONTENT_ITEM_ID]::int[];
        call qp_before_content_item_delete(ids);
		RETURN OLD;
	END;
$$;

