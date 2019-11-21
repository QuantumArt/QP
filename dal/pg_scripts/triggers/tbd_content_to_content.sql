DO $$ BEGIN
    create trigger tbd_content_to_content
        before delete
        on content_to_content
        for each row
    execute procedure process_before_content_to_content_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
