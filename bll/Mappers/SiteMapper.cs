using System;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class SiteMapper : GenericMapper<Site, SiteDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<SiteDAL, Site>()
                .ForMember(biz => biz.LockedBy, opt => opt.MapFrom(src => Utils.Converter.ToInt32(src.LockedBy)))
                .ForMember(biz => biz.Locked, opt => opt.MapFrom(src => Utils.Converter.ToDateTime(src.Locked)))
                .ForMember(biz => biz.IsLive, opt => opt.MapFrom(src => Utils.Converter.ToBoolean(src.IsLive)))
                .ForMember(biz => biz.AssemblingType, opt => opt.MapFrom(src => src.ScriptLanguage))
                .ForMember(biz => biz.SeparateDns, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.StageDns)))
                .ForMember(biz => biz.ExternalCss, opt => opt.MapFrom(src => src.ExternalCss));
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<Site, SiteDAL>()
                .ForMember(data => data.IsLive, opt => opt.MapFrom(src => Utils.Converter.ToInt32(src.IsLive)))
                .ForMember(data => data.ScriptLanguage, opt => opt.MapFrom(src => src.AssemblingType))
                .ForMember(data => data.Locked, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (DateTime?)src.Locked))
                .ForMember(data => data.LockedBy, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (int?)src.LockedBy))
                .ForMember(data => data.AccessRules, opt => opt.Ignore())
                .ForMember(data => data.CodeSnippets, opt => opt.Ignore())
                .ForMember(data => data.ContentGroups, opt => opt.Ignore())
                .ForMember(data => data.Folders, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.LockedByUser, opt => opt.Ignore())
                .ForMember(data => data.PageTemplates, opt => opt.Ignore())
                .ForMember(data => data.Statuses, opt => opt.Ignore())
                .ForMember(data => data.Styles, opt => opt.Ignore())
                .ForMember(data => data.Workflows, opt => opt.Ignore())
                .ForMember(data => data.CustomActions, opt => opt.Ignore());
        }
    }
}
