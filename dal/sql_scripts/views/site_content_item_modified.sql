drop view [dbo].[site_content_item_modified]
GO

create view [dbo].[site_content_item_modified] as
select ci.content_item_id, c.site_id, ci.modified from dbo.content_item ci inner join dbo.content c on ci.content_id = c.content_id
GO
