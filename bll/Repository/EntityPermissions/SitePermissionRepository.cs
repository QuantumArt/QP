using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class SitePermissionRepository : IPermissionRepository
    {
        #region IPermissionRepository Members

        public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetSitePermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public EntityPermission GetById(int id, bool include = true)
        {
            DbQuery<SitePermissionDAL> set = QPContext.EFContext.SitePermissionSet;
            if (include)
            {
                set = set
                    .Include("User")
                    .Include("Group")
                    .Include("LastModifiedByUser");
            }
            return MapperFacade.SitePermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
        }

        public EntityPermission Save(EntityPermission permission) => DefaultRepository.Save<EntityPermission, SitePermissionDAL>(permission);

        public EntityPermission Update(EntityPermission permission) => DefaultRepository.Update<EntityPermission, SitePermissionDAL>(permission);

        public bool CheckUnique(EntityPermission permission)
        {
            return !QPContext.EFContext.SitePermissionSet
                .Any(p =>
                    p.SiteId == permission.ParentEntityId &&
                    (permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
                    (permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
                );
        }

        public void MultipleRemove(IEnumerable<int> IDs)
        {
            DefaultRepository.Delete<SitePermissionDAL>(IDs.ToArray());
        }

        public void Remove(int id)
        {
            DefaultRepository.Delete<SitePermissionDAL>(id);
        }

        #endregion
    }
}
