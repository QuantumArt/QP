declare @cnt numeric

;with numbers(num)  as
(
	select top (1000) ROW_NUMBER() OVER (ORDER BY content_data_id) as num from content_data cd where cd.O2M_DATA is not null
)
select @cnt = max(num) from numbers


if @cnt < 1000
begin
    update content_data set O2M_DATA = try_convert(numeric, cd.data) from content_data cd
    inner join content_attribute ca on ca.attribute_id = cd.attribute_id
    where ca.attribute_type_id = 11 and ca.link_id is null
    and cd.data is not null and isnumeric(cd.data) = 1 and cd.data <> '0' and cd.O2M_DATA is null

    update version_content_data set O2M_DATA = try_convert(numeric, vcd.data) from version_content_data vcd
    inner join content_attribute ca on ca.attribute_id = vcd.attribute_id
    where ca.attribute_type_id = 11 and ca.link_id is null
    and vcd.data is not null and isnumeric(vcd.data) = 1 and vcd.data <> '0' and vcd.O2M_DATA is null
end
GO

