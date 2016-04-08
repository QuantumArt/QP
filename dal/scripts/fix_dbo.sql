-- ************************************** 
-- Pavel Celut
-- version 7.9.7.0
-- Release
-- **************************************

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.0', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.0 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.1
-- PageTemplate refactoring
-- **************************************

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/IndexPages/'
WHERE [code] = 'list_page' 
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/NewPage/'
WHERE [code] = 'new_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/PageProperties/'
WHERE [code] = 'edit_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/RemovePage/'
WHERE [code] = 'remove_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/MultipleRemovePage/'
WHERE [code] = 'multiple_remove_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/CancelPage/'
WHERE [code] = 'cancel_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/SelectPages/'
WHERE [code] = 'select_page_for_object_form'
GO
--

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/IndexTemplateObjects/'
WHERE [code] = 'list_template_object' 
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/IndexPageObjects/'
WHERE [code] = 'list_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/NewPageObject/'
WHERE [code] = 'new_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/NewTemplateObject/'
WHERE [code] = 'new_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/PromotePageObject/'
WHERE [code] = 'promote_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/PageObjectProperties/'
WHERE [code] = 'edit_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/TemplateObjectProperties/'
WHERE [code] = 'edit_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/RemovePageObject/'
WHERE [code] = 'remove_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/RemoveTemplateObject/'
WHERE [code] = 'remove_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/MultipleRemovePageObject/'
WHERE [code] = 'multiple_remove_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/MultipleRemoveTemplateObject/'
WHERE [code] = 'multiple_remove_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/CancelPageObject/'
WHERE [code] = 'cancel_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/CancelTemplateObject/'
WHERE [code] = 'cancel_template_object'
GO
--

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/IndexPageObjectFormats/'
WHERE [code] = 'list_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/IndexTemplateObjectFormats/'
WHERE [code] = 'list_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/IndexPageObjectFormatVersions/'
WHERE [code] = 'list_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/IndexTemplateObjectFormatVersions/'
WHERE [code] = 'list_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CancelTemplateObjectFormat/'
WHERE [code] = 'cancel_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CancelPageObjectFormat/'
WHERE [code] = 'cancel_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/NewPageObjectFormat/'
WHERE [code] = 'new_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/NewTemplateObjectFormat/'
WHERE [code] = 'new_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/PageObjectFormatProperties/'
WHERE [code] = 'edit_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/TemplateObjectFormatProperties/'
WHERE [code] = 'edit_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/TemplateObjectFormatVersionProperties/'
WHERE [code] = 'edit_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/PageObjectFormatVersionProperties/'
WHERE [code] = 'edit_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CompareWithCurrentTemplateObjectFormatVersion/'
WHERE [code] = 'compare_with_cur_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CompareWithCurrentPageObjectFormatVersion/'
WHERE [code] = 'compare_with_cur_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CompareTemplateObjectFormatVersions/'
WHERE [code] = 'compare_template_object_format_versions'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/ComparePageObjectFormatVersions/'
WHERE [code] = 'compare_page_object_format_versions'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/RemoveTemplateObjectFormat/'
WHERE [code] = 'remove_template_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/RemovePageObjectFormat/'
WHERE [code] = 'remove_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/MultipleRemoveTemplateObjectFormatVersion/'
WHERE [code] = 'multiple_remove_template_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/MultipleRemovePageObjectFormatVersion/'
WHERE [code] = 'multiple_remove_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/AssemblePage/'
WHERE [code] = 'assemble_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/MultipleAssemblePage/'
WHERE [code] = 'multiple_assemble_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/AssemblePageObject/'
WHERE [code] = 'assemble_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/AssembleTemplateObject/'
WHERE [code] = 'assemble_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/MultipleAssemblePageObject/'
WHERE [code] = 'multiple_assemble_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/MultipleAssembleTemplateObject/'
WHERE [code] = 'multiple_assemble_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Page/CaptureLockPage/'
WHERE [code] = 'capture_lock_page'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/CaptureLockPageObject/'
WHERE [code] = 'capture_lock_page_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Object/CaptureLockTemplateObject/'
WHERE [code] = 'capture_lock_template_object'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CaptureLockPageObjectFormat/'
WHERE [code] = 'capture_lock_page_object_format'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/CaptureLockTemplateObjectFormat/'
WHERE [code] = 'capture_lock_template_object_format'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.1', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.1 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.2
-- PageTemplate refactoring
-- **************************************

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/RestorePageObjectFormatVersion/'
WHERE [code] = 'restore_page_object_format_version'
GO

UPDATE [BACKEND_ACTION]
SET [CONTROLLER_ACTION_URL] = '~/Format/RestoreTemplateObjectFormatVersion/'
WHERE [code] = 'restore_template_object_format_version'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.2', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.2 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.3
-- Fix Order
-- **************************************

update CONTEXT_MENU_ITEM set [ORDER] = ca.[ORDER] + 1000 FROM custom_action ca inner join backend_action ba on ca.action_id = ba.ID
inner join CONTEXT_MENU_ITEM cmi on cmi.ACTION_ID = ba.ID

update ACTION_TOOLBAR_BUTTON set [ORDER] = ca.[ORDER] + 1000 FROM custom_action ca inner join backend_action ba on ca.action_id = ba.ID
inner join ACTION_TOOLBAR_BUTTON atb on atb.ACTION_ID = ba.ID

update VE_COMMAND set TOOLBAR_IN_ROW_ORDER = 10 + vep.[ORDER]
FROM VE_COMMAND vec INNER JOIN VE_PLUGIN vep on vec.PLUGIN_ID = VEP.ID 
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.3', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.3 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.4
-- Copy page
-- **************************************

insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL)
values('Create Like Page', 'copy_page', dbo.qp_action_type_id('copy'), dbo.qp_entity_type_id('page'), '~/Page/Copy/')
go

insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
values (dbo.qp_action_id('list_page'), dbo.qp_action_id('copy_page'), 'Create like', 'create_like.gif', NULL, 45, 0)
go

insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, ICON, [ORDER])
values(dbo.qp_context_menu_id('page'), dbo.qp_action_id('copy_page'), 'Create like', 'create_like.gif', 40)

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.4', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.4 completed'
GO


-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.5
-- Tree Order
-- **************************************

ALTER TABLE [dbo].[CONTENT_ATTRIBUTE] 
ADD 	
    [TREE_ORDER_FIELD] [numeric](18, 0) NULL CONSTRAINT [DF_TREE_ORDER_FILED]  DEFAULT (NULL)
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.5', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.5 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.6
-- Fix Select Content
-- **************************************

update BACKEND_ACTION set CODE = 'multiple_select_content_for_union', CONTROLLER_ACTION_URL = '~/Content/MultipleSelectForUnion/' where CODE = 'multiple_select_contents_for_union'
update BACKEND_ACTION set CODE = 'multiple_select_content_for_workflow', CONTROLLER_ACTION_URL = '~/Content/MultipleSelectForWorkflow/' where CODE = 'multiple_select_contents_for_workflow'
update BACKEND_ACTION set ENTITY_TYPE_ID = dbo.qp_entity_type_id('content') where CODE = 'multiple_select_contents_for_workflow'

if not exists (select * from BACKEND_ACTION where CODE = 'multiple_select_content_for_custom_action')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL, ALLOW_SEARCH)
values('Multiple Select Contents For Custom Action', 'multiple_select_content_for_custom_action', dbo.qp_action_type_id('multiple_select'), dbo.qp_entity_type_id('content'),1, '~/Content/MultipleSelectForCustomAction/', 1)

if not exists (select * from BACKEND_ACTION where CODE = 'select_content_for_field')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL, ALLOW_SEARCH)
values('Select Content For Field', 'select_content_for_field', dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('content'),1, '~/Content/SelectForField/', 1)

if not exists (select * from BACKEND_ACTION where CODE = 'select_content_for_join')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL, ALLOW_SEARCH)
values('Select Content For JOIN', 'select_content_for_join', dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('content'),1, '~/Content/SelectForJoin/', 1)

if not exists (select * from BACKEND_ACTION where CODE = 'select_content')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL, ALLOW_SEARCH)
values('Select Content', 'select_content', dbo.qp_action_type_id('select'), dbo.qp_entity_type_id('content'),1, '~/Content/Select/', 1)

update backend_action set ALLOW_SEARCH = 0 where CODE = 'multiple_select_content_for_custom_action'
GO


INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.6', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.6 completed'
GO


-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.7
-- Content Attribute OrderTreefField FK
-- **************************************

ALTER TABLE [CONTENT_ATTRIBUTE]
ADD FOREIGN KEY (TREE_ORDER_FIELD)
REFERENCES [CONTENT_ATTRIBUTE]([ATTRIBUTE_ID])

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.7', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.7 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.8
-- Copy page
-- **************************************

insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL)
values('Create Like Field', 'copy_field', dbo.qp_action_type_id('copy'), dbo.qp_entity_type_id('field'), '~/Field/Copy/')
go

insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
values (dbo.qp_action_id('list_field'), dbo.qp_action_id('copy_field'), 'Create like', 'create_like.gif', NULL, 45, 0)
go

insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, ICON, [ORDER])
values(dbo.qp_context_menu_id('field'), dbo.qp_action_id('copy_field'), 'Create like', 'create_like.gif', 40)


INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.8', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.8 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.9
-- M2M def value
-- **************************************
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FIELD_ARTICLE_BIND](
    [ARTICLE_ID] [numeric](18, 0) NOT NULL,
    [FIELD_ID] [numeric](18, 0) NOT NULL,	
 CONSTRAINT [PK_FIELD_ARTICLE_BIND] PRIMARY KEY CLUSTERED 
(
    [ARTICLE_ID] ASC,
    [FIELD_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND]  WITH CHECK ADD  CONSTRAINT [FK_FIELD_ARTICLE_BIND_ARTICLE] FOREIGN KEY([ARTICLE_ID])
REFERENCES [dbo].[CONTENT_ITEM] ([CONTENT_ITEM_ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND] CHECK CONSTRAINT [FK_FIELD_ARTICLE_BIND_ARTICLE]
GO

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND]  WITH CHECK ADD  CONSTRAINT [FK_FIELD_ARTICLE_BIND_FIELD] FOREIGN KEY([FIELD_ID])
REFERENCES [dbo].[CONTENT_ATTRIBUTE] ([ATTRIBUTE_ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND] CHECK CONSTRAINT [FK_FIELD_ARTICLE_BIND_FIELD]
GO

ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30)
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id) from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
        
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids)
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                DELETE from FIELD_ARTICLE_BIND
                    where [ARTICLE_ID] in (select id from @attr_ids)
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END

go


INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.9', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.9 completed'
GO



-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.10
-- M2M def value trigger fix
-- **************************************
--
ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30)
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id) from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
        
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids)
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                DELETE from FIELD_ARTICLE_BIND
                    where [ARTICLE_ID] in (select char_id from @ids)
                    AND [FIELD_ID] in (select id from @attr_ids)
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END
go
--
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.10', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.10 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.11
-- Adding a feature: in csv file for import,  client can add new articles with fields that relate to the current content.
-- **************************************

CREATE PROCEDURE [dbo].[qp_update_o2mfieldvalues]
    @xmlParameter xml
AS
BEGIN
    DECLARE @idoc int
    EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlParameter;
    
    DECLARE @NewO2MValues TABLE (CONTENT_ITEM_ID int, LINKED_ID int, FIELD_ID int)	
    
    INSERT INTO @NewO2MValues
        SELECT * FROM OPENXML(@idoc, '/items/item')
        WITH(
                CONTENT_ITEM_ID int '@id'
                ,LINKED_ID int '@linked_id'
                ,FIELD_ID int '@field_id') 

        BEGIN  
           UPDATE
            [dbo].[CONTENT_DATA]
        SET
            [dbo].[CONTENT_DATA].[DATA] = [@NewO2MValues].LINKED_ID
        FROM
            [dbo].[CONTENT_DATA]
        INNER JOIN
            @NewO2MValues
        ON
            [dbo].[CONTENT_DATA].CONTENT_ITEM_ID = [@NewO2MValues].CONTENT_ITEM_ID AND
            [dbo].[CONTENT_DATA].ATTRIBUTE_ID = [@NewO2MValues].FIELD_ID

        END
END
GO
--
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.11', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.11 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.12
-- Get WordForms for FTS
-- **************************************

if exists(select * from sys.procedures where name = 'usp_fts_parser')
    exec qp_drop_existing 'usp_fts_parser', 'IsProcedure'
GO
    
CREATE PROCEDURE dbo.usp_fts_parser(@searchString nvarchar(max))
with execute as owner
AS
    SELECT display_term FROM sys.dm_fts_parser(@searchString, 1049, 0, 0) 
    union SELECT display_term FROM sys.dm_fts_parser(@searchString, 1033, 0, 0)
GO	

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.12', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.12 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.13
-- Content Modification Table
-- **************************************

if not exists (select * from information_schema.tables where table_name = 'content_modification')
BEGIN
    CREATE TABLE dbo.CONTENT_MODIFICATION
    (
        CONTENT_ID numeric(18, 0) NOT NULL,
        LIVE_MODIFIED datetime NOT NULL,
        STAGE_MODIFIED datetime NOT NULL,
        CONSTRAINT PK_CONTENT_MODIFICATION PRIMARY KEY CLUSTERED (CONTENT_ID),
        CONSTRAINT FK_CONTENT_MODIFICATION_CONTENT_ID FOREIGN KEY (CONTENT_ID) REFERENCES dbo.CONTENT(CONTENT_ID) ON DELETE CASCADE
    )
END
GO

if not exists(select * from CONTENT_MODIFICATION)
begin

    insert into CONTENT_MODIFICATION
    select content_id, max(modified) as live_modified, MAX(modified) as stage_modified from content_item group by content_id
end
GO

exec qp_drop_existing 'ti_insert_modify_row', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[ti_insert_modify_row] ON [dbo].[CONTENT] FOR INSERT
AS
BEGIN
    INSERT INTO CONTENT_MODIFICATION
    SELECT CONTENT_ID, GETDATE(), GETDATE() from inserted
END
GO

ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric, @published_id numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30),
            status_type_id numeric,
            splitted bit
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id), status_type_id, splitted from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
            
            select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
            if exists (select * from @ids where status_type_id = @published_id and splitted = 0)
                update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
            else
                update content_modification set stage_modified = GETDATE() where content_id = @content_id	
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids)
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)	
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END
GO

ALTER PROCEDURE [dbo].[qp_replicate_items] 
@ids nvarchar(max)
AS
BEGIN
    set nocount on
    
    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        status_type_id numeric,
        content_id numeric
    )
    
    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')
    
    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID 

    declare @contents table
    (
        id numeric primary key
    )
    
    insert into @contents
    select distinct content_id from @articles
    
    while exists (select id from @contents)
    begin
        select @content_id = id from @contents
        
        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 0
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 1
        
        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'
        
        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0		
        end
        
        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1							
        end
        
        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and status_type_id = @published_id and splitted = 0)
            update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        else
            update content_modification set stage_modified = GETDATE() where content_id = @content_id	

        
        delete from @contents where id = @content_id
    end
    
    set @sql = 'update content_item  set not_for_replication = 0 where content_item_id in (' + @ids + ' )'
    print @sql
    exec sp_executesql @sql
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.13', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.13 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.14
-- Fix MAXINT error
-- **************************************

ALTER PROCEDURE [dbo].[qp_get_paged_data]
    @select_block nvarchar(max),
    @from_block nvarchar(max),
    @where_block nvarchar(max) = '',
    @order_by_block nvarchar(max),
    @count_only bit = 0,
    @total_records int OUTPUT,
    @start_row bigint = 0,
    @page_size bigint = 0,
    
    @use_security bit = 0,
    @user_id numeric(18,0) = 0,
    @group_id numeric(18,0) = 0,
    @start_level int = 2,
    @end_level int = 4,
    @entity_name nvarchar(100),
    @parent_entity_name nvarchar(100) = '',
    @parent_entity_id numeric(18,0) = 0,
    
    @insert_key varchar(200) = '<$_security_insert_$>'
AS
BEGIN
    SET NOCOUNT ON
    
    -- Получаем фильтр по правам
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    IF (@use_security = 1)
        BEGIN
            EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @user_id,
                @group_id = @group_id,
                @start_level = @start_level,
                @end_level = @end_level,
                @entity_name = @entity_name,
                @parent_entity_name = @parent_entity_name,
                @parent_entity_id = @parent_entity_id,				
                @SQLOut = @security_sql OUTPUT
                
            SET @from_block = REPLACE(@from_block, @insert_key, @security_sql)
        END
        
    -- Получаем общее количество записей
    DECLARE @sql_count AS nvarchar(max)
    
    if (@count_only = 1)
    BEGIN
        SET @sql_count = ''
        SET @sql_count = @sql_count + 'SELECT ' + CHAR(13)
        SET @sql_count = @sql_count + '		@record_count = COUNT(*) ' + CHAR(13)
        SET @sql_count = @sql_count + '	FROM' + CHAR(13)
        SET @sql_count = @sql_count + @from_block + CHAR(13)
        IF (LEN(@where_block) > 0)
            BEGIN
                SET @sql_count = @sql_count + 'WHERE ' + CHAR(13)
                SET @sql_count = @sql_count + @where_block + CHAR(13)
            END


        EXEC sp_executesql 
            @sql_count, 
            N'@record_count int OUTPUT', 
            @record_count = @total_records OUTPUT
    END
    
    -- Задаем номер начальной записи по умолчанию
    IF (@start_row <= 0)
        BEGIN
            SET @start_row = 1
        END
        
    -- Задаем номер конечной записи
    DECLARE @end_row AS bigint
    if (@page_size = 0)
        SET @end_row = 0			
    else
        SET @end_row = @start_row + @page_size - 1		
    
    IF (@count_only = 0)
        BEGIN
            -- Возвращаем результат
            DECLARE @sql_result AS nvarchar(max)
            
            SET @sql_result = ''		
            SET @sql_result = @sql_result + 'WITH PAGED_DATA_CTE' + CHAR(13)
            SET @sql_result = @sql_result + 'AS' + CHAR(13)
            SET @sql_result = @sql_result + '(' + CHAR(13)
            SET @sql_result = @sql_result + '	SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		c.*, ' + CHAR(13)
            SET @sql_result = @sql_result + '		ROW_NUMBER() OVER (ORDER BY ' + @order_by_block + ') AS ROW_NUMBER, COUNT(*) OVER() AS ROWS_COUNT ' + CHAR(13)
            SET @sql_result = @sql_result + '	FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	( ' + CHAR(13)
            SET @sql_result = @sql_result + '		SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @select_block + CHAR(13)
            SET @sql_result = @sql_result + '		FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @from_block + CHAR(13)
            IF (LEN(@where_block) > 0)
                BEGIN
                    SET @sql_result = @sql_result + '		WHERE' + CHAR(13)
                    SET @sql_result = @sql_result + '		' + @where_block + CHAR(13)
                END
            SET @sql_result = @sql_result + '	) AS c ' + CHAR(13)
            SET @sql_result = @sql_result + ')' + CHAR(13) + CHAR(13)
            
            SET @sql_result = @sql_result + 'SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '	* ' + CHAR(13)
            SET @sql_result = @sql_result + 'FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	PAGED_DATA_CTE' + CHAR(13)
            IF (@end_row > 0 or @start_row > 1)
            BEGIN
                SET @sql_result = @sql_result + 'WHERE 1 = 1' + CHAR(13)
                IF @start_row > 1 
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER >= ' + CAST(@start_row AS nvarchar) + ' '
                IF @end_row > 0
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER <= ' + CAST(@end_row AS nvarchar) + ' ' + CHAR(13)
            END	
            SET @sql_result = @sql_result + 'ORDER BY ' + CHAR(13)
            SET @sql_result = @sql_result + '	ROW_NUMBER ASC ' + CHAR(13)

            print(@sql_result)
            EXEC(@sql_result)
        END
    
    SET NOCOUNT OFF
END
GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.14', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.14 completed'
GO


-- ************************************** 
-- Pavel Celut
-- version 7.9.7.15
-- Allow Action Permission
-- **************************************
update ENTITY_TYPE set NAME = 'Page Object Format' where CODE = 'page_object_format'
update ENTITY_TYPE set NAME = 'Template Object Format' where CODE = 'template_object_format'
update ENTITY_TYPE set ACTION_PERMISSION_ENABLE = 1 where CODE in ('template', 'page', 'template_object', 'page_object', 'page_object_format', 'template_object_format', 'page_object_format_version', 'template_object_format_version' )
update BACKEND_ACTION set ENTITY_TYPE_ID = dbo.qp_entity_type_id('page_object'), NAME = 'Save Object' where CODE = 'save_page_object'

exec qp_update_translations 'Page Object Format', 'Формат объекта страницы'
exec qp_update_translations 'Template Object Format', 'Формат объекта шаблона'

GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.15', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.15 completed'
GO


-- ************************************** 
-- Zakirov Kirill
-- version 7.9.7.16
-- Def val m2m
-- **************************************

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND]  DROP  CONSTRAINT [FK_FIELD_ARTICLE_BIND_ARTICLE]
go

ALTER TABLE [dbo].[FIELD_ARTICLE_BIND]  WITH CHECK ADD CONSTRAINT [FK_FIELD_ARTICLE_BIND_ARTICLE] FOREIGN KEY([ARTICLE_ID])
REFERENCES [dbo].[CONTENT_ITEM] ([CONTENT_ITEM_ID])

go


GO
/****** Object:  Trigger [dbo].[td_delete_item]    Script Date: 09/19/2013 15:08:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30)
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id) from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
        
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids)
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)				
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END

go

ALTER TRIGGER [dbo].[tbd_delete_content_item] ON [dbo].[CONTENT_ITEM] INSTEAD OF DELETE
AS 
BEGIN

delete waiting_for_approval from waiting_for_approval wa inner join deleted d on wa.content_item_id = d.content_item_id

delete child_delays from child_delays cd inner join deleted d on cd.child_id = d.content_item_id

IF dbo.qp_get_version_control() IS NOT NULL BEGIN
    delete content_item_version from content_item_version civ inner join deleted d on civ.content_item_id = d.content_item_id
    
    delete item_to_item_version from item_to_item_version iiv 
    inner join content_item_version civ on civ.content_item_version_id = iiv.content_item_version_id
    inner join deleted d on d.content_item_id = civ.content_item_id 

    delete item_to_item_version from item_to_item_version iiv 
    inner join deleted d on d.content_item_id = iiv.linked_item_id 
END

delete item_link_united_full from item_link_united_full ii where ii.item_id in (select content_item_id from deleted) 

DELETE from FIELD_ARTICLE_BIND where [ARTICLE_ID] in (select content_item_id from deleted)

delete content_data from content_data cd inner join deleted d on cd.content_item_id = d.content_item_id

delete content_item from content_item ci inner join deleted d on ci.content_item_id = d.content_item_id

END

GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.16', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.16 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.17
-- Copy contents
-- **************************************

CREATE PROCEDURE [dbo].[qp_add_contents_new_site]
    @oldSiteId numeric,
    @newSiteId numeric
AS
BEGIN
--копирование контентов
INSERT INTO [dbo].[content] ([CONTENT_NAME]
      ,[DESCRIPTION]
      ,[SITE_ID]
      ,[CREATED]
      ,[MODIFIED]
      ,[LAST_MODIFIED_BY]
      ,[friendly_name_plural]
      ,[friendly_name_singular]
      ,[allow_items_permission]
      ,[content_group_id]
      ,[external_id]
      ,[virtual_type]
      ,[virtual_join_primary_content_id]
      ,[is_shared]
      ,[AUTO_ARCHIVE]
      ,[max_num_of_stored_versions]
      ,[version_control_view]
      ,[content_page_size]
      ,[MAP_AS_CLASS]
      ,[NET_CONTENT_NAME]
      ,[NET_PLURAL_CONTENT_NAME]
      ,[USE_DEFAULT_FILTRATION]
      ,[ADD_CONTEXT_CLASS_NAME]
      ,[query]
      ,[alt_query]
      ,[XAML_VALIDATION]
      ,[DISABLE_XAML_VALIDATION]
      ,[DISABLE_CHANGING_ACTIONS])
SELECT [CONTENT_NAME]
      ,[DESCRIPTION]
      ,@newSiteId
      ,[CREATED]
      ,[MODIFIED]
      ,[LAST_MODIFIED_BY]
      ,[friendly_name_plural]
      ,[friendly_name_singular]
      ,[allow_items_permission]
      ,[content_group_id]
      ,[external_id]
      ,[virtual_type]
      ,[virtual_join_primary_content_id]
      ,[is_shared]
      ,[AUTO_ARCHIVE]
      ,[max_num_of_stored_versions]
      ,[version_control_view]
      ,[content_page_size]
      ,[MAP_AS_CLASS]
      ,[NET_CONTENT_NAME]
      ,[NET_PLURAL_CONTENT_NAME]
      ,[USE_DEFAULT_FILTRATION]
      ,[ADD_CONTEXT_CLASS_NAME]
      ,[query]
      ,[alt_query]
      ,[XAML_VALIDATION]
      ,[DISABLE_XAML_VALIDATION]
      ,[DISABLE_CHANGING_ACTIONS]
  FROM [dbo].[content]
  where site_id = @oldSiteId
  
DECLARE @RelsContents table(
        CONTENT_ID_OLD numeric(18,0)
        ,CONTENT_ID_NEW numeric(18,0)

)
INSERT INTO @RelsContents
SELECT c.CONTENT_ID as CONTENT_ID_OLD, nc.CONTENT_ID as CONTENT_ID_NEW 
FROM [dbo].[CONTENT] as c
INNER JOIN [dbo].[CONTENT] as nc on nc.CONTENT_NAME = c.CONTENT_NAME AND nc.SITE_ID = @newSiteId
where c.SITE_ID = @oldSiteId

--копирование связей между контентами

INSERT INTO [dbo].content_to_content (
        [l_content_id]
      ,[r_content_id]
      ,[MAP_AS_CLASS]
      ,[NET_LINK_NAME]
      ,[NET_PLURAL_LINK_NAME]
      ,[SYMMETRIC])
SELECT
       (select CONTENT_ID_NEW from @RelsContents where CONTENT_ID_OLD = l_content_id)
      ,(select CONTENT_ID_NEW from @RelsContents where CONTENT_ID_OLD = r_content_id)
      ,cc.[MAP_AS_CLASS]
      ,[NET_LINK_NAME]
      ,[NET_PLURAL_LINK_NAME]
      ,[SYMMETRIC]
FROM [dbo].content_to_content as cc
INNER JOIN [dbo].CONTENT as c ON c.CONTENT_ID = l_content_id OR c.CONTENT_ID = r_content_id
WHERE c.SITE_ID = @oldSiteId

--Копирование групп
INSERT INTO [dbo].[content_group]
SELECT @newSiteId
      ,[name]
  FROM [dbo].[content_group]
  WHERE site_id = @oldSiteId
  
--обновление групп
DECLARE @RelsGroups table(
        content_group_id_old numeric(18,0)
        ,content_group_id_new numeric(18,0)
)
        
INSERT INTO @RelsGroups
SELECT c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
FROM [dbo].[content_group] as c
INNER JOIN [dbo].[content_group] as nc on nc.name = c.Name AND nc.SITE_ID = @newSiteId
where c.SITE_ID = @oldSiteId

UPDATE [dbo].content
SET content_group_id = (SELECT content_group_id_new from @RelsGroups where content_group_id = content_group_id_old)
where SITE_ID = @newSiteId

-- копирование полей
INSERT INTO CONTENT_ATTRIBUTE
(	[CONTENT_ID]
      ,[ATTRIBUTE_NAME]
      ,[FORMAT_MASK]
      ,[INPUT_MASK]
      ,[ATTRIBUTE_SIZE]
      ,[DEFAULT_VALUE]
      ,[ATTRIBUTE_TYPE_ID]
      ,[RELATED_ATTRIBUTE_ID] --накапливаем информацию
      ,[INDEX_FLAG]
      ,[DESCRIPTION]
      ,[MODIFIED]
      ,[CREATED]
      ,[LAST_MODIFIED_BY]
      ,[ATTRIBUTE_ORDER]
      ,[REQUIRED]
      ,[PERMANENT_FLAG]
      ,[PRIMARY_FLAG]
      ,[RELATION_CONDITION]
      ,[display_as_radio_button]
      ,[view_in_list]
      ,[READONLY_FLAG]
      ,[allow_stage_edit]
      ,[ATTRIBUTE_CONFIGURATION]
      ,[RELATED_IMAGE_ATTRIBUTE_ID] --накапливаем информацию
      ,[persistent_attr_id] --накапливаем информацию
      ,[join_attr_id] --накапливаем информацию
      ,[link_id] --накапливаем информацию
      ,[DEFAULT_BLOB_VALUE]
      ,[AUTO_LOAD]
      ,[FRIENDLY_NAME]
      ,[use_site_library]
      ,[use_archive_articles]
      ,[AUTO_EXPAND]
      ,[relation_page_size]
      ,[doctype]
      ,[full_page]
      ,[RENAME_MATCHED]
      ,[SUBFOLDER]
      ,[DISABLE_VERSION_CONTROL]
      ,[MAP_AS_PROPERTY]
      ,[NET_ATTRIBUTE_NAME]
      ,[NET_BACK_ATTRIBUTE_NAME]
      ,[P_ENTER_MODE]
      ,[USE_ENGLISH_QUOTES]
      ,[BACK_RELATED_ATTRIBUTE_ID] --накапливаем информацию
      ,[IS_LONG]
      ,[EXTERNAL_CSS]
      ,[ROOT_ELEMENT_CLASS]
      ,[USE_FOR_TREE]
      ,[AUTO_CHECK_CHILDREN]
      ,[AGGREGATED]
      ,[CLASSIFIER_ATTRIBUTE_ID] --накапливаем информацию
      ,[IS_CLASSIFIER]
      ,[CHANGEABLE]
      ,[USE_RELATION_SECURITY]
      ,[COPY_PERMISSIONS_TO_CHILDREN]
      ,[ENUM_VALUES]
      ,[SHOW_AS_RADIO_BUTTON]
      ,[USE_FOR_DEFAULT_FILTRATION]
      ,[TREE_ORDER_FIELD])
SELECT (SELECT CONTENT_ID_NEW FROM @RelsContents WHERE CONTENT_ID_OLD = CONTENT_ID)
      ,[ATTRIBUTE_NAME]
      ,[FORMAT_MASK]
      ,[INPUT_MASK]
      ,[ATTRIBUTE_SIZE]
      ,[DEFAULT_VALUE]
      ,[ATTRIBUTE_TYPE_ID]
      ,[RELATED_ATTRIBUTE_ID] --накапливаем информацию
      ,[INDEX_FLAG]
      ,[DESCRIPTION]
      ,[MODIFIED]
      ,[CREATED]
      ,[LAST_MODIFIED_BY]
      ,[ATTRIBUTE_ORDER]
      ,[REQUIRED]
      ,[PERMANENT_FLAG]
      ,[PRIMARY_FLAG]
      ,[RELATION_CONDITION]
      ,[display_as_radio_button]
      ,[view_in_list]
      ,[READONLY_FLAG]
      ,[allow_stage_edit]
      ,[ATTRIBUTE_CONFIGURATION]
      ,[RELATED_IMAGE_ATTRIBUTE_ID] --накапливаем информацию
      ,[persistent_attr_id] --накапливаем информацию
      ,[join_attr_id] --накапливаем информацию
      ,[link_id] --накапливаем информацию
      ,[DEFAULT_BLOB_VALUE]
      ,[AUTO_LOAD]
      ,[FRIENDLY_NAME]
      ,[use_site_library]
      ,[use_archive_articles]
      ,[AUTO_EXPAND]
      ,[relation_page_size]
      ,[doctype]
      ,[full_page]
      ,[RENAME_MATCHED]
      ,[SUBFOLDER]
      ,[DISABLE_VERSION_CONTROL]
      ,[MAP_AS_PROPERTY]
      ,[NET_ATTRIBUTE_NAME]
      ,[NET_BACK_ATTRIBUTE_NAME]
      ,[P_ENTER_MODE]
      ,[USE_ENGLISH_QUOTES]
      ,[BACK_RELATED_ATTRIBUTE_ID] --накапливаем информацию
      ,[IS_LONG]
      ,[EXTERNAL_CSS]
      ,[ROOT_ELEMENT_CLASS]
      ,[USE_FOR_TREE]
      ,[AUTO_CHECK_CHILDREN]
      ,[AGGREGATED]
      ,[CLASSIFIER_ATTRIBUTE_ID] --накапливаем информацию
      ,[IS_CLASSIFIER]
      ,[CHANGEABLE]
      ,[USE_RELATION_SECURITY]
      ,[COPY_PERMISSIONS_TO_CHILDREN]
      ,[ENUM_VALUES]
      ,[SHOW_AS_RADIO_BUTTON]
      ,[USE_FOR_DEFAULT_FILTRATION]
      ,[TREE_ORDER_FIELD]
  FROM [dbo].[CONTENT_ATTRIBUTE] 
  WHERE CONTENT_ID IN (SELECT CONTENT_ID_OLD FROM @RelsContents)
                AND ATTRIBUTE_NAME != 'Title'

-- удалить аттрибуты по умолчанию!

-- добавить аттрибуты схожие с аттрибутами по умолчанию

-- делаем соответствие между связанными аттрибутами
DECLARE @RelsAttrs table(
        Attr_OLD numeric(18,0)
        ,Attr_NEW numeric(18,0)
)

INSERT INTO @RelsAttrs
SELECT ca.ATTRIBUTE_ID as CAT_Old
    ,ca1.ATTRIBUTE_ID as CAT_New 
FROM [dbo].CONTENT_ATTRIBUTE as ca
INNER JOIN @RelsContents as ra ON ra.CONTENT_ID_OLD = ca.CONTENT_ID
LEFT JOIN [dbo].CONTENT_ATTRIBUTE as ca1 ON ca1.ATTRIBUTE_NAME = ca.ATTRIBUTE_NAME AND ca1.CONTENT_ID = (SELECT CONTENT_ID_NEW FROM @RelsContents where CONTENT_ID_OLD = ca.CONTENT_ID)
WHERE ca.CONTENT_ID IN (SELECT CONTENT_ID_OLD from @RelsContents)
ORDER BY ca.ATTRIBUTE_ID

SELECT * FROM @RelsAttrs

UPDATE [dbo].[CONTENT_ATTRIBUTE]
SET		[RELATED_ATTRIBUTE_ID] = (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = RELATED_ATTRIBUTE_ID)
      ,[RELATED_IMAGE_ATTRIBUTE_ID]= (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = RELATED_IMAGE_ATTRIBUTE_ID)
      ,[persistent_attr_id]= (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = persistent_attr_id)
      ,[join_attr_id]= (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = join_attr_id)
      ,[BACK_RELATED_ATTRIBUTE_ID]= (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = BACK_RELATED_ATTRIBUTE_ID)
      ,[CLASSIFIER_ATTRIBUTE_ID]= (SELECT Attr_NEW from @RelsAttrs where Attr_OLD = CLASSIFIER_ATTRIBUTE_ID)
WHERE CONTENT_ID IN (SELECT CONTENT_ID_NEW from @RelsContents)

END
GO

-- сделать соответствие между линками (между контентами)


UPDATE [dbo].BACKEND_ACTION
SET SHORT_NAME = 'Create Like', 
    CONTROLLER_ACTION_URL = '~/CopySite/',
    TYPE_ID = dbo.qp_action_type_id('copy'),
    ENTITY_TYPE_ID =  dbo.qp_entity_type_id('site'),
    IS_MULTISTEP = 1, 
    HAS_SETTINGS = 1,
    WINDOW_WIDTH = 650,
    WINDOW_HEIGHT = 800,
    IS_WINDOW = 1
where code = 'copy_site'
GO

insert into [dbo].CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, ICON, [ORDER], BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('site'), dbo.qp_action_id('copy_site'), 'Create like', 'create_like.gif', 45, 1)

GO


INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.17', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.17 completed'
GO


-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.18
-- FK_TREE_ORDER_FIELD NAME fix
-- **************************************

exec qp_delete_constraint 'CONTENT_ATTRIBUTE', 'TREE_ORDER_FIELD'
GO

ALTER TABLE [dbo].[CONTENT_ATTRIBUTE]  WITH CHECK ADD  CONSTRAINT [FK_CONTENT_ATTRIBUTE_TREE_ORDER_FIELD] FOREIGN KEY([TREE_ORDER_FIELD])
REFERENCES [dbo].[CONTENT_ATTRIBUTE] ([ATTRIBUTE_ID])

GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.18', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.18 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.19
-- SQL 2008 fix (SQL 2005 is not supported later)
-- **************************************
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'Ids' AND ss.name = N'dbo')
DROP TYPE [dbo].[Ids]
GO

CREATE TYPE [dbo].[Ids] AS TABLE(
    [ID] [numeric](18, 0) NULL
)
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.19', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.19 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.20
-- Remove Default Action
-- **************************************

update ENTITY_TYPE set DEFAULT_ACTION_ID = null where CODE = 'content_group'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.20', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.20 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.21
-- Refactoring stored procedures (import)
-- **************************************

ALTER PROCEDURE [dbo].[qp_update_o2mfieldvalues]
    @xmlParameter xml
AS
BEGIN
    DECLARE @NewO2MValues TABLE (CONTENT_ITEM_ID int, LINKED_ID int, FIELD_ID int)	
    
    INSERT INTO @NewO2MValues
        SELECT
         doc.col.value('./@id', 'int') CONTENT_ITEM_ID
         ,doc.col.value('./@linked_id', 'int') LINKED_ID
         ,doc.col.value('./@field_id', 'int') FIELD_ID
        FROM @xmlParameter.nodes('/items/item') doc(col)

        BEGIN  
           UPDATE
            [dbo].[CONTENT_DATA]
        SET
            [dbo].[CONTENT_DATA].[DATA] = [@NewO2MValues].LINKED_ID
        FROM
            [dbo].[CONTENT_DATA]
        INNER JOIN
            @NewO2MValues
        ON
            [dbo].[CONTENT_DATA].CONTENT_ITEM_ID = [@NewO2MValues].CONTENT_ITEM_ID AND
            [dbo].[CONTENT_DATA].ATTRIBUTE_ID = [@NewO2MValues].FIELD_ID

        END
END
GO

ALTER PROCEDURE [dbo].[qp_update_acrticle_modification_date]
    @xmlParameter xml
AS
BEGIN
    
DECLARE @ModifiedArticles TABLE (CONTENT_ITEM_ID int, LAST_MODIFIED_BY int)	
    
INSERT INTO @ModifiedArticles
SELECT
         doc.col.value('./@id', 'int') CONTENT_ITEM_ID
         ,doc.col.value('./@modifiedBy', 'int') modifiedBy
        FROM @xmlParameter.nodes('/items/item') doc(col)

        BEGIN  
           UPDATE
            [dbo].[CONTENT_ITEM]
        SET
            [dbo].[CONTENT_ITEM].[MODIFIED] = GETDATE()
            ,[dbo].[CONTENT_ITEM].[LAST_MODIFIED_BY] = [@ModifiedArticles].[LAST_MODIFIED_BY]
        FROM
            [dbo].[CONTENT_ITEM]
        INNER JOIN
            @ModifiedArticles
        ON
            [dbo].[CONTENT_ITEM].CONTENT_ITEM_ID = [@ModifiedArticles].[CONTENT_ITEM_ID]

        END
END
GO

ALTER PROCEDURE [dbo].[qp_insert_m2m_field_Values]
    @xmlParameter xml
AS
BEGIN
    DECLARE @fieldValues TABLE
    (
      linkId int, 
      id int,
      linkedId int,
      splitted bit 
    )
    INSERT INTO @fieldValues
    SELECT
         doc.col.value('./@linkId', 'int') linkId
         ,doc.col.value('./@id', 'int') id
         ,doc.col.value('./@linkedId', 'int') linkedId
         ,c.SPLITTED as splitted
        FROM @xmlParameter.nodes('/items/item') doc(col)
    
    INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')
    
    INSERT INTO [dbo].[item_to_item] (link_id, l_item_id, r_item_id)
    SELECT linkId, id, linkedId FROM @fieldValues where splitted = 0
    
    INSERT INTO [dbo].[item_link_async]  (link_id, item_id, linked_item_id)
    SELECT linkId, id, linkedId FROM @fieldValues where splitted = 1
    
END
GO

ALTER PROCEDURE [dbo].[qp_insertArticleValues]
    @xmlParameter xml
AS
BEGIN
    DECLARE @NewArticles TABLE (CONTENT_ITEM_ID int, ATTRIBUTE_ID int, DATA nvarchar(3500), BLOB_DATA nvarchar(max))	
    
    INSERT INTO @NewArticles
        SELECT
         doc.col.value('(CONTENT_ITEM_ID)[1]', 'int') CONTENT_ITEM_ID
        ,doc.col.value('(ATTRIBUTE_ID)[1]', 'int') ATTRIBUTE_ID
        ,doc.col.value('(DATA)[1]', 'nvarchar(3500)') DATA  
        ,doc.col.value('(BLOB_DATA)[1]', 'nvarchar(max)') BLOB_DATA 
        FROM @xmlParameter.nodes('/PARAMETERS/FIELDVALUE') doc(col)

        BEGIN  
           UPDATE
            [dbo].[CONTENT_DATA]
        SET
            [dbo].[CONTENT_DATA].[DATA] = CASE WHEN ([@NewArticles].[DATA] IS NULL OR [@NewArticles].[DATA] = '') THEN NULL ELSE [@NewArticles].[DATA] END,
            [dbo].[CONTENT_DATA].[BLOB_DATA] = CASE WHEN ([@NewArticles].[BLOB_DATA] IS NULL OR [@NewArticles].[BLOB_DATA] = '') THEN NULL ELSE [@NewArticles].[BLOB_DATA] END
        FROM
            [dbo].[CONTENT_DATA]
        INNER JOIN
            @NewArticles
        ON
            [dbo].[CONTENT_DATA].CONTENT_ITEM_ID = [@NewArticles].CONTENT_ITEM_ID AND
            [dbo].[CONTENT_DATA].ATTRIBUTE_ID = [@NewArticles].ATTRIBUTE_ID

        END
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.21', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.21 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.22
-- Insert/update m2mValues
-- **************************************

ALTER PROCEDURE [dbo].[qp_insert_m2m_field_Values]
    @xmlParameter xml
AS
BEGIN
    DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
    INSERT INTO @fieldValues
    SELECT
        ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber 
         ,doc.col.value('./@id', 'int') id
         ,doc.col.value('./@linkId', 'int') linkId
         ,doc.col.value('./@value', 'nvarchar(max)') value
         ,c.SPLITTED as splitted
        FROM @xmlParameter.nodes('/items/item') doc(col)
        INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')
    
    DECLARE @I int
    SET @I = 1
    DECLARE @Count int
    SET @Count = (SELECT COUNT(*) from @fieldValues)
    
    WHILE @I <= @Count
    BEGIN
        INSERT INTO @rowValues 
        SELECT id, linkId, value, splitted FROM @fieldValues where rowNumber = @I
        
        DECLARE @rowId numeric
        SET @rowId = (SELECT CONVERT(numeric, id) from @rowValues)
        
        DECLARE @rowLinkId numeric
        SET @rowLinkId = (SELECT CONVERT(numeric, linkId) from @rowValues)
        
        DECLARE @rowSplitted bit
        SET @rowSplitted = (SELECT CONVERT(bit, splitted) from @rowValues)
        
        DECLARE @rowValue nvarchar(max)
        SET @rowValue = (SELECT CONVERT(nvarchar(max), value) from @rowValues)
            
        exec dbo.qp_update_m2m @id = @rowId, @linkId = @rowLinkId, @value = @rowValue, @splitted = @rowSplitted
        SET @I = @I + 1
        DELETE FROM @rowValues
    END
    
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.22', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.22 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.23
-- Articles history doesnt open in new tab for one content
-- **************************************

UPDATE [dbo].[BACKEND_ACTION]
SET TAB_ID = dbo.qp_tab_id('Audit Trail')
where CODE = 'list_status_history'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.23', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.23 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.24
-- Articles history doesnt open in new tab for one content
-- **************************************

INSERT INTO [dbo].[ENTITY_TYPE](
      [NAME]
      ,[CODE]
      ,[PARENT_ID]
      ,[ORDER]
      ,[SOURCE]
      ,[ID_FIELD]
      ,[ORDER_FIELD]
      ,[PARENT_ID_FIELD]
      ,[DEFAULT_ACTION_ID]
  )
  VALUES('Article Status', 'article_status', dbo.qp_entity_type_id('article'), 30, 'CONTENT_ITEM_STATUS_HISTORY', 'STATUS_HISTORY_ID', '[CREATED]', 'CONTENT_ITEM_ID', dbo.qp_action_id('list_status_history'))
  GO

  UPDATE dbo.BACKEND_ACTION
  SET ENTITY_TYPE_ID = dbo.qp_entity_type_id('article_status')
  where CODE = 'list_status_history'
  GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.24', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.24 completed'
GO

-- ************************************** 
-- Kirill Zakirov
-- version 7.9.7.25
-- Search In Archive Articles
-- **************************************

insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, CONTROLLER_ACTION_URL, IS_INTERFACE)
values('Search in Archive Articles', 'search_in_archive_articles', dbo.qp_action_type_id('search'), dbo.qp_entity_type_id('site'), '~/Site/SearchInArchiveArticles/', 1)

insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
values('Refresh Search in Archive Articles', 'refresh_search_in_archive_articles', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('site'))

insert into CONTEXT_MENU_ITEM(CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('site'), dbo.qp_action_id('search_in_archive_articles'), 'Search in Archive Articles', 21, 'search.gif', 0)

insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, ICON, ICON_DISABLED, [ORDER], IS_COMMAND)
values (dbo.qp_action_id('search_in_archive_articles'), dbo.qp_action_id('refresh_search_in_archive_articles'), 'Refresh', 'refresh.gif', NULL, 1, 0)

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.25', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.25 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.26
-- Error while updating content
-- **************************************

ALTER procedure [dbo].[qp_expand](
    @user_id numeric = 0, 
    @code nvarchar(50) = null, 
    @id bigint = 0, 
    @is_folder bit = 0, 
    @is_group bit = 0, 
    @group_item_code nvarchar(50) = null,
    @filter_id bigint = 0,
    @count_only bit = 0, 
    @count int = 0 output 
)
as
begin
    declare @result table
    (
        NUMBER int primary key identity(1, 1),
        ID bigint not null,
        PARENT_ID bigint null,
        PARENT_GROUP_ID bigint null,
        CODE nvarchar(50) null,
        TITLE nvarchar(255) not null,
        IS_FOLDER bit null,
        IS_GROUP bit null,
        GROUP_ITEM_CODE nvarchar(50),
        ICON nvarchar(255) null,
        ICON_MODIFIER nvarchar(10) null,
        CONTEXT_MENU_ID bigint null,
        CONTEXT_MENU_CODE nvarchar(50) null,
        DEFAULT_ACTION_ID bigint null,
        DEFAULT_ACTION_CODE nvarchar(50) null,
        HAS_CHILDREN bit null,
        IS_RECURRING bit null
    )
    
    declare @language_id numeric(18, 0)
    declare @source nvarchar(50), @id_field nvarchar(50), @title_field nvarchar(50)
    declare @parent_id_field nvarchar(50), @icon_field nvarchar(50), @group_parent_id_field nvarchar(50)
    declare @icon_modifier_field nvarchar(50), @order_field nvarchar(50)
    declare @folder_icon nvarchar(50), @has_item_nodes bit
    declare @recurring_id_field nvarchar(50), @source_sp nvarchar(50)
    declare @id_str nvarchar(10), @parent_id bigint, @filter_id_str nvarchar(10)
    declare @default_action_id int, @context_menu_id int
    declare @is_admin bit, @current_is_group bit
    declare @parent_group_code nvarchar(50), @child_group_code nvarchar(50), @current_group_item_code nvarchar(50)
    declare @real_parent_id bigint, @real_parent_id_field nvarchar(50), @real_id_str nvarchar(10)
    
    set @id_str = CAST(@id as nvarchar(10))
    
    if (@filter_id = 0)
        set @filter_id_str = ''
    else
        set @filter_id_str = CAST(@filter_id as nvarchar(10))
    
    select @parent_group_code = ET1.CODE from ENTITY_TYPE ET2 INNER JOIN ENTITY_TYPE ET1 ON ET2.GROUP_PARENT_ID = ET1.ID where ET2.CODE = @code
    and dbo.qp_check_entity_grouping(@user_id, @code) = 1
    
    if @is_group = 1
    begin
        exec dbo.qp_get_parent_entity_id @id, @code, @parent_entity_id = @real_parent_id output
        set @real_id_str = CAST(@real_parent_id as nvarchar(10))
    end
    else begin
        set @real_parent_id = @id
        set @real_id_str = @id_str	
    end
    
    set @current_is_group = 0
    if @parent_group_code is not null begin
        if @is_folder = 1 begin
            set @current_group_item_code = @code
            set @code = @parent_group_code
            set @current_is_group = 1
        end
    end
    else if @group_item_code is not null begin
        if @is_folder = 0 begin
            set @is_folder = 1
            set @code = @group_item_code
        end
    End

    set @language_id = dbo.qp_language(@user_id)
    
    set @is_admin = 0;
    IF EXISTS (select * from user_group_bind where group_id = 1 and user_id = @user_Id) OR @user_id = 1
        set @is_admin = 1;
    
    select 
        @source = source,
        @source_sp = source_sp,
        @id_field = id_field,
        @title_field = TITLE_FIELD, 
        @parent_id_field = PARENT_ID_FIELD,
        @group_parent_id_field = GROUP_PARENT_ID_FIELD,
        @icon_field = ICON_FIELD, 
        @icon_modifier_field = ICON_MODIFIER_FIELD, 
        @folder_icon = FOLDER_ICON, 
        @has_item_nodes = HAS_ITEM_NODES, 
        @recurring_id_field = RECURRING_ID_FIELD, 
        @order_field = order_field, 
        @default_action_id = default_action_id,
        @context_menu_id = CONTEXT_MENU_ID
    from 
        ENTITY_TYPE ET
    where
        ID = dbo.qp_entity_type_id(@code)
        
    if @is_group = 1 
    begin
        set @real_parent_id_field = @parent_id_field
        set @parent_id_field = @group_parent_id_field
    end
    
    if @icon_field is null
        set @icon_field = 'NULL'
    if @icon_modifier_field is null
        set @icon_modifier_field = 'NULL'	
    

    if @is_folder = 1 or @recurring_id_field is not null
    begin
        declare @sql nvarchar(max), @select nvarchar(max), @where nvarchar(max), @order nvarchar(max)

        if @has_item_nodes = 1
        begin
            set @select = @source + '.' + @id_field + ' AS ID, ' + @title_field + ' AS TITLE,  '  + @icon_field + ' AS ICON,  ' + @icon_modifier_field + ' AS ICON_MODIFIER'
            
            set @where = '1 = 1'
            if @parent_id_field is not null and @id_str <> '0' and  @id_str <> ''
                set @where = @where + ' AND ' + @parent_id_field + ' = ' + @id_str
            
            if @recurring_id_field is not null
            begin
                if @is_folder = 1 
                    set @where = @where + ' AND ' + @recurring_id_field + ' is null ' 
                else
                    set @where = @where + ' AND ' + @recurring_id_field + ' = ' + @id_str
            end
            
            if @filter_id_str <> '0' and @filter_id_str <> ''
                set @where = @where + ' AND ' + @source + '.' + @id_field + ' = ' + @filter_id_str
            
            if @order_field is null
                set @order = @title_field
            else
                set @order = @order_field  
            
        end
        
        
        if @source_sp is null
            set @sql = 'select ' + @select + ' from ' +  @source + ' where ' + @where + ' order by ' + @order
        else
        begin
            set @sql = 'exec ' + @source_sp + ' @user_id = ' + cast(@user_id as nvarchar(10)) + ', @permission_level = 1, @select = ''' + @select + ''', @filter = ''' + @where + ''', @order_by = ''' + @order + ''''
            if @real_parent_id_field is not null
                set @sql = @sql + ', @' + LOWER(@real_parent_id_field) + '=' + @real_id_str
            else if @parent_id_field is not null
                set @sql = @sql + ', @' + LOWER(@parent_id_field) + '=' + @id_str
            if @recurring_id_field is not null
                if @is_folder = 1
                    set @sql = @sql + ', @' + LOWER(@recurring_id_field) + '=0'
                else
                    set @sql = @sql + ', @' + LOWER(@recurring_id_field) + '=' + @id_str			
                
            
        end
        print @sql
        insert into @result (ID, TITLE, ICON, ICON_MODIFIER)
        exec sp_executesql @sql
        
        --PRINT @sql;
        
        
        if @count_only = 0
        begin
            --select ID, @id AS PARENT_ID, TITLE, @code AS CODE, 0 AS IS_FOLDER,  AS ICON, @default_action_id AS DEFAULT_ACTION_ID,  AS , dbo.qp_expand_count(@user_id, @code, ID, 0) AS CHILDREN_COUNT  from @result 
            update 
                @result
            set 
                PARENT_ID = @real_parent_id,
                PARENT_GROUP_ID = CASE WHEN @is_group = 1 THEN @id ELSE NULL END,
                CODE = @code, 
                IS_FOLDER = 0,
                IS_GROUP = @current_is_group,
                GROUP_ITEM_CODE = @current_group_item_code,
                ICON = dbo.qp_get_icon(ICON, @code, ICON_MODIFIER), 
                DEFAULT_ACTION_ID = @default_action_id, 
                CONTEXT_MENU_ID = @context_menu_id,
                IS_RECURRING = CASE WHEN @recurring_id_field is not null THEN 1 ELSE 0 END
        end
        else
            select @count = COUNT(ID) from @result	
    end
    else begin
        if @is_admin = 0
        begin
            declare @entitySecQuery nvarchar(max);
            EXEC [dbo].[qp_GetEntityPermissionAsQuery]
                @user_id = @user_id,	
                @SQLOut = @entitySecQuery OUTPUT
            
            CREATE TABLE #sectmp
            (
                PERMISSION_LEVEL int,
                ENTITY_TYPE_ID int
            );				
            set @entitySecQuery = N'insert into #sectmp (PERMISSION_LEVEL, ENTITY_TYPE_ID) ' + @entitySecQuery;
            exec sp_executesql @entitySecQuery;
        end
        
        declare @entitySql nvarchar(max), @condition nvarchar(max)
        set @condition = ' ET.DISABLED = 0 '
        if @code is null
            set @condition = @condition + ' AND ET.PARENT_ID is null '
        else
            set @condition = @condition + ' AND ET.PARENT_ID = dbo.qp_entity_type_id(''' + @code + ''') '
            
        if @is_admin = 0
            set @condition = @condition + ' AND S.PERMISSION_LEVEL > 0 '
            
        if @filter_id_str <> '0' and @filter_id_str <> ''
            set @condition = @condition + ' AND ET.ID = ' + @filter_id_str
            
        if @count_only = 0
        begin
            if @code is not null 
                set @entitySql = ' select ET.ID, ' + @id_str + ', dbo.qp_translate(dbo.qp_pluralize(ET.NAME), ' + cast(@language_id as nvarchar(10)) + '), ET.CODE, 1, 0, dbo.qp_get_icon(NULL, dbo.qp_pluralize(ET.CODE), NULL), ET.FOLDER_DEFAULT_ACTION_ID, ET.FOLDER_CONTEXT_MENU_ID ' + CHAR(13) 
            else
                set @entitySql = ' select ET.ID, ' + @id_str + ', ET.NAME, ET.CODE, 0, 0, dbo.qp_get_icon(NULL, ET.CODE, NULL), ET.DEFAULT_ACTION_ID, ET.CONTEXT_MENU_ID ' + CHAR(13) 
        end
        else
            set @entitySql = ' select @count = COUNT(ET.ID) ' + CHAR(13) 
        
        set @entitySql = @entitySql + ' From ENTITY_TYPE ET ' + CHAR(13)
        
        if @is_admin = 0
            set @entitySql = @entitySql + ' INNER JOIN #sectmp S ON S.ENTITY_TYPE_ID = ID ' + CHAR(13)
            
        set @entitySql = @entitySql + ' WHERE ' + @condition  + CHAR(13)
        
        if @count_only = 0
        begin
            set @entitySql = @entitySql + ' order by ET.[ORDER] ' + CHAR(13)
            print @entitySql
            insert into @result(ID, PARENT_ID, TITLE, CODE, IS_FOLDER, IS_GROUP, ICON, DEFAULT_ACTION_ID, CONTEXT_MENU_ID)
            exec sp_executesql @entitySql
        end
        else begin
            print @entitySql
            exec sp_executesql @entitySql, N'@count int output', @count = @count output
        end
    end
    
    if @count_only = 0
    begin
        declare @i numeric, @total numeric
        declare @local_code nvarchar(50), @local_id numeric, @local_parent_id numeric, @local_is_folder bit, @local_is_recurring bit
        declare @local_is_group bit, @local_group_item_code nvarchar(50)
        declare @children_count int
        set @children_count = 0
        set @i = 1
        select @total = COUNT(NUMBER) from @result
        while @i <= @total
        begin
            select @local_code = code, @local_id = id, @local_parent_id = parent_id, @local_is_folder = is_folder, 
            @local_is_group = is_group, @local_is_recurring = is_recurring, @local_group_item_code = GROUP_ITEM_CODE from @result where NUMBER = @i
            
            if @local_is_folder = 1
                exec dbo.qp_expand @user_id, @local_code, @local_parent_id, 1, @local_is_group, @local_group_item_code, 0, 1, @count = @children_count output
            else
            begin
                if @i = 1 or @local_is_recurring = 1 or @local_is_group = 1
                begin
                    exec dbo.qp_expand @user_id, @local_code, @local_id, 0, @local_is_group, @local_group_item_code, 0, 1, @count = @children_count output
                end
            end
            if @children_count = 0
                update @result set has_children = 0 where NUMBER = @i
            else
                update @result set has_children = 1 where NUMBER = @i
            
            set @i = @i + 1
        end
        
        select 
            TREE_NODE.ID,
            TREE_NODE.CODE, 			
            TREE_NODE.PARENT_ID,
            TREE_NODE.PARENT_GROUP_ID,
            TREE_NODE.IS_FOLDER,
            TREE_NODE.IS_GROUP,
            TREE_NODE.GROUP_ITEM_CODE, 
            TREE_NODE.ICON, 
            TREE_NODE.TITLE, 
            dbo.qp_action_code(TREE_NODE.DEFAULT_ACTION_ID) AS DEFAULT_ACTION_CODE, 
            ACTION_TYPE.CODE AS DEFAULT_ACTION_TYPE_CODE,
            dbo.qp_context_menu_code(TREE_NODE.CONTEXT_MENU_ID) AS CONTEXT_MENU_CODE, 
            TREE_NODE.HAS_CHILDREN
        from
            @result AS TREE_NODE
        left outer join
            BACKEND_ACTION
        on
            TREE_NODE.DEFAULT_ACTION_ID = BACKEND_ACTION.ID	
        left outer join
            ACTION_TYPE
        on
            BACKEND_ACTION.TYPE_ID = ACTION_TYPE.ID	
    end
end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.26', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.26 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.27
-- Content Link entity type
-- **************************************

if not exists(select * from ENTITY_TYPE where CODE = 'content_link')
insert into ENTITY_TYPE(NAME, CODE, PARENT_ID, SOURCE, ID_FIELD, PARENT_ID_FIELD, DISABLED)
values ('Content Link', 'content_link', dbo.qp_entity_type_id('site'), 'SITE_CONTENT_LINK', 'LINK_ID', 'SITE_ID', 1)
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.27', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.27 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.28
-- Undo search actions
-- **************************************

delete from action_toolbar_button where action_id = dbo.qp_action_id('refresh_search_in_archive_articles')
delete from context_menu_item where action_id = dbo.qp_action_id('search_in_archive_articles')
delete from BACKEND_ACTION where code = 'search_in_archive_articles'
delete from BACKEND_ACTION where code = 'refresh_search_in_archive_articles'

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.28', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.28 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.29
-- Merge with product catalog version
-- **************************************
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT' AND COLUMN_NAME = 'PARENT_CONTENT_ID') 
BEGIN
    ALTER TABLE CONTENT ADD PARENT_CONTENT_ID NUMERIC (18, 0) NULL 
    CONSTRAINT FK_CONTENT_PARENT_CONTENT_ID FOREIGN KEY (PARENT_CONTENT_ID) REFERENCES [CONTENT](CONTENT_ID)	
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'PARENT_ATTRIBUTE_ID') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD PARENT_ATTRIBUTE_ID NUMERIC (18, 0) NULL 
    CONSTRAINT FK_CONTENT_ATTRIBUTE_PARENT_ATTRIBUTE_ID FOREIGN KEY (PARENT_ATTRIBUTE_ID) REFERENCES [CONTENT_ATTRIBUTE](ATTRIBUTE_ID)	
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'HIDE') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD HIDE BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_HIDE DEFAULT 0  
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'OVERRIDE') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD OVERRIDE BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUITE_OVERRIDE DEFAULT 0  
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT' AND COLUMN_NAME = 'USE_FOR_CONTEXT') 
BEGIN
    ALTER TABLE CONTENT ADD USE_FOR_CONTEXT BIT NOT NULL CONSTRAINT DF_CONTENT_USE_FOR_CONTEXT DEFAULT 0   
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_FOR_CONTEXT') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD USE_FOR_CONTEXT BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_USE_FOR_CONTEXT DEFAULT 0  
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'USE_FOR_VARIATIONS') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD USE_FOR_VARIATIONS BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_USE_FOR_VARIATIONS DEFAULT 0  
END

--IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_DATA' AND COLUMN_NAME = 'PARENT_CONTENT_DATA_ID') 
--BEGIN
--	ALTER TABLE CONTENT_DATA ADD PARENT_CONTENT_DATA_ID NUMERIC (18, 0) NULL 
--	CONSTRAINT FK_CONTENT_DATA_PARENT_CONTENT_DATA_ID FOREIGN KEY (PARENT_CONTENT_DATA_ID) REFERENCES [CONTENT_DATA](CONTENT_DATA_ID) 
--END
--GO

--CREATE TRIGGER [dbo].[tu_update_child_content_data] ON [dbo].[CONTENT_DATA] FOR UPDATE AS
--BEGIN
--	if object_id('tempdb..#disable_tu_update_child_content_data') is null
--	begin
--		UPDATE cd set DATA = src.DATA, BLOB_DATA = src.BLOB_DATA 
--		from CONTENT_DATA cd 
--		inner join inserted i on cd.PARENT_CONTENT_DATA_ID = i.CONTENT_DATA_ID
--		inner join CONTENT_DATA src on i.ATTRIBUTE_ID = src.ATTRIBUTE_ID and i.CONTENT_ITEM_ID = src.CONTENT_ITEM_ID
--	end
--END
--GO

--CREATE TRIGGER [dbo].[td_delete_child_content_data] ON [dbo].[CONTENT_DATA] FOR DELETE AS
--BEGIN
--	if object_id('tempdb..#disable_td_delete_child_content_data') is null
--	begin
--		DELETE cd 
--		from CONTENT_DATA cd inner join deleted d on cd.PARENT_CONTENT_DATA_ID = d.CONTENT_DATA_ID
--	end
--END
--GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.29', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.29 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.30
-- Merge with product catalog version (2)
-- **************************************

if not exists(select * from BACKEND_ACTION where CODE = 'save_field_and_up')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, NEXT_SUCCESSFUL_ACTION_ID)
values (dbo.qp_action_type_id('save_and_up'), dbo.qp_entity_type_id('field'), 'Save Field and Up', 'save_field_and_up', dbo.qp_action_id('edit_field'))

if not exists(select * from BACKEND_ACTION where CODE = 'update_field_and_up')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, NEXT_SUCCESSFUL_ACTION_ID)
values (dbo.qp_action_type_id('update_and_up'), dbo.qp_entity_type_id('field'), 'Update Field and Up', 'update_field_and_up', dbo.qp_action_id('edit_field'))

if not exists(select * from BACKEND_ACTION where CODE = 'save_content_and_up')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, NEXT_SUCCESSFUL_ACTION_ID)
values (dbo.qp_action_type_id('save_and_up'), dbo.qp_entity_type_id('content'), 'Save Content and Up', 'save_content_and_up', dbo.qp_action_id('edit_content'))

if not exists(select * from BACKEND_ACTION where CODE = 'update_content_and_up')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, NEXT_SUCCESSFUL_ACTION_ID)
values (dbo.qp_action_type_id('update_and_up'), dbo.qp_entity_type_id('content'), 'Update Content and Up', 'update_content_and_up', dbo.qp_action_id('edit_content'))

if not exists(select * from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('save_field_and_up'))
insert into action_toolbar_button(PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_action_id('new_field'), dbo.qp_action_id('save_field_and_up'), 'Save & Up', 15, 'saveup.gif')

if not exists(select * from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('update_field_and_up'))
insert into action_toolbar_button(PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_action_id('edit_field'), dbo.qp_action_id('update_field_and_up'), 'Save & Up', 25, 'saveup.gif')

if not exists(select * from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('save_content_and_up'))
insert into action_toolbar_button(PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_action_id('new_content'), dbo.qp_action_id('save_content_and_up'), 'Save & Up', 15, 'saveup.gif')

if not exists(select * from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('update_content_and_up'))
insert into action_toolbar_button(PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_action_id('edit_content'), dbo.qp_action_id('update_content_and_up'), 'Save & Up', 25, 'saveup.gif')
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.30', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.30 completed'
GO


-- ************************************** 
-- Pavel Celut
-- version 7.9.7.31
-- Fix error locked articles
-- **************************************

update content_item set locked = null, locked_by = null
where CONTENT_ID in (select content_id from CONTENT_ATTRIBUTE where AGGREGATED = 1)
and locked_by is not null
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.31', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.31 completed'
GO 

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.32
-- Fix conflict with Data field
-- **************************************

ALTER procedure [dbo].[qp_get_default_article]
@content_id numeric
as
begin

declare @sql nvarchar(max), @fields nvarchar(max), @prefixed_fields nvarchar(max)
 
if @content_id is not null
begin
    declare @attributes table
    (
        name nvarchar(255)
    )
    insert into @attributes
    select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
    
    SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
    SELECT @prefixed_fields = COALESCE(@prefixed_fields + ', ', '') + 'pt.[' + name + ']' FROM @attributes
    
    set @sql = N'select ' + @prefixed_fields  + N' from
    (
    select ca.ATTRIBUTE_NAME, CASE WHEN ca.attribute_type_id in (9, 10) THEN convert(nvarchar(max), ca.DEFAULT_BLOB_VALUE) ELSE ca.DEFAULT_VALUE END as pivot_data from CONTENT_ATTRIBUTE ca
    where ca.CONTENT_ID = @content_id) as src
    PIVOT
    (
    MAX(src.pivot_data)
    FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
    ) AS pt 
    '
    print @sql
    exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
end
end
GO

ALTER procedure [dbo].[qp_get_versions]
@item_id numeric,
@version_id numeric = 0
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
    
    declare @attributes table
    (
        name nvarchar(255)
    )
    
    declare @main_ids table
    (
        id numeric
    )
    
    insert into @main_ids
    select content_id from CONTENT_ATTRIBUTE where AGGREGATED = 1 and RELATED_ATTRIBUTE_ID in (select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where CONTENT_ID  = @content_id)
    
    insert into @main_ids
    values(@content_id)
    
    
    insert into @attributes(name) 
    select CASE c.CONTENT_ID WHEN @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + '.' + CA.ATTRIBUTE_NAME END 
    from content_attribute ca 
    inner join content c on ca.CONTENT_ID = c.CONTENT_ID 
    where ca.CONTENT_ID in (select id from @main_ids)
    order by C.CONTENT_ID, CA.attribute_order
    
    SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
    SELECT @prefixed_fields = COALESCE(@prefixed_fields + ', ', '') + 'pt.[' + name + ']' FROM @attributes
    
    if @version_id = 0
        set @version_sql = ''
    else
        set @version_sql = ' and vcd.CONTENT_ITEM_VERSION_ID= @version_id'
        
        
    declare @ids nvarchar(max)
    select @ids = coalesce(@ids + ', ', '') + cast(id as nvarchar(10)) from @main_ids
    
    set @sql = N'select pt.content_item_id, pt.version_id, pt.created, pt.created_by, pt.modified, pt.last_modified_by, ' + @prefixed_fields  + N' from
    (
    select civ.CONTENT_ITEM_ID, civ.CREATED, civ.CREATED_BY, civ.MODIFIED, civ.LAST_MODIFIED_BY, vcd.CONTENT_ITEM_VERSION_ID as version_id, 
    case ca.CONTENT_ID when @content_id THEN ca.ATTRIBUTE_NAME ELSE c.CONTENT_NAME + ''.'' + ca.ATTRIBUTE_NAME END AS ATTRIBUTE_NAME,
    dbo.qp_get_version_data(vcd.ATTRIBUTE_ID, vcd.CONTENT_ITEM_VERSION_ID) as pivot_data 
    from CONTENT_ATTRIBUTE ca
    INNER JOIN CONTENT c on ca.CONTENT_ID = c.CONTENT_ID
    left outer join VERSION_CONTENT_DATA vcd on ca.ATTRIBUTE_ID = vcd.ATTRIBUTE_ID
    inner join CONTENT_ITEM_VERSION civ on vcd.CONTENT_ITEM_VERSION_ID = civ.CONTENT_ITEM_VERSION_ID
    where ca.CONTENT_ID in (' + @ids + ') and civ.CONTENT_ITEM_ID = @item_id ' + @version_sql + ') as src
    PIVOT
    (
    MAX(src.pivot_data)
    FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
    ) AS pt order by pt.version_id desc

    '

    exec sp_executesql @sql, N'@content_id numeric, @item_id numeric, @version_id numeric', @content_id = @content_id, @item_id = @item_id, @version_id = @version_id
end
end
GO


ALTER procedure [dbo].[qp_get_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @prefixed_fields nvarchar(max)
declare @content_id numeric
select @content_id = content_id from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
    declare @attributes table
    (
        name nvarchar(255)
    )
    insert into @attributes
    select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
    
    SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

    set @sql = N'select * from
    (
    select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
    case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as pivot_data 
    from CONTENT_ATTRIBUTE ca
    left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
    inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
    where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
    ) as src
    PIVOT
    (
    MAX(src.pivot_data)
    FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
    ) AS pt order by pt.content_item_id desc
    '
    print @sql
    exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER procedure [dbo].[qp_update_with_content_data_pivot]
@item_id numeric
as
begin

declare @sql nvarchar(max), @version_sql nvarchar(100), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
declare @content_id numeric, @splitted bit
select @content_id = content_id, @splitted = SPLITTED from content_item ci where ci.CONTENT_ITEM_ID = @item_id
 
if @content_id is not null
begin
    
    set @table_name = 'content_' + CAST(@content_id as nvarchar)
    if (@splitted = 1)
        set @table_name = @table_name + '_async'
        
    declare @attributes table
    (
        name nvarchar(255)
    )
    insert into @attributes
    select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
    
    SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes
    
    SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
        
    set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
    (
    select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
    case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as pivot_data 
    from CONTENT_ATTRIBUTE ca
    left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
    inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
    where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID = @item_id
    ) as src
    PIVOT
    (
    MAX(src.pivot_data)
    FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
    ) AS pt
    on pt.content_item_id = base.content_item_id
    '
    print @sql
    exec sp_executesql @sql, N'@content_id numeric, @item_id numeric', @content_id = @content_id, @item_id = @item_id
end
end
GO

ALTER procedure [dbo].[qp_update_items_with_content_data_pivot]
@content_id numeric,
@ids nvarchar(max),
@is_async bit
as
begin

    declare @sql nvarchar(max), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
     
    set @table_name = 'content_' + CAST(@content_id as nvarchar)
    if (@is_async = 1)
    set @table_name = @table_name + '_async'
        
    declare @attributes table
    (
        name nvarchar(255) primary key
    )
    
    insert into @attributes
    select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id

    SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

    SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
        
    set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
    (
    select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
    case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as pivot_data 
    from CONTENT_ATTRIBUTE ca
    left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
    inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
    where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID in (' + @ids + ') 
    ) as src
    PIVOT
    (
    MAX(src.pivot_data)
    FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
    ) AS pt
    on pt.content_item_id = base.content_item_id
    '
    print @sql
    exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.32', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.32 completed'
GO 

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.33
-- Copying site
-- **************************************

--changing compatibility level of current database

DECLARE @SQL varchar(1000)
declare @level int
select @level = compatibility_level from sys.databases where name = db_name()
if @level < 100
begin
    SET @SQL = 'ALTER DATABASE ' + db_name() + ' SET COMPATIBILITY_LEVEL = 100 '
    exec(@SQL)
end
GO

CREATE FUNCTION [dbo].[GetRelationsBetweenContents] 
(
    @sourceSiteId int,
    @destinationSiteId int
)
RETURNS 
@relations_between_contents TABLE 
(
    source_content_id int,
    destination_content_id int
)
AS
BEGIN
    insert into @relations_between_contents
        select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @destinationSiteId
        where c.site_id = @sourceSiteId
    return 
END
GO

CREATE FUNCTION [dbo].[GetRelationsBetweenContentLinks]
(
    @sourceSiteId int,
    @destinationSourceId int
)
RETURNS 
@relations_between_contents_links TABLE 
(
    oldvalue int,
    newvalue int
)
AS
BEGIN
    insert into @relations_between_contents_links
    select oldvalues.link_id, newvalues.link_id from (
    select attribute_name, link_id, c.content_name
      from [dbo].[content_attribute] as ca
      inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
      inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'relation' 
      where c.site_id = @sourceSiteId and link_id is not null) as oldvalues
    inner join (
        select attribute_name, link_id, c.content_name
          from [dbo].[content_attribute] as ca
          inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0	
          inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'relation' 
          where c.site_id = @destinationSourceId and link_id is not null)
          as newvalues 
          on newvalues.attribute_name = oldvalues.attribute_name and newvalues.content_name = oldvalues.content_name
    return 
END
GO

CREATE procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @startfrom int,
        @endon int
as
begin
/****** script for selecttopnrows command from ssms  ******/

declare @relscontents table(
        content_id_old numeric(18,0)
        ,content_id_new numeric(18,0)

)
declare @relsattrs table(
        attr_old numeric(18,0)
        ,attr_new numeric(18,0)
)


declare @contentitemstable table( 
        newcontentitemid int,
        contentid int,
        oldcontentitemid int);

declare @copydata table(
        oldattributeid int,
        oldcontentitemid int,
        newdata nvarchar(3500),
        newblobdata ntext,
        newattributeid int,
        newcontentitemid int
)    
        
insert into @relscontents
        select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid


insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca
        inner join @relscontents as ra on ra.content_id_old = ca.content_id
        left join [dbo].content_attribute as ca1 on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.content_id_new
    
declare @todaysDateTime datetime
set @todaysDateTime = GetDate();

    ;with relations_between_statuses
    as (
        select st1.STATUS_TYPE_ID as old_status_type_id, st2.STATUS_TYPE_ID as new_status_type_id from [dbo].[status_type] as st1
        inner join [dbo].[status_type] as st2 on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
        where st1.SITE_ID = @oldSiteId
    ) 
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from (
            select row_number() over (order by content_item_id) as rownumber
              ,[content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.content_id_new as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,[not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
            from [dbo].[content_item] as c1
            inner join [dbo].[content] as c on c.content_id = c1.content_id
            inner join @relscontents as rc on rc.content_id_old = c1.content_id
            inner join relations_between_statuses as rbs on c1.[status_type_id] = rbs.old_status_type_id
            where c.site_id = @oldsiteid
        ) as t
          where t.rownumber between @startfrom and @endon
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;
            
            
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid
    
insert into @copydata
    select r.attribute_id
           ,r.content_item_id
           ,r.DATA
           ,r.blob_data
           ,ra.attr_new
           ,ci.newcontentitemid
     from [dbo].[content_data] as r
    inner join @relsattrs as ra on ra.attr_old = r.attribute_id
    inner join @contentitemstable as ci on ci.oldcontentitemid = r.content_item_id

    
update [dbo].[content_data] 
    set [data] = [@copydata].newdata
      ,[blob_data] = [@copydata].newblobdata
    from [dbo].[content_data] as cd
        inner join @copydata
            on
                cd.content_item_id = [@copydata].newcontentitemid and
                cd.attribute_id = [@copydata].newattributeid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID and at.[TYPE_NAME] != 'Dynamic Image'
update [dbo].[content_data] 
    set [data] = replace(cd1.newdata, 'field_' + CAST(cd1.oldattributeid as varchar), 'field_' + CAST(cd1.newattributeid as varchar))
    from [dbo].[content_data] as cd
        inner join @copydata as cd1
            on
                cd.content_item_id = cd1.newcontentitemid and
                cd.attribute_id = cd1.newattributeid and
                cd1.newdata is not null
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID and at.[TYPE_NAME] = 'Dynamic Image'		 
    
    select newcontentitemid, oldcontentitemid from @contentitemstable
end
GO


CREATE procedure [dbo].[qp_copy_site_contents]
    @oldsiteid numeric,
    @newsiteid numeric,
    @startFrom numeric,
    @endOn numeric
as
begin

    declare @todaysDate datetime
    set @todaysDate = GETDATE()

    declare @new_content_ids table (content_id int)
    -- copying contents
    ;with contents_with_row_number
    as
    (
        select ROW_NUMBER() over(order by content_id) as [row_number] 
            ,[content_name]
          ,[description]
          ,@newsiteid as siteId
          ,@todaysDate as created
          ,@todaysDate as modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from [dbo].[content]
      where site_id = @oldsiteid and virtual_type = 0
    )
    insert into [dbo].[content] ([content_name]
          ,[description]
          ,[site_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context])
    output inserted.CONTENT_ID
        into @new_content_ids
    select [content_name]
          ,[description]
          ,siteId
          ,created
          ,modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from contents_with_row_number
      where row_number between @startFrom and @endOn
  
  
    declare @relations_between_contents table(
            content_id_old int
            ,content_id_new int

    )
    insert into @relations_between_contents
    select source_content_id, destination_content_id from [dbo].GetRelationsBetweenContents(@oldSiteId, @newSiteId)
    
    -- copying attributes
    insert into content_attribute
    (	[content_id]
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,[modified]
          ,[created]
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field])
    select rbc.content_id_new
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,@todaysDate
          ,@todaysDate
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field]
      from [dbo].[content_attribute] as ca
      inner join @relations_between_contents as rbc on ca.CONTENT_ID = rbc.content_id_old
      inner join @new_content_ids as nci on rbc.content_id_new = nci.content_id
      where ca.attribute_name != 'Title'
      
    declare  @rels_attrs table(attr_old int, attr_new int);
    insert into @rels_attrs
        select ca.attribute_id as cat_old
            , ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca
        inner join @relations_between_contents as ra on ra.content_id_old = ca.content_id
        inner join @new_content_ids as nci on ra.content_id_new = nci.content_id
        left join [dbo].content_attribute as ca1 on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.content_id_new	
    
    update ca
    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
    from [dbo].[content_attribute] as ca
        inner join content as c 
            on ca.CONTENT_ID = c.CONTENT_ID 
        inner join @rels_attrs as ra
            on ca.ATTRIBUTE_ID = ra.attr_new
        inner join CONTENT_ATTRIBUTE as ca1 
            on ra.attr_old = ca1.ATTRIBUTE_ID
    where c.SITE_ID = @newsiteid
    
    insert into [dbo].[dynamic_image_attribute]
    select ra.attr_new
      ,[width]
      ,[height]
      ,[type]
      ,[quality]
      ,[max_size]
    from [dbo].[dynamic_image_attribute] as dia
        inner join @rels_attrs as ra on dia.ATTRIBUTE_ID = ra.attr_old
    
            
    select COUNT(*) from @new_content_ids
end
GO

CREATE PROCEDURE [dbo].[qp_copy_site_contents_update]
    @oldsiteid int,
    @newsiteid int
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    --copying links between contents
    declare @relations_between_content_links table(
        oldlink int,
        newlink int
    )
    
    declare @relations_between_contents table(
        source_content_id int,
        destination_content_id int
    )
    insert into @relations_between_contents 
    select source_content_id, destination_content_id from [dbo].GetRelationsBetweenContents(@oldSiteId, @newSiteId)
    
    merge [dbo].content_to_content as t
    using(
    select cc.[link_id]
          ,rbc.destination_content_id 
          ,rbc1.destination_content_id 
          ,cc.[map_as_class]
          ,[net_link_name]
          ,[net_plural_link_name]
          ,[symmetric]
        from [dbo].content_to_content as cc
        inner join [dbo].content as c on c.content_id = l_content_id
        inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
        inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
        where c.site_id = @oldsiteid
    )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
    on 0 = 1
    when not matched then
       insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       output src.[link_id], inserted.[link_id]
        into @relations_between_content_links;

    -- удалить аттрибуты по умолчанию!

    -- добавить аттрибуты схожие с аттрибутами по умолчанию

    -- делаем соответствие между связанными аттрибутами
    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
    )

    insert into @relsattrs
    select ca.attribute_id as cat_old
        ,ca1.attribute_id as cat_new 
    from [dbo].content_attribute as ca
    inner join @relations_between_contents as rbc on ca.content_id = rbc.source_content_id
    left join [dbo].content_attribute as ca1 on ca.attribute_name = ca1.attribute_name and ca1.content_id = rbc.destination_content_id

    update [dbo].[content_attribute]
    set		[related_attribute_id] = rai.attr_new
          ,[related_image_attribute_id]= ria.attr_new
          ,[persistent_attr_id]= pai.attr_new
          ,[join_attr_id]= jai.attr_new
          ,[back_related_attribute_id]= bra.attr_new
          ,[classifier_attribute_id]= cai.attr_new
    from [dbo].[content_attribute] as ca
    left join @relsattrs as rai on rai.attr_old = ca.related_attribute_id
    left join @relsattrs as ria on ria.attr_old = ca.related_image_attribute_id
    left join @relsattrs as pai on pai.attr_old = ca.persistent_attr_id
    left join @relsattrs as jai on jai.attr_old = ca.join_attr_id
    left join @relsattrs as bra on bra.attr_old = ca.back_related_attribute_id
    left join @relsattrs as cai on cai.attr_old = ca.classifier_attribute_id
    where ca.CONTENT_ID in (select destination_content_id from @relations_between_contents)

    update content_attribute
    set link_id = rc.newlink,
        default_value = rc.newlink
        from content_attribute as ca
        inner join @relations_between_content_links as rc on rc.oldlink = ca.link_id
        inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.[TYPE_NAME] = 'Relation'
        inner join @relations_between_contents as rbc on ca.content_id = rbc.destination_content_id
    
    
    -- copying groups
    insert into [dbo].[content_group]
    select @newsiteid
          ,[name]
      from [dbo].[content_group]
      where site_id = @oldsiteid
    
        -- updating groups
    ;with relations_between_groups as 
    (
        select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
        from [dbo].[content_group] as c
        inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid
    )
    update [dbo].content
    set content_group_id = rbg.content_group_id_new
    from [dbo].[CONTENT] as c
        inner join relations_between_groups as rbg on c.content_group_id = rbg.content_group_id_old
    where site_id = @newsiteid	
    
    
    
    -- copying access data
  delete FROM [[dbo].[CONTENT_ACCESS] 
  where CONTENT_ID in (
    select c.CONTENT_ID from content as c
    inner join content as c1 on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @newsiteid
  )
  
  declare @now datetime
  set @now = GETDATE()
  
  insert into [CONTENT_ACCESS]
  (
    [content_id]
      ,[user_id]
      ,[group_id]
      ,[permission_level_id]
      ,[created]
      ,[modified]
      ,[last_modified_by]
      ,[propagate_to_items]
  ) 
  select rbc.destination_content_id
      ,[user_id]
      ,[group_id]
      ,[permission_level_id]
      ,@now
      ,@now
      ,[last_modified_by]
      ,[propagate_to_items]
  from [dbo].[content_access] as ca
    inner join (select source_content_id, destination_content_id from [dbo].GetRelationsBetweenContents(@oldsiteid, @newsiteid)) as rbc on ca.CONTENT_ID = rbc.source_content_id
END
GO

CREATE PROCEDURE [dbo].[qp_copy_site_settings]
    @oldSiteId int,
    @newSiteId int
AS
BEGIN
    SET NOCOUNT ON;

    declare @todaysDate datetime
    set @todaysDate = GETDATE()
    
    -- copying workflows
    insert into [dbo].[workflow]
        (
           [workflow_name]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[site_id]
          ,[create_default_notification]
          ,[apply_by_default]
        )
    SELECT [WORKFLOW_NAME]
          ,w.[DESCRIPTION]
          ,@todaysDate
          ,@todaysDate
          ,w.[LAST_MODIFIED_BY]
          ,@newsiteid
          ,[create_default_notification]
          ,[apply_by_default]
      FROM [dbo].[workflow] as w
        inner join [dbo].[SITE] as s
            on w.SITE_ID = s.SITE_ID and s.SITE_ID = @oldsiteid
    
    --copying workflow rules	
    ;with relations_between_workflows
    as 
    (
        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                ,w2.WORKFLOW_ID as new_workflow_id
        FROM [dbo].[workflow] as w1
            inner join [dbo].[workflow] as w2 
                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @newsiteid
        where w1.SITE_ID = @oldsiteid
    )	
    insert into [dbo].[workflow_rules] 
    select [user_id]
          ,[group_id]
          ,[rule_order]
          ,[predecessor_permission_id]
          ,[successor_permission_id]
          ,[successor_status_id]
          ,[comment]
          ,rbw.new_workflow_id
      from [dbo].[workflow_rules] as wr
        inner join relations_between_workflows as rbw
            on wr.WORKFLOW_ID = rbw.old_workflow_id
            
    
    declare @relations_between_folders table(
        old_folder_id int,
        new_folder_id int
    )
    
    --copying folders		
    merge into [dbo].[folder]
    using (
    select @newsiteid
      ,[folder_id]
      ,[parent_folder_id]
      ,[name]
      ,[description]
      ,[filter]
      ,[path]
      ,@todaysDate
      ,@todaysDate
      ,[last_modified_by]
    from [dbo].[folder]
    where SITE_ID = @oldsiteid)
    as src (site_id,[folder_id],[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    on 0 = 1
    when not matched then
    insert (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    values (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    output src.[folder_id], inserted.[folder_id]
        into @relations_between_folders;
        
    update [dbo].[folder]
    set [parent_folder_id] = rbf.new_folder_id
    from [dbo].[folder] as f
        inner join @relations_between_folders as rbf
            on f.[parent_folder_id] = rbf.old_folder_id
    where f.SITE_ID = @newsiteid
        
END
GO

CREATE PROCEDURE [dbo].[qp_copy_site_templates]
    @oldSiteId int,
    @newSiteId int,
    @start int,
    @end int
AS
BEGIN
    declare @now datetime
    set @now = GETDATE()
    
    if @start = 1
    begin
        delete from [dbo].[page_template]
        where SITE_ID = @newSiteId
    end
    
    
    declare @new_templates table(new_template_id int)
    ;with templates_with_row_number as
    (
        select ROW_NUMBER() over(order by page_template_id) as [row_number] 
          ,[site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from [dbo].[page_template] as pt
    where site_id = @oldSiteId
    )
    insert into [dbo].[page_template](
          [site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    )
    output inserted.PAGE_TEMPLATE_ID
        into @new_templates
    select @newSiteId
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from templates_with_row_number as pt
    where row_number = @start
    
    
    declare @relations_between_templates table(
        page_template_id_old int,
        page_template_id_new int
    )
    
    insert into	@relations_between_templates
        select pto.page_template_id as page_template_id_old, ptn.page_template_id as page_template_id_new from page_template as pto
            inner join page_template as ptn on pto.template_name = ptn.template_name and ptn.site_id = @newSiteId
            inner join @new_templates as nt on ptn.PAGE_TEMPLATE_ID = nt.new_template_id
        where pto.site_id = @oldSiteId
    
    declare @new_pages_added table(page_id int, page_template_id int)
    
    insert into dbo.[page] (
        [page_template_id]
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock])
    output inserted.PAGE_ID, inserted.PAGE_TEMPLATE_ID
        into @new_pages_added
    select 
          rbt.page_template_id_new
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock]
    from [dbo].[page] as p
    inner join @relations_between_templates as rbt on p.page_template_id = rbt.page_template_id_old
    
    declare @relations_between_pages table(
        old_page_id int,
        new_page_id int
    )
    
    insert into @relations_between_pages
    select po.PAGE_ID, pn.PAGE_ID from page as po
        inner join @relations_between_templates as pt
            on po.PAGE_TEMPLATE_ID = pt.page_template_id_old
        inner join PAGE as pn
            on po.PAGE_NAME = pn.PAGE_NAME
        inner join @relations_between_templates as pt1
            on pn.PAGE_TEMPLATE_ID = pt1.page_template_id_new
    
    declare @relations_between_objects table(
        old_object_id int,
        new_object_id int		
    )
    
    
    merge into [dbo].[object]
    using(
    select [object_id]
          ,[parent_object_id]
          ,pt.page_template_id_new as [page_template_id]
          ,rbg.new_page_id as [page_id]
          ,[object_name]
          ,[object_format_id]
          ,o.[description]
          ,[object_type_id]
          ,[use_default_values]
          ,o.[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,o.[locked_by]
          ,o.[enable_viewstate]
          ,[control_custom_class]
          ,o.[disable_databind]
          ,o.[permanent_lock]
    from dbo.[OBJECT] as o
        inner join @relations_between_templates as pt
            on o.PAGE_TEMPLATE_ID = pt.page_template_id_old
        left join @relations_between_pages as rbg
            on o.page_id = rbg.old_page_id
    )as src ([object_id]
          ,[parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])
    on 0 = 1
    when not matched then
    insert ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])  
    values ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])     
    output src.[object_id], inserted.[object_id]     
        into @relations_between_objects;

    insert into [dbo].[OBJECT_FORMAT] (
          [object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    select
        rbo.new_object_id
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock]
    from [dbo].[OBJECT_FORMAT] as oft
    inner join @relations_between_objects as rbo
        on oft.[OBJECT_ID] = rbo.old_object_id
    
    insert into [dbo].[OBJECT_VALUES] 
    select rbo.new_object_id
      ,[variable_name]
      ,[variable_value]
    from [dbo].[object_values] as ov
        inner join @relations_between_objects as rbo
            on ov.OBJECT_ID = rbo.old_object_id
    
    
    ;with relations_between_contents
    as
        (select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newSiteId
        where c.site_id = @oldSiteId
        )
    insert into [dbo].[container]
        (
          [object_id] 
          ,[content_id] 
          ,[allow_order_dynamic]
          ,[order_static]
          ,[order_dynamic]
          ,[filter_value]
          ,[select_start]
          ,[select_total]
          ,[schedule_dependence]
          ,[rotate_content]
          ,[apply_security]
          ,[show_archived]
          ,[cursor_type]
          ,[cursor_location]
          ,[duration]
          ,[enable_cache_invalidation]
          ,[dynamic_content_variable]
          ,[start_level]
          ,[end_level]
          ,[use_level_filtration]
          ,[return_last_modified]
        )
    select rbo.new_object_id
      ,rbc.content_id_new
      ,[allow_order_dynamic]
      ,[order_static]
      ,[order_dynamic]
      ,[filter_value]
      ,[select_start]
      ,[select_total]
      ,[schedule_dependence]
      ,[rotate_content]
      ,[apply_security]
      ,[show_archived]
      ,[cursor_type]
      ,[cursor_location]
      ,[duration]
      ,[enable_cache_invalidation]
      ,[dynamic_content_variable]
      ,[start_level]
      ,[end_level]
      ,[use_level_filtration]
      ,[return_last_modified]
    from [dbo].[container] as c
        inner join @relations_between_objects as rbo
            on c.[OBJECT_ID] = rbo.old_object_id
        inner join relations_between_contents as rbc
            on c.CONTENT_ID = rbc.content_id_old
    
    
    select COUNT(*) from @relations_between_templates
END
GO

CREATE procedure [dbo].[qp_copy_site_update_links]
    @xmlparams xml
as
begin

    declare @linksrel table(
        olditemid int,
        newitemid int
    )
    
    declare @oldsiteid int
    declare @newsiteid int
    
    
    insert into @linksrel
    select doc.col.value('./@oldId', 'int') olditemid
             ,doc.col.value('./@newId', 'int') newitemid
            from @xmlparams.nodes('/items/item') doc(col)
            
    update [dbo].[content_data] 
        set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
        from [dbo].[content_data] cd
        inner join [dbo].[content_attribute] as ca on ca.attribute_id = cd.attribute_id
        inner join [dbo].[attribute_type] as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'relation'
        left join @linksrel as lr on lr.olditemid = cd.data
        inner join @linksrel as lr1 on lr1.newitemid = cd.content_item_id

    set @oldsiteid = (select doc.col.value('./@oldSiteId', 'int') from @xmlparams.nodes('/items') doc(col))

    set @newsiteid = (select doc.col.value('./@newSiteId', 'int') from @xmlparams.nodes('/items') doc(col))

    --updating o2m values
    update [dbo].[content_data] 
        set data = (case when lr1.newitemid is not null then lr1.newitemid else cd.DATA end)
        from [dbo].[content_data] cd
        inner join @linksrel lr on lr.newitemid = cd.CONTENT_ITEM_ID
        left join @linksrel lr1 on lr1.olditemid = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
    
    --updating link values
    update [dbo].[content_data] 
        set data = (case when lc.newvalue is not null then lc.newvalue else cd.DATA end)
        from [dbo].[content_data] cd
        left join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@oldsiteid, @newsiteid)) lc on lc.oldvalue = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join @linksrel as lr1 on lr1.newitemid = cd.content_item_id

    --inserting relations between new items
    insert into [dbo].[item_to_item]
    select r.newvalue, lr_l.newitemid, lr_r.newitemid from [dbo].[item_to_item] as ii
    inner join @linksrel as lr_l on lr_l.olditemid = ii.l_item_id
    inner join @linksrel as lr_r on lr_r.olditemid = ii.r_item_id
    inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@oldsiteid, @newsiteid)) as r on r.oldvalue = ii.link_id	
end
GO

CREATE procedure [dbo].[qp_copy_site_virtual_contents]
    @oldsiteid int,
    @newsiteid int
as
begin
    declare @newvirtualcontents table(
        old_content_id int,
        new_content_id int,
        virtual_type int,
        sqlquery nvarchar(max),
        altquery nvarchar(max)
    )
    declare @relscontents as table(
        content_id_old int,
        content_id_new int
    )
    insert into @relscontents
        select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid

    merge [dbo].[content]
    using (
        select content_id
               ,content_name
              ,[description]
              ,@newsiteid
              ,[created]
              ,[modified]
              ,[last_modified_by]
              ,[friendly_name_plural]
              ,[friendly_name_singular]
              ,[allow_items_permission]
              ,[content_group_id]
              ,[external_id]
              ,[virtual_type]
              ,rc.content_id_new virtual_join_primary_content_id_new
              ,c.virtual_join_primary_content_id
              ,[is_shared]
              ,[auto_archive]
              ,[max_num_of_stored_versions]
              ,[version_control_view]
              ,[content_page_size]
              ,[map_as_class]
              ,[net_content_name]
              ,[net_plural_content_name]
              ,[use_default_filtration]
              ,[query]
              ,[alt_query]
              ,[add_context_class_name]
              ,[xaml_validation]
              ,[disable_xaml_validation]
              ,[disable_changing_actions]
              ,[parent_content_id]
              ,[use_for_context]
          from [dbo].[content] as c
          left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
      where virtual_type != 0 and site_id = @oldsiteid) 
      as src(content_id, content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id_new], [virtual_join_primary_content_id], [is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
      on 0 = 1
      when not matched then
       insert (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id],[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       values (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],virtual_join_primary_content_id_new,[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
        into @newvirtualcontents;    
    
    insert into content_attribute(	
        [content_id]
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,[modified]
          ,[created]
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field])
    select nv.new_content_id
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,[modified]
          ,[created]
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field]
      from [dbo].[content_attribute]
      inner join @newvirtualcontents as nv on nv.old_content_id = content_id 

    -- удалить аттрибуты по умолчанию!
    -- добавить аттрибуты схожие с аттрибутами по умолчанию

    -- делаем соответствие между связанными аттрибутами
    ;with rels_attrs (attr_old, attr_new)
    as (
        select ca.attribute_id as cat_old
            , ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca
        inner join @newvirtualcontents as ra on ra.old_content_id = ca.content_id
        left join [dbo].content_attribute as ca1 on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.new_content_id
    )

    update [dbo].[content_attribute]
    set		[related_attribute_id] = rai.attr_new
          ,[related_image_attribute_id]= ria.attr_new
          ,[persistent_attr_id]= pai.attr_new
          ,[join_attr_id]= jai.attr_new
          ,[back_related_attribute_id]= bra.attr_new
          ,[classifier_attribute_id]= cai.attr_new
    from [dbo].[content_attribute] as ca
    inner join rels_attrs as rai on rai.attr_old = ca.related_attribute_id
    inner join rels_attrs as ria on ria.attr_old = ca.related_image_attribute_id
    inner join rels_attrs as pai on pai.attr_old = ca.persistent_attr_id
    inner join rels_attrs as jai on jai.attr_old = ca.join_attr_id
    inner join rels_attrs as bra on bra.attr_old = ca.back_related_attribute_id
    inner join rels_attrs as cai on cai.attr_old = ca.classifier_attribute_id
   
    insert into union_contents
    select nvc1.new_content_id, rc.content_id_new, rc1.content_id_new 
    from union_contents as uc
    inner join @newvirtualcontents as nvc1 on uc.virtual_content_id = nvc1.old_content_id
    left join @relscontents as rc on uc.union_content_id = rc.content_id_old
    left join @relscontents as rc1 on uc.master_content_id = rc1.content_id_old

    
    declare @relations_between_contents_links table(
            oldvalue int
            ,newvalue int
            
    )
    insert into @relations_between_contents_links
    select oldvalue, newvalue from [dbo].[GetRelationsBetweenContentLinks](@oldSiteId, @newSiteId)
    
    update [dbo].[content_attribute]
    set link_id = lc.newvalue,
        default_value =lc1.newvalue
    from  [dbo].[content_attribute] as ca
        inner join @relations_between_contents_links as lc on ca.link_id = CAST(lc.oldvalue as varchar)
        inner join @relations_between_contents_links as lc1 on ca.default_value = CAST(lc1.oldvalue as varchar)
        inner join content as c on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @newsiteid

    update [dbo].[content]
    set content_group_id = cg1.newGroupId
    from [dbo].[content] as c
        inner join
        (
            select cg.content_group_id as oldGroupId, cg1.content_group_id as newGroupId 
            from [content_group] cg
                inner join content_group cg1 
                    on cg.name = cg1.name and cg1.site_id = @newsiteid
            where cg.site_id = @oldsiteid
        ) as cg1 
            on c.content_group_id = cg1.oldGroupId
    where c.SITE_ID = @newsiteid


    select 	old_content_id
        , new_content_id
        , virtual_type
        , sqlquery
        , altquery
    from @newvirtualcontents
end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.33', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.33 completed'
GO


-- ************************************** 
-- Pavel Celut
-- version 7.9.7.34
-- Fix Add and Remove Site
-- **************************************

ALTER TRIGGER [dbo].[ti_statuses_and_default_notif] ON [dbo].[SITE] 
FOR INSERT
AS
 
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Created',  10, 'Article has been created' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Approved',  50, 'Article has been modified' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'Published',  100, 'Article has been published' ,1 from inserted)
 insert into status_type (site_id, status_type_name, weight, description, last_modified_by)
             (select site_id , 'None',  0, 'No Status has been assigned' ,1 from inserted)
 
INSERT INTO page_template(site_id, template_name, net_template_name, template_picture, created, modified, last_modified_by, charset, codepage, locale, is_system, net_language_id)  
select site_id, 'Default Notification Template', 'Default_Notification_Template', '', getdate(), getdate(), 1, 'utf-8', 65001, 1049, 1, dbo.qp_default_net_language(script_language) from inserted 

insert into content_group (site_id, name)
select site_id, 'Default Group' from inserted 
GO

ALTER TRIGGER [dbo].[tbd_delete_object_format] ON [dbo].[OBJECT_FORMAT] 
INSTEAD OF DELETE
AS
BEGIN
    if object_id('tempdb..#disable_tbd_delete_object_format') is null
    begin
        declare @obj table (
            id numeric identity,
            [object_id] numeric
        )
        declare @i numeric, @count numeric, @object_id numeric, @new_object_format_id numeric 
        
        insert into @obj([object_id])
        select obj.[object_id] from deleted d 
        inner join object obj 
        on obj.object_format_id = d.object_format_id

        set @i = 1
        select @count = count(id) from @obj

        while @i < @count + 1
        begin
            select @object_id = object_id from @obj where id = @i
            
            select top 1 @new_object_format_id = object_format_id from object_format where object_id = @object_id and object_format_id not in (select object_format_id from deleted)
            update object set object_format_id = @new_object_format_id where object_id = @object_id

            set @i = @i + 1
        end

        
        update object set object_format_id = null where object_format_id in (select object_format_id from deleted)
        
        update notifications set format_id = null where format_id in (select object_format_id from deleted)

        delete page_trace from page_trace pt 
        inner join page_trace_format ptf on pt.trace_id = ptf.trace_id
        inner join deleted d on ptf.format_id = d.object_format_id
    end			

    delete object_format from object_format objf 
    inner join deleted d on objf.object_format_id = d.object_format_id

end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.34', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.34 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.35
-- Fix Add New M2O
-- **************************************

ALTER PROCEDURE [dbo].[qp_update_m2o_final] 
@id numeric
AS
BEGIN
    declare @statusId numeric
    declare @splitted bit
    declare @lastModifiedBy numeric
    declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, processed bit not null default 0, primary key(id, attribute_id))
    
    insert into @ids(id, attribute_id, to_remove)
    select * from #resultIds
    
    select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
    
    update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1 
    where content_item_id in (select id from @ids)
    
    update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate() 
    from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
    where r.to_remove = 0
    
    insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
    select r.id, r.attribute_id, @id, NULL, getdate()
    from @ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
    where r.to_remove = 0 and cd.CONTENT_DATA_ID is null
    
    update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate() 
    from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
    where r.to_remove = 1
    
    declare @maxStatus numeric
    declare @resultId numeric
    
    select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

    if @statusId = @maxStatus and @splitted = 0 begin 
    while exists (select * from child_delays where id = @id)
    begin
        select @resultId = child_id from child_delays where id = @id
        print @resultId
        delete from child_delays where id = @id and child_id = @resultId
        if not exists(select * from child_delays where child_id = @resultId)
        begin
            exec qp_merge_article @resultId
        end
    end
    end else if @maxStatus is not null begin
        insert into child_delays (id, child_id) select @id, r.id from @ids r 
        inner join content_item ci on r.id = ci.content_item_id 
        left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
        left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id 
        left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
        where ex.child_id is null and ci.status_type_id = wms.max_status_type_id 
            and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
        
        update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
    end
    
    while exists (select id from @ids where processed = 0)
    begin
        select @resultId = id from @ids where processed = 0
        exec qp_replicate @resultId
        update @ids set processed = 1 where id = @resultId
    end
END

GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.35', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.35 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.36
-- Fix for copying groups
-- **************************************

ALTER PROCEDURE [dbo].[qp_copy_site_contents_update]
    @oldsiteid int,
    @newsiteid int
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    --copying links between contents
    declare @relations_between_content_links table(
        oldlink int,
        newlink int
    )
    
    declare @relations_between_contents table(
        source_content_id int,
        destination_content_id int
    )
    insert into @relations_between_contents 
    select source_content_id, destination_content_id from [dbo].GetRelationsBetweenContents(@oldSiteId, @newSiteId)
    
    merge [dbo].content_to_content as t
    using(
    select cc.[link_id]
          ,rbc.destination_content_id 
          ,rbc1.destination_content_id 
          ,cc.[map_as_class]
          ,[net_link_name]
          ,[net_plural_link_name]
          ,[symmetric]
        from [dbo].content_to_content as cc
        inner join [dbo].content as c on c.content_id = l_content_id
        inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
        inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
        where c.site_id = @oldsiteid
    )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
    on 0 = 1
    when not matched then
       insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       output src.[link_id], inserted.[link_id]
        into @relations_between_content_links;

    -- удалить аттрибуты по умолчанию!

    -- добавить аттрибуты схожие с аттрибутами по умолчанию

    -- делаем соответствие между связанными аттрибутами
    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
    )

    insert into @relsattrs
    select ca.attribute_id as cat_old
        ,ca1.attribute_id as cat_new 
    from [dbo].content_attribute as ca
    inner join @relations_between_contents as rbc on ca.content_id = rbc.source_content_id
    left join [dbo].content_attribute as ca1 on ca.attribute_name = ca1.attribute_name and ca1.content_id = rbc.destination_content_id

    update [dbo].[content_attribute]
    set		[related_attribute_id] = rai.attr_new
          ,[related_image_attribute_id]= ria.attr_new
          ,[persistent_attr_id]= pai.attr_new
          ,[join_attr_id]= jai.attr_new
          ,[back_related_attribute_id]= bra.attr_new
          ,[classifier_attribute_id]= cai.attr_new
    from [dbo].[content_attribute] as ca
    left join @relsattrs as rai on rai.attr_old = ca.related_attribute_id
    left join @relsattrs as ria on ria.attr_old = ca.related_image_attribute_id
    left join @relsattrs as pai on pai.attr_old = ca.persistent_attr_id
    left join @relsattrs as jai on jai.attr_old = ca.join_attr_id
    left join @relsattrs as bra on bra.attr_old = ca.back_related_attribute_id
    left join @relsattrs as cai on cai.attr_old = ca.classifier_attribute_id
    where ca.CONTENT_ID in (select destination_content_id from @relations_between_contents)

    update content_attribute
    set link_id = rc.newlink,
        default_value = rc.newlink
        from content_attribute as ca
        inner join @relations_between_content_links as rc on rc.oldlink = ca.link_id
        inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.[TYPE_NAME] = 'Relation'
        inner join @relations_between_contents as rbc on ca.content_id = rbc.destination_content_id
    
    
    -- copying groups
    delete from [dbo].[content_group]
        where site_id = @newsiteid

    insert into [dbo].[content_group]
    select @newsiteid
          ,[name]
      from [dbo].[content_group]
      where site_id = @oldsiteid
    
        -- updating groups
    ;with relations_between_groups as 
    (
        select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
        from [dbo].[content_group] as c
        inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid
    )
    update [dbo].content
    set content_group_id = rbg.content_group_id_new
    from [dbo].[CONTENT] as c
        inner join relations_between_groups as rbg on c.content_group_id = rbg.content_group_id_old
    where site_id = @newsiteid	
    
    
    
    -- copying access data
  delete FROM [dbo].[CONTENT_ACCESS] 
  where CONTENT_ID in (
    select c.CONTENT_ID from content as c
    inner join content as c1 on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @newsiteid
  )
  
  declare @now datetime
  set @now = GETDATE()
  
  insert into [CONTENT_ACCESS]
  (
    [content_id]
      ,[user_id]
      ,[group_id]
      ,[permission_level_id]
      ,[created]
      ,[modified]
      ,[last_modified_by]
      ,[propagate_to_items]
  ) 
  select rbc.destination_content_id
      ,[user_id]
      ,[group_id]
      ,[permission_level_id]
      ,@now
      ,@now
      ,[last_modified_by]
      ,[propagate_to_items]
  from [dbo].[content_access] as ca
    inner join (select source_content_id, destination_content_id from [dbo].GetRelationsBetweenContents(@oldsiteid, @newsiteid)) as rbc on ca.CONTENT_ID = rbc.source_content_id
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.36', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.36 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.37
-- Optimize process of updating links
-- **************************************
ALTER procedure [dbo].[qp_copy_site_update_links]
    @xmlparams xml,
    @sourceSiteId int,
    @destinationSiteId int
as
begin

    declare @linksrel table(
        olditemid int,
        newitemid int
    )
    
    
    insert into @linksrel
    select doc.col.value('./@oldId', 'int') olditemid
             ,doc.col.value('./@newId', 'int') newitemid
            from @xmlparams.nodes('/item') doc(col)
            
    update [dbo].[content_data] 
        set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
        from [dbo].[content_data] cd
        inner join [dbo].[content_attribute] as ca on ca.attribute_id = cd.attribute_id
        inner join [dbo].[attribute_type] as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'relation'
        inner join [dbo].[CONTENT_ITEM] as ci on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c on c.CONTENT_ID = ci.CONTENT_ID
        left join @linksrel as lr on lr.olditemid = cd.data
        where c.SITE_ID = @destinationSiteId

    --updating o2m values
    update [dbo].[content_data] 
        set data = (case when lr1.newitemid is not null then lr1.newitemid else cd.DATA end)
        from [dbo].[content_data] cd
        left join @linksrel lr1 on lr1.olditemid = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join [dbo].[CONTENT_ITEM] as ci on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c on c.CONTENT_ID = ci.CONTENT_ID
        where c.SITE_ID = @destinationSiteId
    
    --updating link values
    update [dbo].[content_data] 
        set data = (case when lc.newvalue is not null then lc.newvalue else cd.DATA end)
        from [dbo].[content_data] cd
        left join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) lc on lc.oldvalue = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join @linksrel as lr1 on lr1.newitemid = cd.content_item_id

    --inserting relations between new items
    insert into [dbo].[item_to_item]
    select r.newvalue, lr_l.newitemid, lr_r.newitemid from [dbo].[item_to_item] as ii
    inner join @linksrel as lr_l on lr_l.olditemid = ii.l_item_id
    inner join @linksrel as lr_r on lr_r.olditemid = ii.r_item_id
    inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r on r.oldvalue = ii.link_id	
    
    SELECT COUNT(*) from @linksrel
end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.37', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.37 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.38
-- Eliminate redundant access records
-- **************************************

ALTER TRIGGER [dbo].[ti_access_content_item] ON [dbo].[CONTENT_ITEM] FOR INSERT
AS

  declare @ids table
  (
    content_item_id numeric primary key,
    last_modified_by numeric not null,
    content_id numeric not null
  )

  insert into @ids (content_item_id, last_modified_by, content_id)
  select content_item_id, i.last_modified_by, i.content_id from inserted i 
  inner join content c on i.CONTENT_ID = c.CONTENT_ID
  where c.allow_items_permission = 1

  INSERT INTO content_item_access 
    (content_item_id, user_id, permission_level_id, last_modified_by)
  SELECT
    content_item_id, last_modified_by, 1, 1
  FROM @ids i
  WHERE i.LAST_MODIFIED_BY <> 1

  INSERT INTO content_item_access 
    (content_item_id, user_id, group_id, permission_level_id, last_modified_by)
  SELECT
    i.content_item_id, ca.user_id, ca.group_id, ca.permission_level_id, 1 
  FROM content_access AS ca
    INNER JOIN @ids AS i ON ca.content_id = i.content_id
    LEFT OUTER JOIN user_group AS g ON g.group_id = ca.group_id
  WHERE
    (ca.user_id <> i.last_modified_by or ca.user_id IS NULL)
    AND ((g.shared_content_items = 0 and g.GROUP_ID <> 1) OR g.group_id IS NULL)
    AND ca.propagate_to_items = 1

  INSERT INTO content_item_access 
    (content_item_id, group_id, permission_level_id, last_modified_by)
  SELECT DISTINCT
    i.content_item_id, g.group_id, 1, 1
  FROM @ids AS i
    LEFT OUTER JOIN user_group_bind AS gb ON gb.user_id = i.last_modified_by
    LEFT OUTER JOIN user_group AS g ON g.group_id = gb.group_id
  WHERE
    g.shared_content_items = 1 and g.GROUP_ID <> 1
GO

CREATE TABLE #TEMP_CONTENT_ITEM_ACCESS(
    [CONTENT_ITEM_ID] [numeric](18, 0) NOT NULL,
    [USER_ID] [numeric](18, 0) NULL,
    [GROUP_ID] [numeric](18, 0) NULL,
    [PERMISSION_LEVEL_ID] [numeric](18, 0) NOT NULL,
    [CREATED] [datetime] NOT NULL,
    [MODIFIED] [datetime] NOT NULL,
    [LAST_MODIFIED_BY] [numeric](18, 0) NOT NULL,
    [CONTENT_ITEM_ACCESS_ID] [numeric](18, 0) NOT NULL
)

insert into #TEMP_CONTENT_ITEM_ACCESS
select cia.* 
from CONTENT_ITEM_ACCESS cia 
inner join CONTENT_ITEM ci on ci.CONTENT_ITEM_ID = cia.CONTENT_ITEM_ID
inner join CONTENT c on c.CONTENT_ID = ci.CONTENT_ID
where c.allow_items_permission = 1

truncate table CONTENT_ITEM_ACCESS

set identity_insert dbo.content_item_access on

insert into CONTENT_ITEM_ACCESS(content_item_id, USER_ID, GROUP_ID, PERMISSION_LEVEL_ID, CREATED, MODIFIED, LAST_MODIFIED_BY, CONTENT_ITEM_ACCESS_ID)
select content_item_id, USER_ID, GROUP_ID, PERMISSION_LEVEL_ID, CREATED, MODIFIED, LAST_MODIFIED_BY, CONTENT_ITEM_ACCESS_ID from #TEMP_CONTENT_ITEM_ACCESS

set identity_insert dbo.content_item_access off

drop table #TEMP_CONTENT_ITEM_ACCESS

GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.38', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.38 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.39
-- Fix updating templates
-- **************************************

ALTER PROCEDURE [dbo].[qp_copy_site_templates]
    @oldSiteId int,
    @newSiteId int,
    @start int,
    @end int
AS
BEGIN
    declare @now datetime
    set @now = GETDATE()
    
    if @start = 1
    begin
        delete from [dbo].[page_template]
        where SITE_ID = @newSiteId
    end
    
    
    declare @new_templates table(new_template_id int)
    ;with templates_with_row_number as
    (
        select ROW_NUMBER() over(order by page_template_id) as [row_number] 
          ,[site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from [dbo].[page_template] as pt
    where site_id = @oldSiteId
    )
    insert into [dbo].[page_template](
          [site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    )
    output inserted.PAGE_TEMPLATE_ID
        into @new_templates
    select @newSiteId
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from templates_with_row_number as pt
    where row_number = @start
    
    
    declare @relations_between_templates table(
        page_template_id_old int,
        page_template_id_new int
    )
    
    insert into	@relations_between_templates
        select pto.page_template_id as page_template_id_old, ptn.page_template_id as page_template_id_new from page_template as pto
            inner join page_template as ptn on pto.template_name = ptn.template_name and ptn.site_id = @newSiteId
            inner join @new_templates as nt on ptn.PAGE_TEMPLATE_ID = nt.new_template_id
        where pto.site_id = @oldSiteId
    
    declare @new_pages_added table(page_id int, page_template_id int)
    
    insert into dbo.[page] (
        [page_template_id]
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock])
    output inserted.PAGE_ID, inserted.PAGE_TEMPLATE_ID
        into @new_pages_added
    select 
          rbt.page_template_id_new
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock]
    from [dbo].[page] as p
    inner join @relations_between_templates as rbt on p.page_template_id = rbt.page_template_id_old
    
    declare @relations_between_pages table(
        old_page_id int,
        new_page_id int
    )
    
    insert into @relations_between_pages
    select po.PAGE_ID, pn.PAGE_ID from page as po
        inner join @relations_between_templates as pt
            on po.PAGE_TEMPLATE_ID = pt.page_template_id_old
        inner join PAGE as pn
            on po.PAGE_NAME = pn.PAGE_NAME
        inner join @relations_between_templates as pt1
            on pn.PAGE_TEMPLATE_ID = pt1.page_template_id_new
    
    declare @relations_between_objects table(
        old_object_id int,
        new_object_id int		
    )
    
    
    merge into [dbo].[object]
    using(
    select [object_id]
          ,[parent_object_id]
          ,pt.page_template_id_new as [page_template_id]
          ,rbg.new_page_id as [page_id]
          ,[object_name]
          ,[object_format_id]
          ,o.[description]
          ,[object_type_id]
          ,[use_default_values]
          ,o.[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,o.[locked_by]
          ,o.[enable_viewstate]
          ,[control_custom_class]
          ,o.[disable_databind]
          ,o.[permanent_lock]
    from dbo.[OBJECT] as o
        inner join @relations_between_templates as pt
            on o.PAGE_TEMPLATE_ID = pt.page_template_id_old
        left join @relations_between_pages as rbg
            on o.page_id = rbg.old_page_id
    )as src ([object_id]
          ,[parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])
    on 0 = 1
    when not matched then
    insert ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])  
    values ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])     
    output src.[object_id], inserted.[object_id]     
        into @relations_between_objects;

    declare @relations_between_object_formats table(
        source_object_format_id int,
        destination_object_format_id int
    )

    merge [dbo].[OBJECT_FORMAT]
    using (
            select
            [object_format_id]
          ,rbo.new_object_id as [object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock]
    from [dbo].[OBJECT_FORMAT] as oft
    inner join @relations_between_objects as rbo
        on oft.[OBJECT_ID] = rbo.old_object_id
    ) as src([object_format_id]
            ,[object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    on 0 = 1
    when not matched then
    insert ([object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    values ([object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    output src.[object_format_id], inserted.[object_format_id]     
        into @relations_between_object_formats; 
    
    update [dbo].[object]
    set OBJECT_FORMAT_ID = rbof.destination_object_format_id
    from [dbo].[object] as o
        inner join @relations_between_object_formats as rbof
            on o.OBJECT_FORMAT_ID = rbof.source_object_format_id
        inner join @relations_between_objects as rbo
            on o.OBJECT_ID = rbo.new_object_id

    insert into [dbo].[OBJECT_VALUES] 
    select rbo.new_object_id
      ,[variable_name]
      ,[variable_value]
    from [dbo].[object_values] as ov
        inner join @relations_between_objects as rbo
            on ov.OBJECT_ID = rbo.old_object_id
    
    
    ;with relations_between_contents
    as
        (select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newSiteId
        where c.site_id = @oldSiteId
        )
    insert into [dbo].[container]
        (
          [object_id] 
          ,[content_id] 
          ,[allow_order_dynamic]
          ,[order_static]
          ,[order_dynamic]
          ,[filter_value]
          ,[select_start]
          ,[select_total]
          ,[schedule_dependence]
          ,[rotate_content]
          ,[apply_security]
          ,[show_archived]
          ,[cursor_type]
          ,[cursor_location]
          ,[duration]
          ,[enable_cache_invalidation]
          ,[dynamic_content_variable]
          ,[start_level]
          ,[end_level]
          ,[use_level_filtration]
          ,[return_last_modified]
        )
    select rbo.new_object_id
      ,rbc.content_id_new
      ,[allow_order_dynamic]
      ,[order_static]
      ,[order_dynamic]
      ,[filter_value]
      ,[select_start]
      ,[select_total]
      ,[schedule_dependence]
      ,[rotate_content]
      ,[apply_security]
      ,[show_archived]
      ,[cursor_type]
      ,[cursor_location]
      ,[duration]
      ,[enable_cache_invalidation]
      ,[dynamic_content_variable]
      ,[start_level]
      ,[end_level]
      ,[use_level_filtration]
      ,[return_last_modified]
    from [dbo].[container] as c
        inner join @relations_between_objects as rbo
            on c.[OBJECT_ID] = rbo.old_object_id
        inner join relations_between_contents as rbc
            on c.CONTENT_ID = rbc.content_id_old
    
    
    select COUNT(*) from @relations_between_templates
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.39', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.39 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.40
-- Multiple Export
-- **************************************

if not exists (select * from action_type where code = 'multiple_export')
insert into ACTION_TYPE (NAME, CODE, REQUIRED_PERMISSION_LEVEL_ID, ITEMS_AFFECTED)
VALUES ('Multiple Export', 'multiple_export', 2, 255)

if not exists (select * from BACKEND_ACTION where code = 'multiple_export_article')
insert into BACKEND_ACTION (TYPE_ID, ENTITY_TYPE_ID, NAME, code, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
VALUES(dbo.qp_action_type_id('multiple_export'), dbo.qp_entity_type_id('article'), 'Export Selected Articles', 'multiple_export_article', '~/ExportSelectedArticles/', 1, 600, 400, 1, 1)

if not exists (select * from ACTION_TOOLBAR_BUTTON where parent_action_id = dbo.qp_action_id('list_article') and name = 'Export')
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], icon)
values (dbo.qp_action_id('list_article'), dbo.qp_action_id('multiple_export_article'), 'Export', 15, 'other/export.gif')

update CONTEXT_MENU_ITEM set icon = 'other/export.gif' where name = 'Export Articles'
update CONTEXT_MENU_ITEM set icon = 'other/import.gif' where name = 'Import Articles'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.40', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.40 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.41
-- fix m2o relations
-- **************************************

ALTER procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @startfrom int,
        @endon int
as
begin
/****** script for selecttopnrows command from ssms  ******/

declare @relscontents table(
        content_id_old numeric(18,0)
        ,content_id_new numeric(18,0)

)
declare @relsattrs table(
        attr_old numeric(18,0)
        ,attr_new numeric(18,0)
)


declare @contentitemstable table( 
        newcontentitemid int,
        contentid int,
        oldcontentitemid int);

declare @copydata table(
        oldattributeid int,
        oldcontentitemid int,
        newdata nvarchar(3500),
        newblobdata ntext,
        newattributeid int,
        newcontentitemid int
)    
        
insert into @relscontents
        select c.content_id as content_id_old, nc.content_id as content_id_new 
        from [dbo].[content] as c
        inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid


insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca
        inner join @relscontents as ra on ra.content_id_old = ca.content_id
        left join [dbo].content_attribute as ca1 on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.content_id_new
    
declare @todaysDateTime datetime
set @todaysDateTime = GetDate();

    ;with relations_between_statuses
    as (
        select st1.STATUS_TYPE_ID as old_status_type_id, st2.STATUS_TYPE_ID as new_status_type_id from [dbo].[status_type] as st1
        inner join [dbo].[status_type] as st2 on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
        where st1.SITE_ID = @oldSiteId
    ) 
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from (
            select row_number() over (order by content_item_id) as rownumber
              ,[content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.content_id_new as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,[not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
            from [dbo].[content_item] as c1
            inner join [dbo].[content] as c on c.content_id = c1.content_id
            inner join @relscontents as rc on rc.content_id_old = c1.content_id
            inner join relations_between_statuses as rbs on c1.[status_type_id] = rbs.old_status_type_id
            where c.site_id = @oldsiteid
        ) as t
          where t.rownumber between @startfrom and @endon
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;
            
            
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid
    
insert into @copydata
    select r.attribute_id
           ,r.content_item_id
           ,r.DATA
           ,r.blob_data
           ,ra.attr_new
           ,ci.newcontentitemid
     from [dbo].[content_data] as r
    inner join @relsattrs as ra on ra.attr_old = r.attribute_id
    inner join @contentitemstable as ci on ci.oldcontentitemid = r.content_item_id

    
update [dbo].[content_data] 
    set [data] = [@copydata].newdata
      ,[blob_data] = [@copydata].newblobdata
    from [dbo].[content_data] as cd
        inner join @copydata
            on
                cd.content_item_id = [@copydata].newcontentitemid and
                cd.attribute_id = [@copydata].newattributeid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID and at.[TYPE_NAME] != 'Dynamic Image'

update [dbo].[content_data] 
    set [data] = replace(cd1.newdata, 'field_' + CAST(cd1.oldattributeid as varchar), 'field_' + CAST(cd1.newattributeid as varchar))
    from [dbo].[content_data] as cd
        inner join @copydata as cd1
            on
                cd.content_item_id = cd1.newcontentitemid and
                cd.attribute_id = cd1.newattributeid and
                cd1.newdata is not null
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID and at.[TYPE_NAME] = 'Dynamic Image'		 


update [dbo].[content_data] 
    set data = ra.attr_new
    from [dbo].[content_data] cd
        inner join [dbo].[content_attribute] as ca on ca.attribute_id = cd.attribute_id
        inner join [dbo].[attribute_type] as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation Many-to-One'
        inner join [dbo].[CONTENT_ITEM] as ci on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c on c.CONTENT_ID = ci.CONTENT_ID
        right join @relsattrs ra on CAST(ra.attr_old as nvarchar) = cd.DATA
    where c.SITE_ID = @newsiteid and cd.DATA is not null

    select newcontentitemid, oldcontentitemid from @contentitemstable
end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.41', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.41 completed'
GO


-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.42
-- fix copy site
-- **************************************

CREATE PROCEDURE [dbo].[qp_copy_site_copy_contents_attributes]
    @source_site_id int,
    @destination_site_id int,
    @is_contents_virtual bit,
    @new_content_ids nvarchar(max)
AS
BEGIN

    set nocount on;
    declare @todaysDate datetime = GetDate()

    if @new_content_ids is not null begin
    insert into content_attribute
    (	[content_id]
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,[modified]
          ,[created]
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field]
          ,[PARENT_ATTRIBUTE_ID]
          ,[hide]
          ,[override]
          ,[use_for_context]
          ,[use_for_variations])
    select rbc.destination_content_id
          ,[attribute_name]
          ,[format_mask]
          ,[input_mask]
          ,[attribute_size]
          ,[default_value]
          ,[attribute_type_id]
          ,[related_attribute_id] --накапливаем информацию
          ,[index_flag]
          ,[description]
          ,@todaysDate as [modified]
          ,@todaysDate as [created]
          ,[last_modified_by]
          ,[attribute_order]
          ,[required]
          ,[permanent_flag]
          ,[primary_flag]
          ,[relation_condition]
          ,[display_as_radio_button]
          ,[view_in_list]
          ,[readonly_flag]
          ,[allow_stage_edit]
          ,[attribute_configuration]
          ,[related_image_attribute_id] --накапливаем информацию
          ,[persistent_attr_id] --накапливаем информацию
          ,[join_attr_id] --накапливаем информацию
          ,[link_id] --накапливаем информацию
          ,[default_blob_value]
          ,[auto_load]
          ,[friendly_name]
          ,[use_site_library]
          ,[use_archive_articles]
          ,[auto_expand]
          ,[relation_page_size]
          ,[doctype]
          ,[full_page]
          ,[rename_matched]
          ,[subfolder]
          ,[disable_version_control]
          ,[map_as_property]
          ,[net_attribute_name]
          ,[net_back_attribute_name]
          ,[p_enter_mode]
          ,[use_english_quotes]
          ,[back_related_attribute_id] --накапливаем информацию
          ,[is_long]		  
          ,[external_css]
          ,[root_element_class]
          ,[use_for_tree]
          ,[auto_check_children]
          ,[aggregated]
          ,[classifier_attribute_id] --накапливаем информацию
          ,[is_classifier]
          ,[changeable]
          ,[use_relation_security]
          ,[copy_permissions_to_children]
          ,[enum_values]
          ,[show_as_radio_button]
          ,[use_for_default_filtration]
          ,[tree_order_field] --накапливаем информацию
          ,[PARENT_ATTRIBUTE_ID] --накапливаем информацию
          ,[hide]
          ,[override]
          ,[use_for_context]
          ,[use_for_variations]
      from [dbo].[content_attribute] as ca (nolock)
      inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @destination_site_id
                    where c.site_id = @source_site_id and c.virtual_type = @is_contents_virtual
      ) as rbc on ca.CONTENT_ID = rbc.source_content_id and rbc.destination_content_id in (SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ','))
      where ca.attribute_name != 'Title'

      delete ca
        from [dbo].content_attribute as ca (NOLOCK)
        inner join content as c on ca.CONTENT_ID = c.CONTENT_ID 
                                    and c.SITE_ID = @destination_site_id 
                                    and c.virtual_type = @is_contents_virtual 
                                    and c.CONTENT_ID in (SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ','))
        left join(
                 select ca.ATTRIBUTE_ID, ca.ATTRIBUTE_NAME, c.CONTENT_ID, c.CONTENT_NAME
                from [dbo].content_attribute as ca (NOLOCK)
                inner join content as c on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @source_site_id
        ) as ct on ct.CONTENT_NAME = c.CONTENT_NAME and ct.ATTRIBUTE_NAME = ca.ATTRIBUTE_NAME
        where ca.ATTRIBUTE_NAME = 'Title' and ct.ATTRIBUTE_ID is null

    end
END
go

CREATE PROCEDURE [dbo].[qp_copy_site_update_attributes]
    @oldSiteId int,
    @newSiteId int,
    @isvirtual bit,
    @new_content_ids nvarchar(max)
AS
BEGIN

    set nocount on;
    if @new_content_ids is not null begin
    ;with relsattrs as
    (
        select ca.attribute_id as attr_old
            ,ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @newSiteId
                    where c.site_id = @oldSiteId and c.virtual_type = @isvirtual
        ) as rbc on ca.content_id = rbc.source_content_id
        left join [dbo].content_attribute as ca1 (nolock) on ca.attribute_name = ca1.attribute_name and ca1.content_id = rbc.destination_content_id
        where rbc.destination_content_id in (SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ','))
    )
    update [dbo].[content_attribute]
    set		[related_attribute_id] = rai.attr_new
          ,[related_image_attribute_id]= ria.attr_new
          ,[persistent_attr_id]= pai.attr_new
          ,[join_attr_id]= jai.attr_new
          ,[back_related_attribute_id]= bra.attr_new
          ,[classifier_attribute_id]= cai.attr_new
          ,[tree_order_field] = tof.attr_new
          ,[PARENT_ATTRIBUTE_ID] = paid.attr_new
    from [dbo].[content_attribute] as ca (nolock)
    left join relsattrs as rai on rai.attr_old = ca.related_attribute_id
    left join relsattrs as ria on ria.attr_old = ca.related_image_attribute_id
    left join relsattrs as pai on pai.attr_old = ca.persistent_attr_id
    left join relsattrs as jai on jai.attr_old = ca.join_attr_id
    left join relsattrs as bra on bra.attr_old = ca.back_related_attribute_id
    left join relsattrs as cai on cai.attr_old = ca.classifier_attribute_id
    left join relsattrs as tof on tof.attr_old = ca.tree_order_field
    left join relsattrs as paid on paid.attr_old = ca.PARENT_ATTRIBUTE_ID
    where ca.CONTENT_ID in (SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ','))
    end
END
go

ALTER procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @startfrom int,
        @endon int
as
begin

    set nocount on;
    select 1 as A into #disable_tu_update_child_content_data;

    DECLARE @not_for_replication bit
    SET @not_for_replication = 1

    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
            ,primary key (attr_old, attr_new)
    )

    declare @contentitemstable table( 
            newcontentitemid int ,
            contentid int,
            oldcontentitemid int,
            primary key ( newcontentitemid, oldcontentitemid, contentid));

    declare @isVirtual bit = 0
       
    declare @todaysDateTime datetime
    set @todaysDateTime = GetDate();


    insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca (NOLOCK)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0
        ) as ra on ra.source_content_id = ca.content_id
        left join [dbo].content_attribute as ca1 (NOLOCK) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id

    ;with content_items as (
        select row_number() over (order by content_item_id) as rownumber
              ,[content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.destination_content_id as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,@not_for_replication as [not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
            from [dbo].[content_item] as c1 (NOLOCK)
            inner join [dbo].[content] as c (NOLOCK) on c.content_id = c1.content_id
            inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0
            ) as rc on rc.source_content_id = c1.content_id
            inner join (
                select st1.STATUS_TYPE_ID as old_status_type_id, st2.STATUS_TYPE_ID as new_status_type_id from [dbo].[status_type] as st1 (NOLOCK)
                    inner join [dbo].[status_type] as st2 (NOLOCK) 
                        on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                where st1.SITE_ID = @oldSiteId
            ) as rbs on c1.[status_type_id] = rbs.old_status_type_id
            where c.site_id = @oldsiteid
    )
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from content_items as t
          where t.rownumber between @startfrom and @endon
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;
            
                    
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,1 as[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis (NOLOCK)
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid;

    update copydata
    set [data] =(	case	when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.attr_new as varchar)) 
                            when at.[TYPE_NAME] = 'Relation Many-to-One' and cd.DATA is not null then CAST(ra1.attr_new as varchar)
                            else cd.data
                    end)
      ,[blob_data] = cd.BLOB_DATA
    from [dbo].[content_data] as copydata (NOLOCK)
        inner join @relsattrs as ra on ra.attr_new = copydata.ATTRIBUTE_ID
        inner join @contentitemstable as cit on cit.newcontentitemid = copydata.CONTENT_ITEM_ID
        inner join [dbo].[content_data] as cd (NOLOCK) on
            cd.ATTRIBUTE_ID = ra.attr_old and cd.CONTENT_ITEM_ID = cit.oldcontentitemid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca (NOLOCK)
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at (NOLOCK)
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID
        left join @relsattrs ra1 on CAST(ra1.attr_old as nvarchar) = cd.DATA;

    declare @ids varchar(max)
    select @ids = COALESCE(@ids + ', ', '') + CAST(newcontentitemid as nvarchar) from @contentitemstable

    exec qp_replicate_items @ids 

    select newcontentitemid, oldcontentitemid from @contentitemstable

    delete from @contentitemstable
    delete from @relsattrs

end
go

ALTER procedure [dbo].[qp_copy_site_contents]
    @oldsiteid numeric,
    @newsiteid numeric,
    @startFrom numeric,
    @endOn numeric
as
begin

    set nocount on;
    declare @todaysDate datetime = GETDATE()
    declare @isvirtual bit = 0
    declare @new_content_ids table (content_id int)
    -- copying contents

    ;with contents_with_row_number
    as
    (
        select ROW_NUMBER() over(order by content_id) as [row_number] 
            ,[content_name]
          ,[description]
          ,@newsiteid as siteId
          ,@todaysDate as created
          ,@todaysDate as modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from [dbo].[content] (nolock)
      where site_id = @oldsiteid and virtual_type = 0
    )
    insert into [dbo].[content] ([content_name]
          ,[description]
          ,[site_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context])
    output inserted.CONTENT_ID
        into @new_content_ids
    select [content_name]
          ,[description]
          ,siteId
          ,created
          ,modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from contents_with_row_number
      where row_number between @startFrom and @endOn
  
    -- copying attributes 
    declare @content_ids nvarchar(max)

    select @content_ids = COALESCE(@content_ids + ', ', '') + CAST(content_id as nvarchar) from @new_content_ids

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @isvirtual, @content_ids 
      
    declare  @rels_attrs table(attr_old int, attr_new int);
    insert into @rels_attrs
        select ca.attribute_id as cat_old
            , ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = @isvirtual
        ) as ra on ra.source_content_id = ca.content_id
        inner join @new_content_ids as nci on ra.destination_content_id = nci.content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id	
    
    update ca
    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
    from [dbo].[content_attribute] as ca (nolock)
        inner join content as c 
            on ca.CONTENT_ID = c.CONTENT_ID 
        inner join @rels_attrs as ra
            on ca.ATTRIBUTE_ID = ra.attr_new
        inner join CONTENT_ATTRIBUTE as ca1  (nolock)
            on ra.attr_old = ca1.ATTRIBUTE_ID
    where c.SITE_ID = @newsiteid
    
    insert into [dbo].[dynamic_image_attribute]
    select ra.attr_new
      ,[width]
      ,[height]
      ,[type]
      ,[quality]
      ,[max_size]
    from [dbo].[dynamic_image_attribute] as dia (nolock)
        inner join @rels_attrs as ra on dia.ATTRIBUTE_ID = ra.attr_old
    
            
    select COUNT(*) from @new_content_ids
end
go

ALTER PROCEDURE [dbo].[qp_copy_site_contents_update]
    @oldsiteid int,
    @newsiteid int
AS
BEGIN

    set nocount on;

    declare @isVirtual bit = 0
    --copying links between contents
    declare @relations_between_content_links table(
        oldlink int,
        newlink int
    )
    
    declare @relations_between_contents table(
        source_content_id int,
        destination_content_id int
    )
    insert into @relations_between_contents 
                select c.content_id as source_content_id, nc.content_id as destination_content_id 
                from [dbo].[content] as c (nolock)
                inner join [dbo].[content] as nc (nolock) 
                    on nc.content_name = c.content_name and nc.site_id = @newSiteId
                where c.site_id = @oldSiteId and c.virtual_type = @isVirtual
    
    merge [dbo].content_to_content as t
    using(
    select cc.[link_id]
          ,rbc.destination_content_id 
          ,rbc1.destination_content_id 
          ,cc.[map_as_class]
          ,[net_link_name]
          ,[net_plural_link_name]
          ,[symmetric]
        from [dbo].content_to_content as cc (nolock)
        inner join [dbo].content as c (nolock) on c.content_id = l_content_id
        inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
        inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
        where c.site_id = @oldsiteid
    )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
    on 0 = 1
    when not matched then
       insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       output src.[link_id], inserted.[link_id]
        into @relations_between_content_links;

    -- делаем соответствие между связанными аттрибутами
    declare @new_content_ids varchar(8000)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(destination_content_id as nvarchar) from @relations_between_contents

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @isVirtual, @new_content_ids

    update content_attribute
    set link_id = rc.newlink,
        default_value = rc.newlink
        from content_attribute as ca (nolock)
        inner join @relations_between_content_links as rc on rc.oldlink = ca.link_id
        inner join attribute_type as at (nolock) on at.attribute_type_id = ca.attribute_type_id and at.[TYPE_NAME] = 'Relation'
        inner join @relations_between_contents as rbc on ca.content_id = rbc.destination_content_id
    
    
    -- copying groups
    delete from [dbo].[content_group]
        where site_id = @newsiteid

    insert into [dbo].[content_group]
    select @newsiteid
          ,[name]
      from [dbo].[content_group] (nolock)
      where site_id = @oldsiteid
    
    -- updating groups
    ;with relations_between_groups as 
    (
        select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
        from [dbo].[content_group] as c
        inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid
    )
    update [dbo].content
    set content_group_id = rbg.content_group_id_new
    from [dbo].[CONTENT] as c (nolock)
        inner join relations_between_groups as rbg (nolock) on c.content_group_id = rbg.content_group_id_old
    where site_id = @newsiteid	
    
    
    
    -- copying access data
    delete FROM [dbo].[CONTENT_ACCESS] 
    where CONTENT_ID in (
        select c.CONTENT_ID from content as c (nolock)
        inner join [dbo].[content] as c1 (nolock) on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @newsiteid
    )
  
    declare @now datetime
    set @now = GETDATE()
  
    insert into [CONTENT_ACCESS](
        [content_id]
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[propagate_to_items]
    ) 
    select rbc.destination_content_id
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,@now
          ,@now
          ,[last_modified_by]
          ,[propagate_to_items]
    from [dbo].[content_access] as ca (nolock)
        inner join @relations_between_contents as rbc on ca.CONTENT_ID = rbc.source_content_id

END
go


ALTER PROCEDURE [dbo].[qp_copy_site_settings]
    @oldSiteId int,
    @newSiteId int
AS
BEGIN
    set nocount on;

    declare @todaysDate datetime
    set @todaysDate = GETDATE()
    
    -- copying workflows
    insert into [dbo].[workflow]
        (
           [workflow_name]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[site_id]
          ,[create_default_notification]
          ,[apply_by_default]
        )
    SELECT [WORKFLOW_NAME]
          ,w.[DESCRIPTION]
          ,@todaysDate
          ,@todaysDate
          ,w.[LAST_MODIFIED_BY]
          ,@newsiteid
          ,[create_default_notification]
          ,[apply_by_default]
      FROM [dbo].[workflow] as w
        inner join [dbo].[SITE] as s
            on w.SITE_ID = s.SITE_ID and s.SITE_ID = @oldsiteid
    
    --copying workflow rules	
    ;with relations_between_workflows
    as 
    (
        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                ,w2.WORKFLOW_ID as new_workflow_id
        FROM [dbo].[workflow] as w1
            inner join [dbo].[workflow] as w2 
                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @newsiteid
        where w1.SITE_ID = @oldsiteid
    )	
    insert into [dbo].[workflow_rules] 
    select [user_id]
          ,[group_id]
          ,[rule_order]
          ,[predecessor_permission_id]
          ,[successor_permission_id]
          ,[successor_status_id]
          ,[comment]
          ,rbw.new_workflow_id
      from [dbo].[workflow_rules] as wr
        inner join relations_between_workflows as rbw
            on wr.WORKFLOW_ID = rbw.old_workflow_id
            
    
    declare @relations_between_folders table(
        old_folder_id int,
        new_folder_id int
    )
    
    --copying folders		
    merge into [dbo].[folder]
    using (
    select @newsiteid
      ,[folder_id]
      ,[parent_folder_id]
      ,[name]
      ,[description]
      ,[filter]
      ,[path]
      ,@todaysDate
      ,@todaysDate
      ,[last_modified_by]
    from [dbo].[folder]
    where SITE_ID = @oldsiteid)
    as src (site_id,[folder_id],[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    on 0 = 1
    when not matched then
    insert (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    values (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    output src.[folder_id], inserted.[folder_id]
        into @relations_between_folders;
        
    update [dbo].[folder]
    set [parent_folder_id] = rbf.new_folder_id
    from [dbo].[folder] as f
        inner join @relations_between_folders as rbf
            on f.[parent_folder_id] = rbf.old_folder_id
    where f.SITE_ID = @newsiteid
        
END
go

ALTER PROCEDURE [dbo].[qp_copy_site_templates]
    @oldSiteId int,
    @newSiteId int,
    @start int,
    @end int
AS
BEGIN
    set nocount on;

    declare @now datetime
    set @now = GETDATE()
    
    if @start = 1
    begin
        delete from [dbo].[page_template]
        where SITE_ID = @newSiteId
    end
    
    
    declare @new_templates table(new_template_id int)
    ;with templates_with_row_number as
    (
        select ROW_NUMBER() over(order by page_template_id) as [row_number] 
          ,[site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from [dbo].[page_template] as pt (nolock)
    where site_id = @oldSiteId
    )
    insert into [dbo].[page_template](
          [site_id]
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    )
    output inserted.PAGE_TEMPLATE_ID
        into @new_templates
    select @newSiteId
          ,[template_name]
          ,[template_picture]
          ,[description]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[template_body]
          ,[template_folder]
          ,[is_system]
          ,[net_template_name]
          ,[code_behind]
          ,[net_language_id]
          ,[show_filenames]
          ,[enable_viewstate]
          ,[for_mobile_devices]
          ,[preview_template_body]
          ,[preview_code_behind]
          ,[max_num_of_format_stored_versions]
          ,[custom_class_for_pages]
          ,[template_custom_class]
          ,[custom_class_for_generics]
          ,[custom_class_for_containers]
          ,[custom_class_for_forms]
          ,[assemble_in_live]
          ,[assemble_in_stage]
          ,[disable_databind]
          ,[using]
          ,[send_nocache_headers]
    from templates_with_row_number as pt
    where row_number = @start
    
    
    declare @relations_between_templates table(
        page_template_id_old int,
        page_template_id_new int
    )
    
    insert into	@relations_between_templates
        select pto.page_template_id as page_template_id_old, ptn.page_template_id as page_template_id_new from page_template as pto (nolock)
            inner join page_template as ptn (nolock)
                on pto.template_name = ptn.template_name and ptn.site_id = @newSiteId
            inner join @new_templates as nt 
                on ptn.PAGE_TEMPLATE_ID = nt.new_template_id
        where pto.site_id = @oldSiteId
    
    declare @new_pages_added table(page_id int, page_template_id int)
    
    insert into dbo.[page] (
        [page_template_id]
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock])
    output inserted.PAGE_ID, inserted.PAGE_TEMPLATE_ID
        into @new_pages_added
    select 
          rbt.page_template_id_new
          ,[page_name]
          ,[page_filename]
          ,[proxy_cache]
          ,[cache_hours]
          ,[charset]
          ,[codepage]
          ,[locale]
          ,[description]
          ,[reassemble]
          ,@now as created
          ,@now as modified
          ,[last_modified_by]
          ,[assembled]
          ,[last_assembled_by]
          ,[generate_trace]
          ,[page_folder]
          ,[enable_viewstate]
          ,[disable_browse_server]
          ,[set_last_modified_header]
          ,[page_custom_class]
          ,[send_nocache_headers]
          ,[permanent_lock]
    from [dbo].[page] as p (nolock)
    inner join @relations_between_templates as rbt on p.page_template_id = rbt.page_template_id_old
    
    declare @relations_between_pages table(
        old_page_id int,
        new_page_id int
    )
    
    insert into @relations_between_pages
    select po.PAGE_ID, pn.PAGE_ID from page as po (nolock)
        inner join @relations_between_templates as pt
            on po.PAGE_TEMPLATE_ID = pt.page_template_id_old
        inner join PAGE as pn (nolock)
            on po.PAGE_NAME = pn.PAGE_NAME
        inner join @relations_between_templates as pt1
            on pn.PAGE_TEMPLATE_ID = pt1.page_template_id_new
    
    declare @relations_between_objects table(
        old_object_id int,
        new_object_id int		
    )
    
    
    merge into [dbo].[object]
    using(
    select [object_id]
          ,[parent_object_id]
          ,pt.page_template_id_new as [page_template_id]
          ,rbg.new_page_id as [page_id]
          ,[object_name]
          ,[object_format_id]
          ,o.[description]
          ,[object_type_id]
          ,[use_default_values]
          ,o.[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,o.[locked_by]
          ,o.[enable_viewstate]
          ,[control_custom_class]
          ,o.[disable_databind]
          ,o.[permanent_lock]
    from dbo.[OBJECT] as o (nolock)
        inner join @relations_between_templates as pt
            on o.PAGE_TEMPLATE_ID = pt.page_template_id_old
        left join @relations_between_pages as rbg
            on o.page_id = rbg.old_page_id
    )as src ([object_id]
          ,[parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])
    on 0 = 1
    when not matched then
    insert ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])  
    values ([parent_object_id]
          ,[page_template_id]
          ,[page_id]
          ,[object_name]
          ,[object_format_id]
          ,[description]
          ,[object_type_id]
          ,[use_default_values]
          ,[last_modified_by]
          ,[allow_stage_edit]
          ,[global]
          ,[net_object_name]
          ,[locked_by]
          ,[enable_viewstate]
          ,[control_custom_class]
          ,[disable_databind]
          ,[permanent_lock])     
    output src.[object_id], inserted.[object_id]     
        into @relations_between_objects;

    declare @relations_between_object_formats table(
        source_object_format_id int,
        destination_object_format_id int
    )

    merge [dbo].[OBJECT_FORMAT]
    using (
            select
            [object_format_id]
          ,rbo.new_object_id as [object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock]
    from [dbo].[OBJECT_FORMAT] as oft (nolock)
    inner join @relations_between_objects as rbo
        on oft.[OBJECT_ID] = rbo.old_object_id
    ) as src([object_format_id]
            ,[object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    on 0 = 1
    when not matched then
    insert ([object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    values ([object_id]
          ,[format_name]
          ,[description]
          ,[last_modified_by]
          ,[format_body]
          ,[net_language_id]
          ,[net_format_name]
          ,[code_behind]
          ,[assemble_notification_in_live]
          ,[assemble_notification_in_stage]
          ,[assemble_preview_in_live]
          ,[assemble_preview_in_stage]
          ,[tag_name]
          ,[permanent_lock])
    output src.[object_format_id], inserted.[object_format_id]     
        into @relations_between_object_formats; 
    
    update [dbo].[object]
    set OBJECT_FORMAT_ID = rbof.destination_object_format_id
    from [dbo].[object] as o (nolock)
        inner join @relations_between_object_formats as rbof
            on o.OBJECT_FORMAT_ID = rbof.source_object_format_id
        inner join @relations_between_objects as rbo
            on o.OBJECT_ID = rbo.new_object_id

    insert into [dbo].[OBJECT_VALUES] 
    select rbo.new_object_id
      ,[variable_name]
      ,[variable_value]
    from [dbo].[object_values] as ov
        inner join @relations_between_objects as rbo
            on ov.OBJECT_ID = rbo.old_object_id
    
    
    declare @isVirtual bit = null

    insert into [dbo].[container]
        (
          [object_id] 
          ,[content_id] 
          ,[allow_order_dynamic]
          ,[order_static]
          ,[order_dynamic]
          ,[filter_value]
          ,[select_start]
          ,[select_total]
          ,[schedule_dependence]
          ,[rotate_content]
          ,[apply_security]
          ,[show_archived]
          ,[cursor_type]
          ,[cursor_location]
          ,[duration]
          ,[enable_cache_invalidation]
          ,[dynamic_content_variable]
          ,[start_level]
          ,[end_level]
          ,[use_level_filtration]
          ,[return_last_modified]
        )
    select rbo.new_object_id
      ,rbc.destination_content_id
      ,[allow_order_dynamic]
      ,[order_static]
      ,[order_dynamic]
      ,[filter_value]
      ,[select_start]
      ,[select_total]
      ,[schedule_dependence]
      ,[rotate_content]
      ,[apply_security]
      ,[show_archived]
      ,[cursor_type]
      ,[cursor_location]
      ,[duration]
      ,[enable_cache_invalidation]
      ,[dynamic_content_variable]
      ,[start_level]
      ,[end_level]
      ,[use_level_filtration]
      ,[return_last_modified]
    from [dbo].[container] as c (nolock)
        inner join @relations_between_objects as rbo
            on c.[OBJECT_ID] = rbo.old_object_id
        inner join (
            select c.content_id as source_content_id, nc.content_id as destination_content_id 
            from [dbo].[content] as c (nolock)
            inner join [dbo].[content] as nc (nolock) 
                on nc.content_name = c.content_name and nc.site_id = @newSiteId
            where c.site_id = @oldSiteId
        ) as rbc
            on c.CONTENT_ID = rbc.source_content_id
    
    select COUNT(*) from @relations_between_templates
END
go

ALTER procedure [dbo].[qp_copy_site_update_links]
    @xmlparams xml,
    @sourceSiteId int,
    @destinationSiteId int
as
begin

    set nocount on;
    select 1 as A into #disable_tu_update_child_content_data;
    declare @linksrel table(
        olditemid int,
        newitemid int
    )
    
    insert into @linksrel
    select doc.col.value('./@oldId', 'int') olditemid
             ,doc.col.value('./@newId', 'int') newitemid
            from @xmlparams.nodes('/item') doc(col)
            
    --updating o2m values
    update [dbo].[content_data] 
        set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        inner join CONTENT_ATTRIBUTE as ca (nolock) 
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock) 
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join [dbo].[CONTENT_ITEM] as ci (nolock) 
            on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c (nolock) 
            on c.CONTENT_ID = ci.CONTENT_ID
        inner join @linksrel lr 
            on cast(lr.olditemid as varchar) = cd.DATA
        where c.SITE_ID = @destinationSiteId
    
    --updating link values

    update [dbo].[content_data] 
        set data = (case when lc.newvalue is not null then lc.newvalue else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        left join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) lc 
            on  cast(lc.oldvalue as varchar) = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca (nolock)
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock)
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join @linksrel as lr1 
            on lr1.newitemid = cd.content_item_id

    --inserting relations between new items
    insert into [dbo].[item_to_item]
    select r.newvalue, lr_l.newitemid, lr_r.newitemid 
    from [dbo].[item_to_item] as ii (nolock)
    inner join @linksrel as lr_l 
        on lr_l.olditemid = ii.l_item_id
    inner join @linksrel as lr_r 
        on lr_r.olditemid = ii.r_item_id
    inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
        on r.oldvalue = ii.link_id	
    
    SELECT COUNT(*) from @linksrel
end
go

ALTER procedure [dbo].[qp_copy_site_virtual_contents]
    @oldsiteid int,
    @newsiteid int
as
begin
    set nocount on;

    declare @newvirtualcontents table(
        old_content_id int,
        new_content_id int,
        virtual_type int,
        sqlquery nvarchar(max),
        altquery nvarchar(max)
    )
    declare @relscontents as table(
        content_id_old int,
        content_id_new int
    )
    declare @isVirtual bit = 0

    insert into @relscontents
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @newsiteid
                    where c.site_id = @oldsiteid and c.virtual_type = @isVirtual

    merge [dbo].[content]
    using (
        select content_id
               ,content_name
              ,[description]
              ,@newsiteid
              ,[created]
              ,[modified]
              ,[last_modified_by]
              ,[friendly_name_plural]
              ,[friendly_name_singular]
              ,[allow_items_permission]
              ,[content_group_id]
              ,[external_id]
              ,[virtual_type]
              ,rc.content_id_new virtual_join_primary_content_id_new
              ,c.virtual_join_primary_content_id
              ,[is_shared]
              ,[auto_archive]
              ,[max_num_of_stored_versions]
              ,[version_control_view]
              ,[content_page_size]
              ,[map_as_class]
              ,[net_content_name]
              ,[net_plural_content_name]
              ,[use_default_filtration]
              ,[query]
              ,[alt_query]
              ,[add_context_class_name]
              ,[xaml_validation]
              ,[disable_xaml_validation]
              ,[disable_changing_actions]
              ,[parent_content_id]
              ,[use_for_context]
          from [dbo].[content] as c (nolock)
          left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
      where virtual_type != 0 and site_id = @oldsiteid) 
      as src(content_id, content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id_new], [virtual_join_primary_content_id], [is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
      on 0 = 1
      when not matched then
       insert (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id],[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       values (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],virtual_join_primary_content_id_new,[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
        into @newvirtualcontents;    
    

    declare @for_virtual_contents bit = 1
    declare @new_content_ids varchar(max)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(new_content_id as nvarchar) from @newvirtualcontents

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids

    ---- делаем соответствие между связанными аттрибутами

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids


    insert into union_contents
    select nvc1.new_content_id, rc.content_id_new, rc1.content_id_new 
    from union_contents as uc (nolock)
    inner join @newvirtualcontents as nvc1 on uc.virtual_content_id = nvc1.old_content_id
    left join @relscontents as rc on uc.union_content_id = rc.content_id_old
    left join @relscontents as rc1 on uc.master_content_id = rc1.content_id_old

    
    declare @relations_between_contents_links table(
            oldvalue int
            ,newvalue int
            
    )
    insert into @relations_between_contents_links
    select oldvalue, newvalue from [dbo].[GetRelationsBetweenContentLinks](@oldSiteId, @newSiteId)
    
    update [dbo].[content_attribute]
    set link_id = lc.newvalue,
        default_value =lc1.newvalue
    from  [dbo].[content_attribute] as ca (nolock)
        inner join @relations_between_contents_links as lc 
            on ca.link_id = CAST(lc.oldvalue as varchar)
        inner join @relations_between_contents_links as lc1 
            on ca.default_value = CAST(lc1.oldvalue as varchar)
        inner join content as c (nolock) 
            on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @newsiteid

    update [dbo].[content]
    set content_group_id = cg1.newGroupId
    from [dbo].[content] as c (nolock)
        inner join
        (
            select cg.content_group_id as oldGroupId, cg1.content_group_id as newGroupId 
            from [content_group] cg (nolock)
                inner join content_group cg1 
                    on cg.name = cg1.name and cg1.site_id = @newsiteid
            where cg.site_id = @oldsiteid
        ) as cg1 
            on c.content_group_id = cg1.oldGroupId
    where c.SITE_ID = @newsiteid


    select 	old_content_id
        , new_content_id
        , virtual_type
        , sqlquery
        , altquery
    from @newvirtualcontents
end
go

IF object_id(N'[GetRelationsBetweenContents]', N'FN') IS NOT NULL
    DROP FUNCTION [dbo].[GetRelationsBetweenContents]
GO


INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.42', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.42 completed'
GO


-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.43
-- fix copy articles (over 1 mill)
-- **************************************

ALTER procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @startfrom int,
        @endon int
as
begin

    set nocount on;
    select 1 as A into #disable_tu_update_child_content_data;

    DECLARE @not_for_replication bit
    SET @not_for_replication = 1

    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
            ,primary key (attr_old, attr_new)
    )


    declare @relations_between_statuses table(
        old_status_type_id numeric(18,0),
        new_status_type_id numeric(18,0)
    )
    insert into @relations_between_statuses
                    select st1.STATUS_TYPE_ID as old_status_type_id
                        ,st2.STATUS_TYPE_ID as new_status_type_id 
                from [dbo].[status_type] as st1 (NOLOCK)
                inner join [dbo].[status_type] as st2 (NOLOCK) 
                    on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                where st1.SITE_ID = @oldSiteId

    declare @relations_between_contents table(
        source_content_id numeric(18,0),
        destination_content_id numeric(18,0)
    )
    insert into @relations_between_contents
        select c.content_id as source_content_id, 
                nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0


    declare @contentitemstable table( 
            newcontentitemid int ,
            contentid int,
            oldcontentitemid int,
            primary key ( newcontentitemid, oldcontentitemid, contentid));

    declare @isVirtual bit = 0
       
    declare @todaysDateTime datetime
    set @todaysDateTime = GetDate();


    insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca (NOLOCK)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0
        ) as ra on ra.source_content_id = ca.content_id
        left join [dbo].content_attribute as ca1 (NOLOCK) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id

    ;with content_items as (
        select [content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.destination_content_id as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,@not_for_replication as [not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
         from (
            select row_number() over (order by content_item_id) as rownumber
                ,content_item_id
                  ,[visible]
                  ,[status_type_id]
                  ,[created]
                  ,[modified]
                  ,[content_id]
                  ,[last_modified_by]
                  ,[locked_by]
                  ,[archive]
                  ,[not_for_replication]
                  ,[schedule_new_version_publication]
                  ,[splitted]
                  ,[cancel_split]
            from [dbo].[content_item] (NOLOCK)
        )  as c1
            inner join [dbo].[content] as c (NOLOCK) on c.content_id = c1.content_id and c.site_id = @oldsiteid
            inner join @relations_between_contents as rc on rc.source_content_id = c1.content_id 
            inner join @relations_between_statuses as rbs on c1.[status_type_id] = rbs.old_status_type_id
        where c1.rownumber between @startfrom and @endon
    )
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from content_items as t
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;			
                    
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,1 as[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis (NOLOCK)
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid;

    update copydata
    set [data] =(	case	when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.attr_new as varchar)) 
                            when at.[TYPE_NAME] = 'Relation Many-to-One' and cd.DATA is not null then CAST(ra1.attr_new as varchar)
                            else cd.data
                    end)
      ,[blob_data] = cd.BLOB_DATA
    from [dbo].[content_data] as copydata (NOLOCK)
        inner join @relsattrs as ra on ra.attr_new = copydata.ATTRIBUTE_ID
        inner join @contentitemstable as cit on cit.newcontentitemid = copydata.CONTENT_ITEM_ID
        inner join [dbo].[content_data] as cd (NOLOCK) on
            cd.ATTRIBUTE_ID = ra.attr_old and cd.CONTENT_ITEM_ID = cit.oldcontentitemid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca (NOLOCK)
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at (NOLOCK)
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID
        left join @relsattrs ra1 on CAST(ra1.attr_old as nvarchar) = cd.DATA;

    declare @ids varchar(max)
    select @ids = COALESCE(@ids + ', ', '') + CAST(newcontentitemid as nvarchar) from @contentitemstable

    exec qp_replicate_items @ids 

    select newcontentitemid, oldcontentitemid from @contentitemstable

    delete from @contentitemstable
    delete from @relsattrs

end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.43', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.43 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.44
-- fix copy articles
-- **************************************

ALTER procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @startfrom int,
        @endon int
as
begin

    set nocount on;
    select 1 as A into #disable_tu_update_child_content_data;

    DECLARE @not_for_replication bit
    SET @not_for_replication = 1

    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
            ,primary key (attr_old, attr_new)
    )


    declare @relations_between_statuses table(
        old_status_type_id numeric(18,0),
        new_status_type_id numeric(18,0)
    )
    insert into @relations_between_statuses
                    select st1.STATUS_TYPE_ID as old_status_type_id
                        ,st2.STATUS_TYPE_ID as new_status_type_id 
                from [dbo].[status_type] as st1 (NOLOCK)
                inner join [dbo].[status_type] as st2 (NOLOCK) 
                    on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                where st1.SITE_ID = @oldSiteId

    declare @relations_between_contents table(
        source_content_id numeric(18,0),
        destination_content_id numeric(18,0)
    )
    insert into @relations_between_contents
        select c.content_id as source_content_id, 
                nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0


    declare @contentitemstable table( 
            newcontentitemid int ,
            contentid int,
            oldcontentitemid int,
            primary key ( newcontentitemid, oldcontentitemid, contentid));

    declare @isVirtual bit = 0
       
    declare @todaysDateTime datetime
    set @todaysDateTime = GetDate();


    insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca (NOLOCK)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0
        ) as ra on ra.source_content_id = ca.content_id
        left join [dbo].content_attribute as ca1 (NOLOCK) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id

    ;with content_items as (
        select [content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.destination_content_id as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,@not_for_replication as [not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
         from (
            select row_number() over (order by content_item_id) as rownumber
                ,content_item_id
                  ,[visible]
                  ,[status_type_id]
                  ,c2.[created]
                  ,c2.[modified]
                  ,c2.[content_id]
                  ,c2.[last_modified_by]
                  ,[locked_by]
                  ,[archive]
                  ,[not_for_replication]
                  ,[schedule_new_version_publication]
                  ,[splitted]
                  ,[cancel_split]
            from [dbo].[content_item] (NOLOCK) as c2
            inner join [dbo].[content] as c (NOLOCK) on c.content_id = c2.content_id and c.site_id = @oldsiteid
        )  as c1
            inner join @relations_between_contents as rc on rc.source_content_id = c1.content_id 
            inner join @relations_between_statuses as rbs on c1.[status_type_id] = rbs.old_status_type_id
        where c1.rownumber between @startfrom and @endon
    )
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from content_items as t
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;			
                    
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,1 as[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis (NOLOCK)
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid;

    update copydata
    set [data] =(	case	when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.attr_new as varchar)) 
                            when at.[TYPE_NAME] = 'Relation Many-to-One' and cd.DATA is not null then CAST(ra1.attr_new as varchar)
                            else cd.data
                    end)
      ,[blob_data] = cd.BLOB_DATA
    from [dbo].[content_data] as copydata (NOLOCK)
        inner join @relsattrs as ra on ra.attr_new = copydata.ATTRIBUTE_ID
        inner join @contentitemstable as cit on cit.newcontentitemid = copydata.CONTENT_ITEM_ID
        inner join [dbo].[content_data] as cd (NOLOCK) on
            cd.ATTRIBUTE_ID = ra.attr_old and cd.CONTENT_ITEM_ID = cit.oldcontentitemid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca (NOLOCK)
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at (NOLOCK)
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID
        left join @relsattrs ra1 on CAST(ra1.attr_old as nvarchar) = cd.DATA;

    declare @ids varchar(max)
    select @ids = COALESCE(@ids + ', ', '') + CAST(newcontentitemid as nvarchar) from @contentitemstable
    if @ids is not null
        exec qp_replicate_items @ids 

    select newcontentitemid, oldcontentitemid from @contentitemstable

    delete from @contentitemstable
    delete from @relsattrs

end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.44', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.44 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.45
-- fix virtual contents
-- **************************************

ALTER procedure [dbo].[qp_copy_site_contents]
    @oldsiteid numeric,
    @newsiteid numeric,
    @startFrom numeric,
    @endOn numeric
as
begin

    set nocount on;
    declare @todaysDate datetime = GETDATE()
    declare @isvirtual bit = 0
    declare @new_content_ids table (content_id int)
    -- copying contents

    ;with contents_with_row_number
    as
    (
        select ROW_NUMBER() over(order by content_id) as [row_number] 
            ,[content_name]
          ,[description]
          ,@newsiteid as siteId
          ,@todaysDate as created
          ,@todaysDate as modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from [dbo].[content] (nolock)
      where site_id = @oldsiteid and virtual_type = 0
    )
    insert into [dbo].[content] ([content_name]
          ,[description]
          ,[site_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context])
    output inserted.CONTENT_ID
        into @new_content_ids
    select [content_name]
          ,[description]
          ,siteId
          ,created
          ,modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from contents_with_row_number
      where row_number between @startFrom and @endOn
  
    -- copying attributes 
    declare @content_ids nvarchar(max)

    select @content_ids = COALESCE(@content_ids + ', ', '') + CAST(content_id as nvarchar) from @new_content_ids

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @isvirtual, @content_ids 
      
    ;with rels_attrs as (
        select ca.attribute_id as attr_old
            , ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = @isvirtual
        ) as ra on ra.source_content_id = ca.content_id
        inner join @new_content_ids as nci on ra.destination_content_id = nci.content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id	
    )
    insert into [dbo].[dynamic_image_attribute]
    select ra.attr_new
      ,[width]
      ,[height]
      ,[type]
      ,[quality]
      ,[max_size]
    from [dbo].[dynamic_image_attribute] as dia (nolock)
        inner join rels_attrs as ra on dia.ATTRIBUTE_ID = ra.attr_old
    
            
    select COUNT(*) from @new_content_ids
end
go

ALTER procedure [dbo].[qp_copy_site_virtual_contents]
    @oldsiteid int,
    @newsiteid int
as
begin
    set nocount on;

    declare @newvirtualcontents table(
        old_content_id int,
        new_content_id int,
        virtual_type int,
        sqlquery nvarchar(max),
        altquery nvarchar(max)
    )
    declare @relscontents as table(
        content_id_old int,
        content_id_new int
    )
    declare @isVirtual bit = 0

    insert into @relscontents
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @newsiteid
                    where c.site_id = @oldsiteid

    merge [dbo].[content]
    using (
        select content_id
               ,content_name
              ,[description]
              ,@newsiteid
              ,[created]
              ,[modified]
              ,[last_modified_by]
              ,[friendly_name_plural]
              ,[friendly_name_singular]
              ,[allow_items_permission]
              ,[content_group_id]
              ,[external_id]
              ,[virtual_type]
              ,rc.content_id_new virtual_join_primary_content_id_new
              ,c.virtual_join_primary_content_id
              ,[is_shared]
              ,[auto_archive]
              ,[max_num_of_stored_versions]
              ,[version_control_view]
              ,[content_page_size]
              ,[map_as_class]
              ,[net_content_name]
              ,[net_plural_content_name]
              ,[use_default_filtration]
              ,[query]
              ,[alt_query]
              ,[add_context_class_name]
              ,[xaml_validation]
              ,[disable_xaml_validation]
              ,[disable_changing_actions]
              ,[parent_content_id]
              ,[use_for_context]
          from [dbo].[content] as c (nolock)
          left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
      where virtual_type != 0 and site_id = @oldsiteid) 
      as src(content_id, content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id_new], [virtual_join_primary_content_id], [is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
      on 0 = 1
      when not matched then
       insert (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id],[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       values (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],virtual_join_primary_content_id_new,[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
        into @newvirtualcontents;    
    

    declare @for_virtual_contents bit = 1
    declare @new_content_ids varchar(max)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(new_content_id as nvarchar) from @newvirtualcontents

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids

    ---- делаем соответствие между связанными аттрибутами

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids


    insert into union_contents
    select nvc1.new_content_id, rc.content_id_new, rc1.content_id_new 
    from union_contents as uc (nolock)
    inner join @newvirtualcontents as nvc1 on uc.virtual_content_id = nvc1.old_content_id
    left join @relscontents as rc on uc.union_content_id = rc.content_id_old
    left join @relscontents as rc1 on uc.master_content_id = rc1.content_id_old

    
    declare @relations_between_contents_links table(
            oldvalue int
            ,newvalue int
            
    )
    insert into @relations_between_contents_links
    select oldvalue, newvalue from [dbo].[GetRelationsBetweenContentLinks](@oldSiteId, @newSiteId)
    
    update [dbo].[content_attribute]
    set link_id = lc.newvalue,
        default_value =lc1.newvalue
    from  [dbo].[content_attribute] as ca (nolock)
        inner join @relations_between_contents_links as lc 
            on ca.link_id = CAST(lc.oldvalue as varchar)
        inner join @relations_between_contents_links as lc1 
            on ca.default_value = CAST(lc1.oldvalue as varchar)
        inner join content as c (nolock) 
            on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @newsiteid
    

    ;with rels_attrs as (
        select ca.attribute_id as attr_old
            , ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId
        ) as ra on ra.source_content_id = ca.content_id
        inner join @newvirtualcontents as nci on ra.destination_content_id = nci.new_content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id	
    )
    update ca
    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
    from [dbo].[content_attribute] as ca (nolock)
        inner join content as c 
            on ca.CONTENT_ID = c.CONTENT_ID 
        inner join rels_attrs as ra
            on ca.ATTRIBUTE_ID = ra.attr_new
        inner join CONTENT_ATTRIBUTE as ca1  (nolock)
            on ra.attr_old = ca1.ATTRIBUTE_ID
    where c.SITE_ID = @newsiteid

    -- copying groups
    delete from [dbo].[content_group]
        where site_id = @newsiteid

    insert into [dbo].[content_group]
    select @newsiteid
          ,[name]
      from [dbo].[content_group] (nolock)
      where site_id = @oldsiteid
    
    -- updating groups
    ;with relations_between_groups as 
    (
        select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
        from [dbo].[content_group] as c
        inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid
    )
    update [dbo].content
    set content_group_id = rbg.content_group_id_new
    from [dbo].[CONTENT] as c (nolock)
        inner join relations_between_groups as rbg (nolock) on c.content_group_id = rbg.content_group_id_old
    where site_id = @newsiteid	


    select 	old_content_id
        , new_content_id
        , virtual_type
        , sqlquery
        , altquery
    from @newvirtualcontents
end
go

ALTER PROCEDURE [dbo].[qp_copy_site_update_attributes]
    @oldSiteId int,
    @newSiteId int,
    @isvirtual bit,
    @new_content_ids nvarchar(max)
AS
BEGIN

    set nocount on;
    if @new_content_ids is not null begin
    ;with relsattrs as
    (
        select ca.attribute_id as attr_old
            ,ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @newSiteId
                    where c.site_id = @oldSiteId
        ) as rbc on ca.content_id = rbc.source_content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca.attribute_name = ca1.attribute_name and ca1.content_id = rbc.destination_content_id
    )
    update [dbo].[content_attribute]
    set		[related_attribute_id] = rai.attr_new
          ,[related_image_attribute_id]= ria.attr_new
          ,[persistent_attr_id]= pai.attr_new
          ,[join_attr_id]= jai.attr_new
          ,[back_related_attribute_id]= bra.attr_new
          ,[classifier_attribute_id]= cai.attr_new
          ,[tree_order_field] = tof.attr_new
          ,[PARENT_ATTRIBUTE_ID] = paid.attr_new
    from [dbo].[content_attribute] as ca (nolock)
    left join relsattrs as rai on rai.attr_old = ca.related_attribute_id
    left join relsattrs as ria on ria.attr_old = ca.related_image_attribute_id
    left join relsattrs as pai on pai.attr_old = ca.persistent_attr_id
    left join relsattrs as jai on jai.attr_old = ca.join_attr_id
    left join relsattrs as bra on bra.attr_old = ca.back_related_attribute_id
    left join relsattrs as cai on cai.attr_old = ca.classifier_attribute_id
    left join relsattrs as tof on tof.attr_old = ca.tree_order_field
    left join relsattrs as paid on paid.attr_old = ca.PARENT_ATTRIBUTE_ID
    where ca.CONTENT_ID in (SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ','))

    end
END
go

ALTER PROCEDURE [dbo].[qp_copy_site_contents_update]
    @oldsiteid int,
    @newsiteid int
AS
BEGIN

    set nocount on;

    declare @isVirtual bit = 0
    --copying links between contents
    declare @relations_between_content_links table(
        oldlink int,
        newlink int
    )
    
    declare @relations_between_contents table(
        source_content_id int,
        destination_content_id int
    )
    insert into @relations_between_contents 
                select c.content_id as source_content_id, nc.content_id as destination_content_id 
                from [dbo].[content] as c (nolock)
                inner join [dbo].[content] as nc (nolock) 
                    on nc.content_name = c.content_name and nc.site_id = @newSiteId
                where c.site_id = @oldSiteId and c.virtual_type = @isVirtual
    
    merge [dbo].content_to_content as t
    using(
    select cc.[link_id]
          ,rbc.destination_content_id 
          ,rbc1.destination_content_id 
          ,cc.[map_as_class]
          ,[net_link_name]
          ,[net_plural_link_name]
          ,[symmetric]
        from [dbo].content_to_content as cc (nolock)
        inner join [dbo].content as c (nolock) on c.content_id = l_content_id
        inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
        inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
        where c.site_id = @oldsiteid
    )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
    on 0 = 1
    when not matched then
       insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       output src.[link_id], inserted.[link_id]
        into @relations_between_content_links;

    -- делаем соответствие между связанными аттрибутами
    declare @new_content_ids varchar(8000)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(destination_content_id as nvarchar) from @relations_between_contents

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @isVirtual, @new_content_ids

    update content_attribute
    set link_id = rc.newlink,
        default_value = rc.newlink
        from content_attribute as ca (nolock)
        inner join @relations_between_content_links as rc on rc.oldlink = ca.link_id
        inner join attribute_type as at (nolock) on at.attribute_type_id = ca.attribute_type_id and at.[TYPE_NAME] = 'Relation'
        inner join @relations_between_contents as rbc on ca.content_id = rbc.destination_content_id
    
        
    -- copying access data
    delete FROM [dbo].[CONTENT_ACCESS] 
    where CONTENT_ID in (
        select c.CONTENT_ID from content as c (nolock)
        inner join [dbo].[content] as c1 (nolock) on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @newsiteid
    )
  
    declare @now datetime
    set @now = GETDATE()
  
    insert into [CONTENT_ACCESS](
        [content_id]
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[propagate_to_items]
    ) 
    select rbc.destination_content_id
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,@now
          ,@now
          ,[last_modified_by]
          ,[propagate_to_items]
    from [dbo].[content_access] as ca (nolock)
        inner join @relations_between_contents as rbc on ca.CONTENT_ID = rbc.source_content_id

END
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.45', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.45 completed'
GO


-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.46
-- fix for attribute arder in contents
-- **************************************

ALTER procedure [dbo].[qp_copy_site_contents]
    @oldsiteid numeric,
    @newsiteid numeric,
    @startFrom numeric,
    @endOn numeric
as
begin

    set nocount on;
    declare @todaysDate datetime = GETDATE()
    declare @isvirtual bit = 0
    declare @new_content_ids table (content_id int)
    -- copying contents

    ;with contents_with_row_number
    as
    (
        select ROW_NUMBER() over(order by content_id) as [row_number] 
            ,[content_name]
          ,[description]
          ,@newsiteid as siteId
          ,@todaysDate as created
          ,@todaysDate as modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from [dbo].[content] (nolock)
      where site_id = @oldsiteid and virtual_type = 0
    )
    insert into [dbo].[content] ([content_name]
          ,[description]
          ,[site_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context])
    output inserted.CONTENT_ID
        into @new_content_ids
    select [content_name]
          ,[description]
          ,siteId
          ,created
          ,modified
          ,[last_modified_by]
          ,[friendly_name_plural]
          ,[friendly_name_singular]
          ,[allow_items_permission]
          ,[content_group_id]
          ,[external_id]
          ,[virtual_type]
          ,[virtual_join_primary_content_id]
          ,[is_shared]
          ,[auto_archive]
          ,[max_num_of_stored_versions]
          ,[version_control_view]
          ,[content_page_size]
          ,[map_as_class]
          ,[net_content_name]
          ,[net_plural_content_name]
          ,[use_default_filtration]
          ,[add_context_class_name]
          ,[query]
          ,[alt_query]
          ,[xaml_validation]
          ,[disable_xaml_validation]
          ,[disable_changing_actions]
          ,[parent_content_id]
          ,[use_for_context]
      from contents_with_row_number
      where row_number between @startFrom and @endOn
  
    -- copying attributes 
    declare @content_ids nvarchar(max)

    select @content_ids = COALESCE(@content_ids + ', ', '') + CAST(content_id as nvarchar) from @new_content_ids

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @isvirtual, @content_ids 
     
     
    declare @rels_attrs table(
        attr_old int,
        attr_new int
    )  

    insert into @rels_attrs
        select ca.attribute_id as attr_old
            , ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = @isvirtual
        ) as ra on ra.source_content_id = ca.content_id
        inner join @new_content_ids as nci on ra.destination_content_id = nci.content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id	

    insert into [dbo].[dynamic_image_attribute]
    select ra.attr_new
      ,[width]
      ,[height]
      ,[type]
      ,[quality]
      ,[max_size]
    from [dbo].[dynamic_image_attribute] as dia (nolock)
        inner join @rels_attrs as ra on dia.ATTRIBUTE_ID = ra.attr_old
    
    
    update ca
    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
    from [dbo].[content_attribute] as ca (nolock)
        inner join content as c (nolock)
            on ca.CONTENT_ID = c.CONTENT_ID 
        inner join @rels_attrs as ra
            on ca.ATTRIBUTE_ID = ra.attr_new
        inner join CONTENT_ATTRIBUTE as ca1  (nolock)
            on ra.attr_old = ca1.ATTRIBUTE_ID
    where c.SITE_ID = @newsiteid
    
            
    select COUNT(*) from @new_content_ids
end
go


ALTER procedure [dbo].[qp_copy_site_virtual_contents]
    @oldsiteid int,
    @newsiteid int
as
begin
    set nocount on;

    declare @newvirtualcontents table(
        old_content_id int,
        new_content_id int,
        virtual_type int,
        sqlquery nvarchar(max),
        altquery nvarchar(max)
    )
    declare @relscontents as table(
        content_id_old int,
        content_id_new int
    )
    declare @isVirtual bit = 0

    insert into @relscontents
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                    from [dbo].[content] as c (nolock)
                    inner join [dbo].[content] as nc (nolock) 
                        on nc.content_name = c.content_name and nc.site_id = @newsiteid
                    where c.site_id = @oldsiteid

    merge [dbo].[content]
    using (
        select content_id
               ,content_name
              ,[description]
              ,@newsiteid
              ,[created]
              ,[modified]
              ,[last_modified_by]
              ,[friendly_name_plural]
              ,[friendly_name_singular]
              ,[allow_items_permission]
              ,[content_group_id]
              ,[external_id]
              ,[virtual_type]
              ,rc.content_id_new virtual_join_primary_content_id_new
              ,c.virtual_join_primary_content_id
              ,[is_shared]
              ,[auto_archive]
              ,[max_num_of_stored_versions]
              ,[version_control_view]
              ,[content_page_size]
              ,[map_as_class]
              ,[net_content_name]
              ,[net_plural_content_name]
              ,[use_default_filtration]
              ,[query]
              ,[alt_query]
              ,[add_context_class_name]
              ,[xaml_validation]
              ,[disable_xaml_validation]
              ,[disable_changing_actions]
              ,[parent_content_id]
              ,[use_for_context]
          from [dbo].[content] as c (nolock)
          left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
      where virtual_type != 0 and site_id = @oldsiteid) 
      as src(content_id, content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id_new], [virtual_join_primary_content_id], [is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
      on 0 = 1
      when not matched then
       insert (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],[virtual_join_primary_content_id],[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       values (content_name,[description],[site_id],[created],[modified],[last_modified_by],[friendly_name_plural],[friendly_name_singular],[allow_items_permission],[content_group_id],[external_id],[virtual_type],virtual_join_primary_content_id_new,[is_shared]
          ,[auto_archive],[max_num_of_stored_versions],[version_control_view],[content_page_size],[map_as_class],[net_content_name],[net_plural_content_name],[use_default_filtration]
          ,[query],[alt_query],[add_context_class_name],[xaml_validation],[disable_xaml_validation],[disable_changing_actions],[parent_content_id],[use_for_context])
       output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
        into @newvirtualcontents;    
    

    declare @for_virtual_contents bit = 1
    declare @new_content_ids varchar(max)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(new_content_id as nvarchar) from @newvirtualcontents

    exec qp_copy_site_copy_contents_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids

    ---- делаем соответствие между связанными аттрибутами

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @for_virtual_contents, @new_content_ids


    insert into union_contents
    select nvc1.new_content_id, rc.content_id_new, rc1.content_id_new 
    from union_contents as uc (nolock)
    inner join @newvirtualcontents as nvc1 on uc.virtual_content_id = nvc1.old_content_id
    left join @relscontents as rc on uc.union_content_id = rc.content_id_old
    left join @relscontents as rc1 on uc.master_content_id = rc1.content_id_old

    
    declare @relations_between_contents_links table(
            oldvalue int
            ,newvalue int
            
    )
    insert into @relations_between_contents_links
    select oldvalue, newvalue from [dbo].[GetRelationsBetweenContentLinks](@oldSiteId, @newSiteId)
    
    update [dbo].[content_attribute]
    set link_id = lc.newvalue,
        default_value =lc1.newvalue
    from  [dbo].[content_attribute] as ca (nolock)
        inner join @relations_between_contents_links as lc 
            on ca.link_id = CAST(lc.oldvalue as varchar)
        inner join @relations_between_contents_links as lc1 
            on ca.default_value = CAST(lc1.oldvalue as varchar)
        inner join content as c (nolock) 
            on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @newsiteid
    

    ;with rels_attrs as (
        select ca.attribute_id as attr_old
            , ca1.attribute_id as attr_new 
        from [dbo].content_attribute as ca (nolock)
        inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId
        ) as ra on ra.source_content_id = ca.content_id
        inner join @newvirtualcontents as nci on ra.destination_content_id = nci.new_content_id
        inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id	
    )
    update ca
    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
    from [dbo].[content_attribute] as ca (nolock)
        inner join content as c (nolock)
            on ca.CONTENT_ID = c.CONTENT_ID 
        inner join rels_attrs as ra
            on ca.ATTRIBUTE_ID = ra.attr_new
        inner join CONTENT_ATTRIBUTE as ca1  (nolock)
            on ra.attr_old = ca1.ATTRIBUTE_ID
    where c.SITE_ID = @newsiteid

    -- copying groups
    delete from [dbo].[content_group]
        where site_id = @newsiteid

    insert into [dbo].[content_group]
    select @newsiteid
          ,[name]
      from [dbo].[content_group] (nolock)
      where site_id = @oldsiteid
    
    -- updating groups
    ;with relations_between_groups as 
    (
        select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new 
        from [dbo].[content_group] as c
        inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
        where c.site_id = @oldsiteid
    )
    update [dbo].content
    set content_group_id = rbg.content_group_id_new
    from [dbo].[CONTENT] as c (nolock)
        inner join relations_between_groups as rbg (nolock) on c.content_group_id = rbg.content_group_id_old
    where site_id = @newsiteid	


    select 	old_content_id
        , new_content_id
        , virtual_type
        , sqlquery
        , altquery
    from @newvirtualcontents
end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.46', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.46 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.47
-- fix for attribute arder in contents
-- **************************************

ALTER PROCEDURE [dbo].[qp_copy_site_contents_update]
    @oldsiteid int,
    @newsiteid int
AS
BEGIN

    set nocount on;

    declare @isVirtual bit = 0
    --copying links between contents
    declare @relations_between_content_links table(
        oldlink int,
        newlink int
    )
    
    declare @relations_between_contents table(
        source_content_id int,
        destination_content_id int
    )
    insert into @relations_between_contents 
                select c.content_id as source_content_id, nc.content_id as destination_content_id 
                from [dbo].[content] as c (nolock)
                inner join [dbo].[content] as nc (nolock) 
                    on nc.content_name = c.content_name and nc.site_id = @newSiteId
                where c.site_id = @oldSiteId and c.virtual_type = @isVirtual
    
    merge [dbo].content_to_content as t
    using(
    select cc.[link_id]
          ,rbc.destination_content_id 
          ,rbc1.destination_content_id 
          ,cc.[map_as_class]
          ,[net_link_name]
          ,[net_plural_link_name]
          ,[symmetric]
        from [dbo].content_to_content as cc (nolock)
        inner join [dbo].content as c (nolock) on c.content_id = l_content_id
        inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
        inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
        where c.site_id = @oldsiteid
    )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
    on 0 = 1
    when not matched then
       insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
       output src.[link_id], inserted.[link_id]
        into @relations_between_content_links;

    -- делаем соответствие между связанными аттрибутами
    declare @new_content_ids varchar(8000)
    select @new_content_ids = COALESCE(@new_content_ids + ', ', '') + CAST(destination_content_id as nvarchar) from @relations_between_contents

    exec qp_copy_site_update_attributes @oldsiteid, @newsiteid, @isVirtual, @new_content_ids

    update content_attribute
    set link_id = rc.newlink,
        default_value = rc.newlink
        from content_attribute as ca (nolock)
        inner join @relations_between_content_links as rc on rc.oldlink = ca.link_id
        inner join attribute_type as at (nolock) on at.attribute_type_id = ca.attribute_type_id and at.[TYPE_NAME] = 'Relation'
        inner join @relations_between_contents as rbc on ca.content_id = rbc.destination_content_id
    
    
    update [dbo].[content]
    set PARENT_CONTENT_ID = rbc.destination_content_id
    from [dbo].[content] as c
    inner join @relations_between_contents as rbc
        on c.PARENT_CONTENT_ID = rbc.source_content_id
    where c.SITE_ID = @newsiteid
        
    -- copying access data
    delete FROM [dbo].[CONTENT_ACCESS] 
    where CONTENT_ID in (
        select c.CONTENT_ID from content as c (nolock)
        inner join [dbo].[content] as c1 (nolock) on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @newsiteid
    )
  
    declare @now datetime
    set @now = GETDATE()
  
    insert into [CONTENT_ACCESS](
        [content_id]
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[propagate_to_items]
    ) 
    select rbc.destination_content_id
          ,[user_id]
          ,[group_id]
          ,[permission_level_id]
          ,@now
          ,@now
          ,[last_modified_by]
          ,[propagate_to_items]
    from [dbo].[content_access] as ca (nolock)
        inner join @relations_between_contents as rbc on ca.CONTENT_ID = rbc.source_content_id

END
go

ALTER procedure [dbo].[qp_copy_site_update_links]
    @xmlparams xml,
    @sourceSiteId int,
    @destinationSiteId int
as
begin

    set nocount on;


    declare @linksrel table(
        olditemid int,
        newitemid int
    )
    
    insert into @linksrel
    select doc.col.value('./@oldId', 'int') olditemid
             ,doc.col.value('./@newId', 'int') newitemid
            from @xmlparams.nodes('/item') doc(col)
            
    --updating o2m values
    update [dbo].[content_data] 
        set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        inner join CONTENT_ATTRIBUTE as ca (nolock) 
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock) 
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join [dbo].[CONTENT_ITEM] as ci (nolock) 
            on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c (nolock) 
            on c.CONTENT_ID = ci.CONTENT_ID
        inner join @linksrel lr 
            on cast(lr.olditemid as varchar) = cd.DATA
        where c.SITE_ID = @destinationSiteId
    
    --updating link values

    update [dbo].[content_data] 
        set data = (case when lc.newvalue is not null then lc.newvalue else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        left join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) lc 
            on  cast(lc.oldvalue as varchar) = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca (nolock)
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock)
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join @linksrel as lr1 
            on lr1.newitemid = cd.content_item_id

    --inserting relations between new items

    insert into [dbo].[item_to_item]
    select r.newvalue, ii.l_item_id, ii.r_item_id 
    from [dbo].[item_to_item] as ii (nolock)
    inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
        on r.oldvalue = ii.link_id	
    where ii.l_item_id in (select olditemid from @linksrel)

    update [dbo].[item_to_item]
    set l_item_id = lr.newitemid
    from [dbo].[item_to_item] as ii
        inner join @linksrel as lr on
            ii.l_item_id = lr.olditemid
        inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
            on r.newvalue = ii.link_id

    update [dbo].[item_to_item]
    set r_item_id = lr.newitemid
    from [dbo].[item_to_item] as ii
        inner join @linksrel as lr on
            ii.r_item_id = lr.olditemid
        inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
            on r.newvalue = ii.link_id

    SELECT COUNT(*) from @linksrel
end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.47', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.47 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.48
-- copy styles and formats
-- **************************************

ALTER PROCEDURE [dbo].[qp_copy_site_settings]
    @oldSiteId int,
    @newSiteId int
AS
BEGIN
    set nocount on;

    declare @todaysDate datetime
    set @todaysDate = GETDATE()
    
    -- copying workflows
    insert into [dbo].[workflow]
        (
           [workflow_name]
          ,[description]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[site_id]
          ,[create_default_notification]
          ,[apply_by_default]
        )
    SELECT [WORKFLOW_NAME]
          ,w.[DESCRIPTION]
          ,@todaysDate
          ,@todaysDate
          ,w.[LAST_MODIFIED_BY]
          ,@newsiteid
          ,[create_default_notification]
          ,[apply_by_default]
      FROM [dbo].[workflow] as w
        inner join [dbo].[SITE] as s
            on w.SITE_ID = s.SITE_ID and s.SITE_ID = @oldsiteid
    
    --copying workflow rules	
    ;with relations_between_workflows
    as 
    (
        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                ,w2.WORKFLOW_ID as new_workflow_id
        FROM [dbo].[workflow] as w1
            inner join [dbo].[workflow] as w2 
                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @newsiteid
        where w1.SITE_ID = @oldsiteid
    )	
    insert into [dbo].[workflow_rules] 
    select [user_id]
          ,[group_id]
          ,[rule_order]
          ,[predecessor_permission_id]
          ,[successor_permission_id]
          ,[successor_status_id]
          ,[comment]
          ,rbw.new_workflow_id
      from [dbo].[workflow_rules] as wr
        inner join relations_between_workflows as rbw
            on wr.WORKFLOW_ID = rbw.old_workflow_id
            
    
    declare @relations_between_folders table(
        old_folder_id int,
        new_folder_id int
    )
    
    --copying folders		
    merge into [dbo].[folder]
    using (
    select @newsiteid
      ,[folder_id]
      ,[parent_folder_id]
      ,[name]
      ,[description]
      ,[filter]
      ,[path]
      ,@todaysDate
      ,@todaysDate
      ,[last_modified_by]
    from [dbo].[folder]
    where SITE_ID = @oldsiteid)
    as src (site_id,[folder_id],[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    on 0 = 1
    when not matched then
    insert (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    values (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
    output src.[folder_id], inserted.[folder_id]
        into @relations_between_folders;
        
    update [dbo].[folder]
    set [parent_folder_id] = rbf.new_folder_id
    from [dbo].[folder] as f
        inner join @relations_between_folders as rbf
            on f.[parent_folder_id] = rbf.old_folder_id
    where f.SITE_ID = @newsiteid

    -- копирование команд
     delete from [dbo].[VE_COMMAND_SITE_BIND]
     where site_id = @newSiteId

     insert into [dbo].[VE_COMMAND_SITE_BIND]
     SELECT command_id 
            ,@newSiteId
          ,[ON]
      FROM [dbo].[VE_COMMAND_SITE_BIND] (nolock)
      where site_id = @oldSiteId

    -- копирование стилей
     delete from [dbo].[VE_STYLE_SITE_BIND]
     where site_id = @newSiteId

     insert into [dbo].[VE_STYLE_SITE_BIND]
     SELECT style_id
            ,@newSiteId
            ,[ON]
      FROM [dbo].[VE_STYLE_SITE_BIND] (nolock)
      where site_id = @oldSiteId
        
END
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.48', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.48 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.49
-- fix for m2m fields
-- **************************************
ALTER FUNCTION [dbo].[GetRelationsBetweenContentLinks]
(
    @sourceSiteId int,
    @destinationSourceId int
)
RETURNS 
@relations_between_contents_links TABLE 
(
    oldvalue int,
    newvalue int
)
AS
BEGIN
    insert into @relations_between_contents_links
    select distinct oldvalues.link_id, newvalues.link_id from (
    select attribute_name, link_id, c.content_name
      from [dbo].[content_attribute] as ca
      inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
      inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation' 
      where c.site_id = @sourceSiteId and link_id is not null) as oldvalues
    inner join (
        select attribute_name, link_id, c.content_name
          from [dbo].[content_attribute] as ca
          inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0	
          inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation' 
          where c.site_id = @destinationSourceId and link_id is not null)
          as newvalues 
          on newvalues.attribute_name = oldvalues.attribute_name and newvalues.content_name = oldvalues.content_name
    return 
END
go

ALTER TRIGGER [dbo].[ti_item_to_item] ON [dbo].[item_to_item] AFTER INSERT
AS 
BEGIN

if object_id('tempdb..#disable_ti_item_to_item') is null
    begin
        with items (link_id, item_id, linked_item_id)
        AS
        (
            select i1.link_id, i1.l_item_id, i1.r_item_id From inserted i1
            inner join content_to_content c2c on i1.link_id = c2c.link_id 
            where c2c.[symmetric] = 1 and not exists (select * from item_to_item i2 where i1.link_id = i2.link_id and i1.r_item_id = i2.l_item_id and i2.r_item_id = i1.l_item_id)
        )
        insert into item_to_item(link_id, l_item_id, r_item_id)
        select link_id, linked_item_id, item_id from items
    end
END
go


ALTER procedure [dbo].[qp_copy_site_update_links]
    @xmlparams xml,
    @sourceSiteId int,
    @destinationSiteId int
as
begin

    set nocount on;


    declare @linksrel table(
        olditemid numeric,
        newitemid numeric
    )
    
    insert into @linksrel
    select doc.col.value('./@oldId', 'int') olditemid
             ,doc.col.value('./@newId', 'int') newitemid
            from @xmlparams.nodes('/item') doc(col)
            
    --updating o2m values
    update [dbo].[content_data] 
        set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        inner join CONTENT_ATTRIBUTE as ca (nolock) 
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock) 
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join [dbo].[CONTENT_ITEM] as ci (nolock) 
            on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
        inner join [dbo].[CONTENT] as c (nolock) 
            on c.CONTENT_ID = ci.CONTENT_ID
        inner join @linksrel lr 
            on cast(lr.olditemid as varchar) = cd.DATA
        where c.SITE_ID = @destinationSiteId
    
    --updating link values

    update [dbo].[content_data] 
        set data = (case when lc.newvalue is not null then lc.newvalue else cd.DATA end)
        from [dbo].[content_data] cd (nolock)
        left join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) lc 
            on  cast(lc.oldvalue as varchar) = cd.DATA
        inner join CONTENT_ATTRIBUTE as ca (nolock)
            on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join ATTRIBUTE_TYPE as at (nolock)
            on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
        inner join @linksrel as lr1 
            on lr1.newitemid = cd.content_item_id

    --inserting relations between new items

    select 1 as A into #disable_ti_item_to_item

    insert into [dbo].[item_to_item]
    select r.newvalue, i1.l_item_id, i1.r_item_id 
    from [dbo].[item_to_item] as i1 (nolock)
    inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
        on r.oldvalue = i1.link_id
    where i1.l_item_id in (select olditemid from @linksrel) or i1.r_item_id in (select olditemid from @linksrel)

    update [dbo].[item_to_item]
    set l_item_id = lr.newitemid
    from [dbo].[item_to_item] as ii
        inner join @linksrel as lr on
            ii.l_item_id = lr.olditemid
        inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
            on r.newvalue = ii.link_id
    where ii.l_item_id in (select olditemid from @linksrel) 

    update [dbo].[item_to_item]
    set r_item_id = lr.newitemid
    from [dbo].[item_to_item] as ii
        inner join @linksrel as lr on
            ii.r_item_id = lr.olditemid
        inner join (select oldvalue, newvalue from [dbo].GetRelationsBetweenContentLinks(@sourceSiteId, @destinationSiteId)) as r 
            on r.newvalue = ii.link_id

    SELECT COUNT(*) from @linksrel
end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.49', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.49 completed'
GO



-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.50
-- adding option to copy only contents with n articles
-- **************************************
ALTER procedure [dbo].[qp_copy_site_articles] 
        @oldsiteid int,
        @newsiteid int,
        @contentIdsToCopy nvarchar(max),
        @startfrom int,
        @endon int
as
begin

    set nocount on;
    select 1 as A into #disable_tu_update_child_content_data;

    DECLARE @not_for_replication bit
    SET @not_for_replication = 1

    declare @relsattrs table(
            attr_old numeric(18,0)
            ,attr_new numeric(18,0)
            ,primary key (attr_old, attr_new)
    )


    declare @relations_between_statuses table(
        old_status_type_id numeric(18,0),
        new_status_type_id numeric(18,0)
    )
    insert into @relations_between_statuses
                    select st1.STATUS_TYPE_ID as old_status_type_id
                        ,st2.STATUS_TYPE_ID as new_status_type_id 
                from [dbo].[status_type] as st1 (NOLOCK)
                inner join [dbo].[status_type] as st2 (NOLOCK) 
                    on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                where st1.SITE_ID = @oldSiteId

    declare @relations_between_contents table(
        source_content_id numeric(18,0),
        destination_content_id numeric(18,0)
    )
    insert into @relations_between_contents
        select c.content_id as source_content_id, 
                nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0


    declare @contentitemstable table( 
            newcontentitemid int ,
            contentid int,
            oldcontentitemid int,
            primary key ( newcontentitemid, oldcontentitemid, contentid));

    declare @isVirtual bit = 0
       
    declare @todaysDateTime datetime
    set @todaysDateTime = GetDate();


    insert into @relsattrs
        select ca.attribute_id as cat_old
            ,ca1.attribute_id as cat_new 
        from [dbo].content_attribute as ca (NOLOCK)
        inner join (
                    select c.content_id as source_content_id, nc.content_id as destination_content_id 
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock) 
                            on nc.content_name = c.content_name and nc.site_id = @newSiteId
                        where c.site_id = @oldSiteId and c.virtual_type = 0
        ) as ra on ra.source_content_id = ca.content_id
        left join [dbo].content_attribute as ca1 (NOLOCK) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id

    ;with content_items as (
        select [content_item_id]
              ,[visible]
              ,rbs.new_status_type_id as status_type_id
              ,@todaysDateTime as created
              ,@todaysDateTime as modified
              ,rc.destination_content_id as content_id
              ,c1.[last_modified_by]
              ,[locked_by]
              ,[archive]
              ,@not_for_replication as [not_for_replication]
              ,[schedule_new_version_publication]
              ,[splitted]
              ,[cancel_split]
         from (
            select row_number() over (order by content_item_id) as rownumber
                ,content_item_id
                  ,[visible]
                  ,[status_type_id]
                  ,c2.[created]
                  ,c2.[modified]
                  ,c2.[content_id]
                  ,c2.[last_modified_by]
                  ,[locked_by]
                  ,[archive]
                  ,[not_for_replication]
                  ,[schedule_new_version_publication]
                  ,[splitted]
                  ,[cancel_split]
            from [dbo].[content_item] (NOLOCK) as c2
            inner join [dbo].[content] as c (NOLOCK) on c.content_id = c2.content_id and c.site_id = @oldsiteid 
                                                        and c2.CONTENT_ID  in (SELECT convert(numeric, nstr) from dbo.splitNew(@contentIdsToCopy, ','))
        )  as c1
            inner join @relations_between_contents as rc on rc.source_content_id = c1.content_id 
            inner join @relations_between_statuses as rbs on c1.[status_type_id] = rbs.old_status_type_id
        where c1.rownumber between @startfrom and @endon
    )
    merge [dbo].[content_item]
    using(
        select content_item_id
          ,[visible]
          ,[status_type_id]
          ,[created]
          ,[modified]
          ,[content_id]
          ,[last_modified_by]
          ,[locked_by]
          ,[archive]
          ,[not_for_replication]
          ,[schedule_new_version_publication]
          ,[splitted]
          ,[cancel_split]
        from content_items as t
    ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
    on 0 = 1
    when not matched then
        insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
        output inserted.content_item_id, inserted.content_id, src.content_item_id
            into @contentitemstable;			
                    
    insert into [dbo].[CONTENT_ITEM_SCHEDULE](
          [content_item_id]
          ,[maximum_occurences]
          ,[created]
          ,[modified]
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,[use_service]
        )
        SELECT 
            cist.newcontentitemid
          ,[maximum_occurences]
          ,@todaysDateTime
          ,@todaysDateTime
          ,[last_modified_by]
          ,[freq_type]
          ,[freq_interval]
          ,[freq_subday_type]
          ,[freq_subday_interval]
          ,[freq_relative_interval]
          ,[freq_recurrence_factor]
          ,[active_start_date]
          ,[active_end_date]
          ,[active_start_time]
          ,[active_end_time]
          ,[occurences]
          ,[use_duration]
          ,[duration]
          ,[duration_units]
          ,[deactivate]
          ,[delete_job]
          ,1 as[use_service]
          FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis (NOLOCK)
            inner join @contentitemstable as cist
                on cis.CONTENT_ITEM_ID = cist.oldcontentitemid;

    update copydata
    set [data] =(	case	when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.attr_new as varchar)) 
                            when at.[TYPE_NAME] = 'Relation Many-to-One' and cd.DATA is not null then CAST(ra1.attr_new as varchar)
                            else cd.data
                    end)
      ,[blob_data] = cd.BLOB_DATA
    from [dbo].[content_data] as copydata (NOLOCK)
        inner join @relsattrs as ra on ra.attr_new = copydata.ATTRIBUTE_ID
        inner join @contentitemstable as cit on cit.newcontentitemid = copydata.CONTENT_ITEM_ID
        inner join [dbo].[content_data] as cd (NOLOCK) on
            cd.ATTRIBUTE_ID = ra.attr_old and cd.CONTENT_ITEM_ID = cit.oldcontentitemid
        inner join [dbo].[CONTENT_ATTRIBUTE] as ca (NOLOCK)
            on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        inner join [dbo].[attribute_type] as at (NOLOCK)
            on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID
        left join @relsattrs ra1 on CAST(ra1.attr_old as nvarchar) = cd.DATA;

    declare @ids varchar(max)
    select @ids = COALESCE(@ids + ', ', '') + CAST(newcontentitemid as nvarchar) from @contentitemstable
    if @ids is not null
        exec qp_replicate_items @ids 

    select newcontentitemid, oldcontentitemid from @contentitemstable

    delete from @contentitemstable
    delete from @relsattrs

end
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.50', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.50 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.51
-- Additional Context Menu Items
-- **************************************

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('site'), dbo.qp_action_id('new_content'), 'New Content', 15, 'add.gif', 0)

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('site'), dbo.qp_action_id('new_virtual_content'), 'New Virtual Content', 17, 'add.gif', 1)

update CONTEXT_MENU_ITEM set [Order] = [Order] * 10 where [Order] < 10

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('template'), dbo.qp_action_id('new_template_object'), 'New Object', 10, 'add.gif', 0)

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('template'), dbo.qp_action_id('new_page'), 'New Page', 15, 'add.gif', 1)

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('page'), dbo.qp_action_id('new_page_object'), 'New Object', 10, 'add.gif', 1)


insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('content_group'), dbo.qp_action_id('new_content'), 'New Content', 15, 'add.gif', 0)

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('content_group'), dbo.qp_action_id('new_virtual_content'), 'New Virtual Content', 17, 'add.gif', 1)

insert into BACKEND_ACTION(type_id, ENTITY_TYPE_ID, name, code, CONTROLLER_ACTION_URL, IS_INTERFACE)
values (dbo.qp_action_type_id('new'), dbo.qp_entity_type_id('field'), 'New Adjacent Field', 'new_adjacent_field', '~/Field/New/', 1)

insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, name, [order], icon, BOTTOM_SEPARATOR)
values(dbo.qp_context_menu_id('field'), dbo.qp_action_id('new_adjacent_field'), 'New Adjacent Field', 45, 'add.gif', 1)

exec qp_update_translations 'New Adjacent Field', 'Новое смежное поле'

if not exists (select * from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('new_adjacent_field'))
insert into ACTION_TOOLBAR_BUTTON
select dbo.qp_action_id('new_adjacent_field'), action_id, name, [order], icon, ICON_DISABLED, IS_COMMAND 
from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('new_field')

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.51', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.51 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.52
-- New options for relation fields
-- **************************************
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'ORDER_BY_TITLE') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD ORDER_BY_TITLE BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_ORDER_BY_TITLE DEFAULT 0  
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'FIELD_TITLE_COUNT') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD FIELD_TITLE_COUNT INT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_FIELD_TITLE_COUNT DEFAULT 1  
END
GO

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CONTENT_ATTRIBUTE' AND COLUMN_NAME = 'INCLUDE_RELATIONS_IN_TITLE') 
BEGIN
    ALTER TABLE CONTENT_ATTRIBUTE ADD INCLUDE_RELATIONS_IN_TITLE BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_INCLUDE_RELATIONS_IN_TITLE DEFAULT 0  
END
GO

ALTER FUNCTION [dbo].[qp_get_display_fields] 
(	
  @content_id numeric(18,0), 
  @with_relation_field BIT = 0
)
RETURNS TABLE 
AS
RETURN 
(
    select * from
    (
        SELECT  ATTRIBUTE_ID, attribute_name,
          CASE when attribute_type_id in (9, 10) THEN 0
            WHEN attribute_type_id = 13 THEN -1
            WHEN attribute_type_id = 11 AND (@with_relation_field = 0 OR @with_relation_field = 1 AND (LINK_ID IS NOT NULL OR EXISTS(select * from content_attribute ca1 where ca1.BACK_RELATED_ATTRIBUTE_ID = ca.ATTRIBUTE_ID))) THEN -1
            ELSE 1
          END AS attribute_priority,
          view_in_list,
          attribute_order
        FROM content_attribute ca
        WHERE content_id = @content_id
    ) as c
    where attribute_priority >= 0 
)
GO

ALTER FUNCTION [dbo].[qp_get_display_field](@content_id NVARCHAR(255), @with_relation_field BIT = 0) RETURNS NVARCHAR(255)
AS BEGIN
    DECLARE @fld_name NVARCHAR(255)

    SELECT @fld_name = attribute_name FROM (
    SELECT  top 1 attribute_name from [dbo].[qp_get_display_fields](@content_id,  @with_relation_field)
    ORDER BY view_in_list desc, attribute_priority desc, attribute_order asc) AS a

    IF @fld_name is Null
        Set @fld_name = 'content_item_id'
    RETURN @fld_name
END
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.52', 'Copyright &copy; 1998-2013 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.52 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.53
-- Fix access checking to aggregated contents
-- **************************************

ALTER Function [dbo].[qp_is_entity_accessible](
  @entity_name varchar(100)='content',
  @entity_id numeric (18,0),
  @user_id numeric (18,0)=0,
  @group_id numeric (18,0)=0,
  @start_level int=1,
  @end_level int=4,  
  @return_level int=0
)
Returns int
AS 

BEGIN

Declare @FullAccessLevel int
SET @FullAccessLevel = 4

if @user_id = 1 or @group_id = 1 return @FullAccessLevel

if @user_id > 0 and dbo.qp_is_user_admin(@user_id)>0 return @FullAccessLevel


/***********************************/
/**** Declare Table Variables   ****/
/***********************************/
declare @ChildGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 

declare @ParentGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 

declare @UsedGroups table 
( 
    group_id numeric(18,0)
) 

declare @TempParentGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 

declare @Entities table 
( 
    entity_id numeric(18,0) NOT NULL,
    permission_level numeric(18,0) NOT NULL,
    user_id numeric(18,0) NULL,
    group_id numeric(18,0) NULL
) 
/***********************************/

declare @content_id decimal

  If @entity_name='content' 
      Begin
         
         declare @aggr_base_content_id decimal
         
         select @aggr_base_content_id = ca2.CONTENT_ID From content_attribute ca1 
            inner join CONTENT_ATTRIBUTE ca2 on ca1.CLASSIFIER_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID
            where ca1.CONTENT_ID = @entity_id

        if (@aggr_base_content_id is not null)
            set @entity_id = @aggr_base_content_id
     
         insert into @Entities (entity_id, permission_level, user_id, group_id)
           select content_id, permission_level, user_id, group_id from content_access_permlevel
           where content_id = @entity_id
      End
  If @entity_name='content_item'
      Begin
         declare @use_own_security bit
         select @use_own_security = c.allow_items_permission, @content_id = ci.content_id 
            from content c with(nolock) inner join content_item ci with(nolock) on c.content_id = ci.content_id where ci.content_item_id = @entity_id
         if (@use_own_security = 1)
            insert into @Entities (entity_id, permission_level, user_id, group_id)
                select content_item_id, permission_level, user_id, group_id from content_item_access_permlevel
                where content_item_id = @entity_id
        else
            insert into @Entities (entity_id, permission_level, user_id, group_id)
                select @entity_id, permission_level, user_id, group_id from content_access_permlevel
                where content_id = @content_id		
      End
  If @entity_name='site'
      Begin
         insert into @Entities (entity_id, permission_level, user_id, group_id)
           select site_id, permission_level, user_id, group_id from site_access_permlevel
           where site_id = @entity_id
      End
  If @entity_name='folder'
      Begin
         insert into @Entities (entity_id, permission_level, user_id, group_id)
           select folder_id, permission_level, user_id, group_id from folder_access_permlevel
           where folder_id = @entity_id
      End
  If @entity_name='content_folder'
      Begin
        select @content_id = content_id from content_folder with(nolock) where folder_id = @entity_id
        
        insert into @Entities (entity_id, permission_level, user_id, group_id)
            select @entity_id, permission_level, user_id, group_id from content_access_permlevel
            where content_id = @content_id	

      End
  If @entity_name='workflow'
      Begin
         insert into @Entities (entity_id, permission_level, user_id, group_id)
           select workflow_id, permission_level, user_id, group_id from workflow_access_permlevel
           where workflow_id = @entity_id
      End
  If @entity_name='tab'
      Begin
         insert into @Entities (entity_id, permission_level, user_id, group_id)
           select tab_id, permission_level, user_id, group_id from tab_access_permlevel
           where tab_id = @entity_id
      End


Declare @maxLevel int
Declare @nothing_found int
Declare @yes_access int
Declare @no_access int
Declare @current_result int

select @yes_access = 1
select @no_access = 0
select @nothing_found = -1
select @current_result = @nothing_found

if @user_id > 0
Begin
   select @maxLevel = IsNull(max(permission_level),@nothing_found) from @Entities where
       user_id = @user_id

   Select @current_result = @maxLevel   

   if @maxLevel != @nothing_found
   Begin
      if @return_level>0 return @maxLevel
      if @maxLevel < @start_level or @maxLevel> @end_level return @no_access
      if @maxLevel >= @start_level And @maxLevel <= @end_level return @yes_access
   End
   
   insert into @ChildGroups (group_id) select distinct group_id from user_group_bind where user_id = @user_id   
End

if @group_id > 0 AND @user_id <= 0
Begin
   insert into @ChildGroups(group_id) values (@group_id)
End

if (select count(*) from @ChildGroups) = 0
Begin
   return @current_result
End 

select @maxLevel = IsNull(max(permission_level),@nothing_found) from @Entities where
       group_id in (select group_id from @ChildGroups)

Select @current_result = @maxLevel  

if @maxLevel != @nothing_found
Begin
  if @return_level>0 return @maxLevel
  if @maxLevel < @start_level or @maxLevel> @end_level return @no_access
  if @maxLevel >= @start_level And @maxLevel <= @end_level return @yes_access
End

insert into @UsedGroups(group_id) select group_id from @ChildGroups


WHILE 1=1
BEGIN
    insert into @ParentGroups (group_id) select distinct gtg.parent_group_id from group_to_group gtg inner join @ChildGroups cg on gtg.child_group_id = cg.group_id
    if (select count(*) from @ParentGroups) = 0 BREAK

    /* need to check that parent groups are not appearing in child groups */
    insert into @TempParentGroups (group_id) select pg.group_id from @ParentGroups pg where pg.group_id not in(select cg.group_id from @ChildGroups cg) and pg.group_id not in (select group_id from @UsedGroups)
    
    select @maxLevel = IsNull(max(permission_level),@nothing_found) from @Entities where
           group_id in (select group_id from @TempParentGroups) 

    Select @current_result = @maxLevel  

    if @maxLevel != @nothing_found
    Begin
      if @return_level>0 return @maxLevel
      if @maxLevel < @start_level or @maxLevel> @end_level return @no_access
      if @maxLevel >= @start_level And @maxLevel <= @end_level return @yes_access
    End

    delete @ChildGroups
    delete @TempParentGroups
    insert into @ChildGroups (group_id) select (group_id) from @ParentGroups
    delete @ParentGroups
    CONTINUE
END

if @entity_name = 'folder' and @current_result = @nothing_found
begin
    declare @parentFolderId numeric, @siteId numeric
    select @parentFolderId = PARENT_FOLDER_ID, @siteId = site_id from FOLDER where FOLDER_ID = @entity_id
    if @parentFolderId is not null
        set @current_result = dbo.qp_is_entity_accessible('folder', @parentFolderId, @user_id, @group_id, @start_level, @end_level, @return_level)
    else
        set @current_result = dbo.qp_is_entity_accessible('site', @siteId, @user_id, @group_id, @start_level, @end_level, @return_level)

end

return @current_result
END

GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.53', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.53 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.54
-- DB Cleaning
-- **************************************

exec qp_drop_existing 'content_attribute_delete_version', 'IsTrigger'
exec qp_drop_existing '_temp_save_item_to_item', 'IsUserTable'
exec qp_drop_existing '_temp_save_content_to_content', 'IsUserTable'
exec qp_drop_existing 'SCHEDULE_DAILY', 'IsUserTable'
exec qp_drop_existing 'SCHEDULE_MONTHLY', 'IsUserTable'
exec qp_drop_existing 'SCHEDULE_WEEKLY', 'IsUserTable'
exec qp_drop_existing 'SCHEDULE_TYPE', 'IsUserTable'
exec qp_drop_existing 'VE_URL', 'IsUserTable'
exec qp_drop_existing 'DOC', 'IsUserTable'
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.54', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.54 completed'
GO


-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.55
-- Cleaning stored proc
-- **************************************

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_articles') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_articles
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_contents') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_contents
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_contents_update') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_contents_update
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_copy_contents_attributes') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_copy_contents_attributes
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_settings') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_settings
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_templates') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_templates
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_update_attributes') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_update_attributes
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_update_links') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_update_links
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.qp_copy_site_virtual_contents') AND type in (N'P', N'PC'))
    DROP PROCEDURE dbo.qp_copy_site_virtual_contents
GO

IF EXISTS (SELECT * FROM   sys.objects WHERE  object_id = OBJECT_ID(N'dbo.GetRelationsBetweenContentLinks') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
    DROP FUNCTION dbo.GetRelationsBetweenContentLinks
GO

IF EXISTS (SELECT * FROM   sys.objects WHERE  object_id = OBJECT_ID(N'dbo.GetRelationsBetweenContents') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
    DROP FUNCTION dbo.GetRelationsBetweenContents
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.55', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.55 completed'
GO

-- ************************************** 
-- Alexei Aksenov
-- version 7.9.7.56
-- Added possibility to disable insert access content_item
-- **************************************

ALTER TRIGGER [dbo].[ti_access_content_item] ON [dbo].[CONTENT_ITEM] FOR INSERT
AS
    if object_id('tempdb..#disable_ti_access_content_item') is null
    begin

      declare @ids table
      (
        content_item_id numeric primary key,
        last_modified_by numeric not null,
        content_id numeric not null
      )

      insert into @ids (content_item_id, last_modified_by, content_id)
      select content_item_id, i.last_modified_by, i.content_id from inserted i 
      inner join content c on i.CONTENT_ID = c.CONTENT_ID
      where c.allow_items_permission = 1

      INSERT INTO content_item_access 
        (content_item_id, user_id, permission_level_id, last_modified_by)
      SELECT
        content_item_id, last_modified_by, 1, 1
      FROM @ids i
      WHERE i.LAST_MODIFIED_BY <> 1

      INSERT INTO content_item_access 
        (content_item_id, user_id, group_id, permission_level_id, last_modified_by)
      SELECT
        i.content_item_id, ca.user_id, ca.group_id, ca.permission_level_id, 1 
      FROM content_access AS ca
        INNER JOIN @ids AS i ON ca.content_id = i.content_id
        LEFT OUTER JOIN user_group AS g ON g.group_id = ca.group_id
      WHERE
        (ca.user_id <> i.last_modified_by or ca.user_id IS NULL)
        AND ((g.shared_content_items = 0 and g.GROUP_ID <> 1) OR g.group_id IS NULL)
        AND ca.propagate_to_items = 1

      INSERT INTO content_item_access 
        (content_item_id, group_id, permission_level_id, last_modified_by)
      SELECT DISTINCT
        i.content_item_id, g.group_id, 1, 1
      FROM @ids AS i
        LEFT OUTER JOIN user_group_bind AS gb ON gb.user_id = i.last_modified_by
        LEFT OUTER JOIN user_group AS g ON g.group_id = gb.group_id
      WHERE
        g.shared_content_items = 1 and g.GROUP_ID <> 1

    end
GO

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.56', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.56 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.57
-- Remove unused field and trigger
-- **************************************

if exists(select * from sys.foreign_keys where name = 'FK_CONTENT_DATA_PARENT_CONTENT_DATA_ID')
alter table content_data drop FK_CONTENT_DATA_PARENT_CONTENT_DATA_ID

if exists(select * from sys.columns where name = 'parent_content_data_id' and [object_id] = object_id('content_data'))
alter table content_data drop column parent_content_data_id

if exists(select * from sys.triggers where name = 'td_delete_child_content_data')
drop trigger td_delete_child_content_data

if exists(select * from sys.triggers where name = 'tu_update_child_content_data')
drop trigger tu_update_child_content_data

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.57', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.57 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.58
-- Disable part of trigger 
-- **************************************

ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30)
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id) from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
        
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids) and object_id('tempdb..#disable_td_delete_item_o2m_nullify') is null
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)				
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END
GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.58', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.58 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.7.59
-- Fast clearing relations for content 
-- **************************************

exec qp_drop_existing 'qp_clear_relations', 'IsProcedure'
GO

CREATE PROCEDURE dbo.qp_clear_relations(@parent_id numeric)
AS
BEGIN

    declare @attr_ids table
    (
        id numeric primary key,
        content_id numeric,
        name nvarchar(255)
    )
    
    declare @id numeric, @content_id numeric, @name nvarchar(255), @table_name nvarchar(255)
    declare @set nvarchar(max), @sql nvarchar(max)

    insert into @attr_ids
    select ca.attribute_id, ca.content_id, ca.attribute_name from CONTENT_ATTRIBUTE ca 
    inner join content_attribute ca0 on ca.RELATED_ATTRIBUTE_ID = ca0.ATTRIBUTE_ID 
    where ca0.CONTENT_ID = @parent_id and ca.CONTENT_ID <> @parent_id

    select 1 as A into #disable_tiu_content_fill
    
    if exists(select id from @attr_ids)
    begin
        UPDATE content_attribute SET default_value = null 
            WHERE attribute_id IN (select id from @attr_ids) 
    
        UPDATE content_data SET data = NULL, blob_data = NULL 
            WHERE attribute_id IN (select id from @attr_ids) and DATA IS NOT NULL
            
        DELETE from VERSION_CONTENT_DATA
            where ATTRIBUTE_ID in (select id from @attr_ids)
    end

    while exists(select id from @attr_ids)
    begin
        select @id = id, @content_id = content_id, @name = name from @attr_ids
        set @table_name = 'content_' + cast(@content_id as nvarchar(20))
        set @set = ' set [' + @name + '] = NULL where [' + @name + '] is not null '
        set @sql = 'update ' + @table_name + @set
        exec sp_executesql @sql
        print @sql
        set @table_name = @table_name + '_async'
        set @sql = 'update ' + @table_name + @set				
        exec sp_executesql @sql
        print @sql
        delete from @attr_ids where id = @id
    end

END

GO
INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.7.59', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.7.59 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.9.0
-- Release
-- **************************************

ALTER TRIGGER [dbo].[tbd_user] ON [dbo].[USERS] 
INSTEAD OF DELETE
AS
BEGIN
    
    DELETE USER_GROUP_BIND FROM USER_GROUP_BIND c inner join deleted d on c.user_id = d.user_id
    DELETE USER_DEFAULT_FILTER FROM USER_DEFAULT_FILTER f inner join deleted d on f.user_id = d.user_id
         
    UPDATE CONTAINER SET locked = NULL, locked_by = NULL FROM CONTAINER c inner join deleted d on c.locked_by = d.user_id  
    UPDATE CONTENT_FORM SET locked = NULL, locked_by = NULL FROM CONTENT_FORM c inner join deleted d on c.locked_by = d.user_id  
    UPDATE CONTENT_ITEM SET locked = NULL, locked_by = NULL FROM CONTENT_ITEM c inner join deleted d on c.locked_by = d.user_id  
    UPDATE [OBJECT] SET locked = NULL, locked_by = NULL FROM [OBJECT] c inner join deleted d on c.locked_by = d.user_id  
    UPDATE OBJECT_FORMAT SET locked = NULL, locked_by = NULL FROM OBJECT_FORMAT c inner join deleted d on c.locked_by = d.user_id  
    UPDATE PAGE SET locked = NULL, locked_by = NULL FROM PAGE c inner join deleted d on c.locked_by = d.user_id  
    UPDATE PAGE_TEMPLATE SET locked = NULL, locked_by = NULL FROM PAGE_TEMPLATE c inner join deleted d on c.locked_by = d.user_id  
    UPDATE [SITE] SET locked = NULL, locked_by = NULL FROM [SITE] c inner join deleted d on c.locked_by = d.user_id 
    
    UPDATE [SITE] SET last_modified_by = 1 FROM [SITE] c inner join deleted d on c.last_modified_by = d.user_id  

    UPDATE CONTENT SET last_modified_by = 1 FROM CONTENT c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_ITEM SET last_modified_by = 1 FROM CONTENT_ITEM c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_ITEM_SCHEDULE SET last_modified_by = 1 FROM CONTENT_ITEM_SCHEDULE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_ITEM_VERSION SET created_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.created_by = d.user_id
    UPDATE CONTENT_ITEM_VERSION SET last_modified_by = 1 FROM CONTENT_ITEM_VERSION c inner join deleted d on c.last_modified_by = d.user_id
    
    UPDATE CONTENT_ATTRIBUTE SET last_modified_by = 1 FROM CONTENT_ATTRIBUTE c inner join deleted d on c.last_modified_by = d.user_id

    UPDATE PAGE_TEMPLATE SET last_modified_by = 1 FROM PAGE_TEMPLATE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE PAGE SET last_modified_by = 1 FROM PAGE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE PAGE SET last_assembled_by = 1 FROM PAGE c inner join deleted d on c.last_assembled_by  = d.user_id 
    UPDATE OBJECT SET last_modified_by = 1 FROM OBJECT c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE OBJECT_FORMAT SET last_modified_by = 1 FROM OBJECT_FORMAT c inner join deleted d on c.last_modified_by = d.user_id

    UPDATE FOLDER SET last_modified_by = 1 FROM FOLDER c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE FOLDER_ACCESS SET last_modified_by = 1 FROM FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_FOLDER SET last_modified_by = 1 FROM CONTENT_FOLDER c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_FOLDER_ACCESS SET last_modified_by = 1 FROM CONTENT_FOLDER_ACCESS c inner join deleted d on c.last_modified_by = d.user_id

    UPDATE CODE_SNIPPET SET last_modified_by = 1 FROM CODE_SNIPPET c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE STYLE SET last_modified_by = 1 FROM STYLE c inner join deleted d on c.last_modified_by = d.user_id
    
    UPDATE STATUS_TYPE SET last_modified_by = 1 FROM STATUS_TYPE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE WORKFLOW SET last_modified_by = 1 FROM WORKFLOW c inner join deleted d on c.last_modified_by = d.user_id
    
    UPDATE SITE_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE CONTENT_ITEM_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE WORKFLOW_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id

    UPDATE USER_GROUP SET last_modified_by = 1 FROM USER_GROUP c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE USERS SET last_modified_by = 1 FROM USERS c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE NOTIFICATIONS SET last_modified_by = 1 FROM NOTIFICATIONS c inner join deleted d on c.last_modified_by = d.user_id
    
    UPDATE CONTENT_ITEM_STATUS_HISTORY SET user_id = 1 WHERE user_id in (select user_id from deleted)

    UPDATE CUSTOM_ACTION SET LAST_MODIFIED_BY = 1 FROM CUSTOM_ACTION c INNER JOIN deleted d on c.LAST_MODIFIED_BY = d.[USER_ID]
    
    UPDATE NOTIFICATIONS SET FROM_BACKENDUSER_ID = 1 FROM NOTIFICATIONS c inner join deleted d on c.FROM_BACKENDUSER_ID = d.user_id
    
    UPDATE ENTITY_TYPE_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id
    UPDATE ACTION_ACCESS SET last_modified_by = 1 FROM SITE c inner join deleted d on c.last_modified_by = d.user_id
    
    delete users from users c inner join deleted d on c.user_id = d.user_id
END
GO

if not exists(select * from ACTION_TYPE where code = 'multiple_update')
insert into ACTION_TYPE VALUES('Multiple Update', 'multiple_update', 2, 255)

if not exists (select * from BACKEND_ACTION where code = 'multiple_publish_articles')
insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, CODE, CONTROLLER_ACTION_URL, CONFIRM_PHRASE, HAS_PRE_ACTION)
VALUES(dbo.qp_action_type_id('multiple_update'), dbo.qp_entity_type_id('article'), 'Mulitple Publish Articles', 'multiple_publish_articles', '~/Article/MultiplePublish/', 'Do you really want to publish the following articles: {0}?', 1)

if not exists (select * from ACTION_TOOLBAR_BUTTON where ACTION_ID = dbo.qp_action_id('multiple_publish_articles'))
insert into ACTION_TOOLBAR_BUTTON(PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_action_id('list_article'), dbo.qp_action_id('multiple_publish_articles'), 'Publish', 35, 'assemble.gif')

exec qp_update_translations 'Mulitple Publish Articles', 'Множественная публикация статей'
exec qp_update_translations 'Publish', 'Опубликовать'

update BACKEND_ACTION set allow_search = 1 where code = 'list_archive_article'
GO

ALTER PROCEDURE [dbo].[qp_update_m2m]
@id numeric,
@linkId numeric,
@value nvarchar(max),
@splitted bit = 0,
@update_archive bit = 1
AS
BEGIN
    declare @newIds table (id numeric primary key, attribute_id numeric null, has_data bit, splitted bit, has_async bit null)
    declare @ids table (id numeric primary key)
    declare @crossIds table (id numeric primary key)

    insert into @newIds (id) select * from dbo.split(@value, ',')

    IF @splitted = 1
        insert into @ids select linked_item_id from item_link_async where link_id = @linkId and item_id = @id
    ELSE
        insert into @ids select linked_item_id from item_link where link_id = @linkId and item_id = @id

    insert into @crossIds select t1.id from @ids t1 inner join @newIds t2 on t1.id = t2.id
    delete from @ids where id in (select id from @crossIds)
    delete from @newIds where id in (select id from @crossIds)

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    IF @splitted = 0
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id

    IF @splitted = 1
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)
    ELSE
        DELETE FROM item_link_united_full WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)

    IF @splitted = 1
        INSERT INTO item_link_async SELECT @linkId, @id, id from @newIds
    ELSE
        INSERT INTO item_to_item SELECT @linkId, @id, id from @newIds

    if dbo.qp_is_link_symmetric(@linkId) = 1
    begin
    
        with newItems (id, attribute_id, has_data, splitted, has_async) as
        (
        select 
            n.id, ca.attribute_id, 
            case when cd.content_item_id is null then 0 else 1 end as has_data, 
            ci.splitted, 
            case when ila.link_id is null then 0 else 1 end as has_async
        from @newIds n
            inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            inner join content c on ci.content_id = c.content_id
            inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = @linkId
            left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            left join item_link_async ila on @linkId = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @id
        )
        update @newIds 
        set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
        from @newIds n inner join newItems ext on n.id = ext.id
        
        if @splitted = 0
        begin
            update content_data set data = @linkId 
            from content_data cd 
            inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
            where n.has_data = 1
            
            insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
            select n.id, n.attribute_id, @linkId
            from @newIds n 
            where n.has_data = 0 and n.attribute_id is not null
            
            insert into item_link_async(link_id, item_id, linked_item_id)
            select @linkId, n.id, @id
            from @newIds n 
            where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
        end
    end
END
GO

ALTER PROCEDURE [dbo].[qp_update_m2o]
@id numeric,
@fieldId numeric,
@value nvarchar(max),
@update_archive bit = 1
AS
BEGIN
    declare @ids table (id numeric primary key)
    declare @new_ids table (id numeric primary key);
    declare @cross_ids table (id numeric primary key);
    
    declare @contentId numeric, @fieldName nvarchar(255)
    select @contentId = content_id, @fieldName = attribute_name from CONTENT_ATTRIBUTE where ATTRIBUTE_ID = @fieldId

    insert into @ids
    exec qp_get_m2o_ids @contentId, @fieldName, @id
    
    select content_item_id
    from content_data where ATTRIBUTE_ID = @fieldId and DATA = @id

    insert into @new_ids select * from dbo.split(@value, ',');

    insert into @cross_ids select t1.id from @ids t1 inner join @new_ids t2 on t1.id = t2.id
    delete from @ids where id in (select id from @cross_ids);
    delete from @new_ids where id in (select id from @cross_ids);

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    insert into #resultIds(id, attribute_id, to_remove)
    select id, @fieldId as attribute_id, 1 as to_remove from @ids
    union all
    select id, @fieldId as attribute_id, 0 as to_remove from @new_ids
END
GO

ALTER FUNCTION [dbo].[qp_get_display_fields] 
(	
  @content_id numeric(18,0), 
  @with_relation_field BIT = 0
)
RETURNS TABLE 
AS
RETURN 
(
    select * from
    (
        SELECT  ATTRIBUTE_ID, attribute_name,
          CASE WHEN attribute_type_id in (9, 10) THEN CASE WHEN @with_relation_field = 1 THEN 1 ELSE 0 END
            WHEN attribute_type_id = 13 THEN -1
            WHEN attribute_type_id = 11 AND (@with_relation_field = 0 OR @with_relation_field = 1 AND (LINK_ID IS NOT NULL OR EXISTS(select * from content_attribute ca1 where ca1.BACK_RELATED_ATTRIBUTE_ID = ca.ATTRIBUTE_ID))) THEN -1
            ELSE 1
          END AS attribute_priority,
          view_in_list,
          attribute_order
        FROM content_attribute ca
        WHERE content_id = @content_id
    ) as c
    where attribute_priority >= 0 
)
GO

ALTER PROCEDURE [dbo].[qp_get_article_title] 
@content_item_id numeric, 
@content_id numeric, 
@title nvarchar(255) output
AS
BEGIN

    declare @rel_name nvarchar(255), @rel_content_id numeric
    declare @rel_name2 nvarchar(255), @rel_content_id2 numeric
    declare @titleName NVARCHAR(255), @sql nvarchar(2000)
    ;with fields(name, rel_name, rel_content_id, rel_name2, rel_content_id2)
    as
    (
        select top 1 a.attribute_name as name, rca.ATTRIBUTE_NAME, rca.CONTENT_ID, rrca.ATTRIBUTE_NAME, rrca.CONTENT_ID from dbo.qp_get_display_fields(@content_id, 1) a
        inner join content_attribute ca on a.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
        left join content_attribute rca on ca.RELATED_ATTRIBUTE_ID = rca.ATTRIBUTE_ID
        left join content_attribute rrca on rca.RELATED_ATTRIBUTE_ID = rrca.ATTRIBUTE_ID
        order by a.attribute_order
    )
    select @titleName = name, @rel_name = rel_name, @rel_content_id = rel_content_id, @rel_name2 = rel_name2, @rel_content_id2 = rel_content_id2 from fields
    
    if @rel_name2 is not null
    begin
        SET @sql = 'SELECT @title = CAST([' + @rel_name2 + '] AS NVARCHAR (255)) FROM content_' + cast(@rel_content_id2 as varchar) + '_united' +
            ' WHERE content_item_id in (SELECT [' + @rel_name + '] FROM content_' + cast(@rel_content_id as varchar) + '_united' +
            ' WHERE content_item_id in (SELECT [' + @titleName + '] FROM content_' + cast(@content_id as varchar) + '_united' +
            ' WHERE content_item_id =' + cast(@content_item_id as varchar) +'))'

    end
    else if @rel_name is not null
    begin
        SET @sql = 'SELECT @title = CAST([' + @rel_name + '] AS NVARCHAR (255)) FROM content_' + cast(@rel_content_id as varchar) + '_united' +
            ' WHERE content_item_id in (SELECT [' + @titleName + '] FROM content_' + cast(@content_id as varchar) + '_united' +
            ' WHERE content_item_id =' + cast(@content_item_id as varchar) +')'
    end
    else
    begin
        SET @sql = 'SELECT @title = CAST([' + @titleName + '] AS NVARCHAR (255)) FROM content_' + cast(@content_id as varchar) + '_united' +
            ' WHERE content_item_id =' + cast(@content_item_id as varchar)
    end
    exec sp_executesql @sql, N'@title nvarchar(255) out', @title out
END
GO

if not exists (select * from BACKEND_ACTION where code = 'export_virtual_articles')
insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
values (dbo.qp_action_type_id('export'), dbo.qp_entity_type_id('virtual_content'), 'Export Virtual Articles', 'Export Articles', 'export_virtual_articles', '~/ExportArticles/', 1, 600, 400, 1, 1) 

if not exists (select * from CONTEXT_MENU_ITEM where name = 'Export Articles' and CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_content'))
insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
values (dbo.qp_context_menu_id('virtual_content'), dbo.qp_action_id('export_virtual_articles'), 'Export Articles', 40, 'other/export.gif')

if not exists (select * from BACKEND_ACTION where code = 'multiple_export_virtual_article')
insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, SHORT_NAME, CODE, CONTROLLER_ACTION_URL, IS_WINDOW, WINDOW_WIDTH, WINDOW_HEIGHT, IS_MULTISTEP, HAS_SETTINGS)
values (dbo.qp_action_type_id('multiple_export'), dbo.qp_entity_type_id('virtual_article'), 'Export Selected Articles', NULL, 'multiple_export_virtual_article', '~/ExportSelectedArticles/', 1, 600, 400, 1, 1) 

if not exists (select * from ACTION_TOOLBAR_BUTTON where PARENT_ACTION_ID = dbo.qp_action_id('list_virtual_article') and ACTION_ID = dbo.qp_action_id('multiple_export_virtual_article'))
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, NAME, [ORDER], ICON)
values(dbo.qp_action_id('list_virtual_article'), dbo.qp_action_id('multiple_export_virtual_article'), 'Export', 15, 'other/export.gif')

update CONTEXT_MENU_ITEM set ACTION_ID = dbo.qp_action_id('export_virtual_articles') where CONTEXT_MENU_ID = dbo.qp_context_menu_id('virtual_content') and ACTION_ID = dbo.qp_action_id('export_articles')

exec qp_drop_existing 'qp_update_m2m_values', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2m_values]
    @xmlParameter xml
AS
BEGIN
    DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
    INSERT INTO @fieldValues
    SELECT
        ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber 
         ,doc.col.value('./@id', 'int') id
         ,doc.col.value('./@linkId', 'int') linkId
         ,doc.col.value('./@value', 'nvarchar(max)') value
         ,c.SPLITTED as splitted
        FROM @xmlParameter.nodes('/items/item') doc(col)
        INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')


    declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
    declare @ids table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
    declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

    insert into @newIds (id, link_id, linked_item_id, splitted) 
    select a.id, a.linkId, b.nstr, a.splitted from @fieldValues a cross apply dbo.SplitNew(a.value, ',') b
    where b.nstr is not null and b.nstr <> '' and b.nstr <> '0'

    insert into @ids 
    select item_id, link_id, linked_item_id, f.splitted 
    from item_link_async ila inner join @fieldValues f 
    on ila.link_id = f.linkId and ila.item_id = f.id
    where f.splitted = 1
    union all
    select item_id, link_id, linked_item_id, f.splitted 
    from item_link il inner join @fieldValues f 
    on il.link_id = f.linkId and il.item_id = f.id
    where f.splitted = 0

    insert into @crossIds 
    select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @ids t1 inner join @newIds t2 
    on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

    delete @ids from @ids t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
    delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

    delete item_link_async from item_link_async il inner join @fieldValues f on il.item_id = f.id and il.link_id = f.linkId
    where f.splitted = 0 

    delete item_link_united_full from item_link_united_full il 
    where exists( 
        select * from @ids i where il.item_id = i.id and il.link_id = i.link_id
        and i.splitted = 0
    )

    delete item_link_async from item_link_united_full il 
    inner join @ids i on il.item_id = i.id and il.link_id = i.link_id
    where i.splitted = 1

    insert into item_link_async
    select link_id, id, linked_item_id from @newIds 
    where splitted = 1

    insert into item_to_item
    select link_id, id, linked_item_id from @newIds 
    where splitted = 0

    delete from @newIds where dbo.qp_is_link_symmetric(link_id) = 0;

    with newItems (id, link_id, linked_item_id, attribute_id, has_data, splitted, has_async) as
    (
    select 
        n.id, n.link_id, n.linked_item_id, ca.attribute_id, 
        case when cd.content_item_id is null then 0 else 1 end as has_data, 
        ci.splitted, 
        case when ila.link_id is null then 0 else 1 end as has_async
    from @newIds n
        inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
        inner join content c on ci.content_id = c.content_id
        inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
        left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
        left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.id = ila.linked_item_id
    )
    update @newIds 
    set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data, linked_splitted = ext.splitted, linked_has_async = ext.has_async
    from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

    update content_data set data = n.link_id 
    from content_data cd 
    inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
    where n.splitted = 0 and n.linked_has_data = 1

    insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
    select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
    from @newIds n 
    where n.splitted = 0 and n.linked_has_data = 0 and n.linked_attribute_id is not null
            
    insert into item_link_async(link_id, item_id, linked_item_id)
    select n.link_id, n.linked_item_id, n.id
    from @newIds n 
    where n.splitted = 0 and n.linked_splitted = 1 and n.linked_has_async = 0 and n.linked_attribute_id is not null
END
GO

ALTER PROCEDURE [dbo].[qp_get_paged_data]
    @select_block nvarchar(max),
    @from_block nvarchar(max),
    @where_block nvarchar(max) = '',
    @order_by_block nvarchar(max),
    @count_only bit = 0,
    @total_records int OUTPUT,
    @start_row bigint = 0,
    @page_size bigint = 0,
    
    @use_security bit = 0,
    @user_id numeric(18,0) = 0,
    @group_id numeric(18,0) = 0,
    @start_level int = 2,
    @end_level int = 4,
    @entity_name nvarchar(100),
    @parent_entity_name nvarchar(100) = '',
    @parent_entity_id numeric(18,0) = 0,
    @itemIds Ids READONLY,
    @insert_key varchar(200) = '<$_security_insert_$>',
    @separate_count_query bit = 0
AS
BEGIN
    SET NOCOUNT ON
    
    -- Получаем фильтр по правам
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    IF (@use_security = 1)
        BEGIN
            EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @user_id,
                @group_id = @group_id,
                @start_level = @start_level,
                @end_level = @end_level,
                @entity_name = @entity_name,
                @parent_entity_name = @parent_entity_name,
                @parent_entity_id = @parent_entity_id,				
                @SQLOut = @security_sql OUTPUT
                
            SET @from_block = REPLACE(@from_block, @insert_key, @security_sql)
        END
        
    -- Получаем общее количество записей
    DECLARE @sql_count AS nvarchar(max)
    
    if (@count_only = 1 or @separate_count_query = 1)
    BEGIN
        SET @sql_count = ''
        SET @sql_count = @sql_count + 'SELECT ' + CHAR(13)
        SET @sql_count = @sql_count + '		@record_count = COUNT(*) ' + CHAR(13)
        SET @sql_count = @sql_count + '	FROM' + CHAR(13)
        SET @sql_count = @sql_count + @from_block + CHAR(13)
        IF (LEN(@where_block) > 0)
            BEGIN
                SET @sql_count = @sql_count + 'WHERE ' + CHAR(13)
                SET @sql_count = @sql_count + @where_block + CHAR(13)
            END


        EXEC sp_executesql 
            @sql_count, 
            N'@record_count int OUTPUT, @itemIds Ids READONLY',
            @itemIds = @itemIds, 
            @record_count = @total_records OUTPUT
    END
    
    -- Задаем номер начальной записи по умолчанию
    IF (@start_row <= 0)
        BEGIN
            SET @start_row = 1
        END
        
    -- Задаем номер конечной записи
    DECLARE @end_row AS bigint
    if (@page_size = 0)
        SET @end_row = 0			
    else
        SET @end_row = @start_row + @page_size - 1		
    
    IF (@count_only = 0)
        BEGIN
            -- Возвращаем результат
            DECLARE @sql_result AS nvarchar(max)
            
            SET @sql_result = ''		
            SET @sql_result = @sql_result + 'WITH PAGED_DATA_CTE' + CHAR(13)
            SET @sql_result = @sql_result + 'AS' + CHAR(13)
            SET @sql_result = @sql_result + '(' + CHAR(13)
            SET @sql_result = @sql_result + '	SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		c.*, ' + CHAR(13)
            SET @sql_result = @sql_result + '		ROW_NUMBER() OVER (ORDER BY ' + @order_by_block + ') AS ROW_NUMBER'
            if @separate_count_query = 0
                SET @sql_result = @sql_result + ', COUNT(*) OVER() AS ROWS_COUNT ' + CHAR(13)
            SET @sql_result = @sql_result + '	FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	( ' + CHAR(13)
            SET @sql_result = @sql_result + '		SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @select_block + CHAR(13)
            SET @sql_result = @sql_result + '		FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @from_block + CHAR(13)
            IF (LEN(@where_block) > 0)
                BEGIN
                    SET @sql_result = @sql_result + '		WHERE' + CHAR(13)
                    SET @sql_result = @sql_result + '		' + @where_block + CHAR(13)
                END
            SET @sql_result = @sql_result + '	) AS c ' + CHAR(13)
            SET @sql_result = @sql_result + ')' + CHAR(13) + CHAR(13)
            
            SET @sql_result = @sql_result + 'SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '	* ' + CHAR(13)
            SET @sql_result = @sql_result + 'FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	PAGED_DATA_CTE' + CHAR(13)
            IF (@end_row > 0 or @start_row > 1)
            BEGIN
                SET @sql_result = @sql_result + 'WHERE 1 = 1' + CHAR(13)
                IF @start_row > 1 
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER >= ' + CAST(@start_row AS nvarchar) + ' '
                IF @end_row > 0
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER <= ' + CAST(@end_row AS nvarchar) + ' ' + CHAR(13)
            END	
            SET @sql_result = @sql_result + 'ORDER BY ' + CHAR(13)
            SET @sql_result = @sql_result + '	ROW_NUMBER ASC ' + CHAR(13)

            print(@sql_result)
            exec sp_executesql @sql_result, N'@itemIds Ids READONLY', @itemIds = @itemIds
        END
    
    SET NOCOUNT OFF
END
GO

ALTER procedure [dbo].[qp_update_items_with_content_data_pivot]
@content_id numeric,
@ids nvarchar(max),
@is_async bit,
@attr_ids nvarchar(max) = ''
as
begin

    declare @sql nvarchar(max), @fields nvarchar(max), @update_fields nvarchar(max), @prefixed_fields nvarchar(max), @table_name nvarchar(50)
     
    set @table_name = 'content_' + CAST(@content_id as nvarchar)
    if (@is_async = 1)
    set @table_name = @table_name + '_async'
        
    declare @attributes table
    (
        name nvarchar(255) primary key
    )
    
    if @attr_ids = ''
        insert into @attributes
        select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id
    else
        insert into @attributes
        select attribute_name from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id and attribute_id in (select nstr from dbo.SplitNew(@attr_ids, ','))

    if exists (select * from @attributes)
    begin
        SELECT @fields = COALESCE(@fields + ', ', '') + '[' + name + ']' FROM @attributes

        SELECT @update_fields = COALESCE(@update_fields + ', ', '') + 'base.[' + name + '] = pt.[' + name + ']' FROM @attributes
        
        set @sql = N'update base set ' + @update_fields + ' from ' + @table_name + ' base inner join
        (
        select ci.CONTENT_ITEM_ID, ci.STATUS_TYPE_ID, ci.VISIBLE, ci.ARCHIVE, ci.CREATED, ci.MODIFIED, ci.LAST_MODIFIED_BY, ca.ATTRIBUTE_NAME, 
        case WHEN ATTRIBUTE_TYPE_ID IN (9, 10) THEN cast (cd.blob_data as nvarchar(max)) ELSE dbo.qp_correct_data(cd.data, ca.attribute_type_id, ca.attribute_size, ca.default_value) END as pivot_data 
        from CONTENT_ATTRIBUTE ca
        left outer join CONTENT_DATA cd on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join CONTENT_ITEM ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
        where ca.CONTENT_ID = @content_id and cd.CONTENT_ITEM_ID in (' + @ids + ') 
        ) as src
        PIVOT
        (
        MAX(src.pivot_data)
        FOR src.ATTRIBUTE_NAME IN (' + @fields +  N')
        ) AS pt
        on pt.content_item_id = base.content_item_id
        '
        print @sql
        exec sp_executesql @sql, N'@content_id numeric', @content_id = @content_id
    end
end
GO

ALTER PROCEDURE [dbo].[qp_replicate_items] 
@ids nvarchar(max),
@attr_ids nvarchar(max) = ''
AS
BEGIN
    set nocount on
    
    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        status_type_id numeric,
        content_id numeric
    )
    
    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')
    
    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID 

    declare @contents table
    (
        id numeric primary key
    )
    
    insert into @contents
    select distinct content_id from @articles
    
    while exists (select id from @contents)
    begin
        select @content_id = id from @contents
        
        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 0
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 1
        
        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'
        
        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0, @attr_ids		
        end
        
        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1, @attr_ids							
        end
        
        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and status_type_id = @published_id and splitted = 0)
            update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        else
            update content_modification set stage_modified = GETDATE() where content_id = @content_id	

        
        delete from @contents where id = @content_id
    end
    
    set @sql = 'update content_item  set not_for_replication = 0 where content_item_id in (' + @ids + ' )'
    print @sql
    exec sp_executesql @sql
END
GO

exec qp_drop_existing 'qp_aggregated_and_self', 'IsTableFunction'
GO

CREATE function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as 
begin
    
    insert into @ids
    select id from @itemIds

    union all
    
    select AGG_DATA.CONTENT_ITEM_ID
    from CONTENT_ATTRIBUTE ATT
    JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
    JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
    where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1
    and ATT.CONTENT_ID in (select content_id from content_item with(nolock) where content_item_id in (select id from @itemIds)) AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @itemIds)	

    return
end
GO

exec qp_drop_existing 'qp_link_titles', 'IsScalarFunction'
GO


CREATE function [dbo].[qp_link_titles](@link_id int, @id int, @display_attribute_id int, @maxlength int)
returns nvarchar(max)
AS
BEGIN

    declare @names table
    (
        name nvarchar(255) primary key
    )
    declare @result nvarchar(max)

    insert into @names
    select coalesce(data, blob_data) from content_data where attribute_id = @display_attribute_id
    and content_item_id in (select linked_item_id from item_link where link_id = @link_id and item_id = @id)
    
    SELECT @result = COALESCE(@result + ', ', '') +  name  FROM @names

    if @result is null
        set @result = ''

    if (@maxlength > 0 and len(@result) > @maxlength)
        set @result = SUBSTRING(@result, 1, @maxlength) + '...'
    
    return @result
    
END
GO

update entity_type set CONTEXT_NAME = 'content_id' where code = 'virtual_content'
update entity_type set CONTEXT_NAME = 'content_item_id' where code = 'archive_article'
update entity_type set CONTEXT_NAME = 'content_item_id' where code = 'virtual_article'
update entity_type set CONTEXT_NAME = 'attribute_id' where code = 'virtual_field'
GO

ALTER FUNCTION [dbo].[qp_get_display_fields] 
(	
  @content_id numeric(18,0), 
  @with_relation_field BIT = 0
)
RETURNS TABLE 
AS
RETURN 
(
    select * from
    (
        SELECT  ATTRIBUTE_ID, attribute_name,
          CASE WHEN attribute_type_id in (9, 10) THEN CASE WHEN @with_relation_field = 1 THEN 1 ELSE 0 END
            WHEN attribute_type_id = 13 or IS_CLASSIFIER = 1 or attribute_type_id = 11 AND @with_relation_field = 0 THEN -1
            ELSE 1
          END AS attribute_priority,
          view_in_list,
          attribute_order
        FROM content_attribute ca
        WHERE content_id = @content_id
    ) as c
    where attribute_priority >= 0 
)
GO

ALTER PROCEDURE [dbo].[qp_update_m2o_final] 
@id numeric
AS
BEGIN
    if exists(select * from #resultIds)
    begin
        declare @statusId numeric
        declare @splitted bit
        declare @lastModifiedBy numeric
        declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, processed bit not null default 0, primary key(id, attribute_id))
    
        insert into @ids(id, attribute_id, to_remove)
        select * from #resultIds
    
        select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
    
        update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1 
        where content_item_id in (select id from @ids)
    
        update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate() 
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 0
    
        insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
        select r.id, r.attribute_id, @id, NULL, getdate()
        from @ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 0 and cd.CONTENT_DATA_ID is null
    
        update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate() 
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 1
    
        declare @maxStatus numeric
        declare @resultId numeric
    
        select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

        if @statusId = @maxStatus and @splitted = 0 begin 
        while exists (select * from child_delays where id = @id)
        begin
            select @resultId = child_id from child_delays where id = @id
            print @resultId
            delete from child_delays where id = @id and child_id = @resultId
            if not exists(select * from child_delays where child_id = @resultId)
            begin
                exec qp_merge_article @resultId
            end
        end
        end else if @maxStatus is not null begin
            insert into child_delays (id, child_id) select @id, r.id from @ids r 
            inner join content_item ci on r.id = ci.content_item_id 
            left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
            left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id 
            left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
            where ex.child_id is null and ci.status_type_id = wms.max_status_type_id 
                and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
        
            update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
        end
    
        while exists (select id from @ids where processed = 0)
        begin
            select @resultId = id from @ids where processed = 0
            exec qp_replicate @resultId
            update @ids set processed = 1 where id = @resultId
        end
    end
END
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CAN_UNLOCK_ITEMS' and TABLE_NAME = 'USER_GROUP')
    alter table [USER_GROUP] 
    add [CAN_UNLOCK_ITEMS] bit NOT NULL CONSTRAINT [DF_USER_GROUP_CAN_UNLOCK_ITEMS] DEFAULT ((0))
GO

update user_group set can_unlock_items = 1 where group_id = 1

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'API' and TABLE_NAME = 'BACKEND_ACTION_LOG')
    alter table [BACKEND_ACTION_LOG] 
    add [API] bit NOT NULL CONSTRAINT [DF_BACKEND_ACTION_LOG_API] DEFAULT ((0))
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_SERVICE' and TABLE_NAME = 'NOTIFICATIONS')
    alter table [NOTIFICATIONS] 
    add [USE_SERVICE] bit NOT NULL CONSTRAINT [DF_NOTIFICATIONS_USE_SERVICE] DEFAULT ((0))
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_AD_SYNC_SERVICE' and TABLE_NAME = 'DB')
    alter table [DB] 
    add [USE_AD_SYNC_SERVICE] bit NOT NULL CONSTRAINT [DF_DB_USE_AD_SYNC_SERVICE] DEFAULT ((0))
GO

exec qp_drop_existing 'EXTERNAL_NOTIFICATION_QUEUE', 'IsUserTable'
GO

if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'EXTERNAL_NOTIFICATION_QUEUE')
    create table [EXTERNAL_NOTIFICATION_QUEUE] 
    (
        ID NUMERIC PRIMARY KEY IDENTITY(1,1),
        EVENT_NAME nvarchar(50),
        ARTICLE_ID NUMERIC NOT NULL,
        CREATED DATETIME NOT NULL CONSTRAINT [DF_EXTERNAL_NOTIFICATION_QUEUE_CREATED] DEFAULT(GETDATE()),
        URL NVARCHAR(1024) NOT NULL,
        TRIES NUMERIC NOT NULL CONSTRAINT [DF_EXTERNAL_NOTIFICATION_QUEUE_TRIES] DEFAULT ((0)),
        NEW_XML NVARCHAR(MAX) NULL,
        OLD_XML NVARCHAR(MAX) NULL,
        SENT BIT NOT NULL CONSTRAINT [DF_EXTERNAL_NOTIFICATION_QUEUE_SENT] DEFAULT ((0))
    )
GO

ALTER VIEW [dbo].[USER_GROUP_TREE]
WITH SCHEMABINDING
AS
select ug.[GROUP_ID]
      ,ug.[GROUP_NAME]
      ,ug.[DESCRIPTION]
      ,ug.[CREATED]
      ,ug.[MODIFIED]
      ,ug.[LAST_MODIFIED_BY]
      ,U.[LOGIN] as LAST_MODIFIED_BY_LOGIN
      ,ug.[shared_content_items]
      ,ug.[nt_group]
      ,ug.[ad_sid]
      ,ug.[BUILT_IN]
      ,ug.[READONLY]
      ,ug.[use_parallel_workflow]
      ,ug.[CAN_UNLOCK_ITEMS]
      ,gtg.Parent_Group_Id AS PARENT_GROUP_ID 
from dbo.USER_GROUP ug 
left join dbo.Group_To_Group gtg on ug.GROUP_ID = gtg.Child_Group_Id
join dbo.USERS U ON U.[USER_ID] = ug.LAST_MODIFIED_BY

GO

ALTER TRIGGER [dbo].[ti_statuses_and_default_notif] ON [dbo].[SITE] 
FOR INSERT
AS
 
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
GO

update STATUS_TYPE set BUILT_IN = 1 where STATUS_TYPE_NAME in ('Created', 'Approved', 'Published', 'None')
GO

ALTER TRIGGER [dbo].[tbd_delete_content] ON [dbo].[CONTENT] INSTEAD OF DELETE
AS 
BEGIN
    create table #disable_td_delete_item(id numeric)

    UPDATE content_attribute SET related_attribute_id = NULL
    where related_attribute_id in (
        select attribute_id from content_attribute ca
        inner join deleted d on ca.content_id = d.content_id
    )

    UPDATE content_attribute SET CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = 0
    where CLASSIFIER_ATTRIBUTE_ID in (
        select attribute_id from content_attribute ca
        inner join deleted d on ca.content_id = d.content_id
    )

    update content_attribute set link_id = null where link_id in (select link_id from content_link cl
    inner join deleted d on cl.content_id = d.content_id)

    delete USER_DEFAULT_FILTER from USER_DEFAULT_FILTER f
    inner join deleted d on d.content_id = f.CONTENT_ID

    delete content_to_content from content_to_content cc
    inner join deleted d on d.content_id = cc.r_content_id or d.content_id = cc.l_content_id

    delete container from container c
    inner join deleted d on d.content_id = c.content_id 

    delete content_form from content_form cf
    inner join deleted d on d.content_id = cf.content_id 

    delete content_item from content_item ci
    inner join deleted d on d.content_id = ci.content_id

    delete content_tab_bind from content_tab_bind ctb
    inner join deleted d on d.content_id = ctb.content_id

    delete [ACTION_CONTENT_BIND] from [ACTION_CONTENT_BIND] acb
    inner join deleted d on d.content_id = acb.content_id
    
    delete ca from CONTENT_ATTRIBUTE ca
    inner join CONTENT_ATTRIBUTE cad on ca.BACK_RELATED_ATTRIBUTE_ID = cad.ATTRIBUTE_ID
    inner join deleted c on cad.CONTENT_ID = c.CONTENT_ID

    delete content from content c inner join deleted d on c.content_id = d.content_id

    drop table #disable_td_delete_item
END
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'FORM_SCRIPT' and TABLE_NAME = 'CONTENT')
    alter table [CONTENT] 
    add [FORM_SCRIPT] NTEXT NULL
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'HIDE' and TABLE_NAME = 'CONTENT_ACCESS')
    alter table [CONTENT_ACCESS] 
    add [HIDE] bit NOT NULL CONSTRAINT [DF_CONTENT_ACCESS_HIDE] DEFAULT ((0))
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'COLOR' and TABLE_NAME = 'STATUS_TYPE')
    alter table [STATUS_TYPE] 
    add [COLOR] char(6) NULL 
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'ALT_COLOR' and TABLE_NAME = 'STATUS_TYPE')
    alter table [STATUS_TYPE] 
    add [ALT_COLOR] char(6) NULL 
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'AUTO_OPEN_HOME' and TABLE_NAME = 'DB')
    alter table [DB] 
    add [AUTO_OPEN_HOME] bit NOT NULL CONSTRAINT [DF_DB_AUTO_OPEN_HOME] DEFAULT ((0))
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'EXCLUDED_BY_ACTION_ID' and TABLE_NAME = 'BACKEND_ACTION')
    alter table [BACKEND_ACTION] 
    add [EXCLUDED_BY_ACTION_ID] int NULL CONSTRAINT [FK_BACKEND_ACTION_EXCLUDED_BY_ACTION] FOREIGN KEY REFERENCES [dbo].[BACKEND_ACTION] ([ID])
GO

update BACKEND_ACTION set CONTROLLER_ACTION_URL = '~/Home/Home/' where code = 'home'
update BACKEND_ACTION set CONTROLLER_ACTION_URL = '~/Home/About/' where code = 'about'

select * from action_TOOLBAR_BUTTON where name = 'Refresh'

if not exists (select * from BACKEND_ACTION where code = 'refresh_home')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
values('Refresh Home', 'refresh_home', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('db'))

if not exists (select * from BACKEND_ACTION where code = 'refresh_about')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID)
values('Refresh About', 'refresh_about', dbo.qp_action_type_id('refresh'), dbo.qp_entity_type_id('db'))

if not exists (select * from ACTION_TOOLBAR_BUTTON where name = 'Refresh' and PARENT_ACTION_ID = dbo.qp_action_id('home'))
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, ICON, [ORDER], NAME)
values (dbo.qp_action_id('home'), dbo.qp_action_id('refresh_home'), 'refresh.gif', 100, 'Refresh')

if not exists (select * from ACTION_TOOLBAR_BUTTON where name = 'Refresh' and PARENT_ACTION_ID = dbo.qp_action_id('about'))
insert into ACTION_TOOLBAR_BUTTON (PARENT_ACTION_ID, ACTION_ID, ICON, [ORDER], NAME)
values (dbo.qp_action_id('about'), dbo.qp_action_id('refresh_about'), 'refresh.gif', 100, 'Refresh')
GO

ALTER PROCEDURE [dbo].[qp_all_article_search]
    @p_site_id int,
    @p_user_id int,
    @p_searchparam nvarchar(4000),
    @p_order_by nvarchar(max) = N'data.[rank] DESC',
    @p_start_row int = 0,
    @p_page_size int = 0,
    @p_item_id int = null,
    
    @total_records int OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    
    declare @is_admin bit
    select @is_admin = dbo.qp_is_user_admin(@p_user_id)
    
    -- Задаем номер начальной записи по умолчанию
    IF (@p_start_row <= 0)
        BEGIN
            SET @p_start_row = 1
        END
        
    -- Задаем номер конечной записи
    DECLARE @p_end_row AS int
    SET @p_end_row = @p_start_row + @p_page_size - 1
    
    -- свормировать запрос для подмножества контентов к которым есть доступ 
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''
    
    if @is_admin = 0
    begin
        EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @p_user_id,
                @group_id = 0,
                @start_level = 1,
                @end_level = 4,
                @entity_name = 'content',
                @parent_entity_name = 'site',
                @parent_entity_id = @p_site_id,				
                @SQLOut = @security_sql OUTPUT
    end
        
    -- посчитать общее кол-во записей					
    declare @paramdef nvarchar(4000);
    declare @query nvarchar(4000);
    
    create table #temp
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)
    
    create table #temp2
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)	
    
    declare @table_name nvarchar(10)
    if @is_admin = 0
        set @table_name = '#temp'
    else
        set @table_name = '#temp2'

    IF @p_item_id is not null
    begin
        set @query = 'insert into ' + @table_name + CHAR(13)
        set @query = @query + ' select ' + cast(@p_item_id as varchar(20)) + ', 0, 0, 1 ' + CHAR(13)
        exec sp_executesql @query
    end
    
    set @query = 'insert into ' + @table_name + CHAR(13)
        + ' select content_item_id, weight, attribute_id, priority from ' + CHAR(13)
        + ' (select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority, ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number ' + CHAR(13)
        + ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
        + ' inner join content_data cd on ft.[key] = cd.content_data_id) as c where c.number = 1 order by weight desc ' + CHAR(13)
    print @query
        
        
    exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam
    
    if @is_admin = 0
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join (' + @security_sql + ') c on c.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)	
        exec sp_executesql @query
    end
    
    select @total_records = count(distinct content_item_id) from #temp2		
        
    -- главный запрос
    declare @query_template nvarchar(4000);
    set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
        + ' (select ROW_NUMBER() OVER (ORDER BY [priority] DESC, <$_order_by_$>) AS ROW, ' + CHAR(13)
        + ' 	ci.CONTENT_ID as ParentId, ' + CHAR(13)
        + ' 	data.CONTENT_ITEM_ID as Id, ' + CHAR(13)
        + ' 	data.ATTRIBUTE_ID as FieldId, ' + CHAR(13)
        + ' 	attr.ATTRIBUTE_TYPE_ID as FieldTypeId, ' + CHAR(13)
        + ' 	c.CONTENT_NAME as ParentName, ' + CHAR(13) 	
        + ' 	st.STATUS_TYPE_NAME as StatusName, ' + CHAR(13)
        + ' 	ci.CREATED as Created, ' + CHAR(13)
        + ' 	ci.MODIFIED as Modified, ' + CHAR(13)
        + ' 	usr.[LOGIN] as LastModifiedByUser, ' + CHAR(13)	
        + ' 	data.[rank] as Rank, ' + CHAR(13)
        + ' 	data.[priority] as [priority] ' + CHAR(13)
        + '   from #temp2 data ' + CHAR(13)
        + '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
        + '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
        + '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID and c.site_id = @site_id ' + CHAR(13)	
        + '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
        + '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
        + ' ) ' + CHAR(13)
        + ' select ' + CHAR(13)  
        + ' 	ParentId, ' + CHAR(13)
        + ' 	ParentName, ' + CHAR(13)
        + ' 	Id, ' + CHAR(13)
        + ' 	FieldId, ' + CHAR(13)
        + '		(case when FieldTypeId in (9, 10) THEN cd.BLOB_DATA ELSE cd.DATA END) as Text, ' + CHAR(13)		
        + ' 	dbo.qp_get_article_title_func(Id, ParentId) as Name, ' + CHAR(13)	
        + ' 	StatusName, ' + CHAR(13)
        + ' 	pdc.Created, ' + CHAR(13)
        + ' 	pdc.Modified, ' + CHAR(13)
        + ' 	LastModifiedByUser, ' + CHAR(13)	
        + ' 	Rank ' + CHAR(13)
        + ' from PAGED_DATA_CTE pdc ' + CHAR(13)
        + ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
        + ' where ROW between @start_row and @end_row order by row asc';
        
    
    declare @sortExp nvarchar(4000);
    set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;	
    set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);	
    set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
    print @query
    EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;
    
    drop table #temp
    drop table #temp2
END
GO

exec sp_refreshview 'CONTENT_ACCESS_PermLevel'
exec sp_refreshview 'CONTENT_ACCESS_PermLevel_site'

GO
ALTER function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as 
begin
    
    insert into @ids
    select id from @itemIds

    union 	
    select AGG_DATA.CONTENT_ITEM_ID
    from CONTENT_ATTRIBUTE ATT
    JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
    JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
    where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1
    and ATT.CONTENT_ID in (select content_id from content_item with(nolock) where content_item_id in (select id from @itemIds)) AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @itemIds)	

    return
end
GO

ALTER TABLE [dbo].[BACKEND_ACTION] DROP CONSTRAINT [FK_BACKEND_ACTION_EXCLUDED_BY_ACTION]
GO
ALTER TABLE [dbo].[BACKEND_ACTION] DROP COLUMN [EXCLUDED_BY_ACTION_ID]
GO

exec qp_drop_existing 'ACTION_EXCLUSIONS', 'IsUserTable'
GO

CREATE TABLE [dbo].[ACTION_EXCLUSIONS](
    [EXCLUDED_BY_ID] [int] NOT NULL,
    [EXCLUDES_ID] [int] NOT NULL,
 CONSTRAINT [PK_ACTION_EXCLUSIONS] PRIMARY KEY CLUSTERED 
(
    [EXCLUDED_BY_ID] ASC,
    [EXCLUDES_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ACTION_EXCLUSIONS]  WITH CHECK ADD  CONSTRAINT [FK_ACTION_EXCLUSIONS_EXCLUDED_BY] FOREIGN KEY([EXCLUDED_BY_ID])
REFERENCES [dbo].[BACKEND_ACTION] ([ID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ACTION_EXCLUSIONS]  WITH CHECK ADD  CONSTRAINT [FK_ACTION_EXCLUSIONS_EXCLUDES] FOREIGN KEY([EXCLUDES_ID])
REFERENCES [dbo].[BACKEND_ACTION] ([ID])
GO

if not exists (select * from ACTION_TYPE where code = 'crop')
insert into ACTION_TYPE(NAME, CODE, REQUIRED_PERMISSION_LEVEL_ID, ITEMS_AFFECTED) 
VALUES('Crop', 'crop', 2, 1)

if not exists(select * from BACKEND_ACTION where code = 'crop_site_file')
insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
values(dbo.qp_action_type_id('crop'), dbo.qp_entity_type_id('site_file'), 'Crop Site File', 'crop_site_file')

if not exists(select * from BACKEND_ACTION where code = 'crop_content_file')
insert into BACKEND_ACTION(TYPE_ID, ENTITY_TYPE_ID, NAME, CODE)
values(dbo.qp_action_type_id('crop'), dbo.qp_entity_type_id('content_file'), 'Crop Content File', 'crop_content_file')

if not exists(select * from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('crop_content_file'))
insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
values(dbo.qp_context_menu_id('content_file'), dbo.qp_action_id('crop_content_file'), 'Crop', 50, 'crop.gif')

if not exists(select * from CONTEXT_MENU_ITEM where action_id = dbo.qp_action_id('crop_site_file'))
insert into CONTEXT_MENU_ITEM (CONTEXT_MENU_ID, ACTION_ID, NAME, [ORDER], ICON)
values(dbo.qp_context_menu_id('site_file'), dbo.qp_action_id('crop_site_file'), 'Crop', 50, 'crop.gif')
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'CONTENT_FORM_SCRIPT' and TABLE_NAME = 'SITE')
    alter table [SITE] 
    add [CONTENT_FORM_SCRIPT] NTEXT NULL
GO

ALTER PROCEDURE [dbo].[qp_update_m2m_values]
    @xmlParameter xml
AS
BEGIN
    DECLARE @fieldValues TABLE(rowNumber numeric, id numeric, linkId numeric, value nvarchar(max), splitted bit)
    DECLARE @rowValues TABLE(id numeric, linkId numeric, value nvarchar(max), splitted bit)
    INSERT INTO @fieldValues
    SELECT
        ROW_NUMBER() OVER(order by doc.col.value('./@id', 'int')) as rowNumber 
         ,doc.col.value('./@id', 'int') id
         ,doc.col.value('./@linkId', 'int') linkId
         ,doc.col.value('./@value', 'nvarchar(max)') value
         ,c.SPLITTED as splitted
        FROM @xmlParameter.nodes('/items/item') doc(col)
        INNER JOIN content_item as c on c.CONTENT_ITEM_ID = doc.col.value('./@id', 'int')


    declare @newIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit, linked_attribute_id numeric null, linked_has_data bit, linked_splitted bit, linked_has_async bit null)
    declare @ids table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)
    declare @crossIds table (id numeric, link_id numeric, linked_item_id numeric, splitted bit)

    insert into @newIds (id, link_id, linked_item_id, splitted) 
    select a.id, a.linkId, b.nstr, a.splitted from @fieldValues a cross apply dbo.SplitNew(a.value, ',') b
    where b.nstr is not null and b.nstr <> '' and b.nstr <> '0'

    insert into @ids 
    select item_id, link_id, linked_item_id, f.splitted 
    from item_link_async ila inner join @fieldValues f 
    on ila.link_id = f.linkId and ila.item_id = f.id
    where f.splitted = 1
    union all
    select item_id, link_id, linked_item_id, f.splitted 
    from item_link il inner join @fieldValues f 
    on il.link_id = f.linkId and il.item_id = f.id
    where f.splitted = 0

    insert into @crossIds 
    select t1.id, t1.link_id, t1.linked_item_id, t1.splitted from @ids t1 inner join @newIds t2 
    on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

    delete @ids from @ids t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
    delete @newIds from @newIds t1 inner join @crossIds t2 on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id

    delete item_link_async from item_link_async il inner join @fieldValues f on il.item_id = f.id and il.link_id = f.linkId
    where f.splitted = 0 

    delete item_link_united_full from item_link_united_full il 
    where exists( 
        select * from @ids i where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
        and i.splitted = 0
    )

    delete item_link_async from item_link_united_full il 
    inner join @ids i on il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id
    where i.splitted = 1

    insert into item_link_async
    select link_id, id, linked_item_id from @newIds 
    where splitted = 1

    insert into item_to_item
    select link_id, id, linked_item_id from @newIds 
    where splitted = 0

    delete from @newIds where dbo.qp_is_link_symmetric(link_id) = 0;

    with newItems (id, link_id, linked_item_id, attribute_id, has_data, splitted, has_async) as
    (
    select 
        n.id, n.link_id, n.linked_item_id, ca.attribute_id, 
        case when cd.content_item_id is null then 0 else 1 end as has_data, 
        ci.splitted, 
        case when ila.link_id is null then 0 else 1 end as has_async
    from @newIds n
        inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
        inner join content c on ci.content_id = c.content_id
        inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
        left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
        left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.id = ila.linked_item_id
    )
    update @newIds 
    set linked_attribute_id = ext.attribute_id, linked_has_data = ext.has_data, linked_splitted = ext.splitted, linked_has_async = ext.has_async
    from @newIds n inner join newItems ext on n.id = ext.id and n.link_id = ext.link_id and n.linked_item_id = ext.linked_item_id

    update content_data set data = n.link_id 
    from content_data cd 
    inner join @newIds n on cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
    where n.splitted = 0 and n.linked_has_data = 1

    insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
    select distinct n.linked_item_id, n.linked_attribute_id, n.link_id
    from @newIds n 
    where n.splitted = 0 and n.linked_has_data = 0 and n.linked_attribute_id is not null
            
    insert into item_link_async(link_id, item_id, linked_item_id)
    select n.link_id, n.linked_item_id, n.id
    from @newIds n 
    where n.splitted = 0 and n.linked_splitted = 1 and n.linked_has_async = 0 and n.linked_attribute_id is not null
END
GO

ALTER PROCEDURE [dbo].[qp_merge_article] 
@item_id numeric,
@last_modified_by numeric = 1
AS
BEGIN
    if exists (select * from content_item where content_item_id = @item_id and SCHEDULE_NEW_VERSION_PUBLICATION = 1)
    begin
    exec qp_merge_links @item_id 
    UPDATE content_item with(rowlock) set not_for_replication = 1 WHERE content_item_id = @item_id
    UPDATE content_item with(rowlock) set SCHEDULE_NEW_VERSION_PUBLICATION = 0, MODIFIED = GETDATE(), LAST_MODIFIED_BY = @last_modified_by, CANCEL_SPLIT = 0 where CONTENT_ITEM_ID = @item_id 
    exec qp_replicate @item_id
    UPDATE content_item_schedule with(rowlock) set delete_job = 0 WHERE content_item_id = @item_id
    DELETE FROM content_item_schedule with(rowlock) WHERE content_item_id = @item_id
    delete from CHILD_DELAYS with(rowlock) WHERE id = @item_id
    delete from CHILD_DELAYS with(rowlock) WHERE child_id = @item_id
    end
END
GO

ALTER PROCEDURE [dbo].[qp_update_m2o_final] 
@id numeric
AS
BEGIN
    if exists(select * from #resultIds)
    begin
        declare @statusId numeric
        declare @splitted bit
        declare @lastModifiedBy numeric
        declare @ids table (id numeric, attribute_id numeric not null, to_remove bit not null default 0, processed bit not null default 0, primary key(id, attribute_id))
    
        insert into @ids(id, attribute_id, to_remove)
        select * from #resultIds
    
        select @statusId = STATUS_TYPE_ID, @splitted = SPLITTED, @lastModifiedBy = LAST_MODIFIED_BY from content_item where CONTENT_ITEM_ID = @id
    
        update content_item set modified = getdate(), last_modified_by = @lastModifiedBy, not_for_replication = 1 
        where content_item_id in (select id from @ids)
    
        update content_data set content_data.data = @id, content_data.blob_data = null, content_data.modified = getdate() 
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 0
    
        insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
        select r.id, r.attribute_id, @id, NULL, getdate()
        from @ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 0 and cd.CONTENT_DATA_ID is null
    
        update content_data set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate() 
        from content_data cd inner join @ids r on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id 
        where r.to_remove = 1
    
        declare @maxStatus numeric
        declare @resultId numeric
    
        select @maxStatus = max_status_type_id from content_item_workflow ciw left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id where ciw.content_item_id = @id

        if @statusId = @maxStatus and @splitted = 0 begin 
        while exists (select * from child_delays where id = @id)
        begin
            select @resultId = child_id from child_delays where id = @id
            print @resultId
            delete from child_delays where id = @id and child_id = @resultId
            if not exists(select * from child_delays where child_id = @resultId)
            begin
                exec qp_merge_article @resultId, @lastModifiedBy
            end
        end
        end else if @maxStatus is not null begin
            insert into child_delays (id, child_id) select @id, r.id from @ids r 
            inner join content_item ci on r.id = ci.content_item_id 
            left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = @id
            left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id 
            left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
            where ex.child_id is null and ci.status_type_id = wms.max_status_type_id 
                and (ci.splitted = 0 or ci.splitted = 1 and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> @id))
        
            update content_item set schedule_new_version_publication = 1 where content_item_id in (select child_id from child_delays where id = @id)
        end
    
        while exists (select id from @ids where processed = 0)
        begin
            select @resultId = id from @ids where processed = 0
            exec qp_replicate @resultId
            update @ids set processed = 1 where id = @resultId
        end
    end
END
GO

if not exists (select * From information_schema.columns where table_name = 'BACKEND_ACTION' and column_name = 'ADDITIONAL_CONTROLLER_ACTION_URL') 
begin
    ALTER TABLE dbo.BACKEND_ACTION ADD ADDITIONAL_CONTROLLER_ACTION_URL nvarchar(255) NULL
end
GO

if not exists (select * From information_schema.columns where table_name = 'BACKEND_ACTION' and column_name = 'ENTITY_LIMIT') 
begin
    ALTER TABLE dbo.BACKEND_ACTION ADD ENTITY_LIMIT int NULL
end
GO

UPDATE BACKEND_ACTION SET ENTITY_LIMIT = 20, ADDITIONAL_CONTROLLER_ACTION_URL = '~/Multistep/ArchiveArticlesService/' WHERE CODE = 'multiple_move_to_archive_article'
UPDATE BACKEND_ACTION SET ENTITY_LIMIT = 20, ADDITIONAL_CONTROLLER_ACTION_URL = '~/Multistep/RestoreArticlesService/' WHERE CODE = 'multiple_restore_from_archive_article'
UPDATE BACKEND_ACTION SET ENTITY_LIMIT = 20, ADDITIONAL_CONTROLLER_ACTION_URL = '~/Multistep/PublishArticlesService/' WHERE CODE = 'multiple_publish_articles'
UPDATE BACKEND_ACTION SET ENTITY_LIMIT = 20, ADDITIONAL_CONTROLLER_ACTION_URL = '~/Multistep/RemoveArticlesService/' WHERE CODE = 'multiple_remove_article'
UPDATE BACKEND_ACTION SET ENTITY_LIMIT = 20, ADDITIONAL_CONTROLLER_ACTION_URL = '~/Multistep/RemoveArticlesFromArchiveService/' WHERE CODE = 'multiple_remove_archive_article'
GO

exec qp_drop_existing 'qp_delete_single_link', 'IsProcedure'
GO

CREATE PROCEDURE dbo.qp_delete_single_link
@linkId numeric, 
@itemId numeric,
@linkedItemId numeric
AS
BEGIN
    delete from item_link_united_full where link_id = @linkId and item_id = @itemId and linked_item_id = @linkedItemId
END
GO

exec qp_drop_existing 'qp_insert_single_link', 'IsProcedure'
GO

CREATE PROCEDURE dbo.qp_insert_single_link
@linkId numeric, 
@itemId numeric,
@linkedItemId numeric
AS
BEGIN

    declare @splitted bit, @linkedSplitted bit

    select @splitted = splitted from content_item with (nolock) where content_item_id = @itemId
    select @linkedSplitted = splitted from content_item with (nolock) where content_item_id = @linkedItemId

    if @splitted is not null and @linkedSplitted is not null
    begin
        if (@splitted = 1) and not exists (select * From item_link_async where link_id = @linkId and item_id = @itemId and linked_item_id = @linkedItemId)
        insert into item_link_async(link_id, item_id, linked_item_id) values (@linkId, @itemId, @linkedItemId)

        if (@linkedSplitted = 1) and not exists (select * From item_link_async where link_id = @linkId and item_id = @linkedItemId and linked_item_id = @itemId)
        insert into item_link_async(link_id, item_id, linked_item_id) values (@linkId, @linkedItemId, @itemId)
        
        
        if (@splitted = 0 or @linkedSplitted = 0) and not exists (select * From item_to_item where link_id = @linkId and l_item_id = @itemId and r_item_id = @linkedItemId)
        insert into item_to_item (link_id, l_item_id, r_item_id) values (@linkId, @itemId, @linkedItemId)

    end
END
GO

if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'APP_SETTINGS')
    create table dbo.[APP_SETTINGS] 
    (
        [KEY] NVARCHAR(255) PRIMARY KEY NOT NULL,
        VALUE nvarchar(255) NOT NULL
    )
GO

if not exists(select * from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'USE_IN_CHILD_CONTENT_FILTER' and TABLE_NAME = 'CONTENT_ATTRIBUTE')
ALTER TABLE CONTENT_ATTRIBUTE 
    ADD USE_IN_CHILD_CONTENT_FILTER BIT NOT NULL CONSTRAINT DF_CONTENT_ATTRIBUTE_USE_IN_CHILD_CONTENT_FILTER DEFAULT (0)
GO

;with data as
(
select ROW_NUMBER() over (partition by w.workflow_id order by st.weight asc) as new_order,  wr.WORKFLOW_RULE_ID, RULE_ORDER  from workflow_rules wr inner join workflow w on wr.WORKFLOW_ID = w.WORKFLOW_ID
inner join STATUS_TYPE st on SUCCESSOR_STATUS_ID = st.STATUS_TYPE_ID
)
update wr set wr.rule_order = data.new_order from workflow_rules wr inner join data on wr.WORKFLOW_RULE_ID = data.Workflow_rule_id where data.new_order <> wr.RULE_ORDER
GO

declare @content_id numeric
declare contents CURSOR FOR select content_id from content

open contents
fetch next from contents into @content_id
while @@fetch_status = 0
begin
    exec qp_content_frontend_views_recreate @content_id
    fetch next from contents into @content_id
end
close contents
deallocate contents
GO


ALTER PROCEDURE [dbo].[qp_merge_links] 
@content_item_id numeric
AS 
declare @splitted bit, @content_id numeric
BEGIN
    select @splitted = splitted, @content_id = content_id from content_item with(nolock) where content_item_id = @content_item_id
    if @splitted = 1
    BEGIN
        
        declare @linkIds table (link_id numeric, primary key (link_id))
        declare @newIds table (link_id numeric, id numeric, attribute_id numeric null, has_data bit null, splitted bit null, has_async bit null, primary key (link_id, id))
        declare @oldIds table (link_id numeric, id numeric, primary key (link_id, id))
        declare @crossIds table (link_id numeric, id numeric, primary key (link_id, id))

        insert into @linkIds (link_id) select link_id from CONTENT_ATTRIBUTE where content_id = @content_id and link_id is not null
        insert into @newIds (link_id, id) select link_id, linked_item_id from item_link_async where item_id = @content_item_id and link_id in (select link_id from @linkIds)
        insert into @oldIds select link_id, linked_item_id from item_link where item_id = @content_item_id and link_id in (select link_id from @linkIds)
        insert into @crossIds select t1.link_id, t1.id from @oldIds t1 inner join @newIds t2 on t1.id = t2.id and t1.link_id = t2.link_id
        
        delete @oldIds from @oldIds i inner join @crossIds ci on i.link_id = ci.link_id and i.id = ci.id 
        delete @newIds from @newIds i inner join @crossIds ci on i.link_id = ci.link_id and i.id = ci.id
        
        delete item_to_item from item_to_item ii inner join @oldIds i on i.link_id = ii.link_id and i.id = ii.r_item_id
        where ii.l_item_id = @content_item_id
        
        insert into item_to_item (link_id, l_item_id, r_item_id)
        select link_id, @content_item_id, id from @newIds;
        
        with newItems (link_id, id, attribute_id, has_data, splitted, has_async) as
        (
        select 
            n.link_id, n.id, ca.attribute_id, 
            case when cd.content_item_id is null then 0 else 1 end as has_data, 
            ci.splitted, 
            case when ila.link_id is null then 0 else 1 end as has_async
        from @newIds n
            inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            inner join content c on ci.content_id = c.content_id
            inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
            inner join content_to_content c2c on c2c.link_id = n.link_id
            left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            left join item_link_async ila on n.link_id = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @content_item_id
            where c2c.symmetric = 1
        )
        update @newIds 
        set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
        from @newIds n inner join newItems ext on n.link_id = ext.link_id and n.id = ext.id
        
        update content_data set data = n.link_id 
        from content_data cd 
        inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
        where n.has_data = 1
        
        insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
        select n.id, n.attribute_id, n.link_id
        from @newIds n where n.has_data = 0 and n.attribute_id is not null
        
        insert into item_link_async(link_id, item_id, linked_item_id)
        select n.link_id, n.id, @content_item_id
        from @newIds n 
        where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
        
        delete from item_link_async with(rowlock) where item_id = @content_item_id
    END
END
GO


ALTER  TRIGGER [dbo].[tu_update_field] ON [dbo].[CONTENT_ATTRIBUTE] FOR UPDATE 
AS 
BEGIN
if not update(attribute_order) and 
        (
            update(attribute_name) or update(attribute_type_id) 
            or update(attribute_size) or update(index_flag)
        )
    begin
        declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric, @content_id numeric
        declare @indexed numeric, @required numeric
        declare @attribute_type_id numeric, @type_name nvarchar(255), @database_type nvarchar(255)

        declare @new_attribute_name nvarchar(255), @new_attribute_size numeric
        declare @new_indexed numeric, @new_required numeric
        declare @new_attribute_type_id numeric, @new_type_name nvarchar(255), @new_database_type nvarchar(255)
        declare @related_content_id numeric, @new_related_content_id numeric
        declare @link_id numeric, @new_link_id numeric
        
        declare @base_table_name nvarchar(30), @table_name nvarchar(30)
    
        declare @i numeric, @count numeric, @preserve_index bit
    
        declare @ca table (
            id numeric identity(1,1) primary key,
            attribute_id numeric, 
            attribute_name nvarchar(255), 
            attribute_size numeric,
            indexed numeric,
            required numeric, 
            attribute_type_id numeric, 
            type_name nvarchar(255), 
            database_type nvarchar(255),
            content_id numeric,
            related_content_id numeric,
            link_id numeric
        )
    
    /* Collect affected items */
        insert into @ca (attribute_id, attribute_name, attribute_size, indexed, required, attribute_type_id, type_name, database_type, content_id, related_content_id, link_id)
            select d.attribute_id, d.attribute_name, d.attribute_size, d.index_flag, d.required, d.attribute_type_id, at.type_name, at.database_type, d.content_id, 
            isnull(ca1.content_id, 0), isnull(d.link_id, 0)
            from deleted d 
            inner join attribute_type at on d.attribute_type_id = at.attribute_type_id
            inner join content c on d.content_id = c.content_id
            left join CONTENT_ATTRIBUTE ca1 on d.RELATED_ATTRIBUTE_ID = ca1.ATTRIBUTE_ID
            where c.virtual_type = 0

        set @i = 1
        select @count = count(id) from @ca

        while @i < @count + 1
        begin
            select @attribute_id = attribute_id, @attribute_name = attribute_name, @attribute_size = attribute_size,
                @indexed = indexed, @required = required, @attribute_type_id = attribute_type_id, 
                @type_name = type_name, @database_type = database_type, @content_id = content_id,
                @related_content_id = related_content_id, @link_id = link_id 
                from @ca where id = @i

            select @new_attribute_name = ca.attribute_name, @new_attribute_size = ca.attribute_size,
                @new_indexed = ca.index_flag, @new_required = ca.required, @new_attribute_type_id = ca.attribute_type_id, 
                @new_type_name = at.type_name, @new_database_type = at.database_type,
                @new_related_content_id = isnull(ca1.content_id, 0), @new_link_id = isnull(ca.link_id, 0)
                from content_attribute ca 
                inner join attribute_type at on ca.attribute_type_id = at.attribute_type_id
                left join CONTENT_ATTRIBUTE ca1 on ca.RELATED_ATTRIBUTE_ID = ca1.ATTRIBUTE_ID
                where ca.attribute_id = @attribute_id

                set @base_table_name = 'content_' + convert(nvarchar, @content_id)		
                set @table_name = @base_table_name + '_ASYNC'

                if @indexed = 1 and @new_indexed = 1
                    set @preserve_index = 1
                else 
                    set @preserve_index = 0
                    
                if @attribute_type_id <> @new_attribute_type_id 
                    or @link_id <> @new_link_id 
                    or @related_content_id <> @new_related_content_id
                    or (@attribute_size > @new_attribute_size and @attribute_type_id = 1)
                begin
                    exec qp_clear_versions_for_field @attribute_id
                end

                if @indexed = 1 and @new_indexed = 0
                begin
                    exec qp_drop_index @base_table_name, @attribute_name
                    exec qp_drop_index @table_name, @attribute_name
                end

                if @database_type <> @new_database_type or (@attribute_size <> @new_attribute_size and @new_database_type <> 'ntext')
                begin

                    if @database_type = 'ntext' and @new_database_type <> 'ntext'
                        exec qp_copy_blob_data_to_data @attribute_id				
                    else if @database_type <> 'ntext' and @new_database_type = 'ntext'
                        exec qp_copy_data_to_blob_data @attribute_id	

                    exec qp_recreate_column @base_table_name, @attribute_id, @attribute_name, @new_attribute_name, @type_name, @new_type_name, @new_database_type, @new_attribute_size, @preserve_index
                    exec qp_recreate_column @table_name, @attribute_id, @attribute_name, @new_attribute_name, @type_name, @new_type_name, @new_database_type, @new_attribute_size, @preserve_index
                    exec qp_content_united_view_recreate @content_id
                    exec qp_content_frontend_views_recreate @content_id
                end 
                else begin
                    if @attribute_name <> @new_attribute_name
                    begin
                        exec qp_rename_column @base_table_name, @attribute_name, @new_attribute_name, @preserve_index
                        exec qp_rename_column @table_name, @attribute_name, @new_attribute_name, @preserve_index 
                        exec qp_content_united_view_recreate @content_id
                        exec qp_content_frontend_views_recreate @content_id
                    end
                end
                if @attribute_name <> @new_attribute_name
                    UPDATE container Set order_static = REPLACE(order_static, @attribute_name, @new_attribute_name) WHERE content_id = @content_id AND order_static LIKE '%'+ @attribute_name +'%'

                if @indexed = 0 and @new_indexed = 1
                begin
                    exec qp_add_index @base_table_name, @new_attribute_name
                    exec qp_add_index @table_name, @new_attribute_name
                end
            set @i = @i + 1
        end
    end
END
GO

ALTER VIEW [dbo].[item_link_united] AS
select link_id, item_id, linked_item_id from item_link il inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID where ci.splitted = 0
union all
SELECT link_id, item_id, linked_item_id from item_link_async ila 
GO


ALTER PROCEDURE [dbo].[qp_count_child_articles]
    @article_id int,
    @count_archived bit = 1,
    @count int output
AS
BEGIN
    set @count = 0
    declare @baseContentId numeric
    select @baseContentId  = content_id from content_item where content_item_id = @article_id
    declare @relatedFields table (
        id numeric primary key identity(1, 1),
        content_id numeric,
        attribute_name nvarchar(255)
    )
    insert into @relatedFields(content_id, attribute_name)
    select ca.content_id, attribute_name from content_attribute ca inner join content c on ca.content_id = c.CONTENT_ID where c.virtual_type = 0 and ca.AGGREGATED = 0 and attribute_type_id = 11 and related_attribute_id in (select attribute_id from content_attribute ca where ca.content_id = @baseContentId)
    
    declare @content_id numeric, @attribute_name nvarchar(255), @sql nvarchar(max), @archive_sql nvarchar(100)

    if @count_archived = 1
        set @archive_sql = ''
    else
        set @archive_sql = ' and archive = 0'

    declare @total numeric, @i numeric
    select @total = count(id) from @relatedFields
    set @i = 1
    
    while @i <= @total
    begin
        declare @result numeric
        select @content_id = content_id, @attribute_name = attribute_name from @relatedFields where id = @i
        set @sql = N'select @result = count(content_item_id) from content_' + cast(@content_id as nvarchar) + '_united where [' + @attribute_name + '] = @value' + @archive_sql
        exec sp_executesql @sql, N'@result numeric output, @value numeric', @result = @result out, @value = @article_id	
        set @count = @count + @result 
        set @i = @i + 1
    end
END
GO

ALTER PROCEDURE [dbo].[qp_all_article_search]
    @p_site_id int,
    @p_user_id int,
    @p_searchparam nvarchar(4000),
    @p_order_by nvarchar(max) = N'data.[rank] DESC',
    @p_start_row int = 0,
    @p_page_size int = 0,
    @p_item_id int = null,
    
    @total_records int OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    
    declare @is_admin bit
    select @is_admin = dbo.qp_is_user_admin(@p_user_id)
    
    -- Задаем номер начальной записи по умолчанию
    IF (@p_start_row <= 0)
        BEGIN
            SET @p_start_row = 1
        END
        
    -- Задаем номер конечной записи
    DECLARE @p_end_row AS int
    SET @p_end_row = @p_start_row + @p_page_size - 1
    
    -- свормировать запрос для подмножества контентов к которым есть доступ 
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''
    
    if @is_admin = 0
    begin
        EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @p_user_id,
                @group_id = 0,
                @start_level = 1,
                @end_level = 4,
                @entity_name = 'content',
                @parent_entity_name = 'site',
                @parent_entity_id = @p_site_id,				
                @SQLOut = @security_sql OUTPUT
    end
        
    -- посчитать общее кол-во записей					
    declare @paramdef nvarchar(4000);
    declare @query nvarchar(4000);
    
    create table #temp
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)
    
    create table #temp2
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)	
    
    IF @p_item_id is not null
    begin
        set @query = 'insert into #temp' + CHAR(13)
        set @query = @query + ' select ' + cast(@p_item_id as varchar(20)) + ', 0, 0, 1 ' + CHAR(13)
        exec sp_executesql @query
    end
    
    set @query = 'insert into #temp' + CHAR(13)
        + ' select content_item_id, weight, attribute_id, priority from ' + CHAR(13)
        + ' (select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority, ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number ' + CHAR(13)
        + ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
        + ' inner join content_data cd on ft.[key] = cd.content_data_id) as c where c.number = 1 order by weight desc ' + CHAR(13)
    print @query
        
        
    exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam
    set @paramdef = '@site_id int';
    if @is_admin = 0
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c0 on c0.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)	
            + ' inner join (' + @security_sql + ') c on c.CONTENT_ID = c0.CONTENT_ID where c0.site_id = @site_id' + CHAR(13)

        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end
    else
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c on c.CONTENT_ID = ci.CONTENT_ID where c.site_id = @site_id' + CHAR(13)	
        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end
    
    select @total_records = count(distinct content_item_id) from #temp2		
        
    -- главный запрос
    declare @query_template nvarchar(4000);
    set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
        + ' (select ROW_NUMBER() OVER (ORDER BY [priority] DESC, <$_order_by_$>) AS ROW, ' + CHAR(13)
        + ' 	ci.CONTENT_ID as ParentId, ' + CHAR(13)
        + ' 	data.CONTENT_ITEM_ID as Id, ' + CHAR(13)
        + ' 	data.ATTRIBUTE_ID as FieldId, ' + CHAR(13)
        + ' 	attr.ATTRIBUTE_TYPE_ID as FieldTypeId, ' + CHAR(13)
        + ' 	c.CONTENT_NAME as ParentName, ' + CHAR(13) 	
        + ' 	st.STATUS_TYPE_NAME as StatusName, ' + CHAR(13)
        + ' 	ci.CREATED as Created, ' + CHAR(13)
        + ' 	ci.MODIFIED as Modified, ' + CHAR(13)
        + ' 	usr.[LOGIN] as LastModifiedByUser, ' + CHAR(13)	
        + ' 	data.[rank] as Rank, ' + CHAR(13)
        + ' 	data.[priority] as [priority] ' + CHAR(13)
        + '   from #temp2 data ' + CHAR(13)
        + '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
        + '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
        + '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID and c.site_id = @site_id ' + CHAR(13)	
        + '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
        + '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
        + ' ) ' + CHAR(13)
        + ' select ' + CHAR(13)  
        + ' 	ParentId, ' + CHAR(13)
        + ' 	ParentName, ' + CHAR(13)
        + ' 	Id, ' + CHAR(13)
        + ' 	FieldId, ' + CHAR(13)
        + '		(case when FieldTypeId in (9, 10) THEN cd.BLOB_DATA ELSE cd.DATA END) as Text, ' + CHAR(13)		
        + ' 	dbo.qp_get_article_title_func(Id, ParentId) as Name, ' + CHAR(13)	
        + ' 	StatusName, ' + CHAR(13)
        + ' 	pdc.Created, ' + CHAR(13)
        + ' 	pdc.Modified, ' + CHAR(13)
        + ' 	LastModifiedByUser, ' + CHAR(13)	
        + ' 	Rank ' + CHAR(13)
        + ' from PAGED_DATA_CTE pdc ' + CHAR(13)
        + ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
        + ' where ROW between @start_row and @end_row order by row asc';
        
    
    declare @sortExp nvarchar(4000);
    set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;	
    set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);	
    set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
    print @query
    EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;
    
    drop table #temp
    drop table #temp2
END
GO

if not exists (select * from sys.tables where name = 'content_item_splitted')
CREATE TABLE [dbo].[CONTENT_ITEM_SPLITTED](
    [CONTENT_ITEM_ID] [numeric](18, 0) NOT NULL,
 CONSTRAINT [PK_CONTENT_ITEM_SPLITTED] PRIMARY KEY CLUSTERED ( [CONTENT_ITEM_ID] ASC)
)
GO


if not exists (select * from sys.foreign_keys where name = 'FK_CONTENT_ITEM_SPLITTED_CONTENT_ITEM')
begin
    ALTER TABLE [dbo].[CONTENT_ITEM_SPLITTED]  WITH CHECK ADD  CONSTRAINT [FK_CONTENT_ITEM_SPLITTED_CONTENT_ITEM] FOREIGN KEY([CONTENT_ITEM_ID])
    REFERENCES [dbo].[CONTENT_ITEM] ([CONTENT_ITEM_ID])
    ON DELETE CASCADE
end
GO


ALTER VIEW [dbo].[item_link_united] AS
select link_id, item_id, linked_item_id from item_link il where not exists (select * from content_item_splitted cis where il.item_id = cis.CONTENT_ITEM_ID)
union all
SELECT link_id, item_id, linked_item_id from item_link_async ila 
GO

ALTER TRIGGER [dbo].[tu_update_item] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
begin
    if not update(locked_by) and not update(splitted) and not UPDATE(not_for_replication)
    begin
        declare @content_id numeric
        declare @sql nvarchar(max), @table_name varchar(50), @async_table_name varchar(50)
        declare @ids_list nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
        
        declare @contents table
        (
            id numeric primary key
        )
        
        insert into @contents
        select distinct content_id from inserted
        where CONTENT_ID in (select CONTENT_ID from content where virtual_type = 0) 
        
        create table #ids_with_splitted
        (
            id numeric primary key,
            new_splitted bit
        )
        
        declare @ids table
        (
            id numeric primary key,
            splitted bit,
            not_for_replication bit,
            cancel_split bit
        )
        
        while exists (select id from @contents)
        begin
            select @content_id = id from @contents
            
            insert into @ids
            select i.content_item_id, i.SPLITTED, i.not_for_replication, i.cancel_split from inserted i 
            inner join content_item ci on i.content_item_id = ci.content_item_id 
            where ci.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ',', '') + convert(nvarchar, id) from @ids
                        
            set @sql = 'insert into #ids_with_splitted ' 
            set @sql = @sql + ' select content_item_id,'
            set @sql = @sql + ' case' 
            set @sql = @sql + ' when curr_weight < front_weight and is_workflow_async = 1 then 1'
            set @sql = @sql + ' when curr_weight = workflow_max_weight and delayed = 1 then 1'
            set @sql = @sql + ' else 0'
            set @sql = @sql + ' end'
            set @sql = @sql + ' as new_splitted from ('
            set @sql = @sql + ' select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, '
            set @sql = @sql + ' max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, case when ci.cancel_split = 1 then 0 else ciw.is_async end as is_workflow_async, ' 
            set @sql = @sql + ' ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed '
            set @sql = @sql + ' from content_item ci'
            set @sql = @sql + ' inner join content_' + CONVERT(nvarchar, @content_id) + ' c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID'
            set @sql = @sql + ' left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id'
            set @sql = @sql + ' left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID'
            set @sql = @sql + ' left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID'
            set @sql = @sql + ' where ci.content_item_id in (' + @ids_list + ')) as main'
            print @sql
            exec sp_executesql @sql
            
            update base set base.splitted = i.new_splitted from @ids base inner join #ids_with_splitted i on base.id = i.id
            update base set base.splitted = i.splitted from content_item base inner join @ids i on base.CONTENT_ITEM_ID = i.id

            insert into content_item_splitted(content_item_id)
            select id from @ids base where splitted = 1 and not exists (select * from content_item_splitted cis where cis.content_item_id = base.id)

            delete from content_item_splitted where content_item_id in (
                select id from @ids base where splitted = 0 
            )
            
            set @sync_ids_list = null
            select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @ids where splitted = 0 and not_for_replication = 0
            set @async_ids_list = null
            select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @ids where splitted = 1 and not_for_replication = 0
            
            set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
            set @async_table_name = @table_name + '_async'
            
            if @sync_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
                
                exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
                print @sql
                exec sp_executesql @sql		
            end
            
            if @async_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
                
                exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql					
            end
            
            delete from #ids_with_splitted
                                    
            delete from @contents where id = @content_id
            
            delete from @ids
        end
        
        drop table #ids_with_splitted
        
    end
end
GO

insert into content_item_splitted 
select content_item_id from content_item ci where splitted = 1 and not exists(select * from content_item_splitted cis where cis.CONTENT_ITEM_ID = ci.content_item_id)
GO

exec sp_rename 'Ids', 'OldIds'
GO

CREATE TYPE [dbo].[Ids] AS TABLE(
    [ID] [numeric](18, 0) PRIMARY KEY NOT NULL
)
GO

ALTER function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as 
begin
    
    insert into @ids
    select id from @itemIds

    union 	
    select AGG_DATA.CONTENT_ITEM_ID
    from CONTENT_ATTRIBUTE ATT
    JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
    JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
    where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1
    and ATT.CONTENT_ID in (select content_id from content_item with(nolock) where content_item_id in (select id from @itemIds)) AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @itemIds)	

    return
end
GO

ALTER PROCEDURE [dbo].[qp_get_paged_data]
    @select_block nvarchar(max),
    @from_block nvarchar(max),
    @where_block nvarchar(max) = '',
    @order_by_block nvarchar(max),
    @count_only bit = 0,
    @total_records int OUTPUT,
    @start_row bigint = 0,
    @page_size bigint = 0,
    
    @use_security bit = 0,
    @user_id numeric(18,0) = 0,
    @group_id numeric(18,0) = 0,
    @start_level int = 2,
    @end_level int = 4,
    @entity_name nvarchar(100),
    @parent_entity_name nvarchar(100) = '',
    @parent_entity_id numeric(18,0) = 0,
    @itemIds Ids READONLY,
    @insert_key varchar(200) = '<$_security_insert_$>',
    @separate_count_query bit = 0
AS
BEGIN
    SET NOCOUNT ON
    
    -- Получаем фильтр по правам
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    IF (@use_security = 1)
        BEGIN
            EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @user_id,
                @group_id = @group_id,
                @start_level = @start_level,
                @end_level = @end_level,
                @entity_name = @entity_name,
                @parent_entity_name = @parent_entity_name,
                @parent_entity_id = @parent_entity_id,				
                @SQLOut = @security_sql OUTPUT
                
            SET @from_block = REPLACE(@from_block, @insert_key, @security_sql)
        END
        
    -- Получаем общее количество записей
    DECLARE @sql_count AS nvarchar(max)
    
    if (@count_only = 1 or @separate_count_query = 1)
    BEGIN
        SET @sql_count = ''
        SET @sql_count = @sql_count + 'SELECT ' + CHAR(13)
        SET @sql_count = @sql_count + '		@record_count = COUNT(*) ' + CHAR(13)
        SET @sql_count = @sql_count + '	FROM' + CHAR(13)
        SET @sql_count = @sql_count + @from_block + CHAR(13)
        IF (LEN(@where_block) > 0)
            BEGIN
                SET @sql_count = @sql_count + 'WHERE ' + CHAR(13)
                SET @sql_count = @sql_count + @where_block + CHAR(13)
            END


        EXEC sp_executesql 
            @sql_count, 
            N'@record_count int OUTPUT, @itemIds Ids READONLY',
            @itemIds = @itemIds, 
            @record_count = @total_records OUTPUT
    END
    
    -- Задаем номер начальной записи по умолчанию
    IF (@start_row <= 0)
        BEGIN
            SET @start_row = 1
        END
        
    -- Задаем номер конечной записи
    DECLARE @end_row AS bigint
    if (@page_size = 0)
        SET @end_row = 0			
    else
        SET @end_row = @start_row + @page_size - 1		
    
    IF (@count_only = 0)
        BEGIN
            -- Возвращаем результат
            DECLARE @sql_result AS nvarchar(max)
            
            SET @sql_result = ''		
            SET @sql_result = @sql_result + 'WITH PAGED_DATA_CTE' + CHAR(13)
            SET @sql_result = @sql_result + 'AS' + CHAR(13)
            SET @sql_result = @sql_result + '(' + CHAR(13)
            SET @sql_result = @sql_result + '	SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		c.*, ' + CHAR(13)
            SET @sql_result = @sql_result + '		ROW_NUMBER() OVER (ORDER BY ' + @order_by_block + ') AS ROW_NUMBER'
            if @separate_count_query = 0
                SET @sql_result = @sql_result + ', COUNT(*) OVER() AS ROWS_COUNT ' + CHAR(13)
            SET @sql_result = @sql_result + '	FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	( ' + CHAR(13)
            SET @sql_result = @sql_result + '		SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @select_block + CHAR(13)
            SET @sql_result = @sql_result + '		FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '		' + @from_block + CHAR(13)
            IF (LEN(@where_block) > 0)
                BEGIN
                    SET @sql_result = @sql_result + '		WHERE' + CHAR(13)
                    SET @sql_result = @sql_result + '		' + @where_block + CHAR(13)
                END
            SET @sql_result = @sql_result + '	) AS c ' + CHAR(13)
            SET @sql_result = @sql_result + ')' + CHAR(13) + CHAR(13)
            
            SET @sql_result = @sql_result + 'SELECT ' + CHAR(13)
            SET @sql_result = @sql_result + '	* ' + CHAR(13)
            SET @sql_result = @sql_result + 'FROM ' + CHAR(13)
            SET @sql_result = @sql_result + '	PAGED_DATA_CTE' + CHAR(13)
            IF (@end_row > 0 or @start_row > 1)
            BEGIN
                SET @sql_result = @sql_result + 'WHERE 1 = 1' + CHAR(13)
                IF @start_row > 1 
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER >= ' + CAST(@start_row AS nvarchar) + ' '
                IF @end_row > 0
                    SET @sql_result = @sql_result + ' AND ROW_NUMBER <= ' + CAST(@end_row AS nvarchar) + ' ' + CHAR(13)
            END	
            SET @sql_result = @sql_result + 'ORDER BY ' + CHAR(13)
            SET @sql_result = @sql_result + '	ROW_NUMBER ASC ' + CHAR(13)

            print(@sql_result)
            exec sp_executesql @sql_result, N'@itemIds Ids READONLY', @itemIds = @itemIds
        END
    
    SET NOCOUNT OFF
END
GO

DROP TYPE [dbo].[oldIds]
GO

ALTER TRIGGER [dbo].[ti_access_content_item] ON [dbo].[CONTENT_ITEM] FOR INSERT
AS
    if object_id('tempdb..#disable_ti_access_content_item') is null
    begin

      declare @ids table
      (
        content_item_id numeric primary key,
        last_modified_by numeric not null,
        content_id numeric not null
      )

      insert into @ids (content_item_id, last_modified_by, content_id)
      select content_item_id, i.last_modified_by, i.content_id from inserted i 
      inner join content c on i.CONTENT_ID = c.CONTENT_ID
      where c.allow_items_permission = 1

      INSERT INTO content_item_access 
        (content_item_id, user_id, permission_level_id, last_modified_by)
      SELECT
        content_item_id, last_modified_by, 1, 1
      FROM @ids i
      WHERE i.LAST_MODIFIED_BY <> 1

      INSERT INTO content_item_access 
        (content_item_id, user_id, group_id, permission_level_id, last_modified_by)
      SELECT
        i.content_item_id, ca.user_id, ca.group_id, ca.permission_level_id, 1 
      FROM content_access AS ca
        INNER JOIN @ids AS i ON ca.content_id = i.content_id
        LEFT OUTER JOIN user_group AS g ON g.group_id = ca.group_id
      WHERE
        (ca.user_id <> i.last_modified_by or ca.user_id IS NULL)
        AND (
            (g.shared_content_items = 0 and g.GROUP_ID <> 1) 
            OR (g.shared_content_items = 1 and g.GROUP_ID <> 1 
                and not exists(select * from USER_GROUP_BIND ug where ug.USER_ID = i.last_modified_by and ug.GROUP_ID = g.GROUP_ID)
            ) 
            OR g.group_id IS NULL
        )
        AND ca.propagate_to_items = 1

      INSERT INTO content_item_access 
        (content_item_id, group_id, permission_level_id, last_modified_by)
      SELECT DISTINCT
        i.content_item_id, g.group_id, 1, 1
      FROM @ids AS i
        LEFT OUTER JOIN user_group_bind AS gb ON gb.user_id = i.last_modified_by
        LEFT OUTER JOIN user_group AS g ON g.group_id = gb.group_id
      WHERE
        g.shared_content_items = 1 and g.GROUP_ID <> 1

    end
GO

IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'Values' AND ss.name = N'dbo')
CREATE TYPE [dbo].[Values] AS TABLE (
    [ArticleId] [int] NOT NULL,
    [ContentId] [int] NOT NULL,
    [FieldId] [int] NOT NULL,
    [Value] [nvarchar](max) NULL,
    PRIMARY KEY CLUSTERED 
    (
        [ContentId] ASC,
        [FieldId] ASC,
        [ArticleId] ASC
    ) WITH (IGNORE_DUP_KEY = OFF)
)
GO

ALTER function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as 
begin
    
    declare @ids2 Ids
    insert into @ids2
    select id from @itemIds i inner join content_item ci on i.ID = ci.CONTENT_ITEM_ID
    inner join CONTENT_ATTRIBUTE ca on ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1

    insert into @ids
    select id from @itemIds

    union 	
    select AGG_DATA.CONTENT_ITEM_ID
    from CONTENT_ATTRIBUTE ATT
    JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
    JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
    where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1
    and ATT.CONTENT_ID in (select content_id from content_item with(nolock) where content_item_id in (select id from @itemIds)) AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @ids2)	

    return
end
GO



if not exists (select * from BACKEND_ACTION where CODE = 'multiple_select_field_for_export')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL)
values('Multiple Select Fields For Export', 'multiple_select_field_for_export', dbo.qp_action_type_id('multiple_select'), dbo.qp_entity_type_id('field'),1, '~/Field/MultipleSelectForExport/')

if not exists (select * from BACKEND_ACTION where CODE = 'multiple_select_field_for_export_expanded')
insert into BACKEND_ACTION(NAME, CODE, TYPE_ID, ENTITY_TYPE_ID, IS_INTERFACE, CONTROLLER_ACTION_URL)
values('Multiple Select Fields For Export with Expansion', 'multiple_select_field_for_export_expanded', dbo.qp_action_type_id('multiple_select'), dbo.qp_entity_type_id('field'),1, '~/Field/MultipleSelectForExportExpanded/')

update BACKEND_ACTION set WINDOW_HEIGHT = 500 where name = 'Export Selected Articles' or name = 'Export Articles' 
GO


ALTER PROCEDURE [dbo].qp_copy_schedule_to_child_delays
@id numeric
AS
BEGIN
    if exists(select * from content_item_schedule where content_item_id = @id and freq_type = 2) 
    begin 
        update content_item_schedule set delete_job = 1 where use_service = 0 and content_item_id in (select child_id from child_delays where id = @id); 
        delete from content_item_schedule where content_item_id in (select child_id from child_delays where id = @id);
        insert into content_item_schedule (CONTENT_ITEM_ID, MAXIMUM_OCCURENCES, CREATED, MODIFIED, LAST_MODIFIED_BY, freq_type, freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor, active_start_date, active_end_date, active_start_time, active_end_time, occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE)
        select child_id, MAXIMUM_OCCURENCES, GETDATE(), GETDATE(), LAST_MODIFIED_BY, freq_type, freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor, active_start_date, active_end_date, active_start_time, active_end_time, occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE
        from content_item_schedule cis inner join child_delays cd on cis.content_item_id = cd.id where content_item_id = @id
    end
END
GO


ALTER PROCEDURE [dbo].[qp_content_list] 
    @site_id numeric,
    @user_id  numeric,
    @permission_level numeric,
    @order_by varchar(256) = 'content_id',
    @filter varchar(max) = '',
    @select varchar(max) = 'content.*, u.login'
AS

SET NOCOUNT ON

declare @strsql varchar(8000)
declare @SecuritySQL varchar(8000)
declare @use_security bit

IF (dbo.qp_is_user_admin(@user_id) = 1)
    set @use_security = 0
ELSE
    set @use_security = 1

if @use_security = 1	
Begin
    EXEC	dbo.qp_GetPermittedItemsAsQuery
            @user_id = @user_id,
            @group_id = 0,
            @start_level = @permission_level,
            @end_level = 4,
            @entity_name = 'content',
            @parent_entity_name = 'site',
            @parent_entity_id = @site_id,
            
            @SQLOut = @SecuritySQL OUTPUT        
End

SET NOCOUNT OFF

set @strsql = ' select ' + @select + ' from content inner join users as u on content.last_modified_by = u.user_id '

if @use_security = 1
begin
    set @strsql = @strsql + ' inner join (' + @SecuritySQL + ') as pi on content.content_id = pi.content_id and pi.hide = 0 ' 
end

set @strsql = @strsql + ' where content.site_id = ' + cast(@site_id as nvarchar) + ' '
 
if @filter <> '' 
    begin
        set @strsql = @strsql + ' and ' + @filter
    end
if @order_by <> '' 
    begin
        set @strsql = @strsql + ' order by ' + @order_by
    end
    
exec( @strsql )
GO


ALTER procedure [dbo].[qp_expand](
    @user_id numeric = 0, 
    @code nvarchar(50) = null, 
    @id bigint = 0, 
    @is_folder bit = 0, 
    @is_group bit = 0, 
    @group_item_code nvarchar(50) = null,
    @filter_id bigint = 0,
    @count_only bit = 0, 
    @count int = 0 output 
)
as
begin
    declare @result table
    (
        NUMBER int primary key identity(1, 1),
        ID bigint not null,
        PARENT_ID bigint null,
        PARENT_GROUP_ID bigint null,
        CODE nvarchar(50) null,
        TITLE nvarchar(255) not null,
        IS_FOLDER bit null,
        IS_GROUP bit null,
        GROUP_ITEM_CODE nvarchar(50),
        ICON nvarchar(255) null,
        ICON_MODIFIER nvarchar(10) null,
        CONTEXT_MENU_ID bigint null,
        CONTEXT_MENU_CODE nvarchar(50) null,
        DEFAULT_ACTION_ID bigint null,
        DEFAULT_ACTION_CODE nvarchar(50) null,
        HAS_CHILDREN bit null,
        IS_RECURRING bit null
    )
    
    declare @language_id numeric(18, 0)
    declare @source nvarchar(50), @id_field nvarchar(50), @title_field nvarchar(50)
    declare @parent_id_field nvarchar(50), @icon_field nvarchar(50), @group_parent_id_field nvarchar(50)
    declare @icon_modifier_field nvarchar(50), @order_field nvarchar(50)
    declare @folder_icon nvarchar(50), @has_item_nodes bit
    declare @recurring_id_field nvarchar(50), @source_sp nvarchar(50)
    declare @id_str nvarchar(10), @parent_id bigint, @filter_id_str nvarchar(10)
    declare @default_action_id int, @context_menu_id int
    declare @is_admin bit, @current_is_group bit
    declare @parent_group_code nvarchar(50), @child_group_code nvarchar(50), @current_group_item_code nvarchar(50)
    declare @real_parent_id bigint, @real_parent_id_field nvarchar(50), @real_id_str nvarchar(10)
    
    set @id_str = CAST(@id as nvarchar(10))
    
    if (@filter_id = 0)
        set @filter_id_str = ''
    else
        set @filter_id_str = CAST(@filter_id as nvarchar(10))
    
    select @parent_group_code = ET1.CODE from ENTITY_TYPE ET2 INNER JOIN ENTITY_TYPE ET1 ON ET2.GROUP_PARENT_ID = ET1.ID where ET2.CODE = @code
    and dbo.qp_check_entity_grouping(@user_id, @code) = 1
    
    if @is_group = 1
    begin
        exec dbo.qp_get_parent_entity_id @id, @code, @parent_entity_id = @real_parent_id output
        set @real_id_str = CAST(@real_parent_id as nvarchar(10))
    end
    else begin
        set @real_parent_id = @id
        set @real_id_str = @id_str	
    end
    
    set @current_is_group = 0
    if @parent_group_code is not null begin
        if @is_folder = 1 begin
            set @current_group_item_code = @code
            set @code = @parent_group_code
            set @current_is_group = 1
        end
    end
    else if @group_item_code is not null begin
        if @is_folder = 0 begin
            set @is_folder = 1
            set @code = @group_item_code
        end
    End

    set @language_id = dbo.qp_language(@user_id)
    
    set @is_admin = 0;
    IF EXISTS (select * from user_group_bind where group_id = 1 and user_id = @user_Id) OR @user_id = 1
        set @is_admin = 1;
    
    select 
        @source = source,
        @source_sp = source_sp,
        @id_field = id_field,
        @title_field = TITLE_FIELD, 
        @parent_id_field = PARENT_ID_FIELD,
        @group_parent_id_field = GROUP_PARENT_ID_FIELD,
        @icon_field = ICON_FIELD, 
        @icon_modifier_field = ICON_MODIFIER_FIELD, 
        @folder_icon = FOLDER_ICON, 
        @has_item_nodes = HAS_ITEM_NODES, 
        @recurring_id_field = RECURRING_ID_FIELD, 
        @order_field = order_field, 
        @default_action_id = default_action_id,
        @context_menu_id = CONTEXT_MENU_ID
    from 
        ENTITY_TYPE ET
    where
        ID = dbo.qp_entity_type_id(@code)
        
    if @is_group = 1 
    begin
        set @real_parent_id_field = @parent_id_field
        set @parent_id_field = @group_parent_id_field
    end
    
    if @icon_field is null
        set @icon_field = 'NULL'
    if @icon_modifier_field is null
        set @icon_modifier_field = 'NULL'	
    

    if @is_folder = 1 or @recurring_id_field is not null
    begin
        declare @sql nvarchar(max), @select nvarchar(max), @where nvarchar(max), @order nvarchar(max)

        if @has_item_nodes = 1
        begin
            set @select = @source + '.' + @id_field + ' AS ID, ' + @title_field + ' AS TITLE,  '  + @icon_field + ' AS ICON,  ' + @icon_modifier_field + ' AS ICON_MODIFIER'
            
            set @where = '1 = 1'
            if @parent_id_field is not null and @id_str <> '0' and  @id_str <> ''
                set @where = @where + ' AND ' + @parent_id_field + ' = ' + @id_str
            
            if @recurring_id_field is not null
            begin
                if @is_folder = 1 
                    set @where = @where + ' AND ' + @recurring_id_field + ' is null ' 
                else
                    set @where = @where + ' AND ' + @recurring_id_field + ' = ' + @id_str
            end
            
            if @filter_id_str <> '0' and @filter_id_str <> ''
                set @where = @where + ' AND ' + @source + '.' + @id_field + ' = ' + @filter_id_str
            
            if @order_field is null
                set @order = @title_field
            else
                set @order = @order_field  
            
        end
        
        
        if @source_sp is null
            set @sql = 'select ' + @select + ' from ' +  @source + ' where ' + @where + ' order by ' + @order
        else
        begin
            set @sql = 'exec ' + @source_sp + ' @user_id = ' + cast(@user_id as nvarchar(10)) + ', @permission_level = 1, @select = ''' + @select + ''', @filter = ''' + @where + ''', @order_by = ''' + @order + ''''
            if @real_parent_id_field is not null
                set @sql = @sql + ', @' + LOWER(@real_parent_id_field) + '=' + @real_id_str
            else if @parent_id_field is not null
                set @sql = @sql + ', @' + LOWER(@parent_id_field) + '=' + @id_str
            if @recurring_id_field is not null
                if @is_folder = 1
                    set @sql = @sql + ', @' + LOWER(@recurring_id_field) + '=0'
                else
                    set @sql = @sql + ', @' + LOWER(@recurring_id_field) + '=' + @id_str			
                
            
        end
        print @sql
        insert into @result (ID, TITLE, ICON, ICON_MODIFIER)
        exec sp_executesql @sql
        
        --PRINT @sql;
        
        
        if @count_only = 0
        begin
            --select ID, @id AS PARENT_ID, TITLE, @code AS CODE, 0 AS IS_FOLDER,  AS ICON, @default_action_id AS DEFAULT_ACTION_ID,  AS , dbo.qp_expand_count(@user_id, @code, ID, 0) AS CHILDREN_COUNT  from @result 
            update 
                @result
            set 
                PARENT_ID = @real_parent_id,
                PARENT_GROUP_ID = CASE WHEN @is_group = 1 THEN @id ELSE NULL END,
                CODE = @code, 
                IS_FOLDER = 0,
                IS_GROUP = @current_is_group,
                GROUP_ITEM_CODE = @current_group_item_code,
                ICON = dbo.qp_get_icon(ICON, @code, ICON_MODIFIER), 
                DEFAULT_ACTION_ID = @default_action_id, 
                CONTEXT_MENU_ID = @context_menu_id,
                IS_RECURRING = CASE WHEN @recurring_id_field is not null THEN 1 ELSE 0 END
        end
        else
            select @count = COUNT(ID) from @result	
    end
    else begin
        if @is_admin = 0
        begin
            declare @entitySecQuery nvarchar(max);
            EXEC [dbo].[qp_GetEntityPermissionAsQuery]
                @user_id = @user_id,	
                @SQLOut = @entitySecQuery OUTPUT
            
            CREATE TABLE #sectmp
            (
                PERMISSION_LEVEL int,
                ENTITY_TYPE_ID int,
                HIDE bit
            );				
            set @entitySecQuery = N'insert into #sectmp (PERMISSION_LEVEL, ENTITY_TYPE_ID, HIDE) ' + @entitySecQuery;
            exec sp_executesql @entitySecQuery;
        end
        
        declare @entitySql nvarchar(max), @condition nvarchar(max)
        set @condition = ' ET.DISABLED = 0 '
        if @code is null
            set @condition = @condition + ' AND ET.PARENT_ID is null '
        else
            set @condition = @condition + ' AND ET.PARENT_ID = dbo.qp_entity_type_id(''' + @code + ''') '
            
        if @is_admin = 0
            set @condition = @condition + ' AND S.PERMISSION_LEVEL > 0 '
            
        if @filter_id_str <> '0' and @filter_id_str <> ''
            set @condition = @condition + ' AND ET.ID = ' + @filter_id_str
            
        if @count_only = 0
        begin
            if @code is not null 
                set @entitySql = ' select ET.ID, ' + @id_str + ', dbo.qp_translate(dbo.qp_pluralize(ET.NAME), ' + cast(@language_id as nvarchar(10)) + '), ET.CODE, 1, 0, dbo.qp_get_icon(NULL, dbo.qp_pluralize(ET.CODE), NULL), ET.FOLDER_DEFAULT_ACTION_ID, ET.FOLDER_CONTEXT_MENU_ID ' + CHAR(13) 
            else
                set @entitySql = ' select ET.ID, ' + @id_str + ', ET.NAME, ET.CODE, 0, 0, dbo.qp_get_icon(NULL, ET.CODE, NULL), ET.DEFAULT_ACTION_ID, ET.CONTEXT_MENU_ID ' + CHAR(13) 
        end
        else
            set @entitySql = ' select @count = COUNT(ET.ID) ' + CHAR(13) 
        
        set @entitySql = @entitySql + ' From ENTITY_TYPE ET ' + CHAR(13)
        
        if @is_admin = 0
            set @entitySql = @entitySql + ' INNER JOIN #sectmp S ON S.ENTITY_TYPE_ID = ID and S.HIDE = 0 ' + CHAR(13)
            
        set @entitySql = @entitySql + ' WHERE ' + @condition  + CHAR(13)
        
        if @count_only = 0
        begin
            set @entitySql = @entitySql + ' order by ET.[ORDER] ' + CHAR(13)
            print @entitySql
            insert into @result(ID, PARENT_ID, TITLE, CODE, IS_FOLDER, IS_GROUP, ICON, DEFAULT_ACTION_ID, CONTEXT_MENU_ID)
            exec sp_executesql @entitySql
        end
        else begin
            print @entitySql
            exec sp_executesql @entitySql, N'@count int output', @count = @count output
        end
    end
    
    if @count_only = 0
    begin
        declare @i numeric, @total numeric
        declare @local_code nvarchar(50), @local_id numeric, @local_parent_id numeric, @local_is_folder bit, @local_is_recurring bit
        declare @local_is_group bit, @local_group_item_code nvarchar(50)
        declare @children_count int
        set @children_count = 0
        set @i = 1
        select @total = COUNT(NUMBER) from @result
        while @i <= @total
        begin
            select @local_code = code, @local_id = id, @local_parent_id = parent_id, @local_is_folder = is_folder, 
            @local_is_group = is_group, @local_is_recurring = is_recurring, @local_group_item_code = GROUP_ITEM_CODE from @result where NUMBER = @i
            
            if @local_is_folder = 1
                exec dbo.qp_expand @user_id, @local_code, @local_parent_id, 1, @local_is_group, @local_group_item_code, 0, 1, @count = @children_count output
            else
            begin
                if @i = 1 or @local_is_recurring = 1 or @local_is_group = 1
                begin
                    exec dbo.qp_expand @user_id, @local_code, @local_id, 0, @local_is_group, @local_group_item_code, 0, 1, @count = @children_count output
                end
            end
            if @children_count = 0
                update @result set has_children = 0 where NUMBER = @i
            else
                update @result set has_children = 1 where NUMBER = @i
            
            set @i = @i + 1
        end
        
        select 
            TREE_NODE.ID,
            TREE_NODE.CODE, 			
            TREE_NODE.PARENT_ID,
            TREE_NODE.PARENT_GROUP_ID,
            TREE_NODE.IS_FOLDER,
            TREE_NODE.IS_GROUP,
            TREE_NODE.GROUP_ITEM_CODE, 
            TREE_NODE.ICON, 
            TREE_NODE.TITLE, 
            dbo.qp_action_code(TREE_NODE.DEFAULT_ACTION_ID) AS DEFAULT_ACTION_CODE, 
            ACTION_TYPE.CODE AS DEFAULT_ACTION_TYPE_CODE,
            dbo.qp_context_menu_code(TREE_NODE.CONTEXT_MENU_ID) AS CONTEXT_MENU_CODE, 
            TREE_NODE.HAS_CHILDREN
        from
            @result AS TREE_NODE
        left outer join
            BACKEND_ACTION
        on
            TREE_NODE.DEFAULT_ACTION_ID = BACKEND_ACTION.ID	
        left outer join
            ACTION_TYPE
        on
            BACKEND_ACTION.TYPE_ID = ACTION_TYPE.ID	
    end
end
GO

ALTER PROCEDURE [dbo].[qp_GetEntityPermissionAsQuery]
    @user_id numeric(18,0),
    @SQLOut nvarchar(max) OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;
    
    declare @entitySecQuery nvarchar(max);
    
    EXEC	[dbo].[qp_GetPermittedItemsAsQuery]
            @user_id = @user_id,
            @start_level = 0,
            @end_level = 100,
            @entity_name = N'ENTITY_TYPE',
            @SQLOut = @entitySecQuery OUTPUT


    SELECT @SQLOut = REPLACE(		
        REPLACE(N'select COALESCE(L.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL, T.ID AS ENTITY_TYPE_ID, HIDE FROM 
            (<$_security_insert_$>) P1
            LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.[USER_ID] = <$_userid_$>
            RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
            LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL', N'<$_security_insert_$>', @entitySecQuery)
    , N'<$_userid_$>', @user_id)

END
GO


ALTER PROCEDURE [dbo].[qp_GetPermittedItemsAsQuery]
(
  @user_id numeric(18,0)=0,
  @group_id numeric(18,0)=0,
  @start_level int=2,
  @end_level int=4,
  @entity_name varchar(100)='content_item',
  @parent_entity_name varchar(100)='',
  @parent_entity_id numeric(18,0)=0,
  @SQLOut varchar(8000) OUTPUT
)
AS

SET NOCOUNT ON

Declare @sPermissionTable varchar(200)
Declare @sHide varchar(50)
Declare @NewLine char(2)
Declare @sUnion varchar(20)
Declare @sSelectUser varchar(200)
Declare @sSelectGroup varchar(8000)
Declare @sSQL varchar(8000)
Declare @srGroupInList varchar (30)
Declare @srLevelIncrement varchar (30)
Declare @sTemp varchar(8000)
Declare @sWhereParentEntity varchar (8000)
Declare @sDefaultSQL varchar (8000)
Declare @sGroupBy varchar (200)
Declare @intIncrement int 
Declare @CurrentLevelAddition int
Declare @sSQLStart varchar(300)
Declare @sSQLEnd varchar (600)

/***********************************/
/**** Declare Table Variables   ****/
/***********************************/
declare @ChildGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 

declare @ParentGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 

declare @UsedGroups table 
( 
    group_id numeric(18,0)
) 

declare @TempParentGroups table 
( 
    group_id numeric(18,0) PRIMARY KEY
) 
/***********************************/

select @NewLine = CHAR(13) + CHAR(10)
Select @intIncrement = 10
Select @CurrentLevelAddition = 0
Select @sSQLStart = ' select ' + @entity_name + '_id, cast(min(pl) as int)%10 as permission_level, max(hide) as hide from ('
Select @sSQLEnd = ') as qp_zzz group by qp_zzz.' + @entity_name + '_id HAVING cast(min(pl) as int)%10 >= ' + Cast(@start_level AS varchar) + ' AND cast(min(pl) as int)%10 <= ' + Cast(@end_level AS varchar)

Select @sGroupBy =  ' group by ' + @entity_name + '_id '
Select @sWhereParentEntity = '' 
select @sPermissionTable = @entity_name + '_access_PermLevel'

if @parent_entity_name != '' and @parent_entity_id != 0
Begin
   Select @sPermissionTable = @sPermissionTable + '_' + @parent_entity_name
   Select @sWhereParentEntity = ' and ' + @parent_entity_name+ '_id=' + Cast(@parent_entity_id As varchar) + ' '
End

if @entity_name = 'content'
    set @sHide = ', MAX(CONVERT(int, hide)) as hide'
else
    set @sHide = ', 0 as hide'

select @sSQL = ''
select @sTemp = null
Select @srGroupInList = '<@_group_in_list_@>'
Select @srLevelIncrement = '<@_increment_level_@>'
select @sUnion = @NewLine + ' Union All ' + @NewLine
select @sSelectUser = ' select ' + @entity_name + '_id, max(permission_level) as pl' + @sHide + ' from ' + @sPermissionTable +  ' where user_id=' + Cast(@user_id AS varchar) + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sSelectGroup = ' select ' + @entity_name + '_id, max(permission_level) + ' + @srLevelIncrement + ' as pl' + @sHide + ' from ' + @sPermissionTable +  ' where group_id in (' + @srGroupInList + ')' + @NewLine
                      + @sWhereParentEntity + @NewLine
select @sDefaultSQL = ' select 0 as ' + @entity_name + '_id, 0 as permission_level' + @sHide + ' from ' + @sPermissionTable


if @user_id > 0
Begin
   Select @sSQL = @sSelectUser + @sGroupBy
   insert into @ChildGroups (group_id) select distinct group_id from user_group_bind where user_id = @user_id 
   Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
End

if @group_id > 0 AND @user_id <= 0
Begin
   insert into @ChildGroups(group_id) values (@group_id)
End

if (select count(*) from @ChildGroups) = 0
Begin
   if @sSQL != '' Select @SQLOut = @sSQL
   else Select @SQLOut = @sDefaultSQL
   return
End 

SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @ChildGroups  
if @sSQL != '' Select @sSQL = @sSQL + @sUnion
Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp ) 
Select @sSQL = @sSQL + @sGroupBy

insert into @UsedGroups(group_id) select group_id from @ChildGroups

WHILE 1=1
BEGIN
    Select @CurrentLevelAddition = @CurrentLevelAddition + @intIncrement
    select @sTemp = null
    insert into @ParentGroups (group_id) select distinct gtg.parent_group_id from group_to_group gtg inner join @ChildGroups cg on gtg.child_group_id = cg.group_id
    if (select count(*) from @ParentGroups) = 0 BREAK

    /* need to check that parent groups are not appearing in child groups */
    insert into @TempParentGroups (group_id) select pg.group_id from @ParentGroups pg where pg.group_id not in(select cg.group_id from @ChildGroups cg) and pg.group_id not in (select group_id from @UsedGroups)
    if (select count(*) from @TempParentGroups) != 0
    Begin
        SELECT @sTemp = COALESCE(@sTemp + ', ', '') + CAST(group_id AS varchar) FROM @TempParentGroups
        if @sSQL != '' Select @sSQL = @sSQL + @sUnion
        Select @sSQL = @sSQL + Replace( Replace(@sSelectGroup,@srLevelIncrement,@CurrentLevelAddition), @srGroupInList, @sTemp )
        Select @sSQL = @sSQL + @sGroupBy
        insert into @UsedGroups (group_id) select group_id from @TempParentGroups
    End

    delete @ChildGroups
    delete @TempParentGroups
    insert into @ChildGroups (group_id) select (group_id) from @ParentGroups
    delete @ParentGroups
    CONTINUE
END

Select @SQLOut = @sSQLStart + @sSQL + @sSQLEnd
return

GO


ALTER PROCEDURE [dbo].[qp_replicate_items] 
@ids nvarchar(max),
@attr_ids nvarchar(max) = ''
AS
BEGIN
    set nocount on
    
    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        cancel_split bit,
        status_type_id numeric,
        content_id numeric
    )
    
    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')
    
    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.cancel_split = ci.CANCEL_SPLIT, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID 

    declare @contents table
    (
        id numeric primary key
    )
    
    insert into @contents
    select distinct content_id from @articles
    
    while exists (select id from @contents)
    begin
        select @content_id = id from @contents
        
        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 0
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @articles where content_id = @content_id and splitted = 1
        
        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'
        
        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0, @attr_ids		
        end
        
        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1, @attr_ids							
        end
        
        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and (cancel_split = 1 or (splitted = 0 and status_type_id = @published_id)))
            update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        else
            update content_modification set stage_modified = GETDATE() where content_id = @content_id	

        
        delete from @contents where id = @content_id
    end
    
    set @sql = 'update content_item set not_for_replication = 0 where content_item_id in (' + @ids + ' )'
    print @sql
    exec sp_executesql @sql
END
GO

ALTER  TRIGGER [dbo].[td_delete_item] ON [dbo].[CONTENT_ITEM] FOR DELETE AS BEGIN
    
    if object_id('tempdb..#disable_td_delete_item') is null
    begin
    
        declare @content_id numeric, @virtual_type numeric, @published_id numeric
        declare @sql nvarchar(max)
        declare @ids_list nvarchar(max)


        declare @c table (
            id numeric primary key,
            virtual_type numeric
        )
        
        insert into @c
        select distinct d.content_id, c.virtual_type
        from deleted d inner join content c 
        on d.content_id = c.content_id
        
        declare @ids table
        (
            id numeric primary key,
            char_id nvarchar(30),
            status_type_id numeric,
            splitted bit
        )
        
                    
        declare @attr_ids table
        (
            id numeric primary key
        )

        while exists(select id from @c)
        begin
            
            select @content_id = id, @virtual_type = virtual_type from @c
            
            insert into @ids
            select content_item_id, CONVERT(nvarchar, content_item_id), status_type_id, splitted from deleted where content_id = @content_id

            insert into @attr_ids
            select ca1.attribute_id from CONTENT_ATTRIBUTE ca1 
            inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID 
            where ca2.CONTENT_ID = @content_id
            
            set @ids_list = null
            select @ids_list = coalesce(@ids_list + ', ', '') + char_id from @ids
            
            select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
            if exists (select * from @ids where status_type_id = @published_id and splitted = 0)
                update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
            else
                update content_modification set stage_modified = GETDATE() where content_id = @content_id	
        
            /* Drop relations to current item */
            if exists(select id from @attr_ids) and object_id('tempdb..#disable_td_delete_item_o2m_nullify') is null
            begin
                UPDATE content_attribute SET default_value = null 
                    WHERE attribute_id IN (select id from @attr_ids) 
                    AND default_value IN (select char_id from @ids)
            
                UPDATE content_data SET data = NULL, blob_data = NULL 
                    WHERE attribute_id IN (select id from @attr_ids)
                    AND data IN (select char_id from @ids)
                    
                DELETE from VERSION_CONTENT_DATA
                    where ATTRIBUTE_ID in (select id from @attr_ids)
                    AND data IN (select char_id from @ids)	
            end
            
            if @virtual_type = 0 
            begin 		
                exec qp_get_delete_items_sql @content_id, @ids_list, 0, @sql = @sql out
                exec sp_executesql @sql
            
                exec qp_get_delete_items_sql @content_id, @ids_list, 1, @sql = @sql out
                exec sp_executesql @sql
            end

            delete from @c where id = @content_id
            
            delete from @ids
            
            delete from @attr_ids
        end
    end
END
GO


exec qp_drop_existing 'qp_get_upsert_items_sql_new', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_upsert_items_sql_new]
@table_name nvarchar(25), 
@sql nvarchar(max) output 
as
BEGIN
    set @sql = 'update base set '
    set @sql = @sql + ' base.modified = ci.modified, base.last_modified_by = ci.last_modified_by, base.status_type_id = ci.status_type_id, '
    set @sql = @sql + ' base.visible = ci.visible, base.archive = ci.archive '
    set @sql = @sql + ' from ' + @table_name + ' base with(rowlock) '
    set @sql = @sql + ' inner join content_item ci with(rowlock) on base.content_item_id = ci.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id'
    set @sql = @sql + ';' + CHAR(13) + CHAR(10) 
    
    set @sql = @sql + 'insert into ' + @table_name + ' (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)'
    set @sql = @sql + ' select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by, '
    set @sql = @sql + ' case when i2.id is not null then @noneId else ci.status_type_id end as status_type_id, '
    set @sql = @sql + ' ci.visible, ci.archive '
    set @sql = @sql + ' from content_item ci left join ' + @table_name + ' base on ci.content_item_id = base.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id '
    set @sql = @sql + ' left join @ids2 i2 on ci.content_item_id = i2.id '
    set @sql = @sql + ' where base.content_item_id is null'
END
GO

ALTER TRIGGER [dbo].[tu_update_item] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
begin
    if not update(locked_by) and not update(splitted) and not UPDATE(not_for_replication)
    begin
        declare @content_id numeric
        declare @sql nvarchar(max), @table_name varchar(50), @async_table_name varchar(50)
        declare @items_list nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
        
        declare @contents table
        (
            id numeric primary key
        )
        
        insert into @contents
        select distinct content_id from inserted
        where CONTENT_ID in (select CONTENT_ID from content where virtual_type = 0) 
        
        create table #ids_with_splitted
        (
            id numeric primary key,
            new_splitted bit
        )
        
        declare @items table
        (
            id numeric primary key,
            splitted bit,
            not_for_replication bit,
            cancel_split bit
        )

        declare @ids [Ids], @ids2 [Ids]
        
        while exists (select id from @contents)
        begin
            select @content_id = id from @contents
            
            insert into @items
            select i.content_item_id, i.SPLITTED, i.not_for_replication, i.cancel_split from inserted i 
            inner join content_item ci on i.content_item_id = ci.content_item_id 
            where ci.CONTENT_ID = @content_id

            insert into @ids
            select id from @items

            insert into @ids2
            select id from @items where cancel_split = 1
            
            set @items_list = null
            select @items_list = coalesce(@items_list + ',', '') + convert(nvarchar, id) from @items
                        
            set @sql = 'insert into #ids_with_splitted ' 
            set @sql = @sql + ' select content_item_id,'
            set @sql = @sql + ' case' 
            set @sql = @sql + ' when curr_weight < front_weight and is_workflow_async = 1 then 1'
            set @sql = @sql + ' when curr_weight = workflow_max_weight and delayed = 1 then 1'
            set @sql = @sql + ' else 0'
            set @sql = @sql + ' end'
            set @sql = @sql + ' as new_splitted from ('
            set @sql = @sql + ' select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, '
            set @sql = @sql + ' max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, case when i2.id is not null then 0 else ciw.is_async end as is_workflow_async, ' 
            set @sql = @sql + ' ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed '
            set @sql = @sql + ' from content_item ci'
            set @sql = @sql + ' inner join @ids i on i.id = ci.content_item_id'
            set @sql = @sql + ' left join @ids2 i2 on i.id = ci.content_item_id'
            set @sql = @sql + ' inner join content_' + CONVERT(nvarchar, @content_id) + ' c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID'
            set @sql = @sql + ' left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id'
            set @sql = @sql + ' left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID'
            set @sql = @sql + ' left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID'
            set @sql = @sql + ' ) as main'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY', @ids = @ids, @ids2 = @ids2
            
            update base set base.splitted = i.new_splitted from @items base inner join #ids_with_splitted i on base.id = i.id
            update base set base.splitted = i.splitted from content_item base inner join @items i on base.CONTENT_ITEM_ID = i.id

            insert into content_item_splitted(content_item_id)
            select id from @items base where splitted = 1 and not exists (select * from content_item_splitted cis where cis.content_item_id = base.id)

            delete from content_item_splitted where content_item_id in (
                select id from @items base where splitted = 0 
            )
            
            set @sync_ids_list = null
            select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 0 and not_for_replication = 0
            set @async_ids_list = null
            select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 1 and not_for_replication = 0
            
            set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
            set @async_table_name = @table_name + '_async'
            
            if @sync_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
                
                exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
                print @sql
                exec sp_executesql @sql		
            end
            
            if @async_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
                
                exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql					
            end
            
            delete from #ids_with_splitted
                                    
            delete from @contents where id = @content_id
            
            delete from @items
            delete from @ids
            delete from @ids2
        end
        
        drop table #ids_with_splitted
        
    end
end
GO

ALTER TRIGGER [dbo].[ti_insert_item] ON [dbo].[CONTENT_ITEM] FOR INSERT AS
BEGIN
    declare @content_id numeric
    declare @ids_list nvarchar(max)

    declare @table_name varchar(50), @sql nvarchar(max)
    
    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )
        
    insert into @contents
    select distinct i.content_id, st.STATUS_TYPE_ID from inserted i 
    inner join content c on i.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID
    
    declare @ids [Ids]
    declare @ids2 [Ids]
    declare @noneId numeric

    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents
        
        insert into @ids
        select i.content_item_id from inserted i 
        where i.CONTENT_ID = @content_id and i.not_for_replication = 0

        if exists (select id from @ids)
        begin

            insert into @ids2
            select i.content_item_id from inserted i 
            where i.CONTENT_ID = @content_id and i.not_for_replication = 0 and i.SCHEDULE_NEW_VERSION_PUBLICATION = 1

            set @table_name = 'content_' + convert(nvarchar, @content_id)
        
            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @ids, @ids2 = @ids2, @noneId = @noneId

            delete from @ids2
            delete from @ids

        end
        
        delete from @contents where id = @content_id

    end
 
END
GO

ALTER PROCEDURE [dbo].[qp_replicate_items] 
@ids nvarchar(max),
@attr_ids nvarchar(max) = ''
AS
BEGIN
    set nocount on
    
    declare @sql nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)
    declare @table_name nvarchar(50), @async_table_name nvarchar(50)

    declare @content_id numeric, @published_id numeric

    declare @articles table
    (
        id numeric primary key,
        splitted bit,
        cancel_split bit,
        delayed bit,
        status_type_id numeric,
        content_id numeric
    )
    
    insert into @articles(id) SELECT convert(numeric, nstr) from dbo.splitNew(@ids, ',')
    
    update base set base.content_id = ci.content_id, base.splitted = ci.SPLITTED, base.cancel_split = ci.cancel_split, base.delayed = ci.schedule_new_version_publication, base.status_type_id = ci.STATUS_TYPE_ID from @articles base inner join content_item ci on base.id = ci.CONTENT_ITEM_ID 

    declare @contents table
    (
        id numeric primary key,
        none_id numeric
    )
    
    insert into @contents
    select distinct a.content_id, st.STATUS_TYPE_ID from @articles a
    inner join content c on a.CONTENT_ID = c.CONTENT_ID and c.virtual_type = 0
    inner join STATUS_TYPE st on st.STATUS_TYPE_NAME = 'None' and st.SITE_ID = c.SITE_ID


    declare @articleIds [Ids], @syncIds [Ids], @syncIds2 [Ids], @asyncIds [Ids], @asyncIds2 [Ids]
    declare @noneId numeric

    insert into @articleIds select id from @articles
    
    while exists (select id from @contents)
    begin
        select @content_id = id, @noneId = none_id from @contents

        insert into @syncIds select id from @articles where content_id = @content_id and splitted = 0
        insert into @asyncIds select id from @articles where content_id = @content_id and splitted = 1
        insert into @syncIds2 select id from @articles where content_id = @content_id and splitted = 0 and delayed = 1

        set @sync_ids_list = null
        select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @syncIds
        set @async_ids_list = null
        select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @asyncIds
        
        set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
        set @async_table_name = @table_name + '_async'
        
        if @sync_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @syncIds, @ids2 = @syncIds2, @noneId = @noneId
            
            exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @sync_ids_list, 0, @attr_ids
        end

        if @async_ids_list <> ''
        begin
            exec qp_get_upsert_items_sql_new @async_table_name, @sql = @sql out
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY, @noneId numeric', @ids = @asyncIds, @ids2 = @asyncIds2, @noneId = @noneId
            
            exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
            print @sql
            exec sp_executesql @sql
            
            exec qp_update_items_with_content_data_pivot @content_id, @async_ids_list, 1, @attr_ids
        end
        
        select @published_id = status_type_id from STATUS_TYPE where status_type_name = 'Published' and SITE_ID in (select SITE_ID from content where CONTENT_ID = @content_id)
        if exists (select * from @articles where content_id = @content_id and (cancel_split = 1 or (splitted = 0 and status_type_id = @published_id)))
            update content_modification set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @content_id
        else
            update content_modification set stage_modified = GETDATE() where content_id = @content_id	

        
        delete from @contents where id = @content_id

        delete from @syncIds
        delete from @syncIds2
        delete from @asyncIds
    end
    
    update content_item set not_for_replication = 0, CANCEL_SPLIT = 0 where content_item_id in (select id from @articleIds)
END



INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.9.0', 'Copyright &copy; 1998-2014 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.9.0 completed'
GO


