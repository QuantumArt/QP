
CREATE OR REPLACE PROCEDURE fix_sequences()
    LANGUAGE plpgsql
AS
$$
DECLARE rec RECORD;
    DECLARE v_max int8;
    DECLARE rsql text;

BEGIN
    FOR rec IN
        SELECT  d.objid::regclass,
                d.refobjid::regclass,
                a.attname
        FROM 	pg_depend AS d
                    JOIN pg_class AS t
                         ON d.objid = t.oid
                    JOIN pg_attribute AS a
                         ON d.refobjid = a.attrelid
                             AND d.refobjsubid = a.attnum
        WHERE 	d.classid = 'pg_class'::regclass
          AND d.refclassid = 'pg_class'::regclass
          AND t.oid >= 16384
          AND t.relkind = 'S'
          AND d.deptype IN ('a', 'i')
        LOOP
            rsql := 'SELECT setval(' || quote_literal(rec.objid::regclass) || ', (select cast(max('
                        || quote_ident(rec.attname::text) || ') as integer) FROM ' || rec.refobjid::regclass || '))';
            EXECUTE rsql INTO v_max;
            RAISE NOTICE 'sql: %', rsql;
            RAISE NOTICE 'setting sequence for % to %', rec.refobjid::text, v_max;
        END LOOP;
END;
$$;
