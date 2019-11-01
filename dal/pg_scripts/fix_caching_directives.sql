
update content_data cd set BLOB_DATA = replace(cd.BLOB_DATA, '<XmlMappingBehavior', '<Content') where cd.ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE ca where ca.content_id = 376 and ca.ATTRIBUTE_NAME = 'XmlDefinition')
and cd.BLOB_DATA like '%XmlMappingBehavior%';

update content_data cd set BLOB_DATA = replace(cd.BLOB_DATA, '</XmlMappingBehavior', '</Content') where cd.ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE ca where ca.content_id = 376 and ca.ATTRIBUTE_NAME = 'XmlDefinition')
and cd.BLOB_DATA like '%XmlMappingBehavior%';

update content_data cd set BLOB_DATA = replace(cd.BLOB_DATA, ' XmlMappingBehavior.', ' ') where cd.ATTRIBUTE_ID in
(select ATTRIBUTE_ID from CONTENT_ATTRIBUTE ca where ca.content_id = 376 and ca.ATTRIBUTE_NAME = 'XmlDefinition')
and cd.BLOB_DATA like '%XmlMappingBehavior%';