
ALTER  TRIGGER [dbo].[tu_update_field] ON [dbo].[CONTENT_ATTRIBUTE] FOR UPDATE
AS
BEGIN
if not update(attribute_order) and object_id('tempdb..#disable_tu_update_field') is null and
		(
			update(attribute_name) or update(attribute_type_id)
			or update(attribute_size) or update(index_flag) or update(is_long)
		)
	begin
		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric, @content_id numeric
		declare @indexed numeric, @required numeric, @is_long bit
		declare @attribute_type_id numeric, @type_name nvarchar(255), @database_type nvarchar(255)

		declare @new_attribute_name nvarchar(255), @new_attribute_size numeric
		declare @new_indexed numeric, @new_required numeric, @new_is_long bit
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
			link_id numeric,
			is_long bit
		)

	/* Collect affected items */
		insert into @ca (attribute_id, attribute_name, attribute_size, indexed, required, attribute_type_id, type_name, database_type, content_id, related_content_id, link_id, is_long)
			select d.attribute_id, d.attribute_name, d.attribute_size, d.index_flag, d.required, d.attribute_type_id, at.type_name, at.database_type, d.content_id,
			isnull(ca1.content_id, 0), isnull(d.link_id, 0), d.is_long
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
				@related_content_id = related_content_id, @link_id = link_id, @is_long = is_long
				from @ca where id = @i

			select @new_attribute_name = ca.attribute_name, @new_attribute_size = ca.attribute_size,
				@new_indexed = ca.index_flag, @new_required = ca.required, @new_attribute_type_id = ca.attribute_type_id,
				@new_type_name = at.type_name, @new_database_type = at.database_type,
				@new_related_content_id = isnull(ca1.content_id, 0), @new_link_id = isnull(ca.link_id, 0), @new_is_long = ca.IS_LONG
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
				else if @attribute_name <> @new_attribute_name
				begin
					exec qp_rename_column @base_table_name, @attribute_name, @new_attribute_name, @preserve_index
					exec qp_rename_column @table_name, @attribute_name, @new_attribute_name, @preserve_index
					exec qp_content_united_view_recreate @content_id
					exec qp_content_frontend_views_recreate @content_id
				end
				else if @is_long <> @new_is_long
				begin
					exec qp_content_frontend_views_recreate @content_id
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
