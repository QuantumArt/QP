using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.DAL
{
    public class PermissionHelper
    {
        private const string GroupInListPlaceholder = "<@_group_in_list_@>";
        private const string LevelIncrementPlaceholder = "<@_increment_level_@>";

        public static string GetPermittedItemsAsQuery(
            QPModelDataContext context,
            decimal? userIdParam = 0,
            decimal? groupIdParam = 0,
            int? startLevelParam = 2,
            int? endLevelParam = 4,
            string entityNameParam = "content_item",
            string parentEntityNameParam = "",
            decimal? parentEntityIdParam = 0)
        {
            var userId = userIdParam ?? 0;
            var groupId = groupIdParam ?? 0;
            var startLevel = startLevelParam ?? 2;
            var endLevel = endLevelParam ?? 4;
            var entityName = entityNameParam ?? "content_item";
            var parentEntityName = parentEntityNameParam ?? "";
            var parentEntityId = parentEntityIdParam ?? 0;

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(context);
            var isPostgres = dbType == DatabaseType.Postgres;
            var sql = string.Empty;

            const string minPlExpression = "cast(min(pl) as int)%10";
            var entityIdField = $"{entityName}_id";

            var groupBy = $" group by {entityIdField}";
            var whereParentEntity = "";
            var permissionTable = $"{entityName}_access_PermLevel";

            var parentEntityIdField = $"{parentEntityName}_id";

            if (!string.IsNullOrWhiteSpace(parentEntityName) && parentEntityId != 0)
            {
                permissionTable += $"_{parentEntityName}";
                whereParentEntity += $" and {parentEntityIdField} = {parentEntityId}";
            }

            if (isPostgres)
            {
                permissionTable = permissionTable.ToSnakeCase();
            }

            var hide = entityName.Equals("content", StringComparison.InvariantCultureIgnoreCase)
                ? isPostgres ? ", MAX(hide::int) as hide " : ", MAX(CONVERT(int, hide)) as hide "
                : ", 0 as hide ";

            var noLock = isPostgres ? string.Empty : " with(nolock) ";

            var selectUser =
                $@" select
                    {entityIdField},
                    max(permission_level) as pl
                    {hide}
                    from {permissionTable} {noLock}
                    where
                    user_id = {userId}
                    {whereParentEntity}";

            var selectGroup =
                $@" select
                    {entityIdField},
                    max(permission_level) + {LevelIncrementPlaceholder} as pl
                    {hide}
                    from {permissionTable} {noLock}
                    where
                    group_id in ( {GroupInListPlaceholder} )
                    {whereParentEntity}";

            var defaultSql = $" select 0 as {entityIdField}, 0 as permission_level {hide} from {permissionTable}";
            var currentLevelAddition = 0;
            const int intIncrement = 10;
            var childGroups = new List<decimal>();
            if (userId > 0)
            {
                sql = selectUser + groupBy;
                var user = GetUserPropertiesById(context, userId);
                childGroups = user?.Groups?.Select(x => x.Id).Distinct().ToList() ?? new List<decimal>();
                currentLevelAddition += intIncrement;
            }

            if (groupId > 0 && userId <= 0)
            {
                childGroups.Add(groupId);
            }

            if (!childGroups.Any())
            {
                return !string.IsNullOrWhiteSpace(sql) ? sql : defaultSql;
            }

            var groupIds = string.Join(", ", childGroups);
            var unionString = " UNION ALL ";
            if (!string.IsNullOrWhiteSpace(sql))
            {
                sql += unionString;
            }

            sql += selectGroup
                .Replace(LevelIncrementPlaceholder, currentLevelAddition.ToString())
                .Replace(GroupInListPlaceholder, groupIds);

            sql += groupBy;

            var usedGroups = childGroups;
            var parentGroupIds = GetParentGroupIds(context, childGroups);

            while (parentGroupIds.Any())
            {
                currentLevelAddition += intIncrement;
                var newParentGroups = parentGroupIds
                    .Where(x => !childGroups.Contains(x) && !usedGroups.Contains(x))
                    .ToList();
                if (newParentGroups.Any())
                {
                    if (!string.IsNullOrWhiteSpace(sql)) sql += unionString;

                    groupIds = string.Join(", ", newParentGroups);
                    sql += selectGroup
                        .Replace(LevelIncrementPlaceholder, currentLevelAddition.ToString())
                        .Replace(GroupInListPlaceholder, groupIds);
                    sql += groupBy;
                    usedGroups.AddRange(newParentGroups);
                    parentGroupIds = GetParentGroupIds(context, newParentGroups);
                }
            }

            return
                $@"select
                {entityIdField},
                {minPlExpression} as permission_level,
                max(hide) as hide
                from ( {sql} ) as qp_zzz
                group by qp_zzz.{entityIdField}
                HAVING {minPlExpression} >= {startLevel} and {minPlExpression} <= {endLevel}";
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
            var entitySecQuery = GetPermittedItemsAsQuery(context, userIdParam: userId, startLevelParam: 0, endLevelParam: 100, entityNameParam: "entity_type");
            var permissionTable = "entity_type_access_PermLevel";
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
                userIdParam: userId,
                startLevelParam: 0,
                endLevelParam: 100,
                entityNameParam: "BACKEND_ACTION"
                );

            var entitySecQuery = GetEntityPermissionAsQuery(context, userId);

            var query = $@"
select AP.BACKEND_ACTION_ID, COALESCE(AP.PERMISSION_LEVEL, EP.PERMISSION_LEVEL, 0) AS PERMISSION_LEVEL from
		(select L.PERMISSION_LEVEL AS PERMISSION_LEVEL, T.ID AS BACKEND_ACTION_ID, T.ENTITY_TYPE_ID FROM
			({actionSecQuery}) P1
			LEFT JOIN backend_action_access_PermLevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "USER_ID")} = {userId}
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
