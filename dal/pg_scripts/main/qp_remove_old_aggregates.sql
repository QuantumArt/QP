create or replace procedure qp_remove_old_aggregates(ids integer[])
    language plpgsql
as
$$
    BEGIN
        delete from content_item where content_item_id = ANY(qp_aggregates_to_remove(ids));
	END;
$$;
