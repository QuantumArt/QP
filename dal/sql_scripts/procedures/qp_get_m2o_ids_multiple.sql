exec qp_drop_existing 'qp_get_m2o_ids_multiple', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_m2o_ids_multiple]
@contentId numeric,
@fieldName nvarchar(255),
@ids [Ids] READONLY
AS
BEGIN
  declare @sql nvarchar(max)
  set @sql = 'select [' + @fieldName + '], content_item_id from content_' + CAST(@contentId as nvarchar(255)) + '_united where [' + @fieldName + '] in (select id from @ids)'
  exec sp_executesql @sql, N'@ids [Ids] READONLY', @ids = @ids
END

GO
