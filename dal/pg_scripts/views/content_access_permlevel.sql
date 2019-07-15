create or replace view content_access_permlevel(content_id, user_id, group_id, permission_level_id, created, modified,
                                      last_modified_by, propagate_to_items, content_access_id, hide,
                                      permission_level) as
SELECT c.content_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.propagate_to_items,
       c.content_access_id,
       c.hide,
       pl.permission_level
FROM (content_access c
         JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)));

alter table content_access_permlevel
    owner to postgres;

