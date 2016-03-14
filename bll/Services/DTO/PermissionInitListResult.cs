using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.EntityPermissions;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class PermissionInitListResult : InitListResultBase
	{
		public bool IsEnableArticlesPermissionsAccessable { get; set; }
	}
}
