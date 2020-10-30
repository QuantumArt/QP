using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal static class CommonPermissionRepository
    {
        internal static IEnumerable<EntityPermissionLevel> GetPermissionLevels()
        {
            return MapperFacade.EntityPermissionLevelMapper.GetBizList(
                QPContext.EFContext.PermissionLevelSet.OrderByDescending(p => p.Level).ToList()
            );
        }
    }
}
