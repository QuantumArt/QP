using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.DAL
{
    public class PermissionHelper
    {
        public static string GetPermittedItemsAsQuery(
            QPModelDataContext context,
            decimal userId = 0,
            decimal groupId = 0,
            int startLevel = 2,
            int endLevel = 4,
            string entityTypeName = "content_item",
            string parentEntityTypeName = "",
            decimal parentEntityId = 0)
        {

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(context);
            var isPostgres = dbType == DatabaseType.Postgres;
            var level = 0;

            var entityIdField = $"{entityTypeName}_id";
            var parentEntityIdField = $"{parentEntityTypeName}_id";

            var permissionTable = $"{entityTypeName}_access_permlevel";
            var whereParentEntity = "";

            if (!string.IsNullOrWhiteSpace(parentEntityTypeName) && parentEntityId != 0)
            {
                permissionTable += $"_{parentEntityTypeName}";
                whereParentEntity += $" and {parentEntityIdField} = {parentEntityId}";
            }

            if (isPostgres)
            {
                permissionTable = permissionTable.ToSnakeCase();
            }

            var hide = entityTypeName.Equals("content", StringComparison.InvariantCultureIgnoreCase)
                ? isPostgres ? "MIN(hide::int) as hide " : "MIN(CONVERT(int, hide)) as hide "
                : "0 as hide ";

            var hint = isPostgres ? string.Empty : " with(nolock) ";

            var selectUser =
                $@" select {entityIdField} as id, max(permission_level) as pl, {hide}, 0 as level
                    from {permissionTable} {hint}
                    where user_id = {userId} {whereParentEntity}
                    group by {entityIdField}
                ";

            var selectGroup =
                $@" select {entityIdField} as id, max(permission_level) as pl, {hide}, {{0}} as level
                    from {permissionTable} {hint}
                    where group_id in ({{1}}) {whereParentEntity}
                    group by {entityIdField}
                ";

            var defaultSql = $" select {entityIdField}, 0 as permission_level, 0 as hide from {entityTypeName} where 1 = 1 {whereParentEntity}";
            var sbSql = new StringBuilder();
            var groupsToProcess = new List<decimal>();
            var usedGroups = new List<decimal>();

            if (userId > 0)
            {
                sbSql.Append(selectUser);
                var user = GetUserPropertiesById(context, userId);
                groupsToProcess = user?.Groups?.Select(x => x.Id).Distinct().ToList() ?? new List<decimal>();
            }
            else if (groupId > 0)
            {
                groupsToProcess.Add(groupId);
            }

            while (groupsToProcess.Any())
            {
                level += 1;
                if (level > 1 || sbSql.Length > 0)
                {
                    sbSql.Append(" UNION ALL ");
                }
                sbSql.AppendFormat(selectGroup, level, string.Join(", ", groupsToProcess));
                usedGroups.AddRange(groupsToProcess);
                var parentGroupIds = GetParentGroupIds(context, groupsToProcess);
                groupsToProcess = parentGroupIds
                    .Where(x => !groupsToProcess.Contains(x) && !usedGroups.Contains(x))
                    .ToList();
            }

            if (sbSql.Length == 0)
            {
                return defaultSql;
            }

            return
                $@"select id as {entityIdField}, pl as permission_level, hide
                from (
                    select id, pl, hide, ROW_NUMBER() OVER(PARTITION BY id ORDER BY level) as num from (
                        {sbSql}
                    ) as united_permissions
                ) as priority_permissions where priority_permissions.num = 1
                and pl between {startLevel} and {endLevel}";
        }

        private static UserDAL GetUserPropertiesById(QPModelDataContext context, decimal userId)
        {
            return context.UserSet
                .Include(x => x.UserGroupBinds).ThenInclude(y => y.UserGroup)
                .SingleOrDefault(u => u.Id == userId);
        }

        public static string GetEntityPermissionAsQuery(QPModelDataContext context, decimal userId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(context);
            var isPostgres = dbType == DatabaseType.Postgres;
            var entitySecQuery = GetPermittedItemsAsQuery(context, userId, startLevel: 0, endLevel: 100, entityTypeName: "entity_type");
            var permissionTable = "entity_type_access_permlevel";
            if (isPostgres)
            {
                permissionTable = permissionTable.ToSnakeCase();
            }

            return $@"
            select COALESCE(L.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL, T.ID AS ENTITY_TYPE_ID, HIDE FROM
			({entitySecQuery}) P1
			LEFT JOIN {permissionTable} P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.USER_ID = {userId}
			RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
			LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL";
        }

        public static string GetActionPermissionsAsQuery(QPModelDataContext context, decimal userId)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(context);
            var actionSecQuery = GetPermittedItemsAsQuery(context,
                userId,
                startLevel: 0,
                endLevel: 100,
                entityTypeName: "BACKEND_ACTION"
                );

            var entitySecQuery = GetEntityPermissionAsQuery(context, userId);

            var query = $@"
select AP.BACKEND_ACTION_ID, COALESCE(AP.PERMISSION_LEVEL, EP.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL from
		(select L.PERMISSION_LEVEL AS PERMISSION_LEVEL, T.ID AS BACKEND_ACTION_ID, T.ENTITY_TYPE_ID FROM
			({actionSecQuery}) P1
			LEFT JOIN backend_action_access_permlevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "USER_ID")} = {userId}
			RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
			LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
		) AP
		JOIN
        ({entitySecQuery}) EP ON AP.ENTITY_TYPE_ID = EP.ENTITY_TYPE_ID

";
            return query;
        }

        private static List<decimal> GetParentGroupIds(QPModelDataContext context, ICollection<decimal> childGroups)
        {
            return context
                .UserGroupSet
                .Where(x => childGroups.Contains(x.Id))
                .Include(x => x.ParentGroupToGroupBinds)
                .SelectMany(x => x.ParentGroupToGroupBinds)
                .Select(x => x.ParentGroupId)
                .Distinct()
                .ToList();
        }
    }
}
