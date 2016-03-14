-- ************************************** 
-- Pavel Celut
-- version 7.5.7.0
-- Label
-- **************************************

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.5.7.0', 'Copyright &copy; 1998-2007 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.5.7.0 completed'
GO

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

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.5.7.2', 'Copyright &copy; 1998-2007 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.5.7.2 completed'
GO

-- ************************************** 
-- Pavel Celut
-- version 7.9.1.0
-- Release
-- **************************************

-- ************************************** 
-- Max Tertyshnyy
-- version 7.9.0.28
-- qp_get_version_data fix
-- translation
-- **************************************

exec qp_drop_existing 'qp_get_version_data', 'IsScalarFunction'
GO

CREATE function [dbo].[qp_get_version_data](@attribute_id numeric, @version_id numeric) returns nvarchar(max) 
as 
	begin 
	declare @result nvarchar(max) 
	select @result = (case when attribute_type_id in (9, 10) THEN convert(nvarchar(max), cd.BLOB_DATA) ELSE cd.DATA end) from version_content_data cd inner join CONTENT_ATTRIBUTE ca on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID where cd.attribute_id = @attribute_id and content_item_version_id = @version_id 

	return @result 
end
GO


exec qp_update_translations 'Custom Actions', 'Действия'
exec qp_update_translations 'New Custom Action', 'Новое действие'
go

exec qp_update_translations 'Relation Many-to-One', 'Связь "многие-к-одному"'
go

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.0.28', 'Copyright &copy; 1998-2012 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.0.28 completed'
GO


-- ************************************** 
-- Pavel Celut
-- version 7.9.1.0
-- Release
-- **************************************

INSERT INTO SYSTEM_INFO
  (field_name, field_value, copyright)
VALUES
  ('version', '7.9.1.0', 'Copyright &copy; 1998-2012 Quantum Art, Inc. All rights reserved.')
GO

PRINT '7.9.1.0 completed'
GO