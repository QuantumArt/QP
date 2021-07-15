with history(num, id, item_id, version_id, status_type_id, visible, archive) as (
    select row_number() over(partition by CONTENT_ITEM_ID order by status_history_id desc) as num,
           status_history_id, content_item_id,  content_item_version_id, status_type_id, visible, archive
    from content_item_status_history where coalesce(system_status_type_id, 2) = 2
)
update content_item_version v set status_type_id = h1.status_type_id, visible = h1.visible, archive = h1.archive
from history h
inner join history h1 on h.num = h1.num - 1 and h.item_id = h1.item_id
where h.version_id is not null and v.content_item_version_id = h.version_id
  and v.status_type_id is null and h1.status_type_id is not null;



