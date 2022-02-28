create or replace view folder_access_permlevel as
    SELECT c.*, pl.permission_level from FOLDER_ACCESS as c
    INNER JOIN Permission_Level as pl ON c.permission_level_id = pl.permission_level_id;



