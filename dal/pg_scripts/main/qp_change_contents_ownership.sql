CREATE OR REPLACE PROCEDURE qp_change_contents_ownership(username text)
LANGUAGE plpgsql
AS $BODY$
    DECLARE rec RECORD;
        DECLARE myrow record;
    BEGIN
        for myrow in
        select 'ALTER TABLE '|| tablename ||' OWNER TO "'|| username ||'";' as tableq
        from (select tablename from pg_catalog.pg_tables where 
			  tablename SIMILAR TO 'item_link_[\d]+[\w_]*'
			  OR tablename SIMILAR TO 'content_[\d]+[\w_]*'
			 ) t
        loop
            execute myrow.tableq;
        end loop;

        for myrow in
        select 'ALTER VIEW '|| viewname ||' OWNER TO "'|| username ||'";' as viewq
        from (select viewname from pg_catalog.pg_views where 
			  viewname SIMILAR TO 'item_link_[\d]+[\w_]+' 
			  OR viewname SIMILAR TO 'link_[\d]+[\w_]+'
			  OR viewname SIMILAR TO 'content_[\d]+[\w_]+'
			 ) v
        loop
            execute myrow.viewq;
        end loop;
        
    END;
$BODY$;