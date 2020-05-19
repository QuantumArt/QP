
ALTER VIEW [dbo].[item_link] AS
SELECT ii.link_id AS link_id, ii.l_item_id AS item_id, ii.r_item_id AS linked_item_id, ii.is_rev, ii.is_self
FROM item_to_item AS ii
GO

ALTER VIEW [dbo].[item_link_united] AS
select link_id, item_id, linked_item_id, is_rev, is_self from item_link il where not exists (select * from content_item_splitted cis where il.item_id = cis.CONTENT_ITEM_ID)
union all
SELECT link_id, item_id, linked_item_id, is_rev, is_self from item_link_async ila
GO

ALTER VIEW [dbo].[site_item_link] AS
SELECT l.link_id, l.l_item_id, l.r_item_id, c.site_id
FROM item_to_item AS l
  LEFT OUTER JOIN content_item AS i ON i.content_item_id = l.l_item_id
  LEFT OUTER JOIN content AS c ON c.content_id = i.content_id
GO

exec sp_refreshview 'item_link_united_full'
