CREATE OR REPLACE PROCEDURE public.qp_create_content_item_access(
	ids numeric[]
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    items content_item[];
	BEGIN
        items := array_agg(row(ci.*)) from content_item ci inner join content c on ci.content_id = c.content_id
        where content_item_id = ANY(ids) and c.allow_items_permission = 1;

        if items is null then
            return;
        end if;

        INSERT INTO content_item_access (content_item_id, user_id, permission_level_id, last_modified_by)
	    SELECT i.content_item_id, i.last_modified_by, 1, 1 FROM unnest(items) i WHERE i.last_modified_by <> 1;

        INSERT INTO content_item_access (content_item_id, user_id, group_id, permission_level_id, last_modified_by)
	    SELECT i.content_item_id, ca.user_id, ca.group_id, ca.permission_level_id, 1
	    FROM content_access AS ca INNER JOIN unnest(items) i ON ca.content_id = i.content_id
		LEFT OUTER JOIN user_group AS g ON g.group_id = ca.group_id
	    WHERE (ca.user_id <> i.last_modified_by or ca.user_id IS NULL)
		AND ((g.shared_content_items = 0 and g.GROUP_ID <> 1) OR g.group_id IS NULL) AND ca.propagate_to_items = 1;


	  INSERT INTO content_item_access
		(content_item_id, group_id, permission_level_id, last_modified_by)
	  SELECT DISTINCT
		i.content_item_id, g.group_id, 1, 1
	  FROM unnest(items) i
		LEFT OUTER JOIN user_group_bind AS gb ON gb.user_id = i.last_modified_by
		LEFT OUTER JOIN user_group AS g ON g.group_id = gb.group_id
	  WHERE
		g.shared_content_items = 1 and g.GROUP_ID <> 1;

    END;
$BODY$;
