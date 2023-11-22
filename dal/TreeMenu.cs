using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Info;

namespace Quantumart.QP8.DAL
{
    public static class TreeMenu
    {
        public static IEnumerable<DataRow> GetTreeChildNodes(QPModelDataContext context, DbConnection connection, string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId, int userId, bool isAdmin, string customerCode, bool enableContentGrouping)
        {
            var query = GetSqlQuery(context, connection, entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId, userId, isAdmin, customerCode, enableContentGrouping, false);

            return string.IsNullOrWhiteSpace(query) ? Enumerable.Empty<DataRow>() : Common.GetDataRows(connection, query);
        }

        public static long GetTreeChildNodesCount(QPModelDataContext context, DbConnection connection, string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId, int userId, bool isAdmin, string customerCode, bool enableContentGrouping)
        {
            var query = GetSqlQuery(context, connection, entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId, userId, isAdmin, customerCode, enableContentGrouping, true);
            return string.IsNullOrWhiteSpace(query) ? 0 : Common.ExecuteScalarLong(connection, query);
        }

        private static string[] GetDisabledCodes(QPModelDataContext context, string code, int parentId)
        {
            if (code == EntityTypeCode.Site && parentId != 0 && (context.SiteSet.SingleOrDefault(n => n.Id == parentId)?.ExternalDevelopment ?? false))
            {
                return new[] { EntityTypeCode.PageTemplate };
            }

            return Array.Empty<string>();
        }


        private static string GetSqlQuery(QPModelDataContext context, DbConnection connection, string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId, int userId, bool isAdmin, string customerCode, bool enableContentGrouping, bool countOnly = false)
        {


            var entityTypes = EntityTypeCache.GetEntityTypes(context, customerCode, userId);
            var entityType = entityTypes.FirstOrDefault(x => x.Code.Equals(entityTypeCode, StringComparison.InvariantCultureIgnoreCase));

            var parentGroupCode = entityType == null || !enableContentGrouping
                ? null
                : entityTypes.FirstOrDefault(x => x.Id == entityType.GroupParentId)?.Code;



            var realParentId = isGroup ? GetParentEntityId(context, connection, (decimal)parentEntityId, entityTypeCode, customerCode, userId) : parentEntityId;

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

            var newEntityType =  entityTypes.FirstOrDefault(x => x.Code.Equals(newEntityTypeCode, StringComparison.InvariantCultureIgnoreCase));

            var realParentIdStr = realParentId.HasValue ? realParentId.ToString() : "NULL";
            var iconField = newEntityType?.IconField ?? "NULL";
            var iconModifierField = newEntityType?.IconModifierField ?? "NULL";

            var parentIdField = newEntityType?.ParentIdField;
            string realParentIdField = null;
            if (isGroup)
            {
                realParentIdField = parentIdField;
                parentIdField = newEntityType?.GroupParentIdField;
            }

            var sqlSb = new StringBuilder();
            var selectSb = new StringBuilder();
            var whereSb = new StringBuilder();
            var orderSb = new StringBuilder();
            string sql;
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(context);

            var useSecurity = UseSecurity(isAdmin, databaseType);
            if (newIsFolder || !string.IsNullOrWhiteSpace(newEntityType?.RecurringIdField))
            {
                if (newEntityType?.HasItemNodes ?? false)
                {
                    var orderColumn = (string.IsNullOrWhiteSpace(newEntityType.OrderField) ? newEntityType.TitleField : newEntityType.OrderField).FixColumnName(databaseType);
                    selectSb.AppendLine($@"
                        {newEntityType.Source}.{newEntityType.IdField} AS id,
                        {newEntityType.TitleField} AS title,
                        {iconField} as icon,
                        {iconModifierField} as icon_modifier,
                        {orderColumn} as sortorder
");
                    whereSb.AppendLine("1 = 1");

                    if (!string.IsNullOrWhiteSpace(parentIdField) && parentEntityId != 0)
                    {
                        whereSb.AppendLine($" AND {parentIdField} = {parentEntityId}");
                    }

                    if (!string.IsNullOrWhiteSpace(newEntityType.RecurringIdField))
                    {
                        whereSb.AppendLine($" AND {newEntityType.RecurringIdField} {(newIsFolder ? " is null" : $" = {parentEntityId}")}");
                    }

                    if (entityId != 0)
                    {
                        whereSb.AppendLine($" AND {newEntityType.Source}.{newEntityType.IdField} = {entityId}");
                    }

                    orderSb.AppendLine(orderColumn);
                }

                if (string.IsNullOrWhiteSpace(newEntityType.SourceSP))
                {
                    if (!string.IsNullOrWhiteSpace(selectSb.ToString()) && !string.IsNullOrWhiteSpace(newEntityType.Source) && !string.IsNullOrWhiteSpace(whereSb.ToString()))
                    {
                        sqlSb.AppendLine($"select {selectSb} from {newEntityType.Source} where {whereSb}");
                    }
                }
                else
                {
                    decimal? siteId;
                    switch (newEntityType.SourceSP)
                    {
                        case "qp_sites_list":
                            sqlSb.AppendLine(GetSitesListSql(context, selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false, userId, useSecurity));
                            break;
                        case "qp_real_content_list":
                            siteId = !string.IsNullOrWhiteSpace(realParentIdField) ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(context, selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false, siteId, userId, useSecurity));
                            break;
                        case "qp_virtual_content_list":
                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(context, selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), true, siteId, userId, useSecurity));
                            break;
                        case "qp_site_folder_list":
                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            var parentFolderId = newIsFolder ? 0 : parentEntityId.Value;
                            sqlSb.AppendLine(GetSiteFolderList(context, selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), siteId, parentFolderId, userId, useSecurity));
                            break;
                    }
                }

