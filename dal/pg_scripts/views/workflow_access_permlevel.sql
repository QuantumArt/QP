create or replace view workflow_access_permlevel as
SELECT c.*, pl.permission_level from workflow_access as c INNER JOIN permission_level as pl ON c.permission_level_id = pl.permission_level_id;

alter table workflow_access_permlevel
    owner to postgres;

