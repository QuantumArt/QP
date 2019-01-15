using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class TemplateObjectFormatDtoRowMapper : GenericMapper<TemplateObjectFormatDto, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, TemplateObjectFormatDto>(MemberList.Source)
                .ForMember(biz => biz.TemplateName, opt => opt.MapFrom(row => row.Field<string>("TemplateName")))
                .ForMember(biz => biz.ObjectName, opt => opt.MapFrom(row => row.Field<string>("ObjectName")))
                .ForMember(biz => biz.FormatName, opt => opt.MapFrom(row => row.Field<string>("FormatName")))
                ;
        }
    }
}
