
CREATE OR REPLACE FUNCTION public.qp_persist_article(input xml)
RETURNS TABLE(id int, modified timestamp without time zone)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    main_id int;
	    ids int[];
	    cid int;
	    splitted boolean;
        modified timestamp without time zone;
	    data_items value[];
	    m2m_data value[];
	    m2o_data value[];
	    m2m_xml xml;
	    val value;

    BEGIN
        create temp table item as
        select x.* from XMLTABLE('/items/item' PASSING input COLUMNS
			id int PATH '@id',
			content_id int PATH '@content_id',
			status_type_id int PATH '@status_type_id',
			archive int PATH '@archive',
			visible int PATH '@visible',
			last_modified_by int PATH '@last_modified_by',
		    delayed boolean PATH '@delayed',
		    cancel_split boolean PATH '@cancel_split',
		    permanent_lock boolean PATH '@permanent_lock',
		    unique_id uuid PATH '@unique_id'
		) x;

        main_id := i.id from item i;

        if main_id = 0 then
            with result as (
                insert into content_item (
                    content_id, status_type_id, not_for_replication, archive, visible,
                    last_modified_by, schedule_new_version_publication, cancel_split, permanent_lock, unique_id
                )
                (
                    select i.content_id, i.status_type_id, true, i.archive, i.visible, i.last_modified_by,
                    coalesce(i.delayed, false), coalesce(i.cancel_split, false), coalesce(i.permanent_lock, false),
                    coalesce(i.unique_id, md5(random()::text || clock_timestamp()::text)::uuid)
                    from item i
                )
                returning content_item.content_item_id, content_item.modified, content_item.splitted
            )
            select result.content_item_id, result.splitted, result.modified
            into main_id, splitted, modified from result;


        else
            with result as (
                update content_item ci
                    set modified = now(), last_modified_by = i.last_modified_by, not_for_replication = true,
                        status_type_id = coalesce(i.status_type_id, ci.status_type_id),
                        archive = coalesce(i.archive, ci.archive),
                        visible = coalesce(i.visible, ci.visible),
                        schedule_new_version_publication = coalesce(i.delayed, ci.schedule_new_version_publication),
                        permanent_lock = coalesce(i.permanent_lock, ci.permanent_lock),
                        unique_id = coalesce(i.unique_id, ci.unique_id)
                from item i where i.id = ci.content_item_id
                returning content_item_id, ci.modified, ci.splitted
            )
            select result.content_item_id, result.splitted, result.modified
            into main_id, splitted, modified from result;
        end if;

        drop table item;

        data_items := array_agg(row(case when x.id <> 0 then x.id else main_id end, x.field_id, x.value))
        from XMLTABLE('/items/item/data' PASSING input COLUMNS
			id int PATH '../@id',
            field_id int PATH '@field_id',
            value text PATH '.'
		) x;

        select
            array_agg(d.*) filter(where ca.attribute_type_id = 13),
            array_agg(row(d.id, ca.link_id, d.data)) filter(where ca.attribute_type_id = 11 and ca.link_id is not null),
            array_agg(row(d.id, d.field_id,
            case
                when ca.attribute_type_id = 13 then ca.back_related_attribute_id::text
                when ca.attribute_type_id = 11 and ca.link_id is not null then ca.link_id::text
                else d.data end))
        into m2o_data, m2m_data, data_items
        from unnest(data_items) d inner join content_attribute ca on d.field_id = ca.attribute_id;

        raise notice 'M2M %', m2m_data;
        raise notice 'M2O %', m2o_data;
        raise notice 'Data %', data_items;

        update content_data cd set data = d.data
        from unnest(data_items) d
        where content_item_id = d.id and attribute_id = d.field_id;

        ids := ARRAY[main_id];

        if m2m_data is not null then

            m2m_xml := xmlelement(name items, xmlagg(x.m2m)) from
            (
                select xmlelement(name item, xmlattributes(
                    d.id, d.field_id as link_id, d.data as value)
                )
                as m2m from unnest(m2m_data) d
            ) x;

            raise notice 'M2M xml %', m2m_xml;

            call qp_update_m2m_values(m2m_xml);
        end if;

        if m2o_data is not null then
            foreach val in array m2o_data
            loop
                call qp_update_m2o(val.id, val.field_id, val.data);
            end loop;
        end if;

        call qp_replicate_items(ids);

        if m2o_data is not null then
            call qp_update_m2o_final(main_id);
        end if;

        call qp_remove_old_aggregates(ids);

        return query select main_id, modified;

	END;
$BODY$;



alter function qp_persist_article(xml) owner to postgres;





