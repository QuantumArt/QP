DO $$ BEGIN
    create trigger ti_content_data_fill
        after insert
        on content_data
        REFERENCING NEW TABLE AS new_table
        FOR EACH STATEMENT
    execute procedure process_content_data_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


