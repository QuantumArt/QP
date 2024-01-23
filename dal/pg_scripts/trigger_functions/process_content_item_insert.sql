-- FUNCTION: process_content_item_insert()

-- DROP FUNCTION process_content_item_insert();

CREATE OR REPLACE FUNCTION process_content_item_insert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
	DECLARE
	    all_ids int[];
		ids int[];
		ids2 int[];
		content_ids int[];
		cid int;
		none_id int;
    BEGIN

	    all_ids := array_agg(content_item_id) from NEW_TABLE;
		all_ids := COALESCE(all_ids, ARRAY[]::int[]);
	    call qp_create_content_item_access(all_ids);

		insert into content_data (content_item_id, attribute_id, not_for_replication)
		select i.content_item_id, ca.attribute_id, i.not_for_replication
		from new_table i inner join content_attribute ca on i.content_id = ca.content_id;

		content_ids := array_agg(distinct(content_id)) from NEW_TABLE;
		content_ids := COALESCE(content_ids, ARRAY[]::int[]);
		FOREACH cid in array content_ids
		LOOP
			none_id := st.status_type_id from STATUS_TYPE st
			inner join content c on st.site_id = c.site_id and st.status_type_name = 'None'
			where c.content_id = cid;

			ids := array_agg(n.content_item_id) from new_table n
						where n.content_id = cid and not n.not_for_replication;
			ids := COALESCE(ids, ARRAY[]::int[]);

			ids2 := array_agg(n.content_item_id) from new_table n
						where n.content_id = cid and not n.not_for_replication and n.schedule_new_version_publication;
			ids2 := COALESCE(ids2, ARRAY[]::int[]);

			call qp_upsert_items(cid, ids, ids2, none_id, false);

		END LOOP;
		RETURN NULL;
	END
$BODY$;


