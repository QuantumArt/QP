exec qp_drop_existing 'qp_m2o_titles', 'IsScalarFunction'
GO

CREATE FUNCTION [dbo].[qp_m2o_titles](@id int, @field_related_id int, @related_attribute_id int, @maxlength int)
RETURNS nvarchar(max)
AS
BEGIN
	declare @names table
	(
		name nvarchar(255)
	)
	declare @result nvarchar(max)

	insert into @names
	select coalesce(data, blob_data) from CONTENT_DATA where attribute_id = @field_related_id
	and content_item_id in (select content_item_id from content_data where ATTRIBUTE_ID = @related_attribute_id and o2m_data = @id)

	SELECT @result = COALESCE(@result + ', ', '') +  name  FROM @names

	if @result is null
		set @result = ''

	if (@maxlength > 0 and len(@result) > @maxlength)
		set @result = SUBSTRING(@result, 1, @maxlength) + '...'

	return @result

END
GO
