create or replace function qp_context_menu_id(_code text) returns bigint
immutable
    strict
    language plpgsql
as
$$
begin
	return (select id from context_menu where code = _code);
end;
$$;

create or replace function qp_action_type_id(_code text) returns bigint
immutable
    strict
    language plpgsql
as
$$
begin
	return (select id from action_type where code = _code);
end;
$$;

create or replace function qp_entity_type_id(_code text) returns bigint
immutable
    strict
    language plpgsql
as
$$
begin
	return (select id from entity_type where code = _code);
end;
$$;

create or replace function qp_action_id(_code text) returns bigint
immutable
    strict
    language plpgsql
as
$$
begin
	return (select id from backend_action where code = _code);
end;
$$;
