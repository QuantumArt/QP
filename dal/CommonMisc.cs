using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {

        private static string DbSchemaName(DatabaseType databaseType) => SqlQuerySyntaxHelper.DbSchemaName(databaseType);

        private static string WithNoLock(DatabaseType databaseType) => SqlQuerySyntaxHelper.WithNoLock(databaseType);

        private static string WithRowLock(DatabaseType databaseType) => SqlQuerySyntaxHelper.WithRowLock(databaseType);

        private static string Now(DatabaseType databaseType) => SqlQuerySyntaxHelper.Now(databaseType);

        private static string IsTrue(DatabaseType databaseType, string expr) => SqlQuerySyntaxHelper.IsTrue(databaseType, expr);

        private static string IsFalse(DatabaseType databaseType, string expr) => SqlQuerySyntaxHelper.IsFalse(databaseType, expr);

        private static string IdList(DatabaseType databaseType, string name, string alias = "i") => SqlQuerySyntaxHelper.IdList(databaseType, name, alias);

        private static string Escape(DatabaseType databaseType, string entityName) => SqlQuerySyntaxHelper.EscapeEntityName(databaseType, entityName);

        private static string Top(DatabaseType databaseType, int top) => SqlQuerySyntaxHelper.Top(databaseType, top.ToString());

        private static string Limit(DatabaseType databaseType, int top) => SqlQuerySyntaxHelper.Limit(databaseType, top.ToString());

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


        public static IEnumerable<DataRow> GetChildFoldersList(DbConnection sqlConnection, QPModelDataContext context, bool isAdmin, int userId, int id, bool isSite, int? folderId, int permissionLevel, bool countOnly, out int totalRecords)
        {
            totalRecords = -1;
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var entityName = isSite ? EntityTypeCode.OldSiteFolder : EntityTypeCode.ContentFolder;
            var parentEntityName = isSite ? EntityTypeCode.Site : EntityTypeCode.Content;
            var blockFilter = string.Empty;

            var useSecurity = !isAdmin;

            int parentLevel;
            if (entityName == EntityTypeCode.ContentFolder)
            {
                useSecurity = false;
                parentLevel = GetEntityAccessLevel(sqlConnection, context, userId, 0, parentEntityName, id);
                if (parentLevel == 0)
                {
                    blockFilter += " AND 1 = 0 ";
                }

            }
            else
            {
                parentLevel = folderId.HasValue
                    ? GetEntityAccessLevel(sqlConnection, context, userId, 0, EntityTypeCode.SiteFolder, folderId.Value)
                    : GetEntityAccessLevel(sqlConnection, context, userId, 0, parentEntityName, id);
            }

            var securitySql = useSecurity
                ? PermissionHelper.GetPermittedItemsAsQuery(context, userId, 0, 0, 4, entityName, parentEntityName, id)
                : string.Empty;


            var query = $@"
            SELECT
{(countOnly
                    ? "COUNT(c.FOLDER_ID) "
                    : $@"
            c.FOLDER_ID,
            c.NAME,
            c.CREATED,
            c.MODIFIED, 
            c.LAST_MODIFIED_BY,
            {SqlQuerySyntaxHelper.CastToBool(dbType, $"CASE WHEN (SELECT COUNT(FOLDER_ID) FROM {entityName} WHERE PARENT_FOLDER_ID = c.FOLDER_ID) > 0 THEN 1 ELSE 0 END")} AS HAS_CHILDREN,
            mu.{Escape(dbType, "USER_ID")} as MODIFIER_USER_ID,
            mu.FIRST_NAME as MODIFIER_FIRST_NAME,
            mu.LAST_NAME AS MODIFIER_LAST_NAME,
            mu.EMAIL AS MODIFIER_EMAIL,
            mu.{Escape(dbType, "LOGIN")} AS MODIFIER_LOGIN
            {(useSecurity
                ? $", COALESCE(pi.permission_level, {SqlQuerySyntaxHelper.CastToVarchar(dbType, parentLevel.ToString())}) as EFFECTIVE_PERMISSION_LEVEL"
                : string.Empty
                        )}
"
                )} ";

            query += $@"
    FROM {entityName} as c
    {(useSecurity
                ? $"LEFT JOIN ({securitySql}) as pi ON c.folder_id = pi.{entityName}_id"
                : string.Empty
    )}
    {(!countOnly
                ? $" LEFT OUTER JOIN USERS as mu ON mu.user_id = c.last_modified_by "
                : string.Empty)}
    WHERE
    {(folderId.HasValue
                ? $" c.parent_folder_id = '{folderId.Value}' "
                : $" c.parent_folder_id is null and c.{parentEntityName}_id = '{id}'"
    )}

    {(useSecurity
                ? $" AND coalesce(pi.permission_level, 4) >= {permissionLevel}"
                : string.Empty
    )}
";
            query += blockFilter;

            if (!countOnly)
            {
                query += "ORDER BY c.NAME ASC";
            }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                totalRecords = dt.Rows.Count;
                return dt.AsEnumerable().ToArray();
            }
        }

        private static int GetEntityAccessLevel(DbConnection sqlConnection, QPModelDataContext context, int userId, int groupId, string parentEntityName, int id)
        {
            #warning реализовать qp_entity_access_level
            return PermissionLevel.FullAccess;
        }

        // public static IEnumerable<DataRow> GetChildFoldersList(DbConnection sqlConnection, int userId, int id, bool isSite, int? folderId, int permissionLevel, bool countOnly, out int totalRecords)
        // {
        //     totalRecords = -1;
        //     using (var cmd = DbCommandFactory.Create("qp_get_folders_tree", sqlConnection))
        //     {
        //         cmd.CommandType = CommandType.StoredProcedure;
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = id });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "is_site", DbType = DbType.Boolean, Value = isSite });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly });
        //         cmd.Parameters.Add(new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords });
        //
        //         var dt = new DataTable();
        //         DataAdapterFactory.Create(cmd).Fill(dt);
        //         totalRecords = (int)cmd.Parameters["total_records"].Value;
        //         return dt.AsEnumerable().ToArray();
        //     }
        // }

        // public static string GetEntityName(DbConnection sqlConnection, string entityTypeCode, int entityId, int parentEntityId)
        // {
        //
        //     using (var cmd = DbCommandFactory.Create("qp_get_entity_title", sqlConnection))
        //     {
        //         cmd.CommandType = CommandType.StoredProcedure;
        //         cmd.Parameters.AddWithValue("entity_type_code", entityTypeCode);
        //         cmd.Parameters.AddWithValue("entity_id", entityId);
        //         cmd.Parameters.AddWithValue("parent_entity_id", parentEntityId);
        //         cmd.Parameters.Add(
        //             new SqlParameter
        //             {
        //                 ParameterName = "title",
        //                 DbType = DbType.String,
        //                 Size = 255,
        //                 Direction = ParameterDirection.Output,
        //             }
        //         );
        //         cmd.ExecuteNonQuery();
        //         return cmd.Parameters["title"].Value.ToString();
        //     }
        // }

        public static bool CheckEntityExistence(DbConnection sqlConnection, string entityTypeCode, string entitySource, string entityIdField, decimal entityId)
        {
            if (entityTypeCode == EntityTypeCode.CustomerCode) return true;

            string query;
            if (entityTypeCode == EntityTypeCode.Article || entityTypeCode == EntityTypeCode.ArchiveArticle)
            {
                var isArchive = entityTypeCode == EntityTypeCode.ArchiveArticle ? 1 : 0;
                query = $@"
                    SELECT
                    COUNT(CONTENT_ITEM_ID)
                    FROM CONTENT_ITEM
                    WHERE CONTENT_ITEM_ID = {entityId}
                    AND ARCHIVE = {isArchive}
                    ";
            }
            else if(!string.IsNullOrWhiteSpace(entitySource) && !string.IsNullOrWhiteSpace(entityIdField))
            {
                query = $@"
                    SELECT COUNT({entityIdField})
                    FROM {entitySource}
                    WHERE {entityIdField} = {entityId}
                    ";
            }
            else
            {
                return false;
            }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
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

        public static string GetStringFieldValue(DbConnection dbConnection, string fieldName, string entityName, string entityIdField, decimal entityId)
        {
            var query = $"select {fieldName} from {entityName} where {entityIdField} = {entityId}";

            using (var cmd = DbCommandFactory.Create(query, dbConnection))
            {
                var value = cmd.ExecuteScalar();
                return (value == null || value == DBNull.Value) ? null: (string)value;
            }
        }

        public static string GetArticleTitle(DbConnection dbConnection, long contentItemId, decimal contentId, string titleName, string relName, int? relContentId, string relName2, int? relContentId2)
        {
            var query = "";
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(dbConnection);

            if (!string.IsNullOrWhiteSpace(relName2))
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, relName2)} as title
FROM content_{relContentId2}_united
WHERE content_item_id in (SELECT {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, relName)} FROM content_{relContentId}_united
WHERE content_item_id in (SELECT {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, titleName)} FROM content_{contentId}_united
WHERE content_item_id = {contentItemId}))
";
            }
            else if (!string.IsNullOrWhiteSpace(relName))
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, relName)} as title
FROM content_{relContentId}_united
WHERE content_item_id in (SELECT {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, titleName)} FROM content_{contentId}_united
WHERE content_item_id = {contentItemId})
";
            }
            else
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, titleName)} as title
FROM content_{contentId}_united
WHERE content_item_id = {contentItemId}
";
            }

            using (var cmd = DbCommandFactory.Create(query, dbConnection))
            {
                var value = cmd.ExecuteScalar();
                return (value == null || value == DBNull.Value) ? null: (string)value;
            }

        }





    }
}
