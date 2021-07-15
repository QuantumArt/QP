insert into backend_action(type_id, entity_type_id, name, short_name, code, controller_action_url, is_interface)
select (select id from action_type where code = 'read'), (select id from entity_type where code = 'article'),
       'Article Live Properties', 'Live Properties', 'view_live_article', '~/Article/LiveProperties/', true
where not exists(select * from backend_action where name = 'Article Live Properties');

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
select (select id from context_menu where code = 'article'), (select id from backend_action where code = 'view_live_article'),
       'Live Properties', 52, 'properties.gif'
where not exists(select * from context_menu_item where context_menu_id = any(select id from context_menu where code = 'article') and name = 'Live Properties');

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'view_live_article'), (select id from backend_action where code = 'refresh_article'), 'Refresh', 'refresh.gif', 10
where not exists(select * from action_toolbar_button where parent_action_id = any(select id from backend_action where code = 'view_live_article'));

insert into backend_action(type_id, entity_type_id, name, short_name, code, controller_action_url, is_interface)
select (select id from action_type where code = 'read'), (select id from entity_type where code = 'article'),
       'Compare Article Live version with Current', 'Compare Live version with Current', 'compare_article_live_with_current', '~/ArticleVersion/CompareLiveWithCurrent/', true
where not exists(select * from backend_action where name = 'Compare Article Live version with Current');

insert into context_menu_item(context_menu_id, action_id, name, "order", icon)
select (select id from context_menu where code = 'article'), (select id from backend_action where code = 'compare_article_live_with_current'),
       'Compare Live version with Current', 53, 'compare.gif'
where not exists(select * from context_menu_item where context_menu_id = any(select id from context_menu where code = 'article') and name = 'Compare Live version with Current');

insert into action_toolbar_button(parent_action_id, action_id, name, icon, "order")
select (select id from backend_action where code = 'compare_article_live_with_current'), (select id from backend_action where code = 'refresh_article'), 'Refresh', 'refresh.gif', 10
where not exists(select * from action_toolbar_button where parent_action_id = any(select id from backend_action where code = 'compare_article_live_with_current'));

