using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    internal static class ContentPermissionHelper
    {
        /// <summary>
        /// Фильтрует id связанных контентов оставляя только те для которых нет permission уровня равного или больше чем чтение
        /// </summary>
        /// <param name="relatedContentId"></param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static IEnumerable<int> FilterNoPermissionContent(List<int> relatedContentId, int? userId, int? groupId)
        {
            var cIDs = Converter.ToDecimalCollection(relatedContentId.Distinct());

            // query возвращает подмножество связанных контентов для user/group c уровнем доступа >= Read
            var query = QPContext.EFContext.ContentPermissionSet.Where(p => cIDs.Contains(p.ContentId) && p.PermissionLevel.Level >= PermissionLevel.Read);
            if (userId.HasValue)
            {
                var uid = Converter.ToDecimal(userId.Value);
                query = query.Where(p => p.UserId == uid);
            }
            else if (groupId.HasValue)
            {
                var gid = Converter.ToDecimal(groupId.Value);
                query = query.Where(p => p.GroupId == gid);
            }

            var existedPermissionRelContents = Converter.ToInt32Collection(query
                .Select(p => p.ContentId)
                .Distinct()
                .ToArray()
            );

            // Возращает только те связанные контенты которые не входят в множество query
            return relatedContentId.Where(id => !existedPermissionRelContents.Contains(id)).ToArray();
        }

        /// <summary>
        /// Создает permisions ордновременно для множества контентов
        /// </summary>
        /// <param name="contentIDs"></param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <param name="permissionLevel"></param>
        internal static void MultipleSetPermission(List<int> contentIDs, int? userId, int? groupId, int permissionLevel)
        {
            var permissionlevelId = CommonPermissionRepository.GetPermissionLevels().Single(l => l.Level == permissionLevel).Id;
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildContentPermissions(scope.DbConnection, contentIDs, userId, groupId);
                Common.InsertChildContentPermissions(scope.DbConnection, contentIDs, userId, groupId, permissionlevelId, false, QPContext.CurrentUserId, false);
            }
        }
    }
}
