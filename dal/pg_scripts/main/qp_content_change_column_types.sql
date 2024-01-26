CREATE OR REPLACE PROCEDURE qp_content_change_column_types(cid numeric)
LANGUAGE 'plpgsql'
AS $BODY$
    BEGIN
        call qp_content_new_views_drop(cid);
        call qp_content_frontend_views_drop(cid);
        call qp_content_united_view_drop(cid);
        call qp_content_table_change_column_types(cid, true);
        call qp_content_table_change_column_types(cid, false);
        call qp_content_united_view_create(cid);
        call qp_content_frontend_views_create(cid);
        call qp_content_new_views_create(cid);
    END;
$BODY$;