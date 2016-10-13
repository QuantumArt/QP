using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository.ActionPermissions
{
	internal class ActionPermissionChangeRepository : IActionPermissionChangeRepository
	{		

		public EntityPermission ReadForUser(int parentId, int userId)
		{
			var permission = QPContext.EFContext.BackendActionPermissionSet.SingleOrDefault(p => p.ActionId == parentId && p.UserId == userId);			
			return MapperFacade.BackendActionPermissionMapper.GetBizObject(permission);
		}

		public EntityPermission ReadForGroup(int parentId, int groupId)
		{
			var permission = QPContext.EFContext.BackendActionPermissionSet.SingleOrDefault(p => p.ActionId == parentId && p.GroupId == groupId);
			return MapperFacade.BackendActionPermissionMapper.GetBizObject(permission);
		}
		
	}
}
