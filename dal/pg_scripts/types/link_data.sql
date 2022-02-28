DO $$ BEGIN
    create type link_data as
    (
        id numeric(18),
        attribute_id numeric(18),
        has_data boolean,
        splitted boolean,
        has_async boolean
    );



EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
