-- auto-generated definition
create type link_data as
    (
    id numeric(18),
    attribute_id numeric(18),
    has_data boolean,
    splitted boolean,
    has_async boolean
    );

alter type link_data owner to postgres;

