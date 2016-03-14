using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	class WorkFlowRuleMapper : GenericMapper<WorkflowRule, WorkflowRulesDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<WorkflowRule, WorkflowRulesDAL>()
				.ForMember(data => data.StatusType, opt => opt.Ignore())
				.ForMember(data => data.Workflow, opt => opt.Ignore());
		}

		public override void CreateBizMapper()
		{			
			Mapper.CreateMap<WorkflowRulesDAL, WorkflowRule>()
			.ForMember(biz => biz.Description, opt => opt.MapFrom(src => src.Description));
		}
	}
}
