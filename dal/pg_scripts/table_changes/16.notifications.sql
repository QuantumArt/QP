ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS template_id numeric(18,0) NULL;

ALTER TABLE public.notifications ADD COLUMN IF NOT EXISTS hide_recipients BOOLEAN NOT NULL DEFAULT FALSE;
