
declare @hist1 table(version_id numeric primary key, status_type_id numeric, archive bit, visible bit)
declare @hist2 table(version_id numeric primary key, status_type_id numeric, archive bit, visible bit)


;with history(num, id, item_id, version_id, status_type_id, visible, archive) as (
    select row_number() over(partition by CONTENT_ITEM_ID order by status_history_id desc) as num,
           status_history_id, content_item_id,  content_item_version_id, status_type_id, visible, archive
    from content_item_status_history where coalesce(system_status_type_id, 2) = 2
)
insert into @hist1
select h.version_id, h1.status_type_id, h1.archive, h1.visible from history h
inner join history h1 on h.num = h1.num - 1 and h.item_id = h1.item_id
where h.version_id is not null order by h.id desc

while exists(select * from @hist1)
begin
    delete top(100) from @hist1
    output deleted.* into @hist2

    update CONTENT_ITEM_VERSION
    set STATUS_TYPE_ID = h.status_type_id, VISIBLE = h.visible, ARCHIVE = h.archive
    FROM CONTENT_ITEM_VERSION v INNER JOIN @hist2 h ON v.content_item_version_id = h.version_id
    Where v.STATUS_TYPE_ID is null
end
GO