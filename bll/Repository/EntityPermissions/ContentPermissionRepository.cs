using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal interface IContentPermissionRepository : IPermissionRepository
    {
        /// <summary>
        /// Фильтрует id связанных контентов оставляя только те для которых нет permission уровня равного или больше чем чтение
        /// </summary>
        IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentId, int? userId, int? groupId);

        /// <summary>
        /// Возвращает контент
        /// </summary>
        Content GetContentById(int contentId);

        /// <summary>
        /// Создает permisions ордновременно для множества контентов
        /// </summary>
        void MultipleSetPermission(IEnumerable<int> contentIDs, int? userId, int? groupId, int permissionLevel);
    }

    internal class ContentPermissionRepository : IContentPermissionRepository
    {
        public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetContentPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public EntityPermission GetById(int id, bool include = true)
        {
            var set = QPContext.EFContext.ContentPermissionSet.AsQueryable();
            if (include)
            {
                set = set
                    .Include("User")
                    .Include("Group")
                    .Include("LastModifiedByUser");
            }

            return MapperFacade.ContentPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
        }

        public EntityPermission Save(EntityPermission permission) => DefaultRepository.Save<EntityPermission, ContentPermissionDAL>(permission);

        public EntityPermission Update(EntityPermission permission) => DefaultRepository.Update<EntityPermission, ContentPermissionDAL>(permission);

        public bool CheckUnique(EntityPermission permission)
        {
            return !QPContext.EFContext.ContentPermissionSet.Any(p =>
                p.ContentId == permission.ParentEntityId
                && (permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null)
                && (permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
            );
        }

        public void MultipleRemove(IEnumerable<int> ids)
        {
            DefaultRepository.Delete<ContentPermissionDAL>(ids.ToArray());
        }

        public void Remove(int id)
        {
            DefaultRepository.Delete<ContentPermissionDAL>(id);
        }

        public IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentId, int? userId, int? groupId)
            => ContentPermissionHelper.FilterNoPermissionContent(relatedContentId.ToList(), userId, groupId);

        public Content GetContentById(int contentId) => ContentRepository.GetByIdWithFields(contentId);

        public void MultipleSetPermission(IEnumerable<int> contentIds, int? userId, int? groupId, int permissionLevel)
        {
            ContentPermissionHelper.MultipleSetPermission(contentIds.ToList(), userId, groupId, permissionLevel);
        }
    }
}
