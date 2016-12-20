using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
	internal class SiteFolderPermissionMapper : GenericMapper<EntityPermission, SiteFolderPermissionDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<SiteFolderPermissionDAL, EntityPermission>()
				.ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.SiteFolderMapper.GetBizObject(data.Folder)))
				.ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.FolderId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<EntityPermission, SiteFolderPermissionDAL>()
				.ForMember(data => data.Folder, opt => opt.Ignore())
				.ForMember(data => data.FolderId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))

				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Group, opt => opt.Ignore())
				.ForMember(data => data.PermissionLevel, opt => opt.Ignore())
				.ForMember(data => data.User, opt => opt.Ignore());
		}
	}
}
