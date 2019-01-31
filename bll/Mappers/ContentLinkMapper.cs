using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentLinkMapper : GenericMapper<ContentLink, ContentToContentDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentToContentDAL, ContentLink>(MemberList.Source)
                .ForMember(biz => biz.Content, opt => opt.Ignore())
                ;
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentLink, ContentToContentDAL>(MemberList.Destination)
                .ForMember(data => data.Content, opt => opt.Ignore())
                ;
        }
    }
}
