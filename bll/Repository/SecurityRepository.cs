using System;
using System.Data;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    public class SecurityRepository
    {
        internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode)
        {
            using (new QPConnectionScope())
            {
                return Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, QPContext.CurrentUserId, entityTypeCode, entityId, actionTypeCode);
            }
        }

        internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode, int userId)
        {
            using (new QPConnectionScope())
            {
                return Common.IsEntityAccessible(QPConnectionScope.Current.DbConnection, userId, entityTypeCode, entityId, actionTypeCode);
            }
        }

        internal static bool IsEntityAccessibleForUserGroup(string entityTypeCode, int entityId, string actionTypeCode, int userGroupId)
        {
            using (new QPConnectionScope())
            {
                return Common.IsEntityAccessibleForUserGroup(QPConnectionScope.Current.DbConnection, userGroupId, entityTypeCode, entityId, actionTypeCode);
            }
        }

        internal static bool IsActionAccessible(string actionCode) => IsActionAccessible(actionCode, out BackendAction _);

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
                var row = Common.GetActionPermissionsForUser(QPContext.EFContext, scope.DbConnection, userId, action.EntityTypeId, action.Id).FirstOrDefault();
                if (row != null && !row.IsNull("PERMISSION_LEVEL"))
                {
                    return Converter.ToInt32(row.Field<decimal>("PERMISSION_LEVEL"));
                }

                return null;
            }
        }
    }
}
