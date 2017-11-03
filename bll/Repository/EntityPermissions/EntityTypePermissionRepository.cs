using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class EntityTypePermissionRepository : IPermissionRepository
    {
        #region IPermissionRepository Members

        public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetEntityTypePermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public EntityPermission GetById(int id, bool include = true)
        {
            ObjectQuery<EntityTypePermissionDAL> set = QPContext.EFContext.EntityTypePermissionSet;
            if (include)
            {
                set = set
                    .Include("User")
                    .Include("Group")
                    .Include("LastModifiedByUser");
            }
            return MapperFacade.EntityTypePermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
        }

        public EntityPermission Save(EntityPermission permission) => DefaultRepository.Save<EntityPermission, EntityTypePermissionDAL>(permission);

        public bool CheckUnique(EntityPermission permission)
        {
            return !QPContext.EFContext.EntityTypePermissionSet
                .Any(p =>
                    p.EntityTypeId == permission.ParentEntityId &&
                    (permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
                    (permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
                );
        }

        public EntityPermission Update(EntityPermission permission) => DefaultRepository.Update<EntityPermission, EntityTypePermissionDAL>(permission);

        public void MultipleRemove(IEnumerable<int> IDs)
        {
            DefaultRepository.Delete<EntityTypePermissionDAL>(IDs.ToArray());
        }

        public void Remove(int id)
        {
            DefaultRepository.Delete<EntityTypePermissionDAL>(id);
        }

        #endregion
    }
}
