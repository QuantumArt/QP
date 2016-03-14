using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
	public interface IActionPermissionChangeService
	{
		EntityPermission ReadOrDefault(int parentId, int? userId, int? groupId);
		EntityPermission ReadOrDefaultForChange(int parentId, int? userId, int? groupId);		
		IPermissionViewModelSettings ViewModelSettings { get; }
		EntityPermission Change(EntityPermission entityPermission);
		MessageResult Remove(int parentId, int? userId, int? groupId);
	}
}
