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
	internal class ArticlePermissionMapper : GenericMapper<EntityPermission, ArticlePermissionDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ArticlePermissionDAL, EntityPermission>()
				.ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.ArticleMapper.GetBizObject(data.Article)))
				.ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.ArticleId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<EntityPermission, ArticlePermissionDAL>()
				.ForMember(data => data.Article, opt => opt.Ignore())
				.ForMember(data => data.ArticleId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))

				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Group, opt => opt.Ignore())
				.ForMember(data => data.PermissionLevel, opt => opt.Ignore())
				.ForMember(data => data.User, opt => opt.Ignore());
		}
	}
}
