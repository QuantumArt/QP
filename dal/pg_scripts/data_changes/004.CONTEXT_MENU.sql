SELECT setval('context_menu_seq', cast(COALESCE((SELECT MAX(id)+1 FROM context_menu), 1) as int), false) into tmp_val_tbl;
drop table tmp_val_tbl;

insert into context_menu (CODE) values ('plugins')
on conflict do nothing;

insert into context_menu (CODE) values ('plugin')
on conflict do nothing;

insert into context_menu (CODE) values ('plugin_version')
on conflict do nothing;

