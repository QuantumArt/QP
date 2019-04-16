using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentConstraintRuleMapper : GenericMapper<ContentConstraintRule, ContentConstraintRuleDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentConstraintRule, ContentConstraintRuleDAL>(MemberList.Destination)
                .ForMember(data => data.Field, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentConstraintRuleDAL, ContentConstraintRule>(MemberList.Source)
                .ForMember(biz => biz.Field, opt => opt.Ignore());
        }
    }
}
