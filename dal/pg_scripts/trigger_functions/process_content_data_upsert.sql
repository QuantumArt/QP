-- FUNCTION: public.process_content_data_upsert()

-- DROP FUNCTION public.process_content_data_upsert();

CREATE OR REPLACE FUNCTION public.process_content_data_upsert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		ids int[];
		async_ids int[];
	    o2m_ids int[];
		attribute_ids int[];
		attr_id int;
		attr content_attribute;
		source text;
		column_type text;
		sql text;
    BEGIN

		IF TG_OP = 'UPDATE' THEN
            o2m_ids := array_agg(i.content_data_id)
            from new_table i
                inner join old_table o on i.content_data_id = i.content_data_id
                inner join content_attribute ca on ca.attribute_id = i.attribute_id
		        where ca.attribute_type_id = 11 and ca.link_id is null
		        and coalesce(i.data, '') <> coalesce(o.data, '');
		ELSE
            o2m_ids := array_agg(i.content_data_id)
            from new_table i
                inner join content_attribute ca on ca.attribute_id = i.attribute_id
		        where ca.attribute_type_id = 11 and ca.link_id is null
		        and i.data is not null;
        END IF;

         IF o2m_ids is not null THEN
            update content_data set o2m_data = data::numeric where content_data_id = ANY(o2m_ids);
         END IF;

	  	IF NOT EXISTS(select content_data_id from new_table where not not_for_replication) THEN
			RETURN NULL;
		END IF;
		
		IF TG_OP = 'UPDATE' THEN
			IF EXISTS (
				select * from new_table i inner join old_table o on i.content_data_id = o.content_data_id
				where i.splitted <> o.splitted or i.not_for_replication <> o.not_for_replication
				or coalesce(i.o2m_data, 0) <> coalesce(o.o2m_data, 0)) THEN
					RETURN NULL;
			END IF;
		END IF;
		
		attribute_ids := array_agg(distinct(attribute_id)) from new_table;
		attribute_ids := COALESCE(attribute_ids, ARRAY[]::int[]);
		FOREACH attr_id in array attribute_ids
		LOOP
			attr := row(ca.*) from content_attribute ca where ca.attribute_id = attr_id;
								  
			ids := array_agg(i.content_item_id) from new_table i								  
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = attr.attribute_id and not ci.not_for_replication and c.virtual_type = 0 
				and not ci.splitted;
			ids := COALESCE(ids, ARRAY[]::int[]);
								  
			async_ids := array_agg(i.content_item_id) from new_table i							  
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = attr.attribute_id and not ci.not_for_replication and c.virtual_type = 0 
				and ci.splitted;
			async_ids := COALESCE(async_ids, ARRAY[]::int[]);
								  
			IF attr.attribute_type_id in (2,3,11,13) THEN
				column_type := 'numeric';
			ELSEIF attr.attribute_type_id in (4,5,6) THEN
				column_type := 'timestamp without time zone';
			ELSE
				column_type := 'text';								  
			END IF;					  
								  
	   		IF attr.attribute_type_id in (9,10) THEN
				source := 'coalesce(cd.data, cd.blob_data)';
			ELSE
				source := 'qp_correct_data(cd.data::text, %s, %s, ''%s'')';
				source := FORMAT(source, 
					attr.attribute_type_id, attr.attribute_size, coalesce(attr.default_value, '')
				); 								  
			END IF;
								  
			sql := 
				'update %s d set "%s" = %s::%s from content_data cd, unnest($1) where d.content_item_id = unnest' ||
				' and cd.attribute_id = %s and cd.content_item_id = d.content_item_id';
				
								  
			
			IF array_length(ids, 1) > 0 THEN
				
				sql := FORMAT(sql, 'content_' || attr.content_id, lower(attr.attribute_name), source, column_type, attr.attribute_id);
				RAISE NOTICE '%', sql;
				execute sql using ids;
								  
			END IF;
								  
			IF array_length(async_ids, 1) > 0 THEN
				sql := FORMAT(sql, 'content_' || attr.content_id || '_async', attr.attribute_name, source, attr.attribute_id);
				RAISE NOTICE '%', sql;
				execute sql using async_ids;
			END IF;
								  
		END LOOP;
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_data_upsert()
    OWNER TO postgres;