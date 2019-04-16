using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class WorkflowMapper : GenericMapper<Workflow, WorkflowDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Workflow, WorkflowDAL>(MemberList.Destination)
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Site, opt => opt.Ignore())
                .ForMember(data => data.WorkflowRules, opt => opt.Ignore())
                .ForMember(data => data.WorkflowAccess, opt => opt.Ignore())
                ;
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<WorkflowDAL, Workflow>(MemberList.Source);
        }
    }
}
