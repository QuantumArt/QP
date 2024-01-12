
CREATE OR REPLACE PROCEDURE qp_publish(
	ids int[],
	last_modified_by int,
	with_aggregated boolean default true)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        status numeric;
	    ids2 int[];
    BEGIN
        if with_aggregated then
            ids := qp_aggregated_and_self(ids);
        end if;

        status := status_type_id from status_type
        where status_type_name = 'Published' and site_id in (
            select site_id from content c inner join content_item ci on c.content_id = ci.content_id
            where content_item_id = ANY(ids)
        );

        update content_item set status_type_id = status, modified = now(), last_modified_by = $2
        where content_item_id = ANY(ids) and status_type_id <> status and not splitted;

        update content_item set status_type_id = status, modified = now(), last_modified_by = $2 ,
                                schedule_new_version_publication = true
        where content_item_id = ANY(ids) and status_type_id <> status and splitted;

        call qp_merge_delays(ids, last_modified_by);

        ids2 := array_agg(ci.content_item_id) from content_item ci
        where ci.content_item_id = ANY(ids) and not ci.splitted and not ci.schedule_new_version_publication;

        if ids2 is not null then
            ids := ids - ids2;
        end if;

        if array_length(ids, 1) > 0 then
            call qp_merge_articles(ids, last_modified_by);
        end if;


	END;
$BODY$;





