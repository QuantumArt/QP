
CREATE OR REPLACE PROCEDURE public.qp_link_view_create(
	id int
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    link content_to_content;
        view_name text;
	    view_name2 text;
	    view_name3 text;
	    view_name4 text;
        link_table text;
	    link_table_async text;
	    link_table_rev text;
	    link_table_async_rev text;
	    sql text;
	    sql2 text;

	BEGIN
	    select cc.* into link from content_to_content cc where cc.link_id = id;
	    view_name := 'link_' || id;
	    view_name2 := 'link_' || id || '_united';
	    view_name3 := 'item_link_' || id || '_united';
	    view_name4 := 'item_link_' || id || '_united_rev';

	    link_table := 'item_link_' || id;
	    link_table_async := 'item_link_' || id || '_async';
	    link_table_rev := 'item_link_' || id || '_rev';
	    link_table_async_rev := 'item_link_' || id || '_async_rev';

        sql2 := 'CREATE VIEW %s AS select id, linked_id from %s il
             where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID)
             union all SELECT id, linked_id from %s ila';

        sql := 'CREATE VIEW %s AS select il.item_id, il.linked_item_id from %s il
               inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
               where CONTENT_ID = %s  and link_id = %s';

	    execute format(sql, view_name, 'item_link', link.l_content_id, link.link_id);
	    execute format(sql, view_name2, 'item_link_united', link.l_content_id, link.link_id);
	    execute format(sql2, view_name3, link_table, link_table_async);
	    execute format(sql2, view_name4, link_table_rev, link_table_async_rev);

	END;
$BODY$;

alter procedure qp_link_view_create(int) owner to postgres;