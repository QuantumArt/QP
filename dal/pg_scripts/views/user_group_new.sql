CREATE OR REPLACE VIEW user_group_new AS
 SELECT cast(group_id as int) as id, group_name as name from user_group;


