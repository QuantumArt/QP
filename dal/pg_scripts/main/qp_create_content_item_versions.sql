CREATE OR REPLACE PROCEDURE public.qp_create_content_item_versions(
	ids numeric[],
	last_modified_by numeric
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        delete_ids numeric[];
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

        update version_items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
        from version_items items
        inner join content_item ci on items.id = ci.CONTENT_ITEM_ID
        inner join content c on c.CONTENT_ID = ci.CONTENT_ID;

        delete_ids := array_agg(content_item_version_id)
        from (
            select content_item_id, content_item_version_id,
            row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
            from content_item_version civ
            where content_item_id = ANY($1)
        ) c inner join version_items items
        on items.id = c.content_item_id and c.num >= items.max_num;

        if delete_ids is not null then
            DELETE from content_item_version WHERE content_item_version_id = ANY(delete_ids);
        end if;


        drop table version_items;
    END;
$BODY$;

alter PROCEDURE qp_create_content_item_versions(numeric[], numeric) owner to postgres;