CREATE OR REPLACE VIEW public.user_group_bind_new AS
 SELECT cast(group_id as int) as group_id, cast(user_id as int) as user_id from user_group_bind;

ALTER TABLE public.user_group_bind_new
    OWNER TO postgres;
