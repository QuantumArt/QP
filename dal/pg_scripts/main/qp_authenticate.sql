-- FUNCTION: public.qp_authenticate(text, text, boolean, boolean)

-- DROP FUNCTION public.qp_authenticate(text, text, boolean, boolean);

CREATE OR REPLACE FUNCTION public.qp_authenticate(
	login text,
	password text,
	use_nt_login boolean DEFAULT false,
	check_admin_access boolean DEFAULT false)
    RETURNS users
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$
	DECLARE
		user_row users%ROWTYPE;
	BEGIN
		IF use_nt_login THEN
			select * from users where nt_login = $1 into user_row;
			IF user_row is null THEN
				RAISE EXCEPTION 'Your Windows account is not mapped to any QP user' using errcode = 'AUTH5';
			ELSEIF user_row.disabled = 1 THEN
				RAISE EXCEPTION 'Account is disabled. Contact <@MailForErrors@>' using errcode = 'AUTH2';
			ELSEIF user_row.auto_login = 0 THEN
				RAISE EXCEPTION 'Auto login option is switched off for your account. Contact <@MailForErrors@>' using errcode = 'AUTH6';
			END IF;
		ELSE
			select * from users u where u.login = $1 into user_row;
			IF user_row is null THEN
				RAISE EXCEPTION 'Login doesn''t exist' using errcode = 'AUTH1';
			ELSEIF user_row.disabled = 1 THEN
				RAISE EXCEPTION 'Account is disabled. Contact <@MailForErrors@>' using errcode = 'AUTH2';
			ELSEIF qp_get_hash(password, user_row.salt) <> user_row.hash THEN
				RAISE EXCEPTION 'Password is incorrect' using errcode = 'AUTH3';
			ELSEIF check_admin_access THEN
				IF NOT EXISTS(select * from USER_GROUP_BIND where GROUP_ID = 1 and USER_ID = user_row.user_id) THEN
					RAISE EXCEPTION 'Account is not a member of Administrators group' using errcode = 'AUTH4';
				END IF;
			END IF;
		END IF;

        update USERS set LAST_LOGIN = now() where USER_ID = user_row.user_id;
		RETURN user_row;
	END;

$BODY$;

ALTER FUNCTION public.qp_authenticate(text, text, boolean, boolean)
    OWNER TO postgres;

