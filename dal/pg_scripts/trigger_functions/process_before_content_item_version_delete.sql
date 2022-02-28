create or replace function process_before_content_item_version_delete() returns trigger
    language plpgsql
as
$$
DECLARE
		ids int[];
    BEGIN
		ids := ARRAY[OLD.content_item_version_id]::int[];
		call qp_before_content_item_version_delete(ids);
		RETURN OLD;
	END;
$$;



