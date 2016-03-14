using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	public interface IChildEntityPermissionRepository
	{
		IEnumerable<ChildEntityPermissionListItem> List(int siteId, int? groupId, int? userId, ListCommand cmd, out int totalRecords);

		ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId);

		void MultipleChange(int parentId, IEnumerable<int> entityIDs, ChildEntityPermission permissionSettings);

		void ChangeAll(int parentId, ChildEntityPermission permissionSettings);

		void MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId);

		void RemoveAll(int parentId, int? userId, int? groupId);

		EntityPermission GetParentPermission(int parentId, int? userId, int? groupId);
	}
}
