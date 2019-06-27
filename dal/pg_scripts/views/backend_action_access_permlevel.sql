create or replace view backend_action_access_permlevel(action_access_id, user_id, group_id, permission_level, backend_action_id) as
SELECT c.action_access_id,
       c.user_id,
       c.group_id,
       pl.permission_level,
       x.id AS backend_action_id
FROM ((action_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN backend_action x ON ((c.action_id = x.id)));

alter table backend_action_access_permlevel
    owner to postgres;

