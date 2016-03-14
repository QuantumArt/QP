using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using System.Data;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
	public class SecurityRepository
	{
	
		/// <summary>
		/// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для текущего пользователя по entity_type_code и action_type_code
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="entityTypeCode"></param>
		/// <param name="entityId"></param>
		/// <param name="actionTypeCode"></param>
		/// <returns></returns>
		internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode)
		{
			using (new QPConnectionScope())
			{
				return Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, entityTypeCode, entityId, actionTypeCode);
			}
		}

		/// <summary>
		/// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для пользователя по entity_type_code, action_type_code, userId
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="entityTypeCode"></param>
		/// <param name="entityId"></param>
		/// <param name="actionTypeCode"></param>
		/// <returns></returns>
		internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode, int userId)
		{
			using (new QPConnectionScope())
			{
				return Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, userId, entityTypeCode, entityId, actionTypeCode);
			}
		}

		/// <summary>
		/// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для группы пользователей по entity_type_code, action_type_code, userGroupId
		/// </summary>
		/// <param name="userGroupId"></param>
		/// <param name="entityTypeCode"></param>
		/// <param name="entityId"></param>
		/// <param name="actionTypeCode"></param>
		/// <returns></returns>
		internal static bool IsEntityAccessibleForUserGroup(string entityTypeCode, int entityId, string actionTypeCode, int userGroupId)
		{
			using (new QPConnectionScope())
			{
				return Common.IsEntityAccessibleForUserGroup(QPConnectionScope.Current.DbConnection, userGroupId, entityTypeCode, entityId, actionTypeCode);
			}
		}

		/// <summary>
		/// Определение доступа к действию для пользователя по action_code
		/// </summary>
		/// <param name="actionCode"></param>
		/// <returns></returns>
		internal static bool IsActionAccessible(string actionCode)
		{
			BackendAction action;
			return IsActionAccessible(actionCode, out action);
		}
		/// <summary>
		/// Определение доступа к действию для пользователя по action_code
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="userId"></param>
		/// <param name="actionCode"></param>
		/// <returns></returns>
		internal static bool IsActionAccessible(string actionCode, out BackendAction action)
		{
			using (var scope = new QPConnectionScope())
			{
				action = BackendActionRepository.GetByCode(actionCode);
				if (action == null)
					throw new ApplicationException(String.Format("Action is not found: {0}", actionCode));

				if (QPContext.IsAdmin)
					return true;
				else
				{					
					BackendActionType actionType = BackendActionTypeRepository.GetById(action.TypeId);
					if (actionType == null)
						throw new ApplicationException(String.Format("Action Type is not found: {0}", action.TypeId));

					int? userPLevel = GetActionPermissionLevelForUser(action, QPContext.CurrentUserId);
					if (userPLevel == null)
						return false;
					else
						return userPLevel >= actionType.RequiredPermissionLevel;
				}
			}
		}

		private static int? GetActionPermissionLevelForUser(BackendAction action, int userId)
		{
			using (var scope = new QPConnectionScope())
			{
				DataRow row = Common.GetActionPermissionsForUser(scope.DbConnection, userId, action.EntityTypeId, action.Id).FirstOrDefault();
				if (row != null && !row.IsNull("PERMISSION_LEVEL"))
					return Converter.ToInt32(row.Field<decimal>("PERMISSION_LEVEL"));
				else
					return null;
			}
		}
	}
}
