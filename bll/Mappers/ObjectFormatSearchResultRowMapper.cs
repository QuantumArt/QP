using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Mappers
{
	class ObjectFormatSearchResultRowMapper : GenericMapper<ObjectFormatSearchResultListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ObjectFormatSearchResultListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
				.ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
				.ForMember(biz => biz.TemplateName, opt => opt.MapFrom(row => row.Field<string>("TemplateName")))
				.ForMember(biz => biz.PageName, opt => opt.MapFrom(row => row.Field<string>("PageName")))
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))				
				.ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
				.ForMember(biz => biz.ParentName, opt => opt.MapFrom(row => row.Field<string>("ParentName")))
				.ForMember(biz => biz.ParentId, opt => opt.MapFrom(row => row.Field<decimal>("ParentId")))
				.AfterMap(SetBizProperties);
		}

		private static void SetBizProperties(DataRow dataObject, ObjectFormatListItem bizObject)
		{
		}
	}
}
