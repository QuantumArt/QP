using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Npgsql;
using NpgsqlTypes;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {
              public static void PersistArticle(DbConnection currentDbConnection, string xml, out int id)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(currentDbConnection);
            var ns = SqlQuerySyntaxHelper.DbSchemaName(databaseType);
            var sql = $"select id, modified from {ns}.qp_persist_article(@xml)";
            using (var cmd = DbCommandFactory.Create(sql, currentDbConnection))
            {
                cmd.CommandType = CommandType.Text;
                if (databaseType == DatabaseType.Postgres)
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@xml", NpgsqlDbType.Xml) { Value = xml });
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@xml", SqlDbType.Xml) { Value = xml });
                }

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                id = (int)dt.Rows[0]["id"];
            }
        }

        public static void UpdateChildDelayedSchedule(DbConnection connection, int articleId)
        {
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var idParamName = SqlQuerySyntaxHelper.SpParamName(databaseType, "id");
            var sql = databaseType == DatabaseType.SqlServer ? "qp_copy_schedule_to_child_delays" : "call qp_copy_schedule_to_child_delays(@id)";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = databaseType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.AddWithValue(idParamName, articleId);
                cmd.ExecuteNonQuery();
            }
        }

        private static string PgSelectType(int attributeTypeId)
        {
            if (new[] { 2, 3, 11, 13 }.Contains(attributeTypeId))
            {
                return "numeric";
            }
            else if (new[] { 4, 5, 6}.Contains(attributeTypeId))
            {
                return "timestamp without time zone";
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
            var noLock = WithNolock(databaseType);
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
                cmd.CommandType = CommandType.Text;

                var doc = new XDocument(new XElement("guids"));
                doc.Root?.Add(guids.Select(n => new XElement("guid", n.ToString())));
                if (databaseType == DatabaseType.Postgres)
                {
                    cmd.Parameters.Add(new NpgsqlParameter("@xml", NpgsqlDbType.Xml) { Value = doc.ToString() });
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@xml", SqlDbType.Xml) { Value = doc.ToString() });
                }

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
            var table = SqlQuerySyntaxHelper.IdList(databaseType, "@ids", "ids");
            var noLock = WithNolock(databaseType);
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

        public class FieldParameter
        {
            public string Name { get; set; }

            public DbType DbType { get; set; }

            public NpgsqlDbType NpgsqlDbType { get; set; }

            public string Value { get; set; }
        }

        private static DbParameter CreateDbParameter(DatabaseType dbType, FieldParameter item)
        {
            var value = !string.IsNullOrEmpty(item.Value) ? (object)item.Value : DBNull.Value;
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
            var sql = databaseType == DatabaseType.SqlServer ? "qp_create_content_item_versions" : "call qp_create_content_item_versions(@ids, @last_modified_by)";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = databaseType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, databaseType));
                cmd.Parameters.AddWithValue("@last_modified_by", userId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
