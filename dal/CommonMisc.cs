using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using NpgsqlTypes;
using NLog;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using LogManager = NLog.LogManager;
using NLog.Fluent;

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
            switch (connection)
            {

                case SqlConnection _:
                    parameters = new object[] {
                        new SqlParameter { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                        new SqlParameter { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? string.Empty },
                        new SqlParameter { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                        new SqlParameter { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
                    };
                    break;
                case NpgsqlConnection _:
                    parameters = new object[] {
                        new NpgsqlParameter { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                        new NpgsqlParameter { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? string.Empty },
                        new NpgsqlParameter { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                        new NpgsqlParameter { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
                    };
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


        public static void SetContentItemVisible(DbConnection connection, int contentItemId, bool isVisible, int lastModifiedBy = 1)
        {
            var dbType = GetDbType(connection);
            var sql = $"UPDATE content_item {WithRowLock(dbType)} " +
                $"SET visible = @is_visible, modified = {Now(dbType)}, last_modified_by = @last_modified_by " +
                "WHERE content_item_id = @id";

            using (var cmd = DbCommandFactory.Create(sql, connection))
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
            var sql = dbType == DatabaseType.SqlServer ? "qp_merge_article" : "call qp_merge_articles(ARRAY[@item_id]::int[], @last_modified_by);";
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
            var siteFilter = isOnlySite ? "" : " AND c.site_id = @site_id";
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

            var createTableSql = "";
            var dropTableSql = "";
            bool useSql2012Syntax = true;
            if (dbType == DatabaseType.SqlServer)
            {
                createTableSql = $@"
                    create table #temp (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int);
                    create table #temp2 (content_item_id numeric primary key, [rank] int, attribute_id numeric, [priority] int);
                    insert into #temp
                    select content_item_id, weight, attribute_id, priority from
                    (
                        select cd.content_item_id, ft.[rank] as weight, cd.attribute_id, 0 as priority,
                        ROW_NUMBER() OVER(PARTITION BY cd.CONTENT_ITEM_ID ORDER BY [rank] desc) as number
                        from CONTAINSTABLE(content_data, *,  @searchparam) ft
                        inner join content_data cd on ft.[key] = cd.content_data_id
                    ) as c where c.number = 1 order by weight desc;
                ";
                if (articleId.HasValue)
                {
                    createTableSql += $@"
                        if not exists (select * from #temp where content_item_id = @item_id)
                        insert into #temp select cast(@item_id as varchar(20)), 0, 0, 1
                    ";
                }

                createTableSql += $@"
                    insert into #temp2
                    select cd.* from #temp cd
                    inner join content_item ci on cd.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
                    inner join content c on c.CONTENT_ID = ci.CONTENT_ID
                    {securityJoin}
                    where 1 = 1 {siteFilter};
                ";

                dropTableSql = $@"
                drop table #temp;
                drop table #temp2;
                ";

                useSql2012Syntax = GetSqlServerVersion(sqlConnection).Major >= 11;

            }

            var startPaging = dbType == DatabaseType.Postgres ? $@"select *, count(*) over() ""Count"" from ("
                : useSql2012Syntax ? "SELECT "
                : $"WITH PAGED_DATA_CTE AS (SELECT ROW_NUMBER() OVER (ORDER BY {sortExpression}) AS Row,";

            var endPaging = dbType == DatabaseType.Postgres ? $") a LIMIT {pageSize} OFFSET {startRow - 1}"
                : useSql2012Syntax ? $@"ORDER BY {sortExpression} OFFSET {startRow - 1} ROWS FETCH NEXT {pageSize} ROWS ONLY"
                : $@") select * from PAGED_DATA_CTE where Row between {startRow} and {startRow + pageSize - 1} order by Row asc;";

            var articleSql = dbType == DatabaseType.Postgres && articleId.HasValue ? $@"
                    SELECT {PgFtSelect(true)}
                    FROM content_item ci
                    {PgFtCommonJoins()}
                    WHERE ci.content_item_id = @item_id
                    UNION ALL
                "
                : "";

            var sql = (dbType == DatabaseType.SqlServer) ? $@"
                {createTableSql}
                {startPaging}
                count(*) over() as Count, data.CONTENT_ITEM_ID as Id, data.ATTRIBUTE_ID as FieldId, data.[rank] as Rank,
                ci.CREATED as Created, ci.ARCHIVE as Archive, ci.MODIFIED as Modified, ci.CONTENT_ID as ParentId,
                COALESCE(cd.BLOB_DATA, cd.DATA) as Text, dbo.qp_get_article_title_func(ci.content_item_id, ci.content_id) as Name,
            	c.CONTENT_NAME as ParentName, st.STATUS_TYPE_NAME as StatusName, usr.[LOGIN] as LastModifiedByUser

                from #temp2 data
                left join dbo.CONTENT_ATTRIBUTE attr on data.ATTRIBUTE_ID = attr.ATTRIBUTE_ID
                inner join dbo.CONTENT_ITEM ci on data.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID
                left join content_data cd on ci.content_item_id = cd.content_item_id and cd.attribute_id = attr.attribute_id
                inner join dbo.CONTENT c on c.CONTENT_ID = ci.CONTENT_ID
                inner join dbo.STATUS_TYPE st on st.STATUS_TYPE_ID = ci.STATUS_TYPE_ID
                inner join dbo.USERS usr on usr.[USER_ID] = ci.LAST_MODIFIED_BY
                {endPaging}
                {dropTableSql}
                "
                : $@"
                {startPaging}
                {articleSql}
                SELECT {PgFtSelect(false)}
                FROM content_item_ft cif
                    CROSS JOIN {GetPgFtQuery()} q
                    INNER JOIN content_item ci on cif.content_item_id = ci.content_item_id
                    {PgFtCommonJoins()}
                    {securityJoin}
                WHERE ft_data @@ q {siteFilter}
                ORDER BY {sortExpression}
                {endPaging}
            ";

            var dt = new DataTable();
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@site_id", siteId);
                cmd.Parameters.AddWithValue("@searchparam", searchString);
                if (articleId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@item_id", articleId.Value);
                }

                DataAdapterFactory.Create(cmd).Fill(dt);
                totalRecords = dt.AsEnumerable().Any() ? Convert.ToInt32(dt.Rows[0]["Count"]) : 0;
            }

            return dt;
        }

        public static Dictionary<decimal, string> GetPgFtDescriptions(DbConnection connection, int[] ids, string searchString)
        {
            var result = new Dictionary<decimal, string>();
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var headLineOptions = "StartSel=\"<span class=\"\"seachResultHighlight\"\">\", StopSel=\"</span>\"";
            var sql = $@"
                select content_item_id, content_data_id, ts_rank_cd(ft_data, q, 2) as rank,
                ts_headline('russian', coalesce(blob_data, data), q, '{headLineOptions}') as text
                from content_data, {GetPgFtQuery()} q
                WHERE ft_data @@ q and content_item_id in (select id from {IdList(dbType, "@ids")})
                order by rank desc, content_item_id asc
            ";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
                cmd.Parameters.AddWithValue("@searchParam", searchString);
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


        public static string GetPgFtQuery(string query = null)
        {
            var actualQuery = String.IsNullOrEmpty(query) ? "@searchparam" : $"'{Cleaner.ToSafeSqlString(query)}'";
            return $"websearch_to_tsquery('russian', {actualQuery})";
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
                    .Select(row => Convert.ToInt32(row["content_item_id"])).ToArray();
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

         public static string GetDbServerName(DbConnection sqlConnection, string customerCode)
         {
             var isPostgresConnection = IsPostgresConnection(sqlConnection);
             var result = "";
             var query = $"select {(isPostgresConnection ? "inet_server_addr()::text" : @"@@SERVERNAME")} as server_name";

             using (var cmd = DbCommandFactory.Create(query, sqlConnection))
             {
                 cmd.CommandType = CommandType.Text;
                 result = (string)cmd.ExecuteScalar();
             }

             if (isPostgresConnection)
             {
                 var ip = IPAddress.Parse(result.Split('/')[0]);
                 try
                 {
                     result = Dns.GetHostEntry(ip).HostName;
                 }
                 catch (Exception ex)
                 {
                     Logger.ForErrorEvent()
                         .Exception(ex)
                         .Message("Cannot resolve IP Address: {ip}", ip)
                         .Property("customerCode", customerCode)
                         .Log();
                 }
             }

             return result;
         }

         public static int[] GetArticleIdsWithWrongStatuses(DbConnection connection, int[] idsList, int[] statusList)
         {
             var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
             var sql = $@"
                select content_item_id from content_item {WithNoLock(dbType)}
                where content_item_id in (select id from {IdList(dbType, "@ids")})
                and status_type_id not in (select id from {IdList(dbType, "@status_ids")})
             ";
             using (var cmd = DbCommandFactory.Create(sql, connection))
             {
                 cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", idsList, dbType));
                 cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@status_ids", statusList, dbType));

                 return GetDataTableForCommand(cmd).AsEnumerable()
                     .Select(row => Convert.ToInt32(row["content_item_id"])).ToArray();
             }
         }

         public static void RemoveUserDependencies(DbConnection connection, int[] userIds)
         {
             var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
             var sb = new StringBuilder();
             var ids = IdList(dbType, "@user_ids");

             var deleteFormat = $"delete from {{0}} where user_id in (select id from {ids});";
             var deleteTables = new[] { "user_group_bind", "user_default_filter" };
             foreach (var table in deleteTables)
             {
                 sb.AppendFormatLine(deleteFormat, table);
             }

             var unlockFormat = $"update {{0}} set locked = null, locked_by = null where locked_by in (select id from {ids});";
             var unlockTables = new[]
             {
                 "container", "content_form", "object", "object_format", "page", "page_template", "site", "content_item"
             };
             foreach (var table in unlockTables)
             {
                 sb.AppendFormatLine(unlockFormat, table);
             }

             var setToAdminFormat = $"update {{0}} set {{1}} = 1 where {{1}} in (select id from {ids});";
             var setToAdminTablesWithFields = new Dictionary<string, string>()
             {
                 ["content_item_version"] = "created_by", ["page"] = "last_assembled_by", ["db"] = "single_user_id",
                 ["content_item_status_history"] = "user_id", ["notifications"] = "from_backenduser_id"
             };
             foreach (var table in setToAdminTablesWithFields)
             {
                 sb.AppendFormatLine(setToAdminFormat, table.Key, table.Value);
             }

             var setLastModifiedByToAdminTables = new[]
             {
                 "site", "site_access", "folder", "folder_access", "status_type", "workflow", "workflow_access",
                 "content", "content_access", "content_folder", "content_folder_access", "content_attribute",
                 "content_item", "content_item_access", "content_item_version", "content_item_schedule",
                 "users", "user_group", "style", "code_snippet", "notifications", "db",
                 "page_template", "page", "object", "object_format", "object_format_version",
                 "custom_action", "entity_type_access", "action_access", "ve_plugin", "ve_style", "ve_command"
             };
             foreach (var table in setLastModifiedByToAdminTables)
             {
                 sb.AppendFormatLine(setToAdminFormat, table, "last_modified_by");
             }

             using (var cmd = DbCommandFactory.Create(sb.ToString(), connection))
             {
                 cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@user_ids", userIds, dbType));
                 cmd.ExecuteNonQuery();
             }
         }

         public static Dictionary<int, int> GetVersionsToDelete(DbConnection connection, int[] ids)
         {
             var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
             string sql = $@"
                select content_item_version_id, content_id from (
                    select civ.content_item_version_id, civ.content_item_id, ci.content_id, c.max_num_of_stored_versions as max_num,
                    row_number() over(partition by civ.content_item_id order by civ.content_item_version_id desc) as num
                    from content_item_version civ {WithNoLock(dbType)}
                    inner join content_item ci {WithNoLock(dbType)} on ci.CONTENT_ITEM_ID = civ.CONTENT_ITEM_ID
                    inner join content c {WithNoLock(dbType)} on c.CONTENT_ID = ci.CONTENT_ID
                    where ci.content_item_id in (select id from {IdList(dbType, "@ids")})
                ) r where num >= max_num";

             using var cmd = DbCommandFactory.Create(sql, connection);
             cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
             return GetDataTableForCommand(cmd).AsEnumerable().ToDictionary(
                 k => Convert.ToInt32(k["content_item_version_id"]),
                 v => Convert.ToInt32(v["content_id"])
             );
         }

         public static Dictionary<int, int> GetLatestVersions(DbConnection connection, int[] ids)
         {
             var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
             string sql = $@"
                select content_item_version_id, content_id from (
                    select civ.content_item_version_id, civ.content_item_id, ci.content_id,
                    row_number() over(partition by civ.content_item_id order by civ.content_item_version_id desc) as num
                    from content_item_version civ {WithNoLock(dbType)}
                    inner join content_item ci {WithNoLock(dbType)} on ci.CONTENT_ITEM_ID = civ.CONTENT_ITEM_ID
                    where ci.content_item_id in (select id from {IdList(dbType, "@ids")})
                ) r where num = 1";

             using var cmd = DbCommandFactory.Create(sql, connection);
             cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
             return GetDataTableForCommand(cmd).AsEnumerable().ToDictionary(
                 k => Convert.ToInt32(k["content_item_version_id"]),
                 v => Convert.ToInt32(v["content_id"])
             );
         }

         public static Dictionary<int, Dictionary<string, int>> GetFilesForVersions(DbConnection connection, int[] ids)
         {
             var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
             var result = new Dictionary<int, Dictionary<string, int>>();
             string sql = $@"
                select vcd.data, ca.content_id, civ.content_item_version_id
                from content_item_version civ {WithNoLock(dbType)}
                inner join version_content_data vcd {WithNoLock(dbType)} on vcd.content_item_version_id = civ.content_item_version_id
                inner join content_attribute ca {WithNoLock(dbType)} on ca.attribute_id = vcd.attribute_id
                where civ.content_item_version_id in (select id from {IdList(dbType, "@ids")})
                and vcd.data is not null and ca.attribute_type_id in (7, 8)";

             using var cmd = DbCommandFactory.Create(sql, connection);
             cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
             var rows = GetDataTableForCommand(cmd).AsEnumerable();
             foreach (var row in rows)
             {
                 var id = Convert.ToInt32(row["content_item_version_id"]);
                 var contentId = Convert.ToInt32(row["content_id"]);
                 if (!result.ContainsKey(id))
                 {
                     result.Add(id, new Dictionary<string, int>());
                 }
                 result[id].Add(row["data"].ToString() ?? "", contentId);
             }
             return result;
         }
    }
}
