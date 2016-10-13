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
	internal class ContentPermissionMapper : GenericMapper<EntityPermission, ContentPermissionDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ContentPermissionDAL, EntityPermission>()
				.ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.ContentMapper.GetBizObject(data.Content)))
				.ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.ContentId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<EntityPermission, ContentPermissionDAL>()
				.ForMember(data => data.Content, opt => opt.Ignore())
				.ForMember(data => data.ContentId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))

				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Group, opt => opt.Ignore())
				.ForMember(data => data.PermissionLevel, opt => opt.Ignore())
				.ForMember(data => data.User, opt => opt.Ignore());
		}

	}
}
