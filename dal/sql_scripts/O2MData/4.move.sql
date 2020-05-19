update content_data set O2M_DATA = cd.data from content_data cd
inner join content_attribute ca on ca.attribute_id = cd.attribute_id
where ca.attribute_type_id = 11 and ca.link_id is null
and cd.data is not null and cd.data <> '0' and cd.O2M_DATA is null
GO

update version_content_data set O2M_DATA = vcd.data from version_content_data vcd
inner join content_attribute ca on ca.attribute_id = vcd.attribute_id
where ca.attribute_type_id = 11 and ca.link_id is null
and vcd.data is not null and vcd.data <> '0' and vcd.O2M_DATA is null
GO
