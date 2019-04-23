using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal class TreeMenuRepository
    {
        /// <summary>
        /// Возвращает узел дерева
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="isFolder">признак является ли узел директорией</param>
        /// <param name="loadChildNodes">признак, разрешающий предварительную загрузку первого уровня дочерних узлов</param>
        /// <returns>узел дерева</returns>
        internal static TreeNode GetNode(string entityTypeCode, int entityId, int? parentEntityId, bool isFolder, bool isGroup = false, string groupItemCode = null, bool loadChildNodes = false)
        {
            var node = GetChildNodeList(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId).Single();

            if (node != null)
            {
                if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder && string.IsNullOrEmpty(node.Title))
                {
                    node.Title = LibraryStrings.RootFolder;
                }

                if (loadChildNodes)
                {
                    var currentParentEntityId = node.IsFolder ? node.ParentId : node.Id;

                    node.ChildNodes = GetChildNodeList(node.Code, currentParentEntityId, node.IsFolder, node.IsGroup, node.GroupItemCode);
                }
            }

            return node;
        }



        private static string GetTreeNodeSql(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId, bool countOnly = false)
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
            var realParentIdStr = realParentId.HasValue ? realParentId.ToString() : "NULL";

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
            else if (!string.IsNullOrWhiteSpace(groupItemCode) )
            {
                if (!isFolder)
                {
                    newIsFolder = true;
                    newEntityTypeCode = groupItemCode;
                }
            }


            var newEntityType = entityTypes.FirstOrDefault(x => x.Code.Equals(newEntityTypeCode, StringComparison.InvariantCultureIgnoreCase));

            var parentIdField = newEntityType?.ParentIdField;
            string realParentIdField = null;
            if (isGroup)
            {
                realParentIdField = parentIdField;
                parentIdField = newEntityType?.GroupParentIdField;
            }

            var iconField = newEntityType?.IconField ?? "NULL";
            var iconModifierField = newEntityType?.IconModifierField ?? "NULL";

            var sqlSb = new StringBuilder();
            var selectSb = new StringBuilder();
            var whereSb = new StringBuilder();
            var orderSb = new StringBuilder();
            var sql = string.Empty;
            var databaseType = QPContext.DatabaseType;
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
                        whereSb.AppendLine($" AND {newEntityType.RecurringIdField} {(isFolder ? " is null" : $" = {parentEntityId}")}");
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
                            sqlSb.AppendLine(GetSitesListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false));
                            break;
                        case "qp_real_content_list":
                            siteId = !string.IsNullOrWhiteSpace(realParentIdField) ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), false, siteId));
                            break;
                        case "qp_virtual_content_list":
                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            sqlSb.AppendLine(GetContentListSql(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), true, siteId));
                            break;
                        case "qp_site_folder_list":

                            siteId = realParentId.HasValue ? (decimal?)realParentId.Value : parentEntityId;
                            var parentFolderId = newIsFolder ? 0 : parentEntityId.Value;
                            sqlSb.AppendLine(GetSiteFolderList(selectSb.ToString(), whereSb.ToString(), orderSb.ToString(), siteId, parentFolderId));
                            break;
                    }
                }

                if (countOnly)
                {
                    return string.IsNullOrWhiteSpace(sqlSb.ToString())
                        ? null
                        : $"SELECT COUNT(ID) FROM ({sqlSb}) as innerSql";
                }
                else
                {
                    sql = $" SELECT\n" +
                        $"{realParentIdStr} as parent_id,\n" +
                        $"{(isGroup ? $"{parentEntityId}" : "NULL")} as parent_group_id,\n" +
                        $"'{newEntityTypeCode}' as code,\n" +
                        $"{ToBoolSql(databaseType, false)} as is_folder,\n" +
                        $"{ToBoolSql(databaseType, currentIsGroup)} as is_group,\n" +
                        $"{(!string.IsNullOrWhiteSpace(currentGroupItemCode) ? $"'{currentGroupItemCode}'" : "NULL")} as group_item_code,\n" +
                        $"CASE WHEN i.ICON is not null THEN i.ICON\n" +
                        $"WHEN i.ICON_MODIFIER is not null THEN {ConcatStrValuesSql(databaseType, $"'{newEntityTypeCode}'", CastToString(databaseType, "i.ICON_MODIFIER"), "'.gif'")}\n" +
                        $"ELSE {ConcatStrValuesSql(databaseType, $"'{newEntityTypeCode}'", "'.gif'")} END\n" +
                        $"AS icon,\n" +
                        $"{NullableDbValue(newEntityType?.DefaultActionId)} AS default_action_id,\n" +
                        $"{NullableDbValue(newEntityType?.ContextMenuId)} as context_menu_id,\n" +
                        $"{ToBoolSql(databaseType, string.IsNullOrWhiteSpace(newEntityType?.RecurringIdField))} as is_recurring,\n" +
                        $"i.id,\n" +
                        $"i.title,\n" +
                        $"i.sortorder\n" +
                        $"FROM ( {sqlSb} ) as i\n";
                }
            }

            else
            {
                var condition = $" et.disabled = {ToBoolSql(databaseType, false)}";
                condition += string.IsNullOrWhiteSpace(newEntityTypeCode)
                    ? " and et.parent_id is null "
                    : $" and et.parent_id = {entityType.Id} ";

                var useSecurity = !QPContext.IsAdmin;

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
                        {NullableDbValue(parentEntityId)} as parent_id,
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

        private static string NullableDbValue(int? value) => value.HasValue ? value.ToString() : "NULL";

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

        private static string GetSiteFolderList(string select, string filter, string orderBy, decimal? siteId, int parentFolderId)
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

            var userId = QPContext.CurrentUserId;
            var useSecurity = !QPContext.IsAdmin;
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
               -- {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"order by {orderBy}")}
                ";
            return sql;
        }

        private static string GetContentListSql(string select, string filter, string orderBy, bool isVirtual, decimal? siteId)
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
            var userId = QPContext.CurrentUserId;

            var useSecurity = !QPContext.IsAdmin;
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
                -- {(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"order by {orderBy}")}
                ";
            return sql;
        }

        private static string GetSitesListSql(string select, string filter, string orderBy, bool siteIdOnly)
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

            var userId = QPContext.CurrentUserId;

            var useSecurity = !QPContext.IsAdmin;
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
                --{(string.IsNullOrWhiteSpace(orderBy) ? string.Empty : $"order by {orderBy}")}
                ";
            return sql;
        }

        /// <summary>
        /// Возвращает список дочерних узлов для указанного узла
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <param name="parentEntityId">идентификатор родительской сущности</param>
        /// <param name="isFolder">признак является ли узел директорией</param>
        /// <returns>список дочерних узлов</returns>
        internal static IEnumerable<TreeNode> GetChildNodeList(string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId = 0)
        {
            var nodesList = Enumerable.Empty<TreeNode>();

            using (var newScope = new QPConnectionScope())
            {
                var sql = GetTreeNodeSql(entityTypeCode, parentEntityId, isFolder, isGroup, groupItemCode, entityId);
                var dataRows = Common.GetDataRows(newScope.DbConnection, sql).ToList();
                nodesList = MapperFacade.TreeNodeMapper.GetBizList(dataRows);
                var index = 0;
                foreach (var node in nodesList)
                {
                    string countSql = null;
                    if (node.IsFolder)
                    {
                        countSql = GetTreeNodeSql(node.Code, node.ParentId, true, node.IsGroup, node.GroupItemCode, 0, true);
                    }
                    else if (index == 0 || node.IsRecurring || node.IsGroup)
                    {
                        countSql = GetTreeNodeSql(node.Code, node.Id, false, node.IsGroup, node.GroupItemCode, 0, true);
                    }

                    if (countSql != null)
                    {
                        var count = Common.ExecuteScalarLong(newScope.DbConnection, countSql);
                        node.HasChildren = count > 0;
                    }

                    index++;
                }
            }





            if (entityTypeCode == EntityTypeCode.SiteFolder || entityTypeCode == EntityTypeCode.ContentFolder)
            {
                var rootFolderNode = nodesList.Where(n => string.IsNullOrEmpty(n.Title)).SingleOrDefault();
                if (rootFolderNode != null)
                {
                    rootFolderNode.Title = LibraryStrings.RootFolder;
                }
            }

            // Не показываем не администратору контенты для которых запрещены операции редактирования
            if (entityTypeCode == EntityTypeCode.Content || entityTypeCode == EntityTypeCode.VirtualContent)
            {
                if (!QPContext.IsAdmin)
                {
                    var chdIDs = ContentRepository.GetChangeDisabledIDs();
                    if (chdIDs.Any())
                    {
                        nodesList = nodesList.Where(c => !chdIDs.Contains(c.Id)).ToArray();
                    }
                }

                if (nodesList.Any())
                {
                    var firstNode = nodesList.First();
                    if (firstNode.Code == EntityTypeCode.ContentGroup)
                    {
                        var siteId = firstNode.ParentId.Value;
                        var defaultGroupId = ContentRepository.GetDefaultGroupId(siteId);
                        var defaultNode = nodesList.Where(n => n.Id == defaultGroupId).SingleOrDefault();
                        if (defaultNode != null)
                        {
                            defaultNode.Title = Translator.Translate(defaultNode.Title);
                        }

                        nodesList = nodesList.Where(n => n.HasChildren).OrderBy(n => n.Title).ToList();
                    }
                }
            }

            if (entityTypeCode == null)
            {
                var rootNode = nodesList.Where(n => n.Code == EntityTypeCode.CustomerCode).SingleOrDefault();
                if (rootNode != null)
                {
                    rootNode.Title = QPContext.CurrentCustomerCode;
                }
            }

            return nodesList;
        }
    }
}
