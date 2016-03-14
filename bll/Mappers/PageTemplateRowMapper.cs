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
    class PageTemplateRowMapper : GenericMapper<PageTemplateListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, PageTemplateListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
                .ForMember(biz => biz.Folder, opt => opt.MapFrom(row => "\\" + row.Field<string>("Folder")))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => row.Field<string>("Description")))
                .ForMember(biz => biz.IsSystem, opt => opt.MapFrom(row => row.Field<bool>("IsSystem")))                
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal?>("LockedBy"), 0)))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedBy, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("LastModifiedBy"))))
                .ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LastModifiedByLogin"), String.Empty)))
                .ForMember(biz => biz.LockedByFullName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LockedByFullName"), String.Empty)))
                .AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(DataRow dataObject, PageTemplateListItem bizObject)
        {
            bizObject.LockedByIcon = PageTemplate.GetLockedByIcon(bizObject.LockedBy);
            bizObject.LockedByToolTip = PageTemplate.GetLockedByToolTip(bizObject.LockedBy, bizObject.LockedByFullName);
        }
    }

	class PageTemplateSearchResultRowMapper : GenericMapper<PageTemplateSearchListItem, DataRow>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<DataRow, PageTemplateSearchListItem>()
					.ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
					.ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
					.ForMember(biz => biz.Description, opt => opt.Ignore())
					.ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
					.ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))				
					.ForMember(biz => biz.LastModifiedByLogin, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("LastModifiedByLogin"), String.Empty)))
					.ForMember(biz => biz.ParentId, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ParentId"))))
					.ForMember(biz => biz.ParentName, opt => opt.MapFrom(row => row.Field<string>("ParentName")))
					;
		}
	}
}
