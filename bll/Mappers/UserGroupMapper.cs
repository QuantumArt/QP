using System.Linq;
using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UserGroupMapper : GenericMapper<UserGroup, UserGroupDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UserGroup, UserGroupDAL>(MemberList.Destination)
                .ForMember(dal => dal.ContentAccess, opt => opt.Ignore())
                .ForMember(dal => dal.ContentFolderAccess, opt => opt.Ignore())
                .ForMember(dal => dal.ArticleAccess, opt => opt.Ignore())
                .ForMember(dal => dal.FolderAccess, opt => opt.Ignore())
                .ForMember(dal => dal.Notifications, opt => opt.Ignore())
                .ForMember(dal => dal.SiteAccess, opt => opt.Ignore())
                .ForMember(dal => dal.WorkflowAccess, opt => opt.Ignore())
                .ForMember(dal => dal.WorkflowRules, opt => opt.Ignore())
                .ForMember(dal => dal.Users, opt => opt.Ignore())
                .ForMember(dal => dal.ChildGroups, opt => opt.Ignore())
                .ForMember(dal => dal.ParentGroups, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore());
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<UserGroupDAL, UserGroup>(MemberList.Source)
                .ForMember(biz => biz.ParentGroup, opt => opt.MapFrom(
                    data => data.ParentGroups != null && data.ParentGroups.Any() ? data.ParentGroups.FirstOrDefault() : null)
                );
        }
    }
}
