ALTER TRIGGER [dbo].[ti_insert_field] ON [dbo].[CONTENT_ATTRIBUTE] FOR INSERT
AS
BEGIN
	if object_id('tempdb..#disable_ti_insert_field') is null
	begin
		declare @attribute_id numeric, @attribute_name nvarchar(255), @attribute_size numeric, @content_id numeric
		declare @indexed numeric, @required numeric
		declare @attribute_type_id numeric, @type_name nvarchar(255), @database_type nvarchar(255)

		declare @base_table_name nvarchar(30), @table_name nvarchar(30)

		declare @i numeric, @count numeric, @max numeric

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
			content_id numeric
		)

		/* Collect affected items */
		insert into @ca (attribute_id, attribute_name, attribute_size, indexed, required, attribute_type_id, type_name, database_type, content_id)
			select i.attribute_id, i.attribute_name, i.attribute_size, i.index_flag, i.required, i.attribute_type_id, at.type_name, at.database_type, i.content_id
			from inserted i
			inner join attribute_type at on i.attribute_type_id = at.attribute_type_id
			inner join content c on i.content_id = c.content_id
			where c.virtual_type = 0

		set @i = 1
		select @count = count(id) from @ca

		while @i < @count + 1
		begin
			select @attribute_id = attribute_id, @attribute_name = attribute_name, @attribute_size = attribute_size,
				@indexed = indexed, @required = required, @attribute_type_id = attribute_type_id,
				@type_name = type_name, @database_type = database_type, @content_id = content_id
				from @ca where id = @i

				set @base_table_name = 'content_' + convert(nvarchar, @content_id)

				IF NOT EXISTS(SELECT * FROM sysobjects WHERE id = OBJECT_ID(@base_table_name) AND OBJECTPROPERTY(id, 'IsUserTable') = 1)
				begin
					exec qp_rebuild_content @content_id
				end
				else begin

					/* Add column in common and async tables */
					set @table_name = @base_table_name + '_ASYNC'
					exec qp_add_column @base_table_name, @attribute_name, @type_name, @database_type, @attribute_size
					exec qp_add_column @table_name, @attribute_name, @type_name, @database_type, @attribute_size

					/* Create indexes on new fields if required */
					if @indexed = 1
					begin
						exec qp_add_index @base_table_name, @attribute_name
						exec qp_add_index @table_name, @attribute_name
					end

					/* Recreate United View */
					exec qp_content_united_view_recreate @content_id
					exec qp_content_frontend_views_recreate @content_id
				end
			set @i = @i + 1
		end
	END
END
GO
