using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Quantumart.QP8.DAL
{
    public static class Common
    {
        public static long GetContentIdForArticle(SqlConnection connection, long id)
        {
            using (var cmd = SqlCommandFactory.Create("select content_id from content_item with(nolock) where content_item_id = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return (value == null) ? 0 : long.Parse(value.ToString());
            }
        }

        public static string GetTitleName(SqlConnection connection, long contentId)
        {
            using (var cmd = SqlCommandFactory.Create("select dbo.qp_get_display_field(@id, 1)", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", contentId);
                return cmd.ExecuteScalar().ToString();
            }
        }

        public static DataTable GetDisplayFields(SqlConnection connection, long contentId, bool withRelations = false)
        {
            using (var cmd = SqlCommandFactory.Create("SELECT * FROM [dbo].[qp_get_display_fields](@contentId, @with_relation_field)", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.AddWithValue("@with_relation_field", withRelations);
                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);
                return (0 == ds.Tables.Count) ? null : ds.Tables[0];
            }
        }


        public static IEnumerable<DataRow> GetArticleTitleList(SqlConnection connection, long contentId,
            string titleName, string filterIds)
        {
            if (!string.IsNullOrWhiteSpace(filterIds))
            {
                filterIds = "," + filterIds;
            }

            using (var cmd = SqlCommandFactory.Create($"select [{titleName}] as title, content_item_id as id from content_{contentId}_united with(nolock) where content_item_id in (0 {filterIds}) order by content_item_id ", connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static string GetFieldName(SqlConnection connection, int fieldId)
        {
            using (var cmd = SqlCommandFactory.Create("select ATTRIBUTE_NAME from CONTENT_ATTRIBUTE WHERE ATTRIBUTE_ID = @fieldId", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string[] GetArticleFieldValues(SqlConnection connection, int[] ids, int contentId, string name)
        {
            var values = new List<string>();
            using (var cmd = SqlCommandFactory.Create($"select [{name}] from content_{contentId}_united with(nolock) join @ids ON CONTENT_ITEM_ID = Id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(ids)
                });

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        values.Add(dr[0].ToString());
                    }
                }
            }

            return values.ToArray();
        }

        public static string GetArticleFieldValue(SqlConnection connection, int id, int contentId, string name)
        {
            using (var cmd = SqlCommandFactory.Create($"select [{name}] from content_{contentId}_united with(nolock) where content_item_id = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return value?.ToString() ?? string.Empty;
            }
        }

        public static int GetArticleIdByFieldValue(SqlConnection connection, int contentId, string name, string value)
        {
            using (var cmd = SqlCommandFactory.Create($"select content_item_id from content_{contentId}_united with(nolock) where [{name}] = @value", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@value", value);
                var result = cmd.ExecuteScalar();
                return (result == null) ? 0 : (int)(decimal)result;
            }
        }

        public static DataRow GetArticleRow(SqlConnection connection, int id, int contentId, bool isLive)
        {
            var suffix = (isLive) ? "" : "_united";
            using (var cmd = SqlCommandFactory.Create($"select * from content_{contentId}{suffix} with(nolock) where content_item_id = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return (0 == dt.Rows.Count) ? null : dt.Rows[0];
            }
        }

        public static void LockArticleForUpdate(SqlConnection cnn, int id)
        {
            using (var cmd = SqlCommandFactory.Create("select content_item_id from content_item with(rowlock, updlock) where content_item_id = @id", cnn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
            }
        }

        public static DataTable GetArticleTable(SqlConnection connection, IEnumerable<int> ids, int contentId, bool isLive)
        {
            var suffix = (isLive) ? "" : "_united";
            var sql = $"select c.*, ci.locked_by, ci.splitted, ci.schedule_new_version_publication from content_{contentId}{suffix} c with(nolock) left join content_item ci with(nolock) on c.content_item_id = ci.content_item_id ";
            if (ids != null)
            {
                sql = sql + "where c.content_item_id in (select id from @itemIds)";
            }

            if (ids != null && !isLive) //optimization for list of ids
            {
                const string baseSql = "select c.*, ci.locked_by, ci.splitted, ci.schedule_new_version_publication from content_{0}{1} c with(nolock) left join content_item ci with(nolock) on c.content_item_id = ci.content_item_id where c.content_item_id in (select id from @itemIds) and {2}";
                var sb = new StringBuilder();
                sb.AppendLine(string.Format(baseSql, contentId, "", " isnull(ci.splitted, 0) = 0 "));
                sb.AppendLine(" union all ");
                sb.AppendLine(string.Format(baseSql, contentId, "_async", " ci.splitted = 1 "));
                sql = sb.ToString();
            }
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                if (ids != null)
                    cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(ids)
                    });

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return (0 == dt.Rows.Count) ? null : dt;
            }
        }

        public static DataRow GetDefaultArticleRow(SqlConnection connection, int contentId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_default_article", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);
                return 0 == ds.Tables.Count || 0 == ds.Tables[0].Rows.Count ? null : ds.Tables[0].Rows[0];
            }
        }

        public static IList<DataRow> GetArticlesSimpleList(
            SqlConnection cn,
            int userId,
            int contentId,
            string displayExpression,
            ListSelectionMode selectionMode,
            int permissionLevel,
            string filter,
            bool useSecurity,
            IList<int> selectedArticleIDs,
            IList<int> idsToFilter,
            string extraSelect = "",
            string extraFrom = "",
            string orderBy = "")
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendFormatLine(" select c.content_item_id as id, {0}, cast(case WHEN (cis.content_item_id IS NOT NULL) THEN 1 ELSE 0 END as bit) as is_selected ", displayExpression);
            queryBuilder.AppendLine(extraSelect ?? string.Empty);
            queryBuilder.AppendFormatLine(" from content_{0}_united c ", contentId);

            if (useSecurity)
            {
                var securitySql = GetPermittedItemsAsQuery(cn, userId, 0, PermissionLevel.List, PermissionLevel.FullAccess, EntityTypeCode.OldArticle, EntityTypeCode.Content, contentId);
                queryBuilder.AppendFormatLine(" inner join ({0}) as pi on c.content_item_id = pi.content_item_id ", securitySql);
            }

            queryBuilder.Append(selectionMode == ListSelectionMode.AllItems ? " left join " : " inner join ");
            queryBuilder.AppendFormatLine("( select content_item_id from content_{0} where content_item_id IN (select id from @myData)) as cis on c.content_item_id = cis.content_item_id ", contentId);
            queryBuilder.AppendLine(extraFrom ?? string.Empty);
            queryBuilder.AppendLine(string.IsNullOrWhiteSpace(filter) ? string.Empty : $" where {filter}");

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? "c.content_item_id asc" : orderBy;
            queryBuilder.AppendLine($" order by {orderBy}");

            return GetDatatableResult(cn, queryBuilder, GetIdsDatatableParam("@ids", idsToFilter), GetIdsDatatableParam("@myData", selectedArticleIDs));
        }

        public static void ExecuteSql(SqlConnection connection, string sqlString, List<SqlParameter> parameters, string returnIdParamName, out int id)
        {
            id = 0;
            using (var cmd = SqlCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                cmd.ExecuteNonQuery();
                if (!string.IsNullOrEmpty(returnIdParamName))
                {
                    id = (int)cmd.Parameters[returnIdParamName].Value;
                }
            }
        }

        public static void ExecuteSql(SqlConnection connection, string sqlString)
        {
            using (var cmd = SqlCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static DataRow GetArticleVersionRow(SqlConnection connection, int id, int versionId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_versions", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@item_id", id);
                cmd.Parameters.AddWithValue("@version_id", versionId);
                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);
                return (0 == ds.Tables.Count || 0 == ds.Tables[0].Rows.Count) ? null : ds.Tables[0].Rows[0];
            }
        }


        public static void CreateArticleVersion(SqlConnection connection, int userId, int id)
        {
            using (var cmd = SqlCommandFactory.Create("create_content_item_version", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@content_item_id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RestoreArticleVersion(SqlConnection connection, int userId, int id)
        {
            using (var cmd = SqlCommandFactory.Create("restore_content_item_version", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@version_id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool CheckUnique(SqlConnection connection, string code, string name, int id, int parentId = 0,
            int? recurringId = null)
        {
            using (var cmd = SqlCommandFactory.Create("qp_is_entity_exists", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@parent_id", parentId);
                cmd.Parameters.AddWithValue("@recurring_id", (recurringId == null) ? 0 : (int)recurringId);
                return (bool)cmd.ExecuteScalar();
            }
        }

        public static int CountDuplicates(SqlConnection connection, int contentId, string fieldIds, string itemIds)
        {
            using (var cmd = SqlCommandFactory.Create("qp_count_duplicates", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@field_ids", fieldIds);
                cmd.Parameters.AddWithValue("@ids", itemIds);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static DateTime? Lock(SqlConnection connection, string code, int id, int? userId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_lock", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@user_id", userId);
                var result = cmd.ExecuteScalar();
                return (result == DBNull.Value) ? null : (DateTime?)result;
            }
        }

        public static void UpdatePassword(SqlConnection connection, int userId, string password)
        {
            using (
                var cmd = SqlCommandFactory.Create("update users set password = @password where user_id = @user_id",
                    connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetArchiveFlag(SqlConnection connection, IEnumerable<int> articleIDs, int userId, bool flag,
            bool withAggregated)
        {
            var source = withAggregated ? "dbo.qp_aggregated_and_self(@ids)" : "@ids";
            using (var cmd = SqlCommandFactory.Create(
                string.Format(
                    "update content_item with(rowlock) set archive = @flag, modified = getdate(), last_modified_by = @userId where content_item_id in (select id from {0});"
                    +
                    " update content_item with(rowlock) set locked_by = null, locked = null where content_item_id in (select id from {0});",
                    source),
                connection
                ))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@flag", flag);
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(articleIDs)
                });
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Массовая публикация (может использоваться для статей из разных контентов одного сайта)
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="articleIDs"></param>
        /// <param name="userId"></param>
        /// <param name="statusTypeId"></param>
        public static void Publish(SqlConnection connection, IEnumerable<int> articleIDs, int userId,
            bool withAggregated)
        {
            var source = withAggregated ? "dbo.qp_aggregated_and_self(@ids)" : "@ids";
            using (var cmd = SqlCommandFactory.Create(string.Format(@"
                    declare @ids2 table (id numeric primary key)
                    insert into @ids2
                    select id from {0}

                    declare @statusTypeId numeric
                    select @statusTypeId = status_type_id from status_type where status_type_name = 'Published' and site_id in (select site_id from content c inner join content_item ci with(nolock) on c.content_id = ci.content_id inner join @ids2 i on i.id = ci.content_item_id )

                    update content_item with(rowlock) set status_type_id = @statusTypeId, modified = getdate(), last_modified_by = @userId where content_item_id in (select id from @ids2) and status_type_id <> @statusTypeId and splitted = 0;
                    update content_item with(rowlock) set status_type_id = @statusTypeId, modified = getdate(), last_modified_by = @userId, schedule_new_version_publication = 1 where content_item_id in (select id from @ids2) and status_type_id <> @statusTypeId and splitted = 1;
                    delete i from @ids2 i inner join content_item ci with(nolock) on ci.content_item_id = i.id where ci.splitted = 0 and ci.schedule_new_version_publication = 0
                    declare @id numeric
                    while exists (select id from @ids2)
                    begin
                        select @id = id from @ids2
                        exec qp_merge_article @id, @userId
                        delete from @ids2 where id = @id
                    end", source), connection
                ))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(articleIDs)
                });
                cmd.ExecuteNonQuery();
            }
        }

        public static string GetConflictIds(SqlConnection connection, int id, int contentId, string condition,
            List<FieldParameter> parameters)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "SELECT CONTENT_ITEM_ID FROM CONTENT_{0}_UNITED WHERE {1} AND CONTENT_ITEM_ID <> @id",
                            contentId, condition), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                foreach (var parameter in parameters)
                    cmd.Parameters.Add(CreateDbParameter(parameter));
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaList(dt, "CONTENT_ITEM_ID");
            }
        }

        private static string IdCommaList(DataTable dt, string fieldName, bool withSpace = true)
        {
            IEnumerable<string> ids = dt.AsEnumerable().Select(row => row[fieldName].ToString());
            return (withSpace) ? string.Join(", ", ids) : string.Join(",", ids);
        }

        private static Dictionary<int, string> IdCommaListDictionary(DataTable dt, string keyFieldName,
            string valueFieldName, bool withSpace = true)
        {
            var result = new Dictionary<int, List<string>>();
            var data =
                dt.AsEnumerable()
                    .Select(
                        row =>
                            new { Id = (int)(decimal)row[keyFieldName], LinkedId = (int)(decimal)row[valueFieldName] });
            foreach (var item in data)
            {
                if (!result.ContainsKey(item.Id))
                    result.Add(item.Id, new List<string> { item.LinkedId.ToString() });
                else
                    result[item.Id].Add(item.LinkedId.ToString());
            }

            return result.Select(
                n => new KeyValuePair<int, string>(n.Key, string.Join((withSpace) ? ", " : ",", n.Value)))
                .ToDictionary(n => n.Key, n => n.Value);
        }

        private static SqlParameter CreateDbParameter(FieldParameter item)
        {
            var param = new SqlParameter();
            param.ParameterName = item.Name;
            param.DbType = item.DbType;
            param.Value = (!string.IsNullOrEmpty(item.Value)) ? (object)item.Value : DBNull.Value;
            return param;
        }

        public static string GetLinkedArticles(SqlConnection connection, int linkId, int id, bool isLive)
        {
            var suffix = (isLive) ? "" : "_united";
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "select linked_item_id, item_id from item_link{0} with(nolock) where item_id = @id and link_id = @lid",
                            suffix), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@lid", linkId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaList(dt, "linked_item_id", false);
            }
        }

        public static Dictionary<int, string> GetLinkedArticlesMultiple(SqlConnection connection, int linkId,
            IEnumerable<int> ids, bool isLive)
        {
            var sql =
                " select ii.r_item_id as linked_item_id, ii.l_item_id as item_id from @itemIds i inner join item_to_item ii with(nolock, index(ix_l_item_id)) on ii.l_item_id = i.id where link_id = @lid";
            if (!isLive) // optimization for list
            {
                var sb = new StringBuilder();
                sb.AppendLine(sql);
                sb.AppendLine(
                    " and not exists (select * from content_item_splitted cis where cis.content_item_id = ii.l_item_id) ");
                sb.AppendLine(" union all ");
                sb.AppendLine(
                    " select il.linked_item_id, il.item_id from @itemIds i inner join item_link_async il with(nolock) on il.item_id = i.id where link_id = @lid");
                sql = sb.ToString();
            }
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(ids)
                });
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@lid", linkId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaListDictionary(dt, "item_id", "linked_item_id", false);
            }
        }

        public static string GetRelatedArticles(SqlConnection connection, int contentId, string fieldName, int? id,
            bool isLive)
        {
            var suffix = (isLive) ? "" : "_united";
            var action = (id.HasValue) ? " = @id" : " is null ";
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format("select content_item_id from content_{1}{3} with(nolock) where [{0}] {2}",
                            fieldName, contentId, action, suffix), connection))

            {
                cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Decimal)
                {
                    Value = (id == null) ? DBNull.Value : (object)id.Value
                });
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaList(dt, "content_item_id", false);
            }
        }

        public static int[] ExcludeArchived(SqlConnection connection, int[] ids)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "select content_item_id from content_item with(nolock) where content_item_id in (select id from @itemIds) and archive = 0",
                        connection))
            {
                cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(ids)
                });
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().Select(row => (int)(decimal)row["content_item_id"]).ToArray();
            }

        }

        public static Dictionary<int, string> GetRelatedArticlesMultiple(SqlConnection connection, int contentId,
            string fieldName, IEnumerable<int> ids, bool isLive)
        {
            var suffix = (isLive) ? "" : "_united";
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "select content_item_id as linked_item_id, [{0}] as item_id from content_{1}{2} with(nolock) where [{0}] in (select id from @itemIds)",
                            fieldName, contentId, suffix), connection))
            {
                cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(ids)
                });
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaListDictionary(dt, "item_id", "linked_item_id", false);
            }
        }

        public static Dictionary<string, List<string>> GetM2OValuesBatch(SqlConnection connection, int contentId,
            int fieldId, string fieldName, IEnumerable<int> ids, string displayFieldName, int maxNumberOfRecords)
        {
            var result = new Dictionary<string, List<string>>();
            if (ids.Count() > 0)
            {
                var query = new StringBuilder();
                query.AppendFormatLine(" select subsel.content_item_id, {0}, Title from ( ", fieldName);
                query.AppendFormatLine(
                    " select u.content_item_id, u.{0}, CONVERT(NVARCHAR(255),[{1}]) as Title, ROW_NUMBER () over (PARTITION BY {0} order by u.content_item_id) AS [RowNum]",
                    fieldName, displayFieldName);
                query.AppendFormatLine(" from content_{0}_united as u with(nolock) where {2} in ({1})) as subsel ",
                    contentId, string.Join(",", ids), fieldName);
                query.AppendFormatLine(" where subsel.[RowNum] <= {0}", maxNumberOfRecords + 1);

                using (var cmd = SqlCommandFactory.Create(query.ToString(), connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = reader.GetDecimal(1) + "_" + fieldId;
                            var valueToInsert = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            if (result.ContainsKey(key))
                            {
                                result[key].Add(valueToInsert);
                            }
                            else
                                result.Add(key, new List<string> { valueToInsert });
                        }
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, List<string>> GetM2MValuesBatch(SqlConnection sqlConnection,
            IEnumerable<int> ids, int linkId, int maxNumberOfRecords, string displayFieldName, int contentId)
        {
            var result = new Dictionary<string, List<string>>();
            if (ids.Count() > 0)
            {
                var query =
                    string.Format(
                        "SELECT [item_id], [linked_item_id], link_id, title FROM( SELECT [item_id], [linked_item_id], link_id, CONVERT (NVARCHAR(255), [{3}]) as title, "
                        + " ROW_NUMBER() over (partition by item_id "
                        +
                        "order by linked_item_id) AS [RowNum] from [item_link_united] as u with(nolock) inner join content_{4}_united as c with(nolock) on u.linked_item_id = c.CONTENT_ITEM_ID"
                        + " where item_id in ({0}) and link_id = {2}) as subq where subq.RowNum <= {1} ",
                        string.Join(",", ids), maxNumberOfRecords + 1, linkId, displayFieldName, contentId);
                using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = Converter.ToString(reader.GetDecimal(0)) + "_" +
                                      Converter.ToString(reader.GetDecimal(2));
                            var valueToInsert = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            if (result.ContainsKey(key))
                            {
                                result[key].Add(valueToInsert);
                            }
                            else
                            {
                                result.Add(key, new List<string> { valueToInsert });
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static string GetLinkedArticlesForVersion(SqlConnection connection, int fieldId, int versionId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "select linked_item_id from item_to_item_version where attribute_id = @attributeId and content_item_version_id = @versionId",
                        connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attributeId", fieldId);
                cmd.Parameters.AddWithValue("@versionId", versionId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return IdCommaList(dt, "linked_item_id", false);
            }
        }

        public static void RemoveLinkVersions(SqlConnection connection, int fieldId)
        {
            using (
                var cmd = SqlCommandFactory.Create("delete from item_to_item_version where attribute_id = @fid",
                    connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static DateTime GetSqlDate(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("select getdate() as date", connection))
            {
                cmd.CommandType = CommandType.Text;
                return (DateTime)cmd.ExecuteScalar();
            }
        }

        public static int GetBaseFieldId(SqlConnection connection, int fieldId, int articleId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_base_field", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@field_id", fieldId);
                cmd.Parameters.AddWithValue("@article_id", articleId);
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return (int)dr.GetDecimal(0);
                    return fieldId;
                }
            }
        }

        public static int GetSelfRelationFieldId(SqlConnection connection, int contentId)
        {
            using (
                var cmd = SqlCommandFactory.Create("select dbo.qp_get_self_relation_field_id(@contentId)", connection))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                var result = cmd.ExecuteScalar();
                return (result == DBNull.Value) ? 0 : (int)(decimal)result;
            }
        }

        public static int? GetParentEntityId(SqlConnection connection, string entityTypeCode, int entityId)
        {
            int? parentEntityId = null;
            using (var cmd = SqlCommandFactory.Create("qp_get_parent_entity_id", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@entity_type_code", entityTypeCode);
                cmd.Parameters.AddWithValue("@entity_id", entityId);
                cmd.Parameters.Add(new SqlParameter("@parent_entity_id", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                });
                cmd.ExecuteNonQuery();
                parentEntityId = Converter.ToNullableInt32(cmd.Parameters["@parent_entity_id"].Value);
            }
            return parentEntityId;
        }

        public static int[] GetParentEntityIdsForTree(SqlConnection connection, string entityTypeCode, int[] ids)
        {
            var result = new List<int>();
            using (var cmd = SqlCommandFactory.Create("qp_get_parent_entity_ids_for_tree", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@entityTypeCode", entityTypeCode);
                cmd.Parameters.AddWithValue("@entityIds", string.Join(",", ids));
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        result.Add((int)dr.GetDecimal(0));
                }
            }
            return result.ToArray();
        }

        public static void AdjustManyToMany(SqlConnection connection, int id, int newId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "update item_to_item with(rowlock) set l_item_id = @newId where l_item_id = @id and r_item_id = @newId;delete from item_to_item with(rowlock) where r_item_id = @id and l_item_id = @newId",
                        connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@newId", newId);
                cmd.ExecuteNonQuery();
            }
        }

        private static readonly string GET_TRANSLATIONS_QUERY =
            @"select p.PHRASE_ID, p.PHRASE_TEXT, t.LANGUAGE_ID, t.PHRASE_TRANSLATION from translations t inner join phrases p on t.phrase_id = p.phrase_id";

        public static IEnumerable<DataRow> GetTranslations(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create(GET_TRANSLATIONS_QUERY, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public class FieldParameter
        {
            public string Name { get; set; }

            public DbType DbType { get; set; }

            public string Value { get; set; }
        }

        public static DataTable GetBreadCrumbsList(SqlConnection connection, int userId, string entityTypeCode,
            long entityId, long? parentEntityId, bool oneLevel)
        {
            DataTable dt = null;

            using (var cmd = SqlCommandFactory.Create("qp_get_breadcrumbs", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@entity_type_code", entityTypeCode);
                cmd.Parameters.AddWithValue("@entity_id", entityId);
                if (parentEntityId != null)
                {
                    cmd.Parameters.AddWithValue("@parent_entity_id", parentEntityId);
                }
                cmd.Parameters.AddWithValue("@one_level", oneLevel);

                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);
                if (ds.Tables.Count > 0)
                    dt = ds.Tables[0];
            }

            return dt;
        }

        public static bool IsAdmin(SqlConnection connection, int userId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "select count(*) from user_group_bind where group_id = {0} and user_id = @userId",
                            SpecialIds.AdminGroupId), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return ((int)cmd.ExecuteScalar() > 0);
            }
        }

        public static bool CanUnlockItems(SqlConnection connection, int userId)
        {
            var sql = @"
                WITH
                  cte (group_id, parent_group_id, can_unlock_items)
                  AS
                  (
                    SELECT ug.group_id, ug.parent_group_id, ug.can_unlock_items
                    FROM user_group_tree ug
                    inner join USER_GROUP_BIND ugb on ug.GROUP_ID = ugb.GROUP_ID
                    WHERE ugb.USER_ID = @userId
                    UNION ALL
                    SELECT ug2.group_id, ug2.parent_group_id, ug2.can_unlock_items
                    FROM user_group_tree ug2
                    INNER JOIN cte r ON ug2.group_id = r.parent_group_id
                  )
                    select count(*) from cte where can_unlock_items = 1
            ";
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return ((int)cmd.ExecuteScalar() > 0);
            }
        }

        public static int CountArticles(SqlConnection connection, int contentId, bool includeArchive)
        {
            var sql = "select count(*) from content_" + contentId + " with(nolock)";
            if (!includeArchive)
                sql = sql + " where archive = 0";
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                return ((int)cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для пользователя по entity_type_code и action_type_code
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <param name="actionCode"></param>
        /// <returns></returns>
        public static bool IsEntityAccessible(SqlConnection connection, int userId, string entityTypeCode, int entityId,
            string actionTypeCode)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "select dbo.qp_is_entity_action_type_accessible(@userId, 0, @entityTypeCode, @entityId, @actionTypeCode)",
                        connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@entityTypeCode", entityTypeCode);
                cmd.Parameters.AddWithValue("@entityId", entityId);
                cmd.Parameters.AddWithValue("@actionTypeCode", actionTypeCode);
                return (bool)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для пользователя по entity_type_code и action_type_code
        /// </summary>
        public static bool IsEntityAccessibleForUserGroup(SqlConnection connection, int groupId, string entityTypeCode, int entityId, string actionTypeCode)
        {
            using (var cmd = SqlCommandFactory.Create("select dbo.qp_is_entity_action_type_accessible(0, @groupId, @entityTypeCode, @entityId, @actionTypeCode)", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@groupId", groupId);
                cmd.Parameters.AddWithValue("@entityTypeCode", entityTypeCode);
                cmd.Parameters.AddWithValue("@entityId", entityId);
                cmd.Parameters.AddWithValue("@actionTypeCode", actionTypeCode);
                return (bool)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Получение максимального веса, доступного пользователю в цепочке workflow
        /// </summary>
        public static int GetMaxUserWeight(SqlConnection connection, int userId, int workflowId)
        {
            var sql = @"SELECT dbo.qp_get_user_weight(@userId, @workflowId)";
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@workflowId", workflowId);
                var value = cmd.ExecuteScalar();
                return (value == DBNull.Value) ? 0 : (int)(decimal)value;
            }
        }

        public static void ChangeTriggerState(SqlConnection connection, string triggerName, bool enable)
        {
            var tableName = "#disable_" + triggerName;
            var opString = enable ? "drop" : "create";
            var signatureString = enable ? "" : "(id numeric)";

            var checkString = enable
                ? string.Format("if object_id('tempdb..{0}') is not null", tableName)
                : string.Format("if object_id('tempdb..{0}') is null", tableName);
            var sql = string.Format("{3} {0} table {1} {2}", opString, tableName, signatureString, checkString);
            using (var cmd = SqlCommandFactory.Create(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentAccess(SqlConnection connection, int sourceId, int destinationId, int userId)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat("DELETE FROM content_access WHERE content_id = {0};", destinationId);
            sqlBuilder.Append("INSERT INTO content_access");
            sqlBuilder.AppendFormat(" SELECT {0}, user_id, group_id, permission_level_id, GETDATE(), GETDATE(), {1}, propagate_to_items, hide FROM content_access", destinationId, userId);
            sqlBuilder.AppendFormat(" WHERE content_id = {0}", sourceId);
            using (var cmd = SqlCommandFactory.Create(sqlBuilder.ToString(), connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyArticleAccess(SqlConnection sqlConnection, int sourceId, int destinationId, int userId)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat("DELETE FROM content_item_access WHERE content_item_id = {0};", sourceId);
            sqlBuilder.Append("INSERT INTO content_item_access");
            sqlBuilder.AppendFormat(
                " SELECT {0}, user_id, group_id, permission_level_id, GETDATE(), GETDATE(), {1} FROM content_item_access",
                destinationId, userId);
            sqlBuilder.AppendFormat(" WHERE content_item_id = {0}", sourceId);
            using (var cmd = SqlCommandFactory.Create(sqlBuilder.ToString(), sqlConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentFieldsOrder(SqlConnection connection, int currentOrder, int newOrder,
            int contentId)
        {
            if (currentOrder != newOrder)
            {
                var sqlBuilder = new StringBuilder("update [CONTENT_ATTRIBUTE] ");
                if (currentOrder <= 0)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1");
                    sqlBuilder.AppendFormat(" where [ATTRIBUTE_ORDER] > {0}", newOrder);
                }
                else if (currentOrder < newOrder)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER - 1 ");
                    sqlBuilder.AppendFormat(" where [ATTRIBUTE_ORDER] > {0} and [ATTRIBUTE_ORDER] <= {1}", currentOrder,
                        newOrder);
                }
                else if (currentOrder > newOrder)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1");
                    sqlBuilder.AppendFormat(" where [ATTRIBUTE_ORDER] < {0} and [ATTRIBUTE_ORDER] > {1}", currentOrder,
                        newOrder);
                }
                sqlBuilder.AppendFormat(" and CONTENT_ID = {0}", contentId);

                using (var cmd = SqlCommandFactory.Create(sqlBuilder.ToString(), connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int GetStringFieldMaxLength(SqlConnection connection, int contentId, string fieldName)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format("select MAX(LEN([{1}])) from content_{0}_united with(nolock)", contentId,
                            fieldName), connection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                var nullableResult = Converter.ToNullableInt32(objResult);
                return nullableResult.HasValue ? nullableResult.Value : 0;
            }
        }

        public static int GetBlobFieldMaxSymbolLength(SqlConnection connection, int contentId, string fieldName)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format("select MAX(DATALENGTH([{1}])) / 2 from content_{0}_united with(nolock)",
                            contentId, fieldName), connection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                var nullableResult = Converter.ToNullableInt32(objResult);
                return nullableResult.HasValue ? nullableResult.Value : 0;
            }
        }

        public static int? GetNumericFieldMaxValue(SqlConnection connection, int contentId, string fieldName)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format("select MAX([{1}]) from content_{0}_united with(nolock)", contentId, fieldName),
                        connection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                return Converter.ToNullableInt32(objResult);
            }
        }

        /// <summary>
        /// Сужествуют ли значения numeric поля, которые нельзя использовать как ключ для связи O2M
        /// </summary>
        /// <returns>True - нет "плохих" значений</returns>
        public static bool CheckNumericValuesAsO2MForeingKey(SqlConnection connection, int contentId, string fieldName,
            int relatedContentId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "select 1 where EXISTS (select C1.CONTENT_ITEM_ID from content_{0}_united C1 with(nolock) join content_{2}_united C2 with(nolock) ON C1.[{1}] != C2.[CONTENT_ITEM_ID])",
                            contentId, fieldName, relatedContentId), connection))
            {
                cmd.CommandType = CommandType.Text;
                var obj = cmd.ExecuteScalar();
                return obj == null;
            }
        }

        /// <summary>
        /// Существуют ли множественные связи по данному полю.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        public static bool DoPluralLinksExist(SqlConnection connection, int contentId, int linkId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        string.Format(
                            "select 1 where exists(select item_id from item_link_united i inner join content_{0}_united c with(nolock) on i.item_id = c.content_item_id where link_id = @lid group by item_id having COUNT(linked_item_id) > 1)",
                            contentId), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@lid", linkId);
                var obj = cmd.ExecuteScalar();
                return obj != null;
            }
        }

        /// <summary>
        /// Переключает RelationId равный currentRelationFieldId на значение newRelationFieldId
        /// </summary>
        public static void ChangeRelationIdToNewOne(SqlConnection сonnection, int currentRelationFieldId,
            int newRelationFieldId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "update CONTENT_ATTRIBUTE set RELATED_ATTRIBUTE_ID = @newid where RELATED_ATTRIBUTE_ID = @crnid",
                        сonnection))
            {
                cmd.Parameters.AddWithValue("@newid", newRelationFieldId);
                cmd.Parameters.AddWithValue("@crnid", currentRelationFieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ClearEmailField(SqlConnection сonnection, int fieldId)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "UPDATE NOTIFICATIONS set email_attribute_id = null where email_attribute_id = @fid", сonnection)
                )
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Трансформирует даные статей при изменении типа поля с O2M в M2M
        /// </summary>
        /// <param name="p"></param>
        /// <param name="nullable"></param>
        public static void O2MToM2MTranferData(SqlConnection сonnection, int fieldId, int linkId)
        {
            // 1. перенести данные о связях из CONTENT_DATA в item_to_item.
            // 2. В CONTENT_DATA в качестве значения поля для  всех статей установить LinkId
            var cmdText =
                "INSERT INTO [item_to_item] ([link_id],[l_item_id],[r_item_id]) select @linkid as link_id, D.CONTENT_ITEM_ID as l_item_id, D.DATA as r_item_id from CONTENT_DATA D where ATTRIBUTE_ID = @fid and D.DATA is not null; " +
                "update CONTENT_DATA set DATA = @linkid where ATTRIBUTE_ID = @fid;";
            using (var cmd = SqlCommandFactory.Create(cmdText, сonnection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@linkid", linkId);
                cmd.ExecuteNonQuery();
            }

            //
        }

        public static void M2MToO2MTranferData(SqlConnection сonnection, int fieldId, int linkId)
        {
            // перенести данные о связях из item_to_item в CONTENT_DATA.
            //
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "update CONTENT_DATA SET DATA = LD.linked_item_id from dbo.item_link_united LD where LD.item_id = CONTENT_DATA.CONTENT_ITEM_ID and CONTENT_DATA.ATTRIBUTE_ID = @fid and LD.link_id = @linkid",
                        сonnection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@linkid", linkId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Меняет данные, хранимые в CONTENT_DATA для полей M2M или M2O
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="linkId"></param>
        public static void ChangeContentDataForRelation(SqlConnection сonnection, int fieldId, int newId)
        {
            var cmdText =
                "update content_data set data = @new_id where data is not null and attribute_id = @attribute_id";
            using (var cmd = SqlCommandFactory.Create(cmdText, сonnection))
            {
                cmd.Parameters.AddWithValue("@attribute_id", fieldId);
                cmd.Parameters.AddWithValue("@new_id", newId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Обновить Order поля
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="newOrder"></param>
        public static void UpdateFieldOrder(SqlConnection сonnection, int fieldId, int newOrder)
        {
            using (
                var cmd =
                    SqlCommandFactory.Create(
                        "update CONTENT_ATTRIBUTE set ATTRIBUTE_ORDER = @ord where ATTRIBUTE_ID = @fid", сonnection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@ord", newOrder);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Возвращает данные о виртуальном поле
        /// </summary>
        public static DataTable GetVirtualFieldData(SqlConnection сonnection, int contentId)
        {
            var query = "SELECT " +
                        "a.ATTRIBUTE_ID as Id, " +
                        "a.ATTRIBUTE_NAME as Name, " +
                        "a.ATTRIBUTE_TYPE_ID as [Type], " +
                        "pa.content_id AS PersistentContentId, " +
                        "a.persistent_attr_id as PersistentId, " +
                        "pa.attribute_name AS PersistentName, " +
                        "rpa.CONTENT_ID as RelateToPersistentContentId, " +
                        "a.join_attr_id as JoinId " +
                        "FROM content_attribute AS a with(nolock) " +
                        "LEFT OUTER JOIN content_attribute AS pa with(nolock) ON pa.attribute_id = a.persistent_attr_id " +
                        "LEFT OUTER JOIN content_attribute AS rpa with(nolock) ON rpa.attribute_id = pa.related_attribute_id " +
                        "LEFT OUTER JOIN content AS c with(nolock) ON rpa.content_id = c.content_id " +
                        "WHERE a.content_id = @contentId and a.persistent_attr_id IS NOT NULL";

            using (var cmd = SqlCommandFactory.Create(query, сonnection))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Создает United View
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contentId"></param>
        public static void CreateUnitedView(SqlConnection connection, int contentId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_content_united_view_create", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Создает live view и stage view
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contentId"></param>
        public static void CreateFrontedViews(SqlConnection connection, int contentId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_content_frontend_views_create", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаляет view
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="viewName"></param>
        public static void DropView(SqlConnection connection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentNullException("viewName");

            using (var cmd = SqlCommandFactory.Create("qp_drop_existing", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@name", viewName);
                cmd.Parameters.AddWithValue("@flag", "IsView");
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаляет view
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="viewName"></param>
        public static void RefreshView(SqlConnection connection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentNullException("viewName");

            using (var cmd = SqlCommandFactory.Create("sp_refreshview", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@viewname", viewName);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Возвращает список id дочерних виртуальных контентов
        /// </summary>
        /// <param name="contentID"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetVirtualSubContentIDs(SqlConnection connection, int contentId)
        {
            var query = "select content_id from content where virtual_join_primary_content_id = @contentId " +
                        "union " +
                        "select virtual_content_id as content_id from union_contents where union_content_id = @contentId " +
                        "union " +
                        "select virtual_content_id as content_id from user_query_contents where real_content_id = @contentId";

            var result = new List<int>();
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result.ToArray();
        }

        public static IEnumerable<int> GetRealBaseFieldIds(SqlConnection connection, int virtualFieldId)
        {
            var result = new List<int>();
            using (var cmd = SqlCommandFactory.Create("qp_get_real_base_attributes", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@v_attr_id", virtualFieldId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает информацию о дочерних витуальных полях
        /// </summary>
        /// <param name="сonnection"></param>
        /// <param name="rootFieldId"></param>
        /// <returns></returns>
        public static DataTable GetVirtualSubFields(SqlConnection сonnection, IEnumerable<int> rootFieldId)
        {
            var inStatement = "-1";
            if (rootFieldId.Any())
                inStatement = string.Join(",", rootFieldId);

            var query =
                string.Format(
                    "SELECT [BASE_ATTR_ID],[BASE_CNT_ID],[VIRTUAL_ATTR_ID],[VIRTUAL_CNT_ID] FROM [VIRTUAL_ATTR_BASE_ATTR_RELATION] WHERE BASE_ATTR_ID IN ({0})",
                    inStatement);

            using (var cmd = SqlCommandFactory.Create(query, сonnection))
            {
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Возвращает информацию о дочерних витуальных полях
        /// </summary>
        /// <param name="сonnection"></param>
        /// <param name="rootFieldId"></param>
        /// <returns></returns>
        public static DataTable GetVirtualBaseFieldIDs(SqlConnection сonnection, IEnumerable<int> subFieldIds)
        {
            var inStatement = "-1";
            if (subFieldIds.Any())
                inStatement = string.Join(",", subFieldIds);

            var query =
                string.Format(
                    "SELECT distinct [BASE_ATTR_ID], [BASE_CNT_ID], [VIRTUAL_ATTR_ID], [VIRTUAL_CNT_ID] FROM [VIRTUAL_ATTR_BASE_ATTR_RELATION] WHERE VIRTUAL_ATTR_ID IN ({0})",
                    inStatement);

            using (var cmd = SqlCommandFactory.Create(query, сonnection))
            {
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Загрузить данные о связи полей в памяти, начиная с некоторго контента
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="rootContentId"></param>
        /// <returns></returns>
        public static DataTable LoadVirtualFieldsRelations(SqlConnection sqlConnection, int rootContentId)
        {
            const string query = "WITH V2BREL AS " +
                                 "( " +
                                 "SELECT *, 0 as [LEVEL] FROM [VIRTUAL_ATTR_BASE_ATTR_RELATION] where BASE_CNT_ID = @rootContent " +
                                 "union all " +
                                 "SELECT BA.*, [LEVEL] + 1 FROM [VIRTUAL_ATTR_BASE_ATTR_RELATION] BA " +
                                 "join V2BREL on BA.BASE_ATTR_ID = V2BREL.VIRTUAL_ATTR_ID " +
                                 ") " +
                                 "select * from V2BREL order by [LEVEL]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@rootContent", rootContentId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }


        /// <summary>
        /// Получить количество полей связей в Union_ATTR для union-полей
        /// </summary>
        /// <param name="сonnection"></param>
        /// <param name="rootFieldId"></param>
        /// <returns></returns>
        public static DataTable GetUnionFieldRelationCount(SqlConnection сonnection, IEnumerable<int> unionFieldIds)
        {
            var inStatement = "-1";
            if (unionFieldIds.Any())
                inStatement = string.Join(",", unionFieldIds);

            var query =
                string.Format(
                    "select COUNT(union_attr_id) F_COUNT, VIRTUAL_ATTR_ID UNION_FIELD_ID FROM union_attrs GROUP BY VIRTUAL_ATTR_ID HAVING VIRTUAL_ATTR_ID IN ({0})",
                    inStatement);

            using (var cmd = SqlCommandFactory.Create(query, сonnection))
            {
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Получить информацию о таблицах и столбцах которые используются во вью
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static DataTable GetViewColumnUsage(SqlConnection sqlConnection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentException("viewName");

            var sql = @"
               WITH CI(COLUMN_NAME, DATA_TYPE, NUMERIC_SCALE, CHARACTER_MAXIMUM_LENGTH, TABLE_NAME) AS
               (
                 select CI.COLUMN_NAME, CI.DATA_TYPE, CI.NUMERIC_SCALE, Ci.CHARACTER_MAXIMUM_LENGTH, VU.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS CI
                 LEFT JOIN INFORMATION_SCHEMA.VIEW_COLUMN_USAGE VU ON VU.COLUMN_NAME = CI.COLUMN_NAME AND VU.VIEW_NAME = @view_name
                 where CI.TABLE_NAME = @view_name
               )
               SELECT CI.COLUMN_NAME as ColumnName, CI.DATA_TYPE As DbType, CI.NUMERIC_SCALE as NumericScale, CI.CHARACTER_MAXIMUM_LENGTH as CharMaxLength, CI.TABLE_NAME As TableName, COALESCE(CI2.DATA_TYPE, CI.DATA_TYPE) AS TableDbType FROM CI
               LEFT JOIN INFORMATION_SCHEMA.COLUMNS CI2 ON CI2.COLUMN_NAME = CI.COLUMN_NAME AND CI2.TABLE_NAME = CI.TABLE_NAME
            ";

            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@view_name", viewName);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable GetVirtualContentRelations(SqlConnection sqlConnection)
        {
            var query = "SELECT [BASE_CONTENT_ID],[VIRTUAL_CONTENT_ID] FROM [VIRTUAL_CONTENT_RELATION]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }


        public static int CountChildArticles(SqlConnection sqlConnection, int articleId, bool countArchived)
        {
            using (var cmd = SqlCommandFactory.Create("qp_count_child_articles", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@article_id", articleId);
                cmd.Parameters.AddWithValue("@count_archived", countArchived);
                cmd.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.ExecuteNonQuery();
                return (int)cmd.Parameters["@count"].Value;
            }
        }

        private static readonly string GET_VISUAL_EDITOR_CONFIG_QUERY =
            "select ISNULL(A.P_ENTER_MODE, S.P_ENTER_MODE) AS PEnterMode, " +
            "ISNULL(A.USE_ENGLISH_QUOTES, S.USE_ENGLISH_QUOTES) AS UseEnglishQuotes,   " +
            "ISNULL(A.ROOT_ELEMENT_CLASS, S.ROOT_ELEMENT_CLASS) AS RootElementClass,   " +
            "ISNULL(A.EXTERNAL_CSS, S.EXTERNAL_CSS) AS ExternalCss   " +
            "from CONTENT_ATTRIBUTE A  " +
            "join CONTENT C on C.CONTENT_ID = A.CONTENT_ID  " +
            "join [SITE] S on S.SITE_ID = C.SITE_ID  " +
            "WHERE A.ATTRIBUTE_ID = @attr_id";

        public static DataTable GetVisualEditFieldParams(SqlConnection sqlConnection, int fieldID)
        {
            using (var cmd = SqlCommandFactory.Create(GET_VISUAL_EDITOR_CONFIG_QUERY, sqlConnection))
            {
                var dt = new DataTable();
                cmd.Parameters.AddWithValue("@attr_id", fieldID);
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        private static readonly string UPDATE_SITE_FOLDER_SQL =
            "WITH FOLDERS(FOLDER_ID, PARENT_FOLDER_ID, [PATH], NEW_PATH) as " +
            "( " +
            "select FOLDER_ID, PARENT_FOLDER_ID, [PATH], REPLACE([PATH], [PATH], @new_path) AS NEW_PATH from [FOLDER] " +
            "where FOLDER_ID = @folder_id " +
            "union all " +
            "select F.FOLDER_ID, F.PARENT_FOLDER_ID, F.[PATH], REPLACE(F.[PATH], F2.[PATH], F2.NEW_PATH) AS NEW_PATH from [FOLDER] F " +
            "join FOLDERS F2 ON F2.FOLDER_ID = F.PARENT_FOLDER_ID " +
            ") " +
            "UPDATE [FOLDER] SET [FOLDER].[PATH] = NP.NEW_PATH, " +
            " [FOLDER].[MODIFIED] = @modified, " +
            " [FOLDER].[LAST_MODIFIED_BY] = @modified_by " +
            "FROM FOLDERS NP " +
            "WHERE [FOLDER].FOLDER_ID = NP.FOLDER_ID";

        public static void UpdateSiteSubFoldersPath(SqlConnection sqlConnection, int parentFolderId, string path,
            int modifiedBy, DateTime modified)
        {
            using (var cmd = SqlCommandFactory.Create(UPDATE_SITE_FOLDER_SQL, sqlConnection))
            {

                cmd.Parameters.AddWithValue("@folder_id", parentFolderId);
                cmd.Parameters.AddWithValue("@new_path", path);
                cmd.Parameters.AddWithValue("@modified", modified);
                cmd.Parameters.AddWithValue("@modified_by", modifiedBy);
                cmd.ExecuteNonQuery();
            }
        }


        private static readonly string UPDATE_CONTENT_FOLDER_SQL =
            "WITH FOLDERS(FOLDER_ID, PARENT_FOLDER_ID, [PATH], NEW_PATH) as " +
            "( " +
            "select FOLDER_ID, PARENT_FOLDER_ID, [PATH], REPLACE([PATH], [PATH], @new_path) AS NEW_PATH from [content_FOLDER] " +
            "where FOLDER_ID = @folder_id " +
            "union all " +
            "select F.FOLDER_ID, F.PARENT_FOLDER_ID, F.[PATH], REPLACE(F.[PATH], F2.[PATH], F2.NEW_PATH) AS NEW_PATH from [content_FOLDER] F " +
            "join FOLDERS F2 ON F2.FOLDER_ID = F.PARENT_FOLDER_ID " +
            ") " +
            "UPDATE [content_FOLDER] " +
            " SET [content_FOLDER].[PATH] = NP.NEW_PATH, " +
            " [content_FOLDER].[MODIFIED] = @modified, " +
            " [content_FOLDER].[LAST_MODIFIED_BY] = @modified_by " +
            "FROM FOLDERS NP " +
            "WHERE [content_FOLDER].FOLDER_ID = NP.FOLDER_ID";

        public static void UpdateContentSubFoldersPath(SqlConnection sqlConnection, int parentFolderId, string path,
            int modifiedBy, DateTime modified)
        {
            using (var cmd = SqlCommandFactory.Create(UPDATE_CONTENT_FOLDER_SQL, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@folder_id", parentFolderId);
                cmd.Parameters.AddWithValue("@new_path", path);
                cmd.Parameters.AddWithValue("@modified", modified);
                cmd.Parameters.AddWithValue("@modified_by", modifiedBy);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetEntitiesTitles(SqlConnection sqlConnection, string entityTypeCode,
            IEnumerable<int> entitiesIDs)
        {
            if (entitiesIDs != null && entitiesIDs.Any())
            {
                using (var cmd = SqlCommandFactory.Create("qp_get_entity_titles_for_log", sqlConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@entity_type_code", entityTypeCode);
                    cmd.Parameters.AddWithValue("@entity_item_ids", string.Join(",", entitiesIDs));

                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    return dt.AsEnumerable().ToArray();
                }
            }
            return Enumerable.Empty<DataRow>();
        }


        public static IEnumerable<DataRow> GetActionLogPage(SqlConnection sqlConnection, string orderBy,
            string actionTypeCode,
            string entityTypeCode,
            string entityStringId,
            string parentEntityId,
            string entityTitle,
            string from, string to,
            IEnumerable<int> userIDs,
            out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            Debug.Assert(userIDs != null, "userIDs is null");

            var filters = new List<string>();
            if (!string.IsNullOrEmpty(actionTypeCode))
                filters.Add(string.Format("AT.CODE = '{0}'", Cleaner.ToSafeSqlString(actionTypeCode)));
            if (!string.IsNullOrEmpty(entityTypeCode))
                filters.Add(string.Format("ET.CODE = '{0}'", Cleaner.ToSafeSqlString(entityTypeCode)));
            if (!string.IsNullOrEmpty(entityStringId))
                filters.Add(string.Format("L.ENTITY_STRING_ID = '{0}'", Cleaner.ToSafeSqlString(entityStringId)));
            if (!string.IsNullOrEmpty(parentEntityId))
                filters.Add(string.Format("CAST(L.PARENT_ENTITY_ID AS NVARCHAR(25)) = '{0}'",
                    Cleaner.ToSafeSqlString(parentEntityId)));
            if (!string.IsNullOrEmpty(entityTitle))
                filters.Add(string.Format("L.ENTITY_TITLE LIKE '%{0}%'", Cleaner.ToSafeSqlString(entityTitle)));

            DateTime? dt;
            string sqlDT;
            to = string.IsNullOrWhiteSpace(to) ? from : to;
            if (Converter.TryConvertToSqlDateTimeString(from, out sqlDT, out dt, sqlFormat: "yyyyMMdd HH:mm:00"))
                filters.Add(string.Format("L.EXEC_TIME >= '{0}'", sqlDT));
            if (Converter.TryConvertToSqlDateTimeString(to, out sqlDT, out dt, sqlFormat: "yyyyMMdd HH:mm:59"))
                filters.Add(string.Format("L.EXEC_TIME <= '{0}'", sqlDT));

            if (userIDs.Any())
                filters.Add(string.Format("U.[USER_ID] in ({0})", string.Join(",", userIDs)));


            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomerCode,
                "L.ID as Id, L.EXEC_TIME AS ExecutionTime, L.API as IsApi, AT.CODE AS ActionTypeCode, AT.NAME AS ActionTypeName, ET.CODE AS EntityTypeCode, ET.NAME AS EntityTypeName" +
                ", L.ENTITY_STRING_ID AS EntityStringId, L.PARENT_ENTITY_ID AS ParentEntityId, L.ENTITY_TITLE AS EntityTitle, U.[USER_ID] as UserId, U.[LOGIN] as UserLogin",

                "BACKEND_ACTION_LOG L LEFT JOIN USERS U ON L.[USER_ID] = U.[USER_ID]" +
                " INNER JOIN ACTION_TYPE AT ON AT.CODE = L.ACTION_TYPE_CODE" +
                " INNER JOIN ENTITY_TYPE ET ON ET.CODE = L.ENTITY_TYPE_CODE",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "ExecutionTime DESC",
                string.Join(" AND ", filters),
                startRow,
                pageSize,
                out totalRecords
                );
        }


        public static IEnumerable<DataRow> GetCustomActionList(SqlConnection sqlConnection, string orderBy, int startRow,
            int pageSize, out int totalRecords)
        {
            var selectBuilder = new StringBuilder();
            selectBuilder.Append("CA.[ID], CA.[NAME], CA.[ACTION_ID]");
            selectBuilder.Append(", AT.CODE AS ACTION_TYPE_CODE, AT.NAME AS ACTION_TYPE_NAME");
            selectBuilder.Append(", ET.CODE AS ENTITY_TYPE_CODE ,ET.NAME AS ENTITY_TYPE_NAME");
            selectBuilder.Append(
                ", CA.[URL], CA.[ICON_URL], CA.[ORDER], CA.[SITE_EXCLUDED], CA.[CONTENT_EXCLUDED], CA.[SHOW_IN_MENU], CA.[SHOW_IN_TOOLBAR]");
            selectBuilder.Append(", CA.[CREATED], CA.[MODIFIED], CA.[LAST_MODIFIED_BY], U.[USER_ID], U.[LOGIN]");

            var fromBuilder = new StringBuilder();
            fromBuilder.Append(" [dbo].CUSTOM_ACTION CA");
            fromBuilder.Append(" JOIN [dbo].BACKEND_ACTION A ON CA.ACTION_ID = A.ID");
            fromBuilder.Append(" JOIN [dbo].ACTION_TYPE AT ON AT.ID = A.[TYPE_ID]");
            fromBuilder.Append(" JOIN [dbo].ENTITY_TYPE ET ON ET.ID = A.ENTITY_TYPE_ID");
            fromBuilder.Append(" JOIN [dbo].USERS U ON U.[USER_ID] = CA.LAST_MODIFIED_BY");

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomAction,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[ORDER] ASC",
                string.Empty,
                startRow,
                pageSize,
                out totalRecords
                );
        }

        public static List<DataRow> GetLockedArticlesList(SqlConnection sqlConnection, string orderBy, int startRow,
            int pageSize, int userId, out int totalRecords)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                @"[CONTENT_ITEM_ID] as ID
                                      ,it.[content_id] as ParentId
                                      ,dbo.qp_get_article_title_func(it.[Content_Item_Id],it.[content_id]) as Title
                                      ,con.CONTENT_NAME as ContentName
                                      ,site.SITE_NAME as SiteName
                                      ,typ.STATUS_TYPE_NAME as StatusName
                                      ,it.[CREATED] as Created
                                      ,it.[MODIFIED] as Modified
                                      ,us.LOGIN as LastModifiedByUser
                                      ,it.[PERMANENT_LOCK] as IsPermanentLock",

                @"[dbo].[CONTENT_ITEM] as it
                                      INNER JOIN [dbo].[content] as con on con.CONTENT_ID = it.CONTENT_ID
                                      INNER JOIN [dbo].[SITE] as site on site.SITE_ID = con.SITE_ID
                                      INNER JOIN [dbo].[STATUS_TYPE] as typ on typ.STATUS_TYPE_ID = it.STATUS_TYPE_ID
                                      INNER JOIN [dbo].[USERS] as us on us.USER_ID = it.LAST_MODIFIED_BY",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "ID ASC",
                string.Format("it.[locked_by] = {0}", userId),
                startRow,
                pageSize,
                out totalRecords
                ).ToList();
        }

        public static int GetLockedArticlesCount(SqlConnection sqlConnection, int userId)
        {
            var query = "select count(*) from content_item where locked_by = @userId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int GetArticlesWaitingForApprovalCount(SqlConnection sqlConnection, int userId)
        {
            var query =
                @"select count(*) from content_item_workflow ciw with(nolock)
            INNER JOIN content_item ci with(nolock) ON ci.content_item_id = ciw.content_item_id
            INNER JOIN full_workflow_rules wr with(nolock) on ciw.workflow_id = wr.workflow_id AND ci.status_type_id = wr.successor_status_id
            INNER JOIN full_workflow_rules wr2 with(nolock) on wr.workflow_id = wr2.workflow_id AND wr2.rule_order = wr.rule_order + 1
            WHERE (wr2.user_id = @userId OR wr2.group_id IN (SELECT group_id FROM user_group_bind with(nolock) WHERE user_id = @userId))
            AND (
                ci.content_item_id not in (select content_item_id from waiting_for_approval with(nolock))
                OR ci.content_item_id in (select content_item_id from waiting_for_approval with(nolock) where user_id = @userId)
            )";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static List<DataRow> GetArticlesWaitingForApproval(SqlConnection sqlConnection, string orderBy,
            int startRow, int pageSize, int userId, out int totalRecords)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                @"ci.content_item_id as ID
                                    ,ci.content_id as ParentId
                                    ,dbo.qp_get_article_title_func(ci.content_item_id, c.content_id) as Title
                                    ,s.site_name as SiteName
                                    ,c.CONTENT_NAME as ContentName
                                    ,ci.MODIFIED as Modified
                                    ,ci.CREATED as Created
                                    ,typ.STATUS_TYPE_NAME as StatusName
                                    ,us.LOGIN as LastModifiedByUser
                                    ,ci.PERMANENT_LOCK as IsPermanentLock",

                @"content_item_workflow ciw with(nolock)
                                    INNER JOIN content_item ci with(nolock) ON ci.content_item_id = ciw.content_item_id
                                    INNER JOIN full_workflow_rules wr with(nolock) on ciw.workflow_id = wr.workflow_id AND ci.status_type_id = wr.successor_status_id
                                    INNER JOIN full_workflow_rules wr2 with(nolock) on wr.workflow_id = wr2.workflow_id AND wr2.rule_order = wr.rule_order + 1
                                    INNER JOIN content c with(nolock) ON ci.content_id = c.content_id
                                    INNER JOIN site s with(nolock) ON c.site_id = s.site_id
                                    INNER JOIN status_type as typ with(nolock) on typ.STATUS_TYPE_ID = ci.STATUS_TYPE_ID
                                    INNER JOIN users as us on us.USER_ID = ci.LAST_MODIFIED_BY",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "SiteName ASC, ID ASC",
                string.Format(@"(wr2.user_id = {0}
                                            OR wr2.group_id IN (SELECT group_id FROM user_group_bind with(nolock) WHERE user_id={0})
                                        )
                                        AND (
                                            ci.content_item_id not in (select content_item_id from waiting_for_approval with(nolock))
                                            OR ci.content_item_id in (select content_item_id from waiting_for_approval with(nolock) where user_id = {0})
                                        )", userId),
                startRow,
                pageSize,
                out totalRecords
                ).ToList();
        }

        internal static IEnumerable<DataRow> GetSimplePagedListOld(SqlConnection sqlConnection,
            string entityTypeCode, string selectBlock, string fromBlock, string orderBy, string filter, int startRow,
            int pageSize, out int totalRecords,
            int userId = 0, bool useSecurity = false, bool countOnly = false, int groupId = 0,
            int startLevel = PermissionLevel.List, int endLevel = PermissionLevel.FullAccess,
            string parentEntityTypeCode = "", int parentEntityId = 0, IEnumerable<int> selectedIds = null,
            List<SqlParameter> sqlParameters = null)
        {
            var forceCountQuery = entityTypeCode == "content_item" &&
                                  (filter == "c.archive = 0" || string.IsNullOrEmpty(filter));
            using (var cmd = SqlCommandFactory.Create("qp_get_paged_data", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("select_block", selectBlock);
                cmd.Parameters.AddWithValue("from_block", fromBlock);
                cmd.Parameters.AddWithValue("entity_name", entityTypeCode);

                if (!string.IsNullOrEmpty(filter))
                    cmd.Parameters.AddWithValue("where_block", filter);

                if (!string.IsNullOrEmpty(orderBy))
                    cmd.Parameters.AddWithValue("order_by_block", orderBy);

                cmd.Parameters.AddWithValue("count_only", countOnly);
                cmd.Parameters.Add(new SqlParameter("total_records", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                });
                cmd.Parameters.AddWithValue("start_row", startRow);
                cmd.Parameters.AddWithValue("page_size", pageSize);
                if (useSecurity)
                {
                    cmd.Parameters.AddWithValue("use_security", useSecurity);
                    cmd.Parameters.AddWithValue("user_id", userId);
                    cmd.Parameters.AddWithValue("group_id", groupId);
                    cmd.Parameters.AddWithValue("start_level", startLevel);
                    cmd.Parameters.AddWithValue("end_level", endLevel);
                    cmd.Parameters.AddWithValue("parent_entity_name", parentEntityTypeCode);
                    cmd.Parameters.AddWithValue("parent_entity_id", parentEntityId);
                }
                cmd.Parameters.AddWithValue("separate_count_query", forceCountQuery);
                cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                {
                    TypeName = "Ids",
                    Value = IdsToDataTable(selectedIds)
                });
                if (sqlParameters != null)
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                var ds = new DataSet();
                new SqlDataAdapter(cmd).Fill(ds);

                DataTable result = null;
                totalRecords = 0;

                if (ds.Tables.Count > 0)
                {
                    result = ds.Tables[0];
                    totalRecords = (forceCountQuery)
                        ? (int)cmd.Parameters["total_records"].Value
                        : ((result.Rows.Count != 0) ? (int)result.Rows[0][QP8Entities.COUNT_COLUMN] : 0);
                }
                else if (countOnly)
                {
                    totalRecords = (int)cmd.Parameters["total_records"].Value;
                }

                return result.AsEnumerable().ToArray();
            }
        }

        internal static IEnumerable<DataRow> GetSimplePagedList(SqlConnection sqlConnection,
            string entityTypeCode, string selectBlock, string fromBlock, string orderBy, string filter, int startRow,
            int pageSize, out int totalRecords,
            int userId = 0, bool useSecurity = false, bool countOnly = false, int groupId = 0,
            int startLevel = PermissionLevel.List, int endLevel = PermissionLevel.FullAccess,
            string parentEntityTypeCode = "", int parentEntityId = 0, IEnumerable<int> selectedIds = null,
            IList<SqlParameter> sqlParameters = null, bool useSql2012Syntax = false,
            IEnumerable<int> filterIds = null)
        {
            var forceCountQuery = entityTypeCode == "content_item" &&
                                  (filter == "c.archive = 0" || string.IsNullOrEmpty(filter));
            totalRecords = 0;
            DataTable result = null;
            if (useSecurity)
            {
                var securitySql = GetPermittedItemsAsQuery(sqlConnection, userId, groupId, startLevel, endLevel,
                    entityTypeCode, parentEntityTypeCode, parentEntityId);
                fromBlock = fromBlock.Replace("<$_security_insert_$>", securitySql);
            }

            if (countOnly || forceCountQuery)
            {
                var countBuilder = new StringBuilder();
                countBuilder.Append("SELECT @total_records = COUNT(*) from ");
                countBuilder.AppendLine(fromBlock);
                if (!string.IsNullOrEmpty(filter))
                {
                    countBuilder.Append("WHERE ");
                    countBuilder.Append(filter);
                }

                using (var countCmd = SqlCommandFactory.Create(countBuilder.ToString(), sqlConnection))
                {
                    countCmd.CommandType = CommandType.Text;
                    countCmd.Parameters.Add(new SqlParameter("total_records", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    });
                    countCmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(selectedIds)
                    });
                    countCmd.Parameters.Add(new SqlParameter("@filterIds", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(filterIds)
                    });
                    if (sqlParameters != null)
                    {
                        countCmd.Parameters.AddRange(sqlParameters.ToArray());
                    }

                    countCmd.ExecuteNonQuery();
                    totalRecords = (int)countCmd.Parameters["total_records"].Value;
                }
            }

            startRow = (startRow <= 0) ? 1 : startRow;
            if (useSql2012Syntax)
                startRow--;

            var endRow = (pageSize == 0) ? 0 : startRow + pageSize - 1;
            if (useSql2012Syntax && endRow > 0)
                endRow++;

            if (!countOnly)
            {
                var sqlBuilder = new StringBuilder();
                if (useSql2012Syntax)
                {
                    sqlBuilder.AppendFormatLine(" SELECT " + selectBlock);
                    if (!forceCountQuery)
                    {
                        sqlBuilder.AppendLine(", COUNT(*) OVER() AS ROWS_COUNT");
                    }

                    sqlBuilder.AppendLine(" FROM " + fromBlock);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        sqlBuilder.AppendLine("  WHERE " + filter);
                    }

                    sqlBuilder.AppendLine(" ORDER BY " + orderBy);
                    sqlBuilder.AppendLine(@"OFFSET @startRow ROWS ");
                    if (endRow > 0)
                    {
                        sqlBuilder.AppendLine(@"FETCH NEXT @endRow - @startRow ROWS ONLY ");
                    }
                }
                else
                {
                    sqlBuilder.AppendLine("WITH PAGED_DATA_CTE");
                    sqlBuilder.AppendLine("AS");
                    sqlBuilder.AppendLine("(");
                    sqlBuilder.AppendFormatLine("  SELECT c.*, ROW_NUMBER() OVER (ORDER BY {0}) AS ROW_NUMBER", orderBy);
                    if (!forceCountQuery)
                    {
                        sqlBuilder.AppendLine(", COUNT(*) OVER() AS ROWS_COUNT");
                    }

                    sqlBuilder.AppendLine("  FROM (");
                    sqlBuilder.AppendLine("    SELECT " + selectBlock);
                    sqlBuilder.AppendLine("    FROM " + fromBlock);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        sqlBuilder.AppendLine("    WHERE " + filter);
                    }

                    sqlBuilder.AppendLine("  ) AS c ");
                    sqlBuilder.AppendLine(")");
                    sqlBuilder.AppendLine("SELECT * FROM PAGED_DATA_CTE");
                    if (endRow > 0 || startRow > 1)
                        sqlBuilder.AppendLine("WHERE 1 = 1");
                    if (startRow > 1)
                        sqlBuilder.AppendLine("AND ROW_NUMBER >= @startRow");
                    if (endRow > 0)
                        sqlBuilder.AppendLine("AND ROW_NUMBER <= @endRow");

                    sqlBuilder.AppendLine("ORDER BY ROW_NUMBER ASC");
                }

                using (var cmd = SqlCommandFactory.Create(sqlBuilder.ToString(), sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@startRow", SqlDbType.Int) { Value = startRow });
                    cmd.Parameters.Add(new SqlParameter("@endRow", SqlDbType.Int) { Value = endRow });
                    cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(selectedIds)
                    });
                    cmd.Parameters.Add(new SqlParameter("@filterIds", SqlDbType.Structured)
                    {
                        TypeName = "Ids",
                        Value = IdsToDataTable(filterIds)
                    });
                    if (sqlParameters != null)
                        cmd.Parameters.AddRange(sqlParameters.ToArray());

                    var ds = new DataSet();
                    new SqlDataAdapter(cmd).Fill(ds);

                    if (ds.Tables.Count > 0)
                    {
                        result = ds.Tables[0];
                        if (!forceCountQuery)
                        {
                            totalRecords = (result.Rows.Count != 0) ? (int)result.Rows[0][QP8Entities.COUNT_COLUMN] : 0;
                        }
                    }
                }
            }

            return result.AsEnumerable().ToArray();
        }

        public static IEnumerable<DataRow> GetButtonTracePage(SqlConnection sqlConnection, string orderBy,
            out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var fromBuilder = new StringBuilder();
            fromBuilder.Append(" dbo.[BUTTON_TRACE] BT");
            fromBuilder.Append(" INNER JOIN dbo.[USERS] U ON U.[USER_ID] = BT.[USER_ID]");
            fromBuilder.Append(" LEFT JOIN dbo.[BUTTONS] B ON B.BUTTON_ID = BT.BUTTON_ID");
            fromBuilder.Append(" LEFT JOIN dbo.[TOOLBAR_BUTTONS] TB ON TB.TBUTTON_ID = BT.TBUTTON_ID");
            fromBuilder.Append(" LEFT JOIN dbo.[TABS] T ON T.TAB_ID = COALESCE(B.LOCATION_TAB_ID, TB.LOCATION_TAB_ID)");

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomerCode,
                "COALESCE(B.BUTTON_NAME, TB.BUTTON_NAME) as ButtonName, BT.activated as ActivatedTime, U.[USER_ID] as UserId, U.[LOGIN] as UserLogin, T.TAB_NAME as TabName",
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(orderBy) ? orderBy : "ActivatedTime DESC",
                string.Empty,
                startRow,
                pageSize,
                out totalRecords
                );
        }

        /// <summary>
        /// Получить страницу Removed Entities
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <param name="countOnly"></param>
        /// <param name="totalRecords"></param>
        /// <param name="startRow"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IEnumerable<DataRow> GetRemovedEntitiesPage(SqlConnection sqlConnection, string orderBy,
            out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomerCode,
                "RE.ID as EntityId, RE.PARENT_ID as ParentEntityId, RE.ENTITY_NAME as EntityTypeCode, RE.TITLE as EntityTitle" +
                ", U.[USER_ID] as UserId, U.[LOGIN] as UserLogin, RE.[DELETED] as DeletedTime",
                "dbo.[REMOVED_ENTITIES] RE JOIN dbo.[USERS] U ON RE.[USER_ID] = U.[USER_ID]",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "DeletedTime DESC",
                string.Empty,
                startRow,
                pageSize,
                out totalRecords
                );
        }

        /// <summary>
        /// Получить страницу Sessions
        /// </summary>
        public static IEnumerable<DataRow> GetSessionsPage(SqlConnection sqlConnection, bool isFailed, string orderBy,
            out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomerCode,
                "[SESSION_ID] AS SessionId, [LOGIN] as [Login], [USER_ID] as [UserId], [START_TIME] as [StartTime], [END_TIME] as [EndTime], [IP]" +
                ", [BROWSER] as Browser, [SERVER_NAME] as ServerName, [AUTO_LOGGED] as AutoLogged, [SID] as [Sid], [IS_QP7] as IsQP7",
                "dbo.[SESSIONS_LOG]",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[SessionId] DESC",
                (isFailed) ? "[USER_ID] IS NULL" : "[USER_ID] IS NOT NULL",
                startRow,
                pageSize,
                out totalRecords
                );
        }

        public static IEnumerable<DataRow> GetNotificationsPage(SqlConnection sqlConnection, int contentId,
            string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Notification,
                "[NOTIFICATION_ID] AS Id, [NOTIFICATION_NAME] as [Name], n.[CREATED] as [Created], n.[MODIFIED] as [Modified], n.no_email, n.LAST_MODIFIED_BY as[LastModifiedBy], " +
                "n.IS_EXTERNAL as[IsExternal], u2.LOGIN as [LastModifiedByLogin], [FOR_CREATE] as ForCreate, FOR_DELAYED_PUBLICATION as [ForDelayedPublication], " +
                "[FOR_MODIFY] as ForModify, [For_Remove] as ForRemove, [for_status_changed] as [ForStatusChanged], [for_status_partially_changed] as ForStatusPartiallyChanged, [for_frontend] as ForFrontend," +
                "n.GROUP_ID,n.[USER_ID],n.email_attribute_id, COALESCE(u.LOGIN, ug.GROUP_NAME, a.ATTRIBUTE_NAME) as Receiver",
                "dbo.[NOTIFICATIONS] n left outer join user_group ug on n.group_id = ug.group_id " +
                "left outer join users u on n.user_id = u.user_id left outer join CONTENT_ATTRIBUTE as a on n.email_attribute_id = a.ATTRIBUTE_ID " +
                "inner join users u2 on n.LAST_MODIFIED_BY = u2.user_id",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name]",
                "n.CONTENT_ID = " + contentId,
                startRow,
                pageSize,
                out totalRecords
                );
        }

        public static IEnumerable<DataRow> GetVisualEditorPluginsPage(SqlConnection sqlConnection, int contentId,
            string orderBy, out int totalRecords,
            int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.VisualEditorPlugin,
                "p.[ID] as [Id], p.[NAME] as [Name], p.[DESCRIPTION] as [Description], p.[URL] as [Url], p.[ORDER] as [Order], p.[CREATED] as [Created]," +
                "p.[MODIFIED] as [Modified], p.LAST_MODIFIED_BY as[LastModifiedBy], u.LOGIN as [LastModifiedByLogin]",
                "dbo.[VE_PLUGIN] p inner join users u on p.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Order]",
                "",
                startRow,
                pageSize,
                out totalRecords
                );
        }

        public static int GetVisualEditorPluginMaxOrder(SqlConnection sqlConnection)
        {
            using (var cmd = SqlCommandFactory.Create("select MAX([ORDER]) FROM [dbo].[VE_PLUGIN]", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : (int)maxOrder;
            }
        }

        public static int GetVisualEditorStyleMaxOrder(SqlConnection sqlConnection)
        {
            using (var cmd = SqlCommandFactory.Create("select MAX([ORDER]) FROM [dbo].[VE_STYLE]", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : (int)maxOrder;
            }
        }

        public static IEnumerable<DataRow> GetDisplayColumns(SqlConnection sqlConnection, int contentId)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append(
                "SELECT ca.ATTRIBUTE_ID, ca.ATTRIBUTE_NAME, ca.ATTRIBUTE_TYPE_ID, rca.ATTRIBUTE_ID AS RELATED_ATTRIBUTE_ID, rca.ATTRIBUTE_TYPE_ID AS RELATED_ATTRIBUTE_TYPE_ID, ");
            queryBuilder.Append(
                " rca.ATTRIBUTE_NAME AS RELATED_ATTRIBUTE_NAME, rca.CONTENT_ID AS RELATED_CONTENT_ID, ca.IS_CLASSIFIER, ca.view_in_list, ");
            queryBuilder.Append(
                " rrca.ATTRIBUTE_ID AS RELATED_ATTRIBUTE_ID2, rrca.ATTRIBUTE_TYPE_ID AS RELATED_ATTRIBUTE_TYPE_ID2, rrca.ATTRIBUTE_NAME AS RELATED_ATTRIBUTE_NAME2, rrca.CONTENT_ID AS RELATED_CONTENT_ID2, ");
            queryBuilder.Append(
                " CAST(ROW_NUMBER() OVER(PARTITION BY rca.ATTRIBUTE_ID ORDER BY ca.ATTRIBUTE_ORDER ASC) AS NUMERIC) AS 'RELATED_COUNT' ");
            queryBuilder.Append(" FROM CONTENT_ATTRIBUTE AS ca ");
            queryBuilder.Append(" LEFT JOIN CONTENT_ATTRIBUTE AS rca ON rca.ATTRIBUTE_ID = ca.RELATED_ATTRIBUTE_ID ");
            queryBuilder.Append(" LEFT JOIN CONTENT_ATTRIBUTE AS rrca ON rrca.ATTRIBUTE_ID = rca.RELATED_ATTRIBUTE_ID ");
            queryBuilder.Append(" WHERE ca.CONTENT_ID = @content_id AND ca.view_in_list = 1");
            queryBuilder.Append(" ORDER BY ca.permanent_flag DESC, ca.attribute_order ASC");
            using (var cmd = SqlCommandFactory.Create(queryBuilder.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable();
            }
        }

        public static bool IsArticlePermissionsAllowed(SqlConnection sqlConnection, int contentId)
        {
            using (var cmd = SqlCommandFactory.Create("select allow_items_permission from content with(nolock) where content_id = " + contentId, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                return Converter.ToBoolean(objResult);
            }
        }

        public static IEnumerable<DataRow> GetArticlesPage(SqlConnection sqlConnection, ArticlePageOptions options, IList<SqlParameter> sqlParams, out int totalRecords)
        {
            var selectBuilder = new StringBuilder();
            var fromBuilder = new StringBuilder();
            var whereBuilder = new StringBuilder(SqlFilterComposer.Compose(options.CommonFilter, options.ContextFilter));

            Dictionary<int, string> fieldMap = null;
            if (options.ExstensionContentIds.Any())
            {
                fieldMap = GetAggregatedFieldNames(sqlConnection, options.ExstensionContentIds);
            }

            Dictionary<int, string> referenceMap = null;
            if (options.ContentReferences.Any())
            {
                referenceMap = GetFieldNames(sqlConnection, options.ContentReferences.Select(r => r.ReferenceFieldID).ToArray());
            }

            var useSelection = options.SelectedIDs != null && options.SelectedIDs.Any();
            AddStaticColumnsToQuery(options, useSelection, selectBuilder);
            AddSourcesToQuery(options, useSelection, fromBuilder, fieldMap, referenceMap);
            AddDynamicColumnsToQuery(sqlConnection, options, selectBuilder, fromBuilder, Default.MaxViewInListFieldLength + 1);

            var useFullText = AddFullTextFilteringToQuery(sqlConnection, options.FullTextSearch, options.ContentId, options.ExstensionContentIds, fromBuilder);
            AddLinkFilteringToQuery(options.LinkFilters, whereBuilder, sqlParams);
            AddRelationSecurityFilteringToQuery(sqlConnection, options, fromBuilder, whereBuilder);
            if (options.FilterIds != null && options.FilterIds.Any())
            {
                whereBuilder.Append(" AND c.CONTENT_ITEM_ID IN (SELECT ID FROM @filterIds)");
            }

            var defaultSortExpression = useSelection ? "is_selected DESC, MODIFIED DESC" : "MODIFIED DESC";
            var sortExpression = !string.IsNullOrEmpty(options.SortExpression)
                ? options.SortExpression
                : defaultSortExpression;

            sortExpression = options.OnlyIds ? "CONTENT_ITEM_ID ASC" : sortExpression;
            var result = GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.OldArticle,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                sortExpression,
                whereBuilder.ToString(),
                options.StartRecord,
                options.PageSize,
                out totalRecords,
                options.UserId,
                options.UseSecurity,
                selectedIds: options.SelectedIDs,
                filterIds: options.FilterIds,
                sqlParameters: sqlParams,
                useSql2012Syntax: options.UseSql2012Syntax
                );

            DropFullTextTemporaryTable(sqlConnection, useFullText);
            return result;
        }

        public static void AddLinkFilteringToQuery(IEnumerable<ArticleLinkSearchParameter> linkFilters, StringBuilder whereBuilder, ICollection<SqlParameter> sqlParams)
        {
            if (linkFilters == null)
            {
                return;
            }

            foreach (var linkFilter in linkFilters)
            {
                var tableAlias = "c";
                if (linkFilter.ExstensionContentId != 0)
                {
                    tableAlias += "_" + linkFilter.ExstensionContentId;
                    if (linkFilter.ReferenceFieldId != 0)
                    {
                        tableAlias += "_" + linkFilter.ReferenceFieldId;
                    }
                }

                string internalSql;
                string inverseString;
                if (!linkFilter.IsNull)
                {
                    inverseString = linkFilter.Inverse ? "NOT" : "";
                    var paramName = "@link" + linkFilter.LinkId;
                    sqlParams.Add(GetIdsDatatableParam(paramName, linkFilter.Ids));

                    internalSql = linkFilter.IsManyToMany
                        ? string.Format(
                            "{2} EXISTS (select item_id from dbo.item_link_united with(nolock) where {3}.content_item_id = item_id and link_id = {0} AND linked_item_id in (select id from {1}))",
                            linkFilter.LinkId, paramName, inverseString, tableAlias)
                        : string.Format(
                            "{3} EXISTS (select * from content_{0}_united cu with(nolock) where {4}.content_item_id = [{1}] and cu.content_item_id in (select id from {2})) ",
                            linkFilter.ContentId, linkFilter.FieldName, paramName, inverseString, tableAlias);
                }
                else
                {
                    inverseString = linkFilter.Inverse ? "" : "NOT";
                    internalSql = linkFilter.IsManyToMany
                        ? string.Format(
                            "{1} EXISTS (select item_id from dbo.item_link_united with(nolock) where {2}.content_item_id = item_id and link_id = {0})",
                            linkFilter.LinkId, inverseString, tableAlias)
                        : string.Format(
                            "{2} EXISTS (select * from content_{0}_united with(nolock) where {3}.content_item_id = [{1}]) ",
                            linkFilter.ContentId, linkFilter.FieldName, inverseString, tableAlias);
                }

                if (whereBuilder.Length != 0)
                {
                    whereBuilder.Append(" AND ");
                }

                whereBuilder.Append(internalSql);
            }
        }

        private static void AddRelationSecurityFilteringToQuery(SqlConnection connection, ArticlePageOptions options, StringBuilder fromBuilder, StringBuilder whereBuilder)
        {
            if (options.RelationSecurityFilters != null)
            {
                foreach (var filter in options.RelationSecurityFilters)
                {
                    AddRelationSecurityFilteringToQuery(connection, filter, options.UserId, fromBuilder, whereBuilder);
                }
            }
        }

        private static void AddRelationSecurityFilteringToQuery(SqlConnection connection, ArticleRelationSecurityParameter filter, int userId, StringBuilder fromBuilder, StringBuilder whereBuilder)
        {
            var securitySql = GetPermittedItemsAsQuery(connection, userId, 0, PermissionLevel.List, PermissionLevel.FullAccess, EntityTypeCode.OldArticle, EntityTypeCode.Content, filter.RelatedContentId);
            if (filter.IsClassifier)
            {
                if (whereBuilder.Length != 0)
                {
                    whereBuilder.Append(" AND ");
                }

                whereBuilder.AppendFormat("c.[{0}] in ({1})", filter.FieldName, string.Join(",", filter.AllowedContentIds));
            }
            else if (filter.IsManyToMany)
            {
                fromBuilder.AppendFormatLine(
                    " inner join (select distinct linked_item_id from content_{0}_united link_sec_{1} with(nolock) ",
                    filter.RelatedContentId, filter.FieldId);
                fromBuilder.AppendFormatLine(
                    " inner join item_link links_{1} with(nolock) on link_sec_{1}.content_item_id = links_{1}.item_id and links_{1}.link_id = {0} ",
                    filter.LinkId, filter.FieldId);
                fromBuilder.AppendFormatLine(
                    " inner join ({0}) pi_{1} on link_sec_{1}.content_item_id = pi_{1}.content_item_id ", securitySql,
                    filter.FieldId);
                fromBuilder.AppendFormatLine(
                    " ) as sec_items_{0} on c.content_item_id = sec_items_{0}.linked_item_id ", filter.FieldId);
            }
            else
            {
                fromBuilder.AppendFormatLine(
                    " inner join content_{0}_united rel_sec_{1} with(nolock) on c.[{2}] = rel_sec_{1}.content_item_id",
                    filter.RelatedContentId, filter.FieldId, filter.FieldName);
                fromBuilder.AppendFormatLine(
                    " inner join ({0}) pi_{1} on rel_sec_{1}.content_item_id = pi_{1}.content_item_id ", securitySql,
                    filter.FieldId);
            }
        }

        private static void DropFullTextTemporaryTable(SqlConnection sqlConnection, bool useFullText)
        {
            if (useFullText)
            {
                using (var immediateCmd = SqlCommandFactory.Create("DROP TABLE #ft_temp", sqlConnection))
                {
                    immediateCmd.ExecuteNonQuery();
                }
            }
        }

        public static bool AddFullTextFilteringToQuery(SqlConnection cn, ArticleFullTextSearchParameter ftOptions, int contentId, int[] extensionContentIds, StringBuilder fromBuilder)
        {
            var useFullText = !string.IsNullOrEmpty(ftOptions.QueryString) && !(ftOptions.HasError.HasValue && ftOptions.HasError.Value);
            if (useFullText)
            {
                var sb = new StringBuilder();
                sb.Append("CREATE TABLE #ft_temp (content_item_id decimal primary key); ");
                sb.Append("insert into #ft_temp ");
                sb.Append(GetFullTextSearchQuery(cn, contentId, extensionContentIds, ftOptions));

                using (var cmd = SqlCommandFactory.Create(sb.ToString(), cn))
                {
                    cmd.ExecuteNonQuery();
                }

                fromBuilder.AppendLine(" INNER JOIN #ft_temp as qp_fts ON c.content_item_id = qp_fts.content_item_id");
            }

            return useFullText;
        }

        public static IList<int> GetFilterAndFtsSearchResult(SqlConnection cn, int contentId, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions, string searchFilterQuery, ICollection<SqlParameter> filterSqlParams)
        {
            var ftsQuery = string.IsNullOrWhiteSpace(ftsOptions.QueryString) ? string.Empty : GetFullTextSearchQuery(cn, contentId, extensionContentIds, ftsOptions);
            var unionQuery = string.IsNullOrWhiteSpace(ftsQuery) || string.IsNullOrWhiteSpace(searchFilterQuery)
                ? $"{ftsQuery}{searchFilterQuery}"
                : $"{ftsQuery} INTERSECT {searchFilterQuery}";

            return GetDatatableResult(cn, unionQuery, filterSqlParams.ToArray()).Select(dr => (int)dr.Field<decimal>(0)).Distinct().ToList();
        }

        public static string GetFullTextSearchQuery(SqlConnection cn, int contentId, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions)
        {
            string allContentIds;
            IDictionary<int, string> aggregatedFieldMap = null;
            var allfields = string.IsNullOrEmpty(ftsOptions.FieldIdList);
            var textfields = !allfields && ftsOptions.FieldIdList.Contains(",");
            if (allfields || textfields)
            {
                extensionContentIds = GetReferencedAggregatedContentIds(cn, contentId, null);
                aggregatedFieldMap = GetAggregatedFieldNames(cn, extensionContentIds);
                allContentIds = string.Join(",", new[] { contentId }.Concat(extensionContentIds));
            }
            else
            {
                if (extensionContentIds != null && extensionContentIds.Any())
                {
                    aggregatedFieldMap = GetAggregatedFieldNames(cn, extensionContentIds);
                }

                allContentIds = extensionContentIds != null && extensionContentIds.Any()
                    ? string.Join(",", extensionContentIds)
                    : contentId.ToString();
            }

            var sb = new StringBuilder();
            sb.Append($"SELECT DISTINCT TOP({ftsOptions.SearchResultLimit}) ");
            if (extensionContentIds != null && extensionContentIds.Any())
            {
                sb.AppendFormat("case CI.CONTENT_ID when {0} then CI.CONTENT_ITEM_ID ", contentId);
                if (aggregatedFieldMap != null)
                {
                    foreach (var cid in extensionContentIds.Where(aggregatedFieldMap.ContainsKey))
                    {
                        sb.AppendFormat("when {0} then CEX_{0}.{1}  ", cid, aggregatedFieldMap[cid]);
                    }
                }

                sb.Append("end CONTENT_ITEM_ID ");
            }
            else
            {
                sb.Append("CI.CONTENT_ITEM_ID ");
            }

            sb.Append("from content_item CI ");
            sb.Append("join content_data CD on CI.CONTENT_ITEM_ID = CD.CONTENT_ITEM_ID ");
            if (extensionContentIds != null && aggregatedFieldMap != null)
            {
                foreach (var cid in extensionContentIds.Where(aggregatedFieldMap.ContainsKey))
                {
                    sb.AppendFormat("left join CONTENT_{0} CEX_{0} on CI.CONTENT_ITEM_ID = CEX_{0}.CONTENT_ITEM_ID ", cid);
                }
            }

            sb.AppendFormat("where CI.CONTENT_ITEM_ID = CD.CONTENT_ITEM_ID and CI.CONTENT_ID IN ({0}) ", allContentIds);
            var fieldCondition = allfields ? string.Empty : $" CD.ATTRIBUTE_ID in ({ftsOptions.FieldIdList}) and ";
            sb.AppendFormat("and ({0} contains(CD.*, '{1}')) ", fieldCondition, ftsOptions.QueryString);

            int parsedInt;
            var parsed = int.TryParse(ftsOptions.RawQueryString, out parsedInt);
            if (allfields && parsed)
            {
                sb.AppendFormat("union select ci2.content_item_id from content_item ci2 where ci2.content_id IN ({0}) and ci2.CONTENT_ITEM_ID = {1} ", allContentIds, parsedInt);
            }

            return sb.ToString();
        }

        private static void AddDynamicColumnsToQuery(SqlConnection sqlConnection, ArticlePageOptions options, StringBuilder selectBuilder, StringBuilder fromBuilder, int maxDisplayLength)
        {
            if (options.OnlyIds)
            {
                return;
            }

            foreach (var row in GetDisplayColumns(sqlConnection, options.ContentId))
            {
                var viewInList = row.Field<bool>("view_in_list");
                var fieldType = row.Field<decimal>("ATTRIBUTE_TYPE_ID");
                var fieldName = row.Field<string>("ATTRIBUTE_NAME");
                var isFieldAClassifier = row.Field<bool>("IS_CLASSIFIER");
                if (isFieldAClassifier)
                {
                    selectBuilder.AppendFormat(", [cnt_{0}].[CONTENT_NAME] as [{0}]", fieldName);
                    fromBuilder.AppendFormatLine(" LEFT JOIN CONTENT AS [cnt_{0}] ON [cnt_{0}].[content_id] = c.[{0}] ", fieldName);
                }
                else
                {
                    if (viewInList && (fieldType == FieldTypeCodes.VisualEdit || fieldType == FieldTypeCodes.Textbox))
                        selectBuilder.AppendFormat(", substring(c.[{0}], 1, {1}) as [{0}]", fieldName, maxDisplayLength);
                    else
                        selectBuilder.AppendFormat(", c.[{0}]", fieldName);
                }
                var relatedAttributeId = (int?)row.Field<decimal?>("RELATED_ATTRIBUTE_ID");

                if (relatedAttributeId.HasValue)
                {
                    var tableAlias = "rel_" + relatedAttributeId;
                    var fieldAlias = "rel_field_" + relatedAttributeId;
                    var relationNumber = (int)row.Field<decimal>("RELATED_COUNT");
                    var relatedFieldName = row.Field<string>("RELATED_ATTRIBUTE_NAME");
                    var relatedContentId = (int)row.Field<decimal>("RELATED_CONTENT_ID");
                    if (relationNumber > 1)
                    {
                        tableAlias = $"{tableAlias}_{relationNumber}";
                        fieldAlias = $"{fieldAlias}_{relationNumber}";
                    }

                    var attributeTypeId = (int)row.Field<decimal>("RELATED_ATTRIBUTE_TYPE_ID");

                    var currentBlock = GetCurrentBlock(tableAlias, relatedFieldName, attributeTypeId);

                    selectBuilder.AppendFormat(", {0} AS {1}", currentBlock, fieldAlias);

                    fromBuilder.AppendFormatLine(" LEFT JOIN CONTENT_{0}_UNITED AS {1} with(nolock) ON c.[{2}] = {1}.content_item_id ", relatedContentId, tableAlias, fieldName);

                    var relatedAttributeId2 = (int?)row.Field<decimal?>("RELATED_ATTRIBUTE_ID2");
                    if (relatedAttributeId2.HasValue)
                    {
                        var relatedFieldName2 = row.Field<string>("RELATED_ATTRIBUTE_NAME2");
                        var relatedContentId2 = (int)row.Field<decimal>("RELATED_CONTENT_ID2");
                        var attributeTypeId2 = (int)row.Field<decimal>("RELATED_ATTRIBUTE_TYPE_ID2");

                        var tableAlias2 = tableAlias + "_r1";
                        var fieldAlias2 = fieldAlias + "_r1";
                        var currentBlock2 = GetCurrentBlock(tableAlias2, relatedFieldName2, attributeTypeId2);

                        selectBuilder.AppendFormat(", {0} AS {1}", currentBlock2, fieldAlias2);
                        fromBuilder.AppendFormatLine(" LEFT JOIN CONTENT_{0}_UNITED AS {1} with(nolock) ON {2}.[{3}] = {1}.content_item_id ", relatedContentId2, tableAlias2, tableAlias, relatedFieldName);
                    }
                }
            }
        }

        private static string GetCurrentBlock(string tableAlias, string relatedFieldName, int attributeTypeId)
        {
            var currentBlock = $"{tableAlias}.[{relatedFieldName}]";
            if (attributeTypeId == FieldTypeCodes.Textbox || attributeTypeId == FieldTypeCodes.VisualEdit)
            {
                currentBlock = $"cast ({currentBlock} as nvarchar(255))";
            }

            return currentBlock;
        }

        private static void AddSourcesToQuery(ArticlePageOptions options, bool useSelection, StringBuilder fromBuilder, IDictionary<int, string> fieldMap, IDictionary<int, string> referenceMap)
        {
            var tablePrefix = options.UseMainTableForVariations ? "c" : "ch";
            fromBuilder.AppendFormatLine(" dbo.CONTENT_{0}_UNITED c with(nolock) ", options.ContentId);

            if (!options.UseMainTableForVariations)
            {
                fromBuilder.AppendFormatLine(" INNER JOIN CONTENT_{0}_UNITED ch with(nolock) on c.[{1}] = ch.CONTENT_ITEM_ID", options.ContentId, options.VariationFieldName);
            }

            if (!options.IsVirtual)
            {
                fromBuilder.AppendFormatLine(" LEFT JOIN CONTENT_ITEM cil with(nolock) on {0}.CONTENT_ITEM_ID = cil.CONTENT_ITEM_ID AND LOCKED_BY IS NOT NULL", tablePrefix);
                fromBuilder.AppendFormatLine(" LEFT JOIN CONTENT_{0}_ASYNC ca with(nolock) on {1}.CONTENT_ITEM_ID = ca.CONTENT_ITEM_ID", options.ContentId, tablePrefix);
                fromBuilder.AppendFormatLine(" LEFT JOIN dbo.[USERS] lu ON cil.LOCKED_BY = lu.USER_ID");
                fromBuilder.AppendFormatLine(" LEFT JOIN dbo.[CONTENT_ITEM_SCHEDULE] sch ON {0}.CONTENT_ITEM_ID = sch.CONTENT_ITEM_ID", tablePrefix);
            }

            fromBuilder.AppendFormatLine(" LEFT JOIN dbo.[USERS] mu ON {0}.LAST_MODIFIED_BY = mu.USER_ID", tablePrefix);
            fromBuilder.AppendFormatLine(" LEFT JOIN dbo.[STATUS_TYPE] st ON {0}.STATUS_TYPE_ID = st.STATUS_TYPE_ID", tablePrefix);

            if (useSelection)
            {
                fromBuilder.AppendFormatLine(" LEFT OUTER JOIN (SELECT CONTENT_ITEM_ID from CONTENT_ITEM with(nolock) where CONTENT_ITEM_ID in (select id from @itemIds)) AS cis ON {0}.CONTENT_ITEM_ID = cis.CONTENT_ITEM_ID ", tablePrefix);
            }

            if (options.UseSecurity)
            {
                const string innerSql = "SELECT sec.content_item_id AS ALLOWED_CONTENT_ITEM_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec";
                fromBuilder.AppendFormatLine(" INNER JOIN ({0}) AS pl ON {1}.CONTENT_ITEM_ID = pl.ALLOWED_CONTENT_ITEM_ID", innerSql, tablePrefix);
            }

            foreach (var reference in options.ContentReferences.Where(reference => referenceMap.ContainsKey(reference.ReferenceFieldID)))
            {
                fromBuilder.AppendFormatLine(" LEFT JOIN dbo.CONTENT_{0}_UNITED c_{0}_{1} with(nolock) ON c.[{2}] = c_{0}_{1}.CONTENT_ITEM_ID", reference.TargetContentId, reference.ReferenceFieldID, referenceMap[reference.ReferenceFieldID]);
            }

            foreach (var contentId in options.ExstensionContentIds)
            {
                if (fieldMap.ContainsKey(contentId))
                {
                    fromBuilder.AppendFormatLine(" LEFT JOIN dbo.CONTENT_{0}_UNITED c_{0} with(nolock) ON c.CONTENT_ITEM_ID = c_{0}.{1}", contentId, fieldMap[contentId]);
                }
            }
        }

        private static void AddStaticColumnsToQuery(ArticlePageOptions options, bool useSelection, StringBuilder selectBuilder)
        {
            if (options.OnlyIds)
            {
                selectBuilder.AppendFormat(" {0}.CONTENT_ITEM_ID ", (options.UseMainTableForVariations) ? "c" : "ch");
            }
            else
            {
                const string colorString = ", CASE WHEN st.COLOR IS NOT NULL AND ST.ALT_COLOR IS NOT NULL THEN ST.STATUS_TYPE_ID ELSE NULL END AS STATUS_TYPE_COLOR";
                selectBuilder.AppendFormat(
                    " {0}.CONTENT_ITEM_ID, {0}.CREATED, {0}.MODIFIED, {0}.LAST_MODIFIED_BY, st.STATUS_TYPE_NAME{1}, CAST({0}.visible as bit) as visible",
                    options.UseMainTableForVariations ? "c" : "ch", colorString
                );

                if (!options.IsVirtual)
                {
                    selectBuilder.Append(", cil.LOCKED_BY");
                    selectBuilder.Append(", CAST((CASE WHEN (sch.content_item_id IS NOT NULL) THEN 1 ELSE 0 END) AS bit) AS scheduled");
                    selectBuilder.Append(", CAST((CASE WHEN (ca.content_item_id IS NOT NULL) THEN 1 ELSE 0 END) AS bit) AS splitted");
                    selectBuilder.Append(", lu.FIRST_NAME AS LOCKER_FIRST_NAME, lu.LAST_NAME AS LOCKER_LAST_NAME, lu.[LOGIN] AS LOCKER_LOGIN");
                }
                else
                {
                    selectBuilder.Append(", cast(NULL as numeric) AS LOCKED_BY, cast(0 as bit) AS SCHEDULED, cast(0 as bit) AS SPLITTED");
                    selectBuilder.Append(", cast(NULL as nvarchar) AS LOCKER_FIRST_NAME, cast(NULL as nvarchar) AS LOCKER_LAST_NAME, cast(NULL as nvarchar) AS LOCKER_LOGIN");
                }

                selectBuilder.Append(", mu.FIRST_NAME AS MODIFIER_FIRST_NAME, mu.LAST_NAME AS MODIFIER_LAST_NAME ,mu.[LOGIN] AS MODIFIER_LOGIN");
                selectBuilder.Append(useSelection
                    ? ", CASE WHEN (cis.CONTENT_ITEM_ID IS NOT NULL) THEN 1 ELSE 0 END as is_selected"
                    : ", 0 as is_selected");
            }
        }

        public static IEnumerable<DataRow> GetFieldsPage(SqlConnection sqlConnection, FieldPageOptions options, out int totalRecords)
        {
            var aggregatedContentIds = GetReferencedAggregatedContentIds(sqlConnection, options.ContentId);

            var useSelection = (options.SelectedIDs != null && options.SelectedIDs.Any());
            var filter = "cnt.CONTENT_ID = " + options.ContentId;

            if (options.Mode == FieldSelectMode.ForExport)
            {
                filter = "cnt.CONTENT_ID IN (" + string.Join(",", aggregatedContentIds.Union(new[] { options.ContentId })) + ")";
                filter = SqlFilterComposer.Compose(filter, "ca.AGGREGATED = 0");
                filter = SqlFilterComposer.Compose(filter, "ca.ATTRIBUTE_TYPE_ID <> 13");
            }
            else if (options.Mode == FieldSelectMode.ForExportExpanded)
            {
                filter = SqlFilterComposer.Compose(filter, "ca.ATTRIBUTE_TYPE_ID = 11");
            }

            var selectBuilder = new StringBuilder();
            selectBuilder.Append("ca.[ATTRIBUTE_ID] AS Id,  CASE WHEN (cnt.CONTENT_ID = " + options.ContentId + ") THEN [ATTRIBUTE_NAME] ELSE cnt.[CONTENT_NAME] + '.' + [ATTRIBUTE_NAME] END as [Name], [ATTRIBUTE_NAME] as [FieldName], cnt.[CONTENT_NAME] as [ContentName], ca.[CREATED] as [Created], ca.[MODIFIED] as [Modified], ATTRIBUTE_ORDER as [Order]");
            selectBuilder.Append(", FRIENDLY_NAME as FriendlyName, ca.DESCRIPTION as Description, lmb.LOGIN as LastModifiedByUser, ca.ATTRIBUTE_TYPE_ID as TypeCode");
            selectBuilder.Append(", ATTRIBUTE_SIZE as Size, REQUIRED as Required, INDEX_FLAG as Indexed, MAP_AS_PROPERTY as MapAsProperty, VIEW_IN_LIST as ViewInList, at.icon AS TypeIcon, ca.LINK_ID as LinkId");
            if (useSelection)
            {
                selectBuilder.Append(", CASE WHEN (cas.ATTRIBUTE_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");
            }

            var fromBuilder = new StringBuilder();
            fromBuilder.Append("dbo.[CONTENT] cnt INNER JOIN dbo.[CONTENT_ATTRIBUTE] ca ON cnt.CONTENT_ID = ca.CONTENT_ID");
            fromBuilder.Append(" INNER JOIN dbo.[USERS] lmb ON ca.LAST_MODIFIED_BY = lmb.USER_ID");
            fromBuilder.Append(" INNER JOIN [ATTRIBUTE_TYPE] at on ca.attribute_type_id = at.attribute_type_id");
            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT ATTRIBUTE_ID from CONTENT_ATTRIBUTE where ATTRIBUTE_ID in ({0})) AS cas ON ca.ATTRIBUTE_ID = cas.ATTRIBUTE_ID ", string.Join(",", options.SelectedIDs));
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Field,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : "[Order]",
                filter,
                options.StartRecord,
                options.PageSize,
                out totalRecords);
        }

        public static IEnumerable<DataRow> GetContentsPage(SqlConnection sqlConnection, ContentPageOptions options, out int totalRecords)
        {
            var useSelection = (options.SelectedIDs != null && options.SelectedIDs.Any());
            var selectBuilder = new StringBuilder();
            selectBuilder.Append("c.CONTENT_ID as Id, c.CONTENT_NAME as Name, c.DESCRIPTION as Description, c.CREATED as Created, c.MODIFIED as Modified, c.VIRTUAL_TYPE as VirtualType");
            selectBuilder.AppendFormat(", s.SITE_NAME as SiteName, case when cg.CONTENT_GROUP_ID = dbo.qp_default_group_id(c.SITE_ID) then dbo.qp_translate(cg.NAME, {0}) else cg.Name end as GroupName, U.LOGIN as LastModifiedByUser", options.LanguageId);
            if (useSelection)
                selectBuilder.Append(", CASE WHEN (cis.CONTENT_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");

            var fromBuilder = new StringBuilder();
            fromBuilder.Append("dbo.CONTENT c INNER JOIN dbo.SITE s ON c.SITE_ID = s.SITE_ID");
            fromBuilder.Append(" INNER JOIN dbo.[USERS] u ON c.LAST_MODIFIED_BY = U.USER_ID");
            fromBuilder.Append(" LEFT JOIN dbo.[CONTENT_GROUP] cg ON c.CONTENT_GROUP_ID = cg.CONTENT_GROUP_ID");
            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT CONTENT_ID from CONTENT where CONTENT_ID in ({0})) AS cis ON c.CONTENT_ID = cis.CONTENT_ID ", string.Join(",", options.SelectedIDs));
            }

            if (options.Mode == ContentSelectMode.ForWorkflow)
            {
                fromBuilder.Append(" LEFT JOIN dbo.[CONTENT_WORKFLOW_BIND] cwb ON c.CONTENT_ID = cwb.CONTENT_ID");
            }

            if (options.UseSecurity)
            {
                var innerSql = "SELECT sec.content_id AS ALLOWED_CONTENT_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec WHERE HIDE = 0";
                fromBuilder.AppendFormat(" INNER JOIN ({0}) AS pl ON c.CONTENT_ID = pl.ALLOWED_CONTENT_ID ", innerSql);
            }

            var filterBuilder = new StringBuilder();
            if (options.GroupId.HasValue)
            {
                if (options.GroupId.Value > 0)
                    filterBuilder.AppendFormat("C.CONTENT_GROUP_ID = {0} AND ", options.GroupId.Value);
                else
                    filterBuilder.Append("C.CONTENT_GROUP_ID IS NULL AND ");
            }

            if (!string.IsNullOrWhiteSpace(options.ContentName))
                filterBuilder.AppendFormat("C.CONTENT_NAME LIKE '%{0}%' AND ", Cleaner.ToSafeSqlLikeCondition(options.ContentName));

            if (options.Mode == ContentSelectMode.ForUnion)
            {
                filterBuilder.AppendFormat("((C.SITE_ID = {0} or c.is_shared = 1) AND VIRTUAL_TYPE IN (0, 1))", options.SiteId);
            }

            else if (options.Mode == ContentSelectMode.ForJoin)
            {
                filterBuilder.AppendFormat("((C.SITE_ID = {0} or c.is_shared = 1) AND VIRTUAL_TYPE = 0)", options.SiteId);
            }

            else if (options.Mode == ContentSelectMode.ForField)
            {
                filterBuilder.AppendFormat("(C.SITE_ID = {0})", options.SiteId);
            }

            else if (options.Mode == ContentSelectMode.ForContainer)
            {
                filterBuilder.AppendFormat("(C.SITE_ID = {0} or c.is_shared = 1) ", options.SiteId);
            }

            else if (options.Mode == ContentSelectMode.ForForm)
            {
                filterBuilder.AppendFormat("((C.SITE_ID = {0} or c.is_shared = 1) AND c.VIRTUAL_TYPE = 0) ", options.SiteId);
            }

            else if (options.Mode == ContentSelectMode.ForCustomAction)
            {
                filterBuilder.AppendFormat("1 = 1");
            }

            else
            {
                if (options.SiteId.HasValue)
                    filterBuilder.AppendFormat(" C.SITE_ID = {0} AND ", options.SiteId.Value);
                filterBuilder.AppendFormat(" C.VIRTUAL_TYPE {0} 0 ", (options.IsVirtual) ? "<>" : "=");
            }

            if (!string.IsNullOrEmpty(options.CustomFilter))
                filterBuilder.AppendFormat(" AND ({0})", options.CustomFilter);

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Content,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : ((useSelection) ? "isSelected DESC, Name ASC" : "Name ASC"),
                filterBuilder.ToString(),
                options.StartRecord,
                options.PageSize,
                out totalRecords,
                options.UserId,
                options.UseSecurity
            );
        }

        public static IEnumerable<DataRow> GetSitesPage(SqlConnection sqlConnection, SitePageOptions options, out int totalRecords)
        {
            var useSelection = (options.SelectedIDs != null && options.SelectedIDs.Any());

            var selectBuilder = new StringBuilder();
            selectBuilder.Append("s.SITE_ID as Id, s.SITE_NAME as Name, s.DESCRIPTION as Description, s.CREATED as Created, s.MODIFIED as Modified, U.LOGIN as LastModifiedByUser");
            selectBuilder.Append(", s.DNS as Dns, s.LIVE_VIRTUAL_ROOT as UploadUrl, s.IS_LIVE as IsLive, s.LOCKED_BY as LockedBy, u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName ");
            if (useSelection)
                selectBuilder.Append(", CASE WHEN (cis.SITE_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");

            var fromBuilder = new StringBuilder();
            fromBuilder.Append("dbo.SITE s INNER JOIN dbo.[USERS] u ON s.LAST_MODIFIED_BY = u.USER_ID");
            fromBuilder.Append(" LEFT JOIN dbo.[USERS] u2 ON s.LOCKED_BY = u2.USER_ID");
            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT SITE_ID from SITE s where SITE_ID in ({0})) AS cis ON s.SITE_ID = cis.SITE_ID ", string.Join(",", options.SelectedIDs));
            }
            if (options.UseSecurity)
            {
                var innerSql = "SELECT sec.SITE_ID AS ALLOWED_SITE_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec";
                fromBuilder.AppendFormat(" INNER JOIN ({0}) AS pl ON s.SITE_ID = pl.ALLOWED_SITE_ID", innerSql);
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Site,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : ((useSelection) ? "isSelected DESC, Name ASC" : "Name ASC"),
                string.Empty,
                options.StartRecord,
                options.PageSize,
                out totalRecords,
                options.UserId,
                options.UseSecurity
            );
        }

        public static IEnumerable<DataRow> GetToolbarButtonsForAction(SqlConnection sqlConnection, int userId, string actionCode, int entityId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_toolbar_buttons_list_by_action_code", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("action_code", actionCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetActionStatusList(SqlConnection sqlConnection, int userId, string actionCode, int entityId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_action_status_list", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("action_code", actionCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetMenuStatusList(SqlConnection sqlConnection, int userId, string menuCode, int entityId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_menu_status_list", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_code", menuCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetChildTreeNodeList(SqlConnection sqlConnection, int userId, string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId = 0)
        {
            using (var cmd = SqlCommandFactory.Create("qp_expand", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("code", string.IsNullOrEmpty(entityTypeCode) ? DBNull.Value : (object)entityTypeCode);
                cmd.Parameters.AddWithValue("id", (parentEntityId.HasValue) ? parentEntityId.Value : 0);
                cmd.Parameters.AddWithValue("is_folder", isFolder);
                cmd.Parameters.AddWithValue("is_group", isGroup);
                cmd.Parameters.AddWithValue("group_item_code", string.IsNullOrEmpty(groupItemCode) ? DBNull.Value : (object)groupItemCode);
                cmd.Parameters.AddWithValue("filter_id", entityId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static DataRow GetContextMenuById(SqlConnection sqlConnection, int userId, int menuId, bool loadRelatedData = false)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_context_menu_by_id", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_id", menuId);
                cmd.Parameters.AddWithValue("load_related_data", loadRelatedData);

                return ContextMenuDataSetFill(cmd);
            }
        }


        public static DataRow GetContextMenuByCode(SqlConnection sqlConnection, int userId, string menuCode, bool loadRelatedData = false)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_context_menu_by_code", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_code", menuCode);
                cmd.Parameters.AddWithValue("load_related_data", loadRelatedData);

                return ContextMenuDataSetFill(cmd);
            }
        }

        private static DataRow ContextMenuDataSetFill(SqlCommand cmd)
        {
            var dataAdapter = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            dataAdapter.Fill(ds);

            if (ds.Tables.Count > 0)
            {
                if (ds.Tables.Count == 1)
                {
                    var itemsDT = new DataTable();
                    itemsDT.Columns.Add("CONTEXT_MENU_ID", ds.Tables[0].Columns["ID"].DataType);
                    ds.Tables.Add(itemsDT);
                }
                ds.Relations.Add("Menu2Item",
                    ds.Tables[0].Columns["ID"],
                    ds.Tables[1].Columns["CONTEXT_MENU_ID"]
                );
                return ds.Tables[0].AsEnumerable().FirstOrDefault();
            }
            return null;
        }

        public static IEnumerable<DataRow> GetContextMenusList(SqlConnection sqlConnection, int userId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_get_context_menus_list", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }


        public static DataTable GetBaseFieldsForM2O(SqlConnection sqlConnection, int contentId, int fieldId)
        {
            var sb = new StringBuilder();
            sb.Append("select s.site_id, s.site_name, c.content_id, c.content_name, ca.attribute_name, ca.attribute_id from CONTENT_ATTRIBUTE ca ");
            sb.Append("inner join content c on ca.CONTENT_ID = c.content_id ");
            sb.Append("inner join site s on c.SITE_ID = s.site_id ");
            sb.Append("where related_attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id) ");
            sb.Append("and not exists(select * from CONTENT_ATTRIBUTE where ATTRIBUTE_ID <> @field_id AND BACK_RELATED_ATTRIBUTE_ID = ca.attribute_id) ");
            sb.Append("and c.virtual_type = 0");

            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@field_id", fieldId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }


        public static void UpdateChildDelayedSchedule(SqlConnection connection, int articleId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_copy_schedule_to_child_delays", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", articleId);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool CheckRelationCondition(SqlConnection connection, int articleId, int contentId, string relCondition)
        {
            using (var cmd = SqlCommandFactory.Create(string.Format("select count(content_item_id) as cnt from content_{0}_united c with(nolock) where content_item_id = @id and ({1})", contentId, relCondition), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", articleId);
                return (int)cmd.ExecuteScalar() != 0;
            }
        }

        public static string GetCurrentDBVersion(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("qp_versions", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetString(1);
                    }
                }
            }
            return string.Empty;
        }

        private static readonly string APPLY_FIELD_DEFAULT_VALUE_GET_ITEM_IDS_TO_PROCESS_QUERY =
            "select i.CONTENT_ITEM_ID " +
            "from content_item as i " +
            "left outer join CONTENT_DATA d " +
                "ON d.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID and d.ATTRIBUTE_ID = @attr_id " +
            "WHERE " +
                "d.data IS NULL " +
                "and I.CONTENT_ID = @content_id";
        private static readonly string APPLY_FIELD_DEFAULT_VALUE_GET_ITEM_IDS_TO_PROCESS_QUERY_FOR_BLOB =
            "select i.CONTENT_ITEM_ID " +
            "from content_item as i " +
            "left outer join CONTENT_DATA d " +
                "ON d.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID and d.ATTRIBUTE_ID = @attr_id " +
            "WHERE " +
                "d.BLOB_DATA IS NULL " +
                "and I.CONTENT_ID = @content_id";

        private static readonly string APPLY_FIELD_DEFAULT_VALUE_GET_M2M_ITEM_IDS_TO_PROCESS_QUERY =
        " select i.CONTENT_ITEM_ID from content_item as i WHERE i.CONTENT_ID = @content_id and " +
        " not exists (select * from [item_link_united] where item_id = i.CONTENT_ITEM_ID and link_id = @link_id ) ";

        public static IEnumerable<int> ApplyFieldDefaultValue_GetM2MItemIdsToProcess(int contentId, int fieldId, string linkId, SqlConnection connection)
        {
            var query = APPLY_FIELD_DEFAULT_VALUE_GET_M2M_ITEM_IDS_TO_PROCESS_QUERY;
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@attr_id", fieldId);
                cmd.Parameters.AddWithValue("@link_id", linkId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => Converter.ToInt32(r.Field<decimal>(0)))
                    .ToArray();
            }
        }

        public static IEnumerable<int> ApplyFieldDefaultValue_GetItemIdsToProcess(int contentId, int fieldId, bool isBlob, SqlConnection connection)
        {
            string query;
            query = isBlob ? APPLY_FIELD_DEFAULT_VALUE_GET_ITEM_IDS_TO_PROCESS_QUERY_FOR_BLOB : APPLY_FIELD_DEFAULT_VALUE_GET_ITEM_IDS_TO_PROCESS_QUERY;
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@attr_id", fieldId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => Converter.ToInt32(r.Field<decimal>(0)))
                    .ToArray();
            }
        }

        private static readonly string APPLY_FIELD_DEFAULT_VALUE_SET_DEFAULT_VALUE_QUERY_TEMPLATE =
            "UPDATE content_data " +
              "SET {0} = (SELECT {1} FROM content_attribute WHERE attribute_id = @attr_id) " +
            "WHERE {0} IS NULL " +
            "AND attribute_id = @attr_id " +
            "AND CONTENT_ITEM_ID in ({2}) " +
            "INSERT INTO content_data (attribute_id, content_item_id, {0}) " +
            "select @attr_id, i.content_item_id, a.{1} from content_item as i " +
            "LEFT OUTER JOIN content_attribute AS a " +
                  "ON a.content_id = i.content_id AND a.attribute_id = @attr_id " +
            "where i.CONTENT_ITEM_ID in ({2}) and " +
            "i.CONTENT_ITEM_ID not in (SELECT d.content_item_id FROM content_data AS d WHERE d.attribute_id = @attr_id)";

        public static void ApplyFieldDefaultValue_SetDefaultValue(int contentId, int fieldId, bool isBlob, bool isM2M, IEnumerable<int> idsForStep, SqlConnection connection)
        {
            string query;
            var contentDataColumn = isBlob ? "BLOB_DATA" : "DATA";
            var attributeDefValueColumn = isBlob ? "DEFAULT_BLOB_VALUE" : "DEFAULT_VALUE";
            var contentItemsIds = string.Join(",", idsForStep);
            query = string.Format(APPLY_FIELD_DEFAULT_VALUE_SET_DEFAULT_VALUE_QUERY_TEMPLATE,
                contentDataColumn, attributeDefValueColumn, contentItemsIds);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ApplyM2MFieldDefaultValue_SetDefaultValue(int contentId, int fieldId, string linkId, IEnumerable<int> idsForStep, bool symmetric, SqlConnection connection)
        {
            var query = new StringBuilder();
            query.AppendLine("INSERT INTO [item_to_item] ([link_id], [l_item_id], [r_item_id])");
            query.AppendFormatLine("select  @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab cross join CONTENT_ITEM as ci where ci.CONTENT_ITEM_ID in ({0}) and ci.SPLITTED = 0 and ab.FIELD_ID = @fieldId", string.Join(",", idsForStep));
            query.AppendLine("INSERT INTO [item_link_async] ([link_id],[item_id],[linked_item_id])");
            query.AppendFormatLine("select  @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab cross join CONTENT_ITEM as ci where ci.CONTENT_ITEM_ID in ({0}) and ci.SPLITTED = 1 and ab.FIELD_ID = @fieldId", string.Join(",", idsForStep));
            if (symmetric)
            {
                query.AppendLine("INSERT INTO [item_link_async] ([link_id], [item_id], [linked_item_id])");
                query.AppendFormatLine(" select  @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab cross join CONTENT_ITEM as ci inner join CONTENT_ITEM as cii on cii.CONTENT_ITEM_ID = ab.ARTICLE_ID " +
                " where ci.CONTENT_ITEM_ID in ({0}) and ci.SPLITTED = 0 and cii.SPLITTED = 1 and ab.FIELD_ID = @fieldId", string.Join(",", idsForStep));
            }
            using (var cmd = SqlCommandFactory.Create(query.ToString(), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@linkID", linkId);
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> RecreateDynamicImages_GetDataToProcess(int imageFieldId, SqlConnection connection)
        {
            var query = "select content_item_id AS ID, DATA from content_data where ATTRIBUTE_ID = @attr_id and DATA is not null";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", imageFieldId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void RecreateDynamicImages_UpdateDynamicFieldValue(int dynamicFieldId, int articleId, string newValue, SqlConnection connection)
        {

            var sb = new StringBuilder();
            sb.AppendLine("if exists(select content_data_id from content_data where ATTRIBUTE_ID = @attr_id and CONTENT_ITEM_ID = @item_id)");
            sb.AppendLine("	update content_data set data = @new_data where ATTRIBUTE_ID = @attr_id and CONTENT_ITEM_ID = @item_id");
            sb.AppendLine("else	insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA) values(@item_id, @attr_id, @new_data)");
            using (var cmd = SqlCommandFactory.Create(sb.ToString(), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", dynamicFieldId);
                cmd.Parameters.AddWithValue("@item_id", articleId);
                cmd.Parameters.AddWithValue("@new_data", newValue);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> RemovingActions_GetContentsItemInfo(int? siteId, int? contentId, SqlConnection connection)
        {
            var query =
                "select S.SITE_ID, S.SITE_NAME, C.CONTENT_ID, C.CONTENT_NAME, ISNULL(I.[ITEMS_COUNT], 0) AS ITEMS_COUNT from " +
                    "(select  content_id, count(CONTENT_ITEM_ID) [ITEMS_COUNT] from content_item group by content_id) I " +
                    "RIGHT JOIN CONTENT C ON C.CONTENT_ID = I.CONTENT_ID " +
                    "JOIN [SITE] S ON S.SITE_ID = C.SITE_ID " +
                "where (c.content_id = @content_id OR @content_id is null) " +
                    "and (s.site_id = @site_id OR @site_id is null)";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add(new SqlParameter("@content_id", SqlDbType.Int) { Value = (contentId.HasValue) ? (object)contentId.Value : DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@site_id", SqlDbType.Int) { Value = (siteId.HasValue) ? (object)siteId.Value : DBNull.Value });

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int RemovingActions_RemoveContentItems(int contentId, int itemsToDelete, SqlConnection connection)
        {
            var query = "select 1 as A into #disable_td_delete_item_o2m_nullify; " +
                "delete FROM CONTENT_ITEM WHERE CONTENT_ITEM_ID in (" +
                "	select top(@deleted_count) CONTENT_ITEM_ID from CONTENT_ITEM where CONTENT_ID = @content_id order by CONTENT_ITEM_ID " +
                ")";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@deleted_count", itemsToDelete);
                return cmd.ExecuteNonQuery();
            }
        }

        public static int RemovingActions_ClearO2MRelations(int contentId, SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("qp_clear_relations", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@parent_id", contentId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetSharedUnionBaseContentInfo(int siteId, SqlConnection connection)
        {
            var query =
                "select r.virtual_content_id, " +
                    "r.union_content_id as base_content_id, " +
                    "bs.SITE_ID as base_site_id, " +
                    "us.site_id as union_site_id, " +
                    "bc.CONTENT_NAME as base_content_name, " +
                    "uc.CONTENT_NAME as union_content_name, " +
                    "bs.SITE_NAME as base_site_name, " +
                    "us.SITE_NAME as union_site_name " +
                "from union_contents r " +
                "join CONTENT as bc on bc.CONTENT_ID = r.union_content_id " +
                "join [SITE] bs on bc.SITE_ID = bs.SITE_ID " +
                "join CONTENT as uc on uc.CONTENT_ID = r.virtual_content_id " +
                "join [SITE] us on uc.SITE_ID = us.SITE_ID " +
                "where us.site_id <> bs.site_id AND us.site_id <> @site_id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetSharedRelatedContentInfo(int siteId, SqlConnection connection)
        {
            var query =
                "select s.SITE_ID, " +
                    "s.SITE_NAME, " +
                    "c.CONTENT_ID, " +
                    "c.CONTENT_NAME, " +
                    "rs.SITE_ID as rel_site_id, " +
                    "rs.SITE_NAME as rel_site_name, " +
                    "rc.CONTENT_ID as rel_content_id, " +
                    "rc.CONTENT_NAME as rel_content_name " +
                "from content_attribute a " +
                "join content c on c.CONTENT_ID = a.CONTENT_ID " +
                "join [SITE] s on c.SITE_ID = s.SITE_ID " +
                "join content_attribute ra on ra.RELATED_ATTRIBUTE_ID = a.ATTRIBUTE_ID " +
                "join content rc on rc.CONTENT_ID = ra.CONTENT_ID " +
                "join [SITE] rs on rc.SITE_ID = rs.SITE_ID " +
                "where rs.site_id <> s.site_id AND s.site_id = @site_id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int GetSiteArticleCount(int siteId, SqlConnection connection)
        {
            var query =
                "select count(CONTENT_ITEM_ID) from CONTENT_ITEM I " +
                "join CONTENT C ON C.CONTENT_ID = I.CONTENT_ID " +
                "where C.SITE_ID = @site_id";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static int GetSiteContentCount(int siteId, SqlConnection connection)
        {
            var query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static int RemovingActions_RemoveSiteArticles(int siteId, int articleToRemove, SqlConnection connection)
        {
            var query =
                "select 1 as A into #disable_td_delete_item_o2m_nullify; " +
                "DELETE FROM content_item where content_item_id in (select top {0} I.content_item_id from CONTENT_ITEM I " +
                "inner join CONTENT C ON C.CONTENT_ID = I.CONTENT_ID " +
                "where C.SITE_ID = @site_id " +
                "order by I.content_id ASC, I.content_item_id ASC) ";
            query = string.Format(query, articleToRemove);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return cmd.ExecuteNonQuery();
            }
        }



        public static IEnumerable<int> RemovingActions_BatchRemoveContents(int siteId, int contentsToRemove, SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("qp_batch_delete_contents", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                cmd.Parameters.AddWithValue("@count_to_del", contentsToRemove);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => Converter.ToInt32(r.Field<decimal>(0)))
                    .ToArray();
            }
        }

        /// <summary>
        /// Возвращает список ID шаблонов сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<int> AssembleAction_GetSiteTemplatesId(int siteId, SqlConnection connection)
        {
            var result = new List<int>();
            var query = "select PAGE_TEMPLATE_ID from PAGE_TEMPLATE where SITE_ID = @site_id order by PAGE_TEMPLATE_ID";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        public static IEnumerable<DataRow> AssembleAction_GetSitePages(int siteId, SqlConnection connection)
        {
            var result = new List<int>();
            var query = "select P.PAGE_ID Id, T.TEMPLATE_NAME Template, p.PAGE_NAME Name from PAGE P JOIN PAGE_TEMPLATE T ON P.PAGE_TEMPLATE_ID = T.PAGE_TEMPLATE_ID where T.SITE_ID = @site_id order by P.PAGE_ID";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static int AssembleAction_UpdatePageStatus(int pageId, int userId, SqlConnection connection)
        {
            var query = "update page set reassemble = 0, assembled = getdate(), last_assembled_by = @user_Id where page_id = @page_Id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@page_Id", pageId);
                cmd.Parameters.AddWithValue("@user_Id", userId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<int> AssembleAction_GetSiteFormatIds(int siteId, SqlConnection connection)
        {
            var result = new List<int>();
            var query = "select N.FORMAT_ID from NOTIFICATIONS N JOIN CONTENT C ON C.CONTENT_ID = N.CONTENT_ID WHERE N.FORMAT_ID is not null and C.SITE_ID = @site_id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        ///// <summary>
        ///// Возвращает страницу пользователей
        ///// </summary>
        //public static IEnumerable<DataRow> GetSimplePagedList(SqlConnection sqlConnection, string entityTypeCode, string selectBlock, string fromBlock, string orderBy, string filter, int startRow, int pageSize, out int totalRecords, bool countOnly = false)
        //{
        //    using (SqlCommand cmd = SqlCommandFactory.Create("qp_get_paged_data", sqlConnection))
        //    {
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@select_block", selectBlock);
        //        cmd.Parameters.AddWithValue("@from_block", fromBlock);
        //        cmd.Parameters.AddWithValue("@entity_name", entityTypeCode);


        //        if (!String.IsNullOrEmpty(orderBy))
        //            cmd.Parameters.AddWithValue("order_by_block", orderBy);

        //        if (!String.IsNullOrEmpty(filter))
        //            cmd.Parameters.AddWithValue("where_block", filter);

        //        cmd.Parameters.AddWithValue("count_only", countOnly);
        //        cmd.Parameters.Add(new SqlParameter("total_records", SqlDbType.Int) { Direction = ParameterDirection.Output });
        //        cmd.Parameters.AddWithValue("start_row", startRow);
        //        cmd.Parameters.AddWithValue("page_size", pageSize);

        //        DataSet ds = new DataSet();
        //        new SqlDataAdapter(cmd).Fill(ds);

        //        DataTable result = null;
        //        totalRecords = 0;

        //        if (ds.Tables.Count > 0)
        //        {
        //            result = ds.Tables[0];
        //            totalRecords = (result.Rows.Count != 0) ? (int)result.Rows[0][QP8Entities.COUNT_COLUMN] : 0;
        //        }
        //        else if (countOnly)
        //        {
        //            totalRecords = (int)cmd.Parameters["total_records"].Value;
        //        }

        //        return result.AsEnumerable().ToArray();
        //    }
        //}

        public static int CopyUser(int sourceUserId, string newLogin, int currentUserId, SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("qp_copy_user", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@user_id", sourceUserId);
                cmd.Parameters.AddWithValue("@create_by_id", currentUserId);
                cmd.Parameters.AddWithValue("@new_login", newLogin);
                cmd.Parameters.Add(new SqlParameter("@new_user_id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.ExecuteNonQuery();
                return Converter.ToInt32(cmd.Parameters["@new_user_id"].Value, 0);
            }
        }

        /// <summary>
        /// Отфильтровать пользователей прямо или косвенно входящих в группу администраторов
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<int> UserGroups_SelectAdminDescendantGroupUserIDs(IEnumerable<int> userIds, int groupId, SqlConnection connection)
        {
            var result = new List<int>();
            var queryTemplate = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(1 as numeric) as Child_Group_Id, 0 as [Level]
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, [Level] + 1 as [Level]
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            select DISTINCT U.[USER_ID], U.[LOGIN]
                            from G2G AS G
                            join USER_GROUP_BIND GB ON GB.GROUP_ID = G.CHILD_GROUP_ID OR GB.GROUP_ID = G.Parent_Group_Id
                            JOIN USERS U ON GB.[USER_ID] = U.[USER_ID]
                            where GB.[USER_ID] in ({0}) and GB.GROUP_ID <> @group_id";
            var query = string.Format(queryTemplate, string.Join(",", userIds));

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@group_id", groupId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Входит ли группа в иерархию группы Администраторы
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static bool UserGroups_IsGroupAdminDescendant(int groupId, SqlConnection connection)
        {
            var query = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(1 as numeric) as Child_Group_Id, 0 as [Level]
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, [Level] + 1 as [Level]
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT CASE WHEN EXISTS(SELECT * FROM G2G where Child_Group_Id = @group_id) THEN 1 ELSE 0 END";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@group_id", groupId);
                return Converter.ToBoolean(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Проверит возможность образования цикла в иерархии групп
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static bool UserGroups_IsCyclePossible(int groupId, int parentGroupId, SqlConnection connection)
        {
            var query = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(@group_id as numeric) as Child_Group_Id, 0 as [Level]
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, [Level] + 1 as [Level]
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT CASE WHEN EXISTS(select * from G2G where Parent_Group_Id = @parent_group_id or Child_Group_Id = @parent_group_id) THEN 1 ELSE 0 END";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@group_id", groupId);
                cmd.Parameters.AddWithValue("@parent_group_id", parentGroupId);
                return Converter.ToBoolean(cmd.ExecuteScalar());
            }
        }

        public static IEnumerable<int> UserGroups_SelectWorkflowGroupUserIDs(int[] userIds, SqlConnection connection)
        {
            var result = new List<int>();
            var queryTemplate = @"select GB.[USER_ID] from dbo.USER_GROUP_BIND GB
                                    join dbo.USER_GROUP G ON G.GROUP_ID = GB.GROUP_ID
                                    WHERE G.USE_PARALLEL_WORKFLOW = 1 and GB.[USER_ID] in ({0})";
            var query = string.Format(queryTemplate, string.Join(",", userIds));

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает иерархию группы Администраторы
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<DataRow> UserGroups_GetAdministratorsHierarhy(SqlConnection connection)
        {
            var query = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(1 as numeric) as Child_Group_Id, 0 as [Level]
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, [Level] + 1 as [Level]
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT Parent_Group_Id as PARENT, Child_Group_Id AS CHILD, [LEVEL] FROM G2G";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int CopyUserGroup(int sourceGroupId, string newName, int currentUserId, SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("qp_copy_user_group", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@group_id", sourceGroupId);
                cmd.Parameters.AddWithValue("@create_by_id", currentUserId);
                cmd.Parameters.AddWithValue("@new_name", newName);
                cmd.Parameters.Add(new SqlParameter("@new_group_id", SqlDbType.Int) { Direction = ParameterDirection.Output });
                cmd.ExecuteNonQuery();
                return Converter.ToInt32(cmd.Parameters["@new_group_id"].Value, 0);
            }
        }

        public static IEnumerable<DataRow> GetSitePermissionPage(SqlConnection sqlConnection, int siteId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.[SITE_ACCESS_ID] AS [ID]
                                      ,U.[LOGIN] AS [UserLogin]
                                      ,G.GROUP_NAME AS [GroupName]
                                      ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                      ,SA.[propagate_to_contents] as [PropagateToItems]
                                      ,cast(0 as bit) as [Hide]
                                      ,SA.[CREATED]
                                      ,SA.[MODIFIED]
                                      ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                      ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[SITE_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[SITE_ID] = " + siteId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetContentPermissionPage(SqlConnection sqlConnection, int contentId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.[CONTENT_ACCESS_ID] AS [ID]
                                      ,U.[LOGIN] AS [UserLogin]
                                      ,G.GROUP_NAME AS [GroupName]
                                      ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                      ,SA.[PROPAGATE_TO_ITEMS] as [PropagateToItems]
                                      ,SA.[HIDE] as [Hide]
                                      ,SA.[CREATED]
                                      ,SA.[MODIFIED]
                                      ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                      ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[CONTENT_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[CONTENT_ID] = " + contentId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetArticlePermissionPage(SqlConnection sqlConnection, int articleId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.[CONTENT_ITEM_ACCESS_ID] AS [ID]
                                      ,U.[LOGIN] AS [UserLogin]
                                      ,G.GROUP_NAME AS [GroupName]
                                      ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                      ,cast(0 as numeric(18, 0)) as [PropagateToItems]
                                      ,cast(0 as bit) as [Hide]
                                      ,SA.[CREATED]
                                      ,SA.[MODIFIED]
                                      ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                      ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[CONTENT_ITEM_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[CONTENT_ITEM_ID] = " + articleId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetWorkflowPermissionPage(SqlConnection sqlConnection, int workflowId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.[WORKFLOW_ACCESS_ACCESS_ID] AS [ID]
                                      ,U.[LOGIN] AS [UserLogin]
                                      ,G.GROUP_NAME AS [GroupName]
                                      ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                      ,cast(0 as numeric(18, 0)) as [PropagateToItems]
                                      ,cast(0 as bit) as [Hide]
                                      ,SA.[CREATED]
                                      ,SA.[MODIFIED]
                                      ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                      ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[WORKFLOW_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[WORKFLOW_ID] = " + workflowId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetSiteFolderPermissionPage(SqlConnection sqlConnection, int folderId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.[FOLDER_ACCESS_ID] AS [ID]
                                      ,U.[LOGIN] AS [UserLogin]
                                      ,G.GROUP_NAME AS [GroupName]
                                      ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                      ,cast(0 as numeric(18, 0)) as [PropagateToItems]
                                      ,cast(0 as bit) as [Hide]
                                      ,SA.[CREATED]
                                      ,SA.[MODIFIED]
                                      ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                      ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[FOLDER_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[FOLDER_ID] = " + folderId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }


        public static IEnumerable<DataRow> GetEntityTypePermissionPage(SqlConnection sqlConnection, int entityTypeId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.ENTITY_TYPE_ACCESS_ID AS [ID]
                                    ,U.[LOGIN] AS [UserLogin]
                                    ,G.GROUP_NAME AS [GroupName]
                                    ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                    ,cast(0 as numeric(18, 0)) as [PropagateToItems]
                                    ,cast(0 as bit) as [Hide]
                                    ,SA.[CREATED]
                                    ,SA.[MODIFIED]
                                    ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                    ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[ENTITY_TYPE_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[ENTITY_TYPE_ID] = " + entityTypeId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetActionPermissionPage(SqlConnection sqlConnection, int actionId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"SA.ACTION_ACCESS_ID AS [ID]
                                    ,U.[LOGIN] AS [UserLogin]
                                    ,G.GROUP_NAME AS [GroupName]
                                    ,L.PERMISSION_LEVEL_NAME AS [LevelName]
                                    ,cast(0 as numeric(18, 0)) as [PropagateToItems]
                                    ,cast(0 as bit) as [Hide]
                                    ,SA.[CREATED]
                                    ,SA.[MODIFIED]
                                    ,SA.[LAST_MODIFIED_BY] AS [LastModifiedByUserId]
                                    ,U2.[LOGIN] AS [LastModifiedByUser]";
            var fromBlock = @"[ACTION_ACCESS] SA
                                    LEFT JOIN [USERS] U ON U.[USER_ID] = SA.[USER_ID]
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN [USERS] U2 ON U2.[USER_ID] = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : "") + "[ACTION_ID] = " + actionId;

            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.SitePermission, selectBlock, fromBlock,
                    orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetUserGroupPage(SqlConnection sqlConnection, IEnumerable<int> selectedIDs, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var selectBlock = @"G.[GROUP_ID] AS Id,G.[GROUP_NAME] AS Name,G.[DESCRIPTION] AS Description,G.[CREATED],G.[MODIFIED],G.[LAST_MODIFIED_BY] AS LastModifiedByUserId,G.[LAST_MODIFIED_BY_LOGIN] AS LastModifiedByUser,G.[shared_content_items] AS SharedArticles";
            var fromBlock = @"[dbo].[USER_GROUP_TREE] G";

            if (selectedIDs != null && selectedIDs.Any())
            {
                selectBlock += " ,CAST((CASE WHEN (GIS.[GROUP_ID] IS NOT NULL) THEN 1 else 0 end) AS bit) AS IS_SELECTED";
                fromBlock += string.Format(" LEFT OUTER JOIN (select [GROUP_ID] from [USER_GROUP_TREE] where [GROUP_ID] in ({0})) AS GIS ON GIS.[GROUP_ID] = G.[GROUP_ID]", string.Join(",", selectedIDs));
                orderBy = string.IsNullOrWhiteSpace(orderBy) ? "IS_SELECTED DESC, Name ASC" : orderBy;
            }
            else
                orderBy = string.IsNullOrWhiteSpace(orderBy) ? "Name ASC" : orderBy;


            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.UserGroup, selectBlock, fromBlock,
                    orderBy, filter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetUserPage(SqlConnection sqlConnection, UserPageOptions options, out int totalRecords)
        {
            string strFilter = null;
            string orderBy = null;
            var filters = new List<string>(4);
            if (!string.IsNullOrEmpty(options.Login))
                filters.Add(string.Format("U.[LOGIN] LIKE '%{0}%'", options.Login));
            if (!string.IsNullOrEmpty(options.Email))
                filters.Add(string.Format("U.[EMAIL] LIKE '%{0}%'", options.Email));
            if (!string.IsNullOrEmpty(options.FirstName))
                filters.Add(string.Format("U.[FIRST_NAME] LIKE '%{0}%'", options.FirstName));
            if (!string.IsNullOrEmpty(options.LastName))
                filters.Add(string.Format("U.[LAST_NAME] LIKE '%{0}%'", options.LastName));

            if (filters.Any())
                strFilter = string.Join(" AND ", filters);

            var selectBlock = @"U.[USER_ID] as ID, U.[LOGIN], U.FIRST_NAME, U.LAST_NAME, U.EMAIL, U.LANGUAGE_ID, L.LANGUAGE_NAME, U.LAST_LOGIN, U.[DISABLED], U.CREATED, U.MODIFIED, U.LAST_MODIFIED_BY, MU.[LOGIN] as LAST_MODIFIED_BY_LOGIN";
            var fromBlock = @"[dbo].[USERS] U JOIN [dbo].[USERS] MU ON MU.[USER_ID] = U.LAST_MODIFIED_BY JOIN [dbo].LANGUAGES L ON L.LANGUAGE_ID =  U.LANGUAGE_ID";

            if (options.SelectedIDs != null && options.SelectedIDs.Any())
            {
                selectBlock += " ,CAST((CASE WHEN (UIS.[USER_ID] IS NOT NULL) THEN 1 else 0 end) AS bit) AS is_selected";
                fromBlock += string.Format(" LEFT OUTER JOIN (select [USER_ID] from [USERS] where [USER_ID] in ({0})) AS UIS ON UIS.[USER_ID] = U.[USER_ID]", string.Join(",", options.SelectedIDs));
                orderBy = string.IsNullOrWhiteSpace(options.SortExpression) ? "is_selected DESC, LOGIN ASC" : options.SortExpression;
            }
            else
                orderBy = string.IsNullOrWhiteSpace(options.SortExpression) ? "LOGIN ASC" : options.SortExpression;


            return GetSimplePagedList(sqlConnection,
                    EntityTypeCode.User, selectBlock, fromBlock,
                    orderBy, strFilter, options.StartRecord, options.PageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetChildContentPermissionsForUser(SqlConnection sqlConnection, int siteId, int userId, string orderBy, int startRow, int pageSize, out int totalRecords, int? contentId = null)
        {
            var fromBlock = @"(select C.CONTENT_ID AS ID, C.CONTENT_NAME AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                CAST((case when P2.[USER_ID] IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit,
                                CAST(ISNULL(P2.[PROPAGATE_TO_ITEMS], 0) AS BIT) AS PropagateToItems,
                                CAST(ISNULL(P2.[HIDE], 0) AS BIT) AS Hide,
                                L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                 LEFT JOIN content_access_PermLevel_site P2 ON P1.CONTENT_ID = P2.CONTENT_ID and P1.permission_level = p2.permission_level and P2.[USER_ID] = {0}
                                 RIGHT JOIN CONTENT C ON P1.CONTENT_ID = C.CONTENT_ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE C.SITE_ID = {1}) AS TR";
            fromBlock = string.Format(fromBlock, userId, siteId);

            string filter = null;
            if (contentId.HasValue)
                filter = "ID = " + contentId.Value;

            return GetSimplePagedList(sqlConnection, EntityTypeCode.Content, "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords,
                    useSecurity: true, userId: userId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Site, parentEntityId: siteId);
        }

        public static DataRow GetChildContentPermissionForUser(SqlConnection sqlConnection, int siteId, int contentId, int userId)
        {
            int totalRecords;
            var rows = GetChildContentPermissionsForUser(sqlConnection, siteId, userId, "ID asc", 1, 1, out totalRecords, contentId);
            return rows.FirstOrDefault();
        }

        public static IEnumerable<DataRow> GetChildContentPermissionsForGroup(SqlConnection sqlConnection, int siteId, int groupId, string orderBy, int startRow, int pageSize, out int totalRecords, int? contentId = null)
        {
            var fromBlock = @"(select C.CONTENT_ID AS ID, C.CONTENT_NAME AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                CAST((case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit,
                                CAST(ISNULL(P2.[PROPAGATE_TO_ITEMS], 0) AS BIT) AS PropagateToItems,
                                CAST(ISNULL(P2.[HIDE], 0) AS BIT) AS Hide,
                                L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                 LEFT JOIN content_access_PermLevel_site P2 ON P1.CONTENT_ID = P2.CONTENT_ID and P1.permission_level = p2.permission_level and P2.GROUP_ID = {0}
                                 RIGHT JOIN CONTENT C ON P1.CONTENT_ID = C.CONTENT_ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE C.SITE_ID = {1}) AS TR";
            fromBlock = string.Format(fromBlock, groupId, siteId);

            string filter = null;
            if (contentId.HasValue)
                filter = "ID = " + contentId.Value;

            return GetSimplePagedList(sqlConnection, EntityTypeCode.Content, "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords,
                    useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Site, parentEntityId: siteId);
        }

        public static DataRow GetChildContentPermissionForGroup(SqlConnection sqlConnection, int siteId, int contentId, int groupId)
        {
            int totalRecords;
            var rows = GetChildContentPermissionsForGroup(sqlConnection, siteId, groupId, "ID asc", 1, 1, out totalRecords, contentId);
            return rows.FirstOrDefault();
        }

        public static IEnumerable<DataRow> GetChildArticlePermissionsForUser(SqlConnection sqlConnection, int contentId, int userId, string titleFieldName, string orderBy, int startRow, int pageSize, out int totalRecords, int? articleId = null)
        {
            var fromBlock = @"(select CI.CONTENT_ITEM_ID AS ID, cast(CI.[{2}] as nvarchar(1024)) AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                CAST((case when P2.[USER_ID] IS NOT NULL THEN 1 ELSE 0 END) AS BIT)  AS IsExplicit
                                ,CAST(0 AS BIT) AS PropagateToItems
                                ,CAST(0 AS BIT) AS Hide
                                ,L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                LEFT JOIN content_item_access_PermLevel_content P2 ON P1.CONTENT_ITEM_ID = P2.CONTENT_ITEM_ID and P1.PERMISSION_LEVEL = p2.PERMISSION_LEVEL and P2.[USER_ID] = {0}
                                LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                RIGHT JOIN CONTENT_{1} CI with(nolock) ON CI.CONTENT_ITEM_ID = P1.CONTENT_ITEM_ID) AS TR";
            fromBlock = string.Format(fromBlock, userId, contentId, titleFieldName);

            string filter = null;
            if (articleId.HasValue)
                filter = "ID = " + articleId.Value;

            return GetSimplePagedList(sqlConnection, "content_item", "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords,
                    useSecurity: true, userId: userId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Content, parentEntityId: contentId);
        }

        public static DataRow GetChildArticlePermissionForUser(SqlConnection sqlConnection, int contentId, int articleId, int userId)
        {
            int totalRecords;
            var rows = GetChildArticlePermissionsForUser(sqlConnection, contentId, userId, "CONTENT_ITEM_ID", "ID asc", 1, 1, out totalRecords, articleId);
            return rows.FirstOrDefault();
        }


        public static IEnumerable<DataRow> GetChildArticlePermissionsForGroup(SqlConnection sqlConnection, int contentId, int groupId, string titleFieldName, string orderBy, int startRow, int pageSize, out int totalRecords, int? articleId = null)
        {
            var fromBlock = @"(select CI.CONTENT_ITEM_ID AS ID, cast(CI.[{2}] as nvarchar(1024)) AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                CAST((case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit
                                ,CAST(0 AS BIT) AS PropagateToItems
                                ,CAST(0 AS BIT) AS Hide
                                ,L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                LEFT JOIN content_item_access_PermLevel_content P2 ON P1.CONTENT_ITEM_ID = P2.CONTENT_ITEM_ID and P1.PERMISSION_LEVEL = p2.PERMISSION_LEVEL and P2.GROUP_ID = {0}
                                LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                RIGHT JOIN CONTENT_{1} CI with(nolock) ON CI.CONTENT_ITEM_ID = P1.CONTENT_ITEM_ID) AS TR";
            fromBlock = string.Format(fromBlock, groupId, contentId, titleFieldName);

            string filter = null;
            if (articleId.HasValue)
                filter = "ID = " + articleId.Value;

            return GetSimplePagedList(sqlConnection, "content_item", "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords,
                    useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Content, parentEntityId: contentId);
        }

        public static DataRow GetChildArticlePermissionForGroup(SqlConnection sqlConnection, int contentId, int articleId, int groupId)
        {
            int totalRecords;
            var rows = GetChildArticlePermissionsForGroup(sqlConnection, contentId, groupId, "CONTENT_ITEM_ID", "ID asc", 1, 1, out totalRecords, articleId);
            return rows.FirstOrDefault();
        }


        public static void InsertChildContentPermissions(SqlConnection sqlConnection, IEnumerable<int> contentIDs, int? userId, int? groupId, int permissionLevel, bool propagateToItems, int currentUserId, bool hide)
        {
            if (contentIDs == null || !contentIDs.Any() || (!userId.HasValue && !groupId.HasValue))
                return;

            var query = @"INSERT INTO [CONTENT_ACCESS]
                                       ([CONTENT_ID]
                                       ,[USER_ID]
                                       ,[GROUP_ID]
                                       ,[PERMISSION_LEVEL_ID]
                                       ,[PROPAGATE_TO_ITEMS]
                                       ,[HIDE]
                                       ,[CREATED]
                                       ,[MODIFIED]
                                       ,[LAST_MODIFIED_BY])
                            select C.CONTENT_ID, @userId, @groupId, @permissionLevel, @propageteToItems, @hide, GETDATE(), GETDATE(), @modifiedUserId
                            from CONTENT C where C.CONTENT_ID in ({0})";
            query = string.Format(query, string.Join(",", contentIDs));
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@propageteToItems", propagateToItems);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);
                cmd.Parameters.AddWithValue("@hide", hide);


                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildContentPermissions(SqlConnection sqlConnection, int siteId, int? userId, int? groupId, int permissionLevel, bool propagateToItems, int currentUserId, bool hide)
        {
            var query = @"INSERT INTO [CONTENT_ACCESS]
                                       ([CONTENT_ID]
                                       ,[USER_ID]
                                       ,[GROUP_ID]
                                       ,[PERMISSION_LEVEL_ID]
                                       ,[PROPAGATE_TO_ITEMS]
                                       ,[HIDE]
                                       ,[CREATED]
                                       ,[MODIFIED]
                                       ,[LAST_MODIFIED_BY])
                            select C.CONTENT_ID, @userId, @groupId, @permissionLevel, @propageteToItems, @hide, GETDATE(), GETDATE(), @modifiedUserId
                            from CONTENT C where C.SITE_ID = @siteId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@siteId", siteId);
                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@propageteToItems", propagateToItems);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);
                cmd.Parameters.AddWithValue("@hide", hide);


                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildContentPermissions(SqlConnection sqlConnection, IEnumerable<int> contentIDs, int? userId, int? groupId)
        {
            if (contentIDs == null || !contentIDs.Any() || (!userId.HasValue && !groupId.HasValue))
                return;

            var query = @"delete from [CONTENT_ACCESS] where CONTENT_ID in ({0}) and (@userId IS NULL OR [USER_ID] = @userId) and (@groupId IS NULL OR GROUP_ID = @groupId)";
            query = string.Format(query, string.Join(",", contentIDs));
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildContentPermissions(SqlConnection sqlConnection, int siteId, int? userId, int? groupId)
        {
            var query = @"delete CONTENT_ACCESS from CONTENT_ACCESS A
                            JOIN CONTENT C ON A.CONTENT_ID = C.CONTENT_ID
                            where C.SITE_ID = @siteId
                            and (@userId IS NULL OR A.[USER_ID] = @userId) and (@groupId IS NULL OR A.GROUP_ID = @groupId)";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@siteId", siteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildArticlePermissions(SqlConnection sqlConnection, IEnumerable<int> articleIDs, int? userId, int? groupId, int permissionLevel, int currentUserId)
        {
            var query = @"INSERT INTO [CONTENT_ITEM_ACCESS]
                            ([CONTENT_ITEM_ID]
                            ,[USER_ID]
                            ,[GROUP_ID]
                            ,[PERMISSION_LEVEL_ID]
                            ,[CREATED]
                            ,[MODIFIED]
                            ,[LAST_MODIFIED_BY])
                            select CI.CONTENT_ITEM_ID, @userId, @groupId, @permissionLevel, GETDATE(), GETDATE(), @modifiedUserId
                            from CONTENT_ITEM CI where CI.CONTENT_ITEM_ID in ({0})";
            query = string.Format(query, string.Join(",", articleIDs));
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildArticlePermissions(SqlConnection sqlConnection, int contentId, int? userId, int? groupId, int permissionLevel, int currentUserId)
        {
            var query = @"INSERT INTO [CONTENT_ITEM_ACCESS]
                            ([CONTENT_ITEM_ID]
                            ,[USER_ID]
                            ,[GROUP_ID]
                            ,[PERMISSION_LEVEL_ID]
                            ,[CREATED]
                            ,[MODIFIED]
                            ,[LAST_MODIFIED_BY])
                            select CI.CONTENT_ITEM_ID, @userId, @groupId, @permissionLevel, GETDATE(), GETDATE(), @modifiedUserId
                            from CONTENT_ITEM CI where CI.CONTENT_ID = @contentId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildArticlePermissions(SqlConnection sqlConnection, IEnumerable<int> articleIDs, int? userId, int? groupId)
        {
            var query = @"delete from [CONTENT_ITEM_ACCESS] where CONTENT_ITEM_ID in ({0}) and (@userId IS NULL OR [USER_ID] = @userId) and (@groupId IS NULL OR GROUP_ID = @groupId)";
            query = string.Format(query, string.Join(",", articleIDs));
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildArticlePermissions(SqlConnection sqlConnection, int contentId, int? userId, int? groupId)
        {
            var query = @"delete [CONTENT_ITEM_ACCESS] from [CONTENT_ITEM_ACCESS] A
                            JOIN CONTENT_ITEM CI ON A.CONTENT_ITEM_ID = CI.CONTENT_ITEM_ID
                            where CI.CONTENT_ID = @contentId
                            and (@userId IS NULL OR A.[USER_ID] = @userId) and (@groupId IS NULL OR A.GROUP_ID = @groupId)";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@userId", userId);
                else
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);

                if (groupId.HasValue)
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                else
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);

                cmd.Parameters.AddWithValue("@contentId", contentId);

                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsSiteDotNeByObjectFormatId(SqlConnection sqlConnection, int objectFormatId)
        {
            var query = @"select s.script_language from OBJECT_FORMAT obf inner join [OBJECT] o on obf.[OBJECT_ID] = o.[OBJECT_ID]
                            inner join PAGE_TEMPLATE pt on o.PAGE_TEMPLATE_ID = pt.PAGE_TEMPLATE_ID inner join [SITE] s on pt.SITE_ID =  s.SITE_ID
                            where obf.OBJECT_FORMAT_ID = @in_object_format_id";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@in_object_format_id", objectFormatId);
                return string.Equals(Converter.ToString(cmd.ExecuteScalar()), ".net final");
            }
        }

        public static IEnumerable<DataRow> GetEntityTypePermissionsForGroup(SqlConnection sqlConnection, int groupId, int? entityId = null)
        {
            var fromBlock = @"(select T.ID, T.NAME, L.PERMISSION_LEVEL_NAME, CAST((case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit from
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.GROUP_ID = {0}
                                 RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.[ACTION_PERMISSION_ENABLE] = 1 {1}) AS TR";
            fromBlock = string.Format(fromBlock, groupId, entityId.HasValue ? string.Format("AND T.ID = {0}", entityId.Value) : "");

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "ENTITY_TYPE", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords,
                    useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetEntityTypePermissionsForUser(SqlConnection sqlConnection, int userId, int? entityId = null)
        {
            var fromBlock = @"(select T.ID, T.NAME, L.PERMISSION_LEVEL_NAME, CAST((case when P2.[USER_ID] IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit, L.PERMISSION_LEVEL
                                 FROM
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.[USER_ID] = {0}
                                 RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.[ACTION_PERMISSION_ENABLE] = 1 {1}) AS TR";
            fromBlock = string.Format(fromBlock, userId, entityId.HasValue ? string.Format("AND T.ID = {0}", entityId.Value) : "");

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "ENTITY_TYPE", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords,
                    useSecurity: true, userId: userId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetActionPermissionsForGroup(SqlConnection sqlConnection, int groupId, int entityTypeId, int? actionId)
        {
            var fromBlock = @"(select T.ID, T.NAME, COALESCE(L.PERMISSION_LEVEL_NAME, {2}) AS PERMISSION_LEVEL_NAME, CAST((case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit from
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN backend_action_access_PermLevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.GROUP_ID = {0}
                                 RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ENTITY_TYPE_ID = {1} {3} AND T.[TYPE_ID] != dbo.qp_action_type_id('refresh')) AS TR";

            var entityPermissionLevelName = "NULL";
            var entityPermission = GetEntityTypePermissionsForGroup(sqlConnection, groupId, entityTypeId).FirstOrDefault();
            if (entityPermission != null)
                entityPermissionLevelName = string.Format("N'{0}'", entityPermission.Field<string>("PERMISSION_LEVEL_NAME")) ?? "NULL";

            fromBlock = string.Format(fromBlock, groupId, entityTypeId, entityPermissionLevelName,
                actionId.HasValue ? string.Format("AND T.ID = {0}", actionId.Value) : "");

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "backend_action", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords,
                    useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetActionPermissionsForUser(SqlConnection sqlConnection, int userId, int entityTypeId, int? actionId)
        {
            var fromBlock = @"(select T.ID, T.NAME, COALESCE(L.PERMISSION_LEVEL_NAME, {2}) AS PERMISSION_LEVEL_NAME, CAST((case when P2.[USER_ID] IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS IsExplicit,  COALESCE(L.PERMISSION_LEVEL, {4}) AS PERMISSION_LEVEL
                                 FROM
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN backend_action_access_PermLevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.[USER_ID] = {0}
                                 RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ENTITY_TYPE_ID = {1} {3} AND T.[TYPE_ID] != dbo.qp_action_type_id('refresh')) AS TR";

            var entityPermissionLevelName = "NULL";
            var entityPermissionLevel = "NULL";
            var entityPermission = GetEntityTypePermissionsForUser(sqlConnection, userId, entityTypeId).FirstOrDefault();
            if (entityPermission != null)
            {
                entityPermissionLevelName = string.Format("N'{0}'", entityPermission.Field<string>("PERMISSION_LEVEL_NAME")) ?? "NULL";
                if (!entityPermission.IsNull("PERMISSION_LEVEL"))
                    entityPermissionLevel = entityPermission.Field<decimal>("PERMISSION_LEVEL").ToString();
            }

            fromBlock = string.Format(fromBlock, userId, entityTypeId, entityPermissionLevelName,
                actionId.HasValue ? string.Format("AND T.ID = {0}", actionId.Value) : "",
                entityPermissionLevel);

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "backend_action", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords,
                    useSecurity: true, userId: userId, startLevel: 0, endLevel: 100);
        }

        //without paging sample
        public static IEnumerable<DataRow> GetVisualEditorCommandsBySiteId(SqlConnection sqlConnection, int siteId)
        {
            var query = @"SELECT cmd.[ID], cmd.[NAME], cmd.[ALIAS], cmd.[ROW_ORDER], cmd.[TOOLBAR_IN_ROW_ORDER], cmd.[GROUP_IN_TOOLBAR_ORDER], cmd.[COMMAND_IN_GROUP_ORDER]," +
                " bnd.[ON], cmd.[PLUGIN_ID], cmd.[CREATED], cmd.[MODIFIED], cmd.[LAST_MODIFIED_BY] from [dbo].[VE_COMMAND_SITE_BIND] bnd INNER JOIN [dbo].[VE_COMMAND] cmd ON bnd.COMMAND_ID = cmd.ID" +
                " where bnd.SITE_ID = " + siteId + " ORDER BY cmd.[ID]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorCommandsByFieldId(SqlConnection sqlConnection, int fieldId)
        {
            var query = @"SELECT cmd.[ID], cmd.[NAME], cmd.[ALIAS], cmd.[ROW_ORDER], cmd.[TOOLBAR_IN_ROW_ORDER], cmd.[GROUP_IN_TOOLBAR_ORDER], cmd.[COMMAND_IN_GROUP_ORDER]," +
                " bnd.[ON], cmd.[PLUGIN_ID], cmd.[CREATED], cmd.[MODIFIED], cmd.[LAST_MODIFIED_BY] from [dbo].[VE_COMMAND_FIELD_BIND] bnd INNER JOIN [dbo].[VE_COMMAND] cmd ON bnd.COMMAND_ID = cmd.ID" +
                " where bnd.FIELD_ID = " + fieldId + " ORDER BY cmd.[ID]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        /// <summary>
        /// Вкл/Выкл команду в.редактора
        /// </summary>
        public static void UpdateOrInsertSiteVeCommandValue(SqlConnection sqlConnection, int siteId, int commandId, bool value)
        {
            var query = @"begin tran
                            if not exists (select * from VE_COMMAND_SITE_BIND where [COMMAND_ID] = @cId and [SITE_ID] = @sId)
                            begin
                                    INSERT INTO VE_COMMAND_SITE_BIND
                                    ([COMMAND_ID]
                                       ,[SITE_ID]
                                       ,[ON])
                                 VALUES
                                       (@cId
                                       ,@sId
                                       ,@val)
                            end
                            else
                            begin
                                    update VE_COMMAND_SITE_BIND set [ON] = @val where [COMMAND_ID] = @cId and [SITE_ID] = @sId
                            end
                            commit";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateOrInsertSiteVeStyleValue(SqlConnection sqlConnection, int siteId, int styleId, bool value)
        {
            var query = @"begin tran
                            if not exists (select * from VE_STYLE_SITE_BIND where [STYLE_ID] = @cId and [SITE_ID] = @sId)
                            begin
                                    INSERT INTO VE_STYLE_SITE_BIND
                                    ([STYLE_ID]
                                       ,[SITE_ID]
                                       ,[ON])
                                 VALUES
                                       (@cId
                                       ,@sId
                                       ,@val)
                            end
                            else
                            begin
                                    update VE_STYLE_SITE_BIND set [ON] = @val where [STYLE_ID] = @cId and [SITE_ID] = @sId
                            end
                            commit";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", styleId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveSiteVeCommand(SqlConnection sqlConnection, int siteId, int commandId)
        {
            using (var cmd = SqlCommandFactory.Create("DELETE FROM VE_COMMAND_SITE_BIND where [COMMAND_ID] = @cId and [SITE_ID] = @sId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveSiteVeStyle(SqlConnection sqlConnection, int siteId, int styleId)
        {
            using (var cmd = SqlCommandFactory.Create("DELETE FROM VE_STYLE_SITE_BIND where [STYLE_ID] = @cId and [SITE_ID] = @sId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", styleId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveFieldVeCommand(SqlConnection sqlConnection, int fieldId, int commandId)
        {
            using (var cmd = SqlCommandFactory.Create("DELETE FROM VE_COMMAND_FIELD_BIND where [COMMAND_ID] = @cId and [FIELD_ID] = @fId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveFieldVeStyle(SqlConnection sqlConnection, int fieldId, int styleId)
        {
            using (var cmd = SqlCommandFactory.Create("DELETE FROM VE_STYLE_FIELD_BIND where [STYLE_ID] = @sId and [FIELD_ID] = @fId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@sId", styleId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateOrInsertFieldVeCommandValue(SqlConnection sqlConnection, int fieldId, int commandId, bool value)
        {
            var query = @"begin tran
                            if not exists (select * from VE_COMMAND_FIELD_BIND where [COMMAND_ID] = @cId and [FIELD_ID] = @fId)
                            begin
                                    INSERT INTO VE_COMMAND_FIELD_BIND
                                    ([COMMAND_ID]
                                       ,[FIELD_ID]
                                       ,[ON])
                                 VALUES
                                       (@cId
                                       ,@fId
                                       ,@val)
                            end
                            else
                            begin
                                    update VE_COMMAND_FIELD_BIND set [ON] = @val where [COMMAND_ID] = @cId and [FIELD_ID] = @fId
                            end
                            commit";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsVeCommandNameFree(SqlConnection sqlConnection, string name, int pluginId)
        {
            var query = @"select COUNT(*) FROM VE_COMMAND WHERE [NAME] = @name AND [PLUGIN_ID] <> @pluginId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pluginId", pluginId);
                return (Converter.ToInt32(cmd.ExecuteScalar()) == 0);
            }
        }

        public static bool IsVeCommandAliasFree(SqlConnection sqlConnection, string alias, int pluginId)
        {
            var query = @"select COUNT(*) FROM VE_COMMAND WHERE [ALIAS] = @alias AND [PLUGIN_ID] <> @pluginId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@alias", alias);
                cmd.Parameters.AddWithValue("@pluginId", pluginId);
                return (Converter.ToInt32(cmd.ExecuteScalar()) == 0);
            }
        }

        public static void UnlockArticlesLockedByUser(SqlConnection sqlConnection, int userId, int[] IDs)
        {
            var query = @"update CONTENT_ITEM set locked_by = null, locked = null where locked_by = @user_id and content_item_id in ({0})";
            query = string.Format(query, string.Join(",", IDs));
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }
        public static void UnlockAllArticlesLockedByUser(SqlConnection sqlConnection, int userId)
        {
            var query = @"update CONTENT_ITEM set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UnlockAllSitesLockedByUser(SqlConnection sqlConnection, int userId)
        {
            var query = @"update [SITE] set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetVeCommandsMaxRowOrder(SqlConnection sqlConnection)
        {
            var query = @"select MAX([ROW_ORDER]) FROM VE_COMMAND";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : (int)maxOrder;
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesPage(SqlConnection sqlConnection, int contentId, string orderBy, out int totalRecords,
            int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.VisualEditorStyle,
                "s.[ID] as [Id], s.[NAME] as [Name], s.[DESCRIPTION] as [Description], s.[TAG] as [Tag], s.[ORDER] as [Order]," +
                " s.[IS_SYSTEM] as [IsSystem], s.[IS_FORMAT] as [IsFormat], s.[CREATED] as [Created], s.[MODIFIED] as [Modified], u.LOGIN as [LastModifiedByLogin]",
                "dbo.[VE_STYLE] s inner join users u on s.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Order]",
                "",
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesBySiteId(SqlConnection sqlConnection, int siteId)
        {
            var query = @"SELECT s.[ID], s.[NAME], s.[DESCRIPTION], s.[TAG], s.[ORDER], s.[OVERRIDES_TAG], s.[IS_FORMAT], s.[IS_SYSTEM]," +
                " bnd.[ON], s.[ATTRIBUTES], s.[STYLES], s.[CREATED], s.[MODIFIED], s.[LAST_MODIFIED_BY] from [dbo].[VE_STYLE_SITE_BIND] bnd INNER JOIN [dbo].[VE_STYLE] s ON bnd.STYLE_ID = s.ID" +
                " where bnd.SITE_ID = " + siteId + " ORDER BY [Order]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesByFieldId(SqlConnection sqlConnection, int fieldId)
        {
            var query = @"SELECT s.[ID], s.[NAME], s.[DESCRIPTION], s.[TAG], s.[ORDER], s.[OVERRIDES_TAG], s.[IS_FORMAT], s.[IS_SYSTEM]," +
                " bnd.[ON], s.[ATTRIBUTES], s.[STYLES], s.[CREATED], s.[MODIFIED], s.[LAST_MODIFIED_BY] from [dbo].[VE_STYLE_FIELD_BIND] bnd INNER JOIN [dbo].[VE_STYLE] s ON bnd.STYLE_ID = s.ID" +
                " where bnd.FIELD_ID = " + fieldId + " ORDER BY [Order]";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void UpdateOrInsertFieldVeStyleValue(SqlConnection sqlConnection, int fieldId, int styleId, bool value)
        {
            var query = @"begin tran
                            if not exists (select * from VE_STYLE_FIELD_BIND where [STYLE_ID] = @styleId and [FIELD_ID] = @fieldId)
                            begin
                                    INSERT INTO VE_STYLE_FIELD_BIND
                                    ([STYLE_ID]
                                       ,[FIELD_ID]
                                       ,[ON])
                                 VALUES
                                       (@styleId
                                       ,@fieldId
                                       ,@val)
                            end
                            else
                            begin
                                    update VE_STYLE_FIELD_BIND set [ON] = @val where [STYLE_ID] = @styleId and [FIELD_ID] = @fieldId
                            end
                            commit";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@styleId", styleId);
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetWorkflowsPage(SqlConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Workflow,
                "w.[WORKFLOW_ID] AS Id, w.[WORKFLOW_NAME] as [Name], w.[DESCRIPTION] as [Description], w.[MODIFIED] as [Modified], w.[CREATED] as [Created]," +
                "w.[LAST_MODIFIED_BY] as[LastModifiedBy], u.[LOGIN] as [LastModifiedByLogin]",
                "[WORKFLOW] w inner join users u on w.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name]",
                "w.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetStatusTypePage(SqlConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.StatusType,
                "s.[STATUS_TYPE_ID] AS Id, s.[STATUS_TYPE_NAME] as [Name], s.[DESCRIPTION] as [Description], s.[WEIGHT] as [Weight], s.[MODIFIED] as [Modified], s.[CREATED] as [Created]," +
                " s.[LAST_MODIFIED_BY] as[LastModifiedBy], u.[LOGIN] as [LastModifiedByLogin]",
                "[STATUS_TYPE] s inner join users u on s.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Weight]",
                "s.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetStatusTypeWeightsBySiteId(SqlConnection connection, int siteId, int exceptId)
        {
            var query = "select [WEIGHT] FROM [dbo].[STATUS_TYPE] where [SITE_ID] = @siteId and [STATUS_TYPE_ID] <> @id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@siteId", siteId);
                cmd.Parameters.AddWithValue("@id", exceptId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static long GetNumberOfArticlesUsingStatusByStatusId(SqlConnection connection, int id)
        {
            using (var cmd = SqlCommandFactory.Create("select COUNT(*) from [CONTENT_ITEM] where [STATUS_TYPE_ID] = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return (value == null) ? 0 : long.Parse(value.ToString());
            }
        }

        public static long GetNumberOfWorkflowsUsingStatusByStatusId(SqlConnection connection, int id)
        {
            using (var cmd = SqlCommandFactory.Create("select COUNT(*) from [workflow_rules] where [SUCCESSOR_STATUS_ID] = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return (value == null) ? 0 : long.Parse(value.ToString());
            }
        }

        public static void SetNullAssociatedNotificationsStatusTypesIds(SqlConnection connection, int id)
        {
            var query = string.Format("UPDATE [NOTIFICATIONS] SET [notify_on_status_type_id] = null, [for_status_changed] = 0, " +
                "[for_status_partially_changed] = 0 WHERE [notify_on_status_type_id] = {0}", id);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAssociatedContentItemsStatusHistoryRecords(SqlConnection connection, int id)
        {
            var query = string.Format("DELETE FROM [CONTENT_ITEM_STATUS_HISTORY] with (rowlock)  where [STATUS_TYPE_ID] = {0}", id);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAssociatedWaitingForApprovalRecords(SqlConnection connection, int id)
        {
            var query = string.Format("DELETE FROM [WAITING_FOR_APPROVAL] where [STATUS_TYPE_ID] = {0}", id);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static HashSet<int> GetTreeIdsToLoad(SqlConnection sqlConnection, string entityTypeCode, string selectedIds)
        {


            using (var cmd = SqlCommandFactory.Create(string.Format("select * from ENTITY_TYPE where CODE = '{0}'", entityTypeCode), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                if (dt.Rows.Count == 0)
                    return new HashSet<int>();

                var row = dt.Rows[0];
                var parentFieldName = row.Field<string>("RECURRING_ID_FIELD");

                if (string.IsNullOrEmpty(parentFieldName))
                    return new HashSet<int>();
                var idFieldName = row.Field<string>("ID_FIELD");
                var tableName = row.Field<string>("SOURCE");
                return GetTreeIdsToLoad(sqlConnection, tableName, parentFieldName, idFieldName, selectedIds);
            }
        }


        public static HashSet<int> GetTreeIdsToLoad(SqlConnection sqlConnection, string tableName, string parentFieldName, string idFieldName, string selectedIds)
        {
            var result = new HashSet<int>();
            if (!string.IsNullOrEmpty(selectedIds))
            {
                var sb = new StringBuilder();
                sb.AppendLine("WITH IDS (ID)");
                sb.AppendLine("AS");
                sb.AppendLine("(");
                sb.AppendFormatLine("select {0} from {1} where {2} in ({3}) and {0} is not null", parentFieldName, tableName, idFieldName, selectedIds);
                sb.AppendLine("UNION ALL");
                sb.AppendFormatLine("select {0} from {1} c", parentFieldName, tableName);
                sb.AppendFormatLine("inner join IDS on c.{0} = IDS.ID", idFieldName);
                sb.AppendFormatLine("where {0} is not null", parentFieldName);
                sb.AppendLine(")");
                sb.AppendLine("select distinct ID from IDS");
                var sql = sb.ToString();

                using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            result.Add((int)dr.GetDecimal(0));
                    }
                }
            }

            return result;
        }

        public static IEnumerable<DataRow> GetCommandBindingBySiteId(SqlConnection sqlConnection, int siteId)
        {
            var query = string.Format("SELECT [COMMAND_ID], [ON] FROM [VE_COMMAND_SITE_BIND] WHERE [SITE_ID] = {0}", siteId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetCommandBindingByFieldId(SqlConnection sqlConnection, int fieldId)
        {
            var query = string.Format("SELECT [COMMAND_ID], [ON] FROM [VE_COMMAND_FIELD_BIND] WHERE [FIELD_ID] = {0}", fieldId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetStyleBindingByFieldId(SqlConnection sqlConnection, int fieldId)
        {
            var query = string.Format("SELECT [STYLE_ID], [ON] FROM [VE_STYLE_FIELD_BIND] WHERE [FIELD_ID] = {0}", fieldId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetStyleBindingBySiteId(SqlConnection sqlConnection, int siteId)
        {
            var query = string.Format("SELECT [STYLE_ID], [ON] FROM [VE_STYLE_SITE_BIND] WHERE [SITE_ID] = {0}", siteId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        /// <summary>
        /// Возвращает id агрегированных статей для статьи контента
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static IEnumerable<decimal> GetAggregatedArticlesIDs(SqlConnection connection, int articleId, int[] classfierFields, int[] types)
        {
            var query = @"
            declare @attrIds table (attribute_id numeric primary key, content_id numeric, attribute_name nvarchar(255))
            declare @attribute_id numeric, @content_id numeric, @attribute_name nvarchar(255)

            insert into @attrIds(attribute_id, content_id, attribute_name)
            select attribute_id, content_id, attribute_name from content_attribute where classifier_attribute_id in (select id from @ids) and content_id in (select id from @cids)
            declare @sql nvarchar(max)
            set @sql = ''
            while exists(select * from @attrIds)
            begin
                select @attribute_id = attribute_id, @content_id = content_id, @attribute_name = attribute_name from @attrIds
                print @attribute_id
                if @sql <> ''
                    set @sql = @sql + ' union all '
                set @sql = @sql + 'select content_item_id from content_' + cast(@content_id as nvarchar(30)) + '_united where [' + @attribute_name + '] = @article_id'
                delete from @attrIds where attribute_id = @attribute_id
            end
            exec sp_executesql @sql, N'@article_id numeric', @article_id = @article_id";

            var result = new List<decimal>();
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@article_id", articleId);
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(classfierFields) });
                cmd.Parameters.Add(new SqlParameter("@cids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(types) });
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetDecimal(0));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает id статей-вариаций для статьи контента
        /// </summary>
        public static IEnumerable<decimal> GetVariationArticlesIDs(SqlConnection connection, int articleId, int contentId, string variationFieldName)
        {
            var query = string.Format(@"select c.CONTENT_ITEM_ID from content_{0}_united c with(nolock) where c.[{1}] = @article_id", contentId, variationFieldName);

            var result = new List<decimal>();
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@article_id", articleId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetDecimal(0));
                    }
                }
            }
            return result;
        }

        public static void RemoveUserQueryAttrs(SqlConnection connection, int contentId)
        {
            var query = "delete from USER_QUERY_ATTRS WHERE VIRTUAL_CONTENT_ID = @content_id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удалить записи в таблице union_contents для всех полей union-контента
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contentId"></param>
        public static void RemoveUnionAttrsByUnionContent(SqlConnection connection, int contentId)
        {
            var query = @"delete UNION_ATTRS from UNION_ATTRS UA
                            JOIN CONTENT_ATTRIBUTE CA ON UA.virtual_attr_id = CA.ATTRIBUTE_ID
                            WHERE CA.CONTENT_ID = @content_id";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveUnionAttrsByBaseFields(SqlConnection connection, IEnumerable<int> baseFieldIds)
        {
            if (baseFieldIds.Any())
            {
                var query = string.Format("delete from UNION_ATTRS where union_attr_id in ({0})", string.Join(",", baseFieldIds));
                using (var cmd = SqlCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static void BatchInsertUserQueryAttrs(SqlConnection connection, IEnumerable<UserQueryAttrsDAL> records)
        {
            var qb = new StringBuilder();
            foreach (var r in records)
            {
                qb.AppendFormat("INSERT INTO [user_query_attrs] ([virtual_content_id] ,[user_query_attr_id]) VALUES ({0},{1});", r.VirtualContentId, r.UserQueryAttrId);
            }
            var query = qb.ToString();
            if (!string.IsNullOrEmpty(query))
                using (var cmd = SqlCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
        }

        public static void BatchInsertUnionAttrs(SqlConnection connection, IEnumerable<UnionAttrDAL> records)
        {
            var qb = new StringBuilder();
            foreach (var r in records)
            {
                qb.AppendFormat("INSERT INTO [union_attrs] ([virtual_attr_id],[union_attr_id]) VALUES({0},{1});", r.VirtualFieldId, r.UnionFieldId);
            }

            var query = qb.ToString();
            if (!string.IsNullOrEmpty(query))
                using (var cmd = SqlCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
        }

        public static void SetWorkflowBindedContentId(SqlConnection connection, int workflowId, int contentId)
        {
            var query = string.Format("INSERT INTO [content_workflow_bind] ([CONTENT_ID],[WORKFLOW_ID], [is_async]) VALUES({0}, {1}, 1);", contentId, workflowId);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CleanWorkflowContentBindedIds(SqlConnection connection, int workflowId)
        {
            var query = string.Format("DELETE FROM [content_workflow_bind] where WORKFLOW_ID = {0};", workflowId);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetWorkflowContentBindedIds(SqlConnection connection, int workflowId)
        {
            var query = string.Format("SELECT CONTENT_ID FROM [content_workflow_bind] where WORKFLOW_ID = {0};", workflowId);

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int[] GetIdsToAutoArchive(SqlConnection connection, IEnumerable<int> IDs)
        {
            var result = new List<int>();
            var query = "select content_item_id FROM @ids i inner join content_item ci on i.id = ci.content_item_id inner join content c on c.content_id = ci.content_id where c.auto_archive = 1";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(IDs) });
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        result.Add((int)dr.GetDecimal(0));
                }
            }
            return result.ToArray();
        }

        public static void DeleteArticles(SqlConnection connection, IEnumerable<int> IDs, bool withAggregated)
        {
            var source = withAggregated ? "dbo.qp_aggregated_and_self(@ids)" : "@ids";
            if (IDs != null && IDs.Any())
            {
                var query = string.Format("DELETE FROM [content_item] where content_item_id in (select id from {0}) ", source);

                using (var cmd = SqlCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(IDs) });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<DataRow> GetAcceptableBaseFieldIdsForCloning(SqlConnection sqlConnection, string fieldName, string contentIds, int virtualContentId, bool forNew)
        {
            var sb = new StringBuilder();
            if (!forNew)
            {
                sb.Append("select ATTRIBUTE_ID as id, isnull(user_query_attr_id, 0) as sort_order from CONTENT_ATTRIBUTE ca ");
                sb.Append("left join user_query_attrs uqa on ca.attribute_id = uqa.user_query_attr_id and uqa.virtual_content_id = @virtualContentId ");
                sb.AppendFormat("where ATTRIBUTE_NAME = @attrName and CONTENT_ID in ({0}) order by sort_order desc ", contentIds);
            }
            else
            {
                sb.Append("select ATTRIBUTE_ID as id from CONTENT_ATTRIBUTE ca ");
                sb.AppendFormat("where ATTRIBUTE_NAME = @attrName and CONTENT_ID in ({0}) ", contentIds);
            }

            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.Parameters.AddWithValue("@attrName", fieldName);
                cmd.Parameters.AddWithValue("@virtualContentId", virtualContentId);
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static string GetPermittedItemsAsQuery(SqlConnection connection, int userId, int groupId, int startLevel, int endLevel, string entityName, string parentEntityName, int parentEntityId)
        {
            using (var cmd = SqlCommandFactory.Create("qp_GetPermittedItemsAsQuery", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@group_id", groupId);
                cmd.Parameters.AddWithValue("@start_level", startLevel);
                cmd.Parameters.AddWithValue("@end_level", endLevel);
                cmd.Parameters.AddWithValue("@entity_name", entityName);
                cmd.Parameters.AddWithValue("@parent_entity_name", parentEntityName);
                cmd.Parameters.AddWithValue("@parent_entity_id", parentEntityId);

                cmd.Parameters.Add(new SqlParameter("@SQLOut", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
                cmd.ExecuteNonQuery();
                return (string)cmd.Parameters["@SQLOut"].Value;
            }
        }

        public static bool TestM2MValue(SqlConnection sqlConnection, int linkId, int articleId, int testArticleId)
        {
            var result =
                GetLinkedArticles(sqlConnection, linkId, articleId, false)
                .Split(",".ToCharArray())
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => int.Parse(n))
                .ToDictionary(n => n)
            ;
            return result.ContainsKey(testArticleId);
        }

        public static bool TestM2OValue(SqlConnection sqlConnection, int contentId, string fieldName, int articleId, int testArticleId)
        {
            var result =
                GetRelatedArticles(sqlConnection, contentId, fieldName, articleId, false)
                .Split(",".ToCharArray())
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => int.Parse(n))
                .ToDictionary(n => n)
            ;
            return result.ContainsKey(testArticleId);
        }

        /// <summary>
        /// Обновляет значения поля StringEnum в существующих статьях, в соответствии со составом перечисления
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="p1"></param>
        /// <param name="enumValues"></param>
        /// <param name="p2"></param>
        public static void CorrectEnumInContentData(SqlConnection sqlConnection, int fieldId, IEnumerable<string> enumValues, string defValue)
        {
            if (enumValues.Any())
            {
                var query = string.Format(@"update content_data set DATA = @def_value, MODIFIED = GETDATE()
                    where ATTRIBUTE_ID = @field_id
                    and DATA not in ({0}) and DATA IS NOT NULL",
                    string.Join(",", enumValues.Select(v => string.Format("'{0}'", Cleaner.ToSafeSqlString(v))))
                );
                using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@field_id", fieldId);
                    cmd.Parameters.AddWithValue("@def_value", (object)defValue ?? DBNull.Value);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static IEnumerable<DataRow> GetPageTemplatesBySiteId(SqlConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.PageTemplate,
                "p.[PAGE_TEMPLATE_ID] AS Id, p.[is_system] AS [IsSystem], p.[LOCKED_BY] AS [LockedBy], p.[TEMPLATE_NAME] AS [Name], p.[TEMPLATE_FOLDER] AS [Folder], p.[DESCRIPTION] AS [Description], " +
                "p.[CREATED] AS [Created], p.[MODIFIED] AS [Modified], p.LAST_MODIFIED_BY AS [LastModifiedBy], u.LOGIN AS [LastModifiedByLogin], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName",

                "dbo.[PAGE_TEMPLATE] as p inner join users u on p.LAST_MODIFIED_BY = u.user_id left outer join users u2 on p.[LOCKED_BY] = u2.user_id",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] ASC",
                "p.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetPagesByTemplateId(SqlConnection sqlConnection, int templateId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Page,
                "p.[PAGE_ID] AS Id, p.[GENERATE_TRACE] AS [GenerateTrace], p.[LOCKED_BY] AS [LockedBy], p.[REASSEMBLE] AS [Reassemble], p.[PAGE_NAME] AS [Name], p.[DESCRIPTION] AS [Description], " +
                "p.[PAGE_FILENAME] as FileName, p.[page_folder] as Folder, p.[CREATED] AS [Created], p.[MODIFIED] AS [Modified], p.LAST_MODIFIED_BY AS [LastModifiedBy], u.LOGIN AS [LastModifiedByLogin], " +
                "p.[ASSEMBLED] as [Assembled], p.[LAST_ASSEMBLED_BY] as [LastAssembledBy], uu.[LOGIN] as [LastAssembledByLogin], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName, t.[TEMPLATE_NAME] as [TemplateName]",

                "dbo.[PAGE] as p inner join users u on p.LAST_MODIFIED_BY = u.user_id INNER JOIN page_template as t on p.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID] left outer join users uu on p.[LAST_ASSEMBLED_BY] " +
                " = uu.user_id left outer join users u2 on p.[ASSEMBLED] = u2.user_id",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] Asc",
                "p.[PAGE_TEMPLATE_ID] = " + templateId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetTemplateObjectsByTemplateId(SqlConnection sqlConnection, int templateId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.TemplateObject,
                "o.[parent_object_id] as parentId, o.[OBJECT_ID] as Id, o.[ICON] as Icon, o.[LOCKED_BY] AS [LockedBy], o.[OBJECT_NAME] as [Name], o.[DESCRIPTION] as [Description], o.[CREATED] as [Created], o.[MODIFIED] as [Modified]," +
                "u.[LOGIN] as [LastModifiedByLogin], t.[TYPE_NAME] as [TypeName], o.LAST_MODIFIED_BY AS [LastModifiedBy], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName, cast(case when oo.object_id is null then 0 else 1 end as bit) as Overriden ",
                "[TEMPLATE_OBJECT] o inner join USERS u on o.LAST_MODIFIED_BY = u.USER_ID inner join [OBJECT_TYPE] t on o.OBJECT_TYPE_ID = t.OBJECT_TYPE_ID left outer join users u2 on o.[LOCKED_BY] = u2.user_id " +
                "left join (select distinct parent_object_id as object_id from object) oo on o.OBJECT_ID = oo.object_id ",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] ASC",
                "o.[PAGE_TEMPLATE_ID] = " + templateId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetPageObjectsByPageId(SqlConnection sqlConnection, int pageId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.PageObject,
                "o.[parent_object_id] as parentId, o.[OBJECT_ID] as Id, o.[LOCKED_BY] AS [LockedBy], o.[ICON] as Icon, o.[OBJECT_NAME] as [Name], o.[DESCRIPTION] as [Description], o.[CREATED] as [Created], o.[MODIFIED] as [Modified]," +
                "u.[LOGIN] as [LastModifiedByLogin], t.[TYPE_NAME] as [TypeName], o.LAST_MODIFIED_BY AS [LastModifiedBy], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName, cast(0 as bit) as Overriden",
                "[PAGE_OBJECT] o inner join USERS u on o.LAST_MODIFIED_BY = u.USER_ID inner join [OBJECT_TYPE] t on o.OBJECT_TYPE_ID = t.OBJECT_TYPE_ID left outer join users u2 on o.[LOCKED_BY] = u2.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] ASC",
                "o.[PAGE_ID] = " + pageId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetObjectFormatsByObjectId(SqlConnection sqlConnection, int objectId, string orderBy, out int totalRecords, int startRow, int pageSize, bool pageOrTemplate)
        {
            return GetSimplePagedList(
                sqlConnection,
                pageOrTemplate ? EntityTypeCode.PageObjectFormat : EntityTypeCode.TemplateObjectFormat,
                "o.[OBJECT_FORMAT_ID] as Id, o.[LOCKED_BY] AS [LockedBy],  o.[FORMAT_NAME] as [Name], o.[DESCRIPTION] as [Description], o.[CREATED] as [Created], o.[MODIFIED] as [Modified]," +
                "u.[LOGIN] as [LastModifiedByLogin], o.LAST_MODIFIED_BY AS [LastModifiedBy], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName, t.[OBJECT_FORMAT_ID] as [FormatIdToCheck]",
                "[OBJECT_FORMAT] o inner join USERS u on o.LAST_MODIFIED_BY = u.USER_ID inner join [OBJECT] t on o.OBJECT_ID = t.OBJECT_ID left outer join users u2 on o.[LOCKED_BY] = u2.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] ASC",
                "o.[OBJECT_ID] = " + objectId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static string GetSiteScriptLanguageByPageId(SqlConnection sqlConnection, int pageId)
        {
            var sql = @"SELECT s.script_language from PAGE p inner join page_template t on p.PAGE_TEMPLATE_ID = t.PAGE_TEMPLATE_ID inner join [SITE] s on t.SITE_ID = s.SITE_ID
            where p.PAGE_ID = @pageId";
            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@pageId", pageId);
                var value = cmd.ExecuteScalar();
                return value.ToString();
            }
        }

        public static string GetSiteScriptLanguageByTemplateId(SqlConnection sqlConnection, int templateId)
        {
            var sql = @"SELECT s.script_language from page_template t inner join [SITE] s on t.SITE_ID = s.SITE_ID
            where t.PAGE_TEMPLATE_ID = @templateId";
            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@templateId", templateId);
                var value = cmd.ExecuteScalar();
                return value.ToString();
            }
        }

        public static IEnumerable<DataRow> GetPagesBySiteId(SqlConnection sqlConnection, int parentId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Page,
                "p.[PAGE_ID] AS Id, p.[GENERATE_TRACE] AS [GenerateTrace], p.[LOCKED_BY] AS [LockedBy], p.[REASSEMBLE] AS [Reassemble], p.[PAGE_NAME] AS [Name], p.[DESCRIPTION] AS [Description], " +
                "p.[PAGE_FILENAME] as FileName, p.[page_folder] as Folder, p.[CREATED] AS [Created], p.[MODIFIED] AS [Modified], p.LAST_MODIFIED_BY AS [LastModifiedBy], u.LOGIN AS [LastModifiedByLogin], " +
                "p.[ASSEMBLED] as [Assembled], p.[LAST_ASSEMBLED_BY] as [LastAssembledBy], uu.[LOGIN] as [LastAssembledByLogin], u2.FIRST_NAME + ' ' + u2.LAST_NAME as LockedByFullName, t.TEMPLATE_NAME as TemplateName",

                "dbo.[PAGE] as p inner join users as u on p.LAST_MODIFIED_BY = u.user_id left outer join users uu on p.[LAST_ASSEMBLED_BY] = uu.user_id left outer join users u2 on p.[ASSEMBLED] = u2.user_id" +
                " left outer join page_template as t on p.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID]",

                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Name] ASC",
                "t.[SITE_ID] = " + parentId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static void CopyContentCustomActions(SqlConnection sqlConnection, int sourceId, int destinationId)
        {
            var query = @"INSERT INTO [dbo].[ACTION_CONTENT_BIND] (CUSTOM_ACTION_ID, CONTENT_ID)
                                    SELECT CUSTOM_ACTION_ID, {0} FROM  [dbo].[ACTION_CONTENT_BIND] where CONTENT_ID = {1}";

            query = string.Format(query, destinationId, sourceId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void AddHistoryStatus(SqlConnection sqlConnection, int contentItemId, int systemStatusTypeId, int userId, string description)
        {
            var query = @"insert into content_item_status_history(CONTENT_ITEM_ID, SYSTEM_STATUS_TYPE_ID, USER_ID, DESCRIPTION) values ({0}, {1}, {2}, N'{3}')";

            query = string.Format(query, contentItemId, systemStatusTypeId, userId, description);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static List<DataRow> GetStatusHistoryItem(SqlConnection sqlConnection, int artcileId)
        {
            var query = @"select TOP 1 h.STATUS_HISTORY_ID as Id
                                            ,h.STATUS_HISTORY_DATE as ActionDate
                                            ,ISNULL(h1.DESCRIPTION, h.DESCRIPTION) as Comment
                                            ,t.STATUS_TYPE_NAME as StatusTypeName
                                            ,u.LOGIN as ActionMadeBy
                                            ,s.NAME as SystemStatusTypeName
                                    from CONTENT_ITEM_STATUS_HISTORY as h with (nolock)
                                    LEFT JOIN STATUS_TYPE as t with (nolock) on t.STATUS_TYPE_ID = h.STATUS_TYPE_ID
                                    LEFT JOIN USERS as u with (nolock) on u.USER_ID = h.USER_ID
                                    LEFT JOIN (SELECT DESCRIPTION, SYSTEM_STATUS_TYPE_ID, STATUS_HISTORY_ID FROM CONTENT_ITEM_STATUS_HISTORY with (nolock) where SYSTEM_STATUS_TYPE_ID BETWEEN 9 AND 14 AND content_item_id={0}) as h1 ON h1.STATUS_HISTORY_ID = (h.STATUS_HISTORY_ID + 1)
                                    LEFT JOIN SYSTEM_STATUS_TYPE as s with (nolock) on s.ID = h1.SYSTEM_STATUS_TYPE_ID
                                    where h.CONTENT_ITEM_ID = {0} AND h.SYSTEM_STATUS_TYPE_ID IS NULL
                                    order by ActionDate desc";
            query = string.Format(query, artcileId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static List<DataRow> GetAllHistoryStatusesForArticle(SqlConnection sqlConnection, int articleId, string orderBy, int startRow, int pageSize, out int totalRecords)
        {
            return GetSimplePagedList(
             sqlConnection,
             EntityTypeCode.Article,
             @"h.STATUS_HISTORY_ID as ID
                                            ,h.STATUS_HISTORY_DATE as ActionDate
                                            ,ISNULL(h1.DESCRIPTION, h.DESCRIPTION) as Comment
                                            ,t.STATUS_TYPE_NAME as StatusTypeName
                                            ,u.LOGIN as ActionMadeBy
                                            ,s.NAME as SystemStatusTypeName",

             string.Format(@"[dbo].[CONTENT_ITEM_STATUS_HISTORY] as h with (nolock)
                                    LEFT JOIN [dbo].[STATUS_TYPE] as t with (nolock) on t.STATUS_TYPE_ID = h.STATUS_TYPE_ID
                                    LEFT JOIN [dbo].[USERS] as u with (nolock) on u.USER_ID = h.USER_ID
                                    LEFT JOIN (SELECT DESCRIPTION, SYSTEM_STATUS_TYPE_ID, STATUS_HISTORY_ID FROM CONTENT_ITEM_STATUS_HISTORY with (nolock) where SYSTEM_STATUS_TYPE_ID BETWEEN 9 AND 14 AND content_item_id={0}) as h1 ON h1.STATUS_HISTORY_ID = (h.STATUS_HISTORY_ID + 1)
                                    LEFT JOIN [dbo].[SYSTEM_STATUS_TYPE] as s with (nolock) on s.ID = h1.SYSTEM_STATUS_TYPE_ID", articleId),

             !string.IsNullOrEmpty(orderBy) ? orderBy : "ActionDate DESC",
             string.Format("h.CONTENT_ITEM_ID = {0}", articleId),
             startRow,
             pageSize,
             out totalRecords
         ).ToList();
        }

        public static List<DataRow> GetArticlesForExport(SqlConnection sqlConnection, int contentId, string exstensions, string columns, string filter, int startRow, int pageSize, string orderBy, out int totalRecords)
        {
            return GetSimplePagedList(
                 sqlConnection,
                 EntityTypeCode.Article,
                 string.Format("base.[content_item_id] {0}, base.created, base.modified", columns),
                 string.Format("[dbo].[content_{0}] base {1}", contentId, exstensions),
                 string.IsNullOrEmpty(orderBy) ? "base.CONTENT_ITEM_ID DESC" : orderBy,
                 filter,
                 startRow,
                 pageSize,
                 out totalRecords
                ).ToList();
        }

        /// <summary>
        /// Находит все контенты-расширения, задействованные в статьях заданного контента
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="contentId">Идентификатор контента</param>
        /// <returns>идентификаторы контентов</returns>
        public static int[] GetReferencedAggregatedContentIds(SqlConnection sqlConnection, int contentId, int[] articleIds)
        {
            var query = @"
                DECLARE @query NVARCHAR(1000)
                SET @query = ''

                SELECT
                    @query = @query + ATTRIBUTE_NAME + ','
                FROM
                    CONTENT_ATTRIBUTE f
                    JOIN CONTENT c ON c.CONTENT_ID = f.CONTENT_ID
                WHERE
                    c.CONTENT_ID = @contentId AND
                    IS_CLASSIFIER = 1

                IF NOT @query = ''
                BEGIN
                    SET @query =
                    'SELECT DISTINCT ' + SUBSTRING(@query, 1, LEN(@query) - 1) +
                    ' FROM CONTENT_' + LTRIM(STR(@contentId))  +
                    ' WHERE ARCHIVE = 0 AND ' + SUBSTRING(@query, 1, LEN(@query) - 1) + ' IS NOT NULL'
                    IF EXISTS (SELECT NULL FROM @articleIds)
                        SET @query = @query + ' AND CONTENT_ITEM_ID IN (SELECT Id FROM @articleIds)'
                    EXEC sp_executesql @query, N'@articleIds Ids READONLY', @articleIds
                END";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.Add(new SqlParameter("@articleIds", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(articleIds) });

                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<int>();

                    while (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            result.Add((int)reader.GetDecimal(i));
                        }
                    }

                    return result.ToArray();
                }
            }
        }

        public static int[] GetReferencedAggregatedContentIds(SqlConnection sqlConnection, int contentId)
        {
            var query = @"
                SELECT
                    af.CONTENT_ID
                FROM
                    CONTENT_ATTRIBUTE cf
                    JOIN CONTENT_ATTRIBUTE af ON cf.ATTRIBUTE_ID = af.RELATED_ATTRIBUTE_ID
                WHERE
                    cf.CONTENT_ID = @contentId AND
                    af.AGGREGATED = 1";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);

                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<int>();

                    while (reader.Read())
                    {
                        result.Add((int)reader.GetDecimal(0));
                    }

                    return result.ToArray();
                }
            }
        }

        public static Dictionary<int, string> GetAggregatedFieldNames(SqlConnection sqlConnection, int[] contentIds)
        {
            using (var cmd = SqlCommandFactory.Create("SELECT CONTENT_ID, ATTRIBUTE_NAME FROM CONTENT_ATTRIBUTE JOIN @ids ON CONTENT_ID = ID WHERE AGGREGATED = 1", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(contentIds) });

                using (var reader = cmd.ExecuteReader())
                {
                    var result = new Dictionary<int, string>();

                    while (reader.Read())
                    {
                        var id = (int)(decimal)reader["CONTENT_ID"];
                        var name = (string)reader["ATTRIBUTE_NAME"];
                        result[id] = name;
                    }

                    return result;
                }
            }
        }

        public static Dictionary<int, Dictionary<int, int>> GetAggregatedArticleIdsMap(SqlConnection sqlConnection, int contentId, int[] articleIds)
        {
            var query = @"
                DECLARE @query NVARCHAR(MAX)
                SET @query = ''

                SELECT
                    @query = @query + '
                    SELECT
                        ids.Id [Id],' +
                        CONVERT(NVARCHAR(10), f.ATTRIBUTE_ID) +' [FieldId],
                        a.CONTENT_ITEM_ID [ExstensionId]
                    FROM
                        @ids ids
                        JOIN CONTENT_' + CONVERT(NVARCHAR(10), ef.CONTENT_ID) + ' a ON a.' + ef.ATTRIBUTE_NAME +' = ids.Id
                    UNION'
                FROM
                    [CONTENT_ATTRIBUTE] f
                    JOIN [CONTENT_ATTRIBUTE] ef ON ef.CLASSIFIER_ATTRIBUTE_ID = f.ATTRIBUTE_ID
                WHERE
                    f.CONTENT_ID = @contentId

                IF @query <> ''
                BEGIN
                    SET @query = LEFT(@query, LEN(@query) - LEN('UNION'))
                    EXEC sp_executesql @query, N'@ids Ids READONLY', @ids
                END";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(articleIds) });

                using (var reader = cmd.ExecuteReader())
                {
                    var result = new Dictionary<int, Dictionary<int, int>>();
                    Dictionary<int, int> articleMap;

                    while (reader.Read())
                    {
                        var id = (int)(decimal)reader["Id"];
                        var fieldId = (int)reader["FieldId"];
                        var exstensionId = (int)(decimal)reader["ExstensionId"];

                        if (result.ContainsKey(id))
                        {
                            articleMap = result[id];
                        }
                        else
                        {
                            articleMap = new Dictionary<int, int>();
                            result[id] = articleMap;
                        }

                        articleMap[fieldId] = exstensionId;
                    }

                    return result;
                }
            }
        }

        public static Dictionary<int, string> GetFieldNames(SqlConnection sqlConnection, int[] referencefieldIds)
        {
            using (var cmd = SqlCommandFactory.Create("SELECT f.ATTRIBUTE_ID, f.ATTRIBUTE_NAME FROM CONTENT_ATTRIBUTE f JOIN @ids ids ON f.ATTRIBUTE_ID = ids.ID", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(referencefieldIds) });

                using (var reader = cmd.ExecuteReader())
                {
                    var result = new Dictionary<int, string>();

                    while (reader.Read())
                    {
                        var id = (int)(decimal)reader["ATTRIBUTE_ID"];
                        var name = (string)reader["ATTRIBUTE_NAME"];
                        result[id] = name;
                    }

                    return result;
                }
            }
        }

        public static void SetPagesAndObjectsEnableViewState(SqlConnection sqlConnection, int pageTemplateId, bool enableViewState)
        {
            var objectQuery = string.Format("UPDATE [object] SET ENABLE_VIEWSTATE = {0} WHERE page_template_id = {1}", enableViewState ? 1 : 0, pageTemplateId);
            var pageQuery = string.Format("UPDATE [page] SET ENABLE_VIEWSTATE = {0} WHERE page_template_id = {1}", enableViewState ? 1 : 0, pageTemplateId);

            using (var cmd = SqlCommandFactory.Create(objectQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = SqlCommandFactory.Create(pageQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetObjectsDisableDataBinding(SqlConnection sqlConnection, int pageTemplateId, bool disableDataBinding)
        {
            var query = string.Format("UPDATE [object] SET DISABLE_DATABIND = {0} WHERE page_template_id = {1}", disableDataBinding ? 1 : 0, pageTemplateId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetCustomClassForPages(SqlConnection sqlConnection, int pageTemplateId, string customClass)
        {
            var query = string.Format("UPDATE [page] SET [page_custom_class] = '{0}' WHERE page_template_id = {1}", customClass, pageTemplateId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetObjectTypeId(SqlConnection sqlConnection, string typeName)
        {
            var query = string.Format("SELECT [OBJECT_TYPE_ID] FROM [OBJECT_TYPE] WHERE [TYPE_NAME] = '{0}'", typeName);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (int)(decimal)cmd.ExecuteScalar();
            }
        }

        public static void SetCustomClassForObjects(SqlConnection sqlConnection, int pageTemplateId, string customClassForGenerics, string customClassForContainers, string customClassForForms)
        {
            const string generic = "Generic";
            const string container = "Publishing Container";
            const string form = "Publishing Form";

            var genericTypeId = GetObjectTypeId(sqlConnection, generic);
            var containerTypeId = GetObjectTypeId(sqlConnection, container);
            var formTypeId = GetObjectTypeId(sqlConnection, form);

            var genericQuery = string.Format("UPDATE [OBJECT] set control_custom_class = '{0}' WHERE page_template_id = {1} AND OBJECT_TYPE_ID = {2}", customClassForGenerics, pageTemplateId, genericTypeId);
            var containerQuery = string.Format("UPDATE [OBJECT] set control_custom_class = '{0}' WHERE page_template_id = {1} AND OBJECT_TYPE_ID = {2}", customClassForContainers, pageTemplateId, containerTypeId);
            var formQuery = string.Format("UPDATE [OBJECT] set control_custom_class = '{0}' WHERE page_template_id = {1} AND OBJECT_TYPE_ID = {2}", customClassForForms, pageTemplateId, formTypeId);

            using (var cmd = SqlCommandFactory.Create(genericQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = SqlCommandFactory.Create(containerQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = SqlCommandFactory.Create(formQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

        }

        public static void DeleteObjectContainer(SqlConnection sqlConnection, int objectId)
        {
            var query = "Delete FROM CONTAINER WHERE [object_id] = " + objectId;

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteObjectForm(SqlConnection sqlConnection, int objectId)
        {
            var query = "Delete FROM CONTENT_FORM WHERE [object_id] = " + objectId;

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static List<DataRow> GetActiveStatusesByObjectId(SqlConnection sqlConnection, int objectId)
        {
            var query = @"SELECT s.STATUS_TYPE_ID, s.WEIGHT, s.SITE_ID, s.STATUS_TYPE_NAME, s.DESCRIPTION, s.Created, s.Modified, s.LAST_MODIFIED_BY, s.BUILT_IN  FROM CONTAINER_STATUSES AS cs INNER JOIN STATUS_TYPE AS s ON cs.[STATUS_TYPE_ID] = s.[STATUS_TYPE_ID] INNER JOIN users as u on u.USER_ID = s.LAST_MODIFIED_BY WHERE cs.[OBJECT_ID] = @objectId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@objectId", objectId);
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static IEnumerable<DataRow> GetStatusPageForWorkflow(SqlConnection sqlConnection, int workflowId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Notification,
                @" s.STATUS_TYPE_ID as Id, s.STATUS_TYPE_NAME as Name, s.DESCRIPTION as Description, s.[WEIGHT] as Weight,
                s.MODIFIED as Modified, s.[CREATED] as Created, s.LAST_MODIFIED_BY as LastModifiedBy, s.BUILT_IN, u.[LOGIN] as LastModifiedByLogin ",
                @" [workflow_rules] as wr inner join status_type as s on s.[STATUS_TYPE_ID] = wr.[SUCCESSOR_STATUS_ID] inner join USERS as u on u.[USER_ID] = s.LAST_MODIFIED_BY ",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "[Id]",
                "WORKFLOW_ID = " + workflowId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetAllStatusesForWorkflow(SqlConnection sqlConnection, int workflowId)
        {
            var query = @"SELECT s.STATUS_TYPE_ID as Id, s.STATUS_TYPE_NAME as Name, s.DESCRIPTION as Description, s.[WEIGHT] as Weight,
                            s.MODIFIED as Modified, s.[CREATED] as Created, s.LAST_MODIFIED_BY as LastModifiedBy, s.BUILT_IN, u.[LOGIN] as LastModifiedByLogin
                            FROM [workflow_rules] as wr
                            inner join status_type as s on s.[STATUS_TYPE_ID] = wr.[SUCCESSOR_STATUS_ID] inner join USERS as u on u.[USER_ID] = s.LAST_MODIFIED_BY
                            where WORKFLOW_ID = " + workflowId;
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void SetObjectActiveStatuses(SqlConnection sqlConnection, int objectId, IEnumerable<int> activeStatusIds)
        {
            foreach (var statusId in activeStatusIds)
            {
                var query = string.Format(@"INSERT INTO CONTAINER_STATUSES([OBJECT_ID], [STATUS_TYPE_ID]) values ({0}, {1})", objectId, statusId);
                using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<int> GetObjectActiveStatusesIds(SqlConnection sqlConnection, int objectId)
        {
            var result = new List<int>();
            var query = "select [STATUS_TYPE_ID] FROM [CONTAINER_STATUSES] WHERE [OBJECT_ID] = @objectId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@objectId", objectId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        public static void CleanObjectActiveStatuses(SqlConnection sqlConnection, int objectId)
        {
            var query = string.Format(@"DELETE FROM CONTAINER_STATUSES where [OBJECT_ID] = {0}", objectId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UnlockAllTemplatesLockedByUser(SqlConnection sqlConnection, int userId)
        {
            var templateQuery = @"update [PAGE_TEMPLATE] set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(templateQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var pageQuery = @"update [PAGE] set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(pageQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var objectQuery = @"update [OBJECT] set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(objectQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var formatQuery = @"update [OBJECT_FORMAT] set locked_by = null, locked = null where permanent_lock = 0 and locked_by = @user_id";
            using (var cmd = SqlCommandFactory.Create(formatQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetFormatIdsByTemplateId(SqlConnection sqlConnection, int TemplateId)
        {
            var query = @"SELECT f.[OBJECT_FORMAT_ID] FROM [OBJECT_FORMAT] AS f " +
            "INNER JOIN [OBJECT] AS o ON f.[OBJECT_ID] = o.[OBJECT_ID] WHERE o.PAGE_TEMPLATE_ID = @templateId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@templateId", TemplateId);
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }


        public static IEnumerable<DataRow> AssembleAction_TemplatePages(int templateId, SqlConnection sqlConnection)
        {
            var result = new List<int>();
            var query = "select P.PAGE_ID Id, T.TEMPLATE_NAME Template, p.PAGE_NAME Name from PAGE P JOIN PAGE_TEMPLATE T ON P.PAGE_TEMPLATE_ID = T.PAGE_TEMPLATE_ID where T.PAGE_TEMPLATE_ID = @template_id order by P.PAGE_ID";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@template_id", templateId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static IEnumerable<int> AssembleAction_GetTemplateFormatIds(int templateId, SqlConnection sqlConnection)
        {
            var result = new List<int>();
            var query = @"SELECT f.[OBJECT_FORMAT_ID] FROM [OBJECT_FORMAT] AS f " +
            "INNER JOIN [OBJECT] AS o ON f.[OBJECT_ID] = o.[OBJECT_ID] INNER JOIN [PAGE_TEMPLATE] AS t on o.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID] " +
            "INNER JOIN [NOTIFICATIONS] AS n ON n.[FORMAT_ID] = f.[OBJECT_FORMAT_ID] WHERE t.PAGE_TEMPLATE_ID = @template_id";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@template_id", templateId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        public static void IterateRows(SqlConnection connection, string sqlString, Action<IEnumerable<object>> rowIterator)
        {
            using (var cmd = SqlCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        rowIterator(values);
                    }
                }
            }
        }

        public static void UpdatePageAndObjectDateModifiedByObjectId(int objectId, int pageId, SqlConnection sqlConnection)
        {
            var query = string.Format("declare @date datetime set @date = getdate() Update page set MODIFIED = @date where [PAGE_ID] = {0} Update OBJECT set MODIFIED = @date where [OBJECT_ID] = {1}",
                pageId, objectId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateObjectDateModified(int objectId, SqlConnection sqlConnection)
        {
            var query = string.Format("Update OBJECT set MODIFIED = getdate() where [OBJECT_ID] = {0}", objectId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static DataRow GetDBSettingsRow(SqlConnection connection)
        {
            using (var cmd = SqlCommandFactory.Create("select * from db", connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return (0 == dt.Rows.Count) ? null : dt.Rows[0];
            }
        }

        public static IEnumerable<DataRow> GetSearchFormatPage(SqlConnection sqlConnection, string sortExpression, int siteId, int? templateId, int? pageId, string filter, out int totalRecords, int start, int pageSize)
        {
            var filters = new List<string>();
            filters.Add(string.Format("s.SITE_ID = '{0}'", Cleaner.ToSafeSqlString(siteId.ToString())));
            if (templateId.HasValue)
                filters.Add(string.Format("t.[PAGE_TEMPLATE_ID] = '{0}'", Cleaner.ToSafeSqlString(templateId.ToString())));
            if (pageId.HasValue)
                filters.Add(string.Format("p.[PAGE_ID] = '{0}'", Cleaner.ToSafeSqlString(pageId.ToString())));
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filters.Add(string.Format("(f.[FORMAT_BODY] like '%{0}%' OR f.[CODE_BEHIND] like '%{0}%')", Cleaner.ToSafeSqlString(filter)));
            }
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.TemplateObjectFormat,
                " f.[OBJECT_FORMAT_ID] as Id, f.[OBJECT_ID] as ParentId, f.FORMAT_NAME as Name, f.DESCRIPTION as Description, f.CREATED as Created, f.MODIFIED as Modified, t.TEMPLATE_NAME as TemplateName, p.PAGE_NAME as PageName, o.[OBJECT_NAME] as ParentName, U.[LOGIN] as LastModifiedByLogin ",
                " [OBJECT_FORMAT] AS f INNER JOIN [OBJECT] as o on o.[OBJECT_ID] = f.[OBJECT_ID] INNER JOIN [PAGE_TEMPLATE] as t on o.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID] " +
                " INNER JOIN users as u on u.USER_ID = f.LAST_MODIFIED_BY INNER JOIN [site] as s on t.SITE_ID = s.SITE_ID LEFT OUTER JOIN [Page] as p on o.[PAGE_ID] = p.[PAGE_ID] ",
                !string.IsNullOrEmpty(sortExpression) ? sortExpression : "Id",
                string.Join(" AND ", filters),
                start,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetSearchTemplatePage(SqlConnection sqlConnection, string sortExpression, int siteId, string filter, out int totalRecords, int start, int pageSize)
        {
            var filters = new List<string>();
            filters.Add(string.Format("s.SITE_ID = '{0}'", Cleaner.ToSafeSqlString(siteId.ToString())));
            if (!string.IsNullOrWhiteSpace(filter))
                filters.Add(string.Format("(t.[TEMPLATE_BODY] like '%{0}%' OR t.[CODE_BEHIND] like '%{0}%')", Cleaner.ToSafeSqlString(filter)));
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.PageTemplate,
                " t.SITE_ID as ParentId, s.SITE_NAME as ParentName, t.[PAGE_TEMPLATE_ID] as Id, t.[TEMPLATE_NAME] as Name, t.DESCRIPTION as Description, t.CREATED as Created, t.MODIFIED as Modified, U.[LOGIN] as LastModifiedByLogin ",
                " [PAGE_TEMPLATE] AS t  INNER JOIN users as u on u.USER_ID = t.LAST_MODIFIED_BY INNER JOIN [site] as s on t.SITE_ID = s.SITE_ID",
                !string.IsNullOrEmpty(sortExpression) ? sortExpression : "Id",
                string.Join(" AND ", filters),
                start,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetSearchObjectPage(SqlConnection sqlConnection, string sortExpression, int siteId, int? templateId, int? pageId, string filter, out int totalRecords, int start, int pageSize)
        {
            var primaryFilters = new List<string>();
            primaryFilters.Add(string.Format("s.[SITE_ID] = '{0}'", Cleaner.ToSafeSqlString(siteId.ToString())));
            if (templateId.HasValue)
                primaryFilters.Add(string.Format("o.[PAGE_TEMPLATE_ID] = '{0}'", Cleaner.ToSafeSqlString(templateId.ToString())));
            if (pageId.HasValue)
                primaryFilters.Add(string.Format("o.[PAGE_ID] = '{0}'", Cleaner.ToSafeSqlString(pageId.ToString())));
            filter = Cleaner.ToSafeSqlString(filter);
            var secondaryFilters = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                secondaryFilters.Add("[counter] >0");
                secondaryFilters.Add(string.Format("cont.[FILTER_VALUE] like '%{0}%'", filter));
                secondaryFilters.Add(string.Format("cont.[ORDER_DYNAMIC] like '%{0}%'", filter));
                secondaryFilters.Add(string.Format("cont.[SELECT_START] = '{0}'", filter));
                secondaryFilters.Add(string.Format("cont.[SELECT_TOTAL] = '{0}'", filter));
            }
            var query = string.Format(
                string.Concat(
                new List<string>
                {
                " with query_CTE AS (SELECT TOP 10000 ROW_NUMBER() OVER ",
                " (ORDER BY o.[OBJECT_ID] asc) as rowNum, o.[OBJECT_ID] as Id, o.[OBJECT_NAME] as Name, o.DESCRIPTION as Description, o.CREATED as Created, ",
                " o.MODIFIED as Modified,  u.[LOGIN] as LastModifiedByLogin, o.[OBJECT_NAME] as ObjectName, t.[TEMPLATE_NAME] as TemplateName, ",
                " p.[PAGE_NAME] as PageName, s.SITE_ID, t.PAGE_TEMPLATE_ID as PageTemplateId, o.PAGE_ID as PageId, cont.FILTER_VALUE, ",
                " cont.DYNAMIC_CONTENT_VARIABLE, cont.SELECT_START, cont.SELECT_TOTAL, matches.[counter] ",
                " FROM [OBJECT] AS o INNER JOIN [USERS] as u ON u.[USER_ID] = o.LAST_MODIFIED_BY ",
                " INNER JOIN [PAGE_TEMPLATE] as t ON o.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID] INNER JOIN [site] as s ON t.SITE_ID = s.SITE_ID ",
                " LEFT OUTER JOIN (  SELECT ov.[OBJECT_ID], COUNT(ov.[OBJECT_ID]) AS [counter] FROM [OBJECT_VALUES] ov WHERE VARIABLE_NAME like ",
                " '%{0}%' OR VARIABLE_VALUE like '%{0}%'GROUP BY ov.[OBJECT_ID]) as matches on matches.OBJECT_ID = o.[OBJECT_ID] ",
                " LEFT JOIN [CONTAINER] as cont ",
                " ON cont.[OBJECT_ID] = o.[OBJECT_ID] LEFT JOIN [PAGE] AS p ON o.[PAGE_ID] = p.[PAGE_ID] where",
                " {1} {4} {5}) ",

                " select rowNum, ROWS_COUNT = (select count(0) from query_CTE), Id, Name,[Description], Created, Modified, LastModifiedByLogin, ObjectName, TemplateName, PageName, PageId, PageTemplateId, ",
                " [counter] as OvMatchCount from query_CTE ",
                " where   rowNum between {2} and {3}"})
                , filter, string.Join(" AND ", primaryFilters), start, start + pageSize - 1,
                secondaryFilters.Count > 0 ? "and (" + string.Join(" OR ", secondaryFilters) + ")" : string.Empty,
                string.IsNullOrWhiteSpace(sortExpression) ? string.Empty : "ORDER BY " + sortExpression
            );
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                totalRecords = (dt.Rows.Count != 0) ? (int)dt.Rows[0][QP8Entities.COUNT_COLUMN] : 0;
                return dt.AsEnumerable().ToArray();
            }
        }
        public static List<int> InsertArticleIds(SqlConnection sqlConnection, string query)
        {
            var result = new List<int>();
            var insertInto = string.Format(@"
                                DECLARE @NewArticles TABLE (
                                    [ID]         INT
                                )

                                INSERT into [dbo].[CONTENT_ITEM]
                                      ([VISIBLE]
                                      ,[STATUS_TYPE_ID]
                                      ,[CONTENT_ID]
                                      ,[LAST_MODIFIED_BY])
                                    OUTPUT inserted.[content_item_id] INTO @NewArticles
                                    {0}
                                SELECT ID FROM @NewArticles
                                    ", query);

            using (var cmd = SqlCommandFactory.Create(insertInto, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = 0;
                        if (int.TryParse(reader.GetValue(0).ToString(), out id))
                        {
                            result.Add(id);
                        }
                    }
                }
                return result;
            }
        }

        public static void InsertArticleValues(SqlConnection sqlConnection, string xmlParameter)
        {
            using (var cmd = SqlCommandFactory.Create("qp_insertArticleValues", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var prm = cmd.Parameters.AddWithValue("@xmlParameter", xmlParameter);
                var resultDt = new DataTable();

                new SqlDataAdapter(cmd).Fill(resultDt);
            }
        }

        public static void ChangeInsertIdentityState(SqlConnection sqlConnection, string tableName, bool enable)
        {
            var option = (enable) ? "ON" : "OFF";
            var query = string.Format("SET IDENTITY_INSERT {0} {1}", tableName, option);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetFormatVersionsByFormatId(SqlConnection sqlConnection, int formatId, string orderBy, out int totalRecords, int startRow, int pageSize, bool pageOrTemplate)
        {
            return GetSimplePagedList(
                sqlConnection,
                pageOrTemplate ? EntityTypeCode.PageObjectFormatVersion : EntityTypeCode.TemplateObjectFormatVersion,
                "v.[OBJECT_FORMAT_VERSION_ID] as Id, v.[DESCRIPTION] as Description, v.[MODIFIED] as Modified, u.Login as [LastModifiedByLogin]",
                "[OBJECT_FORMAT_VERSION] as v inner join USERS as u on v.[LAST_MODIFIED_BY] = u.[USER_ID]",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "Id",
                "v.[OBJECT_FORMAT_ID] = " + formatId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static void RestoreObjectFormatVersion(SqlConnection connection, int versionId)
        {
            using (var cmd = SqlCommandFactory.Create("restore_object_format_version", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@version_id", versionId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateM2MValues(SqlConnection sqlConnection, string xmlParameter)
        {
            using (var cmd = SqlCommandFactory.Create("qp_update_m2m_values", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var prm = cmd.Parameters.AddWithValue("@xmlParameter", xmlParameter);
                var resultDt = new DataTable();

                new SqlDataAdapter(cmd).Fill(resultDt);
            }
        }


        public static void ValidateO2MValues(SqlConnection sqlConnection, string xmlParameter, string message)
        {
            var sql = @"
            DECLARE @NewArticles TABLE (CONTENT_ITEM_ID int, ATTRIBUTE_ID int, DATA nvarchar(3500), BLOB_DATA nvarchar(max))

            INSERT INTO @NewArticles
                SELECT
                 doc.col.value('(CONTENT_ITEM_ID)[1]', 'int') CONTENT_ITEM_ID
                ,doc.col.value('(ATTRIBUTE_ID)[1]', 'int') ATTRIBUTE_ID
                ,doc.col.value('(DATA)[1]', 'nvarchar(3500)') DATA
                ,doc.col.value('(BLOB_DATA)[1]', 'nvarchar(max)') BLOB_DATA
                FROM @xmlParameter.nodes('/PARAMETERS/FIELDVALUE') doc(col)

                select * from
                (select a.*, ca.ATTRIBUTE_NAME, rca.CONTENT_ID as RELATED_CONTENT_ID from @NewArticles a
                inner join CONTENT_ATTRIBUTE ca on a.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
                inner join CONTENT_ATTRIBUTE rca on ca.RELATED_ATTRIBUTE_ID = rca.ATTRIBUTE_ID and ca.CONTENT_ID <> rca.CONTENT_ID
                inner join CONTENT rc on rc.CONTENT_ID = rca.CONTENT_ID and rc.VIRTUAL_TYPE <> 3
                where a.DATA != ''
                ) as a
                left join CONTENT_ITEM ci on ci.CONTENT_ITEM_ID = convert(numeric, data)
                where ci.CONTENT_ID is null or ci.CONTENT_ID <> a.RELATED_CONTENT_ID
            ";
            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@xmlParameter", SqlDbType.Xml) { Value = xmlParameter });
                var resultDt = new DataTable();
                new SqlDataAdapter(cmd).Fill(resultDt);

                if (resultDt.AsEnumerable().Any())
                {
                    var dr = resultDt.Rows[0];
                    var title = dr.Field<string>("ATTRIBUTE_NAME");
                    var data = dr.Field<string>("DATA");
                    var id = dr.Field<int>("CONTENT_ITEM_ID").ToString();
                    throw new ArgumentException(string.Format(message, id, title, data));
                }
            }
        }

        public static List<int> CheckForArticlesExistence(SqlConnection sqlConnection, List<int> relatedIds, string condition, int contentId)
        {
            var result = new List<int>();
            if (!string.IsNullOrEmpty(condition))
            {
                condition = string.Format(" AND {0}", condition);
            }
            if (relatedIds.Count == 0)
            {
                relatedIds.Add(0);
            }
            var query = string.Format("select content_item_id from content_{0} c with(nolock) where content_item_id in ({1}) {2}", contentId, string.Join(",", relatedIds), condition);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, int> GetExistingArticleIdsMap(SqlConnection sqlConnection, List<string> values, string fieldName, string condition, int contentId)
        {
            var result = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(condition))
            {
                condition = string.Format(" AND {0}", condition);
            }
            if (values.Count > 0)
            {
                var query = string.Format("select content_item_id, [{0}] from content_{1}_united with(nolock) where [{0}] in ({2}) {3}", fieldName, contentId, string.Join(",", values.Select(v => "'" + v + "'")), condition);
                using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = Converter.ToInt32(reader.GetDecimal(0));
                            var value = reader.GetValue(1).ToString();
                            result.Add(value, id);
                        }
                    }
                }
            }
            return result;
        }

        public static void ModifyDataUsingXmlParameter(SqlConnection sqlConnection, string storedProc, string xmlParameter)
        {
            using (var cmd = SqlCommandFactory.Create(storedProc, sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var prm = cmd.Parameters.AddWithValue("@xmlParameter", xmlParameter);
                var resultDt = new DataTable();

                new SqlDataAdapter(cmd).Fill(resultDt);
            }
        }

        public static void RemoveLinksFromM2MField(SqlConnection sqlConnection, int linkId, List<int> articleIds)
        {
            var query = string.Format(@"DELETE FROM [dbo].[item_to_item]
                                            WHERE ([l_item_id] IN ({0}) OR [r_item_id] IN ({0})) and [link_id] = {1}", string.Join(",", articleIds), linkId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetDefaultGroupId(SqlConnection sqlConnection, int siteId)
        {
            var query = string.Format(@"select [dbo].qp_default_group_id({0})", siteId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return DBNull.Value.Equals(result) ? 0 : (int)result;
            }
        }

        public static string GetNewTemplateObjectName(SqlConnection sqlConnection, int contentId)
        {
            string objectName;
            using (var cmd = SqlCommandFactory.Create("qp_get_new_template_object_name", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                objectName = cmd.ExecuteScalar().ToString();
            }
            return objectName;
        }

        public static void CreateContainerStatusBind(SqlConnection sqlConnection, int objectId, int contentId)
        {
            var query = string.Format(@"INSERT INTO CONTAINER_STATUSES SELECT {0}, status_type_id FROM workflow AS w " +
                        "INNER JOIN workflow_rules AS wr ON w.workflow_id = wr.workflow_id " +
                        "INNER JOIN status_type s ON wr.successor_status_id = s.status_type_id " +
                        "INNER JOIN content_workflow_bind cwb ON cwb.workflow_id = w.workflow_id " +
                        "WHERE cwb.content_id = @in_content_id ORDER BY rule_order", objectId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static string FormatBodyNet(int contentId, SqlConnection sqlConnection)
        {
            var formatBody = new StringBuilder();
            var strUserFields = new StringBuilder();
            formatBody.AppendLine("<%@ Import Namespace=\"Quantumart.QPublishing\" %>")
            .AppendLine(GetDefaultNotificationStyle())
            .AppendLine("<asp:Repeater id='container' runat='server' OnItemCreated='R1_ItemCreated'>")
            .AppendLine("<HeaderTemplate>")
            .AppendLine("</HeaderTemplate>")
            .AppendLine("<ItemTemplate>")
            .AppendLine("<%# eventDescription %>")
            .AppendLine("<table cellpadding='0' cellspacing='0' class='my_table'>")
            .AppendLine(CreateRowNotifTable("<strong>Field Name</strong>", "<strong>Field Value</strong>", 0))
            .AppendLine("<!-- User Fields -->");

            var query = "SELECT attribute_name FROM content_attribute WHERE content_id = @in_content_id order by attribute_order";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);

                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        strUserFields.AppendLine(CreateRowNotifTable(dr["attribute_name"].ToString(), "<%#Field(\"" + dr["attribute_name"] + "\")%" + ">", -1));
                }
            }
            formatBody.AppendLine(strUserFields.ToString())
            .AppendLine("<!-- System Fields -->")
            .AppendLine(CreateRowNotifTable("Created", "<%#Field(\"created\")%>", -1))
            .AppendLine(CreateRowNotifTable("Modified", "<%#Field(\"modified\")%>", -1))
            .AppendLine(CreateRowNotifTable("Last Modified By", "<%#Status.GetUserName(lastModifiedBy)%>", -1))
            .AppendLine("</table>")
            .AppendLine("<%# backendLink %>")
            .AppendLine("</ItemTemplate>")
            .Append(Environment.NewLine)
            .AppendLine("<FooterTemplate>")
            .AppendLine("</FooterTemplate>")
            .AppendLine("</asp:Repeater>");
            return formatBody.ToString();
        }

        public static string CodeBehind(string currentCustomerCode, string backendUrl)
        {
            var codeBehind = new StringBuilder();
            codeBehind.AppendLine("using System; ")
            .AppendLine("using System.Web.UI.WebControls;")
            .AppendLine("using Quantumart.QPublishing;")
            .Append(Environment.NewLine)
            .AppendLine("protected Int32 lastModifiedBy, id, statusTypeId;")
            .AppendLine("protected String prevStatus, nextStatus, eventDescription, backendLink;")
            .Append(Environment.NewLine)
            .AppendLine("protected void LoadContainer(Object sender, EventArgs e)")
            .AppendLine("{")
            .AppendLine("	container.DataSource = Data;")
            .AppendLine("	container.DataBind();")
            .AppendLine("}")
            .Append(Environment.NewLine)
            .AppendLine("override public void InitUserHandlers(EventArgs e)")
            .AppendLine("{")
            .AppendLine("	LoadContainer(this, e);")
            .AppendLine("}")
            .Append(Environment.NewLine)
            .AppendLine("protected void R1_ItemCreated(Object Sender, RepeaterItemEventArgs e)")
            .AppendLine("{")
            .AppendLine("	if(e.Item.ItemType == ListItemType.Item) {")
            .AppendLine("		id = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"content_item_id\"));")
            .AppendLine("			backendLink = String.Format(\"<a href='{0}?actionCode=edit_article&entityTypeCode=article&customerCode={1}&entityId={2}'>Link to the article</a>\",\"" + backendUrl + "\", \"" + currentCustomerCode + "\", id);")
            .AppendLine("		lastModifiedBy = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"last_modified_by\"));")
            .AppendLine("		statusTypeId = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"status_type_id\"));")
            .AppendLine("		switch (Value(\"on\")) {")
            .AppendLine("		case \"for_status_changed\": ")
            .AppendLine("			prevStatus = Status.GetPreviousStatus(id);")
            .AppendLine("			nextStatus = Status.GetStatusName(statusTypeId);")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article status was changed from \"+")
            .AppendLine("\"<strong>{0}</strong> to <strong>{1}</strong></div>\",")
            .AppendLine(" prevStatus, Status.GetStatusName(statusTypeId));")
            .AppendLine("			break;")
            .AppendLine("		case \"for_status_partially_changed\":")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong><br>\"+")
            .AppendLine("\"Already Approved: {1}<br>Waiting For Approval: {2}</div>\",")
            .AppendLine(" Status.GetUserName(lastModifiedBy), Status.GetParallelApproved(id, statusTypeId), Status.GetParallelWaitingForApproval(id));")
            .AppendLine("			break;")
            .AppendLine("		case \"for_create\": ")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was created by <strong>{0}</strong></div>\", ")
            .AppendLine("Status.GetUserName(lastModifiedBy));")
            .AppendLine("			break;")
            .AppendLine("		case \"for_modify\": ")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong></div>\", ")
            .AppendLine("Status.GetUserName(lastModifiedBy));")
            .AppendLine("			break;")
            .AppendLine("		case \"for_remove\": ")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was removed by <strong>{0}</strong></div>\", ")
            .AppendLine("Status.GetUserName(lastModifiedBy));")
            .AppendLine("			break;")
            .AppendLine("		case \"for_frontend\": ")
            .AppendLine("			eventDescription = \"<div style='margin: 5px 0px;'>Article was requested from frontend.</div>\";")
            .AppendLine("			break;")
            .AppendLine("		default: ")
            .AppendLine("			eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong></div>\", ")
            .AppendLine("Status.GetUserName(lastModifiedBy));")
            .AppendLine("			break;")
            .AppendLine("		}")
            .AppendLine("	}")
            .AppendLine("}");
            return codeBehind.ToString();
        }

        public static string FormatBodyVBScript(int contentId, string currentCustomerCode, string backendUrl, SqlConnection sqlConnection)
        {
            var formatBody = new StringBuilder();
            formatBody.AppendLine(GetDefaultNotificationStyle())
            .AppendLine("<%")
            .AppendLine("	Dim prevStatus")
            .AppendLine("	prevStatus = GetPreviousStatus(Field(\"content_item_id\"))")
            .AppendLine("	If IsEmpty(prevStatus) Then prevStatus = \"None\" ")
            .AppendLine("%>")
            .AppendLine("<!-- Article Status -->")
            .AppendLine("<div style='margin: 5px 0px;'>Article status was changed from <strong><" + "%=prevStatus%" + "></strong> to <strong><" + "%=GetStatusName(Field(\"status_type_id\"))%" + "></strong></div>")
            .AppendLine("<table cellpadding='0' cellspacing='0' class='my_table'>")
            .AppendLine(CreateRowNotifTable("<strong>Field Name</strong>", "<strong>Field Value</strong>", 0))
            .AppendLine("<!-- User Fields -->");
            var strUserFields = new StringBuilder();
            var strSql = "SELECT attribute_name FROM content_attribute WHERE content_id = @in_content_id";
            using (var cmd = SqlCommandFactory.Create(strSql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        strUserFields.AppendLine(
                        CreateRowNotifTable(dr["attribute_name"].ToString(), "<%=Field(\"" + dr["attribute_name"] + "\")%>", -1));
                }
            }
            formatBody.AppendLine(strUserFields.ToString())
            .AppendLine("<!-- System Fields -->")
            .AppendLine(CreateRowNotifTable("Created", "<%=Field(\"created\")%>", -1))
            .AppendLine(CreateRowNotifTable("Modified", "<%=Field(\"modified\")%>", -1))
            .AppendLine(CreateRowNotifTable("Last Modified By", "<%=GetLastModifiedLogin(Field(\"last_modified_by\"))%>", -1))
            .AppendLine(CreateRowNotifTable("", "<a href='" + backendUrl + "?actionCode=edit_article&entityTypeCode=article&customerCode=" + currentCustomerCode + "&entityId=<%=Field(\"content_item_id\")%>'>Link to the article</a>", -1))
            .AppendLine("</table>");

            return formatBody.ToString();
        }

        public static string CreateRowNotifTable(string td1, string td2, int num)
        {
            var styleStr = num == 0 ? "bgcolor='#C0C0C0'" : string.Empty;
            var result = new StringBuilder();
            result.AppendLine("<tr>")
            .AppendFormatLine("<td class='left_td' nowrap {0}>{1}</td>", styleStr, td1)
            .AppendFormatLine("<td class='right_td' {0}>{1}&nbsp;</td>", styleStr, td2);
            return result.ToString();
        }

        public static string GetDefaultNotificationStyle()
        {
            var defaultStyle = new StringBuilder();
            defaultStyle.AppendLine("<style type='text/css'>")
            .AppendLine(".my_table ")
            .AppendLine("{")
            .AppendLine("	border: 1px solid Black;")
            .AppendLine("}")
            .AppendLine("td.left_td")
            .AppendLine("{")
            .AppendLine("	border-bottom-color:	Black;")
            .AppendLine("	border-bottom-style:	solid;")
            .AppendLine("	border-bottom-width:	1px;")
            .AppendLine("	border-right-style:		solid;")
            .AppendLine("	border-right-width:		1px;")
            .AppendLine("	padding:				4px;")
            .AppendLine("}")
            .AppendLine("td.right_td")
            .AppendLine("{")
            .AppendLine("	border-bottom-color:	Black;")
            .AppendLine("	border-bottom-style:	solid;")
            .AppendLine("	border-bottom-width:	1px;")
            .AppendLine("	padding:				4px;")
            .AppendLine("}")
            .AppendLine("</style>");

            return defaultStyle.ToString();
        }

        public static IEnumerable<DataRow> GetRestTemplateObjects(SqlConnection sqlConnection, int templateId, int siteId)
        {
            var query = string.Format(@"select t.[TEMPLATE_NAME] as TemplateName, t.PAGE_TEMPLATE_ID, o.[OBJECT_ID], o.PAGE_ID,
                           o.[OBJECT_NAME] as ObjectName, CASE WHEN f.[OBJECT_FORMAT_ID] = o.[OBJECT_FORMAT_ID] THEN null ELSE f.[FORMAT_NAME] END as FormatName
                           from [OBJECT_FORMAT] as f inner join [OBJECT] as o on f.[OBJECT_ID] = o.[OBJECT_ID]
                           inner join [PAGE_TEMPLATE] as t on t.[page_template_id] = o.[page_template_id]
                           where t.[SITE_ID] = {0} and t.[PAGE_TEMPLATE_ID] <> {1} and o.[PAGE_ID] is null", siteId, templateId);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void DeleteObjectDefaultValues(SqlConnection sqlConnection, int objectId)
        {
            using (var cmd = SqlCommandFactory.Create(string.Format("delete from OBJECT_VALUES where OBJECT_ID = {0}", objectId), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static bool PageFileNameExists(SqlConnection sqlConnection, string checkPath, int siteId)
        {
            var query = string.Format(@"select COUNT(*)
                                         from PAGE as p left outer join PAGE_TEMPLATE as t on p.PAGE_TEMPLATE_ID = t.PAGE_TEMPLATE_ID
                                         inner join dbo.[SITE] as s on t.SITE_ID = s.[SITE_ID]
                                         where (s.LIVE_DIRECTORY + ISNULL(t.TEMPLATE_FOLDER, '') + ISNULL(p.PAGE_FOLDER, '') + p.PAGE_FILENAME) = '{0}'
                                         and s.SITE_ID = {1}", checkPath, siteId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return (int)result > 0;
            }
        }

        public static void ClearFieldTreeOrder(SqlConnection sqlConnection, int Id)
        {
            var query = string.Format(@"UPDATE dbo.[CONTENT_ATTRIBUTE] set [TREE_ORDER_FIELD] = null
                                         where  [TREE_ORDER_FIELD] = '{0}'", Id);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void MoveFieldOrders(SqlConnection sqlConnection, int contentId, int newOrder)
        {
            var query = string.Format(@"UPDATE [CONTENT_ATTRIBUTE]
                                         SET [ATTRIBUTE_ORDER] = [ATTRIBUTE_ORDER] + 1
                                         WHERE CONTENT_ID = {0} and [ATTRIBUTE_ORDER] >= {1}",
                                         contentId, newOrder);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<int> GetActiveArticlesIdsForM2mField(SqlConnection sqlConnection, int fieldId)
        {
            var query = string.Format("select ARTICLE_ID FROM FIELD_ARTICLE_BIND WHERE FIELD_ID = {0}", fieldId);

            var result = new List<int>();
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Converter.ToInt32(reader.GetDecimal(0)));
                    }
                }
            }
            return result.ToArray();
        }

        public static void UpdateContentModification(SqlConnection sqlConnection, int contentId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("update content_modification with(rowlock) set live_modified = GETDATE(), stage_modified = GETDATE() where content_id = @contentId");
            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var prm = cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentModification(SqlConnection sqlConnection, IEnumerable<int> liveIds, IEnumerable<int> stageIds)
        {
            var sb = new StringBuilder();
            var sql = new List<SqlParameter>();

            if (liveIds != null && liveIds.Any())
            {
                sb.AppendLine("update content_modification with(rowlock) set live_modified = GETDATE() where content_id in (select id from @liveIds)");
                sql.Add(new SqlParameter("@liveIds", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(liveIds) });
            }
            if (stageIds != null && stageIds.Any())
            {
                sb.AppendLine("update content_modification with(rowlock) set stage_modified = GETDATE() where content_id in (select id from @stageIds)");
                sql.Add(new SqlParameter("@stageIds", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(stageIds) });
            }

            if (sql.Any())
            {
                using (var cmd = new SqlCommand(sb.ToString(), sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(sql.ToArray());
                    cmd.ExecuteNonQuery();
                }
            }

        }


        public static void GetContentModification(SqlConnection sqlConnection, IEnumerable<int> articleIds, bool withAggregated, ref IEnumerable<int> liveIds, ref IEnumerable<int> stageIds)
        {
            var source = withAggregated ? "dbo.qp_aggregated_and_self(@ids)" : "@ids";
            var sb = new StringBuilder();
            if (articleIds != null && articleIds.Any())
            {
                sb.AppendLine("declare @fullIds table (id numeric primary key, content_id numeric, is_published bit)");
                sb.AppendLine("insert into @fullIds select ci.content_item_id, ci.content_id, ");
                sb.AppendLine("  case when st.status_type_name = 'Published' and ci.splitted = 0 then 1 else 0 end as is_published ");
                sb.AppendLine(string.Format("  from {0} i inner join content_item ci with(nolock) on i.id = ci.content_item_id ", source));
                sb.AppendLine("  inner join status_type st on ci.status_type_id = st.status_type_id ");
                sb.AppendLine("select cast(content_id as int) as id, cast(max(cast(is_published as int)) as bit) as live from @fullIds group by content_id");


                using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(articleIds) });
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    var rows = dt.AsEnumerable().ToArray();
                    stageIds = rows.Select(n => n.Field<int>("id")).ToList();
                    liveIds = rows.Where(n => n.Field<bool>("live")).Select(n => n.Field<int>("id")).ToList();
                }
            }
        }

        public static void SetFieldM2MDefValue(SqlConnection sqlConnection, int[] defaultArticles, int fieldId)
        {
            var sb = new StringBuilder();
            sb.AppendFormatLine(" DELETE FROM FIELD_ARTICLE_BIND where FIELD_ID = {0} ", fieldId);

            foreach (var artId in defaultArticles)
            {
                sb.AppendFormatLine(" INSERT INTO FIELD_ARTICLE_BIND([ARTICLE_ID],[FIELD_ID]) VALUES ({0},{1}) ", artId, fieldId);
            }

            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenContents(SqlConnection sqlConnection, int oldSiteId, int newSiteId, string newContentIds)
        {
            var sb = new StringBuilder();

            sb.Append(@"	select c.content_id as source_content_id, nc.content_id as destination_content_id
                                from [dbo].[content] as c (nolock)
                                inner join [dbo].[content] as nc (nolock)
                                    on nc.content_name = c.content_name and nc.site_id = @newSiteId
                                    where c.site_id = @oldSiteId");

            if (!string.IsNullOrEmpty(newContentIds))
            {
                sb.AppendFormat(" and nc.content_id in ({0})", newContentIds);
            }


            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationshipsBetweenAttributes(SqlConnection sqlConnection, int oldSiteId, int newSiteId, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"	declare @xmlprms xml = '{0}'

                                ;with relationsBetweenContents as (
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                )
                                select ca.attribute_id as source_attribute_id
                                            , ca1.attribute_id as destination_attribute_id
                                from [dbo].content_attribute as ca (nolock)
                                inner join relations_between_contents as ra on ra.source_content_id = ca.content_id
                                inner join [dbo].content_attribute as ca1 (nolock) on ca1.attribute_name = ca.attribute_name and ca1.content_id = ra.destination_content_id
                                where ra.destination_content_id in (select destinationId from relationsBetweenContents)", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentItemAccess(SqlConnection sqlConnection, string relationsBetweenItems)
        {
            var query = string.Format(@"
                                            declare @now DateTime = GetDate()
                                            declare @xmlprmsContentItems xml = '{0}'
                                            ;with relations_between_content_items as (
                                                select doc.col.value('./@sourceId', 'int') source_content_item_id
                                                 ,doc.col.value('./@destinationId', 'int') destination_content_item_id
                                                from @xmlprmsContentItems.nodes('/items/item') doc(col)
                                            )
                                            insert into [dbo].[content_item_access]
                                            ( [content_item_id]
                                                  ,[user_id]
                                                  ,[group_id]
                                                  ,[permission_level_id]
                                                  ,[created]
                                                  ,[modified]
                                                  ,[last_modified_by])
                                            select rbci.destination_content_item_id
                                                  ,[user_id]
                                                  ,[group_id]
                                                  ,[permission_level_id]
                                                  ,@now [created]
                                                  ,@now [modified]
                                                  ,[last_modified_by]
                                              from [dbo].[content_item_access] as cia (nolock)
                                                inner join relations_between_content_items as rbci on rbci.source_content_item_id = cia.content_item_id", relationsBetweenItems);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static Dictionary<int, int> GetArticleHierarchy(SqlConnection sqlConnection, int contentId, string treeFieldName)
        {
            var result = new Dictionary<int, int>();
            var parentIdParam = (string.IsNullOrEmpty(treeFieldName)) ? "cast(0 as numeric)" : "ISNULL([" + treeFieldName + "], 0)";
            var sql = string.Format("select content_item_id as id, {0} as parent_id from content_{1}_united with(nolock)", parentIdParam, contentId);
            using (var cmd = new SqlCommand(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add((int)rdr.GetDecimal(0), (int)rdr.GetDecimal(1));
                    }
                }
            }
            return result;
        }

        public static void CopyUserQueryContents(SqlConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"
                                    declare @xmlprms xml = '{0}'

                                    ;with relationsBetweenContents as (
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[user_query_contents]
                                    select rbc.destination_content_id
                                          ,rbc1.destination_content_id
                                          ,[is_id_source]
                                      from [dbo].[user_query_contents] as uqc (nolock)
                                    inner join relationsBetweenContents as rbc on uqc.virtual_content_id = rbc.source_content_id
                                    inner join relationsBetweenContents as rbc1 on uqc.real_content_id = rbc1.source_content_id", relationsBetweenContentsXml);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyUserQueryAttributes(SqlConnection sqlConnection, string relationsBetweenContents, string relationsBetweenAttributes)
        {
            var query = string.Format(@"

                                    declare @xmlprmsAttributes xml = '{0}'

                                    declare @relationsBetweenAttributes table(
                                        source_attribute_id int,
                                        destination_attribute_id int
                                    )

                                    insert into @relationsBetweenAttributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                    declare @xmlprms xml = '{1}'

                                    ;with relationsBetweenContents as (
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[user_query_attrs]
                                    select rbc.destination_content_id
                                          ,rba.destination_attribute_id
                                    from [dbo].[user_query_attrs] as uqa (nolock)
                                    inner join relationsBetweenContents as rbc on rbc.source_content_id = uqa.virtual_content_id
                                    inner join @relationsBetweenAttributes as rba on rba.source_attribute_id = uqa.user_query_attr_id", relationsBetweenAttributes, relationsBetweenContents);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyContentItems(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string contentIdsToCopy, int startFrom, int endOn, string relationsBetweenContentsXml, string relationsBetweenStatusesXml)
        {
            var query = string.Format(@"

                                if OBJECT_ID('tempdb..#disable_ti_access_content_item') IS NULL begin
                                    select 1 as A into #disable_ti_access_content_item
                                end

                                declare @isVirtual bit = 0
                                declare @todaysDateTime datetime = GetDate();
                                declare @not_for_replication bit  = 1

                                declare @xmlprmsContents xml = '{0}'

                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )
                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)


                                declare @xmlprmsStatuses xml = '{1}'

                                declare @relations_between_statuses table (
                                    source_status_id int,
                                    destination_status_id int
                                )
                                insert into @relations_between_statuses
                                        select doc.col.value('./@sourceId', 'int') source_status_id
                                         ,doc.col.value('./@destinationId', 'int') destination_status_id
                                        from @xmlprmsStatuses.nodes('/items/item') doc(col)

                                declare @contents table(
                                    id int primary key
                                )

                                insert into @contents
                                select cast(nstr as numeric) from dbo.splitNew(@contentIdsToCopy, ',')

                                declare @items table
                                (
                                    id int primary key
                                )

                                insert into @items
                                select [content_item_id]
                                from (
                                        select row_number() over (order by content_item_id desc) as rownumber
                                            ,content_item_id
                                        from [dbo].[content_item] (NOLOCK) as c2
                                        inner join [dbo].[content] as c (NOLOCK) on c.content_id = c2.content_id
                                        where c.site_id = @oldsiteid and c.CONTENT_ID  in (select id from @contents)
                                    )  as c1
                                    where c1.rownumber between @startfrom and @endon

                                declare @contentitemstable table(
                                        source_item_id int ,
                                        contentid int,
                                        destination_item_id int,
                                        primary key ( destination_item_id, source_item_id, contentid));

                                    ;with content_items as (
                                    select [content_item_id]
                                          ,[visible]
                                          ,rbs.destination_status_id as status_type_id
                                          ,@todaysDateTime as created
                                          ,@todaysDateTime as modified
                                          ,rc.destination_content_id as content_id
                                          ,c1.[last_modified_by]
                                          ,[locked_by]
                                          ,[archive]
                                          ,@not_for_replication as [not_for_replication]
                                          ,[schedule_new_version_publication]
                                          ,[splitted]
                                          ,[cancel_split]
                                    from [dbo].[content_item] (NOLOCK) as c1
                                        inner join @relations_between_contents as rc on rc.source_content_id = c1.content_id
                                        inner join @relations_between_statuses as rbs on c1.[status_type_id] = rbs.source_status_id
                                    where c1.content_item_id in (select id from @items)
                                )
                                merge [dbo].[content_item]
                                using(
                                    select content_item_id
                                      ,[visible]
                                      ,[status_type_id]
                                      ,[created]
                                      ,[modified]
                                      ,[content_id]
                                      ,[last_modified_by]
                                      ,[locked_by]
                                      ,[archive]
                                      ,[not_for_replication]
                                      ,[schedule_new_version_publication]
                                      ,[splitted]
                                      ,[cancel_split]
                                    from content_items as t
                                ) as src(content_item_id, [visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
                                on 0 = 1
                                when not matched then
                                    insert ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
                                    values ([visible], [status_type_id],[created],[modified], content_id,[last_modified_by],[locked_by],[archive],[not_for_replication],[schedule_new_version_publication],[splitted],[cancel_split])
                                    output  src.content_item_id, inserted.content_id, inserted.content_item_id
                                        into @contentitemstable;

                                select source_item_id, destination_item_id from @contentitemstable"
                                                                                                , relationsBetweenContentsXml
                                                                                                , relationsBetweenStatusesXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.Parameters.AddWithValue("@contentIdsToCopy", contentIdsToCopy);
                cmd.Parameters.AddWithValue("@startFrom", startFrom);
                cmd.Parameters.AddWithValue("@endOn", endOn);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentItemSchedule(SqlConnection sqlConnection, string relationsNewContentItemsIdXml)
        {
            var query = string.Format(@"
                                declare @todaysDateTime datetime = GetDate()
                                declare @xmlprms xml = '{0}'

                                ;with relationsBetweenNewContentItemsXml as (
                                        select doc.col.value('./@sourceId', 'int') source_content_item_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_item_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                )
                                insert into [dbo].[CONTENT_ITEM_SCHEDULE](
                                      [content_item_id]
                                      ,[maximum_occurences]
                                      ,[created]
                                      ,[modified]
                                      ,[last_modified_by]
                                      ,[freq_type]
                                      ,[freq_interval]
                                      ,[freq_subday_type]
                                      ,[freq_subday_interval]
                                      ,[freq_relative_interval]
                                      ,[freq_recurrence_factor]
                                      ,[active_start_date]
                                      ,[active_end_date]
                                      ,[active_start_time]
                                      ,[active_end_time]
                                      ,[occurences]
                                      ,[use_duration]
                                      ,[duration]
                                      ,[duration_units]
                                      ,[deactivate]
                                      ,[delete_job]
                                      ,[use_service]
                                    )
                                    SELECT
                                        cist.destination_content_item_id
                                      ,[maximum_occurences]
                                      ,@todaysDateTime
                                      ,@todaysDateTime
                                      ,[last_modified_by]
                                      ,[freq_type]
                                      ,[freq_interval]
                                      ,[freq_subday_type]
                                      ,[freq_subday_interval]
                                      ,[freq_relative_interval]
                                      ,[freq_recurrence_factor]
                                      ,[active_start_date]
                                      ,[active_end_date]
                                      ,[active_start_time]
                                      ,[active_end_time]
                                      ,[occurences]
                                      ,[use_duration]
                                      ,[duration]
                                      ,[duration_units]
                                      ,[deactivate]
                                      ,[delete_job]
                                      ,1 as[use_service]
                                      FROM [dbo].[CONTENT_ITEM_SCHEDULE] as cis (NOLOCK)
                                        inner join relationsBetweenNewContentItemsXml as cist
                                            on cis.CONTENT_ITEM_ID = cist.source_content_item_id;", relationsNewContentItemsIdXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenStatuses(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @"	select st1.STATUS_TYPE_ID as source_status_type_id
                                                    ,st2.STATUS_TYPE_ID as destination_status_type_id
                                            from [dbo].[status_type] as st1 (NOLOCK)
                                            inner join [dbo].[status_type] as st2 (NOLOCK)
                                                on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                                            where st1.SITE_ID = @oldSiteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenLinks(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @" select distinct oldvalues.link_id as source_link_id, newvalues.link_id as destination_link_id from (
                                    select attribute_name, link_id, c.content_name
                                      from [dbo].[content_attribute] as ca
                                      inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
                                      inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation'
                                      where c.site_id = @sourceSiteId and link_id is not null) as oldvalues
                                    inner join (
                                        select attribute_name, link_id, c.content_name
                                          from [dbo].[content_attribute] as ca
                                          inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
                                          inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation'
                                          where c.site_id = @destinationSiteId and link_id is not null)
                                          as newvalues
                                          on newvalues.attribute_name = oldvalues.attribute_name and newvalues.content_name = oldvalues.content_name";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void UpdateContentData(SqlConnection sqlConnection, string relationsBetweenAttributesXml, string relationsNewContentItemsIdXml)
        {
            var query = string.Format(@"

                                select 1 as A into #disable_tu_update_child_content_data;

                                declare @xmlprmsAttributes xml = '{0}'

                                declare @relations_between_attributes table (
                                    source_attribute_id int,
                                    destination_attribute_id int
                                )
                                insert into @relations_between_attributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                declare @xmlprmsContentItems xml = '{1}'

                                declare @relations_between_content_items table (
                                    source_content_item_id int,
                                    destination_content_item_id int
                                )
                                insert into @relations_between_content_items
                                        select doc.col.value('./@sourceId', 'int') source_content_item_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_item_id
                                        from @xmlprmsContentItems.nodes('/items/item') doc(col)

                                update copydata
                                set [data] =(	case	when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.destination_attribute_id as varchar))
                                                        when at.[TYPE_NAME] = 'Relation Many-to-One' and cd.DATA is not null then CAST(ra1.destination_attribute_id as varchar)
                                                        else cd.data
                                                end)
                                  ,[blob_data] = cd.BLOB_DATA
                                from [dbo].[content_data] as copydata (NOLOCK)
                                    inner join @relations_between_attributes as ra on ra.destination_attribute_id = copydata.ATTRIBUTE_ID
                                    inner join @relations_between_content_items as cit on cit.destination_content_item_id = copydata.CONTENT_ITEM_ID
                                    inner join [dbo].[content_data] as cd (NOLOCK) on
                                        cd.ATTRIBUTE_ID = ra.source_attribute_id and cd.CONTENT_ITEM_ID = cit.source_content_item_id
                                    inner join [dbo].[CONTENT_ATTRIBUTE] as ca (NOLOCK)
                                        on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
                                    inner join [dbo].[attribute_type] as at (NOLOCK)
                                        on ca.ATTRIBUTE_TYPE_ID = at.ATTRIBUTE_TYPE_ID
                                    left join @relations_between_attributes ra1 on CAST(ra1.source_attribute_id as nvarchar) = cd.DATA;



                                declare @ids varchar(max)
                                select @ids = COALESCE(@ids + ', ', '') + CAST(destination_content_item_id as nvarchar) from @relations_between_content_items
                                if @ids is not null
                                    exec qp_replicate_items @ids "
                                                                                    , relationsBetweenAttributesXml
                                                                                    , relationsNewContentItemsIdXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetArticlesCountInSite(SqlConnection sqlConnection, int siteId)
        {
            var query = string.Format(@"SELECT COUNT(CONTENT_ITEM_ID)
                      FROM [dbo].[CONTENT_ITEM] as it
                      INNER JOIN [dbo].[CONTENT] as c on c.CONTENT_ID = it.CONTENT_ID
                      where c.SITE_ID = {0}", siteId);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return Converter.ToInt32(result);
            }
        }

        public static void UpdateAttributesAfterCopyingArticles(SqlConnection sqlConnection, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = string.Format(@"declare @xmlprms xml = '{0}'

                                            ;with relations_between_items as (
                                                    select doc.col.value('./@sourceId', 'int') olditemid
                                                    , doc.col.value('./@destinationId', 'int') newitemid
                                                    from @xmlprms.nodes('/item') doc(col)
                                            )
                                            update content_attribute
                                            set default_value = rbi.newitemid
                                            from content_attribute as ca (nolock)
                                            inner join relations_between_items as rbi
                                                on ca.default_value = CAST(rbi.olditemid as varchar)
                                            inner join content as c
                                                on c.content_id = ca.content_id
                                            where c.site_id = @destinationSiteId
                                        ", relationsBetweenArticles);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateO2MValues(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = string.Format(@"
                            declare @xmlprms xml = '{0}'

                            if OBJECT_ID('tempdb..#disable_tu_update_child_content_data') IS NULL begin
                                select 1 as A into #disable_tu_update_child_content_data
                            end

                            ;with relations_between_items as (
                                    select doc.col.value('./@sourceId', 'int') olditemid
                                    , doc.col.value('./@destinationId', 'int') newitemid
                                    from @xmlprms.nodes('/item') doc(col)
                            )
                            update [dbo].[content_data]
                                set data = (case when lr.newitemid is not null then lr.newitemid else cd.DATA end)
                                from [dbo].[content_data] cd (nolock)
                                inner join CONTENT_ATTRIBUTE as ca (nolock)
                                    on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
                                inner join ATTRIBUTE_TYPE as at (nolock)
                                    on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
                                inner join [dbo].[CONTENT_ITEM] as ci (nolock)
                                    on ci.CONTENT_ITEM_ID = cd.CONTENT_ITEM_ID
                                inner join [dbo].[CONTENT] as c (nolock)
                                    on c.CONTENT_ID = ci.CONTENT_ID
                                inner join relations_between_items lr
                                    on cast(lr.olditemid as varchar) = cd.DATA
                                where c.SITE_ID = @destinationSiteId", relationsBetweenArticles);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyArticleWorkflowBind(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = string.Format(@"
                            declare @relations_between_items table(
                                olditemid numeric,
                                newitemid numeric
                            )
                            declare @xmlprms xml = '{0}'

                            insert into @relations_between_items
                            select doc.col.value('./@oldId', 'int') olditemid
                                     ,doc.col.value('./@newId', 'int') newitemid
                                    from @xmlprms.nodes('/item') doc(col)


                            ;with relations_between_workflows
                                    as
                                    (
                                        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                                                ,w2.WORKFLOW_ID as new_workflow_id
                                        FROM [dbo].[workflow] as w1 (nolock)
                                            inner join [dbo].[workflow] as w2 (nolock)
                                                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @destinationSiteId
                                        where w1.SITE_ID = @sourceSiteId
                                    )
                            insert into [dbo].[article_workflow_bind]
                                select lr.newitemid
                                  ,rbw.new_workflow_id
                                  ,[is_async]
                                from [dbo].[article_workflow_bind] as awb (nolock)
                                inner join @relations_between_items as lr on lr.olditemid = awb.content_item_id
                                inner join relations_between_workflows as rbw on rbw.old_workflow_id = awb.workflow_id", relationsBetweenArticles);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyFieldArticleBind(SqlConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenAttributes)
        {
            var query = string.Format(@"
                                declare @xmlprmsAttributes xml = '{0}'

                                declare @relations_between_attributes table (
                                    source_attribute_id int,
                                    destination_attribute_id int
                                )
                                insert into @relations_between_attributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{1}'

                                ;with relations_between_items as (
                                select doc.col.value('./@oldId', 'int') olditemid
                                         ,doc.col.value('./@newId', 'int') newitemid
                                        from @xmlprmsItems.nodes('/item') doc(col)
                                )
                                insert into [dbo].[field_article_bind] ([article_id],[field_id])
                                select lr.newitemid
                                  ,ra.destination_attribute_id
                                from [dbo].[field_article_bind] as fab (nolock)
                                inner join relations_between_items as lr on lr.olditemid = fab.article_id
                                inner join @relations_between_attributes as ra on ra.source_attribute_id = fab.field_id", relationsBetweenAttributes, relationsBetweenArticles);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentDataAfterCopyingArticles(SqlConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = string.Format(@"
                                declare @xmlprmsLinks xml = '{0}'

                                if OBJECT_ID('tempdb..#disable_tu_update_child_content_data') IS NULL begin
                                    select 1 as A into #disable_tu_update_child_content_data
                                end

                                declare @relations_between_links table (
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{1}'

                                ;with relations_between_items as (
                                select doc.col.value('./@sourceId', 'int') olditemid
                                         ,doc.col.value('./@destinationId', 'int') newitemid
                                        from @xmlprmsItems.nodes('/item') doc(col)
                                )
                                update [dbo].[content_data]
                                set data = (case when lc.destination_link_id is not null then lc.destination_link_id else cd.DATA end)
                                from [dbo].[content_data] cd (nolock)
                                left join @relations_between_links lc
                                    on  cast(lc.source_link_id as varchar) = cd.DATA
                                inner join CONTENT_ATTRIBUTE as ca (nolock)
                                    on ca.ATTRIBUTE_ID = cd.ATTRIBUTE_ID
                                inner join ATTRIBUTE_TYPE as at (nolock)
                                    on at.ATTRIBUTE_TYPE_ID = ca.ATTRIBUTE_TYPE_ID and at.TYPE_NAME = 'Relation'
                                inner join relations_between_items as lr1
                                    on lr1.newitemid = cd.content_item_id", relationsBetweenLinks, relationsBetweenArticles);


            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyItemToItems(SqlConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = string.Format(@"

                                if OBJECT_ID('tempdb..#disable_ti_item_to_item') IS NULL begin
                                    select 1 as A into #disable_ti_item_to_item
                                end

                                declare @xmlprmsLinks xml = '{0}'

                                declare @relations_between_links table (
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{1}'

                                ;with relations_between_items as (
                                select doc.col.value('./@sourceId', 'int') olditemid
                                         ,doc.col.value('./@destinationId', 'int') newitemid
                                        from @xmlprmsItems.nodes('/item') doc(col)
                                )
                                insert into [dbo].[item_to_item]
                                select r.destination_link_id, i1.l_item_id, i1.r_item_id
                                from [dbo].[item_to_item] as i1 (nolock)
                                inner join @relations_between_links as r
                                    on r.source_link_id = i1.link_id
                                where i1.l_item_id in (select olditemid from relations_between_items) or i1.r_item_id in (select olditemid from relations_between_items)", relationsBetweenLinks, relationsBetweenArticles);


            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateItemToItem(SqlConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = string.Format(@"
                                declare @xmlprmsLinks xml = '{0}'

                                if OBJECT_ID('tempdb..#disable_ti_item_to_item') IS NULL begin
                                    select 1 as A into #disable_ti_item_to_item
                                end

                                declare @relations_between_links table (
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{1}'

                                declare @relations_between_items table (
                                    source_item_id int,
                                    destination_item_id int
                                )
                                insert into @relations_between_items
                                select doc.col.value('./@sourceId', 'int') source_item_id
                                         ,doc.col.value('./@destinationId', 'int') destination_item_id
                                        from @xmlprmsItems.nodes('/item') doc(col)

                                update [dbo].[item_to_item]
                                set l_item_id = lr.destination_item_id
                                from [dbo].[item_to_item] as ii
                                inner join @relations_between_items as lr on
                                    ii.l_item_id = lr.source_item_id
                                inner join @relations_between_links as r
                                    on r.destination_link_id = ii.link_id
                                where ii.l_item_id in (select source_item_id from @relations_between_items)

                                update [dbo].[item_to_item]
                                set r_item_id = lr.destination_item_id
                                from [dbo].[item_to_item] as ii
                                inner join @relations_between_items as lr on
                                    ii.r_item_id = lr.source_item_id
                                inner join @relations_between_links as r
                                    on r.destination_link_id = ii.link_id", relationsBetweenLinks, relationsBetweenArticles);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        #region Copy site settings
        public static void CopyWorkflow(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"       declare @todaysDate datetime
                                    set @todaysDate = GETDATE()

                                    insert into [dbo].[workflow]
                                        (
                                           [workflow_name]
                                          ,[description]
                                          ,[created]
                                          ,[modified]
                                          ,[last_modified_by]
                                          ,[site_id]
                                          ,[create_default_notification]
                                          ,[apply_by_default]
                                        )
                                    SELECT [WORKFLOW_NAME]
                                          ,w.[DESCRIPTION]
                                          ,@todaysDate
                                          ,@todaysDate
                                          ,w.[LAST_MODIFIED_BY]
                                          ,@destinationSiteId
                                          ,[create_default_notification]
                                          ,[apply_by_default]
                                      FROM [dbo].[workflow] as w
                                        inner join [dbo].[SITE] as s
                                            on w.SITE_ID = s.SITE_ID and s.SITE_ID = @sourceSiteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteAccessRules(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"    declare @todaysDate datetime
                                    set @todaysDate = GETDATE()

                                    -- copying site access rules
                                    insert into [dbo].[site_access](
                                     [site_id]
                                          ,[user_id]
                                          ,[group_id]
                                          ,[permission_level_id]
                                          ,[created]
                                          ,[modified]
                                          ,[last_modified_by]
                                          ,[propagate_to_contents]
                                    )
                                    select @destinationSiteId
                                          ,[user_id]
                                          ,[group_id]
                                          ,[permission_level_id]
                                          ,@todaysDate
                                          ,@todaysDate
                                          ,[last_modified_by]
                                          ,[propagate_to_contents]
                                      from [dbo].[site_access] (nolock)
                                      where site_id = @sourceSiteId and group_id != 1 and permission_level_id != 1 and last_modified_by != 1";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyActionSiteBind(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"               insert into [dbo].[action_site_bind]
                                            select [custom_action_id]
                                                  ,@destinationSiteId
                                              from [dbo].[action_site_bind] (nolock)
                                              where site_id = @sourceSiteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyWorkflowAccess(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"       declare @now DateTime = GetDate()
                                    ;with relations_between_workflows
                                    as
                                    (
                                        SELECT w1.[WORKFLOW_ID] as source_workflow_id
                                                ,w2.WORKFLOW_ID as destination_workflow_id
                                        FROM [dbo].[workflow] as w1
                                            inner join [dbo].[workflow] as w2
                                                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @destinationSiteId
                                        where w1.SITE_ID = @sourceSiteId
                                    )
                                    insert into [dbo].[workflow_access]
                                    ([workflow_id]
                                      ,[user_id]
                                      ,[group_id]
                                      ,[permission_level_id]
                                      ,[created]
                                      ,[modified]
                                      ,[last_modified_by])
                                    select rbw.destination_workflow_id
                                      ,[user_id]
                                      ,[group_id]
                                      ,[permission_level_id]
                                      ,@now as [created]
                                      ,@now as [modified]
                                      ,[last_modified_by]
                                  from [dbo].[workflow_access] as wa (nolock)
                                    inner join relations_between_workflows as rbw on rbw.source_workflow_id = wa.workflow_id";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyWorkflowRules(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"       ;with relations_between_workflows
                                    as
                                    (
                                        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                                                ,w2.WORKFLOW_ID as new_workflow_id
                                        FROM [dbo].[workflow] as w1
                                            inner join [dbo].[workflow] as w2
                                                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @destinationSiteId
                                        where w1.SITE_ID = @sourceSiteId
                                    )
                                    insert into [dbo].[workflow_rules]
                                    select [user_id]
                                          ,[group_id]
                                          ,[rule_order]
                                          ,[predecessor_permission_id]
                                          ,[successor_permission_id]
                                          ,[successor_status_id]
                                          ,[comment]
                                          ,rbw.new_workflow_id
                                      from [dbo].[workflow_rules] as wr (nolock)
                                        inner join relations_between_workflows as rbw
                                            on wr.WORKFLOW_ID = rbw.old_workflow_id";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyFolderAccess(SqlConnection sqlConnection, string relationsBetweenFoldersXml)
        {
            var query = string.Format(@"
                            declare @now DateTime = GetDate()

                            declare @xmlprmsFolders xml = '{0}'

                            ;with relations_between_folders as (
                            select doc.col.value('./@sourceId', 'int') source_folder_id
                                     ,doc.col.value('./@destinationId', 'int') destination_folder_id
                                    from @xmlprmsFolders.nodes('/items/item') doc(col)
                            )
                            insert into [dbo].[folder_access](
                                    [folder_id]
                                  ,[user_id]
                                  ,[group_id]
                                  ,[permission_level_id]
                                  ,[created]
                                  ,[modified]
                                  ,[last_modified_by]
                            )
                            select rbf.destination_folder_id
                                  ,[user_id]
                                  ,[group_id]
                                  ,[permission_level_id]
                                  ,@now as [created]
                                  ,@now as [modified]
                                  ,[last_modified_by]
                              from [dbo].[folder_access] as fa (nolock)
                                inner join relations_between_folders as rbf on rbf.source_folder_id = fa.[folder_id]", relationsBetweenFoldersXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyFolders(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"    declare @todaysDate datetime
                                    set @todaysDate = GETDATE()


                                    declare @relations_between_folders table(
                                        old_folder_id int,
                                        new_folder_id int
                                    )

                                    --copying folders
                                    merge into [dbo].[folder]
                                    using (
                                    select @destinationSiteId
                                      ,[folder_id]
                                      ,[parent_folder_id]
                                      ,[name]
                                      ,[description]
                                      ,[filter]
                                      ,[path]
                                      ,@todaysDate
                                      ,@todaysDate
                                      ,[last_modified_by]
                                    from [dbo].[folder]
                                    where SITE_ID = @sourceSiteId)
                                    as src (site_id,[folder_id],[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
                                    on 0 = 1
                                    when not matched then
                                    insert (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
                                    values (site_id,[parent_folder_id],[name],[description],[filter],[path],[created], [modified],[last_modified_by])
                                    output src.[folder_id], inserted.[folder_id]
                                        into @relations_between_folders;

                                    update [dbo].[folder]
                                    set [parent_folder_id] = rbf.new_folder_id
                                    from [dbo].[folder] as f
                                        inner join @relations_between_folders as rbf
                                            on f.[parent_folder_id] = rbf.old_folder_id
                                    where f.SITE_ID = @destinationSiteId

                                    select old_folder_id as source_folder_id,
                                            new_folder_id as destination_folder_id
                                    from @relations_between_folders
                                    ";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyCommandSiteBind(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"        delete from [dbo].[VE_COMMAND_SITE_BIND]
                                     where site_id = @destinationSiteId

                                     insert into [dbo].[VE_COMMAND_SITE_BIND]
                                     SELECT command_id
                                            ,@destinationSiteId
                                          ,[ON]
                                      FROM [dbo].[VE_COMMAND_SITE_BIND] (nolock)
                                      where site_id = @sourceSiteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyStyleSiteBind(int sourceSiteId, int destinationSiteId, SqlConnection sqlConnection)
        {
            var query = @"        delete from [dbo].[VE_STYLE_SITE_BIND]
                                     where site_id = @destinationSiteId

                                     insert into [dbo].[VE_STYLE_SITE_BIND]
                                     SELECT style_id
                                            ,@destinationSiteId
                                            ,[ON]
                                      FROM [dbo].[VE_STYLE_SITE_BIND] (nolock)
                                      where site_id = @sourceSiteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Copy contents

        public static IEnumerable<DataRow> CopyContents(SqlConnection sqlConnection, int oldSiteId, int newSiteId, int startFrom, int endOn)
        {
            var excludeColumns = new List<string>();
            excludeColumns.Add("content_id");

            var changeValues = new Dictionary<string, string>();
            changeValues.Add("site_id", newSiteId.ToString());
            changeValues.Add("created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));
            changeValues.Add("modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));


            var addValuesForNotifications = new Dictionary<string, string>();


            var query = string.Format(@"	declare @todaysDate datetime = GETDATE()
                                    declare @isvirtual bit = 0
                                    declare @new_content_ids table (content_id int)
                                    -- copying contents

                                    ;with contents_with_row_number
                                    as
                                    (
                                        select ROW_NUMBER() over(order by content_id) as [row_number]
                                            , {0}
                                      from [dbo].[content] (nolock)
                                      where site_id = @oldsiteid and virtual_type = 0
                                    )
                                    insert into [dbo].[content] ({1})
                                    output inserted.CONTENT_ID
                                        into @new_content_ids
                                    select {0}
                                      from contents_with_row_number
                                      where row_number between @startFrom and @endOn

                                    select content_id from @new_content_ids",
                                                       GetColumnsForTable(sqlConnection, "content", excludeColumns, changeValues)
                                                       , GetColumnsForTable(sqlConnection, "content", excludeColumns));

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);
                cmd.Parameters.AddWithValue("@startFrom", startFrom);
                cmd.Parameters.AddWithValue("@endOn", endOn);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopyContentConstraints(SqlConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"
                                    declare @xmlprms xml = '{0}'

                                    declare @relations_between_constrains table(
                                        source_constraint_id int,
                                        destination_constraint_id int
                                    )

                                    ;with relationsBetweenContents as (
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    merge [dbo].[content_constraint]
                                    using (
                                    SELECT cc.constraint_id, rbc.destination_content_id
                                      FROM [dbo].[content_constraint] as cc (nolock)
                                        inner join relationsBetweenContents as rbc
                                            on rbc.source_content_id = cc.content_id
                                    )as src(constraint_id, content_id)
                                    on 0 = 1
                                    when not matched then
                                    insert(content_id)
                                    values(content_id)
                                    output src.constraint_id, inserted.constraint_id
                                        into @relations_between_constrains;

                                    select source_constraint_id, destination_constraint_id from @relations_between_constrains", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentNotifications(SqlConnection sqlConnection, string relationsBetweenContentsXml, string relationsBetweenStatusesXml, string relationsBetweenAttributesXml)
        {

            var changeValues = new Dictionary<string, string>();
            changeValues.Add("created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));
            changeValues.Add("modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));
            changeValues.Add("notify_on_status_type_id", "{rbs.destination_status_id}");
            changeValues.Add("email_attribute_id", "{rba.destination_attribute_id}");

            var query = string.Format(@"
                                    declare @xmlprmsStatuses xml = '{0}'

                                    declare @relationsBetweenStatuses table(
                                        source_status_id int,
                                        destination_status_id int
                                    )

                                    insert into @relationsBetweenStatuses
                                        select doc.col.value('./@sourceId', 'int') source_status_id
                                         ,doc.col.value('./@destinationId', 'int') destination_status_id
                                        from @xmlprmsStatuses.nodes('/items/item') doc(col)

                                    declare @xmlprmsAttributes xml = '{1}'

                                    declare @relationsBetweenAttributes table(
                                        source_attribute_id int,
                                        destination_attribute_id int
                                    )

                                    insert into @relationsBetweenAttributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                    declare @xmlprms xml = '{2}'

                                    ;with relationsBetweenContents as (
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[notifications] ({3})
                                    select {4}
                                    from [dbo].[notifications] as n
                                    inner join relationsBetweenContents as rbc on rbc.source_content_id = n.content_id
                                    left join @relationsBetweenStatuses as rbs on rbs.source_status_id = n.notify_on_status_type_id
                                    left join @relationsBetweenAttributes as rba on rba.source_attribute_id = n.email_attribute_id"
                                               , relationsBetweenStatusesXml
                                               , relationsBetweenAttributesXml
                                               , relationsBetweenContentsXml
                                               , GetColumnsForTable(sqlConnection, "notifications", new List<string> { "notification_id" })
                                               , GetColumnsForTable(sqlConnection, "notifications", new List<string> { "notification_id", "content_id" }, changeValues, new Dictionary<string, string> { { "rbc.destination_content_id", "" } }, "format_id"));

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyDynamicImageAttributes(SqlConnection sqlConnection, string relationsBetweenAttributesXml)
        {
            var query = string.Format(@"       declare @xmlprms xml = '{0}'

                                    ;with relationsBetweenAttributes as (
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[dynamic_image_attribute]
                                    select ra.destination_attribute_id
                                      ,[width]
                                      ,[height]
                                      ,[type]
                                      ,[quality]
                                      ,[max_size]
                                    from [dbo].[dynamic_image_attribute] as dia (nolock)
                                        inner join relationsBetweenAttributes as ra on dia.ATTRIBUTE_ID = ra.source_attribute_id", relationsBetweenAttributesXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

        }

        public static void UpdateAttributesOrder(SqlConnection sqlConnection, int destinationSiteId, string relationsBetweenAttributesXml, string newContentIds)
        {
            var forAttributesOfContents = string.Empty;
            if (!string.IsNullOrEmpty(newContentIds))
                forAttributesOfContents = string.Format("and ca.CONTENT_ID in ({0})", newContentIds);


            var query = string.Format(@"	    declare @xmlprms xml = '{0}'

                                    ;with relationsBetweenAttributes as (
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    update ca
                                    set ATTRIBUTE_ORDER = ca1.ATTRIBUTE_ORDER
                                    from [dbo].[content_attribute] as ca (nolock)
                                        inner join content as c (nolock)
                                            on ca.CONTENT_ID = c.CONTENT_ID
                                        inner join relationsBetweenAttributes as ra
                                            on ca.ATTRIBUTE_ID = ra.destination_attribute_id
                                        inner join CONTENT_ATTRIBUTE as ca1  (nolock)
                                            on ra.source_attribute_id = ca1.ATTRIBUTE_ID
                                    where c.SITE_ID = @newsiteid {1}", relationsBetweenAttributesXml, forAttributesOfContents);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentWorkflowBind(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"

                                    declare @xmlprms xml = '{0}'

                                    declare @relationsBetweenContents table (
                                        source_content_id int,
                                        destination_content_id int
                                    )
                                    insert into @relationsBetweenContents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)


                                    ;with relations_between_workflows
                                    as
                                    (
                                        SELECT w1.[WORKFLOW_ID] as old_workflow_id
                                                ,w2.WORKFLOW_ID as new_workflow_id
                                        FROM [dbo].[workflow] as w1 (nolock)
                                            inner join [dbo].[workflow] as w2 (nolock)
                                                on w1.WORKFLOW_NAME = w2.WORKFLOW_NAME and w2.SITE_ID = @newSiteId
                                        where w1.SITE_ID = @oldSiteId
                                    )
                                    insert into [dbo].[content_workflow_bind]
                                        select rbc.destination_content_id
                                                ,rbw.new_workflow_id
                                                ,[is_async]
                                        from [dbo].[content_workflow_bind] as cwb (nolock)
                                        inner join @relationsBetweenContents as rbc on rbc.source_content_id = cwb.content_id
                                        inner join relations_between_workflows as rbw on rbw.old_workflow_id = cwb.workflow_id
                                        where rbc.destination_content_id in (select destination_content_id from @relationsBetweenContents)"
                                                                                                                                        , relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Copy Virtual Contents

        public static void CopyUnionContents(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string newContentIds)
        {
            if (string.IsNullOrEmpty(newContentIds))
                newContentIds = "0";
            var query = string.Format(@"

                            declare @relscontents as table(
                                content_id_old int,
                                content_id_new int
                            )
                            declare @isVirtual bit = 0

                            insert into @relscontents
                                            select c.content_id as source_content_id, nc.content_id as destination_content_id
                                            from [dbo].[content] as c (nolock)
                                            inner join [dbo].[content] as nc (nolock)
                                                on nc.content_name = c.content_name and nc.site_id = @newsiteid
                                            where c.site_id = @oldsiteid

                            insert into union_contents
                            select nvc1.content_id_new, rc.content_id_new, rc1.content_id_new
                            from union_contents as uc (nolock)
                            inner join @relscontents as nvc1 on uc.virtual_content_id = nvc1.content_id_old
                            left join @relscontents as rc on uc.union_content_id = rc.content_id_old
                            left join @relscontents as rc1 on uc.master_content_id = rc1.content_id_old
                            where nvc1.content_id_new in ({0})", newContentIds);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateVirtualContentAttributes(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @"   ;with relationsBetweenContentLinks as (
                                select distinct oldvalues.link_id as oldvalue, newvalues.link_id as newvalue from (
                                    select attribute_name, link_id, c.content_name
                                      from [dbo].[content_attribute] as ca
                                      inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
                                      inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation'
                                      where c.site_id = @oldsiteid and link_id is not null) as oldvalues
                                    inner join (
                                        select attribute_name, link_id, c.content_name
                                          from [dbo].[content_attribute] as ca
                                          inner join content as c on c.content_id = ca.content_id and c.virtual_type = 0
                                          inner join attribute_type as at on at.attribute_type_id = ca.attribute_type_id and at.type_name = 'Relation'
                                          where c.site_id = @newsiteid and link_id is not null)
                                          as newvalues
                                          on newvalues.attribute_name = oldvalues.attribute_name and newvalues.content_name = oldvalues.content_name
                            )
                            update [dbo].[content_attribute]
                            set link_id = lc.newvalue,
                                default_value =lc1.newvalue
                            from  [dbo].[content_attribute] as ca (nolock)
                                inner join relationsBetweenContentLinks as lc
                                    on ca.link_id = CAST(lc.oldvalue as varchar)
                                inner join relationsBetweenContentLinks as lc1
                                    on ca.default_value = CAST(lc1.oldvalue as varchar)
                                inner join content as c (nolock)
                                    on ca.CONTENT_ID = c.CONTENT_ID and c.SITE_ID = @newsiteid";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsGroups(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @"delete from [dbo].[content_group]
                                where site_id = @newsiteid

                            insert into [dbo].[content_group]
                            select @newsiteid
                                  ,[name]
                              from [dbo].[content_group] (nolock)
                              where site_id = @oldsiteid";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentGroupIds(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @";with relations_between_groups as
                            (
                                select c.content_group_id as content_group_id_old, nc.content_group_id as content_group_id_new
                                from [dbo].[content_group] as c
                                inner join [dbo].[content_group] as nc on nc.name = c.name and nc.site_id = @newsiteid
                                where c.site_id = @oldsiteid
                            )
                            update [dbo].content
                            set content_group_id = rbg.content_group_id_new
                            from [dbo].[CONTENT] as c (nolock)
                                inner join relations_between_groups as rbg (nolock) on c.content_group_id = rbg.content_group_id_old
                            where site_id = @newsiteid";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyVirtualContents(SqlConnection sqlConnection, int siteId, int newSiteId)
        {
            var excludeColumns = new List<string>();
            excludeColumns.Add("content_id");

            var changeValues = new Dictionary<string, string>();
            changeValues.Add("site_id", newSiteId.ToString());
            changeValues.Add("created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));
            changeValues.Add("modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));

            var fieldsToAdd = new Dictionary<string, string>();
            fieldsToAdd.Add("virtual_join_primary_content_id_new", "rc.content_id_new");

            var query = string.Format(@"set nocount on;

                            declare @newvirtualcontents table(
                                content_id_old int,
                                content_id_new int,
                                virtual_type int,
                                sqlquery nvarchar(max),
                                altquery nvarchar(max)
                            )

                            declare @relscontents as table(
                                content_id_old int,
                                content_id_new int
                            )
                            declare @isVirtual bit = 0

                            insert into @relscontents
                                            select c.content_id as source_content_id, nc.content_id as destination_content_id
                                            from [dbo].[content] as c (nolock)
                                            inner join [dbo].[content] as nc (nolock)
                                                on nc.content_name = c.content_name and nc.site_id = @newsiteid
                                            where c.site_id = @oldsiteid

                            merge [dbo].[content]
                            using (
                                select {0}
                                  from [dbo].[content] as c (nolock)
                                  left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
                              where virtual_type != 0 and site_id = @oldsiteid)
                              as src({1})
                              on 0 = 1
                              when not matched then
                               insert ({2})
                               values ({3})
                               output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
                                into @newvirtualcontents;


                            select 	content_id_old
                                , content_id_new
                                , virtual_type
                                , sqlquery
                                , altquery
                            from @newvirtualcontents

                            ",
                                                       GetColumnsForTable(sqlConnection, "content", null, changeValues, fieldsToAdd, "virtual_join_primary_content_id")
                                                     , GetColumnsForTable(sqlConnection, "content", null, null, new Dictionary<string, string> { { "virtual_join_primary_content_id_new", "" } }, "virtual_join_primary_content_id")
                                                     , GetColumnsForTable(sqlConnection, "content", excludeColumns)
                                                     , GetColumnsForTable(sqlConnection, "content", new List<string> { "content_id", "virtual_join_primary_content_id" }, null, new Dictionary<string, string> { { "virtual_join_primary_content_id_new", "" } }, "is_shared"));


            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", siteId);
                cmd.Parameters.AddWithValue("@newsiteid", newSiteId);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }
        #endregion

        public static int GetSiteRealContentCount(int siteId, SqlConnection connection)
        {
            var query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id AND virtual_type = 0";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static int GetSiteContentLinkCount(int siteId, SqlConnection connection)
        {
            var query = "select count(link_id) from SITE_CONTENT_LINK where SITE_ID = @site_id";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static int GetSiteVirtualContentCount(SqlConnection connection, int siteId)
        {
            var query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id AND virtual_type != 0";

            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenContents(SqlConnection connection, int oldSiteId, int newSiteId)
        {
            var query = @"select c.content_id as content_id_old
                                    , nc.content_id as content_id_new
                             from [dbo].[content] as c
                             inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newsiteid
                             where c.site_id = @oldsiteid";
            using (var cmd = SqlCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@oldsiteid", oldSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", newSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int CopySiteTemplates(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, int templateNumber)
        {
            var query = @"
                            declare @now datetime = GETDATE()

                            if @templateNumber = 1
                            begin
                                delete from [dbo].[page_template]
                                where SITE_ID = @newSiteId
                            end


                            declare @new_templates table(new_template_id int)
                            ;with templates_with_row_number as
                            (
                                select ROW_NUMBER() over(order by page_template_id) as [row_number]
                                  ,[site_id]
                                  ,[template_name]
                                  ,[template_picture]
                                  ,[description]
                                  ,[created]
                                  ,[modified]
                                  ,[last_modified_by]
                                  ,[charset]
                                  ,[codepage]
                                  ,[locale]
                                  ,[template_body]
                                  ,[template_folder]
                                  ,[is_system]
                                  ,[net_template_name]
                                  ,[code_behind]
                                  ,[net_language_id]
                                  ,[show_filenames]
                                  ,[enable_viewstate]
                                  ,[for_mobile_devices]
                                  ,[preview_template_body]
                                  ,[preview_code_behind]
                                  ,[max_num_of_format_stored_versions]
                                  ,[custom_class_for_pages]
                                  ,[template_custom_class]
                                  ,[custom_class_for_generics]
                                  ,[custom_class_for_containers]
                                  ,[custom_class_for_forms]
                                  ,[assemble_in_live]
                                  ,[assemble_in_stage]
                                  ,[disable_databind]
                                  ,[using]
                                  ,[send_nocache_headers]
                            from [dbo].[page_template] as pt (nolock)
                            where site_id = @oldSiteId
                            )
                            insert into [dbo].[page_template](
                                  [site_id]
                                  ,[template_name]
                                  ,[template_picture]
                                  ,[description]
                                  ,[created]
                                  ,[modified]
                                  ,[last_modified_by]
                                  ,[charset]
                                  ,[codepage]
                                  ,[locale]
                                  ,[template_body]
                                  ,[template_folder]
                                  ,[is_system]
                                  ,[net_template_name]
                                  ,[code_behind]
                                  ,[net_language_id]
                                  ,[show_filenames]
                                  ,[enable_viewstate]
                                  ,[for_mobile_devices]
                                  ,[preview_template_body]
                                  ,[preview_code_behind]
                                  ,[max_num_of_format_stored_versions]
                                  ,[custom_class_for_pages]
                                  ,[template_custom_class]
                                  ,[custom_class_for_generics]
                                  ,[custom_class_for_containers]
                                  ,[custom_class_for_forms]
                                  ,[assemble_in_live]
                                  ,[assemble_in_stage]
                                  ,[disable_databind]
                                  ,[using]
                                  ,[send_nocache_headers]
                            )
                            output inserted.PAGE_TEMPLATE_ID
                                into @new_templates
                            select @newSiteId
                                  ,[template_name]
                                  ,[template_picture]
                                  ,[description]
                                  ,@now as created
                                  ,@now as modified
                                  ,[last_modified_by]
                                  ,[charset]
                                  ,[codepage]
                                  ,[locale]
                                  ,[template_body]
                                  ,[template_folder]
                                  ,[is_system]
                                  ,[net_template_name]
                                  ,[code_behind]
                                  ,[net_language_id]
                                  ,[show_filenames]
                                  ,[enable_viewstate]
                                  ,[for_mobile_devices]
                                  ,[preview_template_body]
                                  ,[preview_code_behind]
                                  ,[max_num_of_format_stored_versions]
                                  ,[custom_class_for_pages]
                                  ,[template_custom_class]
                                  ,[custom_class_for_generics]
                                  ,[custom_class_for_containers]
                                  ,[custom_class_for_forms]
                                  ,[assemble_in_live]
                                  ,[assemble_in_stage]
                                  ,[disable_databind]
                                  ,[using]
                                  ,[send_nocache_headers]
                            from templates_with_row_number as pt
                            where row_number = @templateNumber


                            select new_template_id from @new_templates";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.Parameters.AddWithValue("@templateNumber", templateNumber);
                var templateIdNew = (int)cmd.ExecuteScalar();
                return templateIdNew;
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenTemplates(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, int templateIdNew)
        {
            var query = @"select pto.page_template_id as source_template_id
                                    ,ptn.page_template_id as destination_template_id
                            from page_template as pto (nolock)
                            inner join page_template as ptn (nolock)
                                  on pto.template_name = ptn.template_name and ptn.site_id = @newSiteId
                            where pto.site_id = @oldSiteId and ptn.PAGE_TEMPLATE_ID = @templateIdNew";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.Parameters.AddWithValue("@templateIdNew", templateIdNew);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenPages(SqlConnection sqlConnection, string relationsBetweenTemplates)
        {
            var query = string.Format(@"	declare @xmlprms xml = '{0}'

                                ;with relations_between_templates as (
                                        select doc.col.value('./@sourceId', 'int') source_page_template_id
                                         ,doc.col.value('./@destinationId', 'int') destination_page_template_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                )
                                select po.PAGE_ID as source_page_id
                                        ,pn.PAGE_ID as destination_page_id
                                from [dbo].[page] as po (nolock)
                                inner join relations_between_templates as pt
                                    on po.PAGE_TEMPLATE_ID = pt.source_page_template_id
                                inner join PAGE as pn (nolock)
                                    on po.PAGE_NAME = pn.PAGE_NAME
                                inner join relations_between_templates as pt1
                                    on pn.PAGE_TEMPLATE_ID = pt1.destination_page_template_id", relationsBetweenTemplates);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplatePages(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenTemplates)
        {
            var query = string.Format(@"
                            declare @now datetime = GETDATE()
                            declare @new_pages_added table(page_id int, page_template_id int)

                            declare @xmlprms xml = '{0}'

                            ;with relations_between_templates as (
                                    select doc.col.value('./@sourceId', 'int') source_page_template_id
                                     ,doc.col.value('./@destinationId', 'int') destination_page_template_id
                                    from @xmlprms.nodes('/items/item') doc(col)
                            )
                            insert into dbo.[page] (
                                [page_template_id]
                                  ,[page_name]
                                  ,[page_filename]
                                  ,[proxy_cache]
                                  ,[cache_hours]
                                  ,[charset]
                                  ,[codepage]
                                  ,[locale]
                                  ,[description]
                                  ,[reassemble]
                                  ,[created]
                                  ,[modified]
                                  ,[last_modified_by]
                                  ,[assembled]
                                  ,[last_assembled_by]
                                  ,[generate_trace]
                                  ,[page_folder]
                                  ,[enable_viewstate]
                                  ,[disable_browse_server]
                                  ,[set_last_modified_header]
                                  ,[page_custom_class]
                                  ,[send_nocache_headers]
                                  ,[permanent_lock])
                            output inserted.PAGE_ID, inserted.PAGE_TEMPLATE_ID
                                into @new_pages_added
                            select
                                  rbt.destination_page_template_id
                                  ,[page_name]
                                  ,[page_filename]
                                  ,[proxy_cache]
                                  ,[cache_hours]
                                  ,[charset]
                                  ,[codepage]
                                  ,[locale]
                                  ,[description]
                                  ,[reassemble]
                                  ,@now as created
                                  ,@now as modified
                                  ,[last_modified_by]
                                  ,[assembled]
                                  ,[last_assembled_by]
                                  ,[generate_trace]
                                  ,[page_folder]
                                  ,[enable_viewstate]
                                  ,[disable_browse_server]
                                  ,[set_last_modified_header]
                                  ,[page_custom_class]
                                  ,[send_nocache_headers]
                                  ,[permanent_lock]
                            from [dbo].[page] as p (nolock)
                            inner join relations_between_templates as rbt on p.page_template_id = rbt.source_page_template_id


                            select page_id, page_template_id from @new_pages_added", relationsBetweenTemplates);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplateObjects(SqlConnection sqlConnection, string relationsBetweenTemplates, string relationsBetweenPages)
        {
            var query = string.Format(@"
                            declare @now datetime = GETDATE()
                            declare @xmlprmsTemplates xml = '{0}'

                            declare @relations_between_templates table (
                                source_template_id int,
                                destination_template_id int
                            )
                            insert into @relations_between_templates
                                    select doc.col.value('./@sourceId', 'int') source_template_id
                                     ,doc.col.value('./@destinationId', 'int') destination_template_id
                                    from @xmlprmsTemplates.nodes('/items/item') doc(col)

                            declare @xmlprmsPages xml = '{1}'

                            declare @relations_between_objects table(
                                source_object_id int,
                                destination_object_id int
                            )

                            ;with relations_between_pages as (
                                    select doc.col.value('./@sourceId', 'int') source_page_id
                                     ,doc.col.value('./@destinationId', 'int') destination_page_id
                                    from @xmlprmsPages.nodes('/items/item') doc(col)
                            )
                            merge into [dbo].[object]
                            using(
                            select [object_id]
                                  ,[parent_object_id]
                                  ,pt.destination_template_id as [page_template_id]
                                  ,rbg.destination_page_id as [page_id]
                                  ,[object_name]
                                  ,[object_format_id]
                                  ,o.[description]
                                  ,[object_type_id]
                                  ,[use_default_values]
                                  ,o.[last_modified_by]
                                  ,[allow_stage_edit]
                                  ,[global]
                                  ,[net_object_name]
                                  ,o.[locked_by]
                                  ,o.[enable_viewstate]
                                  ,[control_custom_class]
                                  ,o.[disable_databind]
                                  ,o.[permanent_lock]
                            from dbo.[OBJECT] as o (nolock)
                                inner join @relations_between_templates as pt
                                    on o.PAGE_TEMPLATE_ID = pt.source_template_id
                                left join relations_between_pages as rbg
                                    on o.page_id = rbg.source_page_id
                            )as src ([object_id]
                                  ,[parent_object_id]
                                  ,[page_template_id]
                                  ,[page_id]
                                  ,[object_name]
                                  ,[object_format_id]
                                  ,[description]
                                  ,[object_type_id]
                                  ,[use_default_values]
                                  ,[last_modified_by]
                                  ,[allow_stage_edit]
                                  ,[global]
                                  ,[net_object_name]
                                  ,[locked_by]
                                  ,[enable_viewstate]
                                  ,[control_custom_class]
                                  ,[disable_databind]
                                  ,[permanent_lock])
                            on 0 = 1
                            when not matched then
                            insert ([parent_object_id]
                                  ,[page_template_id]
                                  ,[page_id]
                                  ,[object_name]
                                  ,[object_format_id]
                                  ,[description]
                                  ,[object_type_id]
                                  ,[use_default_values]
                                  ,[last_modified_by]
                                  ,[allow_stage_edit]
                                  ,[global]
                                  ,[net_object_name]
                                  ,[locked_by]
                                  ,[enable_viewstate]
                                  ,[control_custom_class]
                                  ,[disable_databind]
                                  ,[permanent_lock])
                            values ([parent_object_id]
                                  ,[page_template_id]
                                  ,[page_id]
                                  ,[object_name]
                                  ,[object_format_id]
                                  ,[description]
                                  ,[object_type_id]
                                  ,[use_default_values]
                                  ,[last_modified_by]
                                  ,[allow_stage_edit]
                                  ,[global]
                                  ,[net_object_name]
                                  ,[locked_by]
                                  ,[enable_viewstate]
                                  ,[control_custom_class]
                                  ,[disable_databind]
                                  ,[permanent_lock])
                            output src.[object_id], inserted.[object_id]
                                into @relations_between_objects;

                        select source_object_id, destination_object_id from @relations_between_objects", relationsBetweenTemplates, relationsBetweenPages);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplateObjectFormats(SqlConnection sqlConnection, string relationsBetweenObjects)
        {
            var query = string.Format(@"

                            declare @relations_between_object_formats table(
                                source_object_format_id int,
                                destination_object_format_id int
                            )

                            declare @xmlprmsObjects xml = '{0}'

                            ;with relations_between_objects as (
                                    select doc.col.value('./@sourceId', 'int') source_object_id
                                     ,doc.col.value('./@destinationId', 'int') destination_object_id
                                    from @xmlprmsObjects.nodes('/items/item') doc(col)
                            )
                            merge [dbo].[OBJECT_FORMAT]
                            using (
                                    select
                                    [object_format_id]
                                  ,rbo.destination_object_id as [object_id]
                                  ,[format_name]
                                  ,[description]
                                  ,[last_modified_by]
                                  ,[format_body]
                                  ,[net_language_id]
                                  ,[net_format_name]
                                  ,[code_behind]
                                  ,[assemble_notification_in_live]
                                  ,[assemble_notification_in_stage]
                                  ,[assemble_preview_in_live]
                                  ,[assemble_preview_in_stage]
                                  ,[tag_name]
                                  ,[permanent_lock]
                            from [dbo].[OBJECT_FORMAT] as oft (nolock)
                            inner join relations_between_objects as rbo
                                on oft.[OBJECT_ID] = rbo.source_object_id
                            ) as src([object_format_id]
                                    ,[object_id]
                                  ,[format_name]
                                  ,[description]
                                  ,[last_modified_by]
                                  ,[format_body]
                                  ,[net_language_id]
                                  ,[net_format_name]
                                  ,[code_behind]
                                  ,[assemble_notification_in_live]
                                  ,[assemble_notification_in_stage]
                                  ,[assemble_preview_in_live]
                                  ,[assemble_preview_in_stage]
                                  ,[tag_name]
                                  ,[permanent_lock])
                            on 0 = 1
                            when not matched then
                            insert ([object_id]
                                  ,[format_name]
                                  ,[description]
                                  ,[last_modified_by]
                                  ,[format_body]
                                  ,[net_language_id]
                                  ,[net_format_name]
                                  ,[code_behind]
                                  ,[assemble_notification_in_live]
                                  ,[assemble_notification_in_stage]
                                  ,[assemble_preview_in_live]
                                  ,[assemble_preview_in_stage]
                                  ,[tag_name]
                                  ,[permanent_lock])
                            values ([object_id]
                                  ,[format_name]
                                  ,[description]
                                  ,[last_modified_by]
                                  ,[format_body]
                                  ,[net_language_id]
                                  ,[net_format_name]
                                  ,[code_behind]
                                  ,[assemble_notification_in_live]
                                  ,[assemble_notification_in_stage]
                                  ,[assemble_preview_in_live]
                                  ,[assemble_preview_in_stage]
                                  ,[tag_name]
                                  ,[permanent_lock])
                            output src.[object_format_id], inserted.[object_format_id]
                                into @relations_between_object_formats;

                            select source_object_format_id, destination_object_format_id
                            from @relations_between_object_formats", relationsBetweenObjects);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopySiteUpdateObjects(SqlConnection sqlConnection, string relationsBetweenObjectFormats, string relationsBetweenObjects)
        {
            var query = string.Format(@"
                                declare @xmlprmsObjectFormats xml = '{0}'

                                declare @relations_between_object_formats table(
                                    source_object_format_id int,
                                    destination_object_format_id int
                                )

                                insert into @relations_between_object_formats
                                        select doc.col.value('./@sourceId', 'int') source_object_format_id
                                         ,doc.col.value('./@destinationId', 'int') destination_object_format_id
                                        from @xmlprmsObjectFormats.nodes('/items/item') doc(col)

                                declare @xmlprmsObjects xml = '{1}'

                                ;with relations_between_objects as (
                                        select doc.col.value('./@sourceId', 'int') source_object_id
                                         ,doc.col.value('./@destinationId', 'int') destination_object_id
                                        from @xmlprmsObjects.nodes('/items/item') doc(col)
                                )
                                update [dbo].[object]
                                set OBJECT_FORMAT_ID = rbof.destination_object_format_id
                                from [dbo].[object] as o (nolock)
                                inner join @relations_between_object_formats as rbof
                                    on o.OBJECT_FORMAT_ID = rbof.source_object_format_id
                                inner join relations_between_objects as rbo
                                    on o.OBJECT_ID = rbo.destination_object_id", relationsBetweenObjectFormats, relationsBetweenObjects);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteObjectValues(SqlConnection sqlConnection, string relationsBetweenObjects)
        {
            var query = string.Format(@"

                                    declare @xmlprmsObjects xml = '{0}'

                                    ;with relations_between_objects as (
                                            select doc.col.value('./@sourceId', 'int') source_object_id
                                             ,doc.col.value('./@destinationId', 'int') destination_object_id
                                            from @xmlprmsObjects.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[OBJECT_VALUES]
                                    select rbo.destination_object_id
                                      ,[variable_name]
                                      ,[variable_value]
                                    from [dbo].[object_values] as ov
                                        inner join relations_between_objects as rbo
                                            on ov.OBJECT_ID = rbo.source_object_id", relationsBetweenObjects);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteContainers(SqlConnection sqlConnection, string relationsBetweenObjects, string relationsBetweenContents)
        {
            var query = string.Format(@"
                                declare @xmlprmsContents xml = '{0}'
                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )

                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)

                                declare @xmlprmsObjects xml = '{1}'

                                ;with relations_between_objects as (
                                        select doc.col.value('./@sourceId', 'int') source_object_id
                                         ,doc.col.value('./@destinationId', 'int') destination_object_id
                                        from @xmlprmsObjects.nodes('/items/item') doc(col)
                                )
                                insert into [dbo].[container]
                                (
                                  [object_id]
                                  ,[content_id]
                                  ,[allow_order_dynamic]
                                  ,[order_static]
                                  ,[order_dynamic]
                                  ,[filter_value]
                                  ,[select_start]
                                  ,[select_total]
                                  ,[schedule_dependence]
                                  ,[rotate_content]
                                  ,[apply_security]
                                  ,[show_archived]
                                  ,[cursor_type]
                                  ,[cursor_location]
                                  ,[duration]
                                  ,[enable_cache_invalidation]
                                  ,[dynamic_content_variable]
                                  ,[start_level]
                                  ,[end_level]
                                  ,[use_level_filtration]
                                  ,[return_last_modified]
                                )
                            select rbo.destination_object_id
                              ,rbc.destination_content_id
                              ,[allow_order_dynamic]
                              ,[order_static]
                              ,[order_dynamic]
                              ,[filter_value]
                              ,[select_start]
                              ,[select_total]
                              ,[schedule_dependence]
                              ,[rotate_content]
                              ,[apply_security]
                              ,[show_archived]
                              ,[cursor_type]
                              ,[cursor_location]
                              ,[duration]
                              ,[enable_cache_invalidation]
                              ,[dynamic_content_variable]
                              ,[start_level]
                              ,[end_level]
                              ,[use_level_filtration]
                              ,[return_last_modified]
                            from [dbo].[container] as c (nolock)
                                inner join relations_between_objects as rbo
                                    on c.[OBJECT_ID] = rbo.source_object_id
                                inner join @relations_between_contents as rbc
                                    on c.CONTENT_ID = rbc.source_content_id", relationsBetweenContents, relationsBetweenObjects);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteUpdateNotifications(SqlConnection sqlConnection, string relationsBetweenObjectFormats, string relationsBetweenContents)
        {
            var query = string.Format(@"
                                declare @xmlprmsContents xml = '{0}'
                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )

                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)

                                declare @xmlprmsObjectFormats xml = '{1}'

                                ;with relations_between_object_formats as (
                                        select doc.col.value('./@sourceId', 'int') source_object_format_id
                                         ,doc.col.value('./@destinationId', 'int') destination_object_format_id
                                        from @xmlprmsObjectFormats.nodes('/items/item') doc(col)
                                )
                                update [dbo].[notifications]
                                set [format_id] = rbof.destination_object_format_id
                                from [dbo].[notifications] as n
                                    inner join relations_between_object_formats as rbof
                                        on rbof.source_object_format_id = n.format_id
                                where n.content_id in (select destination_content_id from @relations_between_contents)", relationsBetweenContents, relationsBetweenObjectFormats);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetTemplatesElementsCountOnSite(SqlConnection sqlConnection, int siteId)
        {
            var query = @"SELECT COUNT(*)
                              from [dbo].[PAGE_TEMPLATE]
                                where SITE_ID = @siteId";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@siteId", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }

        }

        public static void UpdateVirtualContent(SqlConnection sqlConnection, string newSqlQuery, int newContentId)
        {
            var query = @"update [dbo].[content]
                                set [query] = @newSqlQuery
                            where [content_id] = @contentId";
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@newSqlQuery", newSqlQuery);
                cmd.Parameters.AddWithValue("@contentId", newContentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsAttributes(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string newContentIds, bool isContentsVirtual)
        {
            if (string.IsNullOrEmpty(newContentIds))
                return;

            var excludeColumns = new List<string>();
            excludeColumns.Add("attribute_id");

            var changeValues = new Dictionary<string, string>();
            changeValues.Add("created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));
            changeValues.Add("modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")));

            var query = string.Format(@"	set nocount on;
                if @new_content_ids is not null begin
                    declare @content_ids table
                    (
                        id numeric primary key
                    )

                    insert into @content_ids(id)
                    SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ',')

                    delete from content_attribute where content_id in (select id from @content_ids)

                    insert into content_attribute ({0})
                    select {1}
                    from [dbo].[content_attribute] as ca (nolock)
                    inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock)
                            on nc.content_name = c.content_name and nc.site_id = @destination_site_id
                        where c.site_id = @source_site_id and {2}
                    ) as rbc on ca.CONTENT_ID = rbc.source_content_id and rbc.destination_content_id in (select id from @content_ids)
                end", GetColumnsForTable(sqlConnection, "content_attribute", excludeColumns)
                  , GetColumnsForTable(sqlConnection, "content_attribute", new List<string> { "attribute_id", "content_id" }, changeValues, new Dictionary<string, string> { { "rbc.destination_content_id", "" } }, "ATTRIBUTE_NAME")
                  , isContentsVirtual ? "c.virtual_type != 0" : "c.virtual_type = 0");
            using (var cmd = new SqlCommand(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@source_site_id", sourceSiteId);
                cmd.Parameters.AddWithValue("@destination_site_id", destinationSiteId);
                cmd.Parameters.AddWithValue("@new_content_ids", newContentIds);
                cmd.Parameters.AddWithValue("@is_contents_virtual", isContentsVirtual);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyStyleFieldBind(SqlConnection sqlConnection, string relationsBetweenAttributes)
        {
            var query = string.Format(@"
                                        declare @xmlprmsAttributes xml = '{0}'
                                        ;with relations_between_attributes as (
                                            select doc.col.value('./@sourceId', 'int') source_attribute_id
                                            ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                            from @xmlprmsAttributes.nodes('/items/item') doc(col)
                                        )
                                                insert into [dbo].[ve_style_field_bind]
                                                select [style_id]
                                                      ,ra.destination_attribute_id
                                                      ,[on]
                                                from [dbo].[ve_style_field_bind] as csfb (nolock)
                                                inner join relations_between_attributes as ra
                                                      on ra.source_attribute_id = csfb.[field_id]", relationsBetweenAttributes);
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyCommandFieldBind(SqlConnection sqlConnection, string relationsBetweenAttributes)
        {
            var query = string.Format(@"
                                        declare @xmlprmsAttributes xml = '{0}'
                                        ;with relations_between_attributes as (
                                            select doc.col.value('./@sourceId', 'int') source_attribute_id
                                            ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                            from @xmlprmsAttributes.nodes('/items/item') doc(col)
                                        )
                                        insert into [dbo].[ve_command_field_bind]
                                        select [command_id]
                                                      ,ra.destination_attribute_id
                                                      ,[on]
                                        from [dbo].[ve_command_field_bind] as csfb (nolock)
                                        inner join relations_between_attributes as ra
                                            on ra.source_attribute_id = csfb.[field_id]", relationsBetweenAttributes);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentConstrainRules(SqlConnection sqlConnection, string relationsBetweenConstraints, string relationsBetweenAttributes)
        {
            var query = string.Format(@"

                                            declare @xmlprmsConstraints xml = '{0}'

                                            declare @relations_between_constraints table (
                                                source_constraint_id int,
                                                destination_constraint_id int
                                            )
                                            insert into @relations_between_constraints
                                                select doc.col.value('./@sourceId', 'int') source_constraint_id
                                                 ,doc.col.value('./@destinationId', 'int') destination_constraint_id
                                            from @xmlprmsConstraints.nodes('/items/item') doc(col)

                                            declare @xmlprmsAttributes xml = '{1}'

                                            ;with relations_between_attributes as (
                                                select doc.col.value('./@sourceId', 'int') source_attribute_id
                                                ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                                from @xmlprmsAttributes.nodes('/items/item') doc(col)
                                            )
                                                insert into [dbo].content_constraint_rule
                                                SELECT rbc.destination_constraint_id
                                                        ,ra.destination_attribute_id
                                                  FROM [dbo].[content_constraint_rule] as ccr
                                                  inner join relations_between_attributes as ra on ra.source_attribute_id = ccr.attribute_id
                                                  inner join @relations_between_constraints as rbc on rbc.source_constraint_id = ccr.constraint_id",
                                                                                                                                                          relationsBetweenConstraints,
                                                                                                                                                          relationsBetweenAttributes);

            using (var cmd = new SqlCommand(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateAttributes(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenAttributesXml, string contentIds)
        {
            var inContents = string.Empty;
            if (!string.IsNullOrEmpty(contentIds))
            {
                inContents = string.Format("and c.content_id in ({0})", contentIds);
            }
            var query = string.Format(@"
                                    declare @xmlprmsAttributes xml = '{0}'
                                        ;with relsattrs as (
                                            select doc.col.value('./@sourceId', 'int') attr_old
                                            ,doc.col.value('./@destinationId', 'int') attr_new
                                            from @xmlprmsAttributes.nodes('/items/item') doc(col)
                                        )
                                    update [dbo].[content_attribute]
                                    set		[related_attribute_id] = rai.attr_new
                                          ,[related_image_attribute_id]= ria.attr_new
                                          ,[persistent_attr_id]= pai.attr_new
                                          ,[join_attr_id]= jai.attr_new
                                          ,[back_related_attribute_id]= bra.attr_new
                                          ,[classifier_attribute_id]= cai.attr_new
                                          ,[tree_order_field] = tof.attr_new
                                          ,[PARENT_ATTRIBUTE_ID] = paid.attr_new
                                    from [dbo].[content_attribute] as ca (nolock)
                                    left join relsattrs as rai on rai.attr_old = ca.related_attribute_id
                                    left join relsattrs as ria on ria.attr_old = ca.related_image_attribute_id
                                    left join relsattrs as pai on pai.attr_old = ca.persistent_attr_id
                                    left join relsattrs as jai on jai.attr_old = ca.join_attr_id
                                    left join relsattrs as bra on bra.attr_old = ca.back_related_attribute_id
                                    left join relsattrs as cai on cai.attr_old = ca.classifier_attribute_id
                                    left join relsattrs as tof on tof.attr_old = ca.tree_order_field
                                    left join relsattrs as paid on paid.attr_old = ca.PARENT_ATTRIBUTE_ID
                                    inner join [dbo].[content] as c on ca.content_id = c.content_id
                                    where c.site_id = @destination_site_id {1}", relationsBetweenAttributesXml, inContents);

            using (var cmd = new SqlCommand(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destination_site_id", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateAttributeLinkIdAndDefaultValue(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenLinksXml)
        {
            var query = string.Format(@"

                                    declare @xmlprmsLinks xml = '{0}'

                                    declare @relationsBetweenLinks table (
                                        source_link_id int,
                                        destination_link_id int
                                    )
                                    insert into @relationsBetweenLinks
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                    update content_attribute
                                    set link_id = rc.destination_link_id,
                                    default_value = rc.destination_link_id
                                    from content_attribute as ca (nolock)
                                    inner join content as c on ca.content_id = c.content_id
                                    inner join @relationsBetweenLinks as rc on rc.source_link_id = ca.link_id
                                    where c.site_id = @newSiteId
                                    ", relationsBetweenLinksXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentsParentContentId(SqlConnection sqlConnection, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"	declare @xmlprmsContents xml = '{0}'

                                ;with relationsBetweenContents as (
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                     ,doc.col.value('./@destinationId', 'int') destination_content_id
                                    from @xmlprmsContents.nodes('/items/item') doc(col)
                                )
                                update [dbo].[content]
                                set PARENT_CONTENT_ID = rbc.destination_content_id
                                from [dbo].[content] as c
                                inner join relationsBetweenContents as rbc
                                    on c.PARENT_CONTENT_ID = rbc.source_content_id
                                where c.SITE_ID = @destinationSiteId", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentAccess(SqlConnection sqlConnection, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"

                                delete FROM [dbo].[CONTENT_ACCESS]
                                where CONTENT_ID in (
                                    select c.CONTENT_ID from content as c (nolock)
                                    inner join [dbo].[content] as c1 (nolock) on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @destinationSiteId
                                )

                                declare @now datetime = GETDATE()

                                declare @xmlprmsContents xml = '{0}'

                                ;with relations_between_contents as (
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                     ,doc.col.value('./@destinationId', 'int') destination_content_id
                                    from @xmlprmsContents.nodes('/items/item') doc(col)
                                )
                                insert into [CONTENT_ACCESS](
                                    [content_id]
                                      ,[user_id]
                                      ,[group_id]
                                      ,[permission_level_id]
                                      ,[created]
                                      ,[modified]
                                      ,[last_modified_by]
                                      ,[propagate_to_items]
                                )
                                select rbc.destination_content_id
                                      ,[user_id]
                                      ,[group_id]
                                      ,[permission_level_id]
                                      ,@now
                                      ,@now
                                      ,[last_modified_by]
                                      ,[propagate_to_items]
                                from [dbo].[content_access] as ca (nolock)
                                    inner join relations_between_contents as rbc on ca.CONTENT_ID = rbc.source_content_id", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContainerStatuses(SqlConnection sqlConnection, string relBetweenStatuses, string relBetweenObjects)
        {
            var query = string.Format(@"

                                    declare @xmlprmsObjects xml = '{0}'

                                    declare @relations_between_objects table(
                                        source_object_id int,
                                        destination_object_id int
                                    )

                                    insert into @relations_between_objects
                                        select doc.col.value('./@sourceId', 'int') source_object_id
                                        ,doc.col.value('./@destinationId', 'int') destination_object_id
                                        from @xmlprmsObjects.nodes('/items/item') doc(col)

                                    declare @xmlprmsStatuses xml = '{1}'
                                    ;with relations_between_statuses as (
                                        select doc.col.value('./@sourceId', 'int') source_status_id
                                        ,doc.col.value('./@destinationId', 'int') destination_status_id
                                        from @xmlprmsStatuses.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[container_statuses]
                                    select rbo.destination_object_id
                                          ,rbs.destination_status_id
                                      from [dbo].[container_statuses] as cs (nolock)
                                        inner join @relations_between_objects as rbo on rbo.source_object_id = cs.object_id
                                        inner join relations_between_statuses as rbs on rbs.source_status_id = cs.status_type_id
                                    ", relBetweenObjects, relBetweenStatuses);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsCustomActions(SqlConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"
                                declare @xmlprmsContents xml = '{0}'

                                ;with relations_between_contents as (
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                     ,doc.col.value('./@destinationId', 'int') destination_content_id
                                    from @xmlprmsContents.nodes('/items/item') doc(col)
                                )
                                insert into [dbo].[ACTION_CONTENT_BIND]
                                select [custom_action_id]
                                      ,rbc.destination_content_id
                                  from [dbo].[ACTION_CONTENT_BIND] as acb (nolock)
                                    inner join relations_between_contents as rbc
                                        on rbc.source_content_id = acb.CONTENT_ID", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentFolders(SqlConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"

                                declare @now datetime = GETDATE()

                                declare @xmlprmsContents xml = '{0}'

                                ;with relations_between_contents as (
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                     ,doc.col.value('./@destinationId', 'int') destination_content_id
                                    from @xmlprmsContents.nodes('/items/item') doc(col)
                                )
                                insert into [dbo].[content_folder](
                                [content_id]
                                  ,[parent_folder_id]
                                  ,[name]
                                  ,[description]
                                  ,[filter]
                                  ,[path]
                                  ,[created]
                                  ,[modified]
                                  ,[last_modified_by]
                                )
                                select rbc.destination_content_id
                                      ,[parent_folder_id]
                                      ,[name]
                                      ,[description]
                                      ,[filter]
                                      ,[path]
                                      ,@now
                                      ,@now
                                      ,[last_modified_by]
                                  from [dbo].[content_folder] as cf
                                    inner join relations_between_contents as rbc
                                        on rbc.source_content_id = cf.content_id", relationsBetweenContentsXml);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentFolders(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = string.Format(@"
                                declare @xmlprmsContents xml = '{0}'
                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )
                                insert into @relations_between_contents
                                    select doc.col.value('./@sourceId', 'int') source_content_id
                                     ,doc.col.value('./@destinationId', 'int') destination_content_id
                                    from @xmlprmsContents.nodes('/items/item') doc(col)


                                ;with relations_between_folders as (
                                  SELECT cf.folder_id as source_folder_id, cf1.folder_id as destination_folder_id
                                  FROM [dbo].[content_FOLDER] as cf (nolock)
                                  inner join [dbo].[content] as c (nolock) on c.CONTENT_ID = cf.content_ID and c.SITE_ID = @sourceSiteId
                                  inner join @relations_between_contents as rbc on rbc.source_content_id = cf.content_id
                                  inner join [dbo].[content_FOLDER] as cf1 (nolock) on cf1.PATH = cf.PATH and cf1.content_id = rbc.destination_content_id
                                  inner join [dbo].[content] as c1 (nolock) on c1.CONTENT_ID = cf1.content_ID and c1.SITE_ID = @destinationSiteId
                                )
                                update [dbo].[content_folder]
                                set parent_folder_id = rbf.destination_folder_id
                                from [dbo].[content_folder] as cf (nolock)
                                    inner join relations_between_folders as rbf
                                        on rbf.source_folder_id = cf.parent_folder_id
                                where cf.content_id in (select destination_content_id from @relations_between_contents)", relationsBetweenContentsXml);


            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyContentLinks(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            var query = @"	set nocount on;

                                --copying links between contents
                                declare @relations_between_content_links table(
                                    oldlink int,
                                    newlink int
                                )

                                declare @relations_between_contents table(
                                    source_content_id int,
                                    destination_content_id int
                                )
                                insert into @relations_between_contents
                                            select c.content_id as source_content_id, nc.content_id as destination_content_id
                                            from [dbo].[content] as c (nolock)
                                            inner join [dbo].[content] as nc (nolock)
                                                on nc.content_name = c.content_name and nc.site_id = @newSiteId
                                            where c.site_id = @oldSiteId

                                merge [dbo].content_to_content as t
                                using(
                                select cc.[link_id]
                                      ,rbc.destination_content_id
                                      ,rbc1.destination_content_id
                                      ,cc.[map_as_class]
                                      ,[net_link_name]
                                      ,[net_plural_link_name]
                                      ,[symmetric]
                                    from [dbo].content_to_content as cc (nolock)
                                    inner join [dbo].content as c (nolock) on c.content_id = l_content_id
                                    inner join @relations_between_contents as rbc on cc.l_content_id = rbc.source_content_id
                                    inner join @relations_between_contents as rbc1 on cc.r_content_id = rbc1.source_content_id
                                    where c.site_id = @oldsiteid
                                )as src([link_id],[l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
                                on 0 = 1
                                when not matched then
                                   insert ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
                                   values ([l_content_id],[r_content_id],[map_as_class],[net_link_name],[net_plural_link_name],[symmetric])
                                   output src.[link_id], inserted.[link_id]
                                    into @relations_between_content_links;

                                select oldlink, newlink from @relations_between_content_links
                                ";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenAttributes(SqlConnection sqlConnection, int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
        {
            var sb = new StringBuilder();

            var virtualType = string.Empty;
            if (forVirtualContents.HasValue)
            {
                virtualType = forVirtualContents.Value ? "and c.virtual_type != 0" : "and c.virtual_type = 0";
            }

            sb.AppendFormat(@"select ca.attribute_id as source_attribute_id
                                                     ,ca1.attribute_id as destination_attribute_id
                                             from [dbo].content_attribute as ca
                                                inner join (
                                                    select c.content_id as source_content_id, nc.content_id as destination_content_id
                                                    from [dbo].[content] as c (nolock)
                                                    inner join [dbo].[content] as nc (nolock)
                                                        on nc.content_name = c.content_name and nc.site_id = {1}
                                                    where c.site_id = {0} {2}
                                                ) as rbc
                                                    on ca.content_id = rbc.source_content_id
                                                left join [dbo].content_attribute as ca1
                                                    on ca.attribute_name = ca1.attribute_name and ca1.content_id = rbc.destination_content_id",
                                                                                                                              sourceSiteId,
                                                                                                                              destinationSiteId,
                                                                                                                              virtualType);

            if (!string.IsNullOrEmpty(contentIds))
            {
                if (byNewContents)
                    sb.AppendFormat(" where rbc.destination_content_id in ({0})", contentIds);
                else
                    sb.AppendFormat(" where rbc.source_content_id in ({0})", contentIds);
            }

            using (var cmd = SqlCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int GetArticlesCountToCopy(SqlConnection sqlConnection, int noMoreThanNArticles, int siteId)
        {
            var query = @"declare @articlesCount int
                                    set @articlesCount = (select SUM(countArticles) from (
                                                                        select COUNT(*) as countArticles, ci.content_id as content_id from content_item as ci
                                                                        inner join content as c on c.CONTENT_ID = ci.CONTENT_ID and c.virtual_type = 0
                                                                        where c.SITE_ID = @siteId
                                                                        group by ci.content_id
                                                                    ) as t
                                                                    where t.countArticles <= @noMoreThanNArticles)

                                    if @articlesCount is null begin
                                        select 0
                                    end
                                    else
                                    begin
                                        select @articlesCount
                                    end";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@noMoreThanNArticles", noMoreThanNArticles);
                cmd.Parameters.AddWithValue("@siteId", siteId);

                return (int)cmd.ExecuteScalar();
            }
        }

        public static string GetContentIdsToCopy(SqlConnection sqlConnection, int noMoreThanNArticles, int sourceSiteId)
        {
            var query = @"   declare @ids varchar(max)
                                select @ids = COALESCE(@ids + ', ', '') + CAST(content_id as nvarchar) from (
                                    select COUNT(*) as countArticles, ci.content_id as content_id from content_item as ci
                                    inner join content as c on c.CONTENT_ID = ci.CONTENT_ID and c.virtual_type = 0
                                    where c.SITE_ID = @siteId
                                    group by ci.content_id
                                ) as t
                                where t.countArticles <= @noMoreThanNArticles
                                if @ids is null begin
                                    select '0'
                                end
                                else begin
                                    select @ids
                                end";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@noMoreThanNArticles", noMoreThanNArticles);
                cmd.Parameters.AddWithValue("@siteId", sourceSiteId);

                var contentIdsToCopy = (string)cmd.ExecuteScalar();
                return contentIdsToCopy;
            }
        }

        public static string GetContentIdsBySiteId(SqlConnection sqlConnection, int sourceSiteId)
        {
            var query = @"   declare @ids varchar(max)
                                        select @ids = COALESCE(@ids + ', ', '') + CAST(content_id as nvarchar) from (
                                        select distinct(ci.content_id) as content_id from content_item (nolock) as ci
                                        inner join content as c on c.CONTENT_ID = ci.CONTENT_ID and c.virtual_type = 0 and c.SITE_ID = @sourceSiteId
                                    ) as t
                                    select @ids";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);

                var contentIdsToCopy = (string)cmd.ExecuteScalar();
                return contentIdsToCopy;
            }
        }

        public static string GetColumnsForTable(SqlConnection sqlConnection, string tableName, List<string> excludeColumns, Dictionary<string, string> valuesToChange, Dictionary<string, string> fieldsToAdd, string insertBeforeField)
        {

            var query = @"select COLUMN_NAME
                            from INFORMATION_SCHEMA.COLUMNS
                            where TABLE_NAME = @tableName";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@tableName", tableName);

                var result = string.Empty;

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                ExcludeColumns(excludeColumns, ref dt);
                ChangeValues(valuesToChange, ref dt);
                AddFields(insertBeforeField, fieldsToAdd, ref dt);

                return string.Join(", ", dt.AsEnumerable().Select(row => row.Field<string>("COLUMN_NAME").ToLower()));
            }
        }

        public static string GetColumnsForTable(SqlConnection sqlConnection, string tableName, List<string> excludeColumns, Dictionary<string, string> valuesToChange)
        {
            return GetColumnsForTable(sqlConnection, tableName, excludeColumns, valuesToChange, null, null);
        }

        public static string GetColumnsForTable(SqlConnection sqlConnection, string tableName, List<string> excludeColumns)
        {
            return GetColumnsForTable(sqlConnection, tableName, excludeColumns, null, null, null);
        }

        public static string GetColumnsForTable(SqlConnection sqlConnection, string tableName)
        {
            return GetColumnsForTable(sqlConnection, tableName, null, null, null, null);
        }

        private static void ChangeValues(Dictionary<string, string> valuesToChange, ref DataTable dt)
        {
            if (valuesToChange == null || valuesToChange.Count == 0)
                return;

            foreach (var keyValue in valuesToChange)
            {
                var row = dt.AsEnumerable().FirstOrDefault(rw => keyValue.Key.ToLower() == rw.Field<string>("COLUMN_NAME").ToLower());
                if (row != null)
                {
                    var rowIndex = dt.Rows.IndexOf(row);
                    if (keyValue.Value.IndexOf("{") > -1 && keyValue.Value.IndexOf("}") > -1)
                        dt.Rows[rowIndex]["COLUMN_NAME"] = string.Format("{0}", keyValue.Value.Replace("{", "").Replace("}", ""));
                    else
                        dt.Rows[rowIndex]["COLUMN_NAME"] = string.Format("'{0}' as [{1}]", keyValue.Value, keyValue.Key);

                }
            }
        }

        private static void ExcludeColumns(List<string> excludeColumns, ref DataTable dt)
        {
            if (excludeColumns == null || excludeColumns.Count == 0)
                return;

            foreach (var columnName in excludeColumns)
            {
                var row = dt.AsEnumerable().FirstOrDefault(rw => columnName.ToLower() == rw.Field<string>("COLUMN_NAME").ToLower());
                if (row != null)
                {
                    dt.Rows.Remove(row);
                }
            }
        }

        private static void AddFields(string insertBeforeField, Dictionary<string, string> fieldsToAdd, ref DataTable dt)
        {
            if (fieldsToAdd == null || fieldsToAdd.Count == 0)
                return;

            foreach (var fieldToAdd in fieldsToAdd)
            {
                var row = dt.NewRow();
                if (!string.IsNullOrEmpty(fieldToAdd.Value))
                    row["COLUMN_NAME"] = string.Format("{0} as {1}", fieldToAdd.Value, fieldToAdd.Key);
                else
                    row["COLUMN_NAME"] = string.Format("{0}", fieldToAdd.Key);
                var insertAfterRow = dt.AsEnumerable().FirstOrDefault(rw => insertBeforeField.ToLower() == rw.Field<string>("COLUMN_NAME").ToLower());
                var insertBeforeRowIndex = dt.Rows.IndexOf(insertAfterRow);

                dt.Rows.InsertAt(row, insertBeforeRowIndex);
            }
        }

        public static void UpdateDefaultFormatId(SqlConnection sqlConnection, int objectId, int formatId)
        {
            var sql = "update [object] set object_format_id = @formatId where [object_id] = @objectId";
            using (var cmd = SqlCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@formatId", formatId);
                cmd.Parameters.AddWithValue("@objectId", objectId);

                cmd.ExecuteNonQuery();
            }
        }

        public static string GetDbName(SqlConnection sqlConnection)
        {
            var query = @"select db_name() as name";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetDbServerName(SqlConnection sqlConnection)
        {
            var query = @"select @@SERVERNAME as server_name";

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (string)cmd.ExecuteScalar();
            }
        }

        public static int[] SortIdsByFieldName(SqlConnection sqlConnection, int[] ids, int contentId, string fieldName)
        {
            var template = @"
                select
                    CONTENT_ITEM_ID
                from
                    content_{0}_united a with(nolock) {1}
                WHERE
                    a.archive = 0
                ORDER BY
                    a.[{2}]";

            var join = "join @ids ids on a.CONTENT_ITEM_ID = ids.Id";

            var query = string.Format(template, contentId, ids == null ? null : join, fieldName);

            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                if (ids != null)
                {
                    cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(ids) });
                }

                var result = new List<int>();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add((int)dr.GetDecimal(0));
                    }

                    return result.ToArray();
                }
            }
        }

        #region ArticleMatching
        #region MatchContentsQuery
        private const string MatchContentsQuery = @"
            DECLARE @fields TABLE
            (
                Id INT NOT NULL,
                ParentId INT,
                Name NVARCHAR(255) NOT NULL,
                [Type] NVARCHAR(255),
                ContentId INT,
                UNIQUE (ParentId, Name, [Type], ContentId)
            )

            INSERT INTO
                @fields(Id, ParentId, Name, Type, ContentId)
            SELECT
                T.N.value('(Id/text())[1]', 'INT') Id,
                T.N.value('(ParentId/text())[1]', 'INT') ParentId,
                T.N.value('(Name/text())[1]', 'NVARCHAR(255)') Name,
                T.N.value('(Type/text())[1]', 'NVARCHAR(255)') Type,
                T.N.value('(ContentId/text())[1]', 'INT') ContentId
            FROM
                @fieldsXml.nodes('ArrayOfFieldInfo/FieldInfo') as T(N)
            ;
            WITH
            Query(Id, ParentId, RootContentId, ContentId, RefContentId, LinkId, Field, BackwardField, AttributeTypeId, DataType) AS
            (
                SELECT
                    f.Id,
                    f.ParentId,
                    s.CONTENT_ID,
                    s.CONTENT_ID,
                    CASE
                        WHEN otm.CONTENT_ID IS NOT NULL THEN otm.CONTENT_ID
                        WHEN mto.CONTENT_ID IS NOT NULL THEN mto.CONTENT_ID
                        WHEN mtm.r_content_id IS NOT NULL THEN mtm.r_content_id
                        WHEN classifier.CONTENT_ID IS NOT NULL THEN classifier.CONTENT_ID
                        ELSE NULL
                    END,
                    s.link_id,
                    f.Name,
                    CASE
                        WHEN mto.ATTRIBUTE_NAME IS NOT NULL THEN mto.ATTRIBUTE_NAME
                        WHEN classifier.ATTRIBUTE_NAME IS NOT NULL THEN classifier.ATTRIBUTE_NAME
                        ELSE NULL
                    END,
                    s.ATTRIBUTE_TYPE_ID,
                    f.Type
                FROM
                    @fields f
                    JOIN CONTENT_ATTRIBUTE s ON f.Name = s.ATTRIBUTE_NAME
                    LEFT JOIN CONTENT_ATTRIBUTE otm ON s.RELATED_ATTRIBUTE_ID = otm.ATTRIBUTE_ID
                    LEFT JOIN CONTENT_ATTRIBUTE mto ON s.BACK_RELATED_ATTRIBUTE_ID = mto.ATTRIBUTE_ID
                    LEFT JOIN CONTENT_TO_CONTENT mtm ON s.LINK_ID = mtm.LINK_ID AND s.CONTENT_ID = mtm.l_content_id
                    LEFT JOIN CONTENT_ATTRIBUTE classifier ON s.ATTRIBUTE_ID = classifier.CLASSIFIER_ATTRIBUTE_ID AND classifier.CONTENT_ID = f.ContentId
                WHERE
                    (
                        s.IS_CLASSIFIER = 0 OR
                        classifier.CONTENT_ID IS NOT NULL
                    ) AND
                    (
                        otm.CONTENT_ID IS NOT NULL OR
                        mto.CONTENT_ID IS NOT NULL OR
                        mtm.r_content_id IS NOT NULL OR
                        f.Type IS NULL OR
                        f.Type = 'date' AND s.ATTRIBUTE_TYPE_ID IN (4, 5, 6) OR
                        f.Type = 'string' AND s.ATTRIBUTE_TYPE_ID IN (1, 7, 8, 9, 10, 12) OR
                        f.Type = 'numeric' AND s.ATTRIBUTE_TYPE_ID IN (2, 3)
                    )
            ),
            FlatQuery(Id, RootContentId, ContentId, RefContentId, LinkId, Alias, ParentAlias, Field, BackwardField, AttributeTypeId, DataType) AS
            (
                SELECT
                    Id,
                    RootContentId,
                    ContentId,
                    RefContentId,
                    LinkId,
                    CAST('root_' + Field AS NVARCHAR(MAX)),
                    CAST('root' AS NVARCHAR(MAX)),
                    Field,
                    BackwardField,
                    AttributeTypeId,
                    DataType
                FROM Query
                    WHERE ParentId IS NULL

                UNION ALL

                SELECT
                    q.Id,
                    fq.RootContentId,
                    q.ContentId,
                    q.RefContentId,
                    q.LinkId,
                    fq.Alias + '_' + q.Field,
                    fq.Alias,
                    q.Field,
                    q.BackwardField,
                    q.AttributeTypeId,
                    q.DataType
                FROM
                    Query q
                    JOIN FlatQuery fq ON q.ParentId = fq.Id
                WHERE
                    fq.RefContentId = q.ContentId
            )
            SELECT RootContentId, ContentId, RefContentId, LinkId, ParentAlias Alias, Field, BackwardField, AttributeTypeId, DataType
            FROM FlatQuery
            WHERE RootContentId IN (SELECT Id FROM @contentIds)";
        #endregion

        public static IEnumerable<DataRow> MatchContents(SqlConnection sqlConnection, int[] contentIds, XDocument fields)
        {
            using (var cmd = SqlCommandFactory.Create(MatchContentsQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@contentIds", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(contentIds) });
                cmd.Parameters.Add(new SqlParameter("@fieldsXml", SqlDbType.Xml) { Value = fields.ToString() });

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> MatchArticles(SqlConnection sqlConnection, Dictionary<string, object> args, string query)
        {
            using (var cmd = SqlCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddRange(args.Select(param => new SqlParameter(param.Key, param.Value)).ToArray());

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }
        #endregion

        #region BatchUpdate

        #region BatchInsertQuery
        private const string BatchInsertQuery = @"
            DECLARE @articles TABLE
            (
                Id INT PRIMARY KEY IDENTITY(1,1),
                ContentId INT,
                ArticleId INT
                UNIQUE (ContentId, ArticleId)
            )

            DECLARE @statuses TABLE
            (
                ContentId INT PRIMARY KEY,
                StatusId INT,
                UNIQUE (StatusId, ContentId)
            )

            DECLARE @ids TABLE
            (
                Id INT PRIMARY KEY IDENTITY(1,1),
                ArticleId INT
                UNIQUE (ArticleId)
            )

            INSERT INTO
                @articles(ContentId, ArticleId)
            SELECT DISTINCT
                ContentId,
                ArticleId
            FROM
                @values
            EXCEPT
                SELECT
                    CONTENT_ID,
                    CONTENT_ITEM_ID
                FROM
                    CONTENT_ITEM
            ORDER BY
                ArticleId DESC

            INSERT INTO
                @statuses(ContentId, StatusId)
            SELECT
                a.ContentId,
                CASE
                    WHEN
                        w.WORKFLOW_ID IS NULL
                    THEN
                    (
                        SELECT STATUS_TYPE_ID
                        FROM STATUS_TYPE t
                        WHERE t.STATUS_TYPE_NAME = 'Published' AND t.SITE_ID = c.SITE_ID)
                    ELSE
                    (
                        SELECT STATUS_TYPE_ID
                        FROM STATUS_TYPE t
                        WHERE t.STATUS_TYPE_NAME = 'None' AND t.SITE_ID = c.SITE_ID
                    )
                END StatusId
            FROM
                @articles a
                JOIN CONTENT c ON a.ContentId = c.CONTENT_ID
                LEFT JOIN CONTENT_WORKFLOW_BIND w ON a.ContentId = w.CONTENT_ID
            GROUP BY
                a.ContentId,
                w.WORKFLOW_ID,
                c.SITE_ID

            INSERT INTO CONTENT_ITEM
            (
                CONTENT_ID,
                VISIBLE,
                STATUS_TYPE_ID,
                LAST_MODIFIED_BY,
                NOT_FOR_REPLICATION
            )
            OUTPUT
                INSERTED.CONTENT_ITEM_ID INTO @ids(ArticleId)
            SELECT
                a.ContentId,
                @visible,
                s.StatusId,
                @userId,
                1
            FROM
                @articles a
                JOIN @statuses s ON a.ContentId = s.ContentId
            ORDER BY
                ArticleId DESC

            SELECT
                old.ArticleId OriginalArticleId,
                new.ArticleId CreatedArticleId,
                old.ContentId
            FROM
                @ids new
                JOIN @articles old ON new.Id = old.Id";
        #endregion

        public static IEnumerable<DataRow> BatchInsert(SqlConnection sqlConnection, DataTable articles, bool visible, int userId)
        {
            using (var cmd = SqlCommandFactory.Create(BatchInsertQuery, sqlConnection))
            {
                cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });
                cmd.Parameters.AddWithValue("@visible", visible ? 1 : 0);
                cmd.Parameters.AddWithValue("@userId", userId);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        #region BatchUpdateQuery
        private const string BatchUpdateQuery = @"
            UPDATE
                CONTENT_ITEM
            SET
                NOT_FOR_REPLICATION = 1,
                MODIFIED = GETDATE(),
                LAST_MODIFIED_BY = @userId
            WHERE
                NOT_FOR_REPLICATION = 0 AND
                CONTENT_ITEM_ID IN (SELECT DISTINCT ArticleId FROM @values)

            UPDATE
                CONTENT_DATA
            SET
                DATA =
                CASE
                    WHEN t.DATABASE_TYPE != 'NTEXT' AND v.Value != '' THEN v.Value
                    ELSE NULL
                END,
                BLOB_DATA =
                CASE
                    WHEN t.DATABASE_TYPE = 'NTEXT' AND v.Value != '' THEN v.Value
                    ELSE NULL
                END
            FROM
                CONTENT_DATA d
                JOIN @values v ON v.ArticleId = d.CONTENT_ITEM_ID AND v.FieldId = d.ATTRIBUTE_ID
                JOIN CONTENT_ATTRIBUTE a ON v.FieldId = a.ATTRIBUTE_ID AND v.ContentId = a.CONTENT_ID
                JOIN ATTRIBUTE_TYPE t ON a.ATTRIBUTE_TYPE_ID = t.ATTRIBUTE_TYPE_ID";
        #endregion

        public static void BatchUpdate(SqlConnection sqlConnection, DataTable articles, int userId)
        {
            using (var cmd = SqlCommandFactory.Create(BatchUpdateQuery, sqlConnection))
            {
                cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        #region GetRelationsQuery
        private const string GetRelationsQuery = @"
            SELECT
                f.ArticleId,
                a.CONTENT_ID ContentId,
                a.ATTRIBUTE_ID FieldId,
                a.ATTRIBUTE_NAME FieldName,
                f.Value FieldValue,
                CASE
                    WHEN otm.CONTENT_ID IS NOT NULL THEN a.CONTENT_ID
                    WHEN mto.CONTENT_ID IS NOT NULL THEN mto.CONTENT_ID
                    WHEN mtm.linked_content_id IS NOT NULL THEN mtm.linked_content_id
                    WHEN classifier.CONTENT_ID IS NOT NULL THEN classifier.CONTENT_ID
                    ELSE NULL
                END RefContentId,
                CASE
                    WHEN otm.CONTENT_ID IS NOT NULL THEN a.ATTRIBUTE_ID
                    WHEN mto.CONTENT_ID IS NOT NULL THEN mto.ATTRIBUTE_ID
                    WHEN classifier.CONTENT_ID IS NOT NULL THEN classifier.ATTRIBUTE_ID
                    ELSE NULL
                END RefFieldId,
                CASE
                    WHEN mtm.link_id IS NOT NULL THEN mtm.link_id
                    ELSE NULL
                END LinkId
            FROM
                @values f
                JOIN CONTENT_ATTRIBUTE a ON f.FieldId = a.ATTRIBUTE_ID AND f.ContentId = a.CONTENT_ID
                LEFT JOIN CONTENT_ATTRIBUTE otm ON a.RELATED_ATTRIBUTE_ID = otm.ATTRIBUTE_ID
                LEFT JOIN CONTENT_ATTRIBUTE mto ON a.BACK_RELATED_ATTRIBUTE_ID = mto.ATTRIBUTE_ID
                LEFT JOIN CONTENT_LINK mtm ON a.LINK_ID = mtm.LINK_ID AND a.CONTENT_ID = mtm.content_id
                LEFT JOIN CONTENT_ATTRIBUTE classifier ON a.ATTRIBUTE_ID = classifier.CLASSIFIER_ATTRIBUTE_ID AND CAST(classifier.CONTENT_ID AS NVARCHAR(MAX)) = f.Value
            WHERE
                otm.CONTENT_ID IS NOT NULL OR
                mto.CONTENT_ID IS NOT NULL OR
                mtm.linked_content_id IS NOT NULL OR
                classifier.CONTENT_ID IS NOT NULL";
        #endregion

        public static IEnumerable<DataRow> GetRelations(SqlConnection sqlConnection, DataTable articles)
        {
            using (var cmd = SqlCommandFactory.Create(GetRelationsQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void ReplicateItems(SqlConnection sqlConnection, int[] articleIds, int[] fieldIds)
        {
            using (var cmd = SqlCommandFactory.Create("qp_replicate_items", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.NVarChar, -1) { Value = string.Join(",", articleIds) });
                cmd.Parameters.Add(new SqlParameter("@attr_ids", SqlDbType.NVarChar, -1) { Value = string.Join(",", fieldIds) });
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        public static DataTable GetFieldTypes(SqlConnection cnn, int[] tableData)
        {
            const string text = "select attribute_id, BACK_RELATED_ATTRIBUTE_ID, attribute_type_id, link_id, is_classifier, " +
                                "cast(case when isnull(cast(enum_values as nvarchar(max)), '') <> '' then 1 else 0 end as bit) as is_string_enum " +
                                "from content_attribute where attribute_id in (select id from @ids)";

            using (var cmd = SqlCommandFactory.Create(text, cnn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.Structured)
                {
                    TypeName = "ids",
                    Value = IdsToDataTable(tableData)
                });

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                return dt;
            }
        }

        public static DataTable IdsToDataTable(IEnumerable<int> ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("id");
            foreach (var id in ids ?? Enumerable.Empty<int>())
            {
                dt.Rows.Add(id);
            }

            return dt;
        }

        public static IList<int> GetParentIdsTreeResult(SqlConnection cn, IList<int> ids, int fieldId)
        {
            var query = GetParentIdsTreeQuery(cn, ids, fieldId);
            return GetDatatableResult(cn, query, GetIdsDatatableParam("@ids", ids), new SqlParameter("@fieldId", fieldId)).Select(dr => (int)dr.Field<decimal>(0)).ToList();
        }

        public static string GetParentIdsTreeQuery(SqlConnection cn, IList<int> ids, int fieldId)
        {
            return @"
                declare @contentId int
                declare @fieldName nvarchar(255)

                select
                    @contentId = CONTENT_ID,
                    @fieldName = ATTRIBUTE_NAME
                from
                    CONTENT_ATTRIBUTE
                where
                    ATTRIBUTE_ID = @fieldId and
                    ATTRIBUTE_TYPE_ID = 11

                if (@contentId is not null and @fieldName is not null)
                begin
                    declare @sql nvarchar(max) ='
                        with Result(Id, ParentId, Lvl)
                        as
                        (
                            select
                                CONTENT_ITEM_ID Id,'
                                + @fieldName + ' ParentId,
                                0 Lvl
                            from
                                content_'+ convert(nvarchar(10), @contentId) +'_united c with(nolock)
                            where
                                CONTENT_ITEM_ID IN (SELECT * FROM @ids)

                            union all

                            select
                                c.CONTENT_ITEM_ID Id,
                                c.Parent,
                                r.Lvl + 1
                            from
                                content_'+ convert(nvarchar(10), @contentId) +'_united c with(nolock)
                                join Result r on c.CONTENT_ITEM_ID = r.ParentId
                        )"
                       + (ids.Count > 1
                       ? "SELECT DISTINCT Id, ParentId FROM Result'"
                       : "SELECT DISTINCT Id, ParentId, Lvl FROM Result ORDER BY Lvl'")
                       + "EXEC sp_executesql @sql, N'@ids dbo.Ids READONLY, @fieldId int', @ids, @fieldId END";
        }

        private static SqlParameter GetIdsDatatableParam(string paramName, IEnumerable<int> ids)
        {
            return new SqlParameter(paramName, SqlDbType.Structured)
            {
                TypeName = "Ids",
                Value = IdsToDataTable(ids)
            };
        }

        private static IList<DataRow> GetDatatableResult(SqlConnection cn, string query, out int totalCount, params SqlParameter[] @params)
        {
            using (var cmd = SqlCommandFactory.Create(query, cn))
            {
                cmd.CommandType = CommandType.Text;
                if (@params != null && @params.Any())
                {
                    cmd.Parameters.AddRange(@params);
                }

                var ds = new DataSet();
                totalCount = new SqlDataAdapter(cmd).Fill(ds);

                if (ds.Tables.Count > 0)
                {
                    return ds.Tables[0].AsEnumerable().ToList();
                }

                return Enumerable.Empty<DataRow>().ToList();
            }
        }

        private static IList<DataRow> GetDatatableResult(SqlConnection cn, string query, params SqlParameter[] @params)
        {
            int totalCount;
            return GetDatatableResult(cn, query, out totalCount, @params);
        }

        private static IList<DataRow> GetDatatableResult(SqlConnection cn, StringBuilder queryBuilder, params SqlParameter[] @params)
        {
            return GetDatatableResult(cn, queryBuilder.ToString(), @params);
        }
    }
}
