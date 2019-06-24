create or replace function qp_is_date(s character varying) returns boolean
    language plpgsql
as
$$
begin
  perform s::date;
  return true;
exception when others then
  return false;
end;
$$;

alter function qp_is_date(varchar) owner to postgres;

