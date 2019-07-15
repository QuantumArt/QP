using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    public class SecurityRepository
    {
        internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode)
        {
            return IsEntityAccessible(entityTypeCode, entityId, actionTypeCode, QPContext.CurrentUserId);
        }

        internal static int GetRequiredPermissionLevel(string actionTypeCode)
        {
            return BackendActionTypeRepository.GetList()
                .SingleOrDefault(n => n.Code == actionTypeCode)?.RequiredPermissionLevel ?? PermissionLevel.Deny;
        }


        internal static bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode, int userId)
        {
            using (new QPConnectionScope())
            {
                var requiredPermissionLevel = GetRequiredPermissionLevel(actionTypeCode);
                var actualLevel = CommonSecurity.GetEntityAccessLevel(QPConnectionScope.Current.DbConnection, QPContext.EFContext, userId, 0, entityTypeCode, entityId);
                return actualLevel >= requiredPermissionLevel;
            }
        }

        internal static bool IsEntityAccessibleForUserGroup(string entityTypeCode, int entityId, string actionTypeCode, int userGroupId)
        {
            using (new QPConnectionScope())
            {
                var requiredPermissionLevel = GetRequiredPermissionLevel(actionTypeCode);
                var actualLevel = CommonSecurity.GetEntityAccessLevel(QPConnectionScope.Current.DbConnection, QPContext.EFContext, 0, userGroupId, entityTypeCode, entityId);
                return actualLevel >= requiredPermissionLevel;
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
