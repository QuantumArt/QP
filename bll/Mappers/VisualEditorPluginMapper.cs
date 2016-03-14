using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	class VisualEditorPluginMapper : GenericMapper<VisualEditorPlugin, VePluginDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<VisualEditorPlugin, VePluginDAL>()
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.VeCommands, opt => opt.Ignore())
				;
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<VePluginDAL, VisualEditorPlugin>();
		}
	}
}
