DO $$ BEGIN
    create trigger tu_plugin_field_value
        after update
        on plugin_field_value
        REFERENCING NEW TABLE AS new_table OLD table as old_table
        FOR EACH STATEMENT
    execute procedure process_plugin_field_value_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

