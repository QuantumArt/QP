

select data, ATTRIBUTE_ID, COUNT(*) from content_data
where ATTRIBUTE_ID in (select ATTRIBUTE_ID From CONTENT_ATTRIBUTE where AGGREGATED = 1)
group by ATTRIBUTE_ID, data
having COUNT(*) > 1


with abc (data, attribute_id , cnt )
as
(
select data, ATTRIBUTE_ID, COUNT(*) from content_data
where ATTRIBUTE_ID in (select ATTRIBUTE_ID From CONTENT_ATTRIBUTE where AGGREGATED = 1)
group by ATTRIBUTE_ID, data
having COUNT(*) > 1
)
delete from content_item where CONTENT_ITEM_ID in
(
  select content_item_id from
  (
    select content_item_id, ROW_NUMBER() over (partition by abc.data order by modified asc) as num  from content_data cd inner join abc on cd.data = abc.data and cd.ATTRIBUTE_ID = abc.ATTRIBUTE_ID

  ) t where num > 1
)
