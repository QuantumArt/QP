update content_data set BLOB_DATA = replace(cast(BLOB_DATA as nvarchar(max)), '<XmlMappingBehavior', '<Content') from content_data where ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where content_id = 376 and CONTENT_ATTRIBUTE.ATTRIBUTE_NAME = 'XmlDefinition')
and BLOB_DATA like '%xmlm%'

update content_data set BLOB_DATA = replace(cast(BLOB_DATA as nvarchar(max)), '</XmlMappingBehavior', '</Content') from content_data where ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where content_id = 376 and CONTENT_ATTRIBUTE.ATTRIBUTE_NAME = 'XmlDefinition')
and BLOB_DATA like '%xmlm%'

update content_data set BLOB_DATA = replace(cast(BLOB_DATA as nvarchar(max)), ' XmlMappingBehavior.', ' ') from content_data where ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE where content_id = 376 and CONTENT_ATTRIBUTE.ATTRIBUTE_NAME = 'XmlDefinition')
and BLOB_DATA like '%xmlm%'




