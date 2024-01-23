CREATE OR REPLACE PROCEDURE qp_batch_delete_contents(site_id int, count_to_del int DEFAULT 20)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    ids int[];
	BEGIN
        ids := array_agg(c.content_id) from content c where c.site_id = $1 limit $2;
        if ids is not null then

	        delete from union_contents
	        where virtual_content_id = ANY(ids) or union_content_id = ANY(ids) or master_content_id = ANY(ids);

	        delete from union_attrs where virtual_attr_id in (
	            select attribute_id from content_attribute where content_id = ANY(ids)
	        );

	        delete from user_query_contents
	        where virtual_content_id = ANY(ids) or real_content_id = ANY(ids);

	        delete from user_query_attrs
	        where virtual_content_id = ANY(ids);

	        delete from user_query_attrs where user_query_attr_id in (
	            select attribute_id from content_attribute where content_id = ANY(ids)
	        );

	        delete from content where content_id = ANY(ids);
	    end if;

    END;
$BODY$;
