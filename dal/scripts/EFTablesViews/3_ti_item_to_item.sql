
ALTER TRIGGER [dbo].[ti_item_to_item] ON [dbo].[item_to_item] AFTER INSERT
AS 
BEGIN

if object_id('tempdb..#disable_ti_item_to_item') is null
	begin
		with items (link_id, item_id, linked_item_id)
		AS
		(
			select i1.link_id, i1.l_item_id, i1.r_item_id From inserted i1
			inner join content_to_content c2c on i1.link_id = c2c.link_id 
			where c2c.[symmetric] = 1 and not exists (select * from item_to_item i2 where i1.link_id = i2.link_id and i1.r_item_id = i2.l_item_id and i2.r_item_id = i1.l_item_id)
		)
		insert into item_to_item(link_id, l_item_id, r_item_id)
		select link_id, linked_item_id, item_id from items
	end

	declare @links table
	(
		id numeric primary key,
		is_symmetric bit,
		l_content_id numeric,
		r_content_id numeric
	)
			
	insert into @links 
	select distinct i.link_id, c2c.[SYMMETRIC], c2c.l_content_id, c2c.r_content_id from inserted i inner join content_to_content c2c on i.link_id = c2c.link_id 

	declare @link_id numeric, @is_symmetric bit, @l_content_id numeric, @r_content_id numeric

	while exists(select id from @links)
	begin
			
		declare @link_items [Links]

		select @link_id = id, @is_symmetric = is_symmetric, @l_content_id = l_content_id, @r_content_id = r_content_id from @links

		insert into @link_items
		select distinct l_item_id, r_item_id from inserted where link_id = @link_id

		if @is_symmetric = 1
		begin
			insert into @link_items
			select r_item_id, l_item_id from inserted i where link_id = @link_id
			and not exists (select * from @link_items where id = i.r_item_id and linked_id = i.l_item_id)
		end

		declare @self_related bit
		select @self_related = case when @r_content_id = @l_content_id then 1 else 0 end 


		exec qp_insert_link_table_item @link_id, @l_content_id, @link_items, 0, 0, 0
		exec qp_insert_link_table_item @link_id, @r_content_id, @link_items, 0, 1, @self_related

		delete from @link_items

		delete from @links where id = @link_id
		
	end
END
GO

