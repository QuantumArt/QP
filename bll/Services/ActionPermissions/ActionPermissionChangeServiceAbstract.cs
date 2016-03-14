using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.BLL.Repository.ActionPermissions;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
	public abstract class ActionPermissionChangeServiceAbstract : IActionPermissionChangeService
	{		
		public abstract IPermissionRepository PermissionRepository { get; }
		public abstract IActionPermissionChangeRepository ChangeRepository { get; }

		#region IActionPermissionChangeService Members

		public EntityPermission ReadOrDefault(int parentId, int? userId, int? groupId)
		{
			return InnerReadOrDefault(parentId, userId, groupId);
		}

		public EntityPermission ReadOrDefaultForChange(int parentId, int? userId, int? groupId)
		{
			return InnerReadOrDefault(parentId, userId, groupId);
		}

		public abstract IPermissionViewModelSettings ViewModelSettings { get; }		

		public EntityPermission Change(EntityPermission entityPermission)
		{
			if (entityPermission.IsNew)
				return PermissionRepository.Save(entityPermission);
			else
				return PermissionRepository.Update(entityPermission);
		}

		public MessageResult Remove(int parentId, int? userId, int? groupId)
		{
			EntityPermission permission = Read(parentId, userId, groupId);
			if (permission != null)
			{
				PermissionRepository.Remove(permission.Id);
			}
			return null;
		}

		#endregion

		private EntityPermission InnerReadOrDefault(int parentId, int? userId, int? groupId)
		{
			EntityPermission permission = Read(parentId, userId, groupId);
			if (permission == null)
			{
				permission = EntityPermission.Create(parentId, PermissionRepository);				
				permission.GroupId = groupId;				
				permission.UserId = userId;
				
			}					
			return permission;
		}

		private EntityPermission Read(int parentId, int? userId, int? groupId)
		{
			EntityPermission permission = null;
			if (!userId.HasValue && !groupId.HasValue)
				return null;
			else if (userId.HasValue)
				permission = ChangeRepository.ReadForUser(parentId, userId.Value);
			else if (groupId.HasValue)
				permission = ChangeRepository.ReadForGroup(parentId, groupId.Value);
			return permission;
		}
	}
}
