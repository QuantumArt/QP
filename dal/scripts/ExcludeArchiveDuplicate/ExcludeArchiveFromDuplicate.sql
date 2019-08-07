exec qp_drop_existing 'qp_count_duplicates', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_count_duplicates]
@content_id numeric, 
@field_ids nvarchar(max),
@ids nvarchar(max) = null,
@includeArchive bit = 0
AS
BEGIN
	declare @field_names_table table (name nvarchar(255))
	insert @field_names_table select ca.ATTRIBUTE_NAME as name from dbo.Split(@field_ids, ',') f inner join CONTENT_ATTRIBUTE ca on f.Items = ca.ATTRIBUTE_ID
	
	declare @currentName nvarchar(255)
	declare @fieldList nvarchar(max)
	set @fieldList = ''
	
	while exists(select * from @field_names_table)
	begin
		select @currentName = name from @field_names_table
		if @fieldList <> ''
			set @fieldList = @fieldList + ', '
		set @fieldList = @fieldList + '[' + @currentName + ']'
		delete from @field_names_table where name = @currentName
	end
	
	declare @where nvarchar(max)
	if @ids is null or @ids = ''
		set @where = ''
	else
		set @where = ' where content_item_id in (' + @ids + ')' 	

	if @includeArchive = 0
		set @where = @where + case when @where ='' then 'where archive = 0' else ' and archive = 0' end

	declare @sql nvarchar(max)
	if @fieldList = ''
		set @sql = 'select 0 as cnt'
	else
		set @sql = 'select coalesce(sum(c.cnt), 0) from (select COUNT(*) as cnt from content_' + CAST(@content_id as nvarchar(20)) + '_united ' + @where + ' group by ' + @fieldList + ' having COUNT(*) > 1) as c'
		
	exec sp_executesql @sql

END