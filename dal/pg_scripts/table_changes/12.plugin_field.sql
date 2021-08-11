CREATE TABLE IF NOT EXISTS public.plugin_field
(
    id            numeric(18)  NOT NULL default nextval('plugin_field_seq')
        constraint pk_plugin_field primary key,
    plugin_id     numeric(18)  NOT NULL
        constraint fk_plugin_field_plugin_id references public.plugin (id) on delete cascade,
    name          varchar(255) NOT NULL,
    description   text         NULL,
    value_type    varchar(50)  NOT NULL,
    relation_type varchar(50)  NOT NULL,
    "order"       int          NOT NULL default (0)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_plugin_field_name ON plugin_field(plugin_id, name);
--drop index ix_plugin_field_name