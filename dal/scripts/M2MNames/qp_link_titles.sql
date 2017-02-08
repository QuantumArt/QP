
ALTER function [dbo].[qp_link_titles](@link_id int, @id int, @display_attribute_id int, @maxlength int)
returns nvarchar(max)
AS
BEGIN

  declare @names table
  (
    name nvarchar(255)
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
