ALTER TRIGGER [dbo].[ti_statuses_and_default_notif] ON [dbo].[SITE]
FOR INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_statuses_and_default_notif') is null
    begin
        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Created',  10, 'Article has been created' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Approved',  50, 'Article has been modified' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'Published',  100, 'Article has been published' ,1, 1 from inserted)

        insert into status_type (site_id, status_type_name, weight, description, last_modified_by, BUILT_IN)
        (select site_id , 'None',  0, 'No Status has been assigned' ,1, 1 from inserted)
 
        INSERT INTO page_template(site_id, template_name, net_template_name, template_picture, created, modified, last_modified_by, charset, codepage, locale, is_system, net_language_id)
        select site_id, 'Default Notification Template', 'Default_Notification_Template', '', getdate(), getdate(), 1, 'utf-8', 65001, 1049, 1, dbo.qp_default_net_language(script_language) from inserted

        insert into content_group (site_id, name)
        select site_id, 'Default Group' from inserted
    end
END
go

