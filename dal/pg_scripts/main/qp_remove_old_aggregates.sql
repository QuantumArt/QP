create or replace procedure qp_remove_old_aggregates(ids integer[])
    language plpgsql
as
$$
    BEGIN
        delete from content_item where content_item_id in (select id from qp_aggregates_to_remove(ids) i(id));
	END;
$$;

alter procedure qp_remove_old_aggregates(integer[]) owner to postgres;