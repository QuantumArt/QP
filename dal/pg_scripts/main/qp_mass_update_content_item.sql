CREATE OR REPLACE FUNCTION qp_mass_update_content_item(input xml, content_id int, last_modified_by int, not_for_replication int, create_versions bool, import_only bool DEFAULT false)
RETURNS TABLE(id numeric)
LANGUAGE 'plpgsql'
as
$$
DECLARE
        new_ids int[];
        old_ids int[];
        old_non_splitted_ids int[];
        old_splitted_ids int[];
        new_non_splitted_ids int[];
        new_splitted_ids int[];
    BEGIN

        create temp table articles as
        select x.* from XMLTABLE('/ITEMS/ITEM' PASSING input COLUMNS
			content_item_id numeric PATH 'CONTENT_ITEM_ID',
			status_type_id numeric PATH 'STATUS_TYPE_ID',
			archive numeric PATH 'ARCHIVE',
			visible numeric PATH 'VISIBLE'
		) x;

        with result as (
            insert into content_item (
                content_id, status_type_id, not_for_replication, archive, visible,last_modified_by
            )
            (
                select $2, a.status_type_id, $4 = 1, a.archive, a.visible, $3 from articles a
                where a.content_item_id = 0
            )
            returning content_item.content_item_id
        )
        select array_agg(result.content_item_id::int) from result into new_ids;
		new_ids := coalesce(new_ids, ARRAY[]::int[]);

        RAISE NOTICE 'New Ids: %', new_ids;

        if not import_only then

            perform ci.content_item_id from content_item ci
            where ci.content_item_id in (select a.content_item_id from articles a)
            for update;

            old_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select a.content_item_id from articles a);

            RAISE NOTICE 'Old Ids: %', old_ids;

            if create_versions then
                call qp_create_content_item_versions(old_ids, $3);
            end if;

            old_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_ids) i(id)) and splitted;

            RAISE NOTICE 'Old Splitted Ids: %', old_splitted_ids;

            old_non_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_ids) i(id)) and not splitted;

            RAISE NOTICE 'Old Non-Splitted Ids: %', old_non_splitted_ids;

        end if;

        update content_item ci
            set modified = now(),
                last_modified_by = $3,
                status_type_id = coalesce(a.status_type_id, ci.status_type_id),
                archive = coalesce(a.archive, ci.archive),
                visible = coalesce(a.visible, ci.visible)
        from articles a where a.content_item_id = ci.content_item_id;

        drop table articles;

        if not import_only then

            new_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_non_splitted_ids) i(id)) and splitted;

            RAISE NOTICE 'New Splitted Ids: %', new_splitted_ids;

            new_non_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_splitted_ids) i(id)) and not splitted;

            RAISE NOTICE 'New Non-Splitted Ids: %', new_non_splitted_ids;

            if new_splitted_ids is not null then
                call qp_split_articles(new_splitted_ids, $3);
            end if;

            if new_non_splitted_ids is not null then
                call qp_merge_articles(new_non_splitted_ids, $3, true);
            end if;
        end if;

        return query select unnest::numeric from unnest(new_ids);

    END;

$$;





