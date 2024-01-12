-- FUNCTION: process_content_item_update()

-- DROP FUNCTION process_content_item_update();

CREATE OR REPLACE FUNCTION process_content_item_update()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
	DECLARE
		splitted_ids int[];
		not_for_replication_ids int[];
		locked_by_ids int[];
		modified_ids int[];
		ids int[];
		content_ids int[];
		cid int;
		items content_item[];
		ids_to_set int[];
		ids_to_reset int[];
		sync_ids int[];
		async_ids int[];
		none_id int;
		published_id int;
		sql text;
    BEGIN
		select
			array_agg(i.content_item_id) filter (where i.splitted <> o.splitted),
			array_agg(i.content_item_id) filter (where i.not_for_replication <> o.not_for_replication),
			array_agg(i.content_item_id) filter (where i.locked_by <> o.locked_by),
			array_agg(i.content_item_id) filter (where i.modified <> o.modified)
		into splitted_ids, not_for_replication_ids, locked_by_ids, modified_ids
		from new_table i inner join old_table o on i.content_item_id = o.content_item_id;

		RAISE NOTICE 'Splitted ids: %', splitted_ids;

		IF splitted_ids is not null THEN
			update content_data set splitted = i.splitted
			from new_table i where content_data.content_item_id = i.content_item_id
			and content_data.content_item_id = ANY(splitted_ids);

			RETURN NULL;
		END IF;

		RAISE NOTICE 'Not for Replication ids: %', not_for_replication_ids;

		IF not_for_replication_ids is not null THEN
			update content_data set not_for_replication = i.not_for_replication
			from new_table i where content_data.content_item_id = i.content_item_id
			and content_data.content_item_id = ANY(not_for_replication_ids);

			RETURN NULL;
		END IF;

		IF locked_by_ids is not null THEN
			RETURN NULL;
		END IF;

		insert into content_data (content_item_id, attribute_id, not_for_replication)
		select i.content_item_id, ca.attribute_id, i.not_for_replication
		from new_table i inner join content_attribute ca on i.content_id = ca.content_id
		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
		where cd.CONTENT_DATA_ID is null;

		content_ids := array_agg(distinct(content_id)) from new_table
		where content_id in (select content_id from content where virtual_type = 0);
		content_ids := COALESCE(content_ids, ARRAY[]::int[]);

		FOREACH cid in array content_ids
			LOOP

				select st1.status_type_id, st2.status_type_id into none_id, published_id
				from status_type st1 inner join status_type st2
				on st1.site_id = st2.site_id and st1.status_type_name = 'None' and st2.status_type_name = 'Published'
				where st1.site_id in (select c.site_id from content c where c.content_id = cid);

				items := array_agg(n.*) from new_table n where n.content_id = cid;
				ids := array_agg(i.content_item_id) from unnest(items) i;
				async_ids := array_agg(i.content_item_id) from unnest(items) i where i.cancel_split;

				sql := '
					create temp table ids_with_splitted as
						select content_item_id, (curr_weight < front_weight and is_workflow_async) or
            				(curr_weight = workflow_max_weight and delayed) as splitted, not_for_replication
						from (
            				select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight,
            				max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight,
							case when i2.id is not null then false else ciw.is_async end as is_workflow_async,
            				ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed, ci.not_for_replication
            				from content_item ci inner join UNNEST($1) i(id) on i.id = ci.content_item_id
            				left join UNNEST($2) i2(id) on i2.id = ci.content_item_id
            				inner join content_%s c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID
            				inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID
            				inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID
            				left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
            				left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID
            				left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID
            			) as main';

				sql := FORMAT(sql, cid);
				RAISE NOTICE 'IDS with splitted %', sql;
				EXECUTE sql using ids, async_ids;

				select
					array_agg(i2.content_item_id) filter (where not i2.splitted and i.splitted),
					array_agg(i2.content_item_id) filter (where i2.splitted and not i.splitted),
					array_agg(i2.content_item_id) filter (where not i2.splitted and not i2.not_for_replication),
					array_agg(i2.content_item_id) filter (where i2.splitted and not i2.not_for_replication)
				into ids_to_reset, ids_to_set, sync_ids, async_ids
				from ids_with_splitted i2 inner join (select * from unnest(items)) i on i2.content_item_id = i.content_item_id;

				drop table ids_with_splitted;

				IF ids_to_set is not null THEN
				    RAISE NOTICE 'ids to set splitted: %', ids_to_set;
            		insert into content_item_splitted(content_item_id)
					select id from (select unnest(ids_to_set) as id) base
            		where not exists (select * from content_item_splitted cis where cis.content_item_id = base.id);

					update content_item set splitted = true where content_item_id = ANY(ids_to_set);
				END IF;

				IF ids_to_reset is not null THEN
				    RAISE NOTICE 'ids to reset splitted: %', ids_to_reset;
					delete from content_item_splitted where content_item_id = ANY(ids_to_reset);
					update content_item set splitted = false where content_item_id = ANY(ids_to_reset);
				END IF;

				IF sync_ids is not null THEN
				    RAISE NOTICE 'ids to update sync: %', sync_ids;
					call qp_upsert_items(cid, sync_ids, ARRAY[]::int[], none_id, false);
					call qp_delete_items(cid, sync_ids, true);
				END IF;

				IF async_ids is not null THEN
				    RAISE NOTICE 'ids to update async: %', async_ids;
					call qp_upsert_items(cid, async_ids, ARRAY[]::int[], none_id, true);
					call qp_update_items_flags(cid, async_ids, false);
				END IF;

			END LOOP;

		IF modified_ids is not null THEN
			insert into content_item_status_history
			(content_item_id, status_type_id, archive, visible, user_id, description, created)
			select i.content_item_id, i.status_type_id, i.archive = 1, i.visible = 1, i.last_modified_by, st.description, now()
			from new_table i INNER JOIN status_type st ON i.status_type_id = st.status_type_id
			where i.content_item_id = ANY(modified_ids);
		END IF;

		RETURN NULL;
	END
$BODY$;


