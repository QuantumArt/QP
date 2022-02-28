DO $$ BEGIN
    create type link as
    (
        id numeric(18),
        linked_id numeric(18)
    );



EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
