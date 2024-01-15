CREATE OR REPLACE VIEW status_type_new AS
 SELECT cast(site_id as int) as site_id, cast(status_type_id as int) as id, status_type_name as name, cast(weight as int) as weight from STATUS_TYPE;



