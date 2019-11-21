DO $$ BEGIN
    create trigger tu_update_item
        after update
        on content_item
        REFERENCING NEW TABLE AS new_table OLD TABLE AS old_table
        FOR EACH STATEMENT
    execute procedure process_content_item_update();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


