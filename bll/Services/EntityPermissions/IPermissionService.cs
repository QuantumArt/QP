using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.EntityPermissions;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	/// <summary>
	/// Интерфейс сервисов для EntityPermission
	/// </summary>
	public interface IPermissionService
	{
		/// <summary>
		/// Стратегии, определяющая поведение View-моделей
		/// </summary>
		IPermissionListViewModelSettings ListViewModelSettings { get; }
		IPermissionViewModelSettings ViewModelSettings { get; }

		IPermissionRepository Repository { get; }

		PermissionInitListResult InitList(int parentEntityId);
		ListResult<EntityPermissionListItem> List(int parentId, ListCommand cmd);
		EntityPermission Read(int id);
		EntityPermission ReadForUpdate(int id);
		EntityPermission New(int parentId);
		EntityPermission Save(EntityPermission permission);
		EntityPermission Update(EntityPermission permission);
		MessageResult Remove(int parentId, int id);
		MessageResult MultipleRemove(int parentId, IEnumerable<int> IDs);

		IEnumerable<EntityPermissionLevel> GetPermissionLevels();

		Article GetParentArticle(int parentEntityId);		
	}
}
