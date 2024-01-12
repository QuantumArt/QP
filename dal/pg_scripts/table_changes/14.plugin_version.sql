CREATE TABLE IF NOT EXISTS plugin_version
(
    id               numeric(18)              NOT NULL default nextval('plugin_version_seq')
        constraint pk_plugin_version primary key,
    plugin_id        numeric(18)              NOT NULL
        constraint fk_plugin_version_plugin_id references plugin(id) on delete cascade,
    contract         text                     NOT NULL,
    created          timestamp with time zone NOT NULL DEFAULT (now()),
    modified         timestamp with time zone NOT NULL DEFAULT (now()),
    last_modified_by numeric(18)              NOT NULL
        constraint fk_plugin_version_last_modified_by references users (user_id)

);

-- drop table plugin_version

