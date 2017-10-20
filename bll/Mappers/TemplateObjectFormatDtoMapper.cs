using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Mappers
{
	class TemplateObjectFormatDtoRowMapper : GenericMapper<TemplateObjectFormatDto, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, TemplateObjectFormatDto>()
				.ForMember(biz => biz.TemplateName, opt => opt.MapFrom(row => row.Field<string>("TemplateName")))
				.ForMember(biz => biz.ObjectName, opt => opt.MapFrom(row => row.Field<string>("ObjectName")))
				.ForMember(biz => biz.FormatName, opt => opt.MapFrom(row => row.Field<string>("FormatName")))				
				;
		}		
	}
}
