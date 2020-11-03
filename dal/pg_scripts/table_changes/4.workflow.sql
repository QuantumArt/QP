ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS is_default boolean NOT NULL DEFAULT false;
