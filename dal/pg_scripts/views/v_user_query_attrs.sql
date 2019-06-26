create or replace view v_user_query_attrs AS
	select vca.ATTRIBUTE_ID as USER_QUERY_ATTR_ID, ca.ATTRIBUTE_ID BASE_ATTR_ID
	from user_query_attrs uqa
	join CONTENT_ATTRIBUTE vca on uqa.virtual_content_id = vca.CONTENT_ID
	join CONTENT_ATTRIBUTE ca on uqa.user_query_attr_id = ca.ATTRIBUTE_ID
	where vca.ATTRIBUTE_NAME = ca.ATTRIBUTE_NAME

alter table v_user_query_attrs
    owner to postgres;