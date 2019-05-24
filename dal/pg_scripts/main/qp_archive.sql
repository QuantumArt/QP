
CREATE OR REPLACE PROCEDURE public.qp_archive(
	ids int[],
	flag boolean,
	last_modified_by int,
	with_aggregated boolean default true)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
    BEGIN
        if with_aggregated then
            ids := qp_aggregated_and_self(ids);
        end if;

        
        update content_item set archive = $2::int, modified = now(), last_modified_by = $3 where content_item_id = ANY(ids);
        update content_item set locked_by = null, locked = null where content_item_id = ANY(ids);
	END;
$BODY$;

alter procedure qp_archive(int[], boolean, int, boolean) owner to postgres;





