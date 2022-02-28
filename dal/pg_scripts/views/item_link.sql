create or replace view item_link(link_id, item_id, linked_item_id, is_rev, is_self) as
SELECT ii.link_id,
       ii.l_item_id AS item_id,
       ii.r_item_id AS linked_item_id,
       ii.is_rev,
       ii.is_self
FROM item_to_item ii;



