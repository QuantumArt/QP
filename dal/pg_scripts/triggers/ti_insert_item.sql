DO $$ BEGIN
    create trigger ti_insert_item
        after insert
        on content_item
        REFERENCING NEW TABLE AS new_table
        FOR EACH STATEMENT
    execute procedure process_content_item_insert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


