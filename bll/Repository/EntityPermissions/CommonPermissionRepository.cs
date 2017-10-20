using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal static class CommonPermissionRepository
	{
		private static Lazy<IEnumerable<EntityPermissionLevel>> permissionLevel = new Lazy<IEnumerable<EntityPermissionLevel>>(() =>
			MapperFacade.EntityPermissionLevelMapper.GetBizList(QPContext.EFContext.PermissionLevelSet.OrderByDescending(p => p.Level).ToList()),
			true);
		internal static IEnumerable<EntityPermissionLevel> GetPermissionLevels() => permissionLevel.Value;
	}
}
