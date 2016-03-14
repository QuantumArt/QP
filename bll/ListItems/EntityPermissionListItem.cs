using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.ListItems
{
	/// <summary>
	/// Элемент списка доступов к сущности
	/// </summary>
	public class EntityPermissionListItem
	{		
		public int Id { get; set; }
		public string UserLogin { get; set; }
		public string GroupName { get; set; }
		public string LevelName { get; set; }
		public bool PropagateToItems { get; set; }

		public bool Hide { get; set; }

		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
		public int LastModifiedByUserId { get; set; }
		public string LastModifiedByUser { get; set; }
	}
}
