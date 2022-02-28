DO $$ BEGIN
    create type link_multiple_splitted as
    (
        id numeric(18),
        link_id numeric(18),
        linked_id numeric(18),
        splitted boolean
    );



EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
