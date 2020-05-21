exec qp_drop_existing 'qp_aggregated_and_self', 'IsTableFunction'
GO

CREATE function [dbo].[qp_aggregated_and_self](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as
begin

	declare @ids2 table (id numeric primary key, attribute_id numeric)
	insert into @ids2(id, attribute_id)
	select id, ca.ATTRIBUTE_ID from @itemIds i inner join content_item ci with(nolock) on i.ID = ci.CONTENT_ITEM_ID
	inner join CONTENT_ATTRIBUTE ca with(nolock) on ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1

	declare @attrIds Ids
	insert into @attrIds
	select distinct attribute_id from @ids2

	insert into @ids
	select id from @itemIds

	union

	select AGG_DATA.CONTENT_ITEM_ID
	from CONTENT_ATTRIBUTE AGG_ATT with(nolock)
	INNER JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
	where AGG_ATT.AGGREGATED = 1 and AGG_ATT.CLASSIFIER_ATTRIBUTE_ID in (select id from @attrIds)
	and AGG_DATA.O2M_DATA in (select id from @ids2)
	return
end
GO