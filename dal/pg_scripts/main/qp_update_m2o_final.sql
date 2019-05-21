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
            if exists(select * from o2m_result_ids) or exists(select * from CHILD_DELAYS where id = $1) then
                item := ci.* from content_item ci where ci.content_item_id = $1;

                max_status := max_status_type_id from content_item_workflow ciw
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ciw.content_item_id = $1;

                if item.status_type_id = max_status and not item.splitted then
                    call qp_merge_delays(ARRAY[id]::int[], item.last_modified_by);
                end if;

                update o2m_result_ids o set remove_delays = true
                from child_delays cd where o.id = cd.child_id and cd.id = $1;

                ids := array_agg(id) from o2m_result_ids;

                update content_item
                set modified = now(), last_modified_by = item.last_modified_by, not_for_replication = 1
                where content_item_id = ANY(ids);

                update content_data cd
                set cd.data = $1, cd.blob_data = null, cd.modified = now()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and not r.to_remove;

                insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
                select r.id, r.attribute_id, $1, NULL, now()
                from o2m_result_ids r left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
                where not r.to_remove and cd.CONTENT_DATA_ID is null;

                update content_data cd
                set content_data.data = null, content_data.blob_data = null, content_data.modified = getdate()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and r.to_remove;

		        delete from CHILD_DELAYS where id = $1 and child_id in (
		            select id from o2m_result_ids where remove_delays
		        );

		        if (item.status_type_id <> coalesce(max_status, 0) or item.splitted) then
                    insert into child_delays (id, child_id)
                    select $1, r.id from o2m_result_ids r
                    inner join content_item ci on r.id = ci.content_item_id
                    left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = $1
                    left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ex.child_id is null and ci.status_type_id = wms.max_status_type_id
                        and (not ci.splitted or ci.splitted and exists(select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and id <> $1))
                        and not r.remove_delays;

                    ids_to_split := array_agg(content_item_id) from content_item
                    where content_item_id in (select child_id from child_delays where id = $1) and not splitted;

                    call qp_split_articles(ids_to_split);

                    update content_item set schedule_new_version_publication = 1
                    where content_item_id in (select child_id from child_delays where id = $1);

                end if;

                call qp_replicate_items(ids);
            end if;

            drop table o2m_result_ids;

        end if;
	END;
$BODY$;

alter procedure qp_update_m2o_final(numeric) owner to postgres;

