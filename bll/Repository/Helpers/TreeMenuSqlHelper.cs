using System;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public class TreeMenuSqlHelper
    {
        public static string GetTreeNodeSql(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId, bool countOnly = false)
        {
            parentEntityId = parentEntityId ?? 0;

            var userId = QPContext.CurrentUserId;
            var user = UserRepository.GetById(userId);
            var enableContentGrouping = (entityTypeCode != EntityTypeCode.Content && entityTypeCode != EntityTypeCode.VirtualContent)
                || user.EnableContentGroupingInTree;

            var entityTypes = EntityTypeRepository.GetList();
            var entityType = entityTypes.FirstOrDefault(x => x.Code.Equals(entityTypeCode, StringComparison.InvariantCultureIgnoreCase));
            var parentGroupCode = entityType == null || !enableContentGrouping
                ? null
                : entityTypes.FirstOrDefault(x => x.Id == entityType.GroupParentId)?.Code;

            var realParentId = isGroup ? EntityTypeRepository.GetParentEntityId((decimal)parentEntityId, entityTypeCode) : parentEntityId;

            var currentIsGroup = false;
            string currentGroupItemCode = null;

            var newEntityTypeCode = entityTypeCode;
            var newIsFolder = isFolder;

            if (!string.IsNullOrWhiteSpace(parentGroupCode))
            {
                if (isFolder)
                {
                    currentGroupItemCode = entityTypeCode;
                    newEntityTypeCode = parentGroupCode;
                    currentIsGroup = true;
                }
            }
            else if (!string.IsNullOrWhiteSpace(groupItemCode))
            {
                if (!isFolder)
                {
                    newIsFolder = true;
                    newEntityTypeCode = groupItemCode;
                }
            }

            var newEntityType = entityTypes.FirstOrDefault(x => x.Code.Equals(newEntityTypeCode, StringComparison.InvariantCultureIgnoreCase));
            var entityTypeDal = MapperFacade.EntityTypeMapper.GetDalObject(newEntityType);

            return GetSql(parentEntityId, isGroup, entityId, countOnly, newIsFolder, newEntityTypeCode, entityTypeDal, realParentId,  currentIsGroup, currentGroupItemCode, userId, QPContext.IsAdmin);
        }

        private static string GetSql(int? parentEntityId, bool isGroup, int entityId, bool countOnly, bool isFolder, string newEntityTypeCode, EntityTypeDAL entityType, decimal? realParentId, bool currentIsGroup, string currentGroupItemCode, int userId, bool isAdmin)
        {
            var realParentIdStr = realParentId.HasValue ? realParentId.ToString() : "NULL";
            var iconField = entityType?.IconField ?? "NULL";
            var iconModifierField = entityType?.IconModifierField ?? "NULL";

            var parentIdField = entityType?.ParentIdField;
            string realParentIdField = null;
            if (isGroup)
            {
                realParentIdField = parentIdField;
                parentIdField = entityType?.GroupParentIdField;
            }

            var sqlSb = new StringBuilder();
            var selectSb = new StringBuilder();
            var whereSb = new StringBuilder();
            var orderSb = new StringBuilder();
            var sql = string.Empty;
            var databaseType = QPContext.DatabaseType;
            if (isFolder || !string.IsNullOrWhiteSpace(entityType?.RecurringIdField))
            {
                if (entityType?.HasItemNodes ?? false)
                {
                    var orderColumn = (string.IsNullOrWhiteSpace(entityType.OrderField) ? entityType.TitleField : entityType.OrderField).FixColumnName(databaseType);
                    selectSb.AppendLine($@"
                        {entityType.Source}.{entityType.IdField} AS id,
                        {entityType.TitleField} AS title,
                        {iconField} as icon,
                        {iconModifierField} as icon_modifier,
                        {orderColumn} as sortorder
");
                    whereSb.AppendLine("1 = 1");

                    if (!string.IsNullOrWhiteSpace(parentIdField) && parentEntityId != 0)
                    {
                        whereSb.AppendLine($" AND {parentIdField} = {parentEntityId}");
                    }

                    if (!string.IsNullOrWhiteSpace(entityType.RecurringIdField))
                    {
                        whereSb.AppendLine($" AND {entityType.RecurringIdField} {(isFolder ? " is null" : $" = {parentEntityId}")}");
                    }

                    if (entityId != 0)
                    {
                        whereSb.AppendLine($" AND {entityType.Source}.{entityType.IdField} = {entityId}");
                    }

                    orderSb.AppendLine(orderColumn);
                }

                if (string.IsNullOrWhiteSpace(entityType.SourceSP))
                {
                    if (!string.IsNullOrWhiteSpace(selectSb.ToString()) && !string.IsNullOrWhiteSpace(entityType.Source) && !string.IsNullOrWhiteSpace(whereSb.ToString()))
                    {
                        sqlSb.AppendLine($"select {selectSb} from {entityType.Source} where {whereSb}");
                    }
                }
                else
                {
                    decimal? siteId;
                    switch (entityType.SourceSP)
                    {
                        case "qp_sites_list":
                            sqlSb.AppendLine(GetSitesListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false, userId, isAdmin));
                            break;
                        case "qp_real_content_list":
                            siteId = !string.IsNullOrWhiteSpace(realParentIdField) ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false, siteId, userId, isAdmin));
                            break;
                        case "qp_virtual_content_list":
                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), true, siteId, userId, isAdmin));
                            break;
                        case "qp_site_folder_list":
                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            var parentFolderId = isFolder ? 0 : parentEntityId.Value;
                            sqlSb.AppendLine(GetSiteFolderList(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), siteId, parentFolderId, userId, isAdmin));
                            break;
                    }
                }

                if (countOnly)
                {
                    return string.IsNullOrWhiteSpace(sqlSb.ToString())
                        ? null
                        : $"SELECT COUNT(ID) FROM ({sqlSb}) as innerSql";
                }

                sql = " SELECT\n" +
                    $"{realParentIdStr} as parent_id,\n" +
                    $"{(isGroup ? $"{parentEntityId}" : "NULL")} as parent_group_id,\n" +
                    $"'{newEntityTypeCode}' as code,\n" +
                    $"{ToBoolSql(databaseType, false)} as is_folder,\n" +
                    $"{ToBoolSql(databaseType, currentIsGroup)} as is_group,\n" +
                    $"{(!string.IsNullOrWhiteSpace(currentGroupItemCode) ? $"'{currentGroupItemCode}'" : "NULL")} as group_item_code,\n" +
                    "CASE WHEN i.ICON is not null THEN i.ICON\n" +
                    $"WHEN i.ICON_MODIFIER is not null THEN {ConcatStrValuesSql(databaseType, $"'{newEntityTypeCode}'", CastToString(databaseType, "i.ICON_MODIFIER"), "'.gif'")}\n" +
                    $"ELSE {ConcatStrValuesSql(databaseType, $"'{newEntityTypeCode}'", "'.gif'")} END\n" +
                    "AS icon,\n" +
                    $"{NullableDbValue(databaseType, entityType?.DefaultActionId)} AS default_action_id,\n" +
                    $"{NullableDbValue(databaseType, entityType?.ContextMenuId)} as context_menu_id,\n" +
                    $"{ToBoolSql(databaseType, string.IsNullOrWhiteSpace(entityType?.RecurringIdField))} as is_recurring,\n" +
                    "i.id,\n" +
                    "i.title,\n" +
                    "i.sortorder\n" +
                    $"FROM ( {sqlSb} ) as i\n";
            }

            else
            {
                var condition = $" et.disabled = {ToBoolSql(databaseType, false)}";
                condition += string.IsNullOrWhiteSpace(newEntityTypeCode)
                    ? " and et.parent_id is null "
                    : $" and et.parent_id = {entityType.Id} ";

                var useSecurity = !isAdmin;

                string securitySql = null;
                if (useSecurity)
                {
                    condition += " and s.permission_level > 0 ";
                    securitySql = PermissionHelper.GetEntityPermissionAsQuery(userId);
                }

                if (entityId != 0)
                {
                    condition += $" and et.id = {entityId}";
                }

                if (countOnly)
                {
                    sql = "select count(et.id) ";
                }
                else
                {
                    var isFolderQuery = !string.IsNullOrWhiteSpace(newEntityTypeCode);

                    sql = $@"select et.id as id,
                        {NullableDbValue(databaseType, parentEntityId)} as parent_id,
                        et.name as title,
                        et.code as code,
                        {ToBoolSql(databaseType, isFolderQuery)} as is_folder,
                        {ToBoolSql(databaseType, false)} as is_group,
                        {ConcatStrValuesSql(databaseType, "et.code", "'.gif'")} as icon,
                        et.{(isFolderQuery ? "folder_" : string.Empty)}default_action_id as default_action_id,
                        et.{(isFolderQuery ? "folder_" : string.Empty)}context_menu_id as context_menu_id,
                        NULL as parent_group_id,
                        NULL as group_item_code,
                        {ToBoolSql(databaseType, false)} as is_recurring,
                        et.""order"" as sortorder
                        ";
                }

                sql += $" from entity_type et {(useSecurity ? $"inner join ( {securitySql} ) s on s.entity_type_id = et.id and (s.hide = 0 or s.hide is null)" : string.Empty)}  where {condition}";
                if (countOnly) return sql;
            }

            return $@" select
                cast(treenode.id as bigint) as id,
                treenode.code,
                cast(treenode.parent_id as bigint) as parent_id,
                cast(treenode.parent_group_id as bigint) as parent_group_id,
                treenode.is_folder,
                treenode.is_group,
                treenode.is_recurring,
                treenode.group_item_code,
                treenode.icon,
                treenode.title,
                backend_action.code as default_action_code,
                action_type.code as default_action_type_code,
                context_menu.code as context_menu_code,
                {ToBoolSql(databaseType, false)} as has_children

                from ( {sql} ) as treenode
                left outer join backend_action on treenode.default_action_id = backend_action.id
                left outer join action_type on backend_action.type_id = action_type.id
                left outer join context_menu on treenode.context_menu_id = context_menu.id
                order by treenode.sortorder
