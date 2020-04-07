exec qp_drop_existing 'qp_all_article_search', 'IsProcedure'
go

CREATE PROCEDURE [dbo].[qp_all_article_search]
    @p_site_id int,
    @p_user_id int,
    @p_searchparam nvarchar(4000),
    @p_order_by nvarchar(max) = N'data.[rank] DESC',
    @p_start_row int = 0,
    @p_page_size int = 0,
    @p_item_id int = null,

    @total_records int OUTPUT
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

    declare @is_admin bit
    select @is_admin = dbo.qp_is_user_admin(@p_user_id)

    -- set number of start record by default
    IF (@p_start_row <= 0)
        BEGIN
            SET @p_start_row = 1
        END

    -- set number of finish record
    DECLARE @p_end_row AS int
    SET @p_end_row = @p_start_row + @p_page_size - 1

    -- make a query for subset of contents which enabled to access
    DECLARE @security_sql AS nvarchar(max)
    SET @security_sql = ''

    if @is_admin = 0
    begin
        EXEC dbo.qp_GetPermittedItemsAsQuery
                @user_id = @p_user_id,
                @group_id = 0,
                @start_level = 1,
                @end_level = 4,
                @entity_name = 'content',
                @parent_entity_name = 'site',
                @parent_entity_id = @p_site_id,
                @SQLOut = @security_sql OUTPUT
    end

    -- count all records
    declare @paramdef nvarchar(4000);
    declare @query nvarchar(4000);

    create table #temp
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    create table #temp2
    (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int)

    set @query = 'insert into #temp' + CHAR(13)
        + ' select content_item_id, weight, attribute_id, priority from ' + CHAR(13)
        + ' (select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority, ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number ' + CHAR(13)
        + ' from CONTAINSTABLE(content_data, *,  @searchparam) ft ' + CHAR(13)
        + ' inner join content_data cd on ft.[key] = cd.content_data_id) as c where c.number = 1 order by weight desc ' + CHAR(13)
    print @query

    exec sp_executesql @query, N'@searchparam nvarchar(4000)', @searchparam = @p_searchparam

    IF @p_item_id is not null
    begin
        set @query = 'if not exists (select * from #temp where content_item_id = ' + cast(@p_item_id as varchar(20)) + ') insert into #temp' + CHAR(13)
        set @query = @query + ' select ' + cast(@p_item_id as varchar(20)) + ', 0, 0, 1 ' + CHAR(13)
		print @query
        exec sp_executesql @query
    end

    set @paramdef = '@site_id int';
    if @is_admin = 0
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c0 on c0.CONTENT_ID = ci.CONTENT_ID ' + CHAR(13)
            + ' inner join (' + @security_sql + ') c on c.CONTENT_ID = c0.CONTENT_ID where c0.site_id = @site_id' + CHAR(13)

        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end
    else
    begin
        set @query = 'insert into #temp2 ' + CHAR(13)
            + ' select cd.* from #temp cd ' + CHAR(13)
            + ' inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
            + ' inner join content c on c.CONTENT_ID = ci.CONTENT_ID where c.site_id = @site_id' + CHAR(13)
        exec sp_executesql @query, @paramdef, @site_id = @p_site_id
    end

    select @total_records = count(distinct content_item_id) from #temp2

    -- main query
    declare @query_template nvarchar(4000);
    set @query_template = N'WITH PAGED_DATA_CTE AS ' + CHAR(13)
        + ' (select ROW_NUMBER() OVER (ORDER BY [priority] DESC, <$_order_by_$>) AS ROW, ' + CHAR(13)
        + ' 	ci.CONTENT_ID as ParentId, ' + CHAR(13)
        + ' 	data.CONTENT_ITEM_ID as Id, ' + CHAR(13)
        + ' 	data.ATTRIBUTE_ID as FieldId, ' + CHAR(13)
        + ' 	attr.ATTRIBUTE_TYPE_ID as FieldTypeId, ' + CHAR(13)
        + ' 	c.CONTENT_NAME as ParentName, ' + CHAR(13)
        + ' 	st.STATUS_TYPE_NAME as StatusName, ' + CHAR(13)
        + ' 	ci.CREATED as Created, ' + CHAR(13)
        + ' 	ci.MODIFIED as Modified, ' + CHAR(13)
        + ' 	usr.[LOGIN] as LastModifiedByUser, ' + CHAR(13)
        + ' 	data.[rank] as Rank, ' + CHAR(13)
        + ' 	data.[priority] as [priority], ' + CHAR(13)
		+ '		ci.ARCHIVE as Archive' + CHAR(13)
        + '   from #temp2 data ' + CHAR(13)
        + '   left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID ' + CHAR(13)
        + '   inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID ' + CHAR(13)
        + '	  inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID and c.site_id = @site_id ' + CHAR(13)
        + '   inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID ' + CHAR(13)
        + '   inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY ' + CHAR(13)
        + ' ) ' + CHAR(13)
        + ' select ' + CHAR(13)
        + ' 	ParentId, ' + CHAR(13)
        + ' 	ParentName, ' + CHAR(13)
        + ' 	Id, ' + CHAR(13)
        + ' 	FieldId, ' + CHAR(13)
        + '		(case when FieldTypeId in (9, 10) THEN cd.BLOB_DATA ELSE cd.DATA END) as Text, ' + CHAR(13)
        + ' 	dbo.qp_get_article_title_func(Id, ParentId) as Name, ' + CHAR(13)
        + ' 	StatusName, ' + CHAR(13)
        + ' 	pdc.Created, ' + CHAR(13)
        + ' 	pdc.Modified, ' + CHAR(13)
        + ' 	LastModifiedByUser, ' + CHAR(13)
        + ' 	Rank, ' + CHAR(13)
		+ '		pdc.Archive ' + CHAR(13)
        + ' from PAGED_DATA_CTE pdc ' + CHAR(13)
        + ' left join content_data cd on pdc.Id = cd.content_item_id and pdc.FieldId = cd.attribute_id ' + CHAR(13)
        + ' where ROW between @start_row and @end_row order by row asc';


    declare @sortExp nvarchar(4000);
    set @sortExp = case when @p_order_by is null or @p_order_by = '' then N'Rank DESC' else @p_order_by end;
    set @query = REPLACE(@query_template, '<$_order_by_$>', @sortExp);
    set @paramdef = '@searchparam nvarchar(4000), @site_id int, @start_row int, @end_row int';
    print @query
    EXECUTE sp_executesql @query, @paramdef, @searchparam = @p_searchparam, @site_id = @p_site_id, @start_row = @p_start_row, @end_row = @p_end_row;

    drop table #temp
    drop table #temp2
END
GO
