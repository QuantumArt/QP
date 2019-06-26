CREATE OR REPLACE PROCEDURE public.qp_before_content_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	    item_ids integer[];
	    attr_ids integer[];
	    link_ids integer[];
	BEGIN
	    IF ids IS NOT NULL THEN

	        CREATE TEMP TABLE disable_td_delete_item(id numeric);

	        item_ids := array_agg(ci.content_item_id) from content_item ci where content_id = ANY(ids);
	        call qp_before_content_item_delete(item_ids);

	        delete from content_item where content_id = ANY(ids);

	        DROP TABLE disable_td_delete_item;

	        attr_ids := array_agg(attribute_id) from content_attribute ca where content_id = ANY(ids);
	        IF attr_ids IS NOT NULL THEN
                update content_attribute set related_attribute_id = NULL where related_attribute_id = ANY(attr_ids);

                update content_attribute set CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = false
                where CLASSIFIER_ATTRIBUTE_ID = ANY(attr_ids);

                delete from content_attribute where BACK_RELATED_ATTRIBUTE_ID = any(attr_ids);

                update content_attribute set TREE_ORDER_FIELD = NULL where TREE_ORDER_FIELD = ANY(attr_ids);
            END IF;

	        link_ids := array_agg(link_id) from content_to_content cc
	            where cc.l_content_id = ANY(ids) or cc.r_content_id = ANY(ids);

	        if link_ids IS NOT NULL THEN
    	        call qp_before_content_to_content_delete(link_ids);

            end if;
            update content_attribute set link_id = null where link_id = ANY(link_ids);

            delete from content_to_content where l_content_id = ANY(ids) or r_content_id = ANY(ids);

            delete from container where content_id = ANY(ids);
            delete from content_form where content_id = ANY(ids);
            delete from user_default_filter where content_id = ANY(ids);
            delete from content_tab_bind where content_id = ANY(ids);
            delete from action_content_bind where content_id = ANY(ids);

        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_delete(integer[])
    OWNER TO postgres;
