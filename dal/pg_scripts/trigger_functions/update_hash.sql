CREATE OR REPLACE FUNCTION update_hash() RETURNS TRIGGER AS $tiu_update_hash$
	DECLARE 
		salt bigint;
	 	old_hash bytea;
	BEGIN
		IF (TG_OP = 'INSERT') THEN
			IF NEW.PASSWORD IS NULL OR NEW.PASSWORD = '' THEN
                RAISE EXCEPTION 'Cannot create user with empty password';
			END IF;
			old_hash = null;
		ELSEIF (TG_OP = 'UPDATE') THEN
			IF NEW.PASSWORD IS NULL OR NEW.PASSWORD = '' OR OLD.PASSWORD = NEW.PASSWORD THEN
				RAISE NOTICE 'No changes in password detected';
				RETURN NEW;
			END IF;
			old_hash = OLD.hash;
		END IF;

		salt = coalesce(salt, floor(random() * 1000000000 + 1)::bigint);
		NEW.salt = salt;
		NEW.hash = qp_get_hash(NEW.password, salt);
		NEW.password = '';
		IF NEW.hash <> old_hash THEN
			NEW.password_modified = NOW();
		END IF;

		RETURN NEW;	
	END
$tiu_update_hash$ LANGUAGE plpgsql;

alter function update_hash() owner to postgres;

