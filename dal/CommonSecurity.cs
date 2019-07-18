using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public class CommonSecurity
    {
        public static DataRow[] GetRelationSecurityFields(DbConnection sqlConnection)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            const string contentLinkSql = @"
                    SELECT
                        link_id AS link_id,
                        l_content_id AS content_id,
                        r_content_id AS linked_content_id
                    FROM content_to_content
                    UNION
                    SELECT
                        link_id AS link_id,
                        r_content_id AS content_id,
                        l_content_id AS linked_content_id
                    FROM content_to_content
                    ";
            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);

            var sqlText = $@"
				select coalesce(ca3.content_id, ca1.content_id) as path_content_id, coalesce(ca4.CONTENT_ID, cl.linked_content_id) as rel_content_id, ca1.content_id,
				{SqlQuerySyntaxHelper.CastToBool(dbType, $"case when ca1.link_id is not null then {trueValue} else {falseValue} end")} as is_m2m,
				{SqlQuerySyntaxHelper.CastToBool(dbType, $"case when ca2.attribute_id is not null then {trueValue} else {falseValue} end")} as is_ext,
				ca1.is_classifier,
				ca1.attribute_id, ca1.attribute_name, ca1.link_id, ca2.ATTRIBUTE_NAME as agg_attribute_name
				from CONTENT_ATTRIBUTE ca1
				left join ({contentLinkSql}) cl on ca1.content_id = cl.content_id and ca1.link_id = cl.link_id
				left join CONTENT_ATTRIBUTE ca4 on ca1.RELATED_ATTRIBUTE_ID = ca4.ATTRIBUTE_ID
				left join content_attribute ca2 on ca1.content_id = ca2.content_id and ca2.AGGREGATED = {trueValue}
				left join content_attribute ca3 on ca2.RELATED_ATTRIBUTE_ID = ca3.attribute_Id
				 where ca1.USE_RELATION_SECURITY = {trueValue}
			 ";

            return Common.GetDataTableForQuery(sqlConnection, sqlText).AsEnumerable().ToArray();
        }

        public class RelationSecurityPathFinder
        {
            public RelationSecurityPathFinder(List<DataRow> pathRows, int contentId, List<RelationSecurityPathItem> pathToCopy = null)
            {
                PathRows = pathRows;
                OldIndex = 0;
                Index = 1;
                CurrentContentId = contentId;
                CurrentPath = pathToCopy?.Select(n => n.Clone()).ToList() ?? new List<RelationSecurityPathItem>();
                ExtraFinders = new List<RelationSecurityPathFinder>();
            }

            public List<DataRow> PathRows { get; set; }

            public int CurrentContentId { get; set; }

            public int CurrentRelatedContentId { get; set; }

            public int OldIndex { get; set; }

            public int Index { get; set; }

            public List<RelationSecurityPathItem> CurrentPath { get; }

            public List<RelationSecurityPathFinder> ExtraFinders { get; private set; }

            public void Compute()
            {
                while (true)
                {
                    var nextRows = PathRows
                        .Where(n => (int)n.Field<decimal>("path_content_id") == CurrentContentId
                            && (CurrentRelatedContentId == 0 || (int?)n.Field<decimal?>("rel_content_id") == CurrentRelatedContentId))
                        .OrderBy(n => (int)n.Field<decimal>("attribute_id"))
                        .ToArray();
                    if (!nextRows.Any())
                    {
                        break;
                    }

                    if (nextRows.Any(n => n.Field<bool>("is_ext")))
                    {
                        if (!nextRows.All(n => n.Field<bool>("is_ext")))
                        {
                            throw new Exception("Incorrect relation security settings: mixed extensions and normal");
                        }

                        var nextIds = nextRows.Select(n => (int)n.Field<decimal>("rel_content_id")).Distinct().OrderBy(n => n).ToArray();
                        if (nextIds.Length > 1)
                        {
                            nextRows = nextRows.Where(n => (int)n.Field<decimal>("rel_content_id") == nextIds[0]).ToArray();
                        }

                        ExtraFinders = nextIds.Skip(1).Select(n => new RelationSecurityPathFinder(PathRows, CurrentContentId, CurrentPath) { CurrentRelatedContentId = n, OldIndex = OldIndex, Index = Index}).ToList();

                        var param = new RelationSecurityPathItem
                        {
                            ContentId = (int)nextRows.First().Field<decimal>("path_content_id"),
                            Extensions = nextRows.Select((n, k) => new RelationSecurityPathItem(n) { Order = Index + k + 1, JoinOrder = OldIndex }).ToArray(),
                            Order = Index,
                            JoinOrder = OldIndex
                        };

                        OldIndex = Index;
                        Index = Index + 1 + param.Extensions.Length;
                        CurrentPath.Add(param);
                    }
                    else
                    {
                        var param = new RelationSecurityPathItem(nextRows.First()) { Order = Index, JoinOrder = OldIndex };
                        OldIndex = Index;
                        Index++;

                        if (nextRows.Length > 1)
                        {
                            param.Secondary = nextRows.Skip(1).Select((n, j) => new RelationSecurityPathItem(n) { Order = Index + j + 1, JoinOrder = param.JoinOrder }).ToArray();
                            Index = Index + param.Secondary.Length;
                        }
                        else
                        {
                            param.Secondary = new RelationSecurityPathItem[] { };
                        }
                        CurrentPath.Add(param);
                    }

                    CurrentContentId = nextRows.Select(n => (int)(n.Field<decimal?>("rel_content_id") ?? 0)).First();
                    CurrentRelatedContentId = 0;
                }


            }

        }

        public static Dictionary<int, bool> CheckArticleSecurity(DbConnection sqlConnection, int contentId, int[] testIds,
            int userId, int startLevel)
        {
            const string columnName = "content_item_id";
            const string entityName = EntityTypeCode.OldArticle;
            const string parentEntityName = EntityTypeCode.Content;
            return CheckSecurity(sqlConnection, contentId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
        }

        public static Dictionary<int, bool> CheckContentSecurity(DbConnection sqlConnection, int siteId, int[] testIds,
            int userId, int startLevel)
        {
            const string columnName = "content_id";
            const string entityName = EntityTypeCode.Content;
            const string parentEntityName = EntityTypeCode.Site;
            return CheckSecurity(sqlConnection, siteId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
        }

        private static Dictionary<int, bool> CheckSecurity(DbConnection sqlConnection, int parentId, IEnumerable<int> testIds, int userId, int startLevel, string entityName, string parentEntityName, string columnName)
        {
            var granted = new Dictionary<int, bool>();
            var securitySql = Common.GetPermittedItemsAsQuery(sqlConnection, userId, 0, startLevel, PermissionLevel.FullAccess,
                entityName, parentEntityName, parentId);

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);

            var sql = $@" select
                i.id,
                {SqlQuerySyntaxHelper.CastToBool(dbType, $"case when pi.{columnName} is null then {falseValue} else {trueValue} end")} as granted
                from  {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")}
				left join ({securitySql}) as pi on pi.{columnName} = i.id ";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", testIds, dbType));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        granted[Convert.ToInt32(reader["id"])] = (bool)reader["granted"];
                    }
                }
            }

            return granted;
        }

        private static bool IsAdmin(DbConnection sqlConnection, int userId, int groupId)
        {
            if (userId != 0)
            {
                return Common.IsAdmin(sqlConnection, userId);
            }

            if (groupId != 0)
            {
                return groupId == SpecialIds.AdminGroupId;
            }

            return false;
        }

        public static int GetEntityAccessLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, string entityTypeCode, int entityId)
        {
            var actualEntityTypeCode = GetActualEntityTypeCode(entityTypeCode);

            if (!IsSecurityDefined(actualEntityTypeCode) || entityId == 0 || IsAdmin(sqlConnection, userId, groupId))
            {
                return PermissionLevel.FullAccess;
            }

            var predefinedLevel = GetPredefinedLevel(sqlConnection, context, userId, groupId, entityId, actualEntityTypeCode);

            if (predefinedLevel.HasValue)
            {
                return predefinedLevel.Value;
            }

            var result = GetPermissionLevel(sqlConnection, entityId, userId, groupId, actualEntityTypeCode);

            if (!result.HasValue && actualEntityTypeCode == EntityTypeCode.OldSiteFolder)
            {
                return GetFolderAccessLevel(sqlConnection, context, userId, groupId, actualEntityTypeCode, entityId);
            }

            return result ?? PermissionLevel.Deny;
        }

        private static bool IsSecurityDefined(string actualEntityTypeCode)
        {
            var entityTypeCodes = new[]
            {
                EntityTypeCode.OldArticle, EntityTypeCode.Content, EntityTypeCode.Site,
                EntityTypeCode.OldSiteFolder, EntityTypeCode.ContentFolder, EntityTypeCode.Workflow
            };

            return entityTypeCodes.Contains(actualEntityTypeCode);
        }

        private static int? GetPredefinedLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, int entityId, string actualEntityTypeCode)
        {
            int? predefinedLevel = null;

            switch (actualEntityTypeCode)
            {
                case EntityTypeCode.Content:
                {
                    predefinedLevel = GetPredefinedContentLevel(sqlConnection, context, userId, groupId, entityId);
                    break;
                }

                case EntityTypeCode.ContentFolder:
                {
                    predefinedLevel = GetPredefinedContentFolderLevel(sqlConnection, context, userId, groupId, entityId);
                    break;
                }

                case EntityTypeCode.OldArticle:
                {
                    predefinedLevel = GetPredefinedArticleLevel(sqlConnection, context, userId, groupId, entityId);
                    break;
                }
            }

            return predefinedLevel;
        }

        private static int GetFolderAccessLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, string entityTypeCode, int entityId)
        {
            var folder = context.SiteFolderSet.Single(n => n.Id == entityId);
            return folder.ParentId.HasValue ?
                GetEntityAccessLevel(sqlConnection, context, userId, groupId, entityTypeCode, (int)folder.ParentId.Value) :
                GetEntityAccessLevel(sqlConnection, context, userId, groupId, EntityTypeCode.Site, (int)folder.SiteId);
        }

        private static int? GetPredefinedArticleLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, int entityId)
        {
            int? resultLevel = null;
            var contentId = context.ArticleSet.Include(n => n.Content)
                .Where(n => n.Id == entityId && n.Content.AllowItemsPermission == 0)
                .Select(n => n.ContentId).SingleOrDefault();

            if (contentId != 0)
            {
                resultLevel = GetEntityAccessLevel(sqlConnection, context, userId, groupId, EntityTypeCode.Content, (int)contentId);
            }

            return resultLevel;
        }

        private static int? GetPredefinedContentFolderLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, int entityId)
        {
            int? resultLevel = null;
            var contentId = context.ContentFolderSet.Where(n => n.Id == entityId)
                .Select(n => n.ContentId)
                .SingleOrDefault();

            if (contentId != 0)
            {
                resultLevel = GetEntityAccessLevel(sqlConnection, context, userId, groupId, EntityTypeCode.Content, (int)contentId);
            }

            return resultLevel;
        }

        private static int? GetPredefinedContentLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, int entityId)
        {
            int? resultLevel = null;
            var classifierId = context.FieldSet.Where(n => n.ContentId == entityId)
                .Where(n => n.ClassifierId != null)
                .Select(n => n.ClassifierId)
                .SingleOrDefault();

            if (classifierId.HasValue)
            {
                var baseContentId = context.FieldSet.Where(n => n.Id == classifierId.Value)
                    .Select(n => n.ContentId)
                    .SingleOrDefault();

                if (baseContentId != 0)
                {
                    resultLevel = GetEntityAccessLevel(sqlConnection, context, userId, groupId, EntityTypeCode.Content, (int)baseContentId);
                }
            }

            return resultLevel;
        }

        private static string GetActualEntityTypeCode(string entityTypeCode)
        {
            string actualEntityTypeCode;
            switch (entityTypeCode)
            {
                case EntityTypeCode.Article:
                    actualEntityTypeCode = EntityTypeCode.OldArticle;
                    break;
                case EntityTypeCode.VirtualContent:
                    actualEntityTypeCode = EntityTypeCode.Content;
                    break;
                case EntityTypeCode.SiteFolder:
                    actualEntityTypeCode = EntityTypeCode.OldSiteFolder;
                    break;
                default:
                    actualEntityTypeCode = entityTypeCode;
                    break;
            }

            return actualEntityTypeCode;
        }

        private static int? GetPermissionLevel(DbConnection sqlConnection, int id, int userId, int groupId,
            string entityTypeName, string parentEntityTypeName = "", int parentId = 0)
        {
            return GetPermissionLevels(
                sqlConnection, new[] { id }, userId, groupId, entityTypeName, parentEntityTypeName, parentId
            )[id];
        }

        private static Dictionary<int,int?> GetPermissionLevels(DbConnection sqlConnection, int[] ids, int userId, int groupId, string entityTypeName, string parentEntityTypeName = "", int parentId = 0)
        {
            var result = new Dictionary<int, int?>();
            var securitySql = Common.GetPermittedItemsAsQuery(sqlConnection, userId, groupId, PermissionLevel.Deny, PermissionLevel.FullAccess,
                entityTypeName, parentEntityTypeName, parentId);

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);

            var sql = $@" select i.id, pi.permission_level from {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")}
				left join ({securitySql}) as pi on pi.{entityTypeName}_id = i.id ";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[Convert.ToInt32(reader["id"])] = Converter.ToNullableInt32(reader["permission_level"]);
                    }
                }
            }

            return result;
        }

        public static string GetSecurityPathSql(DatabaseType dbType, List<RelationSecurityPathItem> items, int contentId)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var sqlBuilder = new StringBuilder();
            var lastItem = items.Last();
            var selects = Enumerable.Repeat(lastItem, 1).Concat(lastItem.Secondary).Select(item => GetSelectExpression(dbType, item));
            sqlBuilder.Append($"select c0.content_item_id as id, {string.Join(", ", selects)} from content_{contentId}_united c0 ");
            foreach (var item in items)
            {
                if (item != lastItem)
                {
                    if (item.LinkId.HasValue)
                    {
                        throw new ApplicationException("Invalid security path settings: m2m");
                    }

                    if (item.Secondary.Any())
                    {
                        throw new ApplicationException("Invalid security path settings: secondary");
                    }
                }

                if (!item.LinkId.HasValue)
                {
                    if (item.Extensions.Any())
                    {
                        var first = item.Extensions.First();
                        var inner = string.Join("union all " + Environment.NewLine,
                            item.Extensions.Select(n =>
                                n.LinkId.HasValue
                                    ? $@"select {n.AggAttributeName} as agg, il.linked_item_id as {first.AttributeName} From content_{n.ContentId}_united c
											inner join item_link il on c.content_item_id = il.item_id and il.link_id = {n.LinkId.Value} {Environment.NewLine} "
                                    : $"select {n.AggAttributeName} as agg, {n.AttributeName} as {first.AttributeName} From content_{n.ContentId}_united {Environment.NewLine}")
                        );

                        sqlBuilder.AppendFormat("inner join ({3}{0}{3}) c{1} on c{1}.agg = c{2}.CONTENT_ITEM_ID{3}", inner, first.Order,
                            item.JoinOrder, Environment.NewLine);
                        sqlBuilder.AppendFormat("inner join content_{0}_united c{1} on c{2}.{3} = c{1}.CONTENT_ITEM_ID{4}",
                            first.RelContentId, item.Order, first.Order, first.AttributeName, Environment.NewLine);
                    }
                    else if (!item.IsClassifier)
                    {
                        sqlBuilder.AppendFormat("inner join content_{0}_united c{1} on c{2}.{3} = c{1}.CONTENT_ITEM_ID{4}",
                            item.RelContentId, item.Order, item.JoinOrder, item.AttributeName, Environment.NewLine);
                    }
                }

                if (item == lastItem && item.Secondary != null)
                {
                    foreach (var secItem in item.Secondary.Where(n => !n.LinkId.HasValue && !n.IsClassifier))
                    {
                        sqlBuilder.AppendFormat("inner join content_{0}_united c{1} on c{2}.{3} = c{1}.CONTENT_ITEM_ID{4}",
                            secItem.RelContentId, secItem.Order, secItem.JoinOrder, secItem.AttributeName, Environment.NewLine);
                    }
                }
            }

            sqlBuilder.Append($"where c0.content_item_id in (select id from {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")})");
            return sqlBuilder.ToString();
        }

        private static string GetSelectExpression(DatabaseType dbType, RelationSecurityPathItem n)
        {
            if (n.IsClassifier)
            {
                return $"c{n.JoinOrder}.{SqlQuerySyntaxHelper.EscapeEntityName(dbType, n.AttributeName)}";
            }

            var fieldName = dbType == DatabaseType.Postgres ? $"\"{n.RelContentId}\"" : $"'{n.RelContentId}'";
            var castToInt = dbType == DatabaseType.Postgres ? "::integer" : string.Empty;

            return n.LinkId.HasValue
                ? $"{SqlQuerySyntaxHelper.DbSchemaName(dbType)}.qp_link_ids({n.LinkId}{castToInt}, c{n.JoinOrder}.CONTENT_ITEM_ID{castToInt}, {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}) as {fieldName}"
                : $"{SqlQuerySyntaxHelper.CastToString(dbType, $"c{n.Order}.content_item_id")} as {fieldName}";
        }

        public static Dictionary<int, bool> CheckLockedBy(DbConnection dbConnection, int[] ids, int currentUserId, bool forceUnlock)
        {
            const string sql = @"select locked_by, content_item_id from content_item ci with(nolock)
				inner join @ids i on i.id = ci.content_item_id where locked_by is not null and locked_by <> @userId ";

            var result = ids.ToDictionary(kvp => kvp, kvp => true);
            if (!forceUnlock)
            {
                using (var cmd = DbCommandFactory.Create(sql, dbConnection))
                {
                    cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = Common.IdsToDataTable(ids)
                    });

                    cmd.Parameters.AddWithValue("@userId", currentUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = (int)(decimal)reader["content_item_id"];
                            result[id] = false;
                        }
                    }
                }
            }

            return result;
        }

        public static RelationSecurityInfo GetRelationSecurityInfo(DbConnection dbConnection, int contentId, int[] ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(dbConnection);
            var result = new RelationSecurityInfo();
            var pathRows = GetRelationSecurityFields(dbConnection);

            var securityPathes = new List<List<RelationSecurityPathItem>>();
            var finder = new RelationSecurityPathFinder(pathRows.ToList(), contentId);
            finder.Compute();
            securityPathes.Add(finder.CurrentPath);


            foreach (var extra in finder.ExtraFinders)
            {
                extra.Compute();
                securityPathes.Add(extra.CurrentPath);
            }

            foreach (var securityPath in securityPathes)
            {
                if (securityPath.Count <= 0)
                {

                    var isEndNode = finder.PathRows.Any(n => (Converter.ToNullableInt32(n["rel_content_id"]) ?? 0 ) == contentId);
                    if (!isEndNode)
                    {
                        result.MakeEmpty();
                    }
                    else
                    {
                        result.AddContentInItemMapping(contentId, ids.ToDictionary(n => n, m => Enumerable.Repeat(m, 1).ToArray()));
                    }

                    return result;
                }

                var lastItem = securityPath.Last();
                var lastItemWithSecondary = Enumerable.Repeat(lastItem, 1).Concat(lastItem.Secondary).ToList();
                var contentIds = lastItemWithSecondary.Where(n => !n.IsClassifier).Select(n => n.RelContentId).ToArray();
                var attNames = lastItemWithSecondary.Where(n => n.IsClassifier).Select(n => n.AttributeName).ToArray();
                foreach (var item in contentIds)
                {
                    result.AddContentInItemMapping(item, new Dictionary<int, int[]>());
                }

                var sql = GetSecurityPathSql(dbType, securityPath, contentId);
                using (var cmd = DbCommandFactory.Create(sql, dbConnection))
                {
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProcessSecurityPathSqlReader(reader, contentIds, attNames, result);
                        }
                    }
                }

                AppendNotFound(ids, contentIds, result);
            }

            return result;
        }

        public static void ClearUserToken(DbConnection dbConnection, int userId, int sessionId)
        {
            var sql = "delete from access_token where UserId = @userId and SessionId = @sessionId";
            using (var cmd = DbCommandFactory.Create(sql, dbConnection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void AppendNotFound(int[] ids, int[] contentIds, RelationSecurityInfo result)
        {
            if (contentIds.Any())
            {
                var firstContentId = contentIds.First();
                var notFound = ids.Where(n => !result.IsItemMappingExists(firstContentId, n)).ToArray();
                foreach (var item in contentIds)
                {
                    foreach (var nfItem in notFound)
                    {
                        result.AppendToItemMapping(item, nfItem, null);
                    }
                }
            }
        }

        private static void ProcessSecurityPathSqlReader(DbDataReader reader, int[] contentIds, string[] classifierNames,
            RelationSecurityInfo result)
        {
            var id = Converter.ToInt32(reader["id"]);
            foreach (var item in contentIds)
            {
                var ints = Converter.ToInt32Collection((string)reader[item.ToString()], ',');
                result.AppendToItemMapping(item, id, ints);
            }

            if (classifierNames != null)
            {
                foreach (var clName in classifierNames)
                {
                    var clValue = Converter.ToInt32(reader[clName]);
                    result.AppendToContentMapping(clValue, id);
                }
            }
        }


        public static IEnumerable<DataRow> GetActionStatusList(QPModelDataContext efContext, DbConnection sqlConnection, int userId, string actionCode, int? actionId, int entityId, string entityCode, bool isAdmin)
        {
            var useSecurity = !isAdmin;
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            string query;

             if (!useSecurity)
             {
                query = $@"
                    SELECT ba.CODE, {SqlQuerySyntaxHelper.ToBoolSql(databaseType, true)} as visible
		            FROM ACTION_TOOLBAR_BUTTON atb
		            INNER JOIN BACKEND_ACTION ba on ba.ID = atb.ACTION_ID
		            INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
		            WHERE atb.PARENT_ACTION_ID = {actionId} AND at.items_affected = 1
                ";
            }
            else
            {
                var level = GetEntityAccessLevel(sqlConnection, efContext, userId, 0, entityCode, entityId);
                var least = SqlQuerySyntaxHelper.Least(databaseType, "SEC.PERMISSION_LEVEL", level.ToString());
                var secQuery = PermissionHelper.GetActionPermissionsAsQuery(efContext, userId);
                query = $@"
                    SELECT ba.CODE,
					CAST((
                        CASE WHEN {least} >= PL.PERMISSION_LEVEL THEN 1 ELSE 0
                    END ) AS BIT) as visible
		            FROM ACTION_TOOLBAR_BUTTON atb
		            INNER JOIN BACKEND_ACTION ba on ba.ID = atb.ACTION_ID
		            INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
					INNER JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = AT.REQUIRED_PERMISSION_LEVEL_ID
					INNER JOIN ({secQuery}) SEC ON SEC.BACKEND_ACTION_ID = ba.ID
		            WHERE atb.PARENT_ACTION_ID = {actionId} AND at.items_affected = 1
                ";
            }

            return Common.GetDataTableForQuery(sqlConnection, query);
        }

        public static IEnumerable<DataRow> GetMenuStatusList(
            DbConnection sqlConnection, QPModelDataContext efContext, int userId, bool isAdmin,
            string menuCode, int entityId)
        {
            var useSecurity = !isAdmin;
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var menuId = efContext.ContextMenuSet.First(x => x.Code == menuCode).Id;
            string query;

             if (!useSecurity)
             {
                query = $@"
                    SELECT ba.CODE, {SqlQuerySyntaxHelper.ToBoolSql(databaseType, true)} as visible
		            FROM CONTEXT_MENU_ITEM cmi
		            INNER JOIN BACKEND_ACTION ba on ba.ID = cmi.ACTION_ID
		            WHERE cmi.context_menu_id = {menuId}
                ";
            }
            else
            {
                var level = GetEntityAccessLevel(sqlConnection, efContext, userId, 0, menuCode, entityId);
                var secQuery = PermissionHelper.GetActionPermissionsAsQuery(efContext, userId);
                var least = SqlQuerySyntaxHelper.Least(databaseType, "SEC.PERMISSION_LEVEL", level.ToString());
                query = $@"
                    SELECT ba.CODE,
					CAST((
                        CASE WHEN {least} >= PL.PERMISSION_LEVEL THEN 1 ELSE 0
                    END ) AS BIT) as visible
		            FROM CONTEXT_MENU_ITEM cmi
		            INNER JOIN BACKEND_ACTION ba on ba.ID = cmi.ACTION_ID
		            INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
					INNER JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = AT.REQUIRED_PERMISSION_LEVEL_ID
					INNER JOIN ({secQuery}) SEC ON SEC.BACKEND_ACTION_ID = ba.ID
		            WHERE cmi.context_menu_id = {menuId}
                ";
            }

            return Common.GetDataTableForQuery(sqlConnection, query);
        }

        public static void CreateContentAccess(DbConnection connection, int id)
        {
            GiveContentAccessToCreator(connection, id);
            GiveContentAccessByPropagatingFromSite(connection, id);
        }

        public static void CreateSiteAccess(DbConnection connection, int id)
        {
            GiveSiteAccessToCreator(connection, id);
        }

        public static void CreateWorkflowAccess(DbConnection connection, int id)
        {
            GiveWorkflowAccessToCreator(connection, id);
        }

        private static string PropagateToItems => "case c.virtual_type when 0 then 1 else 0 end as propagate_to_items";

        private static void GiveContentAccessToCreator(DbConnection connection, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = $@"
                INSERT INTO content_access (content_id, user_id, permission_level_id, last_modified_by, propagate_to_items)
                SELECT content_id, last_modified_by, 1, 1, {PropagateToItems}
                FROM content c where c.content_id = {id}
            " ;
            Common.ExecuteSql(connection, sql);
        }

        private static void GiveSiteAccessToCreator(DbConnection connection, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = $@"
                INSERT INTO site_access (site_id, user_id, permission_level_id, last_modified_by)
                SELECT site_id, last_modified_by, 1, 1
                FROM site s where s.site_id = {id}
            " ;
            Common.ExecuteSql(connection, sql);
        }

        public static void GiveContentAccessByPropagatingFromSite(DbConnection connection, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = $@"
                INSERT INTO content_access (content_id, user_id, group_id, permission_level_id, last_modified_by, propagate_to_items)
                SELECT c.content_id, ca.user_id, ca.group_id, ca.permission_level_id, 1, {PropagateToItems}
                FROM content c inner join site_access ca on ca.site_id = c.site_id
                WHERE c.content_id = {id}
                AND (ca.user_id <> c.last_modified_by OR ca.user_id IS NULL) AND ca.propagate_to_contents = 1
            " ;
            Common.ExecuteSql(connection, sql);
        }

        private static void GiveWorkflowAccessToCreator(DbConnection connection, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = $@"
                INSERT INTO workflow_access (workflow_id, user_id, permission_level_id, last_modified_by)
                SELECT workflow_id, last_modified_by, 1, 1
                FROM workflow w where w.workflow_id = {id}
            " ;
            Common.ExecuteSql(connection, sql);
        }


    }
}
