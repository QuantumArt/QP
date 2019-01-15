using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentWorkflowBindMapper : GenericMapper<ContentWorkflowBind, ContentWorkflowBindDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentWorkflowBindDAL, ContentWorkflowBind>(MemberList.Source)
                .ForMember(biz => biz.Content, opt => opt.Ignore())
                ;
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentWorkflowBind, ContentWorkflowBindDAL>(MemberList.Destination)
                .ForMember(data => data.Content, opt => opt.Ignore())
                ;
        }
    }
}
