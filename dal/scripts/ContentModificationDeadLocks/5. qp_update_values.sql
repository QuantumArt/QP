exec qp_drop_existing 'qp_update_values', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_update_values]
    @values [Values] READONLY
AS
BEGIN

    declare @values2 [Values]
    declare @values3 [Values]

    insert into @values2
    select * from @values

    while exists(select * from @values2)
    begin
        delete from @values3

        delete top(100) from @values2 output DELETED.* into @values3

        update content_data set
        data = case when ca.attribute_type_id in (9,10) then null else v.Value end,
        blob_data = case when ca.attribute_type_id in (9,10) then v.Value else null end
        from content_data cd
        inner join @values3 v on v.ArticleId = cd.content_item_id and v.FieldId = cd.attribute_id
        inner join content_attribute ca on cd.attribute_id = ca.attribute_id

    end

END
GO
