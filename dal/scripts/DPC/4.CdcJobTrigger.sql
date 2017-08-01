EXEC qp_drop_existing '[dbo].[tu_db]', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tu_db]
   ON [dbo].[DB]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;

    DECLARE @CdcChangeBit bit;
    SELECT @CdcChangeBit = i.USE_CDC FROM Inserted i INNER JOIN Deleted d ON i.ID = d.ID AND i.USE_CDC != d.USE_CDC;

    IF @CdcChangeBit = 1 BEGIN
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_attribute',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_data',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_item',
           @captured_column_list = 'content_item_id,visible,status_type_id,created,modified,content_id,last_modified_by,archive,not_for_replication,schedule_new_version_publication,splitted,permanent_lock,cancel_split,unique_id',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'content_to_content',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'item_link_async',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'item_to_item',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
      EXEC sys.sp_cdc_enable_table
           @source_schema = N'dbo',
           @source_name = N'status_type',
           @role_name = N'cdc_admin',
           @supports_net_changes = 0;
    END ELSE IF @CdcChangeBit = 0 BEGIN
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_attribute',
           @capture_instance = N'dbo_content_attribute';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content',
           @capture_instance = N'dbo_content';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_data',
           @capture_instance = N'dbo_content_data';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_item',
           @capture_instance = N'dbo_content_item';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'content_to_content',
           @capture_instance = N'dbo_content_to_content';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'item_link_async',
           @capture_instance = N'dbo_item_link_async';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'item_to_item',
           @capture_instance = N'dbo_item_to_item';
      EXEC sys.sp_cdc_disable_table
           @source_schema = N'dbo',
           @source_name = N'status_type',
           @capture_instance = N'dbo_status_type';
    END
END
GO
