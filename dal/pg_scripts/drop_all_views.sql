DO $$
	declare
		views text[];
		item text;
	    sql text;
	begin

	    views := array_agg(table_name) from information_schema.views c where table_schema = 'public';

		if views is not null then
			foreach item in array views loop
			    RAISE NOTICE 'View %', item;
	            sql := format('drop view if exists %s cascade', item);
			    execute sql;
			end loop;
		end if;
	end;
$$ LANGUAGE plpgsql;