-- PROCEDURE: qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[])

-- DROP PROCEDURE qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]);

CREATE OR REPLACE PROCEDURE qp_update_items_flags(
    content_id integer,
    ids integer[],
    is_async boolean)
LANGUAGE 'plpgsql'

AS $BODY$
    DECLARE
        table_name text;
        use_bool bool;
        conversion text;
        sql text;
    BEGIN

        use_bool := use_native_ef_types from content c where c.content_id = qp_update_items_flags.content_id;

        table_name := 'content_' || content_id;
        IF is_async THEN
            table_name := table_name || '_async';
        END IF;

        conversion := '';
        if use_bool THEN
            conversion := '::int::bool';
        end if;


        sql := 'update %s base set visible = ci.visible%s, archive = ci.archive%s from content_item ci
         where base.content_item_id = ci.content_item_id and ci.content_item_id = ANY($1)';

        sql := FORMAT(sql, table_name, conversion, conversion);
        RAISE NOTICE '%', sql;
        execute sql using ids;
    END;
$BODY$;

