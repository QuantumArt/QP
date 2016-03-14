using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContentLinkMapper : GenericMapper<ContentLink, ContentToContentDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ContentToContentDAL, ContentLink>()
				.ForMember(biz => biz.Content, opt => opt.Ignore())
			;
		}
		
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ContentLink, ContentToContentDAL>()
				.ForMember(data => data.Content, opt => opt.Ignore())
			;			
		}
	}
}
