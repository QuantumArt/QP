-- View: site_access_permlevel

-- DROP VIEW site_access_permlevel;

CREATE OR REPLACE VIEW site_access_permlevel AS
 SELECT c.site_id,
    c.user_id,
    c.group_id,
    c.permission_level_id,
    c.created,
    c.modified,
    c.last_modified_by,
    c.propagate_to_contents,
    c.site_access_id,
    pl.permission_level
   FROM site_access c
     JOIN permission_level pl ON c.permission_level_id = pl.permission_level_id;



