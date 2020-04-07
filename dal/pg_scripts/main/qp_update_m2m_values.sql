-- PROCEDURE: public.qp_update_m2m_values(xml)

-- DROP PROCEDURE public.qp_update_m2m_values(xml);

CREATE OR REPLACE PROCEDURE public.qp_update_m2m_values(
	xml_parameter xml)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		new_ids link_multiple_splitted[];
		old_ids link_multiple_splitted[];
		cross_ids link_multiple_splitted[];
	BEGIN
		create temp table field_values as	
		select x.*, ci.splitted from XMLTABLE(
		'/items/item'  
		PASSING XMLPARSE(DOCUMENT xml_parameter) 
		COLUMNS
			id int PATH '@id',
			link_id int PATH '@linkId',
			value text PATH '@value'
		) x inner join content_item ci on x.id = ci.content_item_id;
		
		new_ids := array_agg(row(a.id, a.link_id, unnest, a.splitted)) 
		from field_values a, unnest(
		    case when a.value = '' then ARRAY[]::int[] else regexp_split_to_array(a.value, E',\\s*')::int[] end
		);
		new_ids := coalesce(new_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'New ids: %', new_ids;
							
		old_ids := array_agg(row(c.*)) from
		(					
		  select ila.item_id, ila.link_id, ila.linked_item_id, f.splitted
		  from item_link_async ila inner join field_values f
		  on ila.link_id = f.link_id and ila.item_id = f.id
		  where f.splitted
		  union all
		  select il.item_id, il.link_id, il.linked_item_id, f.splitted
		  from item_link il inner join field_values f
		  on il.link_id = f.link_id and il.item_id = f.id
		  where not f.splitted
		) c;
		old_ids := coalesce(old_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'Old ids: %', old_ids;		
		
		cross_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))
		from unnest(new_ids) t1 inner join unnest(old_ids) t2 
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id;
		cross_ids := coalesce(cross_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'Cross ids: %', cross_ids;	
		
		old_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))							  
		from unnest(old_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id
		where t2.id is null;
		old_ids := coalesce(old_ids, ARRAY[]::link_multiple_splitted[]);
							
		new_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))							  
		from unnest(new_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id
		where t2.id is null;
		new_ids := coalesce(new_ids, ARRAY[]::link_multiple_splitted[]);
							  
  		delete from item_link_async il using field_values f
		where il.item_id = f.id and il.link_id = f.link_id
		and not f.splitted;
							
  		delete from item_link_async il using unnest(old_ids) i
  		where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_id
  		and i.splitted;
							
  		delete from item_link il using unnest(old_ids) i
  		where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_id
  		and not i.splitted;							

  		delete from item_link_async il using unnest(old_ids) i, content_to_content c
  		where il.linked_item_id = i.id and il.link_id = i.link_id and il.item_id = i.linked_id 
		and c.link_id = i.link_id
  		and not i.splitted and c.symmetric;
							
  		delete from item_link il using unnest(old_ids) i, content_to_content c
  		where il.linked_item_id = i.id and il.link_id = i.link_id and il.item_id = i.linked_id
		and c.link_id = i.link_id							
  		and not i.splitted and c.symmetric;
							
  		insert into item_link_async (link_id, item_id, linked_item_id)
  		select link_id, id, linked_id from unnest(new_ids)
  		where splitted;

  		insert into item_link (link_id, item_id, linked_item_id)
  		select link_id, id, linked_id from unnest(new_ids)
  		where not splitted;
							
  		insert into item_link (link_id, item_id, linked_item_id)
  		select i.link_id, i.linked_id, i.id
		from unnest(new_ids) i, content_to_content c
  		where i.link_id = c.link_id and i.id <> i.linked_id
		and not i.splitted and c.symmetric;							
							
		IF (array_length(new_ids, 1) > 0) THEN
			create temp table multiple_data as select 
				n.id, n.link_id, n.linked_id, n.splitted,
				ca.attribute_id as linked_attribute_id,
    			(cd.content_item_id is not null) as linked_has_data,
    			ci.splitted as linked_splitted,
    			(ila.link_id is not null) as linked_has_async
  			from unnest(new_ids) n
			inner join content_to_content cc on n.link_id = cc.link_id and cc.symmetric
    		inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_id
    		inner join content c on ci.content_id = c.content_id
    		inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    		left join item_link_async ila on n.link_id = ila.link_id and n.linked_id = ila.item_id and n.id = ila.linked_item_id;						
							
  			update content_data cd set data = n.link_id from multiple_data n
  			where cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_id
  			and not n.splitted and n.linked_has_data;
							
  			insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  			select distinct n.linked_id, n.linked_attribute_id, n.link_id from multiple_data n
  			where not n.splitted and not n.linked_has_data and n.linked_attribute_id is not null;

  			insert into item_link_async(link_id, item_id, linked_item_id)
  			select n.link_id, n.linked_id, n.id from multiple_data n
  			where not n.splitted and n.linked_splitted and not n.linked_has_async and n.linked_attribute_id is not null	;
							
			drop table multiple_data;
		END IF;
		drop table field_values;
	END;
	
$BODY$;

alter procedure qp_update_m2m_values(xml) owner to postgres;