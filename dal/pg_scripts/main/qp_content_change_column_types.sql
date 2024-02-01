CREATE OR REPLACE PROCEDURE qp_content_change_column_types(cid numeric)
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    use_native boolean;
BEGIN
    use_native := content.use_native_ef_types from content where content.content_id = cid;
    call qp_content_change_column_types_forced(cid, use_native);
END;
$BODY$;