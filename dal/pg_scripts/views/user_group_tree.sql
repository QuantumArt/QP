create or replace view user_group_tree(group_id, group_name, description, created, modified, last_modified_by,
                            last_modified_by_login, shared_content_items, nt_group, ad_sid, built_in, readonly,
                            use_parallel_workflow, can_unlock_items, parent_group_id, can_manage_scheduled_tasks) as
SELECT ug.group_id,
       ug.group_name,
       ug.description,
       ug.created,
       ug.modified,
       ug.last_modified_by,
       u.login AS last_modified_by_login,
       ug.shared_content_items,
       ug.nt_group,
       ug.ad_sid,
       ug.built_in,
       ug.readonly,
       ug.use_parallel_workflow,
       ug.can_unlock_items,
       gtg.parent_group_id,
       ug.can_manage_scheduled_tasks
FROM ((user_group ug
    LEFT JOIN group_to_group gtg ON ((ug.group_id = gtg.child_group_id)))
         JOIN users u ON ((u.user_id = ug.last_modified_by)));
