using System.Linq;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UserGroupMapper : GenericMapper<UserGroup, UserGroupDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<UserGroup, UserGroupDAL>()
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

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<UserGroupDAL, UserGroup>()
                .ForMember(biz => biz.ParentGroup, opt => opt.MapFrom(data => data.ParentGroups.IsLoaded ? data.ParentGroups.FirstOrDefault() : null));
        }
    }
}
