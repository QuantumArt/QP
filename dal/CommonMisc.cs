using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {

        public static UserDAL Authenticate(DbConnection connection, string login, string password, bool useNtLogin, bool checkAdminAccess)
        {

            object[] parameters;
            DatabaseType dbType;
            switch (connection)
            {

                case SqlConnection _:
                    parameters = new object[] {
                        new SqlParameter { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                        new SqlParameter { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? string.Empty },
                        new SqlParameter { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                        new SqlParameter { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
                    };
                    dbType = DatabaseType.SqlServer;
                    break;
                case NpgsqlConnection _:
                    parameters = new object[] {
                        new NpgsqlParameter { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                        new NpgsqlParameter { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? string.Empty },
                        new NpgsqlParameter { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                        new NpgsqlParameter { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
                    };
                    dbType = DatabaseType.Postgres;
                    break;
                default:
                    throw new ApplicationException("Unknown connection type");
            }

            using (var cmd = DbCommandFactory.Create("qp_authenticate", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);

                var dt = new DataTable();
                var dataAdapter = DataAdapterFactory.Create(cmd);
                dataAdapter.Fill(dt);
                if (dt.Rows.Count == 0)
                {
                    return null;
                }

                var r = dt.Rows[0];
                return new UserDAL()
                {
                    Id = r.Field<decimal>("USER_ID"),
                    Disabled = r.Field<decimal>("DISABLED"),
                    FirstName = r.Field<string>("FIRST_NAME"),
                    LastName = r.Field<string>("LAST_NAME"),
                    Email = r.Field<string>("EMAIL"),
                    AutoLogOn = r.Field<decimal>("AUTO_LOGIN"),
                    NTLogOn = r.Field<string>("NT_LOGIN"),
                    LastLogOn = r.Field<DateTime?>("LAST_LOGIN"),
                    Subscribed = r.Field<decimal>("SUBSCRIBED"),
                    Created = r.Field<DateTime>(FieldName.Created),
                    Modified = r.Field<DateTime>(FieldName.Modified),
                    LastModifiedBy = r.Field<decimal>(FieldName.LastModifiedBy),
                    LanguageId = r.Field<decimal?>("LANGUAGE_ID"),
                    VMode = r.Field<decimal>("VMODE"),
                    AdSid = r.Field<byte[]>("ad_sid"),
                    AllowStageEditField = r.Field<decimal>("allow_stage_edit_field"),
                    AllowStageEditObject = r.Field<decimal>("allow_stage_edit_object"),
                    BuiltIn = r.Field<bool>("BUILT_IN"),
                    LogOn = r.Field<string>("LOGIN"),
                    PasswordModified = r.Field<DateTime>("PASSWORD_MODIFIED"),
                    MustChangePassword = r.Field<bool>("MUST_CHANGE_PASSWORD")
                };


            }

        }

        public static IEnumerable<DataRow> GetChildFoldersList(DbConnection sqlConnection, int userId, int id, bool isSite, int? folderId, int permissionLevel, bool countOnly, out int totalRecords)
        {
            totalRecords = -1;
            using (var cmd = DbCommandFactory.Create("qp_get_folders_tree", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = id });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "is_site", DbType = DbType.Boolean, Value = isSite });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords });

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                totalRecords = (int)cmd.Parameters["total_records"].Value;
                return dt.AsEnumerable().ToArray();
            }
        }

        public static string GetEntityName(DbConnection sqlConnection, string entityTypeCode, int entityId, int parentEntityId)
        {

            using (var cmd = DbCommandFactory.Create("qp_get_entity_title", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("entity_type_code", entityTypeCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);
                cmd.Parameters.AddWithValue("parent_entity_id", parentEntityId);
                cmd.Parameters.Add(
                    new SqlParameter
                    {
                        ParameterName = "title",
                        DbType = DbType.String,
                        Size = 255,
                        Direction = ParameterDirection.Output,
                    }
                );
                cmd.ExecuteNonQuery();
                return cmd.Parameters["title"].Value.ToString();
            }
        }

        public static bool CheckEntityExistence(DbConnection sqlConnection, string entityTypeCode, decimal entityId)
        {
            using (var cmd = DbCommandFactory.Create("qp_check_entity_existence", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("entity_type_code", entityTypeCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);
                return  bool.Parse(cmd.ExecuteScalar().ToString());
            }
        }


        public static void SetContentItemVisible(DbConnection sqlConnection, int contentItemId, bool isVisible, int lastModifiedBy = 1)
        {
            var sql = "UPDATE content_item with(rowlock) SET visible = @is_visible, modified = getdate(), last_modified_by = @last_modified_by WHERE content_item_id = @id";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("id", contentItemId);
                cmd.Parameters.AddWithValue("is_visible", isVisible ? 1 : 0);
                cmd.Parameters.AddWithValue("last_modified_by", lastModifiedBy);

                cmd.ExecuteNonQuery();
            }
        }

        public static void MergeArticle(DbConnection sqlConnection, int contentItemId, int lastModifiedBy = 1)
        {
            using (var cmd = DbCommandFactory.Create("qp_merge_article", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("item_id", contentItemId);
                cmd.Parameters.AddWithValue("last_modified_by", lastModifiedBy);
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable SearchInArticles(DbConnection sqlConnection, int siteId, int userId, string searchString, int? articleId, string sortExpression, int startRow, int pageSize, out int totalRecords)
        {
            totalRecords = -1;
            object[] parameters =
            {
                new SqlParameter { ParameterName = "p_site_id", DbType = DbType.Int32, Value = siteId },
                new SqlParameter { ParameterName = "p_user_id", DbType = DbType.Int32, Value = userId },
                new SqlParameter { ParameterName = "p_item_id", DbType = DbType.Int32, Value = articleId },
                new SqlParameter { ParameterName = "p_searchparam", DbType = DbType.String, Value = Cleaner.ToSafeSqlString(searchString) },
                new SqlParameter { ParameterName = "p_order_by", DbType = DbType.String, Value = sortExpression },
                new SqlParameter { ParameterName = "p_start_row", DbType = DbType.Int32, Value = startRow },
                new SqlParameter { ParameterName = "p_page_size", DbType = DbType.Int32, Value = pageSize },
                new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
            };

            using (var cmd = DbCommandFactory.Create("qp_all_article_search", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                totalRecords = (int)cmd.Parameters["total_records"].Value;
                return dt;
            }
        }

        /// <summary>
        /// Получить словоформы для строки запроса
        /// </summary>
        public static IEnumerable<string> GetWordForms(DbConnection sqlConnection, string searchString)
        {
            using (var cmd = DbCommandFactory.Create("usp_fts_parser", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("searchString", Cleaner.ToSafeSqlString(searchString));
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().Select(r => r.Field<string>("display_term")).ToArray();
            }
        }

        public static Version GetSqlServerVersion(DbConnection sqlConnection)
        {
            string version;
            using (var cmd = DbCommandFactory.Create("SELECT SERVERPROPERTY('productversion') [version]", sqlConnection))
            {
                version = cmd.ExecuteScalar().ToString();
            }

            return new Version(version);
        }

        /// <summary>
        /// Получает список стоп-слов для языка который указан в full text serach индексе на столбце dbo.CONTENT_DATA.DATA
        /// </summary>
        public static IEnumerable<string> GetStopWordList(DbConnection sqlConnection)
        {
            const string query = "select stopword from sys.fulltext_system_stopwords " +
                "where language_id = (" +
                "select top 1 language_id from sys.fulltext_index_columns " +
                "where object_id = object_id('dbo.CONTENT_DATA') " +
                "and column_id = COLUMNPROPERTY (object_id('dbo.CONTENT_DATA'), 'DATA' , 'ColumnId' ))";

            var result = new List<string>();

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            return result;
        }

        public static decimal? GetNumericFieldValue(DbConnection dbConnection, string fieldName, string entityName, string entityIdField, decimal entityId)
        {
            var query = $"select {fieldName} from {entityName} where {entityIdField} = {entityId}";

            using (var cmd = DbCommandFactory.Create(query, dbConnection))
            {
                var value = cmd.ExecuteScalar();
                return (value == null || value == DBNull.Value) ? (decimal?)null : (decimal)value;
            }
        }
    }
}
