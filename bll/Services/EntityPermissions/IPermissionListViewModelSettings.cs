using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	/// <summary>
	/// Определяет интерфейс стратегии для настроект ListViewModel списка сущностей
	/// </summary>
	public interface IPermissionListViewModelSettings
	{
		string EntityTypeCode { get; }
		string ActionCode { get; }
		string AddNewItemActionCode { get; }
		bool IsPropagateable { get; }
		bool CanHide { get; }
		string PermissionEntityTypeCode { get; }
		string ContextMenuCode { get; }
		string ParentPermissionsListAction { get; }
		string ActionCodeForLink { get; set; }
	}

	internal class GenericPermissionListViewModelSettings : IPermissionListViewModelSettings
	{
		#region IPermissionListViewModelSettings Members

		public string EntityTypeCode { get; set; }

		public string ActionCode { get; set; }

		public string AddNewItemActionCode { get; set; }

		public bool IsPropagateable { get; set; }

		public bool CanHide { get; set; }

		public string PermissionEntityTypeCode { get; set; }

		public string ContextMenuCode { get; set; }

		public string ParentPermissionsListAction { get; set; }

		public string ActionCodeForLink { get; set; }
		
		#endregion
	}
}
