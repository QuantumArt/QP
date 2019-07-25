update content_attribute set relation_condition = replace(relation_condition, 'c.[Group]', 'c."group"') where relation_condition is not null;
update content_attribute set relation_condition = replace(relation_condition, 'c.[Parent]', 'c."parent"') where relation_condition is not null;
update content_attribute set relation_condition = replace(relation_condition, 'c.[Type]', 'c."type"') where relation_condition is not null;
update content_attribute set relation_condition = replace(relation_condition, 'isnull(', 'coalesce(') where relation_condition is not null;

