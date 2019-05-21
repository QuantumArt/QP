ALTER TABLE content_data ALTER COLUMN data TYPE text;

ALTER TABLE content_data ADD COLUMN IF NOT EXISTS o2m_data numeric(18,0) NULL;

update content_data cd set o2m_data = data::numeric from content_attribute ca
where ca.attribute_id = cd.attribute_id and ca.attribute_type_id = 11 and ca.link_id is null
and cd.o2m_data is null and cd.data is not null;

-- DROP INDEX public.ix_o2m_data;

CREATE INDEX ix_o2m_data
    ON public.content_data USING btree
    (o2m_data)
    TABLESPACE pg_default;