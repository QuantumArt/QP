using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	class VisualEditorStyleMapper : GenericMapper<VisualEditorStyle, VeStyleDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<VisualEditorStyle, VeStyleDAL>()
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())				
				;
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<VeStyleDAL, VisualEditorStyle>()
			.AfterMap(SetBizProperties);
		}

		private static void SetBizProperties(VeStyleDAL dataObject, VisualEditorStyle bizObject)
		{			
			bizObject.Init();
		}
	}
}
