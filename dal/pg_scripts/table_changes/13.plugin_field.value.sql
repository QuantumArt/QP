CREATE TABLE IF NOT EXISTS public.plugin_field_value
(
    id                   numeric(18) NOT NULL default nextval('plugin_field_value_seq')
        constraint pk_plugin_field_value primary key,
    plugin_field_id      numeric(18) NOT NULL
        constraint fk_plugin_field_value_plugin_field_id references public.plugin_field (id) on delete cascade,
    content_id           numeric(18) NULL
        constraint fk_plugin_field_value_content_id references public.content (content_id),
    site_id              numeric(18) NULL
        constraint fk_plugin_field_value_site_id references public.site (site_id),
    content_attribute_id numeric(18) NULL
        constraint fk_plugin_field_value_content_attribute_id references public.content_attribute (attribute_id) ON DELETE CASCADE,
    value                text        NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_plugin_field_value_content_id ON plugin_field_value(plugin_field_id, content_id)
    WHERE content_id is not null;

CREATE UNIQUE INDEX IF NOT EXISTS ix_plugin_field_value_site_id ON plugin_field_value(plugin_field_id, site_id)
    WHERE site_id is not null;

--drop table plugin_field_value