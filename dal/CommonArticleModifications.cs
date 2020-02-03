using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using Npgsql;
using NpgsqlTypes;
using Quantumart.QP8.Constants;
using LogManager = NLog.LogManager;
using NLog.Fluent;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public static void PersistArticle(DbConnection currentDbConnection, string customerCode, string xml, out int id)
        {

            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(currentDbConnection);
            var ns = SqlQuerySyntaxHelper.DbSchemaName(databaseType);
            var sql = $"select id, modified from {ns}.qp_persist_article(@xml)";
            using (var cmd = DbCommandFactory.Create(sql, currentDbConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xml", xml, databaseType));
                var dt = new DataTable();
                try
                {
                    DataAdapterFactory.Create(cmd).Fill(dt);
                }
                catch (PostgresException ex)
                {
                    Logger.Error()
                        .Exception(ex)
                        .Message("Error while persisting article with xml: {xml}\n Query: {sql}", xml, sql)
                        .Property("customerCode", customerCode)
                        .Write();

                    throw;
                }
                id = (int)dt.Rows[0]["id"];
            }
        }

        public static void UpdateChildDelayedSchedule(DbConnection connection, int articleId)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = SqlQuerySyntaxHelper.SpCall(databaseType, "qp_copy_schedule_to_child_delays", articleId.ToString());
            ExecuteSql(connection, sql);
        }

        private static string PgSelectType(int attributeTypeId)
        {
            if (new[] { 2, 3, 11, 13 }.Contains(attributeTypeId))
            {
                return "numeric";
            }
            else if (new[] { 4, 5, 6}.Contains(attributeTypeId))
            {
                return "timestamp with time zone";
            }
            else
            {
                return "text";
            }
        }

        public static DataRow GetDefaultArticleRow(QPModelDataContext context, DbConnection connection, int contentId)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = "qp_get_default_article";
            if (databaseType == DatabaseType.Postgres)
            {
                var fields = context.FieldSet.Where(n => n.ContentId == contentId).OrderBy(n => n.Name).ToArray();
                var fieldNames = fields.Select(n => n.Name.ToLower()).ToArray();
                var fieldNameResults = String.Join(",", fieldNames.Select(n => $@"""{n}"" TEXT"));
                var fieldSelects = String.Join(",", fields.Select(n => $@"""{n.Name.ToLower()}""::{PgSelectType((int)n.TypeId)}"));
                sql = $@"
			SELECT {fieldSelects} FROM crosstab('
			select 0::numeric as content_item_id, lower(ca.attribute_name),
			case when ca.attribute_type_id in (9, 10) then coalesce(ca.default_value, ca.default_blob_value)
			else qp_correct_data(ca.default_value::text, ca.attribute_type_id, ca.attribute_size, ca.default_value)::text
			end as value from content_attribute ca
			inner join content c on ca.content_id = c.content_id
			where c.content_id = {contentId}
			order by 1,2
			') AS final_result(content_item_id numeric, {fieldNameResults})";
            }


            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                if (databaseType == DatabaseType.Postgres)
                {
                    cmd.CommandType = CommandType.Text;
                }
                else
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@content_id", contentId);
                }

                var ds = new DataSet();
                DataAdapterFactory.Create(cmd).Fill(ds);
                return 0 == ds.Tables.Count || 0 == ds.Tables[0].Rows.Count ? null : ds.Tables[0].Rows[0];
            }
        }

        public static int[] GetArticleIdsByGuids(DbConnection sqlConnection, Guid[] guids)
        {
            if (guids == null)
            {
                throw new ArgumentNullException(nameof(guids));
            }
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var noLock = WithNoLock(databaseType);
            var textCast = (databaseType == DatabaseType.Postgres) ? "::text" : "";
            string xmlSource;
            if (databaseType == DatabaseType.Postgres)
            {
                xmlSource = "XMLTABLE('/guids/guid' PASSING @xml COLUMNS unique_id uuid PATH '.')";
            }
            else
            {
                xmlSource = "(SELECT doc.col.value('.[1]', 'nvarchar(max)') UNIQUE_ID FROM @xml.nodes('/guids/guid') doc(col))";
            }

            var query = $"select coalesce(ci.content_item_id, 0), guids.unique_id{textCast} from {xmlSource} guids left join content_item ci {noLock} on ci.unique_id = guids.unique_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                var doc = new XDocument(new XElement("guids"));
                doc.Root?.Add(guids.Select(n => new XElement("guid", n.ToString())));
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xml", doc.ToString(), databaseType));

                var result = new Dictionary<Guid, int>();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add(new Guid(dr.GetString(1)), (int)dr.GetDecimal(0));
                    }
                }

                return guids.Select(n => result[n]).ToArray();
            }
        }

        public static Guid[] GetArticleGuidsByIds(DbConnection sqlConnection, int[] ids)
        {

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var table = IdList(databaseType, "@ids", "ids");
            var noLock = WithNoLock(databaseType);
            var query = $@"select ci.unique_id, ids.id from {table} left join content_item ci {noLock} on ci.content_item_id = ids.id";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, databaseType));

                var result = new Dictionary<int, Guid>();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add((int)dr.GetDecimal(1), dr.IsDBNull(0) ? Guid.Empty : dr.GetGuid(0));
                    }
                }

                return ids.Select(n => result[n]).ToArray();
            }
        }

        private static DbParameter CreateDbParameter(DatabaseType dbType, FieldParameter item)
        {
            var value = item.ObjectValue ?? DBNull.Value;
            if (dbType == DatabaseType.Postgres)
            {
                return new NpgsqlParameter
                {
                    ParameterName = item.Name,
                    NpgsqlDbType = item.NpgsqlDbType,
                    Value = value
                };
            }
            return new SqlParameter
            {
                ParameterName = item.Name,
                DbType = item.DbType,
                Value = value
            };
        }


        public static string GetConflictIds(DbConnection connection, int id, int contentId, string condition, List<FieldParameter> parameters)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            using (var cmd = DbCommandFactory.Create($"SELECT CONTENT_ITEM_ID FROM CONTENT_{contentId}_UNITED WHERE {condition} AND CONTENT_ITEM_ID <> @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(CreateDbParameter(databaseType, parameter));
                }

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return IdCommaList(dt, FieldName.ContentItemId);
            }
        }

        public static void LockArticleForUpdate(DbConnection cnn, int id)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var withlock = databaseType == DatabaseType.SqlServer ? "with(rowlock, updlock)" : "";
            var forUpdate = databaseType != DatabaseType.SqlServer ? "for update" : "";

            using (var cmd = DbCommandFactory.Create($@"select content_item_id from content_item {withlock} where content_item_id = @id {forUpdate}", cnn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
            }
        }

        public static void CreateArticleVersion(DbConnection connection, int userId, int id)
        {
            CreateArticleVersions(connection, userId, new [] {id});
        }

        public static void CreateArticleVersions(DbConnection connection, int userId, int[] ids)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = SqlQuerySyntaxHelper.SpCall(databaseType, "qp_create_content_item_versions", "@ids, @last_modified_by");
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, databaseType));
                cmd.Parameters.AddWithValue("@last_modified_by", userId);
                cmd.ExecuteNonQuery();
            }
        }


        public static void SetArchiveFlag(DbConnection connection, IEnumerable<int> articleIds, int userId, bool flag, bool withAggregated)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var idParam = withAggregated ? $@"{DbSchemaName(dbType)}.qp_aggregated_and_self(@ids)" : "@ids";
            var source = IdList(dbType, idParam);
            var value = flag ? 1 : 0;

            using (var cmd = DbCommandFactory.Create($@"
                    update content_item {WithRowLock(dbType)} set archive = {value}, modified = {Now(dbType)}, last_modified_by = @userId where content_item_id in (select id from {source});
                    update content_item {WithRowLock(dbType)} set locked_by = null, locked = null where content_item_id in (select id from {source});
                    ", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", articleIds, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Массовая публикация (может использоваться для статей из разных контентов одного сайта)
        /// </summary>
        public static void Publish(DbConnection connection, IEnumerable<int> articleIds, int userId, bool withAggregated)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var idParam = withAggregated ? $@"{DbSchemaName(dbType)}.qp_aggregated_and_self(@ids)" : "@ids";
            var source = IdList(dbType, idParam);
            string sql = $@"call qp_publish(@ids, @userId);";
            if (dbType == DatabaseType.SqlServer)
            {
                sql = $@"
                    declare @ids2 [Ids]

                    insert into @ids2
                    select id from {source}

                    declare @statusTypeId numeric
                    select @statusTypeId = status_type_id from status_type where status_type_name = 'Published' and site_id in (select site_id from content c inner join content_item ci with(nolock) on c.content_id = ci.content_id inner join @ids2 i on i.id = ci.content_item_id )

                    update content_item with(rowlock) set status_type_id = @statusTypeId, modified = getdate(), last_modified_by = @userId where content_item_id in (select id from @ids2) and status_type_id <> @statusTypeId and splitted = 0;
                    update content_item with(rowlock) set status_type_id = @statusTypeId, modified = getdate(), last_modified_by = @userId, schedule_new_version_publication = 1 where content_item_id in (select id from @ids2) and status_type_id <> @statusTypeId and splitted = 1;

                    exec qp_merge_delays @ids2, @userId

                    delete i from @ids2 i inner join content_item ci with(nolock) on ci.content_item_id = i.id where ci.splitted = 0 and ci.schedule_new_version_publication = 0

                    exec qp_merge_articles @ids2, @userId

                    ";
            }

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", articleIds, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteArticles(DbConnection connection, List<int> ids, bool withAggregated)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var idParam = withAggregated ? $@"{DbSchemaName(dbType)}.qp_aggregated_and_self(@ids)" : "@ids";
            var source = IdList(dbType, idParam);

            if (ids != null && ids.Any())
            {
                var query = $"DELETE FROM content_item {WithRowLock(dbType)} where content_item_id in (select id from {source}) ";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void GetContentModification(DbConnection sqlConnection, IEnumerable<int> articleIds, bool withAggregated, bool returnPublishedForLive, ref List<int> liveIds, ref List<int> stageIds)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var idsParam = withAggregated ? $"{DbSchemaName(dbType)}.qp_aggregated_and_self(@ids)" : "@ids";
            var source = IdList(dbType, idsParam);

            var aggFunc = returnPublishedForLive ? "max" : "min";
            var ids = articleIds as int[] ?? articleIds.ToArray();
            if (!ids.Any())
            {
                return;
            }

            var sql = $@"
			    select cast(a.content_id as int) as content_id, cast({aggFunc}(a.is_published) as bit) as is_published from
		        (
			        select ci.content_item_id, ci.content_id,
			        case when st.status_type_name = 'Published' and {IsFalse(dbType, "ci.splitted")} then 1 else 0 end as is_published
			        from {source}
			        inner join content_item ci {WithNoLock(dbType)} on i.id = ci.content_item_id
			        inner join status_type st on ci.status_type_id = st.status_type_id
		        ) a group by a.content_id
            ";


            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                var rows = dt.AsEnumerable().ToArray();
                stageIds = rows.Select(n => n.Field<int>("content_id")).ToList();
                bool Predicate1(DataRow n) => n.Field<bool>("is_published");
                bool Predicate2(DataRow n) => !n.Field<bool>("is_published");

                liveIds = rows.Where(returnPublishedForLive ? (Func<DataRow, bool>)Predicate1 : Predicate2).Select(n => n.Field<int>("content_id")).ToList();
            }
        }

        public static void UpdateContentModification(DbConnection sqlConnection, List<int> liveIds, List<int> stageIds)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = new List<DbParameter>();
            var sb = new StringBuilder();
            ;
            if (liveIds.Any())
            {
                sb.AppendLine($@"update content_modification {WithRowLock(dbType)} set live_modified = {Now(dbType)} where content_id in (select id from {IdList(dbType, "@liveIds")});");
                sql.Add(GetIdsDatatableParam("@liveIds", liveIds, dbType));
            }

            if (stageIds.Any())
            {
                sb.AppendLine($@"update content_modification {WithRowLock(dbType)} set stage_modified = {Now(dbType)} where content_id in (select id from {IdList(dbType, "@stageIds")});");
                sql.Add(GetIdsDatatableParam("@stageIds", stageIds, dbType));
            }

            if (sql.Any())
            {
                using (var cmd = DbCommandFactory.Create(sb.ToString(), sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(sql.ToArray());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int CountChildArticles(DbConnection sqlConnection, int articleId, bool countArchived)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = "qp_count_child_articles";
            if (dbType == DatabaseType.Postgres)
            {
                sql = $@"
                    select count(*)::int from content_data cd
                    inner join content_attribute ca on ca.attribute_id = cd.attribute_id
                    inner join content_item ci on ci.content_item_id = cd.content_item_id
                    where o2m_data = @article_id and not ca.aggregated and ci.archive = 0
                ";

            }

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                string outParam;
                cmd.Parameters.AddWithValue("@article_id", articleId);
                if (dbType == DatabaseType.SqlServer)
                {
                    outParam = "@count";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@count_archived", countArchived);
                    cmd.Parameters.Add(new SqlParameter(outParam, SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    });
                }
                else
                {
                    outParam = "count";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new NpgsqlParameter(outParam, NpgsqlDbType.Integer)
                    {
                        Direction = ParameterDirection.Output
                    });
                }
                cmd.ExecuteNonQuery();
                return (int)cmd.Parameters[outParam].Value;
            }
        }

        public static void AdjustManyToMany(DbConnection connection, int id, int newId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = dbType == DatabaseType.SqlServer ? $@"
                update item_to_item {WithRowLock(dbType)} set l_item_id = @newId where l_item_id = @id and r_item_id = @newId;
                delete from item_to_item {WithRowLock(dbType)} where r_item_id = @id and l_item_id = @newId;
            " : $@"
                insert into item_to_item(link_id, l_item_id, r_item_id)
                select link_id, @newId, @newId from item_to_item where l_item_id = @newId and r_item_id = @id;
                delete from item_to_item where l_item_id = @newId and r_item_id = @id;
            ";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@newId", newId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
