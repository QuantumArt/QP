using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
	class PermissionListItemRowMapper : GenericMapper<EntityPermissionListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, EntityPermissionListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
				.ForMember(biz => biz.UserLogin, opt => opt.MapFrom(row => row.Field<string>("UserLogin")))
				.ForMember(biz => biz.GroupName, opt => opt.MapFrom(row => row.Field<string>("GroupName")))
				.ForMember(biz => biz.LevelName, opt => opt.MapFrom(row => Translator.Translate(row.Field<string>("LevelName"))))
				.ForMember(biz => biz.PropagateToItems, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("PropagateToItems"))))
				.ForMember(biz => biz.Hide, opt => opt.MapFrom(row => row.Field<bool>("Hide")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("CREATED")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("MODIFIED")))
				.ForMember(biz => biz.LastModifiedByUserId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedByUserId"))))
				.ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")));

		}
	}
}
