using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
	class ChildEntityPermissionListItemRowMapper : GenericMapper<ChildEntityPermissionListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ChildEntityPermissionListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
				.ForMember(biz => biz.IsExplicit, opt => opt.MapFrom(row => row.Field<bool>("IsExplicit")))
				.ForMember(biz => biz.Hide, opt => opt.MapFrom(row => row.Field<bool>("Hide")))
				.ForMember(biz => biz.PropagateToItems, opt => opt.MapFrom(row => row.Field<bool>("PropagateToItems")))
				.ForMember(biz => biz.Title, opt => opt.MapFrom(row => row.Field<string>("Title")))
				.ForMember(biz => biz.LevelName, opt => 
					opt.MapFrom(row => Translator.Translate(row.Field<string>("LevelName")) ?? EntityPermissionStrings.UndefinedPermissionLevel)
				);				
		}
	}
}
