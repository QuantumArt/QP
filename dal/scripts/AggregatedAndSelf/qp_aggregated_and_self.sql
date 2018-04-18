ALTER function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as 
begin
	
	declare @ids2 Ids
	insert into @ids2
	select id from @itemIds i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID
	inner join CONTENT_ATTRIBUTE ca with(nolock) on ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1

	insert into @ids
	select id from @itemIds

	union 	
	select AGG_DATA.CONTENT_ITEM_ID
	from CONTENT_ATTRIBUTE ATT with(nolock)
	JOIN CONTENT_ATTRIBUTE AGG_ATT with(nolock) ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
	JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
	where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1
	and ATT.CONTENT_ID in (select content_id from content_item with(nolock) where content_item_id in (select id from @itemIds)) AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @ids2)	

	return
end
