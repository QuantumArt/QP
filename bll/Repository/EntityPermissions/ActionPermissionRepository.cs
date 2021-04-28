using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class ActionPermissionRepository : IPermissionRepository
    {
        public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                var rows = Common.GetActionPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public EntityPermission GetById(int id, bool include = true)
        {
            var set = QPContext.EFContext.BackendActionPermissionSet.AsQueryable();
            if (include)
            {
                set = set.Include("User").Include("Group").Include("LastModifiedByUser");
            }

            return MapperFacade.BackendActionPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
        }

        public EntityPermission Save(EntityPermission permission) => DefaultRepository.Save<EntityPermission, BackendActionPermissionDAL>(permission);

        public bool CheckUnique(EntityPermission permission)
        {
            return !QPContext.EFContext.BackendActionPermissionSet
                .Where(p => p.ActionId == permission.ParentEntityId)
                .AsEnumerable()
                .Any(p => PermissionUserOrGroupEquals(permission, p));
        }

        private static bool PermissionUserOrGroupEquals(EntityPermissionBase permission, BackendActionPermissionDAL p) =>
            (permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
            (permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null);


        public EntityPermission Update(EntityPermission permission) => DefaultRepository.Update<EntityPermission, BackendActionPermissionDAL>(permission);

        public void MultipleRemove(IEnumerable<int> ids)
        {
            DefaultRepository.Delete<BackendActionPermissionDAL>(ids.ToArray());
        }

        public void Remove(int id)
        {
            DefaultRepository.Delete<BackendActionPermissionDAL>(id);
        }
    }
}
