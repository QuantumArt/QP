SET NOCOUNT ON

declare @ids [Ids]
declare @modifiers table (num numeric identity(1, 1) primary key, id numeric)
declare @count numeric
declare @clear bit
set @clear = 1

declare @count0 numeric
declare @count1 numeric, @count2 numeric
declare @count3 numeric, @count4 numeric

declare @link_id numeric, @attr_id numeric

select @link_id = link_id, @attr_id = ATTRIBUTE_ID from content_attribute where content_id = 287 and attribute_name = 'Modifiers'

insert into @ids
select top 2 content_item_id from content_287 c where status_type_id = 125 and not exists (select * from content_item_splitted cis where cis.content_item_id = c.content_item_id)
and exists (select * from item_link i where c.CONTENT_ITEM_ID = i.item_id and i.link_id = @link_id)

insert into @modifiers
select top 3 content_item_id from content_300 c 
where not exists (select * from item_link il where link_id = @link_id and item_id in (select id from @ids) and linked_item_id = c.CONTENT_ITEM_ID)

if @clear = 1
begin
  delete from item_link_async where item_id in (select id from @ids)
  delete from content_287_async where content_item_id in (select id from @ids)
  update content_item set SCHEDULE_NEW_VERSION_PUBLICATION = 0 where content_item_id in (select id from @ids)
end

select @count0 = count(*) from item_link where item_id in (select id from @ids)

insert into item_to_item(l_item_id, r_item_id, link_id)
select id, (select id from @modifiers where num = 1), @link_id from @ids

update content_item set SCHEDULE_NEW_VERSION_PUBLICATION = 1 where content_item_id in (select id from @ids)

exec qp_split_articles @ids


delete from item_link_async
where item_id in (select id from @ids) and link_id = @link_id and linked_item_id = (select id from @modifiers where num = 1)

insert into item_link_async(item_id, linked_item_id, link_id)
select id, (select id from @modifiers where num = 2), @link_id from @ids

insert into item_link_async(item_id, linked_item_id, link_id)
select id, (select id from @modifiers where num = 3), @link_id from @ids

select @count1 = count(*) from item_link where item_id in (select id from @ids)
select @count2 = count(*) from item_link_async where item_id in (select id from @ids)


set @count3 = @count0 + 2
set @count4 = @count0 + 4

exec qp_assert_num_equal @count1, @count3, 'M2M in main table before merge'
exec qp_assert_num_equal @count2, @count4, 'M2M in async table before merge'


select @count1 = count(*) from content_287 where content_item_id in (select id from @ids)
select @count2 = count(*) from content_287_async where content_item_id in (select id from @ids)

exec qp_assert_num_equal @count1, 2, 'Articles in main table before merge'
exec qp_assert_num_equal @count2, 2, 'Articles in async table before merge'

exec qp_merge_articles @ids


select @count1 = count(*) from item_link where item_id in (select id from @ids)
select @count2 = count(*) from item_link_async where item_id in (select id from @ids)
set @count3 = @count0 + 4

exec qp_assert_num_equal @count1, @count3, 'M2M in main table after merge'
exec qp_assert_num_equal @count2, 0, 'M2M in async table after merge'

select @count1 = count(*) from content_287 where content_item_id in (select id from @ids)
select @count2 = count(*) from content_287_async where content_item_id in (select id from @ids)

exec qp_assert_num_equal @count1, 2, 'Articles in main table after merge'
exec qp_assert_num_equal @count2, 0, 'Articles in async table after merge'

select @count1 = count(*) from item_link where item_id in (select id from @ids) and link_id = @link_id and linked_item_id in (select id from @modifiers where num = 1)
exec qp_assert_num_equal @count1, 0, 'Deleted links'
select @count2 = count(*) from item_link where item_id in (select id from @ids) and link_id = @link_id and linked_item_id in (select id from @modifiers where num in (2,3))
exec qp_assert_num_equal @count2, 4, 'Appended links'

delete from item_link where item_id in (select id from @ids) and link_id = @link_id and linked_item_id in (select id from @modifiers where num in (1,2,3))

 --delete from item_link_async where item_id in (select id from @ids)
 --delete from content_287_async where content_item_id in (select id from @ids)
 --update content_item set SCHEDULE_NEW_VERSION_PUBLICATION = 0 where content_item_id in (select id from @ids)