create or replace view entity_type_access_perm_level(entity_type_access_id, user_id, group_id, permission_level, entity_type_id) as
SELECT c.entity_type_access_id,
       c.user_id,
       c.group_id,
       pl.permission_level,
       x.id AS entity_type_id
FROM ((entity_type_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN entity_type x ON ((c.entity_type_id = x.id)));

alter table entity_type_access_perm_level
    owner to postgres;

