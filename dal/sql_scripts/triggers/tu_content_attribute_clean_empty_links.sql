ALTER TRIGGER [dbo].[tu_content_attribute_clean_empty_links] ON [dbo].[CONTENT_ATTRIBUTE] FOR UPDATE
AS
BEGIN
	if update(link_id) and object_id('tempdb..#disable_tu_content_attribute_clean_empty_links') is null
	begin
		declare @link_id numeric, @attribute_id numeric, @version numeric
		declare @i numeric, @count numeric
		declare @links table (
			id numeric identity(1,1) primary key,
			link_id numeric,
			attribute_id numeric
		)

		insert into @links (link_id, attribute_id)
		select d.link_id, d.attribute_id from deleted d inner join inserted i on d.attribute_id = i.attribute_id where d.link_id IS NOT NULL AND (i.link_id IS NULL OR i.link_id <> d.link_id) 

		set @i = 1
		select @count = count(id) from @links
		set @version = dbo.qp_get_version_control()		

		while @i < @count + 1
		begin
			select @link_id = link_id, @attribute_id = attribute_id from @links where id = @i
			
			exec qp_drop_link_with_check @link_id
			
			if @version is not null
			   DELETE FROM item_to_item_version WHERE attribute_id = @attribute_id
			
			set @i = @i + 1
		end
	end
END
GO