
ALTER TRIGGER [dbo].[td_item_to_item] ON [dbo].[item_to_item] AFTER DELETE
AS 
BEGIN
	if object_id('tempdb..#disable_td_item_to_item') is null
	begin
		delete item_to_item from item_to_item ii 
			inner join deleted d on ii.link_id = d.link_id and ii.l_item_id = d.r_item_id and ii.r_item_id = d.l_item_id
			inner join content_to_content c2c on d.link_id = c2c.link_id
			where c2c.[symmetric] = 1
	end

	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct d.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from deleted d inner join content_to_content c2c on d.link_id = c2c.link_id
	
		declare @count numeric
		select @count = count(*) from @links
		print (@count) 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct l_item_id, r_item_id from deleted where link_id = @link_id

		if @is_symmetric = 1
		begin
			insert into @link_items
			select distinct r_item_id, l_item_id from deleted d where link_id = @link_id
			and not exists (select * from @link_items where id = d.r_item_id and linked_id = d.l_item_id)
		end

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 

		exec qp_delete_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
		exec qp_delete_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO