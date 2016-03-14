using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ToolbarButtonMapper : GenericMapper<ToolbarButton, ToolbarButtonDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ToolbarButton, ToolbarButtonDAL>()
				.ForMember(data => data.Action, opt => opt.Ignore())
				.ForMember(data => data.ParentAction, opt => opt.Ignore());
		}
	}
}
