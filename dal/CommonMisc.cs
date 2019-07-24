using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using NpgsqlTypes;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {

        internal static string DbSchemaName(DatabaseType databaseType) => SqlQuerySyntaxHelper.DbSchemaName(databaseType);

        internal static string WithNoLock(DatabaseType databaseType) => SqlQuerySyntaxHelper.WithNoLock(databaseType);

        internal static string WithRowLock(DatabaseType databaseType) => SqlQuerySyntaxHelper.WithRowLock(databaseType);

        internal static string Now(DatabaseType databaseType) => SqlQuerySyntaxHelper.Now(databaseType);

        internal static string IsTrue(DatabaseType databaseType, string expr) => SqlQuerySyntaxHelper.IsTrue(databaseType, expr);

        internal static string IsFalse(DatabaseType databaseType, string expr) => SqlQuerySyntaxHelper.IsFalse(databaseType, expr);

        internal static string IdList(DatabaseType databaseType, string name, string alias = "i") => SqlQuerySyntaxHelper.IdList(databaseType, name, alias);

        internal static string Escape(DatabaseType databaseType, string entityName) => SqlQuerySyntaxHelper.EscapeEntityName(databaseType, entityName);

        internal static string Top(DatabaseType databaseType, int top) => SqlQuerySyntaxHelper.Top(databaseType, top.ToString());

        internal static string Limit(DatabaseType databaseType, int top) => SqlQuerySyntaxHelper.Limit(databaseType, top.ToString());

        internal static DbParameter GetIdsDatatableParam(string paramName, IEnumerable<int> ids, DatabaseType databaseType = DatabaseType.SqlServer) => SqlQuerySyntaxHelper.GetIdsDatatableParam(paramName, ids, databaseType);

        private static DbParameter GetIntArrayPostgresParam(string paramName, List<int> ints) => new NpgsqlParameter(paramName, NpgsqlDbType.Array | NpgsqlDbType.Integer)
        {
            Value = ints.ToArray()
        };


        private static DatabaseType GetDbType(DbConnection sqlConnection) => DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
        private static DatabaseType GetDbType(QPModelDataContext context) => DatabaseTypeHelper.ResolveDatabaseType(context);

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
            var entityTypeName = isSite ? EntityTypeCode.OldSiteFolder : EntityTypeCode.ContentFolder;
            var parentEntityTypeName = isSite ? EntityTypeCode.Site : EntityTypeCode.Content;
            var blockFilter = string.Empty;

            var useSecurity = !isAdmin;

            int parentLevel;
            if (entityTypeName == EntityTypeCode.ContentFolder)
            {
                useSecurity = false;
                parentLevel = CommonSecurity.GetEntityAccessLevel(sqlConnection, context, userId, 0, parentEntityTypeName, id);
                if (parentLevel == 0)
                {
                    blockFilter += " AND 1 = 0 ";
                }

            }
            else
            {
                parentLevel = folderId.HasValue
                    ? CommonSecurity.GetEntityAccessLevel(sqlConnection, context, userId, 0, EntityTypeCode.SiteFolder, folderId.Value)
                    : CommonSecurity.GetEntityAccessLevel(sqlConnection, context, userId, 0, parentEntityTypeName, id);
            }

            var securitySql = useSecurity ? PermissionHelper.GetPermittedItemsAsQuery(
                    context, userId, 0, PermissionLevel.Deny, PermissionLevel.FullAccess,
                    entityTypeName, parentEntityTypeName, id
                ) : string.Empty;

            var childrenParam = SqlQuerySyntaxHelper.CastToBool(dbType,
                $@"
                    CASE WHEN (
                        SELECT COUNT(FOLDER_ID) FROM {entityTypeName} WHERE PARENT_FOLDER_ID = c.FOLDER_ID
                    ) > 0 THEN 1 ELSE 0 END
            ");
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
            {childrenParam} AS HAS_CHILDREN,
            mu.{Escape(dbType, "USER_ID")} as MODIFIER_USER_ID,
            mu.FIRST_NAME as MODIFIER_FIRST_NAME,
            mu.LAST_NAME AS MODIFIER_LAST_NAME,
            mu.EMAIL AS MODIFIER_EMAIL,
            mu.{Escape(dbType, "LOGIN")} AS MODIFIER_LOGIN
            {(useSecurity
                ? $", COALESCE(pi.permission_level, {parentLevel}) as EFFECTIVE_PERMISSION_LEVEL"
                : string.Empty
                        )}
"
                )} ";

            query += $@"
    FROM {entityTypeName} as c
    {(useSecurity
                ? $"LEFT JOIN ({securitySql}) as pi ON c.folder_id = pi.{entityTypeName}_id"
                : string.Empty
    )}
    {(!countOnly
                ? $" LEFT OUTER JOIN USERS as mu ON mu.user_id = c.last_modified_by "
                : string.Empty)}
    WHERE
    {(folderId.HasValue
                ? $" c.parent_folder_id = '{folderId.Value}' "
                : $" c.parent_folder_id is null and c.{parentEntityTypeName}_id = '{id}'"
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

            return GetDataTableForQuery(sqlConnection, query).AsEnumerable().ToArray();
        }


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
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = dbType == DatabaseType.SqlServer ? "qp_merge_article" : "call qp_merge_article(@item_id, last_modified_by);";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.AddWithValue("@item_id", contentItemId);
                cmd.Parameters.AddWithValue("@last_modified_by", lastModifiedBy);
                cmd.ExecuteNonQuery();
            }
        }

        private static string PgFtSelect(bool forId)
        {
            var rank = (forId) ? "1" : "ts_rank_cd(cif.ft_data, q, 2)";
            return  $@"
                c.content_name ""ParentName"", ci.content_id ""ParentId"", ci.content_item_id ""Id"",
                0 ""FieldId"", '' ""Text"", ci.created ""Created"", ci.modified ""Modified"", ci.archive ""Archive"",
                {rank} ""Rank"", st.status_type_name ""StatusName"", u.login ""LastModifiedByUser"",
                qp_get_article_title_func(ci.content_item_id, ci.content_id) ""Name""
            ";
        }

        private static string PgFtCommonJoins()
        {
            return @"
                INNER JOIN status_type st on ci.status_type_id = st.status_type_id
                INNER JOIN users u on ci.last_modified_by = u.user_id
                INNER JOIN content c on c.content_id = ci.content_id
            ";
        }

        public static DataTable SearchInArticles(QPModelDataContext context, DbConnection sqlConnection, int siteId, int userId, string searchString, int? articleId, string sortExpression, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var isOnlySite = context.SiteSet.Count() == 1;
            var siteFilter = isOnlySite ? "" : " AND c.site_id = @p_site_id";
            var isAdmin = IsAdmin(sqlConnection, userId);
            var securityJoin = "";
            if (!isAdmin)
            {
                if (articleId.HasValue)
                {
                    var level = CommonSecurity.GetEntityAccessLevel(sqlConnection, context, userId, 0, EntityTypeCode.Article, articleId.Value);
                    if (level < PermissionLevel.List)
                    {
                        articleId = null;
                    }
                }
                var securitySql = GetPermittedItemsAsQuery(sqlConnection, userId, 0, PermissionLevel.List, PermissionLevel.FullAccess, EntityTypeCode.Content, EntityTypeCode.Site, siteId);
                securityJoin = $"INNER JOIN ({securitySql}) sec on sec.content_id = ci.content_id ";
            }

            var safeSearchString = Cleaner.ToSafeSqlString(searchString);
            totalRecords = -1;
            var sql = (dbType == DatabaseType.SqlServer) ? "qp_all_article_search" :
                $@"SELECT {PgFtSelect(false)}
            FROM content_item_ft cif
                CROSS JOIN {GetPgFtQuery(safeSearchString)} q
                INNER JOIN content_item ci on cif.content_item_id = ci.content_item_id
                {PgFtCommonJoins()}
                {securityJoin}
            WHERE ft_data @@ q {siteFilter}
            ORDER BY {sortExpression} OFFSET {startRow - 1} LIMIT {pageSize};";

            if (articleId.HasValue)
            {
                sql = $@"
                    SELECT {PgFtSelect(true)}
                    FROM content_item ci
                    {PgFtCommonJoins()}
                    WHERE ci.content_item_id = {articleId.Value}
                    UNION ALL
                " + sql;
            }

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@p_site_id", siteId);
                cmd.Parameters.AddWithValue("@p_user_id", userId);
                cmd.Parameters.AddWithValue("@p_item_id", articleId);
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_searchparam", safeSearchString);
                    cmd.Parameters.AddWithValue("@p_order_by", sortExpression);
                    cmd.Parameters.AddWithValue("@p_start_row", startRow);
                    cmd.Parameters.AddWithValue("@p_page_size", pageSize);
                    cmd.Parameters.Add(new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords });
                }

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                if (dbType == DatabaseType.SqlServer)
                {
                    totalRecords = (int)cmd.Parameters["total_records"].Value;
                }
                else if (dbType == DatabaseType.Postgres)
                {
                    totalRecords = GetPgSearchInArticlesCount(sqlConnection, safeSearchString, articleId, securityJoin);
                }

                return dt;
            }
        }

        public static Dictionary<decimal, string> GetPgFtDescriptions(DbConnection connection, int[] ids, string searchString)
        {
            var result = new Dictionary<decimal, string>();
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var headLineOptions = "StartSel=\"<span class=\"\"seachResultHighlight\"\">\", StopSel=\"</span>\"";
            var sql = $@"
                select content_item_id, content_data_id, ts_rank_cd(ft_data, q, 2) as rank,
                ts_headline('russian', coalesce(blob_data, data), q, '{headLineOptions}') as text
                from content_data, {GetPgFtQuery(Cleaner.ToSafeSqlString(searchString))} q
                WHERE ft_data @@ q and content_item_id in (select id from {IdList(dbType, "@ids")})
                order by rank desc, content_item_id asc
            ";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                foreach (var row in dt.AsEnumerable())
                {
                    var id = row.Field<decimal>("content_item_id");
                    if (!result.ContainsKey(id))
                    {
                        result.Add(id, row.Field<string>("text"));
                    }
                }

                return result;
            }
        }


        public static string GetPgFtQuery(string searchString)
        {
            return $"websearch_to_tsquery('russian', '{searchString}')";
        }

        public static int GetPgSearchInArticlesCount(DbConnection connection, string searchString, int? articleId, string securityJoin)
        {
            var sql = $@"
                SELECT count(*) FROM content_item_ft cif
                CROSS JOIN {GetPgFtQuery(searchString)} query
                inner join content_item ci on ci.content_item_id = cif.content_item_id
                {securityJoin}
                WHERE cif.ft_data @@ query";
            var result = (int)ExecuteScalarLong(connection, sql);
            if (articleId.HasValue)
            {
                var sql2 = $@"
                    SELECT count(*) FROM content_item ci
                    where ci.content_item_id = " + articleId.Value;
                result += (int)ExecuteScalarLong(connection, sql2);
            }

            return result;
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
                return (value == null || value == DBNull.Value) ? null: Convert.ToString(value);
            }
        }

        public static string GetArticleTitle(DbConnection dbConnection, long contentItemId, decimal contentId, string titleName, string relName, int? relContentId, string relName2, int? relContentId2)
        {
            var query = "";
            var databaseType = DatabaseTypeHelper.ResolveDatabaseType(dbConnection);

            if (!string.IsNullOrWhiteSpace(relName2))
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, Escape(databaseType, relName2))} as title
FROM content_{relContentId2}_united
WHERE content_item_id in (SELECT {Escape(databaseType, relName)} FROM content_{relContentId}_united
WHERE content_item_id in (SELECT {Escape(databaseType, titleName)} FROM content_{contentId}_united
WHERE content_item_id = {contentItemId}))
";
            }
            else if (!string.IsNullOrWhiteSpace(relName))
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, Escape(databaseType, relName))} as title
FROM content_{relContentId}_united
WHERE content_item_id in (SELECT {Escape(databaseType, titleName)} FROM content_{contentId}_united
WHERE content_item_id = {contentItemId})
";
            }
            else
            {
                query = $@"
SELECT {SqlQuerySyntaxHelper.CastToString(databaseType, Escape(databaseType, titleName))} as title
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

        public static void UpdateM2MValues(DbConnection sqlConnection, string xmlParameter)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = dbType == DatabaseType.SqlServer ? "qp_update_m2m_values" : "call qp_update_m2m_values(@xmlParameter);";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xmlParameter", xmlParameter, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static int[] CheckArchiveArticle(DbConnection connection, int[] ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = $@"select content_item_id from content_item {WithNoLock(dbType)} where content_item_id in (select id from {IdList(dbType, "@ids")}) and archive = 1";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
                return GetDataTableForCommand(cmd).AsEnumerable()
                    .Select(row => (int)(decimal)row["content_item_id"]).ToArray();
            }
        }

         public static IEnumerable<DataRow> GetToolbarButtonsForAction(
             QPModelDataContext context, DbConnection sqlConnection, int userId, bool isAdmin,
             int actionId, string entityTypeCode, int entityId)
        {

            var databaseType = GetDbType(context);
            var useSecurity = !isAdmin;
            var level = CommonSecurity.GetEntityAccessLevel(sqlConnection, context, userId, 0, entityTypeCode, entityId);
            var least = SqlQuerySyntaxHelper.Least(databaseType, "SEC.PERMISSION_LEVEL", level.ToString());
            var query = $@"
        SELECT
			ba.ID AS ACTION_ID, ba.CODE AS ACTION_CODE, bat.CODE AS ACTION_TYPE_CODE, ba2.ID AS PARENT_ACTION_ID,
			ba2.CODE AS PARENT_ACTION_CODE, atb.NAME AS NAME, bat.ITEMS_AFFECTED,
			atb.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "ORDER")},
			COALESCE(ca.ICON_URL, atb.ICON) AS ICON, atb.ICON_DISABLED,atb.IS_COMMAND
		FROM
			ACTION_TOOLBAR_BUTTON AS atb
			INNER JOIN BACKEND_ACTION AS ba ON atb.ACTION_ID = ba.ID
			LEFT OUTER JOIN CUSTOM_ACTION AS ca ON ca.ACTION_ID = ba.ID
			INNER JOIN ACTION_TYPE AS bat ON bat.ID = ba.TYPE_ID
            {(!useSecurity ? string.Empty : "INNER JOIN PERMISSION_LEVEL PL ON PL.PERMISSION_LEVEL_ID = bat.REQUIRED_PERMISSION_LEVEL_ID")}
			INNER JOIN BACKEND_ACTION AS ba2 ON atb.PARENT_ACTION_ID = ba2.ID
            {(!useSecurity ? string.Empty : $"INNER JOIN ({PermissionHelper.GetActionPermissionsAsQuery(context, userId)}) SEC ON SEC.BACKEND_ACTION_ID = ba.ID")}

		WHERE
			atb.PARENT_ACTION_ID = {actionId}
            {(!useSecurity ? string.Empty : $"AND ({least} >= PL.PERMISSION_LEVEL or bat.CODE = 'refresh')")}
		ORDER BY
			{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "ORDER")}
";
            return GetDataTableForQuery(sqlConnection, query);
        }

         public static IEnumerable<DataRow> GetDataTableForQuery(DbConnection sqlConnection, string query)
         {
             using (var cmd = DbCommandFactory.Create(query, sqlConnection))
             {
                 return GetDataTableForCommand(cmd);
             }
         }

         public static IEnumerable<DataRow> GetDataTableForCommand(DbCommand cmd)
         {
             var dt = new DataTable();
             DataAdapterFactory.Create(cmd).Fill(dt);
             return dt.AsEnumerable().ToArray();
         }
    }
}
