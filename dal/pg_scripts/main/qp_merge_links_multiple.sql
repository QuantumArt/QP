-- PROCEDURE: qp_merge_links_multiple(integer[], boolean)

-- DROP PROCEDURE qp_merge_links_multiple(integer[], boolean);

CREATE OR REPLACE PROCEDURE qp_merge_links_multiple(
	ids integer[],
	force_merge boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		ids_with_links link[];
		new_ids item_link[];
		old_ids item_link[];
		cross_ids item_link[];
	BEGIN
	    ids := coalesce(ids, ARRAY[]::int[]);
		IF coalesce(array_length(ids, 1), 0) = 0 THEN
			RETURN;
		END IF;

		ids_with_links := array_agg(distinct row(i.id, iti.link_id)) from (select unnest(ids) as id) i
  		inner join content_item ci on ci.CONTENT_ITEM_ID = i.id and (ci.SPLITTED or force_merge)
	  	inner join item_to_item iti on iti.l_item_id = ci.content_item_id; 
		ids_with_links := coalesce(ids_with_links, ARRAY[]::link[]);
		IF coalesce(array_length(ids_with_links, 1), 0) = 0 THEN
			RETURN;
		END IF;

		new_ids := array_agg(ila.*)
		from item_link_async ila inner join unnest(ids_with_links) i
		on ila.item_id = i.id and ila.link_id = i.linked_id;
		new_ids := coalesce(new_ids, ARRAY[]::item_link[]);

		old_ids := array_agg(il.*)
		from item_link il inner join unnest(ids_with_links) i
		on il.item_id = i.id and il.link_id = i.linked_id;
		old_ids := coalesce(old_ids, ARRAY[]::item_link[]);

		cross_ids := array_agg(t1.*)
		from unnest(new_ids) t1 inner join unnest(old_ids) t2
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id;
		cross_ids := coalesce(cross_ids, ARRAY[]::item_link[]);

		old_ids := array_agg(t1.*)
		from unnest(old_ids) t1 left join unnest(cross_ids) t2
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
		where t2.item_id is null;
		old_ids := coalesce(old_ids, ARRAY[]::item_link[]);

		new_ids := array_agg(t1.*)
		from unnest(new_ids) t1 left join unnest(cross_ids) t2
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
		where t2.item_id is null;
		new_ids := coalesce(new_ids, ARRAY[]::item_link[]);

  		delete from item_link il using unnest(old_ids) i
  		where il.item_id = i.item_id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id;

  		delete from item_link il using unnest(old_ids) i, content_to_content c
		where il.linked_item_id = i.item_id and il.link_id = i.link_id and il.item_id = i.linked_item_id
		and i.link_id = c.link_id and c.symmetric;

  		insert into item_link (link_id, item_id, linked_item_id)
  		select link_id, item_id, linked_item_id from unnest(new_ids) i;

  		insert into item_link (link_id, item_id, linked_item_id)
  		select i.link_id, i.linked_item_id, i.item_id
		from unnest(new_ids) i
		inner join content_to_content c on i.link_id = c.link_id
		left join item_link il on i.link_id = il.link_id and i.item_id = il.linked_item_id and i.linked_item_id = il.item_id
		where c.symmetric and il.item_id is null;

		IF (array_length(new_ids, 1) > 0) THEN
			create temp table multiple_data as select
				n.item_id, n.link_id, n.linked_item_id,
				ca.attribute_id as linked_attribute_id,
    			(cd.content_item_id is not null) as linked_has_data,
    			(ila.link_id is not null) as linked_has_async
  			from unnest(new_ids) n
			inner join content_to_content cc on n.link_id = cc.link_id and cc.symmetric
    		inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
    		inner join content c on ci.content_id = c.content_id
    		inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    		left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.item_id = ila.linked_item_id;

  			update content_data cd set data = n.link_id from multiple_data n
  			where cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  			and n.linked_has_data;

  			insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  			select distinct n.linked_item_id, n.linked_attribute_id, n.link_id from multiple_data n
  			where not n.linked_has_data and n.linked_attribute_id is not null;

			drop table multiple_data;
		END IF;

  		delete from item_link_async ila using unnest(ids_with_links) i
		where ila.item_id = i.id and ila.link_id = i.linked_id;

	END;

$BODY$;


