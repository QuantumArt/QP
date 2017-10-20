using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	class WorkflowMapper : GenericMapper<Workflow, WorkflowDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<Workflow, WorkflowDAL>()
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Site, opt => opt.Ignore())
				.ForMember(data => data.WorkflowRules, opt => opt.Ignore())
				.ForMember(data => data.WorkflowAccess, opt => opt.Ignore())
				;
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<WorkflowDAL, Workflow>();
		}
	}
}
