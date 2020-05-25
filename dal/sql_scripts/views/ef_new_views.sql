
exec qp_drop_existing 'STATUS_TYPE_NEW', 'IsView'
GO

create view STATUS_TYPE_NEW as
select cast(site_id as int) as site_id, cast(status_type_id as int) as id, status_type_name as name, cast([weight] as int) as weight from STATUS_TYPE
GO
exec qp_drop_existing 'USER_NEW', 'IsView'
GO

create view USER_NEW as
select cast([user_id] as int) as [id], [login], nt_login, l.iso_code, first_name, last_name, email from users u
inner join LANGUAGES l on l.language_id = u.language_id
GO

exec qp_drop_existing 'USER_GROUP_NEW', 'IsView'
GO

create view USER_GROUP_NEW as
select cast([group_id] as int) as [id], group_name as name from user_group
GO

exec qp_drop_existing 'USER_GROUP_BIND_NEW', 'IsView'
GO

create view USER_GROUP_BIND_NEW as
select cast([group_id] as int) as [group_id], cast([user_id] as int) as [user_id]
from USER_GROUP_BIND
GO