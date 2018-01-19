exec qp_drop_existing 'qp_update_m2o', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_m2o]
@id numeric,
@fieldId numeric,
@value nvarchar(max),
@update_archive bit = 1
AS
BEGIN
    declare @ids table (id numeric primary key)
    declare @new_ids table (id numeric primary key);
    declare @cross_ids table (id numeric primary key);

    declare @contentId numeric, @fieldName nvarchar(255)
    select @contentId = content_id, @fieldName = attribute_name from CONTENT_ATTRIBUTE where ATTRIBUTE_ID = @fieldId

    insert into @ids
    exec qp_get_m2o_ids @contentId, @fieldName, @id

    select content_item_id
    from content_data where ATTRIBUTE_ID = @fieldId and DATA = CAST(@id as nvarchar)

    insert into @new_ids select * from dbo.split(@value, ',');

    insert into @cross_ids select t1.id from @ids t1 inner join @new_ids t2 on t1.id = t2.id
    delete from @ids where id in (select id from @cross_ids);
    delete from @new_ids where id in (select id from @cross_ids);

    if @update_archive = 0
    begin
        delete from @ids where id in (select content_item_id from content_item where ARCHIVE = 1)
    end

    insert into #resultIds(id, attribute_id, to_remove)
    select id, @fieldId as attribute_id, 1 as to_remove from @ids
    union all
    select id, @fieldId as attribute_id, 0 as to_remove from @new_ids
END
GO
