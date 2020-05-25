create or replace procedure qp_split_articles(ids integer[], last_modified_by integer DEFAULT 1)
    language plpgsql
as
$$
DECLARE
		content_ids int[];
		cid int;
		items link[];
		ids2 int[];
		table_name text;
		sql text;
    BEGIN
		items := array_agg(row(ci.content_id, ci.content_item_id)) from content_item ci where ci.content_item_id = ANY(ids);
        items = coalesce(items, ARRAY[]::link[]);

		content_ids := array_agg(distinct(i.id)) from unnest(items) i;
		content_ids = coalesce(content_ids, ARRAY[]::int[]);
		
		FOREACH cid in array content_ids
		LOOP
			ids2 := array_agg(i.linked_id) from unnest(items) i where i.id = cid;
			ids2 = coalesce(ids2, ARRAY[]::int[]);
	    	sql := '
					insert into content_%s_async 
					select * from content_%s_async c where content_item_id = ANY($1) and not exists(
						select * from content_%s_async a where a.content_item_id = c.content_item_id
					)';
			sql := FORMAT(sql, cid, cid, cid);
			RAISE NOTICE '%', sql;
			execute sql using ids2;							  
								  
		END LOOP;
										  
  		insert into item_link_async select * from item_to_item ii where l_item_id = ANY(ids)
  		and link_id in (select link_id from content_attribute ca where ca.content_id = ANY(content_ids))
  		and not exists (select * from item_link_async ila where ila.item_id = ii.l_item_id);										  
										  
	END
$$;

alter procedure qp_split_articles(integer[], integer) owner to postgres;

