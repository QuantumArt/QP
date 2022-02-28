create or replace view template_object(object_id, parent_object_id, page_template_id, page_id, object_name, object_format_id,
                            description, object_type_id, use_default_values, created, modified, last_modified_by,
                            allow_stage_edit, global, net_object_name, locked, locked_by, enable_viewstate,
                            control_custom_class, disable_databind, permanent_lock, icon) as
SELECT o.object_id,
       o.parent_object_id,
       o.page_template_id,
       o.page_id,
       o.object_name,
       o.object_format_id,
       o.description,
       o.object_type_id,
       o.use_default_values,
       o.created,
       o.modified,
       o.last_modified_by,
       o.allow_stage_edit,
       o.global,
       o.net_object_name,
       o.locked,
       o.locked_by,
       o.enable_viewstate,
       o.control_custom_class,
       o.disable_databind,
       o.permanent_lock,
       ot.icon
FROM (object o
         JOIN object_type ot ON ((o.object_type_id = ot.object_type_id)))
WHERE (o.page_id IS NULL);


