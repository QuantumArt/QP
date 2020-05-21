CREATE OR REPLACE PROCEDURE public.qp_copy_schedule_to_child_delays(id int)
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
        if exists(select * from content_item_schedule where content_item_id = $1 and freq_type = 2) then
            update content_item_schedule set delete_job = true where not use_service and content_item_id in (
                select child_id from child_delays cd where cd.id = $1
            );
            delete from content_item_schedule where content_item_id in (
                select child_id from child_delays cd where cd.id = $1
            );
            insert into content_item_schedule (
                CONTENT_ITEM_ID, MAXIMUM_OCCURENCES, CREATED, MODIFIED, LAST_MODIFIED_BY, freq_type,
                freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor,
                active_start_date, active_end_date, active_start_time, active_end_time,
                occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE, start_date, end_date
            )
            select child_id, MAXIMUM_OCCURENCES, now(), now(), LAST_MODIFIED_BY, freq_type,
                freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor,
                active_start_date, active_end_date, active_start_time, active_end_time,
                occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE, start_date, end_date
            from content_item_schedule cis inner join child_delays cd on cis.content_item_id = cd.id
            where content_item_id = $1;
        end if;
    END;
$BODY$;

ALTER PROCEDURE public.qp_copy_schedule_to_child_delays(int)
    OWNER TO postgres;

