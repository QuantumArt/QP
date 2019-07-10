using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

            using (var cmd = DbCommandFactory.Create(sqlText, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
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


    }
}
