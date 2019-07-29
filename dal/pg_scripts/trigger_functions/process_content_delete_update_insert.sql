-- FUNCTION: public.process_content_delete_update_insert()

-- DROP FUNCTION public.process_content_delete_update_insert();

CREATE OR REPLACE FUNCTION public.process_content_delete_update_insert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
BEGIN
	DELETE FROM content_group
	  WHERE "name" <> 'Default Group'
	  AND NOT EXISTS(SELECT * FROM content WHERE content.content_group_id = content_group.content_group_id);
	  RETURN NULL;
END;
$BODY$;

ALTER FUNCTION public.process_content_delete_update_insert()
    OWNER TO postgres;
