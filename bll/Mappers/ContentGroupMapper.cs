using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContentGroupMapper : GenericMapper<ContentGroup, ContentGroupDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ContentGroup, ContentGroupDAL>()
				.ForMember(data => data.Site, opt => opt.Ignore())
				.ForMember(data => data.Contents, opt => opt.Ignore())
			;			
		}
	}
}
