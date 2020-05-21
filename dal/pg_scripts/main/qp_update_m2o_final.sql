CREATE OR REPLACE PROCEDURE public.qp_update_m2o_final(
	id numeric)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    item content_item;
	    max_status numeric;
	    ids int[];
	    ids_to_split int[];
	BEGIN
    	if exists(select * from information_schema.tables where table_name = 'o2m_result_ids') then
            if exists(select * from o2m_result_ids) or exists(select * from CHILD_DELAYS cd where cd.id = $1) then
                item := row(ci.*) from content_item ci where ci.content_item_id = $1;

                max_status := max_status_type_id from content_item_workflow ciw
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ciw.content_item_id = $1;

                if max_status is null then
                    max_status :=  st.status_type_id from content_item
                    inner join content c on content_item.content_id = c.content_id
                    inner join site s on c.site_id = s.site_id
                    inner join status_type st on s.site_id = st.site_id and status_type_name = 'Published'
                    where content_item_id = $1;
                end if;

                if item.status_type_id = max_status and not item.splitted then
                    call qp_merge_delays(ARRAY[id]::int[], item.last_modified_by::int);
                    RAISE NOTICE 'Delays merged: %', clock_timestamp();
                end if;

                update o2m_result_ids o set remove_delays = true
                from child_delays cd where o.id = cd.child_id and cd.id = $1;

                ids := array_agg(o.id) from o2m_result_ids o;
                ids = coalesce(ids, ARRAY[]::int[]);

                update content_item
                set modified = now(), last_modified_by = item.last_modified_by, not_for_replication = true
                where content_item_id = ANY(ids);

                RAISE NOTICE 'Not for replication: %', ids;

                update content_data cd
                set data = $1, blob_data = null, modified = now()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and not r.to_remove;

                insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
                select r.id, r.attribute_id, $1, NULL, now()
                from o2m_result_ids r
                left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
                where not r.to_remove and cd.CONTENT_DATA_ID is null;

                update content_data cd
                set data = null, blob_data = null, modified = now()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and r.to_remove;

                RAISE NOTICE 'content_data updated: %', clock_timestamp();

		        delete from CHILD_DELAYS cd where cd.id = $1 and child_id in (
		            select o.id from o2m_result_ids o where remove_delays
		        );

                RAISE NOTICE 'Child delays removed: %', clock_timestamp();

		        if (item.status_type_id <> max_status or item.splitted) then
                    insert into child_delays (id, child_id)
                    select $1, r.id from o2m_result_ids r
                    inner join content_item ci on r.id = ci.content_item_id
                    left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = $1
                    left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ex.child_id is null and ci.status_type_id = wms.max_status_type_id
                        and (not ci.splitted or ci.splitted and exists(
                            select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and r.id <> $1)
                        )
                        and not r.remove_delays;

                    RAISE NOTICE 'Child delays inserted: %', clock_timestamp();

                    ids_to_split := array_agg(content_item_id) from content_item
                    where content_item_id in (select child_id from child_delays cd where cd.id = $1) and not splitted;

                    if ids_to_split is not null then
                        call qp_split_articles(ids_to_split);
                        RAISE NOTICE 'Articles splitted: %', ids_to_split;
                    end if;

                    update content_item set schedule_new_version_publication = true
                    where content_item_id in (select child_id from child_delays cd where cd.id = $1);

                end if;

                call qp_replicate_items(ids);

                RAISE NOTICE 'Articles replicated: % %', ids, clock_timestamp();

            end if;

            drop table o2m_result_ids;

        end if;
	END;
$BODY$;

alter procedure qp_update_m2o_final(numeric) owner to postgres;

