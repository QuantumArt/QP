ALTER function [dbo].[qp_get_version_data](@attribute_id numeric, @version_id numeric) returns nvarchar(max)
as
begin
	declare @result nvarchar(max)
	select @result = convert(nvarchar(max), coalesce(cd.BLOB_DATA, cd.DATA)) from version_content_data cd inner join CONTENT_ATTRIBUTE ca on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID where cd.attribute_id = @attribute_id and content_item_version_id = @version_id
	return @result
end
GO