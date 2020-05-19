
ALTER PROCEDURE [dbo].[create_content_item_version]
    @uid NUMERIC,
    @content_item_id NUMERIC
AS
BEGIN
    declare @ids [Ids]

    insert into @ids values (@content_item_id)

    exec qp_create_content_item_versions @ids, @uid
END
GO
