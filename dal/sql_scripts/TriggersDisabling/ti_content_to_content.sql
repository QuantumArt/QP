ALTER TRIGGER [dbo].[ti_content_to_content] ON [dbo].[content_to_content] AFTER INSERT
AS
BEGIN
    if object_id('tempdb..#disable_ti_content_to_content') is null
    begin
		declare @link_id numeric, @i numeric, @count numeric, @inscount numeric

		declare @cc table (
			id numeric identity(1,1) primary key,
			link_id numeric
		)

		select @count = count(link_id) from inserted

		if (@count = 1) -- prevent @@identity change (for restore site)
		begin
			select @link_id = link_id from inserted
			exec qp_build_link_view @link_id
		end
		else if (@count > 1)
		begin
			insert into @cc (link_id) select i.link_id from inserted i

			set @i = 1

			while @i < @count + 1
			begin
				select @link_id = link_id from @cc where id = @i
				exec qp_build_link_view @link_id
				set @i = @i + 1
			end
		end
	end
END
GO