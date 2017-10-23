exec qp_drop_existing 'qp_aggregates_to_remove', 'IsTableFunction'
GO

CREATE function [dbo].[qp_aggregates_to_remove](@itemIds Ids READONLY)
returns @ids table (id numeric primary key)
as
begin

    declare @ids2 Ids
    insert into @ids2
    select id from @itemIds i inner join content_item ci on i.ID = ci.CONTENT_ITEM_ID and ci.SPLITTED = 0
	where exists(select * from CONTENT_ATTRIBUTE ca where ca.CONTENT_ID = ci.CONTENT_ID and ca.IS_CLASSIFIER = 1)

    if exists (select * from @ids2)
    begin
        insert into @ids

        select AGG_DATA.CONTENT_ITEM_ID
        from CONTENT_ATTRIBUTE ATT
        JOIN CONTENT_ATTRIBUTE AGG_ATT ON AGG_ATT.CLASSIFIER_ATTRIBUTE_ID = ATT.ATTRIBUTE_ID
        JOIN CONTENT_DATA AGG_DATA with(nolock) ON AGG_DATA.ATTRIBUTE_ID = AGG_ATT.ATTRIBUTE_ID
        JOIN CONTENT_DATA CLF_DATA with(nolock) ON CLF_DATA.ATTRIBUTE_ID = ATT.ATTRIBUTE_ID AND cast(CLF_DATA.CONTENT_ITEM_ID as nvarchar(8)) = AGG_DATA.DATA
        where ATT.IS_CLASSIFIER = 1 AND AGG_ATT.AGGREGATED = 1 AND CLF_DATA.DATA <> cast(AGG_ATT.CONTENT_ID as nvarchar(8))
        and ATT.CONTENT_ID in (
            select content_id from content_item with(nolock)
            where content_item_id in (select id from @itemIds)
        )
        AND AGG_DATA.DATA in  (select cast(id as nvarchar(8)) from @ids2)
    end

    return
end
GO
