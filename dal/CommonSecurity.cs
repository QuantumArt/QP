using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public class CommonSecurity
    {
        public static DataRow[] GetRelationSecurityFields(SqlConnection sqlConnection)
        {
            const string sqlText = @"
				select coalesce(ca3.content_id, ca1.content_id) as path_content_id, coalesce(ca4.CONTENT_ID, cl.linked_content_id) as rel_content_id, ca1.content_id, 
				cast(case when ca1.link_id is not null then 1 else 0 end as bit) as is_m2m,
				cast(case when ca2.attribute_id is not null then 1 else 0 end as bit) as is_ext,
				ca1.is_classifier,
				ca1.attribute_id, ca1.attribute_name, ca1.link_id, ca2.ATTRIBUTE_NAME as agg_attribute_name
				from CONTENT_ATTRIBUTE ca1
				left join content_link cl on ca1.content_id = cl.content_id and ca1.link_id = cl.link_id
				left join CONTENT_ATTRIBUTE ca4 on ca1.RELATED_ATTRIBUTE_ID = ca4.ATTRIBUTE_ID
				left join content_attribute ca2 on ca1.content_id = ca2.content_id and ca2.AGGREGATED = 1
				left join content_attribute ca3 on ca2.RELATED_ATTRIBUTE_ID = ca3.attribute_Id
				 where ca1.USE_RELATION_SECURITY = 1
			 ";

            using (var cmd = SqlCommandFactory.Create(sqlText, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static List<RelationSecurityPathItem> FindRelationSecurityPath(List<DataRow> pathRows, int contentId)
        {
            var result = new List<RelationSecurityPathItem>();

            var currentContentId = contentId;

            var oldI = 0;
            var i = 1;

            while (true)
            {
                var nextRows = pathRows
                    .Where(n => (int)n.Field<decimal>("path_content_id") == currentContentId)
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

                    var nextIds = nextRows.Select(n => (int)n.Field<decimal>("rel_content_id")).Distinct();
                    if (nextIds.Count() > 1)
                    {
                        throw new Exception("Incorrect relation security settings: extensions targets different contents");
                    }

                    var param = new RelationSecurityPathItem
                    {
                        ContentId = (int)nextRows.First().Field<decimal>("path_content_id"),
                        Extensions = nextRows.Select((n, k) => new RelationSecurityPathItem(n) { Order = i + k + 1, JoinOrder = oldI }).ToArray(),
                        Order = i,
                        JoinOrder = oldI
                    };

                    oldI = i;
                    i = i + 1 + param.Extensions.Length;
                    result.Add(param);
                }
                else
                {
                    var param = new RelationSecurityPathItem(nextRows.First()) { Order = i, JoinOrder = oldI };
                    oldI = i;
                    i++;

                    if (nextRows.Length > 1)
                    {
                        param.Secondary = nextRows.Skip(1).Select((n, j) => new RelationSecurityPathItem(n) { Order = i + j + 1, JoinOrder = param.JoinOrder }).ToArray();
                        i = i + param.Secondary.Length;
                    }
                    else
                    {
                        param.Secondary = new RelationSecurityPathItem[] { };
                    }
                    result.Add(param);
                }

                currentContentId = nextRows.Select(n => (int)(n.Field<decimal?>("rel_content_id") ?? 0)).First();
            }

            return result;
        }

        public static Dictionary<int, bool> CheckArticleSecurity(SqlConnection sqlConnection, int contentId, int[] testIds,
            int userId, int startLevel)
        {
            const string columnName = "content_item_id";
            const string entityName = EntityTypeCode.OldArticle;
            const string parentEntityName = EntityTypeCode.Content;
            return CheckSecurity(sqlConnection, contentId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
        }

        public static Dictionary<int, bool> CheckContentSecurity(SqlConnection sqlConnection, int siteId, int[] testIds,
            int userId, int startLevel)
        {
            const string columnName = "content_id";
            const string entityName = EntityTypeCode.Content;
            const string parentEntityName = EntityTypeCode.Site;
            return CheckSecurity(sqlConnection, siteId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
        }

        private static Dictionary<int, bool> CheckSecurity(SqlConnection sqlConnection, int parentId, IEnumerable<int> testIds, int userId, int startLevel, string entityName, string parentEntityName, string columnName)
        {
            var granted = new Dictionary<int, bool>();
            var securitySql = Common.GetPermittedItemsAsQuery(sqlConnection, userId, 0, startLevel, PermissionLevel.FullAccess,
                entityName, parentEntityName, parentId);

            var sql = string.Format(
                @" select i.id, cast((case when pi.{1} is null then 0 else 1 end) as bit) as granted from @ids i
				left join ({0}) as pi on pi.{1} = i.id "
                , securitySql, columnName);

            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = Common.IdsToDataTable(testIds)
                });
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        granted[(int)(decimal)reader["id"]] = (bool)reader["granted"];
                    }
                }
            }

            return granted;
        }

        public static string GetSecurityPathSql(List<RelationSecurityPathItem> items, int contentId)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var sqlBuilder = new StringBuilder();
            var lastItem = items.Last();
            var selects = Enumerable.Repeat(lastItem, 1).Concat(lastItem.Secondary).Select(GetSelectExpression);
            sqlBuilder.Append($"select c0.content_item_id as id, {string.Join(", ", selects)} from content_{contentId}_united c0 ");
            foreach (var item in items)
            {
                if (item != lastItem)
                {
                    if (item.LinkId.HasValue)
                    {
                        throw new ApplicationException("Invalid security path settings: m2m");
                    }

                    if (item.Secondary != null && item.Secondary.Any())
                    {
                        throw new ApplicationException("Invalid security path settings: secondary");
                    }
                }

                if (!item.LinkId.HasValue)
                {
                    if (item.Extensions != null && item.Extensions.Any())
                    {
                        var first = item.Extensions.First();
                        var inner = string.Join("union all " + Environment.NewLine,
                            item.Extensions.Select(n =>
                                n.LinkId.HasValue
                                    ? string.Format(
                                        @"select {0} as agg, il.linked_item_id as {3} From content_{2}_united c
											inner join item_link il on c.content_item_id = il.item_id and il.link_id = {1} {4} ",
                                        n.AggAttributeName, n.LinkId.Value, n.ContentId, first.AttributeName, Environment.NewLine
                                    )
                                    : string.Format(
                                        "select {0} as agg, {1} as {3} From content_{2}_united {4}", n.AggAttributeName, n.AttributeName, n.ContentId,
                                        first.AttributeName, Environment.NewLine
                                    ))
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

            sqlBuilder.AppendFormat("where c0.content_item_id in (select id from @ids)");
            return sqlBuilder.ToString();
        }

        private static string GetSelectExpression(RelationSecurityPathItem n)
        {
            if (n.IsClassifier)
            {
                return $"c{n.JoinOrder}.[{n.AttributeName}]";
            }

            return n.LinkId.HasValue
                ? $"dbo.qp_link_ids({n.LinkId}, c{n.JoinOrder}.CONTENT_ITEM_ID, 1) as '{n.RelContentId}'"
                : $"cast(c{n.Order}.content_item_id as nvarchar(30)) as '{n.RelContentId}'";
        }

        public static Dictionary<int, bool> CheckLockedBy(SqlConnection dbConnection, int[] ids, int currentUserId, bool forceUnlock)
        {
            const string sql = @"select locked_by, content_item_id from content_item ci with(nolock) 
				inner join @ids i on i.id = ci.content_item_id where locked_by is not null and locked_by <> @userId ";

            var result = ids.ToDictionary(kvp => kvp, kvp => true);
            if (!forceUnlock)
            {
                using (var cmd = SqlCommandFactory.Create(sql, dbConnection))
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

        public static RelationSecurityInfo GetRelationSecurityInfo(SqlConnection dbConnection, int contentId, int[] ids)
        {
            var result = new RelationSecurityInfo();
            var pathRows = GetRelationSecurityFields(dbConnection);
            var securityPath = FindRelationSecurityPath(pathRows.ToList(), contentId);
            if (securityPath.Count <= 0)
            {
                var isEndNode = pathRows.Any(n => (int)(n.Field<decimal?>("rel_content_id") ?? 0) == contentId);
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

            var sql = GetSecurityPathSql(securityPath, contentId);
            using (var cmd = SqlCommandFactory.Create(sql, dbConnection))
            {
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = Common.IdsToDataTable(ids)
                });
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProcessSecurityPathSqlReader(reader, contentIds, attNames, result);
                    }
                }
            }

            AppendNotFound(ids, contentIds, result);
            return result;
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

        private static void ProcessSecurityPathSqlReader(SqlDataReader reader, int[] contentIds, string[] classifierNames,
            RelationSecurityInfo result)
        {
            var id = (int)(decimal)reader["id"];
            foreach (var item in contentIds)
            {
                var ints = Converter.ToInt32Collection((string)reader[item.ToString()], ',');
                result.AppendToItemMapping(item, id, ints);
            }

            if (classifierNames != null)
            {
                foreach (var clName in classifierNames)
                {
                    var clValue = Converter.ToInt32((decimal)reader[clName]);
                    result.AppendToContentMapping(clValue, id);
                }
            }
        }
    }
}
