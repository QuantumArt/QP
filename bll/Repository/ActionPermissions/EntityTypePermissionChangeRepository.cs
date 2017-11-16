using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.ActionPermissions
{
    internal class EntityTypePermissionChangeRepository : IActionPermissionChangeRepository
    {
        public EntityPermission ReadForUser(int parentId, int userId)
        {
            var permission = QPContext.EFContext.EntityTypePermissionSet.SingleOrDefault(p => p.EntityTypeId == parentId && p.UserId == userId);
            return MapperFacade.EntityTypePermissionMapper.GetBizObject(permission);
        }

        public EntityPermission ReadForGroup(int parentId, int groupId)
        {
            var permission = QPContext.EFContext.EntityTypePermissionSet.SingleOrDefault(p => p.EntityTypeId == parentId && p.GroupId == groupId);
            return MapperFacade.EntityTypePermissionMapper.GetBizObject(permission);
        }
    }
}
