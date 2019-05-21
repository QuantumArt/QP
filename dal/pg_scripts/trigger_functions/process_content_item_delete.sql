-- FUNCTION: public.process_content_item_insert()

-- DROP FUNCTION public.process_content_item_insert();

CREATE OR REPLACE FUNCTION public.process_content_item_delete()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		ids int[];
		content_ids int[];
		cid int;
		published_id int;
		is_virtual boolean;
		char_ids text[];
		o2m_ids int[];
    BEGIN
	
		IF NOT EXISTS(SELECT * FROM information_schema.tables where table_name = 'disable_td_delete_item') THEN
			content_ids := array_agg(distinct(content_id)) from OLD_TABLE;
			content_ids := COALESCE(content_ids, ARRAY[]::int[]);		
			
		FOREACH cid in array content_ids
			LOOP
				select st.status_type_id, c.virtual_type <> 0 into published_id, is_virtual from STATUS_TYPE st
				inner join content c on st.site_id = c.site_id and st.status_type_name = 'Published'
				where c.content_id = cid;

				ids := array_agg(n.content_item_id) from OLD_TABLE n where n.content_id = cid;
									
				IF EXISTS (select * from OLD_TABLE where status_type_id = published_id and not splitted) THEN
					update content_modification set live_modified = now(), stage_modified = now() where content_id = cid;
				ELSE
					update content_modification set stage_modified = now() where content_id = cid;
				END IF;									
								 
            	o2m_ids := array_agg(ca1.attribute_id) from CONTENT_ATTRIBUTE ca1
            		inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID
            		where ca2.CONTENT_ID = cid;
									
				IF o2m_ids is not null AND NOT EXISTS(
					SELECT * FROM information_schema.tables where table_name = 'disable_td_delete_item_o2m_nullify'	
				) THEN
					char_ids := array_agg(unnest::text) from unnest(ids);
									
	                UPDATE content_attribute SET default_value = null
                    	WHERE attribute_id = ANY(o2m_ids)
                    	AND default_value = ANY(char_ids);

					UPDATE content_data SET data = NULL, blob_data = NULL
						WHERE attribute_id = ANY(o2m_ids)
						AND o2m_data = ANY(ids);

					DELETE from VERSION_CONTENT_DATA
						where ATTRIBUTE_ID = ANY(o2m_ids)
						AND data = ANY(char_ids);
									
				END IF;
				
								 
				IF NOT is_virtual THEN
					call qp_delete_items(cid, ids, false);
					call qp_delete_items(cid, ids, true);
				END IF;
								 

			END LOOP;
		END IF;
							 
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_item_delete()
    OWNER TO postgres;
