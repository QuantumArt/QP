DO $$ BEGIN
    create type column_to_process as
    (
        table_name text,
        column_name text
    );

    alter type column_to_process owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;