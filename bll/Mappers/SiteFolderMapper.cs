using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class SiteFolderMapper : GenericMapper<SiteFolder, SiteFolderDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<SiteFolderDAL, SiteFolder>()
			  .ForMember(biz => biz.StoredPath, opt => opt.MapFrom(data => data.Path));
		}

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<SiteFolder,SiteFolderDAL>()
              .ForMember(data => data.Site, opt => opt.Ignore())
			  .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }
	}
}
