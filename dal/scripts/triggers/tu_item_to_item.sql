exec qp_drop_existing 'tu_item_to_item', 'IsTrigger'
go

CREATE TRIGGER [dbo].[tu_item_to_item] ON [dbo].[item_to_item] AFTER UPDATE
AS
BEGIN

    if object_id('tempdb..#disable_tu_item_to_item') is null
    BEGIN

	    if update(l_item_id) or update(r_item_id)
	    BEGIN

            declare @links table
	        (
	    	    id numeric primary key,
	    	    item_id numeric
	        )

	    	insert into @links
	    	select distinct link_id, l_item_id from inserted

	    	declare @link_id numeric, @item_id numeric , @query nvarchar(max)

	    	while exists(select id from @links)
	    	BEGIN

	    		select @link_id = id from @links
	    		select 	@item_id = item_id from @links

	    		declare @table_name nvarchar(50), @table_name_rev nvarchar(50)
	    		set @table_name = 'item_link_' + cast(@link_id as varchar)
	    		set @table_name_rev = 'item_link_' + cast(@link_id as varchar) + '_rev'

	    		declare @linked_item numeric
	    		select @linked_item = l_item_id from inserted

	    		set @query = 'update ' + @table_name + ' set linked_id = @linked_item where id = @item_id'
	    		print @query
	    		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

	    		set @query = 'update ' + @table_name_rev + ' set linked_id = @linked_item where id = @item_id'
	    		print @query
	    		exec sp_executesql @query, N'@item_id numeric, @linked_item numeric', @item_id = @item_id , @linked_item = @linked_item

	    		delete from @links where id = @link_id
	    	END
	    END
    END
END
GO
