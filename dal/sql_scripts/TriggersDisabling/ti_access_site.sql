ALTER TRIGGER ti_access_site ON SITE 
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_access_site') is null
    begin
        insert into site_access (site_id, user_id, permission_level_id, last_modified_by)
        (select site_id, last_modified_by, 1, 1 from inserted i where i.last_modified_by > 1 )
        insert into site_access (site_id, group_id, permission_level_id, last_modified_by)
        (select site_id, 1, 1, 1 from inserted)
    end
END
go

