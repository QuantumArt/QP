
drop procedure if exists qp_content_new_views_create(numeric);

CREATE OR REPLACE PROCEDURE qp_content_new_views_create(
    cid numeric
)
    LANGUAGE 'plpgsql'

AS $BODY$
DECLARE
    use_native boolean;
BEGIN
    use_native := use_native_ef_types from content where content_id = cid;
    call qp_content_new_views_create_force_native(cid, use_native);
END;
$BODY$;


