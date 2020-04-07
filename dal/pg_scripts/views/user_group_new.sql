CREATE OR REPLACE VIEW public.user_group_new AS
 SELECT cast(group_id as int) as id, group_name as name from user_group;

ALTER TABLE public.user_group_new
    OWNER TO postgres;
