ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS template_id numeric(18,0) NULL;

ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS hide_recipients BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS use_email_from_content BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS category_attribute_id NUMERIC(18,0) REFERENCES content_attribute;

ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS confirmation_template_id NUMERIC(18,0) NULL;

