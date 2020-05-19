ALTER TRIGGER [dbo].[td_content_to_content] ON [dbo].[content_to_content] AFTER DELETE
AS
BEGIN
    if object_id('tempdb..#disable_td_content_to_content') is null
    begin 
        declare @link_id numeric, @i numeric, @count numeric
      
        declare @cc table (
          id numeric identity(1,1) primary key,
          link_id numeric
        )
      
        insert into @cc (link_id) select d.link_id from deleted d
      
        set @i = 1
        select @count = count(id) from @cc
      
        while @i < @count + 1
        begin
          select @link_id = link_id from @cc where id = @i
          exec qp_drop_link_view @link_id
          set @i = @i + 1
        end
    end
END
GO