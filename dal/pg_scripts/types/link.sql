DO $$ BEGIN
    create type link as
    (
        id numeric(18),
        linked_id numeric(18)
    );

    alter type link owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
