using AutoMapper;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class PageTemplateMappper : GenericMapper<PageTemplate, PageTemplateDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<PageTemplate, PageTemplateDAL>()
                .ForMember(data => data.NetLanguages, opt => opt.Ignore())
                .ForMember(data => data.Object, opt => opt.Ignore())
                .ForMember(data => data.Page, opt => opt.Ignore())
                .ForMember(data => data.Site, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Locked, opt => opt.MapFrom(src => (src.LockedBy == 0) ? null : (DateTime?)src.Locked))
				.ForMember(data => data.LockedBy, opt => opt.MapFrom(src => (src.LockedBy == 0) ? null : Utils.Converter.ToNullableDecimal((int?)src.LockedBy)))
                .AfterMap(SetDalProperties);
        }

        private static void SetDalProperties(PageTemplate bizObject, PageTemplateDAL dataObject)
        {
            dataObject.TemplatePicture = string.Empty;

			if (!bizObject.SiteIsDotNet)
			{
				dataObject.NetLanguageId = null;
				dataObject.CodeBehind = null;
				dataObject.PreviewCodeBehind = null;
				dataObject.EnableViewstate = true;
				dataObject.DisableDatabind = false;
				dataObject.CustomClassForPages = null;
				dataObject.TemplateCustomClass = null;
				dataObject.CustomClassForGenerics = null;
				dataObject.CustomClassForContainers = null;
				dataObject.CustomClassForForms = null;
				dataObject.Using = null;
			}
        }		
    }
}
