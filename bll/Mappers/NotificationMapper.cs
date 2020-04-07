using AutoMapper;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class NotificationMapper : GenericMapper<Notification, NotificationsDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Notification, NotificationsDAL>(MemberList.Destination)
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.Workflow, opt => opt.Ignore())
                .ForMember(data => data.FromUser, opt => opt.Ignore())
                .ForMember(data => data.ToUser, opt => opt.Ignore())
                .ForMember(data => data.ToUserGroup, opt => opt.Ignore())
                .ForMember(data => data.FromBackenduserId, opt => opt.MapFrom(src => src.FromBackenduser ? src.FromBackenduserId : SpecialIds.AdminUserId))
                .ForMember(data => data.Content, opt => opt.Ignore())
                .AfterMap(SetDalProperties);
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<NotificationsDAL, Notification>(MemberList.Source).AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(NotificationsDAL dataObject, Notification bizObject)
        {
            bizObject.SelectedReceiverType = bizObject.ComputeReceiverType();
            if (string.IsNullOrEmpty(bizObject.ExternalUrl))
            {
                bizObject.ExternalUrl = bizObject.Content.Site.ExternalUrl;
            }
        }

        private static void SetDalProperties(Notification bizObject, NotificationsDAL dataObject)
        {
            if (string.Equals(bizObject.ExternalUrl, bizObject.Content.Site.ExternalUrl) || !bizObject.IsExternal)
            {
                dataObject.ExternalUrl = null;
            }
        }
    }
}
