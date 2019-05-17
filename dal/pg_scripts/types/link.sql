create type link as
    (
    id numeric(18),
    linked_id numeric(18)
    );

alter type link owner to postgres;