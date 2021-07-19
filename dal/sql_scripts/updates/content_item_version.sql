declare @hist1 table(version_id numeric primary key, status_type_id numeric, archive bit, visible bit)
declare @hist2 table(version_id numeric primary key, status_type_id numeric, archive bit, visible bit)

create table #status_history (num int, item_id decimal, version_id decimal, status_type_id int, visible bit, archive bit)
create clustered index ix_temp_status_history ON #status_history (item_id, num)
create nonclustered index ix_temp_version ON #status_history (version_id)

insert into #status_history
select row_number() over(partition by CONTENT_ITEM_ID order by status_history_id desc) as num,
content_item_id,  content_item_version_id, status_type_id, visible, archive
from content_item_status_history with(nolock) where datediff(month, CREATED, getdate()) <= 12

insert into @hist1
select h.version_id, h1.status_type_id, h1.archive, h1.visible
from #status_history h
inner join #status_history h1 on h.item_id = h1.item_id and h.num = h1.num - 1
inner join content_item_version v with(nolock) on h.version_id = v.content_item_version_id
where h1.status_type_id is not null

while exists(select * from @hist1)
begin
    delete top(100) from @hist1
    output deleted.* into @hist2

    update CONTENT_ITEM_VERSION with(rowlock)
    set STATUS_TYPE_ID = h.status_type_id, VISIBLE = h.visible, ARCHIVE = h.archive
    FROM CONTENT_ITEM_VERSION v INNER JOIN @hist2 h ON v.content_item_version_id = h.version_id
end

drop table #status_history
GO
