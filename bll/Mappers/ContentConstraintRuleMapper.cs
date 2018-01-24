using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentConstraintRuleMapper : GenericMapper<ContentConstraintRule, ContentConstraintRuleDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<ContentConstraintRule, ContentConstraintRuleDAL>()
                .ForMember(data => data.Field, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ContentConstraintRuleDAL, ContentConstraintRule>()
                .ForMember(biz => biz.Field, opt => opt.Ignore());
        }
    }
}
