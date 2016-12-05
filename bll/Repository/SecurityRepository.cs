using System;
using System.Data;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    public class SecurityRepository
    {
        /// <summary>
        /// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для текущего пользователя по entity_type_code и action_type_code
        /// </summary>
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
        internal static bool IsActionAccessible(string actionCode, out BackendAction action)
        {
            using (new QPConnectionScope())
            {
                action = BackendActionRepository.GetByCode(actionCode);
                if (action == null)
                {
                    throw new ApplicationException($"Action is not found: {actionCode}");
                }

                if (QPContext.IsAdmin)
                {
                    return true;
                }

                var actionType = BackendActionTypeRepository.GetById(action.TypeId);
                if (actionType == null)
                {
                    throw new ApplicationException($"Action Type is not found: {action.TypeId}");
                }

                var userPLevel = GetActionPermissionLevelForUser(action, QPContext.CurrentUserId);
                if (userPLevel == null)
                {
                    return false;
                }

                return userPLevel >= actionType.RequiredPermissionLevel;
            }
        }

        private static int? GetActionPermissionLevelForUser(BackendAction action, int userId)
        {
            using (var scope = new QPConnectionScope())
            {
                var row = Common.GetActionPermissionsForUser(scope.DbConnection, userId, action.EntityTypeId, action.Id).FirstOrDefault();
                if (row != null && !row.IsNull("PERMISSION_LEVEL"))
                {
                    return Converter.ToInt32(row.Field<decimal>("PERMISSION_LEVEL"));
                }

                return null;
            }
        }
    }
}
