DO $$
	declare
		indices text[];
		item text;
		new_item text;
		sql text;
	begin

		indices := array_agg(indexname) FROM pg_indexes where indexname like '%public.%';
		if indices is not null then
			foreach item in array indices loop
				new_item := replace(item, 'public.', 'dbo.');
				sql := format('alter index "%s" rename to "%s"', item, new_item);
				raise notice '%', sql;
				execute sql;
			end loop;
		end if;
	end;
$$ LANGUAGE plpgsql;

