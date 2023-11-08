insert into action_type(name, code, required_permission_level_id, items_affected)
    values ('Auto resize',  'auto_resize', 2,1)
on conflict do nothing;