                if (countOnly)
                {
                    return string.IsNullOrWhiteSpace(sqlSb.ToString())
                        ? null
                        : $"SELECT COUNT(ID) FROM ({sqlSb}) as innerSql";
                }

                if (string.IsNullOrWhiteSpace(sqlSb.ToString()))
                {
                    return null;
                }
                sql = " SELECT\n" +
                    $"{realParentIdStr} as parent_id,\n" +
                    $"{(isGroup ? $"{parentEntityId}" : "NULL")} as parent_group_id,\n" +
                    $"'{newEntityTypeCode}' as code,\n" +
                    $"{SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)} as is_folder,\n" +
                    $"{SqlQuerySyntaxHelper.ToBoolSql(databaseType, currentIsGroup)} as is_group,\n" +
                    $"{(!string.IsNullOrWhiteSpace(currentGroupItemCode) ? $"'{currentGroupItemCode}'" : "NULL")} as group_item_code,\n" +
                    "CASE WHEN i.ICON is not null THEN i.ICON\n" +
                    $"WHEN i.ICON_MODIFIER is not null THEN {SqlQuerySyntaxHelper.ConcatStrValues(databaseType, $"'{newEntityTypeCode}'", SqlQuerySyntaxHelper.CastToString(databaseType, "i.ICON_MODIFIER"), "'.gif'")}\n" +
                    $"ELSE {SqlQuerySyntaxHelper.ConcatStrValues(databaseType, $"'{newEntityTypeCode}'", "'.gif'")} END\n" +
                    "AS icon,\n" +
                    $"{SqlQuerySyntaxHelper.NullableDbValue(databaseType, newEntityType?.DefaultActionId)} AS default_action_id,\n" +
                    $"{SqlQuerySyntaxHelper.NullableDbValue(databaseType, newEntityType?.ContextMenuId)} as context_menu_id,\n" +
                    $"{SqlQuerySyntaxHelper.ToBoolSql(databaseType, !string.IsNullOrWhiteSpace(newEntityType?.RecurringIdField))} as is_recurring,\n" +
                    "i.id,\n" +
                    "i.title,\n" +
                    "i.sortorder\n" +
                    $"FROM ( {sqlSb} ) as i\n";
            }

            else
            {
                var codes = GetDisabledCodes(context, newEntityTypeCode, parentEntityId ?? 0);
                var condition = $" et.disabled = {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)}";
                if (codes.Any())
                {
                    condition += $" and et.code not in ({string.Join(",", codes.Select(n => "'" + n + "'"))})";
                }
                condition += string.IsNullOrWhiteSpace(newEntityTypeCode)
                    ? " and et.parent_id is null "
                    : $" and et.parent_id = {newEntityType.Id} ";




                string securitySql = null;
                if (useSecurity)
                {
                    condition += " and s.permission_level > 0 ";
                    securitySql = PermissionHelper.GetEntityPermissionAsQuery(context, userId);
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
                    var plural = isFolderQuery ? "s" : "";
                    var icon = SqlQuerySyntaxHelper.ConcatStrValues(databaseType, "et.code", $"'{plural}.gif'");

                    sql = $@"select et.id as id,
                        {SqlQuerySyntaxHelper.NullableDbValue(databaseType, parentEntityId)} as parent_id,
                        et.name as title,
                        et.code as code,
                        {SqlQuerySyntaxHelper.ToBoolSql(databaseType, isFolderQuery)} as is_folder,
                        {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)} as is_group,
                        {icon} as icon,
                        et.{(isFolderQuery ? "folder_" : string.Empty)}default_action_id as default_action_id,
                        et.{(isFolderQuery ? "folder_" : string.Empty)}context_menu_id as context_menu_id,
                        NULL as parent_group_id,
                        NULL as group_item_code,
                        {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)} as is_recurring,
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
                {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)} as has_children

                from ( {sql} ) as treenode
                left outer join backend_action on treenode.default_action_id = backend_action.id
                left outer join action_type on backend_action.type_id = action_type.id
                left outer join context_menu on treenode.context_menu_id = context_menu.id
                order by treenode.sortorder
