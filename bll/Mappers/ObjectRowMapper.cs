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
    class ObjectRowMapper : GenericMapper<ObjectListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, ObjectListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.TypeName, opt => opt.MapFrom(row => row.Field<string>("TypeName")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedBy"))))
                .ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
                .ForMember(biz => biz.Icon, opt => opt.MapFrom(row => row.Field<string>("Icon")))
				.ForMember(biz => biz.LockedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal?>("LockedBy"), 0)))
				.ForMember(biz => biz.LockedByFullName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LockedByFullName"), String.Empty)))
				.ForMember(biz => biz.ParentId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("parentId"), null)))
				.ForMember(biz => biz.Overriden, opt => opt.MapFrom(row => row.Field<bool>("Overriden")))
				.AfterMap(SetBizProperties);
        }

		private static void SetBizProperties(DataRow dataObject, ObjectListItem bizObject)
		{
			bizObject.LockedByIcon = BllObject.GetLockedByIcon(bizObject.LockedBy);
			bizObject.LockedByToolTip = BllObject.GetLockedByToolTip(bizObject.LockedBy, bizObject.LockedByFullName);
		}
    }

	class ObjectSearchResultRowMapper : GenericMapper<ObjectSearchListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, ObjectSearchListItem>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
				.ForMember(biz => biz.TemplateName, opt => opt.MapFrom(row => row.Field<string>("TemplateName")))
				.ForMember(biz => biz.PageName, opt => opt.MapFrom(row => row.Field<string>("PageName")))
				.ForMember(biz => biz.Description, opt => opt.Ignore())
				.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
				.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
				.ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByLogin")))
				.AfterMap(SetBizProperties);
		}

		private static void SetBizProperties(DataRow dataObject, ObjectSearchListItem bizObject)
		{
			bizObject.ParentId = dataObject.Field<decimal?>("PageId") == null ? Converter.ToInt32(dataObject.Field<decimal?>("PageTemplateId")) : Converter.ToInt32(dataObject.Field<decimal?>("PageId"));
		}
	}
}
