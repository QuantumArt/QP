CREATE OR REPLACE VIEW workflow_access_permlevel_site AS
  SELECT c.*, pl.permission_level, x.site_id from workflow_access as c
  INNER JOIN permission_level as pl ON c.permission_level_id = pl.permission_level_id
  INNER JOIN workflow as x ON c.workflow_id = x.workflow_id;

