-- auto-generated definition
create type link_multiple as
    (
    id numeric(18),
    link_id numeric(18),
    linked_id numeric(18),
    splitted boolean
    );

alter type link_multiple owner to postgres;

