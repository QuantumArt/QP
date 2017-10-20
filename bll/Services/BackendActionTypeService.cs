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
		    if (string.IsNullOrWhiteSpace(actionCode))
			{
			    return null;
			}

		    return BackendActionTypeRepository.GetCodeByActionCode(actionCode);
		}


		public static BackendActionType GetByCode(string actionTypeCode)
		{
		    if (string.IsNullOrWhiteSpace(actionTypeCode))
			{
			    return null;
			}

		    return BackendActionTypeRepository.GetByCode(actionTypeCode);
		}
	}
}
