SET NOCOUNT ON

declare @ids [Ids]
declare @count numeric, @count2 numeric

insert into @ids
select top 2 content_item_id from content_item_version group by content_item_id having count(*) = 10

exec qp_create_content_item_versions @ids, 1

select @count = count(*) from (select content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id having count(*) = 10) c

exec qp_assert_num_equal @count, 2, 'Same version count'

declare @date DATETIME
select @date = max(created) from (select max(CREATED) as created, content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id) c

set @count = day(@date)
set @count2 = day(getdate())
exec qp_assert_num_equal @count, @count2, 'Day from date'

set @count = month(@date)
set @count2 = month(getdate())
exec qp_assert_num_equal @count, @count2, 'Month from date'

set @count = year(@date)
set @count2 = year(getdate())
exec qp_assert_num_equal @count, @count2, 'Year from date'


