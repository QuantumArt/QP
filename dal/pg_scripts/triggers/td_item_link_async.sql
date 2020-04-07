DO $$ BEGIN
    create trigger td_item_link_async
        after delete
        on item_link_async
        referencing OLD TABLE as old_table
        for each statement
    execute procedure process_item_to_item_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
