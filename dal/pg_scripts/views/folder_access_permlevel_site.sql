create or replace view folder_access_permlevel_site as
    SELECT c.*, pl.permission_level, x.site_id from FOLDER_ACCESS as c
    INNER JOIN Permission_Level as pl ON c.permission_level_id = pl.permission_level_id
    INNER JOIN folder as x ON c.folder_id = x.folder_id;

alter table folder_access_permlevel_site
    owner to postgres;

