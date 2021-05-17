ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS is_default boolean NOT NULL DEFAULT false;
ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS use_direction_controls boolean NOT NULL DEFAULT false;

update workflow set is_default = true where workflow_name = 'general' and not exists (select * from workflow where is_default);
