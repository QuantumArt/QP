using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentMapper : GenericMapper<Content, ContentDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ContentDAL, Content>(MemberList.Source)
                .ForMember(biz => biz.Fields, opt => opt.Ignore())
                .ForMember(biz => biz.Constraints, opt => opt.Ignore())
                .ForMember(biz => biz.GroupId, opt => opt.MapFrom(dal => dal.GroupId))
                .ForMember(biz => biz.JoinRootId, opt => opt.MapFrom(dal => dal.JoinId))
                .ForMember(biz => biz.StoredVirtualType, opt => opt.MapFrom(dal => dal.VirtualType))
                .ForMember(biz => biz.UserQuery, opt => opt.MapFrom(dal => dal.Query))
                .ForMember(biz => biz.UserQueryAlternative, opt => opt.MapFrom(dal => dal.AltQuery))
                .ForMember(biz => biz.WorkflowBinding, opt => opt.Ignore())
                .ForMember(biz => biz.ParentContent, opt => opt.Ignore())
                .ForMember(biz => biz.ChildContents, opt => opt.Ignore())
                .AfterMap(SetBizProperties);
            ;
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Content, ContentDAL>(MemberList.Destination)
                .ForMember(data => data.GroupId, opt => opt.MapFrom(biz => biz.GroupId))
                .ForMember(data => data.JoinId, opt => opt.MapFrom(biz => biz.JoinRootId))
                .ForMember(data => data.Query, opt => opt.MapFrom(biz => biz.UserQuery))
                .ForMember(data => data.AltQuery, opt => opt.MapFrom(biz => biz.UserQueryAlternative))
                .ForMember(data => data.AccessRules, opt => opt.Ignore())
                .ForMember(data => data.Articles, opt => opt.Ignore())
                .ForMember(data => data.Containers, opt => opt.Ignore())
                .ForMember(data => data.Constraints, opt => opt.Ignore())
                .ForMember(data => data.Fields, opt => opt.Ignore())
                .ForMember(data => data.Folders, opt => opt.Ignore())
                .ForMember(data => data.Group, opt => opt.Ignore())
                .ForMember(data => data.JoinContents, opt => opt.Ignore())
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.LinkedContents, opt => opt.Ignore())
                .ForMember(data => data.Notifications, opt => opt.Ignore())
                .ForMember(data => data.Site, opt => opt.Ignore())
                .ForMember(data => data.UnionContents, opt => opt.Ignore())
                .ForMember(data => data.UnionContents1, opt => opt.Ignore())
                .ForMember(data => data.UnionContents2, opt => opt.Ignore())
                .ForMember(data => data.UserQueryAttrs, opt => opt.Ignore())
                .ForMember(data => data.UserQueryContents, opt => opt.Ignore())
                .ForMember(data => data.UserQueryContents1, opt => opt.Ignore())
                .ForMember(data => data.WorkflowBinding, opt => opt.Ignore())
                .ForMember(data => data.CustomActions, opt => opt.Ignore())
                .ForMember(data => data.ParentContent, opt => opt.Ignore())
                .ForMember(data => data.ChildContents, opt => opt.Ignore())
                .AfterMap(SetDalProperties);
        }

        private static void SetBizProperties(ContentDAL dataObject, Content bizObject)
        {
            if (dataObject != null && dataObject.MaxNumOfStoredVersions == 0)
            {
                bizObject.MaxNumOfStoredVersions = Content.DefaultLimitOfStoredVersions;
                bizObject.UseVersionControl = false;
            }
        }

        private static void SetDalProperties(Content bizObject, ContentDAL dataObject)
        {
            if (bizObject != null && !bizObject.UseVersionControl)
            {
                dataObject.MaxNumOfStoredVersions = 0;
            }
        }
    }
}
