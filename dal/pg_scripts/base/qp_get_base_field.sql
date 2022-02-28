create or replace function qp_get_base_field(field_id integer, article_id integer)
returns integer
    stable
    language plpgsql
as
$$
DECLARE
    virtual_type int;
    result int;
    attr content_attribute;
BEGIN

    attr := row(ca.*) from content_attribute ca where ca.attribute_id = $1;
    virtual_type := c.virtual_type from content c where c.content_id = attr.content_id;

    if virtual_type = 1 then
		result := ca.persistent_attr_id from content_attribute ca where attribute_id = attr.attribute_id;
    elseif virtual_type = 2 then
		result := ua.union_attr_id from union_attrs ua inner join content_attribute ca on ua.union_attr_id = ca.attribute_id where virtual_attr_id = attr.attribute_id and ca.content_id in (select content_id from content_item where content_item_id = $2);
    elseif virtual_type = 3 then
	    result := ca.attribute_id from content_attribute ca where attribute_name = attr.attribute_name and content_id in (select real_content_id from user_query_contents where virtual_content_id = attr.content_id);
    else
        result := attr.attribute_id;
    end if;

    return coalesce(result, attr.attribute_id);


END;
$$;

