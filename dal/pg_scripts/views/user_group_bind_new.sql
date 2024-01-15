CREATE OR REPLACE VIEW user_group_bind_new AS
 SELECT cast(group_id as int) as group_id, cast(user_id as int) as user_id from user_group_bind;


