DO $$ BEGIN
    create trigger td_delete_item
        after delete
        on content_item
        REFERENCING OLD TABLE AS old_table
        FOR EACH STATEMENT
    execute procedure process_content_item_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

