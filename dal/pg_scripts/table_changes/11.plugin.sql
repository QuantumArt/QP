CREATE TABLE IF NOT EXISTS public.plugin
(
    id                       numeric(18)              NOT NULL default nextval('plugin_seq')
        constraint pk_plugin primary key,
    name                     varchar(255)             NOT NULL,
    description              text                     NULL,
    code                     varchar(50)              NULL,
    contract                 text                     NULL,
    version                  varchar(10)              NULL,
    "order"                  int                      NOT NULL DEFAULT (0),
    service_url              varchar(512)             NULL,
    allow_multiple_instances boolean                  NOT NULL DEFAULT true,
    instance_key             varchar(50)              NULL,
    created                  timestamp with time zone NOT NULL DEFAULT (now()),
    modified                 timestamp with time zone NOT NULL DEFAULT (now()),
    last_modified_by         numeric(18)              NOT NULL
        constraint fk_plugin_last_modified_by references public.users (user_id)
);


CREATE UNIQUE INDEX IF NOT EXISTS ix_plugin_name ON plugin(name);

-- drop table public.plugin
