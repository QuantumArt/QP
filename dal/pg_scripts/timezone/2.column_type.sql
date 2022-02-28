DO $$ BEGIN
    create type column_to_process as
    (
        table_name text,
        column_name text
    );


EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
