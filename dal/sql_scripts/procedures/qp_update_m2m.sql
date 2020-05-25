exec qp_drop_existing 'qp_update_m2m', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2m]
@id numeric,
@linkId numeric,
@value nvarchar(max),
@splitted bit = 0,
@update_archive bit = 1
AS
BEGIN
    declare @newIds table (id numeric primary key, attribute_id numeric null, has_data bit, splitted bit, has_async bit null)
    declare @ids table (id numeric primary key)
    declare @crossIds table (id numeric primary key)

    insert into @newIds (id) select * from dbo.split(@value, ',')

    IF @splitted = 1
        insert into @ids select linked_item_id from item_link_async where link_id = @linkId and item_id = @id
    ELSE
        insert into @ids select linked_item_id from item_link where link_id = @linkId and item_id = @id

    insert into @crossIds select t1.id from @ids t1 inner join @newIds t2 on t1.id = t2.id
    delete from @ids where id in (select id from @crossIds)
    delete from @newIds where id in (select id from @crossIds)

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    IF @splitted = 0
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id

    IF @splitted = 1
        DELETE FROM item_link_async WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)
    ELSE
        DELETE FROM item_link_united_full WHERE link_id = @linkId AND item_id = @id and linked_item_id in (select id from @ids)

    IF @splitted = 1
        INSERT INTO item_link_async (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds
    ELSE
        INSERT INTO item_link (link_id, item_id, linked_item_id) SELECT @linkId, @id, id from @newIds

    if dbo.qp_is_link_symmetric(@linkId) = 1
    begin

        with newItems (id, attribute_id, has_data, splitted, has_async) as
        (
        select
            n.id, ca.attribute_id,
            case when cd.content_item_id is null then 0 else 1 end as has_data,
            ci.splitted,
            case when ila.link_id is null then 0 else 1 end as has_async
        from @newIds n
            inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            inner join content c on ci.content_id = c.content_id
            inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = @linkId
            left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            left join item_link_async ila on @linkId = ila.link_id and n.id = ila.item_id and ila.linked_item_id = @id
        )
        update @newIds
        set attribute_id = ext.attribute_id, has_data = ext.has_data, splitted = ext.splitted, has_async = ext.has_async
        from @newIds n inner join newItems ext on n.id = ext.id

        if @splitted = 0
        begin
            update content_data set data = @linkId
            from content_data cd
            inner join @newIds n on cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
            where n.has_data = 1

            insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
            select n.id, n.attribute_id, @linkId
            from @newIds n
            where n.has_data = 0 and n.attribute_id is not null

            insert into item_link_async(link_id, item_id, linked_item_id)
            select @linkId, n.id, @id
            from @newIds n
            where n.splitted = 1 and n.has_async = 0 and n.attribute_id is not null
        end
    end
END
GO