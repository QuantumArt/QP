using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class WorkFlowRuleMapper : GenericMapper<WorkflowRule, WorkflowRulesDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<WorkflowRule, WorkflowRulesDAL>(MemberList.Destination)
                .ForMember(data => data.StatusType, opt => opt.Ignore())
                .ForMember(data => data.Workflow, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<WorkflowRulesDAL, WorkflowRule>(MemberList.Source)
                .ForMember(biz => biz.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}
