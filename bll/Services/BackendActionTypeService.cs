using System;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services
{
	public class BackendActionTypeService
	{
		/// <summary>
		/// Возвращает код типа действия по коду действия
		/// </summary>
		/// <param name="actionCode">код действия</param>
		/// <returns>код типа действия</returns>
		public static string GetCodeByActionCode(string actionCode)
		{
			if (String.IsNullOrWhiteSpace(actionCode))
				return null;
			else
				return BackendActionTypeRepository.GetCodeByActionCode(actionCode);
		}


		public static BackendActionType GetByCode(string actionTypeCode)
		{
			if (String.IsNullOrWhiteSpace(actionTypeCode))
				return null;
			else
				return BackendActionTypeRepository.GetByCode(actionTypeCode);
		}
	}
}
