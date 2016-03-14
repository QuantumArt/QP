using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	/// <summary>
	/// Определяет интерфейс стратегии для настроект ListViewModel списка сущностей
	/// </summary>
	public interface IPermissionViewModelSettings
	{
		string EntityTypeCode { get; }
		string ActionCode { get; }
		bool IsPropagateable { get; }
		bool CanHide { get; set; }
	}

	internal class GenericPermissionViewModelSettings : IPermissionViewModelSettings
	{
		#region IPermissionListViewModelSettings Members

		public string EntityTypeCode { get; set; }

		public string ActionCode { get; set; }

		public bool IsPropagateable { get; set; }

		public bool CanHide { get; set; }

		#endregion
	}
}
