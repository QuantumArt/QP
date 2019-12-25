DO LANGUAGE plpgsql $$
DECLARE
	lids int[];
	lid int;
BEGIN
	lids := array_agg(link_id) from content_to_content where link_id in (select link_id from content_attribute);
	foreach lid in array lids
	loop
		call qp_link_view_create(lid);
	end loop;
END;
$$;