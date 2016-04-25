--select * from link_21 where linked_item_id = 2096 or item_id = 2096
--select * from link_21 where linked_item_id = 2096
--select * from content_to_content where link_id = 21
--GO
set nocount on

declare @links [Links]

insert into @links
values 
 (699603, 2096),
 (699604, 2096),
 (2096, 699605)

insert into item_to_item (link_id, l_item_id, r_item_id)
select 21, id, linked_id from @links

select count(*) from item_link_21
select count(*) from item_link_21_rev

select * from item_link_21 where linked_id = 2096 and id in (699603, 699604, 699605)
select * from item_link_21_rev where id = 2096 and linked_id in (699603, 699604, 699605)



delete from item_to_item where link_id = 21 and l_item_id = 2096 and r_item_id in (699603, 699604, 699605)
GO

declare @links [Links]

insert into @links
values 
 (699603, 2096),
 (699604, 2096),
 (2096, 699605)

insert into item_link_async(link_id, item_id, linked_item_id)
select 21, id, linked_id from @links

select count(*) from item_link_21_async
select count(*) from item_link_21_async_rev

select * from item_link_21_async where linked_id = 2096 and id in (699603, 699604, 699605)
select * from item_link_21_async_rev where id = 2096 and linked_id in (699603, 699604, 699605)

delete item_link_async from item_link_async inner join @links on link_id = 21 and item_id = id and linked_item_id = LINKED_ID

select * from content_item where content_id = 294
GO


declare @links [Links]

insert into @links
values 
 (1704, 2000),
 (1705, 2001),
 (2002, 1706)

insert into item_to_item (link_id, l_item_id, r_item_id)
select 71, id, linked_id from @links
union
select 72, id, linked_id from @links

select count(*) from item_link_71
select count(*) from item_link_71_rev
select count(*) from item_link_72
select count(*) from item_link_72_rev

select il.* from item_link_71 il inner join @links il2 on il.id = il2.id and il.linked_id = il2.linked_id
select il.* from item_link_71_rev il inner join @links il2 on il.id = il2.linked_id and il.linked_id = il2.id

select il.* from item_link_72 il inner join @links il2 on il.id = il2.id and il.linked_id = il2.linked_id
select il.* from item_link_72_rev il inner join @links il2 on il.id = il2.linked_id and il.linked_id = il2.id

delete item_to_item from item_to_item inner join @links on link_id in (71, 72) and l_item_id = id and r_item_id = LINKED_ID

select il.* from item_link_71 il inner join @links il2 on il.id = il2.id and il.linked_id = il2.linked_id
select il.* from item_link_71_rev il inner join @links il2 on il.id = il2.linked_id and il.linked_id = il2.id

GO


