create or replace function qp_get_user_weight(user_id int, workflow_id int)
returns int
    stable
    language plpgsql
as
$$
DECLARE
    result int;
BEGIN

    result := max(st.weight) FROM workflow_rules wr
		INNER JOIN status_type st ON wr.successor_status_id = st.status_type_id
		WHERE wr.workflow_id = $2 and wr.user_id = $1;

    if result is null then
		WITH RECURSIVE groups(group_id) AS
		(
		    select ug.group_id from user_group_bind ug where ug.user_id = $1
		    UNION ALL
		    select gg.parent_group_id from group_to_group gg inner join groups g on g.group_id = gg.child_group_id
		 )
		select max(st.weight) into result FROM workflow_rules wr
		INNER JOIN status_type st ON wr.successor_status_id = st.status_type_id
		WHERE wr.workflow_id = $2 and wr.group_id in (select group_id from groups);

	end if;

	return result;

END;
$$;


