create or replace view workflow_max_statuses AS

select workflow_id, STATUS_TYPE_ID as max_status_type_id from
(
	select wr.workflow_id, st.status_type_id, ROW_NUMBER() OVER (PARTITION BY wr.WORKFLOW_ID ORDER BY wr.RULE_ORDER DESC ) AS order
	from workflow_rules wr
	inner join status_type st on wr.successor_status_id = st.status_type_id
) as v
where v.order = 1;


