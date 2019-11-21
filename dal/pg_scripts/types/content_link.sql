DO $$ BEGIN
    create type content_link as
    (
        id numeric(18),
        is_symmetric boolean,
        l_content_id numeric(18),
        r_content_id numeric(18)
    );

    alter type content_link owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


