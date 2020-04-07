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
	dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ca.attribute_type_id, ca.attribute_size, ca.default_value) as pivot_data
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