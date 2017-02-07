SET NOCOUNT ON

declare @ids [Ids]
declare @count numeric

insert into @ids
select top 2 content_item_id from content_288 c1 where [Type] = 305 and exists (select * from content_291 where Product = c1.CONTENT_ITEM_ID)
union
select top 1 content_item_id from content_287 c2 where exists (select * from content_288 where MarketingProduct = c2.CONTENT_ITEM_ID)

exec qp_create_content_item_versions @ids, 1

declare @count1 numeric, @count2 numeric
declare @link_id numeric, @attr_id numeric
declare @link2_id numeric, @attr2_id numeric

-- Test M2O

select @link_id = link_id, @attr_id = ATTRIBUTE_ID from content_attribute where content_id = 288 and attribute_name = 'Parameters'
select @link2_id = link_id, @attr2_id = ATTRIBUTE_ID from content_attribute where content_id = 287 and attribute_name = 'Products'

select @count1 = count(*) from

(
  select content_item_id from content_291 where [PRoduct] in (select id from @ids)
  union all
  select content_item_id from content_288 where [MarketingProduct] in (select id from @ids)

) c


select @count2 = count(*) from item_to_item_version ii
inner join
(
  select max(content_item_version_id) as id, content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id

) v on v.id = ii.content_item_version_id and attribute_id in (@attr_id, @attr2_id)

exec qp_assert_num_equal @count1, @count2, 'M2O'

-- Test M2M

select @link_id = link_id, @attr_id = ATTRIBUTE_ID from content_attribute where content_id = 288 and attribute_name = 'Regions'

select @count1 = count(*) from item_link where item_id in (select id from @ids) and link_id = @link_id

select @count2 = count(*) from item_to_item_version ii
inner join
(
  select max(content_item_version_id) as id, content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id

) v on v.id = ii.content_item_version_id and attribute_id = @attr_id


exec qp_assert_num_equal @count1, @count2, 'M2M'

select @count1 = count(*) from content_data where content_item_id in (select content_item_id from content_305 where Product in (select id from @ids))

select @count2 = count(*) from version_content_data vcd
inner join
(
  select max(content_item_version_id) as id, content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id

) v on v.id = vcd.content_item_version_id and attribute_id in (select attribute_id from content_attribute where content_id = 305)

exec qp_assert_num_equal @count1, @count2, 'Extensions'

select @count1 = count(*) from content_data where content_item_id in (select id from @ids)

select @count2 = count(*) from version_content_data vcd
inner join
(
  select max(content_item_version_id) as id, content_item_id from content_item_version where content_item_id in (select id from @ids) group by content_item_id

) v on v.id = vcd.content_item_version_id and attribute_id in (select attribute_id from content_attribute where content_id in (287, 288))

exec qp_assert_num_equal @count1, @count2, 'Main data'
