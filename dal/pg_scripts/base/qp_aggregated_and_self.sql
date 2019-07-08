create or replace function qp_aggregated_and_self(ids integer[]) returns integer[]
    stable
    language plpgsql
as
$$
DECLARE
    classifier_ids int[];
    item_ids int[];
    agg_ids int[];
BEGIN
    item_ids := ids;
    classifier_ids := array_agg(distinct(ca.ATTRIBUTE_ID)) from content_attribute ca
        inner join content_item ci on ca.CONTENT_ID = ci.CONTENT_ID
        where ca.IS_CLASSIFIER and ci.content_item_id = ANY(ids);

    if classifier_ids is not null then
        agg_ids := array_agg(cd.CONTENT_ITEM_ID) FROM content_data cd
        inner join content_attribute ca on ca.attribute_id = cd.attribute_id
	    where ca.AGGREGATED and ca.CLASSIFIER_ATTRIBUTE_ID = ANY(classifier_ids)
        and cd.o2m_data = ANY(ids);
        if agg_ids is not null then
            item_ids := array_cat(item_ids, agg_ids);
        end if;
    end if;
	return item_ids;
END;
$$;
