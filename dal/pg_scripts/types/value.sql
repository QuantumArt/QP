DO $$ BEGIN
    create type value as
    (
        id numeric,
        field_id numeric,
        data text
    );

    alter type value owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
