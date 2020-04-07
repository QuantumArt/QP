-- PROCEDURE: public.qp_update_m2m(numeric, numeric, text, boolean, boolean)

-- DROP PROCEDURE public.qp_update_m2m(numeric, numeric, text, boolean, boolean);

CREATE OR REPLACE PROCEDURE public.qp_update_m2m(
	id numeric,
	link_id numeric,
	value text,
	splitted boolean,
	update_archive boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  new_ids int[];
	  old_ids int[];
	  cross_ids int[];
	  archive_ids int[];
	  is_symmetric boolean;
	  data_items link_data[];
	BEGIN
		RAISE NOTICE 'Start: %', clock_timestamp();			
		is_symmetric := "symmetric" from content_to_content cc where cc.link_id = $2;
		IF value is null OR value = '' THEN
			new_ids = ARRAY[]::int[];
		ELSE
			new_ids := regexp_split_to_array(value, E',\\s*')::int[];
		END IF;
		
		IF splitted THEN
			old_ids := array_agg(linked_item_id) from item_link_async ila where ila.link_id = $2 and item_id = $1;
		ELSE
			old_ids := array_agg(linked_item_id) from item_link il where il.link_id = $2 and item_id = $1;		
		END IF;
		old_ids := coalesce(old_ids, ARRAY[]::int[]);
		
		cross_ids := new_ids & old_ids;
		old_ids := old_ids - cross_ids;
		new_ids := new_ids - cross_ids;
							
		RAISE NOTICE 'Arrays calculated: %',  clock_timestamp();								
		
		IF not update_archive and array_length(old_ids, 1) > 1 THEN
			archive_ids := array_agg(content_item_id) from content_item where content_item_id = ANY(old_ids) AND archive = 1;
			archive_ids = coalesce(archive_ids, ARRAY[]::int[]);			
			old_ids := old_ids - archive_ids;
		END IF;
								   
		RAISE NOTICE 'Archive calculated: %',  clock_timestamp();								
	   
		IF splitted THEN
			DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND item_id = $1 and linked_item_id = ANY(old_ids);			
		ELSE
			DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND item_id = $1;
			DELETE FROM item_to_item ii WHERE ii.link_id = $2 AND l_item_id = $1 and r_item_id = ANY(old_ids);
			IF is_symmetric THEN
				DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND linked_item_id = $1 and item_id = ANY(old_ids);
				DELETE FROM item_to_item ii WHERE ii.link_id = $2 AND r_item_id = $1 and l_item_id = ANY(old_ids);			
			END IF;
		END IF;
								   
		RAISE NOTICE 'Deleted: %',  clock_timestamp();		

		IF splitted THEN
        	INSERT INTO item_link_async SELECT $2, $1, unnest from unnest(new_ids);
    	ELSE
        	INSERT INTO item_link SELECT $2, $1, unnest from unnest(new_ids);
			IF is_symmetric THEN
 				INSERT INTO item_link SELECT $2, unnest, $1 from unnest(new_ids);			
			END IF;
		END IF;
								   
		RAISE NOTICE 'Inserted: %',  clock_timestamp();								   
		
		IF is_symmetric and not splitted and array_length(new_ids, 1) > 0 THEN
			data_items := array_agg(
					row(n.id, ca.attribute_id, cd.attribute_id is not null, ci.splitted, ila.link_id is not null)
				)
				from (select unnest(new_ids) as id) n
            	inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            	inner join content c on ci.content_id = c.content_id
            	inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = $2
            	left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            	left join item_link_async ila on $2 = ila.link_id and n.id = ila.item_id and ila.linked_item_id = $1;
								   
			data_items := COALESCE(data_items, ARRAY[]::link_data[]);
								   
								   
			RAISE NOTICE 'Data items received: %',  clock_timestamp();
				
			IF array_length(data_items, 1) > 0 THEN	

				update content_data cd set data = $2 from unnest(data_items) n
				where cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
				and n.has_data;
								   
				RAISE NOTICE 'content_data updated:%',  clock_timestamp();								   

				insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
				select n.id, n.attribute_id, $2								  
				from unnest(data_items) n
				where not n.has_data and n.attribute_id is not null;

				RAISE NOTICE 'content_data inserted:%',  clock_timestamp();								   

				insert into item_link_async(link_id, item_id, linked_item_id)
				select $2, n.id, $1
				from unnest(data_items) n
				where n.splitted and not n.has_async and n.attribute_id is not null;

				RAISE NOTICE 'item_link_async inserted: %',  clock_timestamp();								   
								   
			END IF;
								  
		END IF;
	END;
$BODY$;

alter procedure qp_update_m2m(numeric, numeric, text, boolean, boolean) owner to postgres;
