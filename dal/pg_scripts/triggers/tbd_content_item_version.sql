DO $$ BEGIN
    create trigger tbd_delete_item_version
        before delete
        on content_item_version
        for each row
    execute procedure process_before_content_item_version_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
