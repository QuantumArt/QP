DO $$ BEGIN
    create trigger tbd_content
        before delete
        on content
        for each row
    execute procedure process_before_content_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
