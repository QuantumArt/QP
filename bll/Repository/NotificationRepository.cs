using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;

namespace Quantumart.QP8.BLL.Repository
{
    internal class NotificationRepository
    {
        internal static Notification UpdateProperties(Notification notification) => DefaultRepository.Update<Notification, NotificationsDAL>(notification);

        public void SendNotification(string connectionString, int siteId, string code, int id, bool isLive)
        {
            var cnn = new DBConnector(connectionString) { CacheData = false };
            #if !NET_STANDARD
            QPConfiguration.SetAppSettings(cnn.DbConnectorSettings);
            #endif
            cnn.SendNotification(siteId, code, id, string.Empty, isLive);
        }

        public void ClearEmailField(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ClearEmailField(scope.DbConnection, fieldId);
            }
        }

        public IEnumerable<Notification> GetUserNotifications(int userId)
        {
            return MapperFacade.NotificationMapper.GetBizList(QPContext.EFContext
                .NotificationsSet
                .Where(n => n.ToUser.Id == userId)
                .ToList()
            );
        }

        public IEnumerable<Notification> GetUserGroupNotifications(int groupId)
        {
            return MapperFacade.NotificationMapper.GetBizList(QPContext.EFContext
                .NotificationsSet
                .Where(n => n.ToUserGroup.Id == groupId)
                .ToList()
            );
        }

        internal static IEnumerable<Notification> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MapperFacade.NotificationMapper.GetBizList(QPContext.EFContext.NotificationsSet
                .Where(f => decIDs.Contains(f.Id))
                .ToList()
            );
        }

        internal static IEnumerable<NotificationListItem> List(ListCommand cmd, int contentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetNotificationsPage(scope.DbConnection, contentId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MapperFacade.NotificationListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static Notification GetPropertiesById(int id)
        {
            return MapperFacade.NotificationMapper.GetBizObject(QPContext.EFContext.NotificationsSet
                .Include("LastModifiedByUser")
                .Include("WorkFlow")
                .Include("FromUser")
                .Include("ToUser")
                .Include("ToUserGroup")
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static Notification SaveProperties(Notification notification)
        {
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Notification, notification);
            var result = DefaultRepository.Save<Notification, NotificationsDAL>(notification);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Notification);
            return result;
        }

        public static void Delete(int id)
        {
            DefaultRepository.Delete<NotificationsDAL>(id);
        }

        public static void MultipleDelete(int[] ids)
        {
            if (ids.Length > 0)
            {
                DefaultRepository.Delete<NotificationsDAL>(ids);
            }
        }

        internal static void CopySiteUpdateNotifications(string relationsBetweenObjectFormats, string relationsBetweenContents)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopySiteUpdateNotifications(scope.DbConnection, relationsBetweenObjectFormats, relationsBetweenContents);
            }
        }

        internal static void CopyContentNotifications(string relationsBetweenContentsXml, string relationsBetweenStatusesXml, string relationsBetweenAttributesXml)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentNotifications(QPConnectionScope.Current.DbConnection, relationsBetweenContentsXml, relationsBetweenStatusesXml, relationsBetweenAttributesXml);
            }
        }

        internal static IEnumerable<Notification> GetContentNotifications(int contentId, IEnumerable<string> codes)
        {
            return MapperFacade.NotificationMapper.GetBizList(QPContext.EFContext.NotificationsSet
                .Where(g => g.ContentId == contentId)
                .FilterByCode(codes)
                .ToList()
            );
        }
    }

    internal static class NotificationRepositoryExtensions
    {
        internal static IQueryable<NotificationsDAL> FilterByCode(this IQueryable<NotificationsDAL> source, IEnumerable<string> codes)
        {
            var codesSet = new HashSet<string>(codes);
            return source.Where(g =>
                g.ForCreate && codesSet.Contains(NotificationCode.Create) ||
                g.ForModify && codesSet.Contains(NotificationCode.Update) ||
                g.ForRemove && codesSet.Contains(NotificationCode.Delete) ||
                g.ForFrontend.Value && codesSet.Contains(NotificationCode.Custom) ||
                g.ForStatusChanged.Value && codesSet.Contains(NotificationCode.ChangeStatus) ||
                g.ForStatusPartiallyChanged && codesSet.Contains(NotificationCode.PartialChangeStatus) ||
                g.ForDelayedPublication && codesSet.Contains(NotificationCode.DelayedPublication)
            );
        }
    }
}
