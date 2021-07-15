ALTER PROCEDURE [dbo].[qp_create_content_item_versions]
  @ids [Ids] READONLY,
  @last_modified_by NUMERIC
AS
BEGIN
    declare @tm datetime
    select @tm = getdate()

    declare @items table (id numeric, cnt int, last_version_id int, new_version_id int, content_id numeric, max_num int)

    insert into @items (id, cnt)
    select i.ID, count(civ.content_item_version_id) from @ids i
    left join content_item_version civ on civ.content_item_id = i.id
    group by i.ID

    --print 'init completed'


    update @items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
    from @items items
    inner join content_item ci with(nolock) on items.id = ci.CONTENT_ITEM_ID
    inner join content c on c.CONTENT_ID = ci.CONTENT_ID

    --print 'max_num updated'

    declare @delete_ids [Ids]

    insert into @delete_ids
    select content_item_version_id from
    (
        select content_item_id, content_item_version_id,
        row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
        from content_item_version civ
        where content_item_id in (select id from @ids)
    ) c
    inner join @items items
    on items.id = c.content_item_id and c.num >= items.max_num

    DELETE item_to_item_version WHERE content_item_version_id in (select id from @delete_ids)
    DELETE content_item_version WHERE content_item_version_id in (select id from @delete_ids)

    --print 'exceeded deleted'

    DECLARE @NewVersions TABLE(ID INT)

    INSERT INTO content_item_version (
        version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by,
        status_type_id, archive, visible
    )
    output inserted.[CONTENT_ITEM_VERSION_ID] INTO @NewVersions
    SELECT @tm, 'backup', NULL, content_item_id, @last_modified_by, modified, last_modified_by,
           status_type_id, archive, visible
    from content_item where CONTENT_ITEM_ID in (select id from @ids);

    --print 'versions inserted'

    update @items set new_version_id = zip.version_id
    FROM @items items
    INNER JOIN
    (
        select x.item_id, y.version_id from
        (
        select id as item_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @items
        ) x
        inner join
        (
          select id as version_id, ROW_NUMBER() OVER (ORDER BY id) AS num FROM @NewVersions
        ) y
        on x.num = y.num
    ) zip
    on zip.item_id = items.id

    --print 'new versions updated'


    -- -- Get Extensions info
    declare @contentIds TABLE
    (
        id numeric
    )

    insert into @contentIds
    select convert(numeric, DATA) as ids
    from
    (
        select distinct DATA from content_data
        where CONTENT_ITEM_ID in (select id from @ids)
        and DATA is not null
        and ATTRIBUTE_ID in (
        select attribute_id from CONTENT_ATTRIBUTE where content_id in (select distinct content_id from @items) and IS_CLASSIFIER = 1)
    ) as p

    --print 'contents defined'

    declare @aggregates TABLE (content_id numeric, attribute_name nvarchar(255), attribute_id numeric)
    insert into @aggregates
    select ca.content_id, ca.ATTRIBUTE_NAME, ca.ATTRIBUTE_ID
    from CONTENT_ATTRIBUTE ca where ca.aggregated = 1 and ca.CONTENT_ID in (select * from @contentIds)

    declare @extensions TABLE (id numeric, agg_id numeric, content_id numeric)
    insert into @extensions
    select O2M_DATA, CONTENT_ITEM_ID, a.content_id from content_data cd
    inner join @aggregates a on cd.ATTRIBUTE_ID = a.attribute_id
    where O2M_DATA in (select id from @ids)

    --print 'extensions received'

    declare @main_ids TABLE
    (
        version_id numeric,
        id numeric,
        content_id numeric
    )

    insert into @main_ids
    select i.new_version_id, e.agg_id, e.content_id from @extensions e inner join @items i on e.id = i.id

    insert into @main_ids
    select i.new_version_id, i.id, i.content_id from @items i

    declare @total numeric
    select @total = count(*) from @main_ids
    print @total

    --print 'main defined'

    -- Store content item data
    INSERT INTO version_content_data (attribute_id, content_item_version_id, data, blob_data, o2m_data, created)
    SELECT attribute_id, m.version_id, data, blob_data, o2m_data, @tm
    FROM content_data cd inner join @main_ids m on cd.CONTENT_ITEM_ID = m.id

    --print 'content_data saved'


    -- Store Many-to-Many slice
    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
    SELECT m.version_id, ca.attribute_id, linked_item_id
    FROM item_link_united AS il
    INNER JOIN content_attribute AS ca ON ca.link_id = il.link_id
    INNER JOIN content_item AS ci ON ci.content_id =  ca.content_id AND ci.content_item_id = il.item_id
    inner join @main_ids m on il.item_id = m.id

    --print 'm2m saved'

    -- Store Many-to-One slice
    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
    SELECT m.version_id, ca.attribute_id, cd.content_item_id
    FROM content_data AS cd
    INNER JOIN content_attribute AS ca ON ca.BACK_RELATED_ATTRIBUTE_ID = cd.ATTRIBUTE_ID
    inner join @main_ids m on cd.O2M_DATA = m.id and ca.CONTENT_ID = m.content_id

    --print 'm2o saved'

    -- Write status history log
    INSERT INTO content_item_status_history
    (content_item_id, user_id, description, created, content_item_version_id, system_status_type_id)
    select id, @last_modified_by, 'Record version backup has been created', @tm, new_version_id, 2
    from @items
END
GO
