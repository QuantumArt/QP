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
	cast(coalesce(vcd.blob_data, vcd.data) as nvarchar(max)) as pivot_data
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
