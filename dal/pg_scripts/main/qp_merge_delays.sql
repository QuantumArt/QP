CREATE OR REPLACE PROCEDURE qp_merge_delays(ids integer[], last_modified_by integer DEFAULT 1)
    language plpgsql
as
$$
DECLARE
		ids2 int[];
		ids3 int[];
    BEGIN
		ids2 := array_agg(cd.child_id) from child_delays cd where cd.id = ANY(ids);
		IF ids2 is not null THEN
			ids3 := array_agg(cd.child_id) from child_delays cd where cd.id = ANY(ids)
			and not exists (select * from CHILD_DELAYS cd2 where cd2.child_id = cd.child_id and cd2.id <> cd.id);

			IF ids3 is not null THEN
				call qp_merge_articles(ids3, last_modified_by);
			END IF;

			DELETE FROM child_delays WHERE id = ANY(ids2);

		END IF;
	END;
$$;



