ALTER PROCEDURE [dbo].[qp_get_update_column_sql]
@table_name nvarchar(255),
@content_item_ids nvarchar(max),
@attribute_id numeric,
@attribute_type_id numeric,
@attribute_size numeric,
@default_value nvarchar(255),
@attribute_name nvarchar(255),
@sql nvarchar(max) output
AS
BEGIN
	declare @source_function nvarchar(512)
	set @source_function = 'dbo.qp_correct_data( cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ' + convert(nvarchar, @attribute_type_id) + ', ' + convert(nvarchar, @attribute_size) + ', N''' + isnull(@default_value, '') + ''')'

	set @sql = ' update ' + @table_name + ' set [' + @attribute_name + '] = ' + @source_function + ' from content_data cd '
	set @sql = @sql + ' where ' + @table_name + '.content_item_id in (' + @content_item_ids + ')'
	set @sql = @sql + ' and cd.attribute_id = ' + convert(nvarchar, @attribute_id) + ' and cd.content_item_id = ' + @table_name + '.content_item_id '
END
GO