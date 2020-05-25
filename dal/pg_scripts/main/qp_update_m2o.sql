-- PROCEDURE: public.qp_update_m2o(numeric, numeric, text, boolean)

-- DROP PROCEDURE public.qp_update_m2o(numeric, numeric, text, boolean);

CREATE OR REPLACE PROCEDURE public.qp_update_m2o(
	id numeric,
	field_id numeric,
	ids text,
	update_archive boolean default false)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    attr content_attribute;
	    new_ids int[];
	    old_ids int[];
	    cross_ids int[];
	    archive_ids int[];
	BEGIN
	    attr := row(ca.*) from content_attribute ca where ca.attribute_id = field_id;

	    old_ids := array_agg(cd.content_item_id)
	    from content_data cd where cd.attribute_id = attr.back_related_attribute_id and cd.o2m_data = id;
	    old_ids = coalesce(old_ids, ARRAY[]::int[]);


		RAISE NOTICE 'Start: %', clock_timestamp();
		IF ids is null OR ids = '' THEN
			new_ids = ARRAY[]::int[];
		ELSE
			new_ids := regexp_split_to_array(ids, E',\\s*')::int[];
		END IF;

		cross_ids := new_ids & old_ids;
		old_ids := old_ids - cross_ids;
		new_ids := new_ids - cross_ids;

		RAISE NOTICE 'Arrays calculated: %, to add: %', clock_timestamp(), new_ids;

		IF not update_archive and array_length(old_ids, 1) > 1 THEN
			archive_ids := array_agg(content_item_id) from content_item where content_item_id = ANY(old_ids) AND archive = 1;
			archive_ids = coalesce(archive_ids, ARRAY[]::int[]);
			old_ids := old_ids - archive_ids;
		END IF;

		RAISE NOTICE 'Archive calculated: %, to remove: %, ', clock_timestamp(), old_ids;

		create temp table if not exists o2m_result_ids
		(
		    id numeric, attribute_id numeric, to_remove boolean, remove_delays boolean
		);

		insert into o2m_result_ids
        select unnest, attr.back_related_attribute_id, true, false from unnest(old_ids)
        union all
        select unnest, attr.back_related_attribute_id, false, false from unnest(new_ids);

		RAISE NOTICE 'Result returned: %',  clock_timestamp();


	END;
$BODY$;

alter procedure qp_update_m2o(numeric, numeric, text, boolean) owner to postgres;
