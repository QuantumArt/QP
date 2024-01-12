CREATE OR REPLACE PROCEDURE qp_create_content_item_versions(
	ids numeric[],
	last_modified_by numeric
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        delete_ids numeric[];
	    ext_ids numeric[];
	    agg_ids numeric[];
	    attr content_attribute;
	    exts link_multiple[];
	    main link_multiple[];
	    main2 link_multiple[];
	    m2o_attrs content_attribute[];
	    current_ids numeric[];
	    current_m2o link[];

    BEGIN
	    create temp table version_items(
	        id numeric primary key,
	        cnt numeric,
	        last_version_id numeric,
	        new_version_id numeric,
	        content_id numeric,
	        max_num numeric
	    );

        insert into version_items (id, cnt)
        select i.id, count(civ.content_item_version_id) from unnest(ids) i(id)
        left join content_item_version civ on civ.content_item_id = i.id
        group by i.id;

        RAISE NOTICE 'Init versions completed: %',  clock_timestamp();

        update version_items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
        from version_items items
        inner join content_item ci on items.id = ci.CONTENT_ITEM_ID
        inner join content c on c.CONTENT_ID = ci.CONTENT_ID;

        RAISE NOTICE 'max_num updated: %',  clock_timestamp();

        delete_ids := array_agg(content_item_version_id)
        from (
            select content_item_id, content_item_version_id,
            row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
            from content_item_version civ
            where content_item_id = ANY($1)
        ) c inner join version_items items
        on items.id = c.content_item_id and c.num >= items.max_num;

        if delete_ids is not null then
            call qp_before_content_item_version_delete(delete_ids::int[]);
            DELETE from content_item_version WHERE content_item_version_id = ANY(delete_ids);
        end if;

        RAISE NOTICE 'Exceeded deleted: %',  clock_timestamp();

        WITH inserted(id, content_item_id) AS (
            INSERT INTO content_item_version (
                version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by,
                status_type_id, archive, visible )
            SELECT now(), 'backup', NULL, ci.content_item_id, $2, ci.modified, ci.last_modified_by,
                ci.status_type_id, ci.archive::int::boolean, ci.visible::int::boolean
            FROM content_item ci WHERE CONTENT_ITEM_ID = ANY(ids)
            RETURNING content_item_version_id, content_item_id
        )
        update version_items vi set new_version_id = i.id from inserted i where vi.id = i.content_item_id;

        RAISE NOTICE 'New versions updated: %',  clock_timestamp();

        select array_agg(a.data::numeric) into ext_ids from
        (
            select distinct DATA from content_data
            where CONTENT_ITEM_ID = ANY(ids)
            and DATA is not null
            and ATTRIBUTE_ID in (
                select attribute_id from CONTENT_ATTRIBUTE where content_id in (select distinct content_id from version_items) and IS_CLASSIFIER
            )
	    ) as a;

	    RAISE NOTICE 'Extensions defined: %',  clock_timestamp();

	    agg_ids := array_agg(ca.attribute_id) from CONTENT_ATTRIBUTE ca
	    where ca.aggregated and ca.CONTENT_ID = ANY(ext_ids);

	    exts := array_agg(row(cd.o2m_data, ca.content_id, cd.content_item_id))
	    from content_data cd inner join CONTENT_ATTRIBUTE ca on ca.attribute_id = cd.attribute_id
	    where cd.o2m_data = ANY(ids) and cd.attribute_id = ANY(agg_ids);

	    RAISE NOTICE 'Extensions received: %',  clock_timestamp();
        RAISE NOTICE 'exts: %', exts;

        main := array_agg(row(i.new_version_id, e.link_id, e.linked_id))
        from unnest(exts) e inner join version_items i on e.id = i.id;
        RAISE NOTICE 'main: %', main;

        main2 := array_agg(row(i.new_version_id, i.content_id, i.id)) from version_items i;
        RAISE NOTICE 'main2: %', main2;

        main := array_cat(main, main2);


        RAISE NOTICE 'Main defined: %',  clock_timestamp();

        INSERT INTO version_content_data (attribute_id, content_item_version_id, data, blob_data, o2m_data, created)
        SELECT attribute_id, m.id, data, blob_data, o2m_data, now()
        FROM content_data cd inner join unnest(main) m on cd.CONTENT_ITEM_ID = m.linked_id;

	    RAISE NOTICE 'Data saved: %',  clock_timestamp();

        INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
        SELECT m.id, ca.attribute_id, linked_item_id
        FROM item_link_united AS il
        INNER JOIN content_attribute AS ca ON ca.link_id = il.link_id
        INNER JOIN content_item AS ci ON ci.content_id =  ca.content_id AND ci.content_item_id = il.item_id
        inner join unnest(main) m  on il.item_id = m.linked_id;

	    RAISE NOTICE 'M2M saved: %',  clock_timestamp();

	    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
        SELECT m.id, ca.attribute_id, cd.content_item_id
        FROM content_data AS cd
        INNER JOIN content_attribute AS ca ON ca.BACK_RELATED_ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join unnest(main) m on cd.O2M_DATA = m.linked_id and ca.CONTENT_ID = m.link_id;

	    RAISE NOTICE 'M2O saved: %',  clock_timestamp();

        INSERT INTO content_item_status_history
        (content_item_id, user_id, description, created, content_item_version_id,system_status_type_id)
        select v.id, $2, 'Record version backup has been created', now(), v.new_version_id, 2
        from version_items v;

        drop table version_items;
    END;
$BODY$;
