SELECT setval('action_type_seq', cast(COALESCE((SELECT MAX(id)+1 FROM action_type), 1) as int), false) into tmp_val_tbl;
drop table tmp_val_tbl;

insert into action_type(name, code, required_permission_level_id, items_affected)
    values ('Auto resize',  'auto_resize', 2,1)
on conflict do nothing;
