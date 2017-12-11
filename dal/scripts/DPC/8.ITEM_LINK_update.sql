
update item_link set is_rev = 1
from item_link il
inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
inner join content_to_content cc on il.link_id = cc.link_id and ci.CONTENT_ID <> cc.l_content_id
and is_rev = 0
GO

update item_link_async set is_rev = 1
from item_link_async il
inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
inner join content_to_content cc on il.link_id = cc.link_id and ci.CONTENT_ID <> cc.l_content_id
and is_rev = 0
GO

update item_link set is_self = 1
from item_link il inner join content_to_content cc on il.link_id = cc.link_id where cc.l_content_id = cc.r_content_id
GO


update item_link_async set is_self = 1
from item_link_async il inner join content_to_content cc on il.link_id = cc.link_id where cc.l_content_id = cc.r_content_id
GO

