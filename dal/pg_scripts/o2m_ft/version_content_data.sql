ALTER TABLE version_content_data ALTER COLUMN data TYPE text;

ALTER TABLE version_content_data ADD COLUMN IF NOT EXISTS o2m_data numeric(18,0) NULL;

update version_content_data cd set o2m_data = data::numeric from content_attribute ca
where ca.attribute_id = cd.attribute_id and ca.attribute_type_id = 11 and ca.link_id is null
and cd.o2m_data is null and cd.data is not null;

-- DROP INDEX ix_o2m_data;

CREATE INDEX IF NOT EXISTS ix_version_o2m_data
    ON version_content_data USING btree
    (o2m_data)
    TABLESPACE pg_default;
