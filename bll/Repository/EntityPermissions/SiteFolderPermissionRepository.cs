using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class SiteFolderPermissionRepository : IPermissionRepository
    {
        public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetSiteFolderPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MappersRepository.PermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public EntityPermission GetById(int id, bool include = true)
        {
            ObjectQuery<SiteFolderPermissionDAL> set = QPContext.EFContext.SiteFolderPermissionSet;
            if (include)
            {
                set = set.Include("User").Include("Group").Include("LastModifiedByUser");
            }

            return MappersRepository.SiteFolderPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
        }

        public EntityPermission Save(EntityPermission permission)
        {
            return DefaultRepository.Save<EntityPermission, SiteFolderPermissionDAL>(permission);
        }

        public EntityPermission Update(EntityPermission permission)
        {
            return DefaultRepository.Update<EntityPermission, SiteFolderPermissionDAL>(permission);
        }


        public bool CheckUnique(EntityPermission permission)
        {
            return !QPContext.EFContext.SiteFolderPermissionSet.Any(p =>
                p.FolderId == permission.ParentEntityId &&
                (permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
                (permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null));
        }

        public void MultipleRemove(IEnumerable<int> IDs)
        {
            DefaultRepository.Delete<SiteFolderPermissionDAL>(IDs.ToArray());
        }

        public void Remove(int id)
        {
            DefaultRepository.Delete<SiteFolderPermissionDAL>(id);
        }
    }
}
