if not exists (select * from context_menu where code = 'plugins')
    insert into context_menu (CODE)
    values ('plugins')

if not exists (select * from context_menu where code = 'plugin')
    insert into context_menu (CODE)
    values ('plugin')

if not exists (select * from context_menu where code = 'plugin_version')
    insert into context_menu (CODE)
    values ('plugin_version')


GO

