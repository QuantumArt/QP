ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS is_default boolean NOT NULL DEFAULT false;
ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS use_direction_controls boolean NOT NULL DEFAULT false;

