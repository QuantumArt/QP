DO $$ BEGIN
    create trigger tu_content_data_fill
        after update
        on content_data
        REFERENCING NEW TABLE AS new_table OLD table as old_table
        FOR EACH STATEMENT
    execute procedure process_content_data_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

