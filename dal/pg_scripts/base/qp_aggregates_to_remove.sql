create or replace function qp_aggregates_to_remove(ids integer[]) returns integer[]
    stable
    language plpgsql
as
$$
DECLARE
    agg_ids int[];
BEGIN
    agg_ids := array_agg(cd2.CONTENT_ITEM_ID) FROM content_item ci
    inner join content_attribute ca on ca.CONTENT_ID = ci.CONTENT_ID and ca.is_classifier
    inner join content_data cd on cd.attribute_id = ca.attribute_id and cd.content_item_id = ci.content_item_id
    inner join content_attribute ca2 on ca2.classifier_attribute_id = ca.attribute_id and ca2.aggregated
    inner join content_data cd2 on cd2.o2m_data = ci.content_item_id and cd2.attribute_id = ca2.attribute_id
    where ci.content_item_id = ANY(ids) and cd.data <> ca2.content_id::text;

	return coalesce(agg_ids, ARRAY[]::int[]);
END;
$$;
