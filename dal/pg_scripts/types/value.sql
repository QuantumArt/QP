DO $$ BEGIN
    create type value as
    (
        id numeric,
        field_id numeric,
        data text
    );



EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
