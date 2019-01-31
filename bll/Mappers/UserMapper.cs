using System;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UserMapper : GenericMapper<User, UserDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<User, UserDAL>(MemberList.Destination)
                .ForMember(data => data.LanguageId, opt => opt.MapFrom(src => src.LanguageId))
                .ForMember(data => data.AccessRules, opt => opt.Ignore())
                .ForMember(data => data.Container, opt => opt.Ignore())
                .ForMember(data => data.Content, opt => opt.Ignore())
                .ForMember(data => data.ContentAccess, opt => opt.Ignore())
                .ForMember(data => data.ContentFolderAccess, opt => opt.Ignore())
                .ForMember(data => data.ContentForm, opt => opt.Ignore())
                .ForMember(data => data.FolderAccess, opt => opt.Ignore())
                .ForMember(data => data.Groups, opt => opt.Ignore())
                .ForMember(data => data.Languages, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedSites, opt => opt.Ignore())
                .ForMember(data => data.LockedByArticles, opt => opt.Ignore())
                .ForMember(data => data.LockedSites, opt => opt.Ignore())
                .ForMember(data => data.Notifications, opt => opt.Ignore())
                .ForMember(data => data.NotificationsForBackendUser, opt => opt.Ignore())
                .ForMember(data => data.Object, opt => opt.Ignore())
                .ForMember(data => data.ObjectFormat, opt => opt.Ignore())
                .ForMember(data => data.Page, opt => opt.Ignore())
                .ForMember(data => data.PageTemplate, opt => opt.Ignore())
                .ForMember(data => data.SiteAccess, opt => opt.Ignore())
                .ForMember(data => data.UserToPanel, opt => opt.Ignore())
                .ForMember(data => data.WaitingForApproval, opt => opt.Ignore())
                .ForMember(data => data.WorkflowAccess, opt => opt.Ignore())
                .ForMember(data => data.WorkflowRules, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.STATUS_TYPE, opt => opt.Ignore())
                .ForMember(data => data.LastLogOn, opt => opt.MapFrom(biz => biz.LastLogOn == default(DateTime) ? null : biz.LastLogOn))

      ;
        }
    }
}