";
        }

        private static bool UseSecurity(bool isAdmin, DatabaseType databaseType) => !isAdmin ;//&& databaseType != DatabaseType.Postgres;

        private static string GetContentListSql(QPModelDataContext context, string select, string filter, string orderBy, bool isVirtual, decimal? siteId, int userId, bool useSecurity)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "content_id";
            }

            if (string.IsNullOrWhiteSpace(@select))
            {
                @select = "content.*, u.login";
            }

            if (!@select.Contains("as sortorder"))
            {
                @select += $", {orderBy} as sortorder";
            }

            filter += $" and virtual_type {(isVirtual ? "<> 0" : "=0")}";


            var securitySql = string.Empty;
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(context, userId: userId, startLevel: 1, entityTypeName: "content", parentEntityTypeName: "site", parentEntityId: siteId.Value);
            }

            var sql = $@"
                select {@select}
                from content inner join users as u on content.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on content.content_id = pi.content_id and pi.hide = 0 ")}
                where content.site_id = {siteId}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $"and {filter}")}
                ";
            return sql;
        }

        private static string GetSitesListSql(QPModelDataContext context, string select, string filter, string orderBy, bool siteIdOnly, int userId, bool useSecurity)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "site_id";
            }

            if (string.IsNullOrWhiteSpace(@select))
            {
                @select = "site.*, u.login";
            }

            if (!@select.Contains("as sortorder"))
            {
                @select += $", {orderBy} as sortorder";
            }




            var securitySql = string.Empty;
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(context, userId: userId, startLevel: 1, entityTypeName: "site");
            }

            var sql = $@"
                select {(siteIdOnly ? "site.site_id" : @select)}
                from site
                inner join users as u on site.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on site.site_id = pi.site_id ")}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $"and {filter}")}
                ";
            return sql;
        }

        private static string GetSiteFolderList(QPModelDataContext context, string select, string filter, string orderBy, decimal? siteId, int parentFolderId, int userId, bool useSecurity)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "folder_id";
            }

            if (string.IsNullOrWhiteSpace(@select))
            {
                @select = "folder.*, u.login";
            }

            if (!@select.Contains("as sortorder"))
            {
                @select += $", {orderBy} as sortorder";
            }

            var securitySql = string.Empty;
            var parentEntityId = parentFolderId != 0 ? parentFolderId : siteId.Value;
            var parentEntityName = parentFolderId != 0 ? "parent_folder" : "site";
            if (useSecurity)
            {
                securitySql = PermissionHelper.GetPermittedItemsAsQuery(context, userId: userId, startLevel: 1, entityTypeName: "folder", parentEntityTypeName: parentEntityName, parentEntityId: parentEntityId);
            }

            var sql = $@"
                select {@select} from folder inner join users as u on folder.last_modified_by = u.user_id
                {(!useSecurity ? string.Empty : $"inner join ( {securitySql} ) as pi on folder.folder_id = pi.folder_id ")}
                {(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" and {filter}")}
                ";
            return sql;
        }


        private static decimal? GetParentEntityId(QPModelDataContext context, DbConnection connection, decimal entityId, string entityTypeCode, string customerCode, int userId)
        {
            var entity = EntityTypeCache.GetEntityTypes(context, customerCode, userId)
                .FirstOrDefault(x => x.Code.Equals(entityTypeCode, StringComparison.InvariantCultureIgnoreCase));
            if (entity?.Source == null || entity.IdField == null)
            {
                return null;
            }

            return Common.GetNumericFieldValue(connection, entity.ParentIdField, entity.Source, entity.IdField, entityId);

        }


    }
}
