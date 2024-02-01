
CREATE OR REPLACE PROCEDURE qp_content_frontend_views_create(
    cid numeric,
    is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
    DECLARE
        use_native boolean;
    BEGIN
        use_native := use_native_ef_types from content where content_id = cid;
        call qp_content_frontend_views_create_force_native(cid, is_new, use_native);
    END;
$BODY$;


