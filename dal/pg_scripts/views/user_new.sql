CREATE OR REPLACE VIEW public.user_new AS
 SELECT cast(user_id as int) as id, login ,nt_login, l.iso_code, first_name, last_name, email  from users u
     inner join LANGUAGES l on l.language_id = u.language_id;

