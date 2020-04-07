DO $$ BEGIN
    create trigger tiu_update_hash
        before insert or update
        on users
        for each row
    execute procedure update_hash();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


