DO LANGUAGE plpgsql $$
DECLARE
	cids int[];
	cid int;
BEGIN
	cids := array_agg(content_id) from content where virtual_type = 0;
	foreach cid in array cids
	loop
		call qp_content_new_views_create(cid);
	end loop;
END;
$$;