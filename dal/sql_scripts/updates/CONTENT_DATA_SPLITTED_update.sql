update CONTENT_DATA set SPLITTED = i.splitted
from content_data cd inner join CONTENT_ITEM i on i.content_item_id = cd.CONTENT_ITEM_ID
WHERE i.SPLITTED = 1
GO