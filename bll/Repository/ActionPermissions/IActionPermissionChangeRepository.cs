using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Repository.ActionPermissions
{
	public interface IActionPermissionChangeRepository
	{
		EntityPermission ReadForUser(int parentId, int p);

		EntityPermission ReadForGroup(int parentId, int p);
	}
}
