create view content_item_access_permlevel(content_item_id, user_id, group_id, permission_level_id, created, modified,
                                          last_modified_by, content_item_access_id, permission_level) as
SELECT c.content_item_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.content_item_access_id,
       pl.permission_level
FROM (content_item_access c
         JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)));

alter table content_item_access_permlevel
    owner to postgres;

