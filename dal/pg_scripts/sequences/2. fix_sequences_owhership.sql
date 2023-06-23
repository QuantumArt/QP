CREATE OR REPLACE PROCEDURE fix_seq_ownership()
    LANGUAGE plpgsql
AS $$
DECLARE rec RECORD;
    DECLARE rsql text;

BEGIN
    FOR rec IN
        select
            u.usename as owner_name,
            substring(pg_get_expr(d.adbin, d.adrelid) from E'^nextval\\(''([^'']*)''(?:::text|::regclass)?\\)') as seq_name
        FROM pg_class c
                 JOIN pg_user u on (u.usesysid=c.relowner)
                 JOIN pg_attribute a on (c.oid=a.attrelid)
                 JOIN pg_attrdef d on (a.attrelid=d.adrelid and a.attnum=d.adnum)
                 JOIN pg_namespace n on (c.relnamespace=n.oid)
        where has_schema_privilege(n.oid,'USAGE')
          and n.nspname not like 'pg!_%' escape '!'
            and has_table_privilege(c.oid,'SELECT')
            and (not a.attisdropped)
            and pg_get_expr(d.adbin, d.adrelid) ~ '^nextval'
        LOOP
            rsql := 'ALTER SEQUENCE '|| rec.seq_name
                        ||' OWNER TO '|| rec.owner_name ||';';

            RAISE NOTICE 'sql: %', rsql;
            EXECUTE rsql;

        END LOOP;
END;
$$;
