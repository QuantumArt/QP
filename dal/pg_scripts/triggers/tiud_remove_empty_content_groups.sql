-- Trigger: tiud_remove_empty_content_groups

-- DROP TRIGGER tiud_remove_empty_content_groups ON public.content;

CREATE TRIGGER tiud_remove_empty_content_groups
    AFTER INSERT OR DELETE OR UPDATE
    ON public.content
    FOR EACH STATEMENT
    EXECUTE PROCEDURE public.process_content_delete_update_insert();
