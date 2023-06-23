CREATE OR REPLACE PROCEDURE fix_seq_ownership()
    LANGUAGE plpgsql
AS $$
DECLARE rec RECORD;
    DECLARE rsql text;
    DECLARE test_name text;
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
        
    ALTER SEQUENCE content_item_seq OWNED BY content_item.content_item_id;

    test_name := c.relname 
        FROM pg_class c
            JOIN pg_attribute a on (c.oid=a.attrelid)
            JOIN pg_attrdef d on (a.attrelid=d.adrelid and a.attnum=d.adnum)
            JOIN pg_namespace n on (c.relnamespace=n.oid)
        where has_schema_privilege(n.oid,'USAGE')
            and has_table_privilege(c.oid,'SELECT')
            and c.relname <> 'content_item'
            and substring(pg_get_expr(d.adbin, d.adrelid) 
                from E'^nextval\\(''([^'']*)''(?:::text|::regclass)?\\)') = 'content_item_seq';
        
        IF test_name is not null THEN
            rsql := 'DROP TABLE ' || test_name ||';';
            RAISE NOTICE 'sql: %', rsql;
            EXECUTE rsql;
        END IF;        
END;
$$;
