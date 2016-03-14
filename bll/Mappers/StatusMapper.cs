using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	class StatusMapper : GenericMapper<StatusType, StatusTypeDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<StatusType, StatusTypeDAL>()
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.WorkflowRules, opt => opt.Ignore())
				;
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<StatusTypeDAL, StatusType>();
		}		
	}
}
