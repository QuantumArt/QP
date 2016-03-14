using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContentFolderMapper : GenericMapper<ContentFolder, ContentFolderDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ContentFolderDAL, ContentFolder>()			  
			  .ForMember(biz => biz.StoredPath, opt => opt.MapFrom(data => data.Path));
		}

		public override void CreateDalMapper()
        {
            Mapper.CreateMap<ContentFolder, ContentFolderDAL>()
              .ForMember(data => data.Content, opt => opt.Ignore())
			  .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }

	}
}
