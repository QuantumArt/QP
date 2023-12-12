update entity_type_access set permission_level_id = 6 
where entity_type_id = 1 and group_id in (select group_id from user_group where group_name = 'Everyone')
and permission_level_id <> 6;
