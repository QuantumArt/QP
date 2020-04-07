DO $$ BEGIN
    create trigger tbd_delete_item
        before delete
        on content_item
        for each row
    execute procedure process_before_content_item_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

