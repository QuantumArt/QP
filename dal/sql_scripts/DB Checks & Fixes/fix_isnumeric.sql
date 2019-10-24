
select count(data) from content_data where data is not null and ISNUMERIC(data) = 0 and attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where ATTRIBUTE_TYPE_ID = 2)
update content_data set data = null where data is not null and ISNUMERIC(data) = 0 and attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where ATTRIBUTE_TYPE_ID = 2)

select count(data) from content_data where data is not null and data like '%,%' and attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where ATTRIBUTE_TYPE_ID = 2)
update content_data set data = replace(data, ',', '.') where data is not null and data like '%,%' and attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where ATTRIBUTE_TYPE_ID = 2)
