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
			string sqlText = @"
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

			using (SqlCommand cmd = SqlCommandFactory.Create(sqlText, sqlConnection))
			{
				cmd.CommandType = CommandType.Text;
				DataTable dt = new DataTable();
				new SqlDataAdapter(cmd).Fill(dt);
				return dt.AsEnumerable().ToArray();
			}
		}

		public static List<RelationSecurityPathItem> FindRelationSecurityPath(IEnumerable<DataRow> pathRows, int contentId)
		{
			var result = new List<RelationSecurityPathItem>();

			int currentContentId = contentId;

			int old_i = 0;
			int i = 1;

			while (true)
			{
				var nextRows = pathRows
					.Where(n => (int) n.Field<decimal>("path_content_id") == currentContentId)
					.OrderBy(n => (int) n.Field<decimal>("attribute_id"))
					.ToArray();
				if (!nextRows.Any())
					break;
				else
				{
					if (nextRows.Any(n => n.Field<bool>("is_ext")))
					{
						if (!nextRows.All(n => n.Field<bool>("is_ext")))
							throw new Exception("Incorrect relation security settings: mixed extensions and normal");

						var nextIds = nextRows.Select(n => (int) n.Field<decimal>("rel_content_id")).Distinct();
						if (nextIds.Count() > 1)
							throw new Exception("Incorrect relation security settings: extensions targets different contents");

						var param =
							new RelationSecurityPathItem()
							{
								ContentId = (int) nextRows.First().Field<decimal>("path_content_id"),
								Extensions =
									nextRows.Select((n, k) => new RelationSecurityPathItem(n) {Order = i + k + 1, JoinOrder = old_i}).ToArray(),
								Order = i,
								JoinOrder = old_i
							};
						old_i = i;
						i = i + 1 + param.Extensions.Count();

						result.Add(param);
					}
					else
					{
						var param = new RelationSecurityPathItem(nextRows.First()) {Order = i, JoinOrder = old_i};
						old_i = i;
						i++;
						if (nextRows.Count() > 1)
						{
							param.Secondary =
								nextRows.Skip(1)
									.Select((n, j) => new RelationSecurityPathItem(n) {Order = i + j + 1, JoinOrder = param.JoinOrder})
									.ToArray();
							i = i + param.Secondary.Count();
						}
						else
						{
							param.Secondary = new RelationSecurityPathItem[] {};
						}
						result.Add(param);


					}
					currentContentId = nextRows.Select(n => (int)(n.Field<decimal?>("rel_content_id") ?? 0)).First();
				}
			}

			return result;
		}

		public static Dictionary<int, bool> CheckArticleSecurity(SqlConnection sqlConnection, int contentId, int[] testIds,
			int userId, int startLevel)
		{
			var columnName = "content_item_id";
			var entityName = EntityTypeCode.OldArticle;
			var parentEntityName = EntityTypeCode.Content;

			return CheckSecurity(sqlConnection, contentId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
		}

		public static Dictionary<int, bool> CheckContentSecurity(SqlConnection sqlConnection, int siteId, int[] testIds,
	int userId, int startLevel)
		{
			var columnName = "content_id";
			var entityName = EntityTypeCode.Content;
			var parentEntityName = EntityTypeCode.Site;

			return CheckSecurity(sqlConnection, siteId, testIds, userId, startLevel, entityName, parentEntityName, columnName);
		}

		private static Dictionary<int, bool> CheckSecurity(SqlConnection sqlConnection, int parentId, int[] testIds, int userId, int startLevel, string entityName, string parentEntityName, string columnName)
		{
			var granted = new Dictionary<int, bool>();
			string securitySql = Common.GetPermittedItemsAsQuery(sqlConnection, userId, 0, startLevel, PermissionLevel.FullAccess,
				entityName, parentEntityName, parentId);

			var sql = String.Format(
				@" select i.id, cast((case when pi.{1} is null then 0 else 1 end) as bit) as granted from @ids i 
				left join ({0}) as pi on pi.{1} = i.id "
				, securitySql, columnName);

			using (SqlCommand cmd = SqlCommandFactory.Create(sql, sqlConnection))
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
						granted[(int) (decimal) reader["id"]] = (bool) reader["granted"];
					}
				}
			}

			return granted;
		}

		public static string GetSecurityPathSql(List<RelationSecurityPathItem> items, int contentId)
		{
			if (items == null)
				throw new ArgumentNullException("Invalid security path");
			var sqlBuilder = new StringBuilder();
			var selectBuilder = new StringBuilder();

			var lastItem = items.Last();

			var selects = Enumerable.Repeat<RelationSecurityPathItem>(lastItem, 1).
				Concat(lastItem.Secondary).
				Select(n => GetSelectExpression(n));

			sqlBuilder.Append(string.Format("select c0.content_item_id as id, {0} from content_{1}_united c0 ",
				string.Join(", ", selects), contentId));

			foreach (var item in items)
			{
				if (item != lastItem)
				{
					if (item.LinkId.HasValue)
						throw new ApplicationException("Invalid security path settings: m2m");
					else if (item.Secondary != null && item.Secondary.Any())
						throw new ApplicationException("Invalid security path settings: secondary");
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
				return String.Format("c{0}.[{1}]", n.JoinOrder, n.AttributeName);
			if (n.LinkId.HasValue)
				return string.Format("dbo.qp_link_ids({0}, c{1}.CONTENT_ITEM_ID, 1) as '{2}'", n.LinkId, n.JoinOrder, n.RelContentId);
			else
				return string.Format("cast(c{0}.content_item_id as nvarchar(30)) as '{1}'", n.Order, n.RelContentId);
		}

		public static Dictionary<int, bool> CheckLockedBy(SqlConnection dbConnection, int[] ids, int currentUserId,
			bool forceUnlock)
		{
			string sql =
				@"select locked_by, content_item_id from content_item ci with(nolock) 
				inner join @ids i on i.id = ci.content_item_id where locked_by is not null and locked_by <> @userId ";

			var result = ids.ToDictionary(n => n, k => true);

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
							var id = (int) (decimal) reader["content_item_id"];
							result[id] = false;
						}
					}

				}
			}

			return result;


		}
		public static RelationSecurityInfo GetRelationSecurityInfo(SqlConnection dbConnection,
			int contentId, int[] ids)
		{
			var result = new RelationSecurityInfo();
			var pathRows = GetRelationSecurityFields(dbConnection);
			var securityPath = FindRelationSecurityPath(pathRows, contentId);

			if (securityPath.Count <= 0)
			{
				var isEndNode = pathRows.Any(n => (int)(n.Field<decimal?>("rel_content_id") ?? 0) == contentId);
				if (!isEndNode)
					result.MakeEmpty();
				else
					result.AddContentInItemMapping(contentId, ids.ToDictionary(n => n, m => Enumerable.Repeat(m, 1).ToArray()));
				return result;
			}

			var lastItem = securityPath.Last();
			var lastItemWithSecondary = Enumerable.Repeat(lastItem, 1).Concat(lastItem.Secondary);
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
			var id = (int) (decimal) reader["id"];
			foreach (var item in contentIds)
			{
				var ints = Converter.ToInt32Collection((string) reader[item.ToString()], ',');
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

		public class RelationSecurityPathItem
		{

			public RelationSecurityPathItem()
			{
			}

			public RelationSecurityPathItem(DataRow row)
			{
				AttributeName = row.Field<string>("attribute_name");
				AggAttributeName = row.Field<string>("agg_attribute_name");
				AttributeId = (int) row.Field<decimal>("attribute_id");
				ContentId = (int) row.Field<decimal>("content_id");
				RelContentId = (int?) row.Field<decimal?>("rel_content_id") ?? 0;
				LinkId = (int?) row.Field<decimal?>("link_id");
				IsClassifier = row.Field<bool>("is_classifier");
			}

			public int ContentId { get; set; }

			public int RelContentId { get; set; }

			public string AttributeName { get; set; }

			public string AggAttributeName { get; set; }

			public int AttributeId { get; set; }

			public int? LinkId { get; set; }

			public int Order { get; set; }

			public int JoinOrder { get; set; }

			public bool IsClassifier { get; set; }

			public RelationSecurityPathItem[] Extensions { get; set; }

			public RelationSecurityPathItem[] Secondary { get; set; }

		}

		public class RelationSecurityInfo
		{
			public RelationSecurityInfo()
			{
				Data = new Dictionary<int, Dictionary<int, int[]>>();
				ContentData = new Dictionary<int, int>();
			}

			private Dictionary<int, Dictionary<int, int[]>> Data { get; set; }

			private Dictionary<int, int> ContentData { get; set; } 

			public IEnumerable<int> ContentIds
			{
				get { return Data.Keys; }
			}

			public Dictionary<int, int[]> GetItemMapping(int contentId)
			{
				Dictionary<int, int[]> result;
				if (Data.TryGetValue(contentId, out result))
					return result;
				else
					throw new ApplicationException("Security mapping not exists:" + contentId);
			}

			public bool IsEmpty
			{
				get
				{
					return Data == null;
				}
			}

			public void AddContentInItemMapping(int contentId, Dictionary<int, int[]> initValues)
			{
				Data.Add(contentId, initValues);
			}

			internal void AppendToItemMapping(int contentId, int id, int[] ids)
			{

				if (!Data[contentId].ContainsKey(id))
				{
					Data[contentId].Add(id, ids);
				}
				else
				{
					Data[contentId][id] = Data[contentId][id].Concat(ids).ToArray();
				}
			}

			public bool IsItemMappingExists(int contentId, int id)
			{
				return Data[contentId].ContainsKey(id);
			}

			public void MakeEmpty()
			{
				Data = null;
			}

			public void AppendToContentMapping(int contentId, int id)
			{
				ContentData.Add(id, contentId);
			}

			public int[] GetContentIdsFromContentMapping()
			{
				return ContentData.Select(n => n.Value).Distinct().ToArray();
			}

			public Dictionary<int, int> GetContentMapping()
			{
				return ContentData;
			} 
		}
	}
}