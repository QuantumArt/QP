DO $$ BEGIN
    create trigger td_plugin_field_value
        after delete
        on plugin_field_value
        REFERENCING OLD table as old_table
        FOR EACH STATEMENT
    execute procedure process_plugin_field_value_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

