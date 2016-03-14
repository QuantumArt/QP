using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Уровень доступа к сущности
	/// </summary>
	public class EntityPermissionLevel
	{
		private static string _list = "List";
		private static string _fullAccess = "Full Access";

		public int Id { get; set; }
		public int Level { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public static EntityPermissionLevel GetList()
		{
			return GetEntityPermissionLevelByName(_list);
		}

		public static EntityPermissionLevel GetFullAccess()
		{
			return GetEntityPermissionLevelByName(_fullAccess);
		}

		private static EntityPermissionLevel GetEntityPermissionLevelByName(string name)
		{
			return PageTemplateRepository.GetPermissionLevelByName(name);
		}
	}
}
