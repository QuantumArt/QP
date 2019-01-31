using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ViewTypeMapper : GenericMapper<ViewType, ViewTypeDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ViewTypeDAL, ViewType>(MemberList.Source)
                .ForMember(biz => biz.Name, opt => opt.MapFrom(data => Translator.Translate(data.Name)));
        }
    }
}
