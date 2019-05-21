



update content set modified = now(), query = 'select content_id as content_item_id, content_name as Name, created, modified, last_modified_by, cast(125 as numeric) as status_type_id, cast(1 as numeric) as visible, cast(0 as numeric) as archive from content c where exists (select * from content_attribute where related_attribute_id in (select attribute_id from content_attribute where content_id = 339) and AGGREGATED and content_id = c.content_id)'
where content_id = 348;

update content set modified = now(), query = 'select content_id as content_item_id, content_name as Name, created, modified, last_modified_by, cast(125 as numeric) as status_type_id, cast(1 as numeric) as visible, cast(0 as numeric) as archive from content c where exists (select * from content_attribute where related_attribute_id in (select attribute_id from content_attribute where content_id = 361) and AGGREGATED and content_id = c.content_id)'
where content_id = 369;

update content set modified = now(), query = 'select content_id as content_item_id, content_name as Name, created, modified, last_modified_by, cast(125 as numeric) as status_type_id, cast(1 as numeric) as visible, cast(0 as numeric) as archive from content c where exists (select * from content_attribute where related_attribute_id in (select attribute_id from content_attribute where content_id = 383) and AGGREGATED and content_id = c.content_id)'
where content_id = 387;

call qp_content_user_query_view_recreate(348);
call qp_content_user_query_view_recreate(369);
call qp_content_user_query_view_recreate(387);
call qp_content_user_query_view_recreate(380);