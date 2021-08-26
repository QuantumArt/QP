insert into context_menu (CODE) values ('plugins')
on conflict do nothing;

insert into context_menu (CODE) values ('plugin')
on conflict do nothing;

insert into context_menu (CODE) values ('plugin_version')
on conflict do nothing;

