create or replace view full_workflow_rules(workflow_rule_id, user_id, group_id, rule_order, predecessor_permission_id,
                                successor_permission_id, successor_status_id, comment, workflow_id) as
    SELECT workflow_rules.workflow_rule_id,
           workflow_rules.user_id,
           workflow_rules.group_id,
           workflow_rules.rule_order,
           workflow_rules.predecessor_permission_id,
           workflow_rules.successor_permission_id,
           workflow_rules.successor_status_id,
           workflow_rules.comment,
           workflow_rules.workflow_id
    FROM workflow_rules
    UNION ALL
    SELECT 0                                  AS workflow_rule_id,
           1                                  AS user_id,
           NULL::numeric                      AS group_id,
           0                                  AS rule_order,
           NULL::numeric                      AS predecessor_permission_id,
           NULL::numeric                      AS successor_permission_id,
           st.status_type_id                  AS successor_status_id,
           '(no comments)'::character varying AS comment,
           w.workflow_id
    FROM (workflow w
             JOIN status_type st ON ((w.site_id = st.site_id)))
    WHERE ((st.status_type_name)::text = 'None'::text);


