using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal static class CommonPermissionRepository
	{
		private static Lazy<IEnumerable<EntityPermissionLevel>> permissionLevel = new Lazy<IEnumerable<EntityPermissionLevel>>(() =>
			MapperFacade.EntityPermissionLevelMapper.GetBizList(QPContext.EFContext.PermissionLevelSet.OrderByDescending(p => p.Level).ToList()),
			true);
		internal static IEnumerable<EntityPermissionLevel> GetPermissionLevels()
		{
			return permissionLevel.Value;
		}
	}
}
