using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;


namespace Quantumart.QP8.BLL.Repository
{
    class NotificationRepository
    {
        internal static Notification UpdateProperties(Notification notification)
        {
            return DefaultRepository.Update<Notification, NotificationsDAL>(notification);
        }

        public void SendNotification(string connectionString, int siteId, string code, int id, bool isLive)
        {
            QPConfiguration.SetAppSettings(DBConnector.AppSettings);
            DBConnector cnn = new DBConnector(connectionString) {CacheData = false};
            cnn.SendNotification(siteId, code, id, String.Empty, isLive);
        }

        /// <summary>
        /// Удалить ссылки на поле
        /// </summary>
        /// <param name="fieldId"></param>
        public void ClearEmailField(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ClearEmailField(scope.DbConnection, fieldId);
            }
        }

        public IEnumerable<Notification> GetUserNotifications(int userId)
        {
            return MappersRepository.NotificationMapper.GetBizList(QPContext.EFContext
                .NotificationsSet
                .Where(n => n.ToUser.Id == userId)
                .ToList()
            );
        }

        public IEnumerable<Notification> GetUserGroupNotifications(int groupId)
        {
            return MappersRepository.NotificationMapper.GetBizList(QPContext.EFContext
                .NotificationsSet
                .Where(n => n.ToUserGroup.Id == groupId)
                .ToList()
            );
        }

        /// <summary>
        /// Возвращает список по ids
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Notification> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MappersRepository.NotificationMapper
                .GetBizList(QPContext.EFContext.NotificationsSet
                    .Where(f => decIDs.Contains(f.Id))
                    .ToList()
                );
        }

        internal static IEnumerable<NotificationListItem> List(ListCommand cmd, int contentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.GetNotificationsPage(scope.DbConnection, contentId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MappersRepository.NotificationListItemRowMapper.GetBizList(rows.ToList());                     
            }
        }

        internal static Notification GetPropertiesById(int id)
        {
            return MappersRepository.NotificationMapper.GetBizObject(QPContext.EFContext.NotificationsSet
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
                DefaultRepository.Delete<NotificationsDAL>(ids);
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
            return MappersRepository.NotificationMapper.GetBizList(
                QPContext.EFContext.NotificationsSet
                .Where(g => g.ContentId == contentId && g.UseService && g.IsExternal)
                .FilterByCode(codes)
                .ToList()
            );
        }

        internal static IEnumerable<Notification> GetContentNotifications(IEnumerable<int> articleIds, IEnumerable<string> codes)
        {
            var contentIds = QPContext.EFContext.ArticleSet.GroupBy(a => a.ContentId).Select(g => g.Key).ToArray();

            return MappersRepository.NotificationMapper.GetBizList(
                QPContext.EFContext.NotificationsSet
                .Where(g => contentIds.Contains(g.ContentId) && g.UseService && g.IsExternal)
                .FilterByCode(codes)
                .ToList()
            );
        }
    }

    internal static class NotificationRepositoryExtensions
    {
        internal static IQueryable<NotificationsDAL> FilterByCode(this IQueryable<NotificationsDAL> source, IEnumerable<string> codes)
        {
            var c = new HashSet<string>(codes);
            return source.Where(g =>
                        g.ForCreate && c.Contains(NotificationCode.Create) ||
                        g.ForModify && c.Contains(NotificationCode.Update) ||
                        g.ForRemove && c.Contains(NotificationCode.Delete) ||
                        g.ForFrontend.Value && c.Contains(NotificationCode.Custom) ||
                        g.ForStatusChanged.Value && c.Contains(NotificationCode.ChangeStatus) ||
                        g.ForStatusPartiallyChanged && c.Contains(NotificationCode.PartialChangeStatus) ||
                        g.ForDelayedPublication && c.Contains(NotificationCode.DelayedPublication)
                    );
        }
    }

}
