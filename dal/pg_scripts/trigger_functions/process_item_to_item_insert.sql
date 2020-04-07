-- FUNCTION: public.process_item_to_item_insert()

-- DROP FUNCTION public.process_item_to_item_insert();

CREATE OR REPLACE FUNCTION public.process_item_to_item_insert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		content_links content_link[];
		item content_link;
		link_items link[];
		self_related boolean;
		is_async boolean;
    BEGIN
	  	content_links := array_agg(
			distinct row(i.link_id, c2c.SYMMETRIC, c2c.l_content_id, c2c.r_content_id)
		) from new_table i inner join content_to_content c2c on i.link_id = c2c.link_id;
		
		IF array_length(content_links, 1) > 0 THEN
			FOREACH item in array content_links
			LOOP
			    IF TG_TABLE_NAME = 'item_to_item' THEN
				    link_items := array_agg(distinct row(l_item_id, r_item_id)) from new_table where link_id = item.id;
				ELSE
				    link_items := array_agg(distinct row(item_id, linked_item_id)) from new_table where link_id = item.id;
				END IF;
				self_related := item.l_content_id = item.r_content_id;
				is_async := TG_TABLE_NAME = 'item_link_async';														   
				CALL qp_insert_link_table_item(item.id, item.l_content_id, link_items, is_async, false, false);
				CALL qp_insert_link_table_item(item.id, item.r_content_id, link_items, is_async, true, self_related);

			END LOOP;
		END IF;
		RETURN NULL;												   	
    END;
$BODY$;

ALTER FUNCTION public.process_item_to_item_insert()
    OWNER TO postgres;
