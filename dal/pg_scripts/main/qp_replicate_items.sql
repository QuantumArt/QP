-- PROCEDURE: public.qp_replicate_items(integer[], integer[], integer)

-- DROP PROCEDURE public.qp_replicate_items(integer[], integer[], integer);

CREATE OR REPLACE PROCEDURE public.qp_replicate_items(
	ids integer[],
	attr_ids integer[] DEFAULT NULL::integer[],
	modification_update_interval integer DEFAULT '-1'::integer)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		setting_value text;
		default_modification_update_interval int = 30;
		modification_update_interval int;
		content_ids int[];
		id int;
		site_id int;		
		none_id int;
		published_id int;
		articles content_item[];
		live_expired boolean = true;
		stage_expired boolean = true;
		live_modified timestamp without time zone;
		stage_modified timestamp without time zone;
		sync_ids int[];
		async_ids int[];
		sync_ids_delayed int[];
		
		table_name text;
		sql text;
    BEGIN
		setting_value := VALUE from APP_SETTINGS where key = 'CONTENT_MODIFICATION_UPDATE_INTERVAL';
		IF setting_value is not null and qp_is_numeric(setting_value) THEN
			default_modification_update_interval := setting_value::numeric::int;
		END IF;
		
		IF modification_update_interval < 0 THEN
			modification_update_interval := default_modification_update_interval;
		END IF;
		
		articles := array_agg(ci.*) from content_item ci where ci.content_item_id = ANY(ids);
		content_ids := array_agg(distinct(a.content_id)) from unnest(articles) a;
												  
		
		FOREACH id in array content_ids
		LOOP
			select st1.status_type_id, st2.status_type_id into none_id, published_id
			from status_type st1 inner join status_type st2
			on st1.site_id = st2.site_id and st1.status_type_name = 'None' and st2.status_type_name = 'Published'
			where st1.site_id in (select c.site_id from content c where c.content_id = id);										  
										  
			IF modification_update_interval > 0 THEN
            	select cm.live_modified, cm.stage_modified into live_modified, stage_modified
									   from CONTENT_MODIFICATION cm where cm.content_id = id;
				live_expired := extract(epoch from now() - live_modified) >= modification_update_interval;
				stage_expired := extract(epoch from now() - stage_modified) >= modification_update_interval;
			END IF;
									   
			sync_ids := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and not a.splitted;
			async_ids := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and a.splitted;
			sync_ids_delayed := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and not a.splitted and a.schedule_new_version_publication;
			sync_ids_delayed := coalesce(sync_ids_delayed, ARRAY[]::int[]);										  
								   
			IF sync_ids is not null THEN
				call qp_upsert_items(id, sync_ids, sync_ids_delayed, none_id, false);
				call qp_delete_items(id, sync_ids, true);
				call qp_update_items_with_content_data_pivot(id, sync_ids, false, attr_ids);
			END IF;
										  
			IF async_ids is not null THEN
				call qp_upsert_items(id, async_ids, ARRAY[]::int[], none_id, true);
				call qp_update_items_flags(id, async_ids, false);
				call qp_update_items_with_content_data_pivot(id, async_ids, true, attr_ids);										  
			END IF;					
        
			IF EXISTS (
				select * from unnest(articles) a where a.content_id = id and (
					a.cancel_split or (not a.splitted and a.status_type_id = published_id)
				)
			) THEN
				IF (live_expired or stage_expired) THEN
                	update content_modification cm set live_modified = now(), stage_modified = now() where cm.content_id = id;
				END IF;
			ELSE
				IF (stage_expired) THEN
                	update content_modification cm set stage_modified = now() where cm.content_id = id;									 
				END IF;
			END IF;
									 
		END LOOP;
									 
   		update content_item set not_for_replication = false, CANCEL_SPLIT = false where content_item_id = ANY(ids);							  
										  
	END;
$BODY$;

alter procedure qp_replicate_items(integer[], integer[], integer) owner to postgres;