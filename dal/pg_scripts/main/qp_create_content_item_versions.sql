CREATE OR REPLACE PROCEDURE public.qp_create_content_item_versions(
	ids numeric[],
	last_modified_by numeric
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        delete_ids numeric[];
	    ext_ids numeric[];
	    agg_attrs content_attribute[];
	    attr content_attribute;
	    base_attr content_attribute;
	    current_exts link_multiple[];
	    exts link_multiple[];
	    main link_multiple[];
	    main2 link_multiple[];
	    attr_name text;
	    cid numeric;
	    sql text;
	    m2o_attrs content_attribute[];
	    base_attrs content_attribute[];
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
            INSERT INTO content_item_version (version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by)
            SELECT now(), 'backup', NULL, ci.content_item_id, $2, ci.modified, ci.last_modified_by
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

	    select array_agg(ca.*) into agg_attrs
	    from CONTENT_ATTRIBUTE ca where ca.aggregated and ca.CONTENT_ID = ANY(ext_ids);

	    exts := ARRAY[]::link_multiple[];

	    if agg_attrs is not null then
            FOREACH attr in ARRAY agg_attrs
            loop
                sql := 'select array_agg(row("%s", %s, content_item_id)) from content_%s_united where "%s" = ANY($1)';
                attr_name := lower(attr.attribute_name);
                sql := format(sql, attr_name, attr.content_id, attr.content_id, attr_name);
                execute sql using ids into current_exts;

                if current_exts is not null then
                    exts := array_cat(exts, current_exts);
                end if;

                RAISE NOTICE 'Extension % received: %', attr.content_id,  clock_timestamp();
                RAISE NOTICE 'exts: %', exts;

            end loop;
	    end if;

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


        base_attrs := array_agg(ca.*) from content_attribute ca
        inner join content_attribute rca on ca.attribute_id = rca.back_related_attribute_id
        where rca.content_id in (select link_id from unnest(main));

        m2o_attrs := array_agg(ca.*) from content_attribute ca
        where back_related_attribute_id is not null and content_id in (select link_id from unnest(main));

	    RAISE NOTICE 'M20 defined: %',  clock_timestamp();

	    if m2o_attrs is not null then
            foreach attr in array m2o_attrs
            loop

                base_attr := row(a.*) from unnest(base_attrs) a
                where a.attribute_id = attr.back_related_attribute_id;

                current_ids := array_agg(m.linked_id) from unnest(main) m
                where m.link_id = attr.content_id;

                current_m2o := qp_get_m2o_ids_multiple(
                    base_attr.content_id::int, base_attr.attribute_name::text, current_ids::int[]
                );

                INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
                SELECT i.new_version_id, attr.attribute_id, v.linked_id from unnest(current_m2o) v
                inner join version_items i on v.id = i.id;

                RAISE NOTICE 'M2O % saved: %', attr.attribute_id,  clock_timestamp();

            end loop;
        end if;

        INSERT INTO content_item_status_history
        (content_item_id, user_id, description, created, content_item_version_id,system_status_type_id)
        select v.id, $2, 'Record version backup has been created', now(), v.new_version_id, 2
        from version_items v;

        drop table version_items;
    END;
$BODY$;

ALTER PROCEDURE qp_create_content_item_versions(numeric[], numeric) owner to postgres;