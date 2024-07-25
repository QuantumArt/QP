-- PROCEDURE: qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[])

-- DROP PROCEDURE qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]);

CREATE OR REPLACE PROCEDURE qp_update_items_with_content_data_pivot(
	content_id integer,
	ids integer[],
	is_async boolean,
	attr_ids integer[])
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  table_name text;
	  attributes content_attribute[];
	  attr_names text[];
	  cross_tab text;
	  attrs_result text[];
	  res text;
	  attrs_select text[];
	  sel text;
	  attrs_update text[];
	  upd text;
      use_native bool;
	BEGIN
        use_native := use_native_ef_types from content
            where content.content_id = qp_update_items_with_content_data_pivot.content_id;

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;

		IF attr_ids IS NULL THEN
			attributes := array_agg(ca.* order by ca.attribute_name) from CONTENT_ATTRIBUTE ca
				where ca.content_id = $1;
		ELSE
			attributes := array_agg(ca.* order by ca.attribute_name) from CONTENT_ATTRIBUTE ca
				where ca.content_id = $1 AND attribute_id = ANY(attr_ids);
		END IF;
		attr_ids := array_agg(attribute_id) from unnest(attributes) a;

		IF array_length(attributes, 1) > 0 THEN

			attr_names := array_agg(lower(a.attribute_name)) from unnest(attributes) a;

			attrs_update := array_agg(FORMAT('"%s" = pt."%s"', unnest, unnest)) from unnest(attr_names);
			upd := array_to_string(attrs_update, ', ');

			attrs_result := array_agg(FORMAT('"%s" TEXT', unnest)) from unnest(attr_names);
			attrs_result := array_prepend('content_item_id NUMERIC', attrs_result);
			res := array_to_string(attrs_result, ', ');

			attrs_select := array_agg(FORMAT('"%s"::%s', b.name, b.type)) from (
			select lower(a.attribute_name) as name,
            CASE WHEN a.attribute_type_id in (2,11,13) THEN
                    'numeric'
                WHEN a.attribute_type_id in (3) and not use_native THEN
                    'numeric'
                WHEN a.attribute_type_id in (3) and use_native THEN
                    'bool'
				WHEN a.attribute_type_id in (4,5,6) THEN
					'timestamp with time zone'
				ELSE
					'text'
				END AS type from unnest(attributes) a
			) b;
			attrs_select := array_prepend('content_item_id', attrs_select);
			sel := array_to_string(attrs_select, ', ');

			cross_tab := 'update %s base set %s from (
			SELECT %s FROM crosstab(''
			select content_item_id, lower(ca.attribute_name),
			case when ca.attribute_type_id in (9, 10) then coalesce(cd.data, cd.blob_data)
			else qp_correct_data(cd.data::text, ca.attribute_type_id, ca.attribute_size, ca.default_value)::text
			end as value from content_data cd
			inner join content_attribute ca on cd.attribute_id = ca.attribute_id
			where content_item_id in (%s) and cd.attribute_id in (%s)
			order by content_item_id, ca.attribute_name
			'') AS final_result(%s)) pt where pt.content_item_id = base.content_item_id;';

			cross_tab := FORMAT(cross_tab, table_name, upd, sel, array_to_string(ids, ', '), array_to_string(attr_ids, ', '), res);
			RAISE NOTICE '%', cross_tab;
			execute cross_tab;
		END IF;
	END;
$BODY$;
