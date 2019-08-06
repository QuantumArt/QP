
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
	    dbo.qp_correct_data(cast(coalesce(cd.blob_data, cd.data) as nvarchar(max)), ca.attribute_type_id, ca.attribute_size, ca.default_value) as pivot_data
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
