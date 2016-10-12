using System;
using AutoMapper;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class PageMapper : GenericMapper<Page, PageDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<Page, PageDAL>()
                .ForMember(data => data.ContentForm, opt => opt.Ignore())
                .ForMember(data => data.Object, opt => opt.Ignore())
                .ForMember(data => data.PageTemplate, opt => opt.Ignore())
                .ForMember(data => data.PageTrace, opt => opt.Ignore())
                .ForMember(data => data.Locked, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (DateTime?)src.Locked))
                .ForMember(data => data.LockedBy, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (int?)src.LockedBy))
                .ForMember(data => data.Assembled, opt => opt.MapFrom(src => src.LastAssembledBy == 0 ? DateTime.Now : (DateTime?)src.Assembled))
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.LockedByUser, opt => opt.Ignore())
                .AfterMap(SetDalProperties);
        }

        private static void SetDalProperties(Page bizObject, PageDAL dataObject)
        {
            SetDalProxyCache(bizObject, dataObject);

            dataObject.Codepage = PageTemplateRepository.GetCharsetByName(bizObject.Charset).Codepage;

            if (!bizObject.PageTemplate.SiteIsDotNet)
            {
                dataObject.CustomClass = null;
                dataObject.EnableViewstate = false;
            }

            if (!bizObject.ProxyCache && !bizObject.BrowserCaching)
            {
                dataObject.CacheHours = 0;
            }
        }

        private static void SetDalProxyCache(Page bizObject, PageDAL dataObject)
        {
            if (bizObject.ProxyCache && bizObject.BrowserCaching)
            {
                dataObject.ProxyCache = 3;
            }
            else if (bizObject.ProxyCache && !bizObject.BrowserCaching)
            {
                dataObject.ProxyCache = 1;
            }
            else if (!bizObject.ProxyCache && bizObject.BrowserCaching)
            {
                dataObject.ProxyCache = 2;
            }
            else if (!bizObject.ProxyCache && !bizObject.BrowserCaching)
            {
                dataObject.ProxyCache = 0;
            }
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<PageDAL, Page>().AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(PageDAL dataObject, Page bizObject)
        {
            SetBizProxyCache(dataObject, bizObject);
        }

        private static void SetBizProxyCache(PageDAL dataObject, Page bizObject)
        {
            switch ((int)dataObject.ProxyCache)
            {
                case 0:
                    bizObject.ProxyCache = false;
                    bizObject.BrowserCaching = false;
                    break;
                case 1:
                    bizObject.ProxyCache = true;
                    bizObject.BrowserCaching = false;
                    break;
                case 2:
                    bizObject.ProxyCache = false;
                    bizObject.BrowserCaching = true;
                    break;
                case 3:
                    bizObject.ProxyCache = true;
                    bizObject.BrowserCaching = true;
                    break;
                default:
                    bizObject.ProxyCache = false;
                    bizObject.BrowserCaching = false;
                    break;
            }

            if (dataObject.CacheHours == 0)
            {
                bizObject.CacheHours = 1;
            }
        }
    }
}
