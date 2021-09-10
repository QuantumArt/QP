DO $$ BEGIN
    create trigger ti_plugin_field_value
        after insert
        on plugin_field_value
        REFERENCING NEW TABLE AS new_table
        FOR EACH STATEMENT
    execute procedure process_plugin_field_value_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
