insert into backend_action (type_id, entity_type_id, name, code, controller_action_url, is_interface)
VALUES(public.qp_action_type_id('read'), public.qp_entity_type_id('db'), 'Scheduled Tasks', 'scheduled_tasks', '~/Home/ScheduledTasks/', true) ON CONFLICT DO NOTHING;

INSERT INTO context_menu_item(context_menu_id, action_id, name, "order")
VALUES(public.qp_context_menu_id('db'), public.qp_action_id('scheduled_tasks'), 'Scheduled Tasks', 90) ON CONFLICT DO NOTHING;

insert into backend_action(name, code, type_id, entity_type_id)
values('Refresh Scheduled Tasks', 'refresh_scheduled_tasks', public.qp_action_type_id('refresh'), public.qp_entity_type_id('db')) ON CONFLICT DO NOTHING;

insert into action_toolbar_button (parent_action_id, action_id, icon, "order", name)
values (public.qp_action_id('scheduled_tasks'), public.qp_action_id('refresh_scheduled_tasks'), 'refresh.gif', 100, 'Refresh') ON CONFLICT DO NOTHING;