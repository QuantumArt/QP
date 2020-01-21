CREATE OR REPLACE PROCEDURE public.qp_change_timestamp_zone_time(
	table_name text,
	column_name text,
	use_timezone boolean default true
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    time_sql text;
	    sql text;
	BEGIN
	    time_sql := case when use_timezone then 'with' else 'without' end;
        sql := 'alter table %s alter column %s type timestamp %s time zone';
	    sql := format(sql, table_name, column_name, time_sql);
	    execute sql;
	END;
$BODY$;

alter procedure qp_change_timestamp_zone_time(text, text, boolean) owner to postgres;