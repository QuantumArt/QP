
CREATE OR REPLACE PROCEDURE public.qp_link_view_drop(
	id int
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        view_name text;
	    view_name2 text;
	    view_name3 text;
	    view_name4 text;
	    sql text;

	BEGIN
	    view_name := 'link_' || id;
	    view_name2 := 'link_' || id || '_united';
	    view_name3 := 'item_link_' || id || '_united';
	    view_name4 := 'item_link_' || id || '_united_rev';

        sql := 'drop VIEW IF EXISTS %s ';

	    execute format(sql, view_name);
	    execute format(sql, view_name2);
	    execute format(sql, view_name3);
	    execute format(sql, view_name4);

	END;
$BODY$;

alter procedure qp_link_view_drop(int) owner to postgres;