";
        }

        private static string CastToString(DatabaseType databaseType, string columnName)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"CAST({columnName} as nvarchar)";
                case DatabaseType.Postgres:
                    return $"{columnName}::varchar";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        private static string NullableDbValue(DatabaseType databaseType, int? value)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return value.HasValue ? value.ToString() : "NULL";
                case DatabaseType.Postgres:
                    return value.HasValue ? value.ToString() : "NULL::numeric";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        private static string ConcatStrValuesSql(DatabaseType databaseType, params string[] p)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return string.Join(" + ", p);
                case DatabaseType.Postgres:
                    return string.Join(" || ", p);
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        private static string ToBoolSql(DatabaseType databaseType, bool boolValue)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return boolValue ? "cast(1 as bit)" : "cast(0 as bit)";
                case DatabaseType.Postgres:
                    return boolValue ? "TRUE" : "FALSE";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        private static string GetSiteFolderList(string select, string filter, string orderBy, decimal? siteId, int parentFolderId, int userId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "folder_id";
            }

            if (string.IsNullOrWhiteSpace(select))
            {
                select = "folder.*, u.login";
            }

            if (!select.Contains("as sortorder"))
            {
                select += $", {orderBy} as sortorder";
            }

            var useSecurity = !isAdmin;
            var securitySql = string.Empty;
            var parentEntityId = parentFolderId != 0 ? parentFolderId : siteId;
            var parentEntityName = parentFolderId != 0 ? "parent_folder" : "site";
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(userIdParam: userId, startLevelParam: 1, entityNameParam: "folder", parentEntityNameParam: parentEntityName, parentEntityIdParam: parentEntityId);
            }

            var sql = $@"
                select {select} from folder inner join users as u on folder.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on folder.folder_id = pi.folder_id ")}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" and {filter}")}
                ";
            return sql;
        }

        private static string GetContentListSql(string select, string filter, string orderBy, bool isVirtual, decimal? siteId, int userId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "content_id";
            }

            if (string.IsNullOrWhiteSpace(select))
            {
                select = "content.*, u.login";
            }

            if (!select.Contains("as sortorder"))
            {
                select += $", {orderBy} as sortorder";
            }

            filter += $" and virtual_type {(isVirtual ? "<> 0" : "=0")}";

            var useSecurity = !isAdmin;
            var securitySql = string.Empty;
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(userIdParam: userId, startLevelParam: 1, entityNameParam: "content", parentEntityNameParam: "site", parentEntityIdParam: siteId);
            }

            var sql = $@"
                select {select}
                from content inner join users as u on content.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on content.content_id = pi.content_id and pi.hide = 0 ")}
                where content.site_id = {siteId}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $"and {filter}")}
                ";
            return sql;
        }

        private static string GetSitesListSql(string select, string filter, string orderBy, bool siteIdOnly, int userId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "site_id";
            }

            if (string.IsNullOrWhiteSpace(select))
            {
                select = "site.*, u.login";
            }

            if (!select.Contains("as sortorder"))
            {
                select += $", {orderBy} as sortorder";
            }



            var useSecurity = !isAdmin;
            var securitySql = string.Empty;
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(userIdParam: userId, startLevelParam: 1, entityNameParam: "site");
            }

            var sql = $@"
                select {(siteIdOnly ? "site.site_id" : select)}
                from site
                inner join users as u on site.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on site.site_id = pi.site_id ")}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $"and {filter}")}
                ";
            return sql;
        }
    }
}
