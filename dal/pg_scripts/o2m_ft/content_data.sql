DO $$
BEGIN
	IF EXISTS (
		SELECT NULL
		FROM information_schema.columns
		WHERE
			table_schema = 'public' AND
			table_name = 'content_data' AND
			column_name = 'data' AND
			data_type <> 'text')
	THEN
		ALTER TABLE content_data ALTER COLUMN data TYPE text;
	END IF;
END
$$

ALTER TABLE content_data ADD COLUMN IF NOT EXISTS o2m_data numeric(18,0) NULL;

ALTER TABLE content_data ADD COLUMN IF NOT EXISTS ft_data tsvector NULL;


update content_data cd set o2m_data = data::numeric from content_attribute ca
where ca.attribute_id = cd.attribute_id and ca.attribute_type_id = 11 and ca.link_id is null
and cd.o2m_data is null and cd.data is not null;

-- DROP INDEX public.ix_o2m_data;

CREATE INDEX IF NOT EXISTS ix_o2m_data
    ON public.content_data USING btree
    (o2m_data)
    TABLESPACE pg_default;

update content_data cd
set ft_data = to_tsvector('russian', coalesce(cd.data, cd.blob_data))
from content_attribute ca
where ca.attribute_id = cd.attribute_id
and cd.ft_data is null and coalesce(cd.data, cd.blob_data) is not null;

CREATE INDEX IF NOT EXISTS ix_ft_data ON content_data USING gist(ft_data);


DO $$
BEGIN

  BEGIN

    ALTER TABLE public.content_data
        ADD CONSTRAINT fk_content_data_content_attribute FOREIGN KEY (attribute_id)
        REFERENCES public.content_attribute (attribute_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE;


  EXCEPTION
    WHEN duplicate_object THEN RAISE NOTICE 'Table constraint fk_content_data_content_attribute already exists';
  END;

END $$;

DO $$
BEGIN
  BEGIN

    ALTER TABLE public.content_data
        ADD CONSTRAINT fk_content_data_content_item FOREIGN KEY (content_item_id)
        REFERENCES public.content_item (content_item_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;

  EXCEPTION
    WHEN duplicate_object THEN RAISE NOTICE 'Table constraint fk_content_data_content_item already exists';
  END;

END $$;

