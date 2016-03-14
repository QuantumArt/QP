using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class TreeNodeMapper : GenericMapper<TreeNode, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, TreeNode>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<long>("ID"))))
				.ForMember(biz => biz.Code, opt => opt.MapFrom(row => row.Field<string>("CODE")))
				.ForMember(biz => biz.ParentId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<long?>("PARENT_ID"))))
				.ForMember(biz => biz.ParentGroupId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<long?>("PARENT_GROUP_ID"))))
				.ForMember(biz => biz.IsFolder, opt => opt.MapFrom(row => row.Field<bool>("IS_FOLDER")))
				.ForMember(biz => biz.IsGroup, opt => opt.MapFrom(row => row.Field<bool>("IS_GROUP")))
				.ForMember(biz => biz.GroupItemCode, opt => opt.MapFrom(row => row.Field<string>("GROUP_ITEM_CODE")))
				.ForMember(biz => biz.Icon, opt => opt.MapFrom(row => row.Field<string>("ICON")))
				.ForMember(biz => biz.Title, opt => opt.MapFrom(row => row.Field<string>("TITLE")))
				.ForMember(biz => biz.DefaultActionCode, opt => opt.MapFrom(row => row.Field<string>("DEFAULT_ACTION_CODE")))
				.ForMember(biz => biz.DefaultActionTypeCode, opt => opt.MapFrom(row => row.Field<string>("DEFAULT_ACTION_TYPE_CODE")))
				.ForMember(biz => biz.ContextMenuCode, opt => opt.MapFrom(row => row.Field<string>("CONTEXT_MENU_CODE")))
				.ForMember(biz => biz.HasChildren, opt => opt.MapFrom(row => row.Field<bool>("HAS_CHILDREN")));				
		}
	}
}
