CREATE SEQUENCE IF NOT EXISTS public.notifications_sent_seq START 1;

ALTER TABLE public.notifications_sent ADD COLUMN IF NOT EXISTS id int NOT NULL DEFAULT nextval('notifications_sent_seq'::regclass);

ALTER TABLE public.notifications_sent DROP CONSTRAINT IF EXISTS pk_notifications_sent;

ALTER TABLE public.notifications_sent ADD CONSTRAINT pk_notifications_sent PRIMARY KEY (id);
