declare @query varchar(max) = '
	select
		s.site_id,
		s.site_name,
		c.content_id,
		c.content_name,
		a.attribute_name,
		l1.link_id,
		case when l2.cnt is null then 0 else l2.cnt end cnt,
		l1.cnt [EF cnt],
		case when l2.cnt is null then 0 else l2.cnt end - l1.cnt [diff]
	from ('

select @query = @query +
	'select ' + cast(link_id as varchar) + ' link_id, sum(cnt) cnt from' +
	'(select count(*) cnt from item_link_' + cast(link_id as varchar) + ' union all ' +
	'select count(*) cnt from item_link_' + cast(link_id as varchar) + '_rev) t' +
	' union all '
from content_attribute
where link_id is not null
group by link_id

declare @len int = len(@query) - len(' union all ') + 1
set @query = substring(@query, 0, @len) + ') l1
	left join (select link_id, count(*) cnt from item_link group by link_id) l2 on l1.link_id = l2.link_id
	join content_attribute a on l1.link_id = a.link_id
	join content c on a.content_id = c.content_id
	join site s on c.site_id = s.site_id
	order by diff desc'
exec(@query)