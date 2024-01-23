DO $$ BEGIN
	CREATE TYPE link_multiple AS
	(
		id numeric(18,0),
		link_id numeric(18,0),
		linked_id numeric(18,0)
	);


EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
