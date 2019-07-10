using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {
        public static long GetContentIdForArticle(DbConnection connection, long id)
        {
            var dbType = GetDbType(connection);
            using (var cmd = DbCommandFactory.Create($"select content_id from content_item {WithNoLock(dbType)} where content_item_id = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return value == null ? 0 : long.Parse(value.ToString());
            }
        }

        /* public static string GetTitleName(DbConnection connection, long contentId)
         {

             var displayFieldsQuery = GetDisplayFieldsQuery(connection, contentId, true);
             var isPostgres = IsPostgresConnection(connection);

             var query = $@"
 select
 case when attribute_name is not null then attribute_name else 'content_item_id' end
 from (
     select {(isPostgres ? string.Empty : "TOP 1")} attribute_name
     from ({displayFieldsQuery}) as df
     order by view_in_list desc, attribute_priority desc, attribute_order asc
     {(isPostgres ? "LIMIT 1" : string.Empty)}
 ) as a
 ";


             using (var cmd = DbCommandFactory.Create(query, connection))
             {
                 cmd.CommandType = CommandType.Text;
                 // cmd.Parameters.AddWithValue("@id", contentId);
                 return cmd.ExecuteScalar().ToString();
             }
         }

         public static DataTable GetDisplayFields(DbConnection connection, long contentId, bool withRelations = false)
         {
             var query = GetDisplayFieldsQuery(connection, contentId, withRelations);

             using (var cmd = DbCommandFactory.Create(query, connection))
             {
                 cmd.CommandType = CommandType.Text;
                 // cmd.Parameters.AddWithValue("@contentId", contentId);
                 // cmd.Parameters.AddWithValue("@with_relation_field", withRelations);
                 var ds = new DataSet();
                 DataAdapterFactory.Create(cmd).Fill(ds);
                 return 0 == ds.Tables.Count ? null : ds.Tables[0];
             }
         }

         private static string GetDisplayFieldsQuery(DbConnection connection, long contentId, bool withRelations)
         {

             return $@"
 select *
 from (
         SELECT attribute_id, attribute_name,
         CASE
             WHEN attribute_type_id in (9, 10)
                 THEN {(withRelations ? "1" : "0")}
             WHEN attribute_type_id = 13 or is_classifier = 1 or
                  attribute_type_id = 11 AND {(!withRelations ? "1=1" : "1=0")} THEN -1
             ELSE 1
             END AS attribute_priority,
         view_in_list,
         attribute_order
         FROM content_attribute ca
         WHERE content_id = {contentId}
     ) as c
 where attribute_priority >= 0
 ";
         }*/

        public static string GetFieldName(DbConnection connection, int fieldId)
        {
            using (var cmd = DbCommandFactory.Create("select ATTRIBUTE_NAME from CONTENT_ATTRIBUTE WHERE ATTRIBUTE_ID = @fieldId", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string[] GetArticleFieldValues(DbConnection connection, int[] ids, int contentId, string name)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var values = new List<string>();
            var sql = $@"
                select {SqlQuerySyntaxHelper.EscapeEntityName(dbType, name)} from content_{contentId}_united c {WithNoLock(dbType)}
                inner join {SqlQuerySyntaxHelper.IdList(dbType, "@ids", "i")} ON c.content_item_id = i.id
                ";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
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

        public static string GetArticleFieldValue(DbConnection connection, int id, int contentId, string name)
        {
            var dbType = GetDbType(connection);
            using (var cmd = DbCommandFactory.Create($"select {Escape(dbType, name)} from content_{contentId}_united {WithNoLock(dbType)} where content_item_id = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return value?.ToString() ?? string.Empty;
            }
        }

        public static Dictionary<int, string> GetContentFieldValues(DbConnection connection, int contentId, string name)
        {
            var values = new Dictionary<int, string>();
            var databaseType = GetDbType(connection);
            var escapedNameColumn = SqlQuerySyntaxHelper.EscapeEntityName(databaseType, name);
            using (var cmd = DbCommandFactory.Create($"select content_item_id, {escapedNameColumn} from content_{contentId}_united {WithNoLock(databaseType)} where {escapedNameColumn} is not null", connection))
            {
                cmd.CommandType = CommandType.Text;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        values.Add((int)(decimal)dr[0], dr[1]?.ToString() ?? string.Empty);
                    }
                }

                return values;
            }
        }

        public static int GetArticleIdByFieldValue(DbConnection connection, int contentId, string name, string value)
        {
            var databaseType = GetDbType(connection);
            using (var cmd = DbCommandFactory.Create($"select content_item_id from content_{contentId}_united {WithNoLock(databaseType)} where {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, name)} = @value", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@value", value);
                var result = cmd.ExecuteScalar();
                return result == null ? 0 : (int)(decimal)result;
            }
        }

        public static DataRow GetArticleRow(DbConnection connection, int id, int contentId, bool isLive, bool excludeArchive = false)
        {
            var databaseType = GetDbType(connection);
            var suffix = isLive ? string.Empty : "_united";
            var isExcludeArchive = excludeArchive ? $"and archive = {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)}" : string.Empty;
            using (var cmd = DbCommandFactory.Create($"select * from content_{contentId}{suffix} {WithNoLock(databaseType)} where content_item_id = @id {isExcludeArchive}", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return 0 == dt.Rows.Count ? null : dt.Rows[0];
            }
        }

        public static DataTable GetArticleTable(DbConnection connection, IEnumerable<int> ids, int contentId, bool isVirtual, bool isLive, bool excludeArchive = false, string filter = "", bool returnOnlyIds = false)
        {
            var dbType = GetDbType(connection);
            var fields = returnOnlyIds ? "c.content_item_id" : "c.*, ci.locked_by, ci.splitted, ci.schedule_new_version_publication";
            var baseSql = $"select {fields} from content_{contentId}{{0}} c {WithNoLock(dbType)}" +
                $" left join content_item ci {WithNoLock(dbType)} on c.content_item_id = ci.content_item_id {{1}} {{2}}";


            var conditions = new List<string>();

            if (ids != null)
            {
                conditions.Add($"c.content_item_id in (select id from {IdList(dbType, "@itemIds")})");
            }

            if (excludeArchive)
            {
                conditions.Add("c.archive = 0");
            }

            if (!string.IsNullOrEmpty(filter))
            {
                conditions.Add(filter);
            }

            if (!conditions.Any())
            {
                conditions.Add("1 = 1");
            }

            var where = " where " + string.Join(" and ", conditions);
            var sql = "";

            if (ids != null && !isLive && !isVirtual && !returnOnlyIds) //optimization for list of ids
            {
                var sb = new StringBuilder();
                var splitted = $"coalesce(ci.splitted, {SqlQuerySyntaxHelper.ToBoolSql(dbType, false)})";

                sb.AppendLine(string.Format(baseSql, string.Empty, where, $" and {IsFalse(dbType, splitted)} "));
                sb.AppendLine(" union all ");
                sb.AppendLine(string.Format(baseSql, "_async", where, $" and {IsTrue(dbType, "ci.splitted")} "));
                sql = sb.ToString();
            }
            else
            {
                sql = string.Format(baseSql, isLive ? string.Empty : "_united", where, "");
            }

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                if (ids != null)
                {
                    cmd.Parameters.Add(GetIdsDatatableParam("@itemIds", ids, dbType));
                }

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.Rows.Count == 0 ? null : dt;
            }
        }

        public static IList<DataRow> GetArticlesSimpleList(
            QPModelDataContext efContext,
            DbConnection cn,
            int userId,
            int contentId,
            string displayExpression,
            ListSelectionMode selectionMode,
            int permissionLevel,
            string filter,
            bool useSecurity,
            IList<int> selectedArticleIds,
            IList<int> idsToFilter,
            int? searchLimit = null,
            string extraSelect = "",
            string extraFrom = "",
            string orderBy = "")
        {
            var databaseType = GetDbType(cn);
            string securityJoin = "";
            if (useSecurity)
            {
                var securitySql = PermissionHelper.GetPermittedItemsAsQuery(
                    efContext, userId, 0, PermissionLevel.List, PermissionLevel.FullAccess, EntityTypeCode.OldArticle, EntityTypeCode.Content, contentId
                );
                securityJoin = $"INNER JOIN (({securitySql}) as pi ON c.content_item_id = pi.content_item_id)";

            }
            var top = searchLimit.HasValue ? Top(databaseType, searchLimit.Value) : String.Empty;
            var limit = searchLimit.HasValue ? Limit(databaseType, searchLimit.Value) : String.Empty;
            var whereStmt = string.IsNullOrWhiteSpace(filter) ? string.Empty : $" WHERE {filter}";
            var actualOrderBy = string.IsNullOrWhiteSpace(orderBy) ? "c.content_item_id ASC" : orderBy;
            var isSelectedExpression = SqlQuerySyntaxHelper.CastToBool(databaseType, "CASE WHEN (cis.content_item_id IS NOT NULL) THEN 1 ELSE 0 END");
            var selectionJoin = selectionMode == ListSelectionMode.AllItems ? " LEFT JOIN " : " INNER JOIN ";

            var query = $@"
                SELECT {top}
                c.content_item_id AS id, {displayExpression}, {isSelectedExpression} AS is_selected
                {extraSelect}
                FROM content_{contentId}_united c
                {securityJoin}
                {selectionJoin}
                ( SELECT content_item_id FROM content_{contentId} WHERE content_item_id IN (select id from {IdList(databaseType, "@myData")})) AS cis ON c.content_item_id = cis.content_item_id
                {extraFrom}
                {whereStmt}
                ORDER BY {actualOrderBy}
                {limit}
            ";
            return GetDatatableResult(cn, query, GetIdsDatatableParam("@ids", idsToFilter, databaseType), GetIdsDatatableParam("@myData", selectedArticleIds, databaseType));
        }

        public static void ExecuteSql(DbConnection connection, string sqlString, List<DbParameter> parameters, string returnIdParamName, out int id)
        {
            id = 0;
            using (var cmd = DbCommandFactory.Create(sqlString, connection))
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

        public static void ExecuteSql(DbConnection connection, string sqlString)
        {
            using (var cmd = DbCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetDataRows(DbConnection connection, string sqlString)
        {
            using (var cmd = DbCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static long ExecuteScalarLong(DbConnection connection, string sqlString)
        {
            using (var cmd = DbCommandFactory.Create(sqlString, connection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return Convert.ToInt64(result);
            }
        }

        public static DataRow GetArticleVersionRow(QPModelDataContext context, DbConnection connection, int id, int versionId)
        {
            var dbType = GetDbType(connection);

            var query = "qp_get_versions";

            if (dbType == DatabaseType.Postgres)
            {
                var contentId = context.ArticleSet.FirstOrDefault(x => x.Id == id)?.ContentId;
                if (contentId == null)
                {
                    return null;
                }

                var fieldIds = context.FieldSet.Where(x => x.ContentId == contentId).Select(x => x.Id).ToList();

                var mainIds = context.FieldSet.Where(x => x.Aggregated && x.RelationId.HasValue && fieldIds.Contains(x.RelationId.Value)).Select(x => x.ContentId).ToList();
                mainIds.Add(contentId.Value);

                var fields = context
                    .FieldSet
                    .AsNoTracking()
                    .Include(x => x.Content)
                    .Where(x => mainIds.Contains(x.ContentId))
                    .OrderBy(x => x.ContentId)
                    .ThenBy(x => x.Order);

                var fieldNames = fields.Select(n => PgFieldName(n, contentId)).ToArray();
                var fieldSelects = string.Join(",", fields.Select(n => $"ct.\"{PgFieldName(n, contentId)}\"::{PgSelectType((int)n.TypeId)}"));
                var fieldNameResults = string.Join(",", fieldNames.Select(n => $@"""{n}"" TEXT"));
                var categorySelectQuery = $"select * from (values {string.Join(",", fieldNames.Select(fn => $"(''{fn}'')"))}) v";


                query = $@"
select civ.content_item_id, ct.version_id, civ.created, civ.created_by, civ.modified, civ.last_modified_by, {fieldSelects}
from content_item_version civ
inner join (
    select * from crosstab(
    '
select vcd.content_item_version_id::numeric as version_id,
    case ca.content_id when {contentId} then ca.attribute_name else c.content_name || ''.'' || ca.attribute_name end as attribute_name,
    qp_get_version_data(vcd.attribute_id, vcd.content_item_version_id) as value
from content_attribute ca
inner join content c on ca.content_id = c.content_id
left outer join version_content_data vcd on ca.attribute_id = vcd.attribute_id
inner join content_item_version civ on vcd.content_item_version_id = civ.content_item_version_id
where ca.content_id in ({string.Join(",", mainIds)}) and civ.content_item_id = {id} {(versionId == 0 ? string.Empty : $" and vcd.content_item_version_id = {versionId} ")}
order by 1, 2
', '{categorySelectQuery}'
    ) as ct(version_id numeric, {fieldNameResults})
) ct on civ.content_item_version_id = ct.version_id

";
            }

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                if (dbType == DatabaseType.Postgres)
                {
                    cmd.CommandType = CommandType.Text;
                }
                else
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@item_id", id);
                    cmd.Parameters.AddWithValue("@version_id", versionId);
                }

                var ds = new DataSet();
                DataAdapterFactory.Create(cmd).Fill(ds);
                return 0 == ds.Tables.Count || 0 == ds.Tables[0].Rows.Count ? null : ds.Tables[0].Rows[0];
            }
        }

        private static string PgFieldName(FieldDAL fieldDAL, decimal? contentId) => fieldDAL.ContentId == contentId ? $"{fieldDAL.Name}" : $"{fieldDAL.Content.Name}.{fieldDAL.Name}";

        public static void RestoreArticleVersion(DbConnection connection, int userId, int id)
        {
            using (var cmd = DbCommandFactory.Create("restore_content_item_version", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@version_id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // public static bool CheckUnique(DbConnection connection, string code, string name, int id, int parentId = 0, int? recurringId = null)
        // {
        //     using (var cmd = DbCommandFactory.Create("qp_is_entity_exists", connection))
        //     {
        //         cmd.CommandType = CommandType.StoredProcedure;
        //         cmd.Parameters.AddWithValue("@code", code);
        //         cmd.Parameters.AddWithValue("@name", name);
        //         cmd.Parameters.AddWithValue("@id", id);
        //         cmd.Parameters.AddWithValue("@parent_id", parentId);
        //         cmd.Parameters.AddWithValue("@recurring_id", recurringId ?? 0);
        //         return (bool)cmd.ExecuteScalar();
        //     }
        // }

        public static bool CheckUnique(DbConnection connection, string source, string titleField, string parentIdField, string idField, string recurringIdField, string name, int id, int? parentId, int? recurringId)
        {
            if (parentId == 0) parentId = null;
            if (recurringId == 0) recurringId = null;
            var condition = "";
            if (parentId != null && !string.IsNullOrWhiteSpace(parentIdField))
            {
                condition += $" and {parentIdField} = {parentId}";
            }

            if (recurringId != null && !string.IsNullOrWhiteSpace(recurringIdField))
            {
                condition += $" and {recurringIdField} = {recurringId}";
            }

            var query = $"select COUNT({idField}) from {source} where {titleField} = '{name}' and {idField} <> {id} {condition}";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                var count = Convert.ToInt32(result);
                return count > 0;
            }

        }

        public static int CountDuplicates(DbConnection connection, int contentId, int[] fieldIds, int[] itemIds)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = dbType == DatabaseType.SqlServer ? "qp_count_duplicates" : "select qp_count_duplicates(@content_id, @field_ids, @ids);";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.Parameters.AddWithValue("@field_ids", Converter.ToIdCommaList(fieldIds));
                    cmd.Parameters.AddWithValue("@ids", Converter.ToIdCommaList(itemIds));
                }
                else
                {
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@field_ids", fieldIds, dbType));
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", itemIds, dbType));
                }
                return (int)cmd.ExecuteScalar();
            }
        }

        // public static DateTime? Lock(DbConnection connection, string code, int id, int? userId)
        // {
        //     using (var cmd = DbCommandFactory.Create("qp_lock", connection))
        //     {
        //         cmd.CommandType = CommandType.StoredProcedure;
        //         cmd.Parameters.AddWithValue("@code", code);
        //         cmd.Parameters.AddWithValue("@id", id);
        //         cmd.Parameters.AddWithValue("@user_id", userId);
        //         var result = cmd.ExecuteScalar();
        //         return result == DBNull.Value ? null : (DateTime?)result;
        //     }
        // }

        public static DateTime? Lock(DbConnection connection, string source, string idField, int id, int? userId)
        {
            var databaseType = GetDbType(connection);
            var lockedValue = (userId.HasValue ? SqlQuerySyntaxHelper.Now(databaseType) : "NULL");
            var query = $@"
                {(databaseType == DatabaseType.SqlServer
                ? $@"
                    declare @locked datetime
                    set @locked = {lockedValue}
                    "
                : string.Empty)}

                UPDATE {source}
                SET
                    locked_by = {SqlQuerySyntaxHelper.NullableDbValue(databaseType, userId)},
                    locked = {(databaseType == DatabaseType.Postgres ? lockedValue : "@locked")},
                    permanent_lock = {SqlQuerySyntaxHelper.ToBoolSql(databaseType, false)}
                WHERE
                    {idField} = @id
                {(databaseType == DatabaseType.Postgres
                    ? SqlQuerySyntaxHelper.Returning(databaseType, "locked")
                    : "select @locked")}
                ";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("@user_id", userId.HasValue ? userId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);

                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? null : (DateTime?)result;
            }

        }

        public static void UpdatePassword(DbConnection connection, int userId, string password)
        {
            using (var cmd = DbCommandFactory.Create("update users set password = @password where user_id = @user_id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }


        private static string IdCommaList(DataTable dt, string fieldName, bool withSpace = true)
        {
            IEnumerable<string> ids = dt.AsEnumerable().Select(row => row[fieldName].ToString());
            return withSpace ? string.Join(", ", ids) : string.Join(",", ids);
        }

        private static Dictionary<int, string> IdCommaListDictionary(DataTable dt, string keyFieldName, string valueFieldName, bool withSpace = true, IEnumerable<int> defaultKeys = null)
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
                {
                    result.Add(item.Id, new List<string> { item.LinkedId.ToString() });
                }
                else
                {
                    result[item.Id].Add(item.LinkedId.ToString());
                }
            }

            if (defaultKeys != null)
            {
                foreach (var key in defaultKeys)
                {
                    if (!result.ContainsKey(key))
                        result.Add(key, new List<string>());
                }
            }

            return result.Select(
                    n => new KeyValuePair<int, string>(n.Key, string.Join(withSpace ? ", " : ",", n.Value)))
                .ToDictionary(n => n.Key, n => n.Value);
        }


        private static Dictionary<int, Dictionary<int, List<int>>> GroupedLinks(
            DataTable dt, string keyFieldName, string valueFieldName, string groupFieldName, IEnumerable<int> groupIds)
        {
            var result = groupIds.Select(n => new { Id = n, Dict = new Dictionary<int, List<int>>() }).ToDictionary(n => n.Id, m => m.Dict);

            var data =
                dt.AsEnumerable()
                    .Select(
                        row =>
                            new
                            {
                                Id = (int)(decimal)row[keyFieldName],
                                LinkedId = (int)(decimal)row[valueFieldName],
                                LinkId = (int)(decimal)row[groupFieldName]
                            });

            foreach (var item in data)
            {
                var group = result[item.LinkId];

                if (!group.ContainsKey(item.Id))
                {
                    group.Add(item.Id, new List<int> { item.LinkedId });
                }
                else
                {
                    group[item.Id].Add(item.LinkedId);
                }
            }

            return result;
        }

        public static Dictionary<int, string> GetDefaultLinkedArticles(DbConnection connection, IEnumerable<int> linkIds)
        {
            if (!linkIds.Any())
            {
                return new Dictionary<int, string>();
            }
            else
            {
                var databaseType = GetDbType(connection);
                var query = $@"
                    select article_id, link_id
                    from field_article_bind f inner join content_attribute ca on ca.attribute_id = f.field_id
                    and ca.link_id in (select id from {IdList(databaseType, "@linkIds")}) ";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(GetIdsDatatableParam("@linkIds", linkIds, databaseType));
                    var dt = new DataTable();
                    DataAdapterFactory.Create(cmd).Fill(dt);
                    return IdCommaListDictionary(dt, "link_id", "article_id", false, linkIds);
                }
            }
        }

        public static Dictionary<int, string> GetLinkedArticles(DbConnection connection, IEnumerable<int> linkIds, int id, bool isLive, bool excludeArchive = false)
        {
            if (!linkIds.Any())
            {
                return new Dictionary<int, string>();
            }
            else
            {
                var databaseType = GetDbType(connection);
                var suffix = isLive ? string.Empty : "_united";
                var isArchive = excludeArchive ? $"join content_item ci {WithNoLock(databaseType)} on linked_item_id = ci.CONTENT_ITEM_ID and ci.ARCHIVE = 0" : string.Empty;
                var query = $@"
select linked_item_id, item_id, link_id
from item_link{suffix} {WithNoLock(databaseType)} {isArchive}
where item_id = @id and link_id in (select id from {IdList(databaseType, "@linkIds")}) ";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.Add(GetIdsDatatableParam("@linkIds", linkIds, databaseType));
                    var dt = new DataTable();
                    DataAdapterFactory.Create(cmd).Fill(dt);
                    return IdCommaListDictionary(dt, "link_id", "linked_item_id", false, linkIds);
                }
            }
        }

        public static Dictionary<int, Dictionary<int, List<int>>> GetLinkedArticlesMultiple(DbConnection connection, IEnumerable<int> linkIds, IEnumerable<int> ids, bool isLive, bool excludeArchive = false)
        {
            var dbType = GetDbType(connection);
            var sql = $@" select
                        ii.r_item_id as linked_item_id,
                        ii.l_item_id as item_id,
                        ii.link_id
                        from {IdList(dbType, "@itemIds", "i")}
                            inner join item_to_item ii {(dbType == DatabaseType.SqlServer ? "with(nolock, index(ix_l_item_id))" : string.Empty)} on ii.l_item_id = i.id
                        where link_id in (select id from {IdList(dbType, "@linkIds", "l")}) ";

            if (excludeArchive)
            {
                sql = $@" select
                            ii.r_item_id as linked_item_id,
                            ii.l_item_id as item_id,
                            ii.link_id
                            from {IdList(dbType, "@itemIds", "i")}
                                inner join item_to_item ii {(dbType == DatabaseType.SqlServer ? "with(nolock, index(ix_l_item_id))" : string.Empty)} on ii.l_item_id = i.id
                         inner join content_item ci {WithNoLock(dbType)} on ci.CONTENT_ITEM_ID = ii.r_item_id
                         where link_id in (select id from {IdList(dbType, "@linkIds", "l")}) and ci.ARCHIVE = 0";
            }
            if (!isLive)
            {
                var sb = new StringBuilder();
                sb.AppendLine(sql);
                sb.AppendLine(" and not exists (select * from content_item_splitted cis where cis.content_item_id = ii.l_item_id) ");
                sb.AppendLine(" union all ");
                sb.AppendLine($@"
                    select il.linked_item_id,
                        il.item_id,
                        il.link_id
                        from {IdList(dbType, "@itemIds", "i")}
                            inner join item_link_async il {WithNoLock(dbType)} on il.item_id = i.id
                        where link_id in (select id from {IdList(dbType, "@linkIds", "l")} )");
                sql = sb.ToString();
            }
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(GetIdsDatatableParam("@itemIds", ids, dbType));
                cmd.Parameters.Add(GetIdsDatatableParam("@linkIds", linkIds, dbType));


                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return GroupedLinks(dt, "item_id", "linked_item_id", "link_id", linkIds);
            }
        }

        public class FieldInfo
        {

            public int ContentId { get; set; }

            public string Name { get; set; }

            public int Id { get; set; }

            public override bool Equals(object obj) => Id == ((FieldInfo)obj).Id;

            public override int GetHashCode() => Id;
        }

        public static Dictionary<int, string> GetRelatedArticles(DbConnection connection, IEnumerable<FieldInfo> fiList, int? id, bool isLive, bool excludeArchive = false)
        {
            if (!fiList.Any())
            {
                return new Dictionary<int, string>();
            }
            else
            {
                var databaseType = GetDbType(connection);
                var suffix = isLive ? string.Empty : "_united";
                var action = id.HasValue ? " = @id" : " is null ";
                var isArchive = excludeArchive ? " and archive = 0" : string.Empty;
                var fieldIds = fiList.Select(n => n.Id).ToArray();
                var strTemplates = fiList.Select(fi => $@"
select
content_item_id,
cast({fi.Id} as decimal) as field_id
from content_{fi.ContentId}{suffix} {WithNoLock(databaseType)}
where {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fi.Name)} {action} {isArchive}");

                var sql = string.Join(Environment.NewLine + "union all" + Environment.NewLine, strTemplates);

                using (var cmd = DbCommandFactory.Create(sql, connection))
                {

                    cmd.Parameters.AddWithValue("@id", id);


                    var dt = new DataTable();
                    DataAdapterFactory.Create(cmd).Fill(dt);
                    return IdCommaListDictionary(dt, "field_id", "content_item_id", false, fieldIds);
                }
            }
        }

        public static int[] ExcludeArchived(DbConnection connection, int[] ids)
        {
            var dbType = GetDbType(connection);
            var query = $"select content_item_id from content_item {WithNoLock(dbType)} where content_item_id in (select id from {IdList(dbType, "@itemIds")}) and archive = 0";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.Parameters.Add(GetIdsDatatableParam("@itemIds", ids, dbType));
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().Select(row => (int)(decimal)row["content_item_id"]).ToArray();
            }
        }

        public static Dictionary<int, Dictionary<int, List<int>>> GetRelatedArticlesMultiple(DbConnection connection, IEnumerable<FieldInfo> fiList, IEnumerable<int> ids, bool isLive, bool excludeArchive = false)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var suffix = isLive ? string.Empty : "_united";
            var isArchive = excludeArchive ? " and archive = 0" : string.Empty;
            var fieldIds = fiList.Select(n => n.Id).ToArray();

            var strTemplates = fiList.Select(fi => $@"
                select content_item_id as linked_item_id, {Escape(dbType, fi.Name)} as item_id, cast({fi.Id} as decimal) as field_id
                from content_{fi.ContentId}{suffix} {WithNoLock(dbType)}
                where {Escape(dbType, fi.Name)} in (select id from {IdList(dbType, "@itemIds")}) {isArchive}
            ");

            var sql = string.Join(Environment.NewLine + "union all" + Environment.NewLine, strTemplates);
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@itemIds", ids, dbType));
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return GroupedLinks(dt, "item_id", "linked_item_id", "field_id", fieldIds);
            }
        }

        public static string GetQueryForO2MValues(DatabaseType dbType, string fieldName, string displayFieldName, int contentId, List<int> ids, int maxNumberOfRecords)
        {

            var query = $@"
            select subsel.content_item_id, {fieldName}, Title from (
                select u.content_item_id, u.{fieldName}, {SqlQuerySyntaxHelper.CastToString(dbType, Escape(dbType, displayFieldName))} as Title, ROW_NUMBER ()
                over (PARTITION BY {fieldName} order by u.content_item_id) AS {Escape(dbType, "RowNum")}
                from content_{contentId}_united as u {WithNoLock(dbType)} where {fieldName} in ({string.Join(",", ids)})) as subsel
                where subsel.{Escape(dbType, "RowNum")} <= {maxNumberOfRecords + 1}
";


            // var query = new StringBuilder();
            // query.Append($" select subsel.content_item_id, {fieldName}, Title from ( ", );
            // query.AppendFormatLine(" select u.content_item_id, u.{0}, CONVERT(NVARCHAR(255),[{1}]) as Title, ROW_NUMBER () over (PARTITION BY {0} order by u.content_item_id) AS [RowNum]", fieldName, displayFieldName);
            // query.AppendFormatLine(" from content_{0}_united as u with(nolock) where {2} in ({1})) as subsel ", contentId, string.Join(",", ids), fieldName);
            // query.AppendFormatLine(" where subsel.[RowNum] <= {0}", maxNumberOfRecords + 1);
            return query;
        }

        public static Dictionary<string, List<string>> GetM2OValuesBatch(DbConnection connection, int contentId, int fieldId, string fieldName, List<int> ids, string displayFieldName, int maxNumberOfRecords)
        {
            var dbType = GetDbType(connection);
            var result = new Dictionary<string, List<string>>();
            if (ids.Any())
            {
                var query = GetQueryForO2MValues(dbType, fieldName, displayFieldName, contentId, ids, maxNumberOfRecords);

                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = reader.GetDecimal(1) + "_" + fieldId;
                            var valueToInsert = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
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

        public static Dictionary<Tuple<int, int>, List<int>> GetM2OValues(DbConnection connection, int contentId, int fieldId, string fieldName, List<int> ids, string displayFieldName, int maxNumberOfRecords)
        {
            var dbType = GetDbType(connection);
            var result = new Dictionary<Tuple<int, int>, List<int>>();
            if (ids.Any())
            {
                var query = GetQueryForO2MValues(dbType, fieldName, displayFieldName, contentId, ids, maxNumberOfRecords);

                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var articleId = (int)reader.GetDecimal(1);

                            var relatedArticleId = reader.IsDBNull(0) ? 0 : (int)reader.GetDecimal(0);
                            var key = Tuple.Create(articleId, fieldId);
                            if (result.ContainsKey(key))
                            {
                                result[key].Add(relatedArticleId);
                            }
                            else
                            {
                                result.Add(key, new List<int> { relatedArticleId });
                            }

                        }
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, List<string>> GetM2MValuesBatch(DbConnection sqlConnection, List<int> ids, int linkId, int maxNumberOfRecords, string displayFieldName, int contentId)
        {
            var result = new Dictionary<string, List<string>>();
            if (ids.Any())
            {
                var databaseType = GetDbType(sqlConnection);
                var titleField = databaseType == DatabaseType.Postgres
                    ? $"{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, displayFieldName)}::text"
                    : $"CONVERT (NVARCHAR(255), {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, displayFieldName)})";
                var query = $@"
SELECT item_id, linked_item_id, link_id, title
FROM (
SELECT item_id, linked_item_id, link_id, {titleField} as title, ROW_NUMBER()
over (partition by item_id order by linked_item_id) AS RowNum
from item_link_united as u {WithNoLock(databaseType)}
inner join content_{contentId}_united as c {WithNoLock(databaseType)} on u.linked_item_id = c.CONTENT_ITEM_ID
where item_id in (select id from {(databaseType == DatabaseType.Postgres ? "unnest(@ids) i(id)" : "@ids")}) and link_id = {linkId}) as subq
where subq.RowNum <= {maxNumberOfRecords + 1} ";

                using (var cmd = DbCommandFactory.Create(query, sqlConnection))
                {
                    switch (databaseType)
                    {
                        case DatabaseType.SqlServer:
                            cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids));
                            break;
                        case DatabaseType.Postgres:
                            cmd.Parameters.Add(GetIntArrayPostgresParam("@ids", ids));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var key = Converter.ToString(reader.GetDecimal(0)) + "_" + Converter.ToString(reader.GetDecimal(2));
                            var valueToInsert = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
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



        public static string GetLinkedArticlesForVersion(DbConnection connection, int fieldId, int versionId)
        {
            using (var cmd = DbCommandFactory.Create("select linked_item_id from item_to_item_version where attribute_id = @attributeId and content_item_version_id = @versionId", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attributeId", fieldId);
                cmd.Parameters.AddWithValue("@versionId", versionId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return IdCommaList(dt, "linked_item_id", false);
            }
        }

        public static void RemoveLinkVersions(DbConnection connection, int fieldId)
        {
            using (var cmd = DbCommandFactory.Create("delete from item_to_item_version where attribute_id = @fid", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static DateTime GetSqlDate(DbConnection connection)
        {
            var dbType = GetDbType(connection);
            using (var cmd = DbCommandFactory.Create($"select {Now(dbType)} as date", connection))
            {
                cmd.CommandType = CommandType.Text;
                return (DateTime)cmd.ExecuteScalar();
            }
        }

        public static int GetBaseFieldId(DbConnection connection, int fieldId, int articleId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            using (var cmd = DbCommandFactory.Create("qp_get_base_field", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@field_id", fieldId);
                cmd.Parameters.AddWithValue("@article_id", articleId);
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return (dbType == DatabaseType.Postgres) ? dr.GetInt32(0) : (int)dr.GetDecimal(0);
                    }

                    return fieldId;
                }
            }
        }

        public static int GetSelfRelationFieldId(DbConnection connection, int contentId)
        {
            using (var cmd = DbCommandFactory.Create("select dbo.qp_get_self_relation_field_id(@contentId)", connection))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : (int)(decimal)result;
            }
        }


        public static int[] GetParentEntityIdsForTree(
            DbConnection connection,
            string entityTypeCode,
            int[] ids,
            decimal? contentId,
            string selfRelationFieldName,
            string entitySource = null,
            string entityIdField = null,
            string entityRecurringIdField = null)
        {
            var result = new List<int>();
            var dbType = GetDbType(connection);

            string query;
            if (entityTypeCode == EntityTypeCode.Article)
            {
                query = $@"
                SELECT DISTINCT {Escape(dbType, selfRelationFieldName)}
                FROM content_{contentId}_united
                WHERE {Escape(dbType, selfRelationFieldName)} is not null and content_item_id in ({string.Join(",", ids)})
                ";
            }
            else
            {
                query = $@"
                SELECT DISTINCT {Escape(dbType, entityRecurringIdField)}
                FROM {entitySource}
                WHERE {Escape(dbType, entityIdField)} in ({string.Join(",", ids)})
                AND {Escape(dbType, entityRecurringIdField)} is not null
                ";

            }

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("@entityTypeCode", entityTypeCode);
                // cmd.Parameters.AddWithValue("@entityIds", string.Join(",", ids));
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add((int)dr.GetDecimal(0));
                    }
                }
            }

            return result.ToArray();
        }

        private const string GetTranslationsQuery = @"select p.PHRASE_ID, p.PHRASE_TEXT, t.LANGUAGE_ID, t.PHRASE_TRANSLATION from translations t inner join phrases p on t.phrase_id = p.phrase_id";

        public static IEnumerable<DataRow> GetTranslations(DbConnection connection)
        {
            using (var cmd = DbCommandFactory.Create(GetTranslationsQuery, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }



        // public static DataTable GetBreadCrumbsList(DbConnection connection, int userId, string entityTypeCode, long entityId, long? parentEntityId, bool oneLevel)
        // {
        //     DataTable dt = null;
        //     #warning реализовать для postgres
        //     if (IsPostgresConnection(connection))
        //     {
        //         return dt;
        //     }
        //     using (var cmd = DbCommandFactory.Create("qp_get_breadcrumbs", connection))
        //     {
        //         cmd.CommandType = CommandType.StoredProcedure;
        //         cmd.Parameters.AddWithValue("@user_id", userId);
        //         cmd.Parameters.AddWithValue("@entity_type_code", entityTypeCode);
        //         cmd.Parameters.AddWithValue("@entity_id", entityId);
        //         cmd.Parameters.AddWithValue("@one_level", oneLevel);
        //
        //         if (parentEntityId != null)
        //         {
        //             cmd.Parameters.AddWithValue("@parent_entity_id", parentEntityId);
        //         }
        //
        //         var ds = new DataSet();
        //         DataAdapterFactory.Create(cmd).Fill(ds);
        //         if (ds.Tables.Count > 0)
        //         {
        //             dt = ds.Tables[0];
        //         }
        //     }
        //
        //     return dt;
        // }

        public static bool IsAdmin(DbConnection connection, int userId)
        {
            using (var cmd = DbCommandFactory.Create($"select count(*) from user_group_bind where group_id = {SpecialIds.AdminGroupId} and user_id = @userId", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt64(result) > 0;
            }
        }

        public static int[] GetGroupIds(DbConnection connection, int userId)
        {
            using (var cmd = DbCommandFactory.Create("select group_id from user_group_bind where user_id = @userId", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                var isSqlServer = connection is SqlConnection;
                return dt.AsEnumerable().Select(n => (int)n.Field<decimal>("group_id")).ToArray();
            }
        }

        public static bool CanUnlockItems(DbConnection connection, int userId)
        {
            var dbType = GetDbType(connection);
            string sql = $@"
                WITH
                  {SqlQuerySyntaxHelper.RecursiveCte(dbType)} cte (group_id, parent_group_id, can_unlock_items)
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
                  select count(*) from cte where can_unlock_items = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static int CountArticles(DbConnection connection, int contentId, bool includeArchive)
        {
            var databaseType = GetDbType(connection);
            var sql = $"select count(*) from content_{contentId} {WithNoLock(databaseType)}";
            if (!includeArchive)
            {
                sql = sql + $" where archive = 0";
            }

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для пользователя по entity_type_code и action_type_code
        /// </summary>
        public static bool IsEntityAccessible(DbConnection connection, int userId, string entityTypeCode, int entityId, string actionTypeCode)
        {
            #warning реализовать для postgres
            if (IsPostgresConnection(connection)) return true;
            using (var cmd = DbCommandFactory.Create("select dbo.qp_is_entity_action_type_accessible(@userId, 0, @entityTypeCode, @entityId, @actionTypeCode)", connection))
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
        public static bool IsEntityAccessibleForUserGroup(DbConnection connection, int groupId, string entityTypeCode, int entityId, string actionTypeCode)
        {
            #warning реализовать нормально! заглушка
            var dbType = GetDbType(connection);
            if (dbType == DatabaseType.Postgres)
            {
                return true;
            }


            using (var cmd = DbCommandFactory.Create("select dbo.qp_is_entity_action_type_accessible(0, @groupId, @entityTypeCode, @entityId, @actionTypeCode)", connection))
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
        public static int GetMaxUserWeight(DbConnection connection, int userId, int workflowId)
        {
            const string sql = @"SELECT dbo.qp_get_user_weight(@userId, @workflowId)";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@workflowId", workflowId);
                var value = cmd.ExecuteScalar();
                return value == DBNull.Value ? 0 : (int)(decimal)value;
            }
        }

        public static void ChangeTriggerState(DbConnection connection, string triggerName, bool enable)
        {
            var tableName = "#disable_" + triggerName;
            var opString = enable ? "drop" : "create";
            var signatureString = enable ? string.Empty : "(id numeric)";
            var checkString = enable
                ? $"if object_id('tempdb..{tableName}') is not null"
                : $"if object_id('tempdb..{tableName}') is null";

            var sql = string.Format("{3} {0} table {1} {2}", opString, tableName, signatureString, checkString);
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentAccess(DbConnection connection, int sourceId, int destinationId, int userId)
        {
            var dbType = GetDbType(connection);
            var sql = $@"
                DELETE FROM content_access WHERE content_id = {destinationId};
                INSERT INTO content_access (content_id, user_id, group_id, permission_level_id, created, modified, last_modified_by, propagate_to_items, hide)
                SELECT {destinationId}, user_id, group_id, permission_level_id, {Now(dbType)}, {Now(dbType)}, {userId}, propagate_to_items, hide FROM content_access
                WHERE content_id = {sourceId}
            ";

            ExecuteSql(connection, sql);

        }

        public static void CopyArticleAccess(DbConnection sqlConnection, int sourceId, int destinationId, int userId)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendFormat("DELETE FROM content_item_access WHERE content_item_id = {0};", sourceId);
            sqlBuilder.Append("INSERT INTO content_item_access");
            sqlBuilder.AppendFormat(" SELECT {0}, user_id, group_id, permission_level_id, GETDATE(), GETDATE(), {1} FROM content_item_access", destinationId, userId);
            sqlBuilder.AppendFormat(" WHERE content_item_id = {0}", sourceId);

            ExecuteSql(sqlConnection, sqlBuilder.ToString());
        }

        public static void UpdateContentFieldsOrder(DbConnection connection, int currentOrder, int newOrder, int contentId)
        {
            if (currentOrder != newOrder)
            {
                var sqlBuilder = new StringBuilder("update CONTENT_ATTRIBUTE ");
                if (currentOrder <= 0)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1");
                    sqlBuilder.AppendFormat(" where ATTRIBUTE_ORDER > {0}", newOrder);
                }
                else if (currentOrder < newOrder)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER - 1 ");
                    sqlBuilder.AppendFormat(" where ATTRIBUTE_ORDER > {0} and ATTRIBUTE_ORDER <= {1}", currentOrder,
                        newOrder);
                }
                else if (currentOrder > newOrder)
                {
                    sqlBuilder.Append(" set ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1");
                    sqlBuilder.AppendFormat(" where ATTRIBUTE_ORDER < {0} and ATTRIBUTE_ORDER > {1}", currentOrder,
                        newOrder);
                }

                sqlBuilder.AppendFormat(" and CONTENT_ID = {0}", contentId);
                using (var cmd = DbCommandFactory.Create(sqlBuilder.ToString(), connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int GetStringFieldMaxLength(DbConnection connection, int contentId, string fieldName)
        {
            var dbType = GetDbType(connection);
            using (var cmd = DbCommandFactory.Create($"select MAX({SqlQuerySyntaxHelper.GetFieldLength(dbType, fieldName)}) from content_{contentId}_united {WithNoLock(dbType)}", connection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                var nullableResult = Converter.ToNullableInt32(objResult);
                return nullableResult ?? 0;
            }
        }

        public static int GetBlobFieldMaxSymbolLength(DbConnection connection, int contentId, string fieldName)
        {
            using (var cmd = DbCommandFactory.Create(string.Format("select MAX(DATALENGTH([{1}])) / 2 from content_{0}_united with(nolock)", contentId, fieldName), connection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                var nullableResult = Converter.ToNullableInt32(objResult);
                return nullableResult ?? 0;
            }
        }

        public static int? GetNumericFieldMaxValue(DbConnection connection, int contentId, string fieldName)
        {
            using (var cmd = DbCommandFactory.Create(string.Format("select MAX([{1}]) from content_{0}_united with(nolock)", contentId, fieldName), connection))
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
        public static bool CheckNumericValuesAsO2MForeingKey(DbConnection connection, int contentId, string fieldName, int relatedContentId)
        {
            using (var cmd = DbCommandFactory.Create(string.Format("select 1 where EXISTS (select C1.CONTENT_ITEM_ID from content_{0}_united C1 with(nolock) join content_{2}_united C2 with(nolock) ON C1.[{1}] != C2.[CONTENT_ITEM_ID])", contentId, fieldName, relatedContentId), connection))
            {
                cmd.CommandType = CommandType.Text;
                var obj = cmd.ExecuteScalar();
                return obj == null;
            }
        }

        /// <summary>
        /// Существуют ли множественные связи по данному полю.
        /// </summary>
        public static bool DoPluralLinksExist(DbConnection connection, int contentId, int linkId)
        {
            using (
                var cmd =
                    DbCommandFactory.Create(
                        $"select 1 where exists(select item_id from item_link_united i inner join content_{contentId}_united c with(nolock) on i.item_id = c.content_item_id where link_id = @lid group by item_id having COUNT(linked_item_id) > 1)", connection))
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
        public static void ChangeRelationIdToNewOne(DbConnection connection, int currentRelationFieldId, int newRelationFieldId)
        {
            using (var cmd = DbCommandFactory.Create("update CONTENT_ATTRIBUTE set RELATED_ATTRIBUTE_ID = @newid where RELATED_ATTRIBUTE_ID = @crnid", connection))
            {
                cmd.Parameters.AddWithValue("@newid", newRelationFieldId);
                cmd.Parameters.AddWithValue("@crnid", currentRelationFieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ClearEmailField(DbConnection connection, int fieldId)
        {
            using (var cmd = DbCommandFactory.Create("UPDATE NOTIFICATIONS set email_attribute_id = null where email_attribute_id = @fid", connection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Трансформирует даные статей при изменении типа поля с O2M в M2M
        /// </summary>
        public static void O2MtoM2MTranferData(DbConnection connection, int fieldId, int linkId)
        {
            // 1. перенести данные о связях из CONTENT_DATA в item_to_item.
            // 2. В CONTENT_DATA в качестве значения поля для  всех статей установить LinkId
            const string cmdText = "INSERT INTO [item_to_item] ([link_id],[l_item_id],[r_item_id]) select @linkid as link_id, D.CONTENT_ITEM_ID as l_item_id, D.DATA as r_item_id from CONTENT_DATA D where ATTRIBUTE_ID = @fid and D.DATA is not null; " +
                "update CONTENT_DATA set DATA = @linkid where ATTRIBUTE_ID = @fid;";

            using (var cmd = DbCommandFactory.Create(cmdText, connection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@linkid", linkId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void M2MtoO2MTranferData(DbConnection connection, int fieldId, int linkId)
        {
            // перенести данные о связях из item_to_item в CONTENT_DATA.
            const string cmdText = "update CONTENT_DATA SET DATA = LD.linked_item_id from dbo.item_link_united LD where LD.item_id = CONTENT_DATA.CONTENT_ITEM_ID and CONTENT_DATA.ATTRIBUTE_ID = @fid and LD.link_id = @linkid; " +
                "update CONTENT_DATA SET DATA = NULL where CONTENT_DATA.ATTRIBUTE_ID = @fid and CONTENT_DATA.DATA = @linkid;";

            using (var cmd = DbCommandFactory.Create(cmdText, connection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@linkid", linkId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Меняет данные, хранимые в CONTENT_DATA для полей M2M или M2O
        /// </summary>
        public static void ChangeContentDataForRelation(DbConnection connection, int fieldId, int newId)
        {
            const string cmdText = "update content_data set data = @new_id where data is not null and attribute_id = @attribute_id";
            using (var cmd = DbCommandFactory.Create(cmdText, connection))
            {
                cmd.Parameters.AddWithValue("@attribute_id", fieldId);
                cmd.Parameters.AddWithValue("@new_id", newId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Обновить Order поля
        /// </summary>
        public static void UpdateFieldOrder(DbConnection connection, int fieldId, int newOrder)
        {
            using (var cmd = DbCommandFactory.Create("update CONTENT_ATTRIBUTE set ATTRIBUTE_ORDER = @ord where ATTRIBUTE_ID = @fid", connection))
            {
                cmd.Parameters.AddWithValue("@fid", fieldId);
                cmd.Parameters.AddWithValue("@ord", newOrder);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Возвращает данные о виртуальном поле
        /// </summary>
        public static DataTable GetVirtualFieldData(DbConnection connection, int contentId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);

            var query = "SELECT " +
                "a.ATTRIBUTE_ID as Id, " +
                "a.ATTRIBUTE_NAME as Name, " +
                $"a.ATTRIBUTE_TYPE_ID as {Escape(dbType, "Type")}, " +
                "pa.content_id AS PersistentContentId, " +
                "a.persistent_attr_id as PersistentId, " +
                "pa.attribute_name AS PersistentName, " +
                "rpa.CONTENT_ID as RelateToPersistentContentId, " +
                "a.join_attr_id as JoinId " +
                $"FROM content_attribute AS a {WithNoLock(dbType)} " +
                $"LEFT OUTER JOIN content_attribute AS pa {WithNoLock(dbType)} ON pa.attribute_id = a.persistent_attr_id " +
                $"LEFT OUTER JOIN content_attribute AS rpa {WithNoLock(dbType)} ON rpa.attribute_id = pa.related_attribute_id " +
                $"LEFT OUTER JOIN content AS c {WithNoLock(dbType)} ON rpa.content_id = c.content_id " +
                "WHERE a.content_id = @contentId and a.persistent_attr_id IS NOT NULL";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.Parameters.AddWithValue("@contentId", contentId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Создает United View
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contentId"></param>
        public static void CreateUnitedView(DbConnection connection, int contentId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = dbType == DatabaseType.SqlServer ? "qp_content_united_view_create" : "call qp_content_united_view_create(@content_id)";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Создает live view и stage view
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contentId"></param>
        public static void CreateFrontendViews(DbConnection connection, int contentId)
        {
            using (var cmd = DbCommandFactory.Create("qp_content_frontend_views_create", connection))
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
        public static void DropView(DbConnection connection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var sql = dbType == DatabaseType.SqlServer ? "qp_drop_existing" : "drop view if exists " + viewName + " cascade";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@name", viewName);
                    cmd.Parameters.AddWithValue("@flag", "IsView");
                }
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Удаляет view
        /// </summary>
        public static void RefreshView(DbConnection connection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            const string sql = "if exists(select * from INFORMATION_SCHEMA.VIEWS where table_name = @viewName) exec sp_refreshview @viewName";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@viewname", viewName);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Возвращает список id дочерних виртуальных контентов
        /// </summary>
        public static IEnumerable<int> GetVirtualSubContentIDs(DbConnection connection, int contentId)
        {
            const string query = "select content_id from content where virtual_join_primary_content_id = @contentId " +
                "union " +
                "select virtual_content_id as content_id from union_contents where union_content_id = @contentId " +
                "union " +
                "select virtual_content_id as content_id from user_query_contents where real_content_id = @contentId";

            var result = new List<int>();
            using (var cmd = DbCommandFactory.Create(query, connection))
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

        public static IEnumerable<int> GetRealBaseFieldIds(DbConnection connection, int virtualFieldId)
        {
            var result = new List<int>();
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            var recursive = dbType == DatabaseType.SqlServer ? "" : "RECURSIVE";
            var sql = $@" WITH {recursive} TREE(BASE_ATTR_ID, BASE_CNT_VTYPE) AS
            (
                select BASE_ATTR_ID, BASE_CNT_VTYPE FROM VIRTUAL_ATTR_BASE_ATTR_RELATION
                where VIRTUAL_ATTR_ID = @v_attr_id

                union all

                select R.BASE_ATTR_ID, R.BASE_CNT_VTYPE FROM VIRTUAL_ATTR_BASE_ATTR_RELATION R
                join TREE T ON T.BASE_ATTR_ID = R.VIRTUAL_ATTR_ID
            )
            select BASE_ATTR_ID from TREE where BASE_CNT_VTYPE = 0 ";

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
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
        public static DataTable GetVirtualSubFields(DbConnection connection, List<int> rootFieldId)
        {
            var inStatement = "-1";
            if (rootFieldId.Any())
            {
                inStatement = string.Join(",", rootFieldId);
            }

            var query = $"SELECT BASE_ATTR_ID, BASE_CNT_ID, VIRTUAL_ATTR_ID, VIRTUAL_CNT_ID FROM VIRTUAL_ATTR_BASE_ATTR_RELATION WHERE BASE_ATTR_ID IN ({inStatement})";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Возвращает информацию о дочерних витуальных полях
        /// </summary>
        public static DataTable GetVirtualBaseFieldIDs(DbConnection connection, List<int> subFieldIds)
        {
            var inStatement = "-1";
            if (subFieldIds.Any())
            {
                inStatement = string.Join(",", subFieldIds);
            }

            var query = $"SELECT distinct BASE_ATTR_ID, BASE_CNT_ID, VIRTUAL_ATTR_ID, VIRTUAL_CNT_ID FROM VIRTUAL_ATTR_BASE_ATTR_RELATION WHERE VIRTUAL_ATTR_ID IN ({inStatement})";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Загрузить данные о связи полей в памяти, начиная с некоторго контента
        /// </summary>
        public static DataTable LoadVirtualFieldsRelations(DbConnection sqlConnection, int rootContentId)
        {
            var dbType = GetDbType(sqlConnection);
            string query = $@"
            WITH {SqlQuerySyntaxHelper.RecursiveCte(dbType)} V2BREL AS (
                SELECT *, 0 as {Escape(dbType, "LEVEL")} FROM VIRTUAL_ATTR_BASE_ATTR_RELATION where BASE_CNT_ID = @rootContent
                union all
                SELECT BA.*, {Escape(dbType, "LEVEL")} + 1 FROM VIRTUAL_ATTR_BASE_ATTR_RELATION BA
                join V2BREL on BA.BASE_ATTR_ID = V2BREL.VIRTUAL_ATTR_ID
            )
            select * from V2BREL order by {Escape(dbType, "LEVEL")}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@rootContent", rootContentId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Получить количество полей связей в Union_ATTR для union-полей
        /// </summary>
        public static DataTable GetUnionFieldRelationCount(DbConnection connection, List<int> unionFieldIds)
        {
            var inStatement = "-1";
            if (unionFieldIds.Any())
            {
                inStatement = string.Join(",", unionFieldIds);
            }

            var query = $"select cast(COUNT(union_attr_id) as int) F_COUNT, VIRTUAL_ATTR_ID UNION_FIELD_ID FROM union_attrs GROUP BY VIRTUAL_ATTR_ID HAVING VIRTUAL_ATTR_ID IN ({inStatement})";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Получить информацию о таблицах и столбцах которые используются во вью
        /// </summary>
        public static DataTable GetViewColumnUsage(DbConnection sqlConnection, string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentException("viewName");
            }

            const string sql = @"
               WITH CI(COLUMN_NAME, DATA_TYPE, NUMERIC_SCALE, CHARACTER_MAXIMUM_LENGTH, TABLE_NAME) AS
               (
                 select CI.COLUMN_NAME, CI.DATA_TYPE, CI.NUMERIC_SCALE, Ci.CHARACTER_MAXIMUM_LENGTH, VU.TABLE_NAME from INFORMATION_SCHEMA.COLUMNS CI
                 LEFT JOIN INFORMATION_SCHEMA.VIEW_COLUMN_USAGE VU ON VU.COLUMN_NAME = CI.COLUMN_NAME AND VU.VIEW_NAME = @view_name
                 where CI.TABLE_NAME = @view_name
               )
               SELECT CI.COLUMN_NAME as ColumnName, CI.DATA_TYPE As DbType, CI.NUMERIC_SCALE as NumericScale, CI.CHARACTER_MAXIMUM_LENGTH as CharMaxLength, CI.TABLE_NAME As TableName, COALESCE(CI2.DATA_TYPE, CI.DATA_TYPE) AS TableDbType FROM CI
               LEFT JOIN INFORMATION_SCHEMA.COLUMNS CI2 ON CI2.COLUMN_NAME = CI.COLUMN_NAME AND CI2.TABLE_NAME = CI.TABLE_NAME";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@view_name", viewName);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable GetVirtualContentRelations(DbConnection sqlConnection)
        {
            const string query = "SELECT BASE_CONTENT_ID,VIRTUAL_CONTENT_ID FROM VIRTUAL_CONTENT_RELATION";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        public static IList<int> GetChildArticles(DbConnection cn, IList<int> ids, string fieldName, int contentId, string filter)
        {
            var dbType = GetDbType(cn);
            var customFilter = string.IsNullOrWhiteSpace(filter) ? string.Empty : $"AND {filter}";
            var parentFilter = ids.Any() ? $"c.{fieldName} IN ({string.Join(",", ids)})" : $"c.{fieldName} IS NULL";
            var query = $"SELECT c.content_item_id FROM content_{contentId}_united c {WithNoLock(dbType)} WHERE {parentFilter} {customFilter}";
            return GetDatatableResult(cn, query).Select(dr => (int)dr.Field<decimal>(0)).ToList();
        }

        private static string GetVisualEditorConfigQuery(DatabaseType dbType)
        {
            return "select COALESCE(A.P_ENTER_MODE, S.P_ENTER_MODE) AS PEnterMode, " +
                "COALESCE(A.USE_ENGLISH_QUOTES, S.USE_ENGLISH_QUOTES) AS UseEnglishQuotes,   " +
                "COALESCE(A.DISABLE_LIST_AUTO_WRAP, S.DISABLE_LIST_AUTO_WRAP) AS DisableListAutoWrap,   " +
                "COALESCE(A.ROOT_ELEMENT_CLASS, S.ROOT_ELEMENT_CLASS) AS RootElementClass,   " +
                "COALESCE(A.EXTERNAL_CSS, S.EXTERNAL_CSS) AS ExternalCss   " +
                "from CONTENT_ATTRIBUTE A  " +
                "join CONTENT C on C.CONTENT_ID = A.CONTENT_ID  " +
                $"join {Escape(dbType, "SITE")} S on S.SITE_ID = C.SITE_ID  " +
                "WHERE A.ATTRIBUTE_ID = @attr_id";
        }


        public static DataTable GetVisualEditFieldParams(DbConnection sqlConnection, int fieldId)
        {
            var dbType = GetDbType(sqlConnection);
            using (var cmd = DbCommandFactory.Create(GetVisualEditorConfigQuery(dbType), sqlConnection))
            {
                cmd.Parameters.AddWithValue("@attr_id", fieldId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        public static void UpdateSiteSubFoldersPath(DbConnection sqlConnection, int parentFolderId, string path, int modifiedBy, DateTime modified)
        {
            var dbType = GetDbType(sqlConnection);
            var pathColumnName = Escape(dbType, "PATH");
            var folderTableName = Escape(dbType, "FOLDER");
            var query = $@"WITH {SqlQuerySyntaxHelper.RecursiveCte(dbType)} FOLDERS(FOLDER_ID, PARENT_FOLDER_ID, {pathColumnName}, NEW_PATH) as (
                select FOLDER_ID, PARENT_FOLDER_ID, {pathColumnName}, REPLACE({pathColumnName}, {pathColumnName}, @new_path) AS NEW_PATH from {folderTableName}
                where FOLDER_ID = @folder_id
                union all
                select F.FOLDER_ID, F.PARENT_FOLDER_ID, F.{pathColumnName}, REPLACE(F.{pathColumnName}, F2.{pathColumnName}, F2.NEW_PATH) AS NEW_PATH from {folderTableName} F
                join FOLDERS F2 ON F2.FOLDER_ID = F.PARENT_FOLDER_ID
                )
                UPDATE {folderTableName} SET {pathColumnName} = NP.NEW_PATH,
                 MODIFIED = @modified,
                 LAST_MODIFIED_BY = @modified_by
                FROM FOLDERS NP
                WHERE {folderTableName}.FOLDER_ID = NP.FOLDER_ID";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@folder_id", parentFolderId);
                cmd.Parameters.AddWithValue("@new_path", path);
                cmd.Parameters.AddWithValue("@modified", modified);
                cmd.Parameters.AddWithValue("@modified_by", modifiedBy);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentSubFoldersPath(DbConnection sqlConnection, int parentFolderId, string path, int modifiedBy, DateTime modified)
        {
            var dbType = GetDbType(sqlConnection);
            var pathColumnName = Escape(dbType, "PATH");
            var folderTableName = Escape(dbType, "content_FOLDER");

            var query = $@"WITH {SqlQuerySyntaxHelper.RecursiveCte(dbType)} FOLDERS(FOLDER_ID, PARENT_FOLDER_ID, {pathColumnName}, NEW_PATH) as
                (
                select FOLDER_ID, PARENT_FOLDER_ID, {pathColumnName}, REPLACE({pathColumnName}, {pathColumnName}, @new_path) AS NEW_PATH from {folderTableName}
                where FOLDER_ID = @folder_id
                union all
                select F.FOLDER_ID, F.PARENT_FOLDER_ID, F.{pathColumnName}, REPLACE(F.{pathColumnName}, F2.{pathColumnName}, F2.NEW_PATH) AS NEW_PATH from {folderTableName} F
                join FOLDERS F2 ON F2.FOLDER_ID = F.PARENT_FOLDER_ID
                )
                UPDATE {folderTableName}
                 SET {pathColumnName} = NP.NEW_PATH,
                 MODIFIED = @modified,
                 LAST_MODIFIED_BY = @modified_by
                FROM FOLDERS NP
                WHERE {folderTableName}.FOLDER_ID = NP.FOLDER_ID";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@folder_id", parentFolderId);
                cmd.Parameters.AddWithValue("@new_path", path);
                cmd.Parameters.AddWithValue("@modified", modified);
                cmd.Parameters.AddWithValue("@modified_by", modifiedBy);
                cmd.ExecuteNonQuery();
            }
        }

        // public static IEnumerable<DataRow> GetEntitiesTitles(DbConnection sqlConnection, string entityTypeCode, List<int> entitiesIDs)
        // {
        //
        //     if (entitiesIDs != null && entitiesIDs.Any())
        //     {
        //         using (var cmd = DbCommandFactory.Create("qp_get_entity_titles_for_log", sqlConnection))
        //         {
        //             cmd.CommandType = CommandType.StoredProcedure;
        //             cmd.Parameters.AddWithValue("@entity_type_code", entityTypeCode);
        //             cmd.Parameters.AddWithValue("@entity_item_ids", string.Join(",", entitiesIDs));
        //
        //             var dt = new DataTable();
        //             DataAdapterFactory.Create(cmd).Fill(dt);
        //             return dt.AsEnumerable().ToArray();
        //         }
        //     }
        //
        //     return Enumerable.Empty<DataRow>();
        // }

        public static IEnumerable<DataRow> GetEntitiesTitles(DbConnection connection, string titleField, decimal? contentId, List<int> entitiesIDs)
        {
            if (entitiesIDs != null && entitiesIDs.Any() && !string.IsNullOrWhiteSpace(titleField) && contentId.HasValue)
            {
                var dbType = GetDbType(connection);
                var query = $"SELECT content_item_id as ID, {SqlQuerySyntaxHelper.CastToString(dbType, titleField)} as TITLE from content_{contentId.Value}_united WHERE content_item_id IN ({string.Join(",", entitiesIDs)})";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    var dt = new DataTable();
                    DataAdapterFactory.Create(cmd).Fill(dt);
                    return dt.AsEnumerable().ToArray();
                }
            }

            return Enumerable.Empty<DataRow>();
        }

        public static IEnumerable<DataRow> GetEntitiesTitles(DbConnection connection, string source, string idField, string titleField, List<int> entitiesIDs)
        {
            if (entitiesIDs != null && entitiesIDs.Any() && !string.IsNullOrWhiteSpace(titleField) && !string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(idField))
            {
                var dbType = GetDbType(connection);
                var query = $"SELECT {idField} as ID, {titleField} as TITLE from {source} WHERE {idField} IN ({string.Join(",", entitiesIDs)})";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    var dt = new DataTable();
                    DataAdapterFactory.Create(cmd).Fill(dt);
                    return dt.AsEnumerable().ToArray();
                }
            }

            return Enumerable.Empty<DataRow>();
        }

        public static IEnumerable<DataRow> GetActionLogPage(
            DbConnection sqlConnection,
            string orderBy,
            string actionCode,
            string actionTypeCode,
            string entityTypeCode,
            string entityStringId,
            string parentEntityId,
            string entityTitle,
            string from, string to,
            List<int> userIds,
            out int totalRecords,
            int startRow = 0,
            int pageSize = 0)
        {
            Debug.Assert(userIds != null, "userIDs is null");
            var dbType = GetDbType(sqlConnection);

            var filters = new List<string>();

            if (!string.IsNullOrEmpty(actionCode))
            {
                filters.Add($"BA.CODE = '{Cleaner.ToSafeSqlString(actionCode)}'");
            }

            if (!string.IsNullOrEmpty(actionTypeCode))
            {
                filters.Add($"AT.CODE = '{Cleaner.ToSafeSqlString(actionTypeCode)}'");
            }

            if (!string.IsNullOrEmpty(entityTypeCode))
            {
                filters.Add($"ET.CODE = '{Cleaner.ToSafeSqlString(entityTypeCode)}'");
            }

            if (!string.IsNullOrEmpty(entityStringId))
            {
                filters.Add($"L.ENTITY_STRING_ID = '{Cleaner.ToSafeSqlString(entityStringId)}'");
            }

            if (!string.IsNullOrEmpty(parentEntityId))
            {
                filters.Add($"{SqlQuerySyntaxHelper.CastToVarchar(dbType, "L.PARENT_ENTITY_ID")} = '{Cleaner.ToSafeSqlString(parentEntityId)}'");
            }

            if (!string.IsNullOrEmpty(entityTitle))
            {
                filters.Add($"L.ENTITY_TITLE LIKE '%{Cleaner.ToSafeSqlString(entityTitle)}%'");
            }

            to = string.IsNullOrWhiteSpace(to) ? from : to;
            if (Converter.TryConvertToSqlDateTimeString(from, out string sqlDt, out DateTime? dt, sqlFormat: "yyyyMMdd HH:mm:00"))
            {
                filters.Add($"L.EXEC_TIME >= '{sqlDt}'");
            }

            if (Converter.TryConvertToSqlDateTimeString(to, out sqlDt, out dt, sqlFormat: "yyyyMMdd HH:mm:59"))
            {
                filters.Add($"L.EXEC_TIME <= '{sqlDt}'");
            }

            if (userIds.Any())
            {
                filters.Add($"U.{Escape(dbType, "USER_ID")} in ({string.Join(",", userIds)})");
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.CustomerCode,
                $@"L.ID as Id, L.EXEC_TIME AS ExecutionTime, L.API as IsApi, AT.CODE AS ActionTypeCode, AT.NAME AS ActionTypeName, ET.CODE AS EntityTypeCode, ET.NAME AS EntityTypeName,
                L.ENTITY_STRING_ID AS EntityStringId, L.PARENT_ENTITY_ID AS ParentEntityId, L.ENTITY_TITLE AS EntityTitle, U.{Escape(dbType, "USER_ID")} as UserId, U.{Escape(dbType, "LOGIN")} as UserLogin, BA.NAME as ActionName",
                $"BACKEND_ACTION_LOG L LEFT JOIN USERS U ON L.{Escape(dbType, "USER_ID")} = U.{Escape(dbType, "USER_ID")}" +
                " INNER JOIN ACTION_TYPE AT ON AT.CODE = L.ACTION_TYPE_CODE" +
                " INNER JOIN ENTITY_TYPE ET ON ET.CODE = L.ENTITY_TYPE_CODE" +
                " INNER JOIN BACKEND_ACTION BA ON BA.CODE = L.ACTION_CODE"
                ,
                !string.IsNullOrEmpty(orderBy) ? orderBy : "ExecutionTime DESC",
                string.Join(" AND ", filters),
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetCustomActionList(DbConnection sqlConnection, string orderBy, int startRow, int pageSize, out int totalRecords)
        {
            var selectBuilder = new StringBuilder();
            var dbType = GetDbType(sqlConnection);
            var ns = DbSchemaName(dbType);
            selectBuilder.Append("CA.ID, CA.NAME, CA.ACTION_ID");
            selectBuilder.Append(", AT.CODE AS ACTION_TYPE_CODE, AT.NAME AS ACTION_TYPE_NAME");
            selectBuilder.Append(", ET.CODE AS ENTITY_TYPE_CODE ,ET.NAME AS ENTITY_TYPE_NAME");
            selectBuilder.Append($", CA.URL, CA.ICON_URL, CA.{Escape(dbType, "ORDER")}, CA.SITE_EXCLUDED, CA.CONTENT_EXCLUDED, CA.SHOW_IN_MENU, CA.SHOW_IN_TOOLBAR");
            selectBuilder.Append(", CA.CREATED, CA.MODIFIED, CA.LAST_MODIFIED_BY, U.USER_ID, U.LOGIN");

            var fromBuilder = new StringBuilder();
            fromBuilder.Append($" {ns}.CUSTOM_ACTION CA");
            fromBuilder.Append($" JOIN {ns}.BACKEND_ACTION A ON CA.ACTION_ID = A.ID");
            fromBuilder.Append($" JOIN {ns}.ACTION_TYPE AT ON AT.ID = A.TYPE_ID");
            fromBuilder.Append($" JOIN {ns}.ENTITY_TYPE ET ON ET.ID = A.ENTITY_TYPE_ID");
            fromBuilder.Append($" JOIN {ns}.USERS U ON U.USER_ID = CA.LAST_MODIFIED_BY");

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

        public static List<DataRow> GetLockedArticlesList(DbConnection sqlConnection, string orderBy, int startRow, int pageSize, int userId, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var ns = DbSchemaName(dbType);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                $@"CONTENT_ITEM_ID as ID
                                      ,it.content_id as ParentId
                                      ,{ns}.qp_get_article_title_func(it.Content_Item_Id, it.content_id) as Title
                                      ,con.CONTENT_NAME as ContentName
                                      ,site.SITE_NAME as SiteName
                                      ,typ.STATUS_TYPE_NAME as StatusName
                                      ,it.CREATED as Created
                                      ,it.MODIFIED as Modified
                                      ,us.LOGIN as LastModifiedByUser
                                      ,it.PERMANENT_LOCK as IsPermanentLock",
                $@"{ns}.CONTENT_ITEM as it
                                      INNER JOIN {ns}.{Escape(dbType, "content")} as con on con.CONTENT_ID = it.CONTENT_ID
                                      INNER JOIN {ns}.{Escape(dbType, "site")} as site on site.SITE_ID = con.SITE_ID
                                      INNER JOIN {ns}.STATUS_TYPE as typ on typ.STATUS_TYPE_ID = it.STATUS_TYPE_ID
                                      INNER JOIN {ns}.{Escape(dbType, "USERS")} as us on us.USER_ID = it.LAST_MODIFIED_BY",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "ID ASC",
                $"it.{Escape(dbType, "locked_by")} = {userId}",
                startRow,
                pageSize,
                out totalRecords
            ).ToList();
        }

        public static int GetLockedArticlesCount(DbConnection sqlConnection, int userId)
        {
            const string query = "select count(*) from content_item where locked_by = @userId";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int GetArticlesWaitingForApprovalCount(DbConnection sqlConnection, int userId)
        {
            const string query =
                @"select count(*) from content_item_workflow ciw with(nolock)
                    INNER JOIN content_item ci with(nolock) ON ci.content_item_id = ciw.content_item_id
                    INNER JOIN full_workflow_rules wr with(nolock) on ciw.workflow_id = wr.workflow_id AND ci.status_type_id = wr.successor_status_id
                    INNER JOIN full_workflow_rules wr2 with(nolock) on wr.workflow_id = wr2.workflow_id AND wr2.rule_order = wr.rule_order + 1
                    WHERE (wr2.user_id = @userId OR wr2.group_id IN (SELECT group_id FROM user_group_bind with(nolock) WHERE user_id = @userId))
                    AND (
                        ci.content_item_id not in (select content_item_id from waiting_for_approval with(nolock))
                        OR ci.content_item_id in (select content_item_id from waiting_for_approval with(nolock) where user_id = @userId)
                    )";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static List<DataRow> GetArticlesWaitingForApproval(DbConnection sqlConnection, string orderBy, int startRow, int pageSize, int userId, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var ns = DbSchemaName(dbType);
            var withNoLock = WithNoLock(dbType);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                $@"ci.content_item_id as ID
                                    ,ci.content_id as ParentId
                                    ,{ns}.qp_get_article_title_func(ci.content_item_id, c.content_id) as Title
                                    ,s.site_name as SiteName
                                    ,c.CONTENT_NAME as ContentName
                                    ,ci.MODIFIED as Modified
                                    ,ci.CREATED as Created
                                    ,typ.STATUS_TYPE_NAME as StatusName
                                    ,us.LOGIN as LastModifiedByUser
                                    ,ci.PERMANENT_LOCK as IsPermanentLock",
                $@"content_item_workflow ciw {withNoLock}
                                    INNER JOIN content_item ci {withNoLock} ON ci.content_item_id = ciw.content_item_id
                                    INNER JOIN full_workflow_rules wr {withNoLock} on ciw.workflow_id = wr.workflow_id AND ci.status_type_id = wr.successor_status_id
                                    INNER JOIN full_workflow_rules wr2 {withNoLock} on wr.workflow_id = wr2.workflow_id AND wr2.rule_order = wr.rule_order + 1
                                    INNER JOIN content c {withNoLock} ON ci.content_id = c.content_id
                                    INNER JOIN site s {withNoLock} ON c.site_id = s.site_id
                                    INNER JOIN status_type as typ {withNoLock} on typ.STATUS_TYPE_ID = ci.STATUS_TYPE_ID
                                    INNER JOIN users as us on us.USER_ID = ci.LAST_MODIFIED_BY",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "SiteName ASC, ID ASC",
                $@"(wr2.user_id = {userId}
                                            OR wr2.group_id IN (SELECT group_id FROM user_group_bind {withNoLock} WHERE user_id={userId})
                                        )
                                        AND (
                                            ci.content_item_id not in (select content_item_id from waiting_for_approval {withNoLock})
                                            OR ci.content_item_id in (select content_item_id from waiting_for_approval {withNoLock} where user_id = {userId})
                                        )",
                startRow,
                pageSize,
                out totalRecords
            ).ToList();
        }

        internal static IEnumerable<DataRow> GetSimplePagedListOld(DbConnection sqlConnection,
            string entityTypeCode, string selectBlock, string fromBlock, string orderBy, string filter, int startRow,
            int pageSize, out int totalRecords,
            int userId = 0, bool useSecurity = false, bool countOnly = false, int groupId = 0,
            int startLevel = PermissionLevel.List, int endLevel = PermissionLevel.FullAccess,
            string parentEntityTypeCode = "", int parentEntityId = 0, IEnumerable<int> selectedIds = null,
            List<DbParameter> sqlParameters = null)
        {
            var forceCountQuery = entityTypeCode == "content_item" && (filter == "c.archive = 0" || string.IsNullOrEmpty(filter));
            using (var cmd = DbCommandFactory.Create("qp_get_paged_data", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("select_block", selectBlock);
                cmd.Parameters.AddWithValue("from_block", fromBlock);
                cmd.Parameters.AddWithValue("entity_name", entityTypeCode);

                if (!string.IsNullOrEmpty(filter))
                {
                    cmd.Parameters.AddWithValue("where_block", filter);
                }

                if (!string.IsNullOrEmpty(orderBy))
                {
                    cmd.Parameters.AddWithValue("order_by_block", orderBy);
                }

                cmd.Parameters.AddWithValue("count_only", countOnly);
                cmd.Parameters.Add(new SqlParameter("total_records", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                });

                cmd.Parameters.AddWithValue("start_row", startRow);
                cmd.Parameters.AddWithValue("page_size", pageSize);
                if (useSecurity)
                {
                    cmd.Parameters.AddWithValue("use_security", true);
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
                {
                    cmd.Parameters.AddRange(sqlParameters.ToArray());
                }

                var ds = new DataSet();
                DataAdapterFactory.Create(cmd).Fill(ds);

                DataTable result = null;
                totalRecords = 0;

                if (ds.Tables.Count > 0)
                {
                    result = ds.Tables[0];
                    totalRecords = forceCountQuery
                        ? (int)cmd.Parameters["total_records"].Value
                        : (result.Rows.Count != 0 ? (int)result.Rows[0][QPModelDataContext.CountColumn] : 0);
                }
                else if (countOnly)
                {
                    totalRecords = (int)cmd.Parameters["total_records"].Value;
                }

                return result.AsEnumerable().ToArray();
            }
        }

        internal static IEnumerable<DataRow> GetSimplePagedList(
            DbConnection sqlConnection,
            string entityTypeCode,
            string selectBlock,
            string fromBlock,
            string orderBy,
            string filter,
            int startRow,
            int pageSize,
            out int totalRecords,
            int userId = 0,
            bool useSecurity = false,
            bool countOnly = false,
            int groupId = 0,
            int startLevel = PermissionLevel.List,
            int endLevel = PermissionLevel.FullAccess,
            string parentEntityTypeCode = "",
            int parentEntityId = 0,
            List<int> selectedIds = null,
            IList<DbParameter> sqlParameters = null,
            bool useSql2012Syntax = false,
            List<int> filterIds = null)
        {
            totalRecords = 0;
            DataTable result = null;
            var databaseType = GetDbType(sqlConnection);
            if (useSecurity)
            {
                var securitySql = GetPermittedItemsAsQuery(sqlConnection, userId, groupId, startLevel, endLevel, entityTypeCode, parentEntityTypeCode, parentEntityId);
                fromBlock = fromBlock.Replace("<$_security_insert_$>", securitySql);
            }

            var forceCountQuery = entityTypeCode == "content_item" && (filter == "c.archive = 0" || string.IsNullOrEmpty(filter));
            if (countOnly || forceCountQuery)
            {
                var countBuilder = new StringBuilder();
                countBuilder.Append("SELECT COUNT(*) from ");
                countBuilder.AppendLine(fromBlock);
                if (!string.IsNullOrEmpty(filter))
                {
                    countBuilder.Append("WHERE ");
                    countBuilder.Append(filter);
                }

                using (var countCmd = DbCommandFactory.Create(countBuilder.ToString(), sqlConnection))
                {
                    countCmd.CommandType = CommandType.Text;
                    countCmd.Parameters.Add(GetIdsDatatableParam("@itemIds", selectedIds, databaseType));
                    countCmd.Parameters.Add(GetIdsDatatableParam("@filterIds", filterIds, databaseType));

                    if (sqlParameters != null)
                    {
                        countCmd.Parameters.AddRange(sqlParameters.ToArray());
                    }



                    var countResult = countCmd.ExecuteScalar();
                    totalRecords = Convert.ToInt32(countResult);
                }
            }

            startRow = startRow <= 0 ? 1 : startRow;
            if (useSql2012Syntax)
            {
                startRow--;
            }

            var endRow = pageSize == 0 ? 0 : startRow + pageSize - 1;
            if (useSql2012Syntax && endRow > 0)
            {
                endRow++;
            }

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
                        sqlBuilder.AppendLine(@"FETCH NEXT (@endRow - @startRow) ROWS ONLY ");
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
                    {
                        sqlBuilder.AppendLine("WHERE 1 = 1");
                    }
                    if (startRow > 1)
                    {
                        sqlBuilder.AppendLine("AND ROW_NUMBER >= @startRow");
                    }
                    if (endRow > 0)
                    {
                        sqlBuilder.AppendLine("AND ROW_NUMBER <= @endRow");
                    }

                    sqlBuilder.AppendLine("ORDER BY ROW_NUMBER ASC");
                }

                using (var cmd = DbCommandFactory.Create(sqlBuilder.ToString(), sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@startRow", startRow);
                    cmd.Parameters.AddWithValue("@endRow", endRow);
                    cmd.Parameters.Add(GetIdsDatatableParam("@itemIds", selectedIds, databaseType));
                    cmd.Parameters.Add(GetIdsDatatableParam("@filterIds", filterIds, databaseType));

                    if (sqlParameters != null)
                    {
                        cmd.Parameters.AddRange(sqlParameters.ToArray());
                    }

                    // switch (databaseType)
                    // {
                    //     case DatabaseType.SqlServer:
                    //         cmd.Parameters.Add(new SqlParameter("@startRow", SqlDbType.Int) { Value = startRow });
                    //         cmd.Parameters.Add(new SqlParameter("@endRow", SqlDbType.Int) { Value = endRow });
                    //         cmd.Parameters.Add(new SqlParameter("@itemIds", SqlDbType.Structured)
                    //         {
                    //             TypeName = "Ids",
                    //             Value = IdsToDataTable(selectedIds)
                    //         });
                    //
                    //         cmd.Parameters.Add(new SqlParameter("@filterIds", SqlDbType.Structured)
                    //         {
                    //             TypeName = "Ids",
                    //             Value = IdsToDataTable(filterIds)
                    //         });
                    //
                    //         if (sqlParameters != null)
                    //         {
                    //             cmd.Parameters.AddRange(sqlParameters.ToArray());
                    //         }
                    //         break;
                    //     case DatabaseType.Postgres:
                    //         cmd.Parameters.Add(new NpgsqlParameter("@startRow", NpgsqlDbType.Integer) { Value = startRow });
                    //         cmd.Parameters.Add(new NpgsqlParameter("@endRow", NpgsqlDbType.Integer) { Value = endRow });
                    //         cmd.Parameters.Add(GetIdsDatatableParam("@itemIds", selectedIds, databaseType));
                    //         cmd.Parameters.Add(GetIdsDatatableParam("@filterIds", filterIds, databaseType));
                    //
                    //         if (sqlParameters != null)
                    //         {
                    //             cmd.Parameters.AddRange(sqlParameters.ToArray());
                    //         }
                    //         break;
                    //     default:
                    //         throw new ArgumentOutOfRangeException();
                    // }


                    var ds = new DataSet();
                    DataAdapterFactory.Create(cmd).Fill(ds);
                    if (ds.Tables.Count > 0)
                    {
                        result = ds.Tables[0];
                        if (!forceCountQuery)
                        {
                            totalRecords = result.Rows.Count != 0 ? Convert.ToInt32(result.Rows[0][QPModelDataContext.CountColumn]) : 0;
                        }
                    }
                }
            }

            return result.AsEnumerable().ToArray();
        }

        public static IEnumerable<DataRow> GetButtonTracePage(DbConnection sqlConnection, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
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
        public static IEnumerable<DataRow> GetRemovedEntitiesPage(DbConnection sqlConnection, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0) => GetSimplePagedList(
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

        /// <summary>
        /// Получить страницу Sessions
        /// </summary>
        public static IEnumerable<DataRow> GetSessionsPage(DbConnection sqlConnection, bool isFailed, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0) => GetSimplePagedList(
            sqlConnection,
            EntityTypeCode.CustomerCode,
            "[SESSION_ID] AS SessionId, [LOGIN] as [Login], [USER_ID] as [UserId], [START_TIME] as [StartTime], [END_TIME] as [EndTime], [IP]" +
            ", [BROWSER] as Browser, [SERVER_NAME] as ServerName, [AUTO_LOGGED] as AutoLogged, [SID] as [Sid], [IS_QP7] as IsQP7",
            "dbo.[SESSIONS_LOG]",
            !string.IsNullOrEmpty(orderBy) ? orderBy : "[SessionId] DESC",
            isFailed ? "[USER_ID] IS NULL" : "[USER_ID] IS NOT NULL",
            startRow,
            pageSize,
            out totalRecords
        );

        public static IEnumerable<DataRow> GetNotificationsPage(DbConnection sqlConnection, int contentId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);

            var selectBlock = $@"
NOTIFICATION_ID AS Id,
NOTIFICATION_NAME as {Escape(dbType, "Name")},
n.CREATED as Created,
n.MODIFIED as Modified,
n.no_email,
n.LAST_MODIFIED_BY as LastModifiedBy,
n.IS_EXTERNAL as IsExternal,
u2.LOGIN as LastModifiedByLogin,
FOR_CREATE as ForCreate,
FOR_DELAYED_PUBLICATION as ForDelayedPublication,
FOR_MODIFY as ForModify,
For_Remove as ForRemove,
for_status_changed as ForStatusChanged,
for_status_partially_changed as ForStatusPartiallyChanged,
for_frontend as ForFrontend,
n.GROUP_ID,
n.USER_ID,
n.email_attribute_id,
COALESCE(u.LOGIN, ug.GROUP_NAME, a.ATTRIBUTE_NAME) as Receiver";


            var fromBlock = $@"{DbSchemaName(dbType)}.NOTIFICATIONS n left outer join user_group ug on n.group_id = ug.group_id
                left outer join users u on n.user_id = u.user_id left outer join CONTENT_ATTRIBUTE as a on n.email_attribute_id = a.ATTRIBUTE_ID
                inner join users u2 on n.LAST_MODIFIED_BY = u2.user_id";


            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Notification,
                selectBlock,
                fromBlock,
                !string.IsNullOrEmpty(orderBy) ? orderBy : Escape(dbType, "Name"),
                $"n.CONTENT_ID = {contentId}",
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetVisualEditorPluginsPage(DbConnection sqlConnection, int contentId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);
            var escapedOrderColumnName = Escape(dbType, "ORDER");
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.VisualEditorPlugin,
                $"p.ID as Id, p.NAME as Name, p.DESCRIPTION as Description, p.URL as Url, p.{escapedOrderColumnName} as {escapedOrderColumnName}, p.CREATED as Created," +
                "p.MODIFIED as Modified, p.LAST_MODIFIED_BY as LastModifiedBy, u.LOGIN as LastModifiedByLogin",
                "VE_PLUGIN p inner join users u on p.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : escapedOrderColumnName,
                "",
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static int GetVisualEditorPluginMaxOrder(DbConnection sqlConnection)
        {
            using (var cmd = DbCommandFactory.Create("select MAX([ORDER]) FROM [dbo].[VE_PLUGIN]", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : (int)maxOrder;
            }
        }

        public static int GetVisualEditorStyleMaxOrder(DbConnection sqlConnection)
        {
            using (var cmd = DbCommandFactory.Create("select MAX([ORDER]) FROM [dbo].[VE_STYLE]", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : (int)maxOrder;
            }
        }

        public static IEnumerable<DataRow> GetDisplayColumns(DbConnection sqlConnection, int contentId)
        {
            var queryBuilder = new StringBuilder();
            var databaseType = GetDbType(sqlConnection);
            queryBuilder.Append(
                "SELECT ca.ATTRIBUTE_ID, ca.ATTRIBUTE_NAME, ca.ATTRIBUTE_TYPE_ID, rca.ATTRIBUTE_ID AS RELATED_ATTRIBUTE_ID, rca.ATTRIBUTE_TYPE_ID AS RELATED_ATTRIBUTE_TYPE_ID, ");
            queryBuilder.Append(
                " rca.ATTRIBUTE_NAME AS RELATED_ATTRIBUTE_NAME, rca.CONTENT_ID AS RELATED_CONTENT_ID, ca.IS_CLASSIFIER, ca.view_in_list, ");
            queryBuilder.Append(
                " rrca.ATTRIBUTE_ID AS RELATED_ATTRIBUTE_ID2, rrca.ATTRIBUTE_TYPE_ID AS RELATED_ATTRIBUTE_TYPE_ID2, rrca.ATTRIBUTE_NAME AS RELATED_ATTRIBUTE_NAME2, rrca.CONTENT_ID AS RELATED_CONTENT_ID2, ");
            queryBuilder.Append(
                " CAST(ROW_NUMBER() OVER(PARTITION BY rca.ATTRIBUTE_ID ORDER BY ca.ATTRIBUTE_ORDER ASC) AS NUMERIC) AS RELATED_COUNT ");
            queryBuilder.Append(" FROM CONTENT_ATTRIBUTE AS ca ");
            queryBuilder.Append(" LEFT JOIN CONTENT_ATTRIBUTE AS rca ON rca.ATTRIBUTE_ID = ca.RELATED_ATTRIBUTE_ID ");
            queryBuilder.Append(" LEFT JOIN CONTENT_ATTRIBUTE AS rrca ON rrca.ATTRIBUTE_ID = rca.RELATED_ATTRIBUTE_ID ");
            queryBuilder.Append($" WHERE ca.CONTENT_ID = @content_id AND ca.view_in_list = {SqlQuerySyntaxHelper.ToBoolSql(databaseType, true)}");
            queryBuilder.Append(" ORDER BY ca.permanent_flag DESC, ca.attribute_order ASC");
            using (var cmd = DbCommandFactory.Create(queryBuilder.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable();
            }
        }

        public static bool IsArticlePermissionsAllowed(DbConnection sqlConnection, int contentId)
        {
            var dbType = GetDbType(sqlConnection);
            using (var cmd = DbCommandFactory.Create($"select allow_items_permission from content {WithNoLock(dbType)} where content_id = " + contentId, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var objResult = cmd.ExecuteScalar();
                return Converter.ToBoolean(objResult);
            }
        }

        public static IEnumerable<DataRow> GetArticlesPage(DbConnection sqlConnection, ArticlePageOptions options, IList<DbParameter> sqlParams, out int totalRecords)
        {
            var databaseType = GetDbType(sqlConnection);
            var selectBuilder = new StringBuilder();
            var fromBuilder = new StringBuilder();
            var whereBuilder = new StringBuilder(SqlFilterComposer.Compose(options.CommonFilter, options.ContextFilter));

            Dictionary<int, string> fieldMap = null;
            if (options.ExtensionContentIds.Any())
            {
                fieldMap = GetAggregatedFieldNames(sqlConnection, options.ExtensionContentIds);
            }

            Dictionary<int, string> referenceMap = null;
            if (options.ContentReferences.Any())
            {
                referenceMap = GetFieldNames(sqlConnection, options.ContentReferences.Select(r => r.ReferenceFieldId).ToArray());
            }

            var useSelection = options.SelectedIDs != null && options.SelectedIDs.Any();
            AddStaticColumnsToQuery(databaseType, options, useSelection, selectBuilder);
            AddSourcesToQuery(databaseType, options, useSelection, fromBuilder, fieldMap, referenceMap);
            AddDynamicColumnsToQuery(sqlConnection, options, selectBuilder, fromBuilder, Default.MaxViewInListFieldLength + 1);

            var useFullText = AddFullTextFilteringToQuery(sqlConnection, options.FullTextSearch, options.ContentId, options.ExtensionContentIds, fromBuilder);
            AddLinkFilteringToQuery(options.LinkFilters, whereBuilder, sqlParams, fieldMap, referenceMap, databaseType);
            AddRelationSecurityFilteringToQuery(sqlConnection, options, fromBuilder, whereBuilder);
            if (options.FilterIds != null && options.FilterIds.Any())
            {
                whereBuilder.Append($" AND c.CONTENT_ITEM_ID IN (SELECT ID FROM {(databaseType == DatabaseType.Postgres ? "unnest(@filterIds) i(id)" : "@filterIds")})");
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
                selectedIds: options.SelectedIDs.ToList(),
                filterIds: options.FilterIds.ToList(),
                sqlParameters: sqlParams,
                useSql2012Syntax: options.UseSql2012Syntax
            );

            DropFullTextTemporaryTable(sqlConnection, useFullText);
            return result;
        }

        public static void AddLinkFilteringToQuery(
            IEnumerable<ArticleLinkSearchParameter> linkFilters,
            StringBuilder whereBuilder,
            ICollection<DbParameter> sqlParams,
            IDictionary<int, string> fieldMap = null,
            IDictionary<int, string> referenceMap = null,
            DatabaseType dbType = DatabaseType.SqlServer)
        {
            if (linkFilters == null)
            {
                return;
            }

            var ns = DbSchemaName(dbType);

            foreach (var linkFilter in linkFilters)
            {
                var tableAlias = "c";
                if (fieldMap != null && fieldMap.ContainsKey(linkFilter.ExtensionContentId))
                {
                    tableAlias += "_" + linkFilter.ExtensionContentId;
                    if (referenceMap != null && referenceMap.ContainsKey(linkFilter.ReferenceFieldId))
                    {
                        tableAlias += "_" + linkFilter.ReferenceFieldId;
                    }
                }

                string internalSql;
                string inverseString;
                if (!linkFilter.IsNull)
                {
                    var paramName = "@link" + linkFilter.LinkId;
                    var unionAllSqlString = linkFilter.UnionAll ? $" GROUP BY item_id HAVING COUNT(item_id) = (SELECT COUNT(*) FROM {IdList(dbType, paramName)})" : string.Empty;
                    sqlParams.Add(GetIdsDatatableParam(paramName, linkFilter.Ids, dbType));

                    inverseString = linkFilter.Inverse ? "NOT " : string.Empty;
                    internalSql = linkFilter.IsManyToMany
                        ? $"{inverseString} EXISTS (select item_id from {ns}.item_link_united {WithNoLock(dbType)} where {tableAlias}.content_item_id = item_id and link_id = {linkFilter.LinkId} AND linked_item_id in (select id from {IdList(dbType, paramName)}){unionAllSqlString})"
                        : $"{inverseString} EXISTS (select * from content_{linkFilter.ContentId}_united cu {WithNoLock(dbType)} where {tableAlias}.content_item_id = {Escape(dbType, linkFilter.FieldName)} and cu.content_item_id in (select id from {IdList(dbType, paramName)})) ";
                }
                else
                {
                    inverseString = linkFilter.Inverse ? string.Empty : "NOT ";
                    internalSql = linkFilter.IsManyToMany
                        ? $"{inverseString}EXISTS (select item_id from {ns}.item_link_united {WithNoLock(dbType)} where {tableAlias}.content_item_id = item_id and link_id = {linkFilter.LinkId})"
                        : $"{inverseString}EXISTS (select * from content_{linkFilter.ContentId}_united {WithNoLock(dbType)} where {tableAlias}.content_item_id = {Escape(dbType, linkFilter.FieldName)}) ";
                }

                if (whereBuilder.Length != 0)
                {
                    whereBuilder.Append(" AND ");
                }

                whereBuilder.Append(internalSql);
            }
        }

        private static void AddRelationSecurityFilteringToQuery(DbConnection connection, ArticlePageOptions options, StringBuilder fromBuilder, StringBuilder whereBuilder)
        {
            if (options.RelationSecurityFilters != null)
            {
                foreach (var filter in options.RelationSecurityFilters)
                {
                    AddRelationSecurityFilteringToQuery(connection, filter, options.UserId, fromBuilder, whereBuilder);
                }
            }
        }

        private static void AddRelationSecurityFilteringToQuery(DbConnection connection, ArticleRelationSecurityParameter filter, int userId, StringBuilder fromBuilder, StringBuilder whereBuilder)
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
                fromBuilder.AppendFormatLine(" inner join (select distinct linked_item_id from content_{0}_united link_sec_{1} with(nolock) ", filter.RelatedContentId, filter.FieldId);
                fromBuilder.AppendFormatLine(" inner join item_link links_{1} with(nolock) on link_sec_{1}.content_item_id = links_{1}.item_id and links_{1}.link_id = {0} ", filter.LinkId, filter.FieldId);
                fromBuilder.AppendFormatLine(" inner join ({0}) pi_{1} on link_sec_{1}.content_item_id = pi_{1}.content_item_id ", securitySql, filter.FieldId);
                fromBuilder.AppendFormatLine(" ) as sec_items_{0} on c.content_item_id = sec_items_{0}.linked_item_id ", filter.FieldId);
            }
            else
            {
                fromBuilder.AppendFormatLine(" inner join content_{0}_united rel_sec_{1} with(nolock) on c.[{2}] = rel_sec_{1}.content_item_id", filter.RelatedContentId, filter.FieldId, filter.FieldName);
                fromBuilder.AppendFormatLine(" inner join ({0}) pi_{1} on rel_sec_{1}.content_item_id = pi_{1}.content_item_id ", securitySql, filter.FieldId);
            }
        }

        private static void DropFullTextTemporaryTable(DbConnection sqlConnection, bool useFullText)
        {
            if (useFullText)
            {
                using (var immediateCmd = DbCommandFactory.Create("DROP TABLE #ft_temp", sqlConnection))
                {
                    immediateCmd.ExecuteNonQuery();
                }
            }
        }

        public static bool AddFullTextFilteringToQuery(DbConnection cn, ArticleFullTextSearchParameter ftsOptions, int contentId, int[] extensionContentIds, StringBuilder fromBuilder)
        {
            var useFullText = !string.IsNullOrEmpty(ftsOptions.QueryString) && !(ftsOptions.HasError.HasValue && ftsOptions.HasError.Value);
            if (useFullText)
            {
                var sb = new StringBuilder();
                sb.Append("CREATE TABLE #ft_temp (content_item_id decimal primary key); ");
                sb.Append("insert into #ft_temp ");
                sb.Append(GetFullTextSearchQuery(cn, contentId, extensionContentIds, ftsOptions));

                using (var cmd = DbCommandFactory.Create(sb.ToString(), cn))
                {
                    cmd.ExecuteNonQuery();
                }

                fromBuilder.AppendLine(" INNER JOIN #ft_temp as qp_fts ON c.content_item_id = qp_fts.content_item_id");
            }

            return useFullText;
        }

        public static IList<int> GetFilterAndFtsSearchResult(DbConnection cn, int contentId, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions, string searchFilterQuery, ICollection<DbParameter> filterSqlParams)
        {
            var ftsQuery = string.IsNullOrWhiteSpace(ftsOptions.QueryString) ? string.Empty : GetFullTextSearchQuery(cn, contentId, extensionContentIds, ftsOptions);
            var unionQuery = string.IsNullOrWhiteSpace(ftsQuery) || string.IsNullOrWhiteSpace(searchFilterQuery)
                ? $"{ftsQuery}{searchFilterQuery}"
                : $"{ftsQuery} INTERSECT {searchFilterQuery}";

            return GetDatatableResult(cn, unionQuery, filterSqlParams.ToArray()).Select(dr => (int)dr.Field<decimal>(0)).Distinct().ToList();
        }

        public static string GetFullTextSearchQuery(DbConnection cn, int contentId, int[] extensionContentIds, ArticleFullTextSearchParameter ftsOptions)
        {
            string allContentIds;
            IDictionary<int, string> aggregatedFieldMap = null;
            var allfields = string.IsNullOrEmpty(ftsOptions.FieldIdList);
            var textfields = !allfields && ftsOptions.FieldIdList.Contains(",");
            if (allfields || textfields)
            {
                extensionContentIds = GetReferencedAggregatedContentIds(null, cn, contentId, null);
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

        private static void AddDynamicColumnsToQuery(DbConnection sqlConnection, ArticlePageOptions options, StringBuilder selectBuilder, StringBuilder fromBuilder, int maxDisplayLength)
        {
            if (options.OnlyIds)
            {
                return;
            }

            var databaseType = GetDbType(sqlConnection);

            foreach (var row in GetDisplayColumns(sqlConnection, options.ContentId))
            {
                var viewInList = row.Field<bool>("view_in_list");
                var fieldType = row.Field<decimal>("ATTRIBUTE_TYPE_ID");
                var fieldName = row.Field<string>("ATTRIBUTE_NAME");
                var isFieldAClassifier = row.Field<bool>("IS_CLASSIFIER");
                if (isFieldAClassifier)
                {
                    selectBuilder.AppendFormat($", {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, $"cnt_{fieldName}")}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "CONTENT_NAME")} as {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)}", fieldName);
                    fromBuilder.AppendFormatLine($" LEFT JOIN CONTENT AS cnt_{fieldName} ON cnt_{fieldName}.content_id = c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)} ");
                }
                else
                {
                    if (viewInList && (fieldType == FieldTypeCodes.VisualEdit || fieldType == FieldTypeCodes.Textbox))
                    {
                        selectBuilder.AppendFormat($", substring(c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)}, 1, {maxDisplayLength}) as {SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)}", fieldName, maxDisplayLength);
                    }
                    else
                    {
                        selectBuilder.Append($", c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)}");
                    }
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
                    var currentBlock = GetCurrentBlock(databaseType, tableAlias, relatedFieldName, attributeTypeId);

                    selectBuilder.AppendFormat(", {0} AS {1}", currentBlock, fieldAlias);
                    fromBuilder.AppendLine($" LEFT JOIN CONTENT_{relatedContentId}_UNITED AS {tableAlias} {WithNoLock(databaseType)} ON c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, fieldName)} = {tableAlias}.content_item_id ");

                    var relatedAttributeId2 = (int?)row.Field<decimal?>("RELATED_ATTRIBUTE_ID2");
                    if (relatedAttributeId2.HasValue)
                    {
                        var relatedFieldName2 = row.Field<string>("RELATED_ATTRIBUTE_NAME2");
                        var relatedContentId2 = (int)row.Field<decimal>("RELATED_CONTENT_ID2");
                        var attributeTypeId2 = (int)row.Field<decimal>("RELATED_ATTRIBUTE_TYPE_ID2");

                        var tableAlias2 = tableAlias + "_r1";
                        var fieldAlias2 = fieldAlias + "_r1";
                        var currentBlock2 = GetCurrentBlock(databaseType, tableAlias2, relatedFieldName2, attributeTypeId2);

                        selectBuilder.AppendFormat(", {0} AS {1}", currentBlock2, fieldAlias2);
                        fromBuilder.AppendLine($" LEFT JOIN CONTENT_{relatedContentId2}_UNITED AS {tableAlias2} {WithNoLock(databaseType)} ON {tableAlias}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, relatedFieldName)} = {tableAlias2}.content_item_id ");
                    }
                }
            }
        }

        private static string GetCurrentBlock(DatabaseType databaseType, string tableAlias, string relatedFieldName, int attributeTypeId)
        {
            var currentBlock = $"{tableAlias}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, relatedFieldName)}";
            if (attributeTypeId == FieldTypeCodes.Textbox || attributeTypeId == FieldTypeCodes.VisualEdit)
            {
                currentBlock = $"cast ({currentBlock} as {(databaseType == DatabaseType.Postgres ? "varchar" : "nvarchar")}(255))";
            }

            return currentBlock;
        }

        private static void AddSourcesToQuery(DatabaseType databaseType, ArticlePageOptions options, bool useSelection, StringBuilder fromBuilder, IDictionary<int, string> fieldMap, IDictionary<int, string> referenceMap)
        {
            var isPostgres = databaseType == DatabaseType.Postgres;
            var tablePrefix = options.UseMainTableForVariations ? "c" : "ch";
            fromBuilder.AppendLine($" {DbSchemaName(databaseType)}.CONTENT_{options.ContentId}_UNITED c {WithNoLock(databaseType)}");

            if (!options.UseMainTableForVariations)
            {
                fromBuilder.AppendLine($" INNER JOIN CONTENT_{options.ContentId}_UNITED ch {WithNoLock(databaseType)} on c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, options.VariationFieldName)} = ch.CONTENT_ITEM_ID");
            }

            if (!options.IsVirtual)
            {
                fromBuilder.AppendLine($" LEFT JOIN CONTENT_ITEM cil {WithNoLock(databaseType)} on {tablePrefix}.CONTENT_ITEM_ID = cil.CONTENT_ITEM_ID AND LOCKED_BY IS NOT NULL");
                fromBuilder.AppendLine($" LEFT JOIN CONTENT_{options.ContentId}_ASYNC ca {WithNoLock(databaseType)} on {tablePrefix}.CONTENT_ITEM_ID = ca.CONTENT_ITEM_ID");
                fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "USERS")} lu {WithNoLock(databaseType)} ON cil.LOCKED_BY = lu.USER_ID");
                fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "CONTENT_ITEM_SCHEDULE")} sch {WithNoLock(databaseType)} ON {tablePrefix}.CONTENT_ITEM_ID = sch.CONTENT_ITEM_ID");
            }

            fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "USERS")} mu {WithNoLock(databaseType)} ON {tablePrefix}.LAST_MODIFIED_BY = mu.USER_ID");
            fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "STATUS_TYPE")} st {WithNoLock(databaseType)} ON {tablePrefix}.STATUS_TYPE_ID = st.STATUS_TYPE_ID");

            if (useSelection)
            {
                fromBuilder.AppendLine($" LEFT OUTER JOIN (SELECT CONTENT_ITEM_ID from CONTENT_ITEM {WithNoLock(databaseType)} where CONTENT_ITEM_ID in (select id from {(databaseType == DatabaseType.Postgres ? "unnest(@itemIds) i(id)" : "@itemIds")})) AS cis ON {tablePrefix}.CONTENT_ITEM_ID = cis.CONTENT_ITEM_ID ");
            }

            if (options.UseSecurity)
            {
                const string innerSql = "SELECT sec.content_item_id AS ALLOWED_CONTENT_ITEM_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec";
                fromBuilder.AppendLine($" INNER JOIN ({innerSql}) AS pl ON {tablePrefix}.CONTENT_ITEM_ID = pl.ALLOWED_CONTENT_ITEM_ID");
            }

            foreach (var reference in options.ContentReferences.Where(reference => referenceMap.ContainsKey(reference.ReferenceFieldId)))
            {
                fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.CONTENT_{reference.TargetContentId}_UNITED c_{reference.TargetContentId}_{reference.ReferenceFieldId} {WithNoLock(databaseType)} ON c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, referenceMap[reference.ReferenceFieldId])} = c_{reference.TargetContentId}_{reference.ReferenceFieldId}.CONTENT_ITEM_ID");
            }

            foreach (var contentId in options.ExtensionContentIds)
            {
                if (fieldMap.ContainsKey(contentId))
                {
                    fromBuilder.AppendLine($" LEFT JOIN {DbSchemaName(databaseType)}.CONTENT_{contentId}_UNITED c_{contentId} {WithNoLock(databaseType)} ON c.CONTENT_ITEM_ID = c_{contentId}.{fieldMap[contentId]}");
                }
            }
        }

        private static void AddStaticColumnsToQuery(DatabaseType databaseType, ArticlePageOptions options, bool useSelection, StringBuilder selectBuilder)
        {
            var tablePrefix = options.UseMainTableForVariations ? "c" : "ch";
            if (options.OnlyIds)
            {
                selectBuilder.Append($" {tablePrefix}.CONTENT_ITEM_ID ");
            }
            else
            {
                const string colorString = ", CASE WHEN st.COLOR IS NOT NULL AND ST.ALT_COLOR IS NOT NULL THEN ST.STATUS_TYPE_ID ELSE NULL END AS STATUS_TYPE_COLOR";
                selectBuilder.Append(
                    $" {tablePrefix}.CONTENT_ITEM_ID, {tablePrefix}.CREATED, {tablePrefix}.MODIFIED, {tablePrefix}.LAST_MODIFIED_BY, st.STATUS_TYPE_NAME{colorString}, {SqlQuerySyntaxHelper.CastToBool(databaseType, $"{tablePrefix}.visible")} as visible"
                );

                var falseValue = SqlQuerySyntaxHelper.ToBoolSql(databaseType, false);
                if (!options.IsVirtual)
                {
                    selectBuilder.Append(", cil.LOCKED_BY");
                    selectBuilder.Append($", {SqlQuerySyntaxHelper.CastToBool(databaseType, "(CASE WHEN (sch.content_item_id IS NOT NULL) THEN 1 ELSE 0 END)")} AS scheduled");
                    selectBuilder.Append($", {SqlQuerySyntaxHelper.CastToBool(databaseType, "(CASE WHEN (ca.content_item_id IS NOT NULL) THEN 1 ELSE 0 END)")} AS splitted");
                    selectBuilder.Append($", lu.FIRST_NAME AS LOCKER_FIRST_NAME, lu.LAST_NAME AS LOCKER_LAST_NAME, lu.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "LOGIN")} AS LOCKER_LOGIN");
                }
                else
                {
                    var nullStringValue = SqlQuerySyntaxHelper.CastToString(databaseType, "NULL");
                    selectBuilder.Append($", cast(NULL as numeric) AS LOCKED_BY, {falseValue} AS SCHEDULED, {falseValue} AS SPLITTED");
                    selectBuilder.Append($", {nullStringValue} AS LOCKER_FIRST_NAME, {nullStringValue} AS LOCKER_LAST_NAME, {nullStringValue} AS LOCKER_LOGIN");
                }

                selectBuilder.Append($", mu.FIRST_NAME AS MODIFIER_FIRST_NAME, mu.LAST_NAME AS MODIFIER_LAST_NAME ,mu.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "LOGIN")} AS MODIFIER_LOGIN");
                selectBuilder.Append(useSelection
                    ? $", {SqlQuerySyntaxHelper.CastToBool(databaseType, "(CASE WHEN (cis.CONTENT_ITEM_ID IS NOT NULL) THEN 1 ELSE 0 END)")} as is_selected"
                    : $", {falseValue} as is_selected");
            }
        }

        public static IEnumerable<DataRow> GetFieldsPage(DbConnection sqlConnection, FieldPageOptions options, out int totalRecords)
        {
            var aggregatedContentIds = GetReferencedAggregatedContentIds(sqlConnection, options.ContentId);
            var dbType = GetDbType(sqlConnection);

            var useSelection = options.SelectedIDs != null && options.SelectedIDs.Any();
            var filter = "cnt.CONTENT_ID = " + options.ContentId;

            if (options.Mode == FieldSelectMode.ForExport || options.Mode == FieldSelectMode.ForExportExpanded)
            {
                filter = "cnt.CONTENT_ID IN (" + string.Join(",", aggregatedContentIds.Union(new[] { options.ContentId })) + ")";
                filter = SqlFilterComposer.Compose(filter, $"ca.AGGREGATED = {SqlQuerySyntaxHelper.ToBoolSql(dbType, false)}");
                filter = options.Mode == FieldSelectMode.ForExport ? SqlFilterComposer.Compose(filter, "ca.ATTRIBUTE_TYPE_ID <> 13") : SqlFilterComposer.Compose(filter, "ca.ATTRIBUTE_TYPE_ID in (11, 13)");
            }


            var selectBuilder = new StringBuilder();
            selectBuilder.Append($@"ca.ATTRIBUTE_ID AS Id,
  CASE WHEN (cnt.CONTENT_ID = {options.ContentId}) THEN ATTRIBUTE_NAME ELSE {SqlQuerySyntaxHelper.ConcatStrValues(dbType, "cnt.CONTENT_NAME", "'.'", "ATTRIBUTE_NAME")} END as Name,
 ATTRIBUTE_NAME as FieldName,
 cnt.CONTENT_NAME as ContentName,
 ca.CREATED as Created,
 ca.MODIFIED as Modified,
 ATTRIBUTE_ORDER as {Escape(dbType, "Order")}");
            selectBuilder.Append(", FRIENDLY_NAME as FriendlyName, ca.DESCRIPTION as Description, lmb.LOGIN as LastModifiedByUser, ca.ATTRIBUTE_TYPE_ID as TypeCode");
            selectBuilder.Append(", ATTRIBUTE_SIZE as Size, REQUIRED as Required, INDEX_FLAG as Indexed, MAP_AS_PROPERTY as MapAsProperty, VIEW_IN_LIST as ViewInList, at.icon AS TypeIcon, ca.LINK_ID as LinkId");
            if (useSelection)
            {
                selectBuilder.Append(", CASE WHEN (cas.ATTRIBUTE_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");
            }

            var fromBuilder = new StringBuilder();
            fromBuilder.Append("CONTENT cnt INNER JOIN CONTENT_ATTRIBUTE ca ON cnt.CONTENT_ID = ca.CONTENT_ID");
            fromBuilder.Append($" INNER JOIN {DbSchemaName(dbType)}.{Escape(dbType, "USERS")} lmb ON ca.LAST_MODIFIED_BY = lmb.USER_ID");
            fromBuilder.Append(" INNER JOIN ATTRIBUTE_TYPE at on ca.attribute_type_id = at.attribute_type_id");
            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT ATTRIBUTE_ID from CONTENT_ATTRIBUTE where ATTRIBUTE_ID in ({0})) AS cas ON ca.ATTRIBUTE_ID = cas.ATTRIBUTE_ID ", string.Join(",", options.SelectedIDs));
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Field,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : Escape(dbType, "Order"),
                filter,
                options.StartRecord,
                options.PageSize,
                out totalRecords);
        }

        public static IEnumerable<DataRow> GetContentsPage(DbConnection sqlConnection, ContentPageOptions options, out int totalRecords)
        {
            var useSelection = options.SelectedIDs != null && options.SelectedIDs.Any();
            var dbType = GetDbType(sqlConnection);
            var selectBuilder = new StringBuilder();

            selectBuilder.Append("c.CONTENT_ID as Id, c.CONTENT_NAME as Name, c.DESCRIPTION as Description, c.CREATED as Created, c.MODIFIED as Modified, c.VIRTUAL_TYPE as VirtualType");
            selectBuilder.AppendFormat(", s.SITE_NAME as SiteName, cg.Name as GroupName, U.LOGIN as LastModifiedByUser", options.LanguageId);
            if (useSelection)
            {
                selectBuilder.Append(", CASE WHEN (cis.CONTENT_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");
            }

            var fromBuilder = new StringBuilder();
            var ns = DbSchemaName(dbType);
            fromBuilder.Append($"{ns}.CONTENT c INNER JOIN {ns}.SITE s ON c.SITE_ID = s.SITE_ID");
            fromBuilder.Append($" INNER JOIN {ns}.{Escape(dbType,"USERS")} u ON c.LAST_MODIFIED_BY = U.USER_ID");
            fromBuilder.Append($" LEFT JOIN {ns}.CONTENT_GROUP cg ON c.CONTENT_GROUP_ID = cg.CONTENT_GROUP_ID");
            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT CONTENT_ID from CONTENT where CONTENT_ID in ({0})) AS cis ON c.CONTENT_ID = cis.CONTENT_ID ", string.Join(",", options.SelectedIDs));
            }

            if (options.Mode == ContentSelectMode.ForWorkflow)
            {
                fromBuilder.Append($" LEFT JOIN {ns}.CONTENT_WORKFLOW_BIND cwb ON c.CONTENT_ID = cwb.CONTENT_ID");
            }

            if (options.UseSecurity)
            {
                const string innerSql = "SELECT sec.content_id AS ALLOWED_CONTENT_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec WHERE HIDE = 0";
                fromBuilder.AppendFormat(" INNER JOIN ({0}) AS pl ON c.CONTENT_ID = pl.ALLOWED_CONTENT_ID ", innerSql);
            }

            var filterBuilder = new StringBuilder();
            if (options.GroupId.HasValue)
            {
                if (options.GroupId.Value > 0)
                {
                    filterBuilder.AppendFormat("C.CONTENT_GROUP_ID = {0} AND ", options.GroupId.Value);
                }
                else
                {
                    filterBuilder.Append("C.CONTENT_GROUP_ID IS NULL AND ");
                }
            }

            if (!string.IsNullOrWhiteSpace(options.ContentName))
            {
                filterBuilder.AppendFormat("C.CONTENT_NAME LIKE '%{0}%' AND ", Cleaner.ToSafeSqlLikeCondition(options.ContentName));
            }

            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);
            switch (options.Mode)
            {
                case ContentSelectMode.ForUnion:
                    filterBuilder.Append($"((C.SITE_ID = {options.SiteId} or c.is_shared = {trueValue}) AND VIRTUAL_TYPE IN (0, 1))");
                    break;

                case ContentSelectMode.ForJoin:
                    filterBuilder.Append($"((C.SITE_ID = {options.SiteId} or c.is_shared = {trueValue}) AND VIRTUAL_TYPE = 0)");
                    break;

                case ContentSelectMode.ForField:
                    filterBuilder.AppendFormat("(C.SITE_ID = {0})", options.SiteId);
                    break;

                case ContentSelectMode.ForContainer:
                    filterBuilder.Append($"(C.SITE_ID = {options.SiteId} or c.is_shared = {trueValue}) ");
                    break;

                case ContentSelectMode.ForForm:
                    filterBuilder.Append($"((C.SITE_ID = {options.SiteId} or c.is_shared = {trueValue}) AND c.VIRTUAL_TYPE = 0) ");
                    break;

                case ContentSelectMode.ForCustomAction:
                    filterBuilder.AppendFormat("1 = 1");
                    break;

                default:
                    if (options.SiteId.HasValue)
                    {
                        filterBuilder.AppendFormat(" C.SITE_ID = {0} AND ", options.SiteId.Value);
                    }

                    filterBuilder.AppendFormat(" C.VIRTUAL_TYPE {0} 0 ", options.IsVirtual ? "<>" : "=");
                    break;
            }

            if (!string.IsNullOrEmpty(options.CustomFilter))
            {
                filterBuilder.AppendFormat(" AND ({0})", options.CustomFilter);
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Content,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : (useSelection ? "isSelected DESC, Name ASC" : "Name ASC"),
                filterBuilder.ToString(),
                options.StartRecord,
                options.PageSize,
                out totalRecords,
                options.UserId,
                options.UseSecurity
            );
        }

        public static IEnumerable<DataRow> GetSitesPage(DbConnection sqlConnection, SitePageOptions options, out int totalRecords)
        {
            var useSelection = options.SelectedIDs != null && options.SelectedIDs.Any();
            var selectBuilder = new StringBuilder();
            var dbType = GetDbType(sqlConnection);

            selectBuilder.Append("s.SITE_ID as Id, s.SITE_NAME as Name, s.DESCRIPTION as Description, s.CREATED as Created, s.MODIFIED as Modified, U.LOGIN as LastModifiedByUser");
            selectBuilder.Append($", s.DNS as Dns, s.LIVE_VIRTUAL_ROOT as UploadUrl, s.IS_LIVE as IsLive, s.LOCKED_BY as LockedBy, {SqlQuerySyntaxHelper.ConcatStrValues(dbType, "u2.FIRST_NAME", "' '", "u2.LAST_NAME")} as LockedByFullName ");
            if (useSelection)
            {
                selectBuilder.Append(", CASE WHEN (cis.SITE_ID IS NOT NULL) THEN 1 ELSE 0 END as isSelected");
            }

            var fromBuilder = new StringBuilder();
            fromBuilder.Append($"{DbSchemaName(dbType)}.SITE s INNER JOIN {DbSchemaName(dbType)}.{Escape(dbType, "USERS")} u ON s.LAST_MODIFIED_BY = u.USER_ID");
            fromBuilder.Append($" LEFT JOIN {DbSchemaName(dbType)}.{Escape(dbType, "USERS")} u2 ON s.LOCKED_BY = u2.USER_ID");

            if (useSelection)
            {
                fromBuilder.AppendFormat(" LEFT OUTER JOIN (SELECT SITE_ID from SITE s where SITE_ID in ({0})) AS cis ON s.SITE_ID = cis.SITE_ID ", string.Join(",", options.SelectedIDs));
            }

            if (options.UseSecurity)
            {
                const string innerSql = "SELECT sec.SITE_ID AS ALLOWED_SITE_ID, sec.permission_level AS PERMISSION_LEVEL FROM (<$_security_insert_$>) AS sec";
                fromBuilder.AppendFormat(" INNER JOIN ({0}) AS pl ON s.SITE_ID = pl.ALLOWED_SITE_ID", innerSql);
            }

            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Site,
                selectBuilder.ToString(),
                fromBuilder.ToString(),
                !string.IsNullOrEmpty(options.SortExpression) ? options.SortExpression : (useSelection ? "isSelected DESC, Name ASC" : "Name ASC"),
                string.Empty,
                options.StartRecord,
                options.PageSize,
                out totalRecords,
                options.UserId,
                options.UseSecurity
            );
        }

        public static IEnumerable<DataRow> GetToolbarButtonsForAction(QPModelDataContext context, DbConnection sqlConnection, int userId, bool isAdmin, int? actionId, string actionCode, int entityId)
        {

            var databaseType = GetDbType(context);
            #warning заглушка по security для постгрес
            var useSecurity = !isAdmin && databaseType != DatabaseType.Postgres;


            var query = $@"
        SELECT
			ba.ID AS ACTION_ID,
			ba.CODE AS ACTION_CODE,
			bat.CODE AS ACTION_TYPE_CODE,
			ba2.ID AS PARENT_ACTION_ID,
			ba2.CODE AS PARENT_ACTION_CODE,
			atb.NAME AS NAME,
			bat.ITEMS_AFFECTED,
			atb.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "ORDER")},
			COALESCE(ca.ICON_URL, atb.ICON) AS ICON,
			atb.ICON_DISABLED,
			atb.IS_COMMAND
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
            {(!useSecurity ? string.Empty : "AND (SEC.PERMISSION_LEVEL >= PL.PERMISSION_LEVEL or bat.CODE = ''refresh'')")}
			-- AND dbo.qp_action_visible(@p2, @p3, @p4, ba.CODE) = 1
		ORDER BY
			{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "ORDER")}
";

            // #warning реализовать для postgres
            // if (IsPostgresConnection(sqlConnection)) return new List<DataRow>();
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("user_id", userId);
                // cmd.Parameters.AddWithValue("action_code", actionCode);
                // cmd.Parameters.AddWithValue("entity_id", entityId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetActionStatusList(QPModelDataContext efContext, DbConnection sqlConnection, int userId, string actionCode, int? actionId, int entityId, string entityCode, bool isAdmin)
        {
            var useSecurity = !isAdmin;
            var databaseType = GetDbType(efContext);
            string query;

            // if (!useSecurity)
            // {
            #warning прикрутить security (из qp_get_action_status_list)
            query = $@"
        SELECT ba.CODE, {SqlQuerySyntaxHelper.ToBoolSql(databaseType, true)} as visible
		FROM ACTION_TOOLBAR_BUTTON atb
		INNER JOIN BACKEND_ACTION ba on ba.ID = atb.ACTION_ID
		INNER JOIN ACTION_TYPE at on ba.TYPE_ID = at.ID
		WHERE atb.PARENT_ACTION_ID = {actionId} AND at.items_affected = 1
";
            // }
            // else
            // {
            //     var secQuery = PermissionHelper.GetActionPermissionsAsQuery(efContext, userId);
            //
            // }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("user_id", userId);
                // cmd.Parameters.AddWithValue("action_code", actionCode);
                // cmd.Parameters.AddWithValue("entity_id", entityId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetMenuStatusList(DbConnection sqlConnection, int userId, string menuCode, int entityId)
        {
            using (var cmd = DbCommandFactory.Create("qp_get_menu_status_list", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_code", menuCode);
                cmd.Parameters.AddWithValue("entity_id", entityId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetChildTreeNodeList(DbConnection sqlConnection, int userId, string entityTypeCode, int? parentEntityId, bool isFolder, bool isGroup, string groupItemCode, int entityId = 0)
        {
            using (var cmd = DbCommandFactory.Create("qp_expand", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("code", string.IsNullOrEmpty(entityTypeCode) ? DBNull.Value : (object)entityTypeCode);
                cmd.Parameters.AddWithValue("id", parentEntityId ?? 0);
                cmd.Parameters.AddWithValue("is_folder", isFolder);
                cmd.Parameters.AddWithValue("is_group", isGroup);
                cmd.Parameters.AddWithValue("group_item_code", string.IsNullOrEmpty(groupItemCode) ? DBNull.Value : (object)groupItemCode);
                cmd.Parameters.AddWithValue("filter_id", entityId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }





        public static DataRow GetContextMenuById(DbConnection sqlConnection, int userId, int menuId, bool loadRelatedData = false)
        {
            using (var cmd = DbCommandFactory.Create("qp_get_context_menu_by_id", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_id", menuId);
                cmd.Parameters.AddWithValue("load_related_data", loadRelatedData);

                return ContextMenuDataSetFill(cmd);
            }
        }

        public static DataRow GetContextMenuByCode(DbConnection sqlConnection, int userId, string menuCode, bool loadRelatedData = false)
        {
            using (var cmd = DbCommandFactory.Create("qp_get_context_menu_by_code", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("menu_code", menuCode);
                cmd.Parameters.AddWithValue("load_related_data", loadRelatedData);

                return ContextMenuDataSetFill(cmd);
            }
        }

        private static DataRow ContextMenuDataSetFill(DbCommand cmd)
        {
            var dataAdapter = DataAdapterFactory.Create(cmd);
            var ds = new DataSet();
            dataAdapter.Fill(ds);

            if (ds.Tables.Count > 0)
            {
                if (ds.Tables.Count == 1)
                {
                    var itemsDt = new DataTable();
                    itemsDt.Columns.Add("CONTEXT_MENU_ID", ds.Tables[0].Columns["ID"].DataType);
                    ds.Tables.Add(itemsDt);
                }

                ds.Relations.Add("Menu2Item",
                    ds.Tables[0].Columns["ID"],
                    ds.Tables[1].Columns["CONTEXT_MENU_ID"]
                );

                return ds.Tables[0].AsEnumerable().FirstOrDefault();
            }

            return null;
        }

        public static IEnumerable<DataRow> GetContextMenusList(DbConnection sqlConnection, int userId)
        {
            using (var cmd = DbCommandFactory.Create("qp_get_context_menus_list", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("user_id", userId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static DataTable GetBaseFieldsForM2O(DbConnection sqlConnection, int contentId, int fieldId)
        {
            var sb = new StringBuilder();
            sb.Append("select s.site_id, s.site_name, c.content_id, c.content_name, ca.attribute_name, ca.attribute_id from CONTENT_ATTRIBUTE ca ");
            sb.Append("inner join content c on ca.CONTENT_ID = c.content_id ");
            sb.Append("inner join site s on c.SITE_ID = s.site_id ");
            sb.Append("where related_attribute_id in (select attribute_id from CONTENT_ATTRIBUTE where CONTENT_ID = @content_id) ");
            sb.Append("and not exists(select * from CONTENT_ATTRIBUTE where ATTRIBUTE_ID <> @field_id AND BACK_RELATED_ATTRIBUTE_ID = ca.attribute_id) ");
            sb.Append("and c.virtual_type = 0");

            using (var cmd = DbCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@field_id", fieldId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt;
            }
        }

        public static bool CheckRelationCondition(DbConnection connection, int articleId, int contentId, string relCondition)
        {
            using (var cmd = DbCommandFactory.Create($"select count(content_item_id) as cnt from content_{contentId}_united c with(nolock) where content_item_id = @id and ({relCondition})", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", articleId);
                return (int)cmd.ExecuteScalar() != 0;
            }
        }

        public static string GetCurrentDbVersion(DbConnection connection)
        {
            #warning разобраться с переносом/не переносом в постгрес, пока заглушка
            if (IsPostgresConnection(connection)) return "7.9.9.0";
            using (var cmd = DbCommandFactory.Create("qp_versions", connection))
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

        private const string ApplyFieldDefaultValueGetItemIdsToProcessQuery =
            "select i.CONTENT_ITEM_ID " +
            "from content_item as i " +
            "left outer join CONTENT_DATA d " +
            "ON d.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID and d.ATTRIBUTE_ID = @attr_id " +
            "WHERE " +
            "d.data IS NULL " +
            "and I.CONTENT_ID = @content_id";

        private const string ApplyFieldDefaultValueGetItemIdsToProcessQueryForBlob =
            "select i.CONTENT_ITEM_ID " +
            "from content_item as i " +
            "left outer join CONTENT_DATA d " +
            "ON d.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID and d.ATTRIBUTE_ID = @attr_id " +
            "WHERE " +
            "d.BLOB_DATA IS NULL " +
            "and I.CONTENT_ID = @content_id";


        public static IEnumerable<int> ApplyFieldDefaultValue_GetM2MItemIdsToProcess(int contentId, int fieldId, int linkId, DbConnection connection)
        {
            var query = $@" select i.CONTENT_ITEM_ID from content_item as i WHERE i.CONTENT_ID = @content_id
            and not exists (select * from item_link_united where item_id = i.CONTENT_ITEM_ID and link_id = @link_id ) ";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@attr_id", fieldId);
                cmd.Parameters.AddWithValue("@link_id", linkId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().Select(r => Converter.ToInt32(r.Field<decimal>(0))).ToArray();
            }
        }

        public static IEnumerable<int> ApplyFieldDefaultValue_GetItemIdsToProcess(int contentId, int fieldId, bool isBlob, DbConnection connection)
        {
            var query = isBlob ? ApplyFieldDefaultValueGetItemIdsToProcessQueryForBlob : ApplyFieldDefaultValueGetItemIdsToProcessQuery;
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                cmd.Parameters.AddWithValue("@attr_id", fieldId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => Converter.ToInt32(r.Field<decimal>(0)))
                    .ToArray();
            }
        }

        public static void ApplyFieldDefaultValue_SetDefaultValue(int contentId, int fieldId, bool isBlob, bool isM2M, IEnumerable<int> idsForStep, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            var contentDataColumn = isBlob && dbType == DatabaseType.SqlServer ? "BLOB_DATA" : "DATA";
            var attributeDefValueColumn = "COALESCE(a.DEFAULT_BLOB_VALUE, a.DEFAULT_VALUE)";
            var contentItemsIds = string.Join(",", idsForStep);
            var sql = $@"UPDATE content_data
                SET {contentDataColumn} = (SELECT {attributeDefValueColumn} FROM content_attribute a WHERE attribute_id = @attr_id)
                WHERE {contentDataColumn} IS NULL
                AND attribute_id = @attr_id
                AND CONTENT_ITEM_ID in (select id from {IdList(dbType, "@ids")});
                INSERT INTO content_data (attribute_id, content_item_id, {contentDataColumn})
                select @attr_id, i.content_item_id, {attributeDefValueColumn} from content_item as i
                LEFT OUTER JOIN content_attribute a
                ON a.content_id = i.content_id AND a.attribute_id = @attr_id
                where i.CONTENT_ITEM_ID in (select id from {IdList(dbType, "@ids")}) and
                i.CONTENT_ITEM_ID not in (SELECT d.content_item_id FROM content_data AS d WHERE d.attribute_id = @attr_id)";

            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", fieldId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", idsForStep, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static void ApplyM2MFieldDefaultValue_SetDefaultValue(int contentId, int fieldId, int linkId, List<int> idsForStep, bool symmetric, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            var query = new StringBuilder();
            query.AppendLine($@"INSERT INTO item_to_item (link_id, l_item_id, r_item_id)");
            query.AppendLine($@"select @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab
                cross join CONTENT_ITEM as ci where ci.CONTENT_ITEM_ID in (select id from {IdList(dbType, "@ids")})
                and {IsFalse(dbType, "ci.splitted")} and ab.FIELD_ID = @fieldId;");
            query.AppendLine($@"INSERT INTO item_link_async (link_id,item_id,linked_item_id)");
            query.AppendLine($@"select @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab
                cross join CONTENT_ITEM as ci where ci.CONTENT_ITEM_ID in (select id from {IdList(dbType, "@ids")})
                and {IsTrue(dbType, "ci.splitted")} and ab.FIELD_ID = @fieldId;");
            if (symmetric)
            {
                query.AppendLine($@"INSERT INTO item_link_async (link_id, item_id, linked_item_id)");
                query.AppendLine($@"select  @linkID, ci.CONTENT_ITEM_ID, ARTICLE_ID FROM FIELD_ARTICLE_BIND as ab
                    cross join CONTENT_ITEM as ci
                    inner join CONTENT_ITEM as cii on cii.CONTENT_ITEM_ID = ab.ARTICLE_ID
                    where ci.CONTENT_ITEM_ID in (select id from {IdList(dbType, "@ids")}) and ab.FIELD_ID = @fieldId
                    and {IsFalse(dbType, "ci.splitted")} and {IsTrue(dbType, "cii.splitted")};");
            }

            using (var cmd = DbCommandFactory.Create(query.ToString(), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@linkID", linkId);
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", idsForStep, dbType));
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> RecreateDynamicImages_GetDataToProcess(int imageFieldId, DbConnection connection)
        {
            const string query = "select content_item_id AS ID, DATA from content_data where ATTRIBUTE_ID = @attr_id and DATA is not null";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", imageFieldId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void RecreateDynamicImages_UpdateDynamicFieldValue(int dynamicFieldId, int articleId, string newValue, DbConnection connection)
        {
            var sb = new StringBuilder();
            sb.AppendLine("if exists(select content_data_id from content_data where ATTRIBUTE_ID = @attr_id and CONTENT_ITEM_ID = @item_id)");
            sb.AppendLine(" update content_data set data = @new_data where ATTRIBUTE_ID = @attr_id and CONTENT_ITEM_ID = @item_id");
            sb.AppendLine("else insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA) values(@item_id, @attr_id, @new_data)");
            using (var cmd = DbCommandFactory.Create(sb.ToString(), connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@attr_id", dynamicFieldId);
                cmd.Parameters.AddWithValue("@item_id", articleId);
                cmd.Parameters.AddWithValue("@new_data", newValue);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetSharedUnionBaseContentInfo(int siteId, DbConnection connection)
        {
            const string query =
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
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetSharedRelatedContentInfo(int siteId, DbConnection connection)
        {
            const string query =
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
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int GetSiteArticleCount(int siteId, DbConnection connection)
        {
            const string query =
                "select count(CONTENT_ITEM_ID) from CONTENT_ITEM I " +
                "join CONTENT C ON C.CONTENT_ID = I.CONTENT_ID " +
                "where C.SITE_ID = @site_id";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int GetSiteContentCount(int siteId, DbConnection connection)
        {
            const string query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@site_id", siteId);

                var count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public static int RemovingActions_RemoveSiteArticles(int siteId, int articleToRemove, DbConnection connection)
        {
            var query = @"
                select 1 as A into #disable_td_delete_item_o2m_nullify;
                select 1 as A into #disable_td_item_to_item;

                select top {0} I.content_item_id into #top_items
                from CONTENT_ITEM I
                inner join CONTENT C ON C.CONTENT_ID = I.CONTENT_ID
                where C.SITE_ID = @site_id
                order by I.content_id ASC, I.content_item_id ASC

                DELETE FROM item_to_item where r_item_id in (select CONTENT_ITEM_ID from #top_items)
                DELETE FROM content_item where content_item_id in (select CONTENT_ITEM_ID from #top_items)
            ";

            query = string.Format(query, articleToRemove);

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<int> RemovingActions_BatchRemoveContents(int siteId, int contentsToRemove, DbConnection connection)
        {
            using (var cmd = DbCommandFactory.Create("qp_batch_delete_contents", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                cmd.Parameters.AddWithValue("@count_to_del", contentsToRemove);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().Select(r => Converter.ToInt32(r.Field<decimal>(0))).ToArray();
            }
        }

        public static IEnumerable<int> AssembleAction_GetSiteTemplatesId(int siteId, DbConnection connection)
        {
            var result = new List<int>();
            const string query = "select PAGE_TEMPLATE_ID from PAGE_TEMPLATE where SITE_ID = @site_id order by PAGE_TEMPLATE_ID";
            using (var cmd = DbCommandFactory.Create(query, connection))
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

        public static IEnumerable<DataRow> AssembleAction_GetSitePages(int siteId, DbConnection connection)
        {
            const string query = "select P.PAGE_ID Id, T.TEMPLATE_NAME Template, p.PAGE_NAME Name from PAGE P JOIN PAGE_TEMPLATE T ON P.PAGE_TEMPLATE_ID = T.PAGE_TEMPLATE_ID where T.SITE_ID = @site_id order by P.PAGE_ID";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static int AssembleAction_UpdatePageStatus(int pageId, int userId, DbConnection connection)
        {
            const string query = "update page set reassemble = 0, assembled = getdate(), last_assembled_by = @user_Id where page_id = @page_Id";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@page_Id", pageId);
                cmd.Parameters.AddWithValue("@user_Id", userId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<int> AssembleAction_GetSiteFormatIds(int siteId, DbConnection connection)
        {
            var result = new List<int>();
            const string query = "select N.FORMAT_ID from NOTIFICATIONS N JOIN CONTENT C ON C.CONTENT_ID = N.CONTENT_ID WHERE N.FORMAT_ID is not null and C.SITE_ID = @site_id";
            using (var cmd = DbCommandFactory.Create(query, connection))
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

        public static int CopyUser(int sourceUserId, string newLogin, int currentUserId, DbConnection connection)
        {
            using (var cmd = DbCommandFactory.Create("qp_copy_user", connection))
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
        public static IEnumerable<int> UserGroups_SelectAdminDescendantGroupUserIDs(IEnumerable<int> userIds, int groupId, DbConnection connection)
        {
            var result = new List<int>();
            const string queryTemplate = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
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

            using (var cmd = DbCommandFactory.Create(query, connection))
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
        public static bool UserGroups_IsGroupAdminDescendant(int groupId, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            var levelColumnName = Escape(dbType, "Level");
            var query = $@"with {SqlQuerySyntaxHelper.RecursiveCte(dbType)} G2G (Parent_Group_Id, Child_Group_Id, {levelColumnName}) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(1 as numeric) as Child_Group_Id, 0 as {levelColumnName}
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, {levelColumnName} + 1 as {levelColumnName}
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT CASE WHEN EXISTS(SELECT * FROM G2G where Child_Group_Id = @group_id) THEN 1 ELSE 0 END";

            using (var cmd = DbCommandFactory.Create(query, connection))
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
        public static bool UserGroups_IsCyclePossible(int groupId, int parentGroupId, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            var levelColumnName = Escape(dbType, "Level");
            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);
            var query = $@"with {SqlQuerySyntaxHelper.RecursiveCte(dbType)} G2G (Parent_Group_Id, Child_Group_Id, {levelColumnName}) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(@group_id as numeric) as Child_Group_Id, 0 as {levelColumnName}
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, {levelColumnName} + 1 as {levelColumnName}
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT CASE WHEN EXISTS(select * from G2G where Parent_Group_Id = @parent_group_id or Child_Group_Id = @parent_group_id) THEN {trueValue} ELSE {falseValue} END";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@group_id", groupId);
                cmd.Parameters.AddWithValue("@parent_group_id", parentGroupId);
                return Converter.ToBoolean(cmd.ExecuteScalar());
            }
        }

        public static IEnumerable<int> UserGroups_SelectWorkflowGroupUserIDs(int[] userIds, DbConnection connection)
        {
            var result = new List<int>();
            const string queryTemplate = @"select GB.[USER_ID] from dbo.USER_GROUP_BIND GB
                                    join dbo.USER_GROUP G ON G.GROUP_ID = GB.GROUP_ID
                                    WHERE G.USE_PARALLEL_WORKFLOW = 1 and GB.[USER_ID] in ({0})";

            var query = string.Format(queryTemplate, string.Join(",", userIds));
            using (var cmd = DbCommandFactory.Create(query, connection))
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
        public static IEnumerable<DataRow> UserGroups_GetAdministratorsHierarhy(DbConnection connection)
        {
            const string query = @"with G2G (Parent_Group_Id, Child_Group_Id, [Level]) AS
                            (
                                select CAST(NULL as numeric) AS Parent_Group_Id, CAST(1 as numeric) as Child_Group_Id, 0 as [Level]
                                union all
                                select G.Parent_Group_Id, G.Child_Group_Id, [Level] + 1 as [Level]
                                from group_to_group G
                                join G2G ON G.Parent_Group_Id = G2G.Child_Group_Id
                            )
                            SELECT Parent_Group_Id as PARENT, Child_Group_Id AS CHILD, [LEVEL] FROM G2G";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int CopyUserGroup(int sourceGroupId, string newName, int currentUserId, DbConnection connection)
        {


            using (var cmd = DbCommandFactory.Create("qp_copy_user_group", connection))
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

        public static IEnumerable<DataRow> GetSitePermissionPage(DbConnection sqlConnection, int siteId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.SITE_ACCESS_ID AS ID
                                      ,U.{Escape(dbType, "LOGIN")} AS UserLogin
                                      ,G.GROUP_NAME AS GroupName
                                      ,L.PERMISSION_LEVEL_NAME AS LevelName
                                      ,SA.propagate_to_contents as PropagateToItems
                                      ,{SqlQuerySyntaxHelper.ToBoolSql(dbType, false)} as {Escape(dbType, "Hide")}
                                      ,SA.CREATED
                                      ,SA.MODIFIED
                                      ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                      ,U2.{Escape(dbType, "LOGIN")} AS LastModifiedByUser";

            var fromBlock = $@"SITE_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "SITE_ID = " + siteId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetContentPermissionPage(DbConnection sqlConnection, int contentId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.CONTENT_ACCESS_ID AS ID
                                      ,U.{Escape(dbType, "LOGIN")} AS UserLogin
                                      ,G.GROUP_NAME AS GroupName
                                      ,L.PERMISSION_LEVEL_NAME AS LevelName
                                      ,SA.PROPAGATE_TO_ITEMS as PropagateToItems
                                      ,SA.HIDE as Hide
                                      ,SA.CREATED
                                      ,SA.MODIFIED
                                      ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                      ,U2.{Escape(dbType, "LOGIN")} AS LastModifiedByUser";

            var fromBlock = $@"CONTENT_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "CONTENT_ID = " + contentId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetArticlePermissionPage(DbConnection sqlConnection, int articleId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.CONTENT_ITEM_ACCESS_ID AS ID
                                      ,U.LOGIN AS UserLogin
                                      ,G.GROUP_NAME AS GroupName
                                      ,L.PERMISSION_LEVEL_NAME AS LevelName
                                      ,cast(0 as numeric(18, 0)) as PropagateToItems
                                      ,{SqlQuerySyntaxHelper.CastToBool(dbType, SqlQuerySyntaxHelper.ToBoolSql(dbType, false))} as Hide
                                      ,SA.CREATED
                                      ,SA.MODIFIED
                                      ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                      ,U2.LOGIN AS LastModifiedByUser";

            var fromBlock = $@"CONTENT_ITEM_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "CONTENT_ITEM_ID = " + articleId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetWorkflowPermissionPage(DbConnection sqlConnection, int workflowId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.WORKFLOW_ACCESS_ACCESS_ID AS ID
                                      ,U.LOGIN AS UserLogin
                                      ,G.GROUP_NAME AS GroupName
                                      ,L.PERMISSION_LEVEL_NAME AS LevelName
                                      ,cast(0 as numeric(18, 0)) as PropagateToItems
                                      ,{SqlQuerySyntaxHelper.CastToBool(dbType, SqlQuerySyntaxHelper.ToBoolSql(dbType, false))} as Hide
                                      ,SA.CREATED
                                      ,SA.MODIFIED
                                      ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                      ,U2.LOGIN AS LastModifiedByUser";

            var fromBlock = $@"WORKFLOW_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "WORKFLOW_ID = " + workflowId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetSiteFolderPermissionPage(DbConnection sqlConnection, int folderId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);

            var selectBlock = $@"SA.FOLDER_ACCESS_ID AS ID
                                      ,U.LOGIN AS UserLogin
                                      ,G.GROUP_NAME AS GroupName
                                      ,L.PERMISSION_LEVEL_NAME AS LevelName
                                      ,cast(0 as numeric(18, 0)) as PropagateToItems
                                      ,{SqlQuerySyntaxHelper.CastToBool(dbType, SqlQuerySyntaxHelper.ToBoolSql(dbType, false))} as Hide
                                      ,SA.CREATED
                                      ,SA.MODIFIED
                                      ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                      ,U2.LOGIN AS LastModifiedByUser";

            const string fromBlock = @"FOLDER_ACCESS SA
                                    LEFT JOIN USERS U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN USERS U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "FOLDER_ID = " + folderId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetEntityTypePermissionPage(DbConnection sqlConnection, int entityTypeId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.ENTITY_TYPE_ACCESS_ID AS ID
                                    ,U.LOGIN AS UserLogin
                                    ,G.GROUP_NAME AS GroupName
                                    ,L.PERMISSION_LEVEL_NAME AS LevelName
                                    ,cast(0 as numeric(18, 0)) as PropagateToItems
                                    ,{SqlQuerySyntaxHelper.CastToBool(dbType, SqlQuerySyntaxHelper.ToBoolSql(dbType, false))} as Hide
                                    ,SA.CREATED
                                    ,SA.MODIFIED
                                    ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                    ,U2.LOGIN AS LastModifiedByUser";

            var fromBlock = $@"ENTITY_TYPE_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "ENTITY_TYPE_ID = " + entityTypeId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetActionPermissionPage(DbConnection sqlConnection, int actionId, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = $@"SA.ACTION_ACCESS_ID AS ID
                                    ,U.LOGIN AS UserLogin
                                    ,G.GROUP_NAME AS GroupName
                                    ,L.PERMISSION_LEVEL_NAME AS LevelName
                                    ,cast(0 as numeric(18, 0)) as PropagateToItems
                                    ,{SqlQuerySyntaxHelper.CastToBool(dbType, SqlQuerySyntaxHelper.ToBoolSql(dbType, false))} as Hide
                                    ,SA.CREATED
                                    ,SA.MODIFIED
                                    ,SA.LAST_MODIFIED_BY AS LastModifiedByUserId
                                    ,U2.LOGIN AS LastModifiedByUser";

            var fromBlock = $@"ACTION_ACCESS SA
                                    LEFT JOIN {Escape(dbType, "USERS")} U ON U.USER_ID = SA.USER_ID
                                    LEFT JOIN USER_GROUP G ON G.GROUP_ID = SA.GROUP_ID
                                    JOIN PERMISSION_LEVEL L ON L.PERMISSION_LEVEL_ID = SA.PERMISSION_LEVEL_ID
                                    JOIN {Escape(dbType, "USERS")} U2 ON U2.USER_ID = SA.LAST_MODIFIED_BY";

            var localFilter = (!string.IsNullOrWhiteSpace(filter) ? filter + " AND " : string.Empty) + "ACTION_ID = " + actionId;
            return GetSimplePagedList(sqlConnection, EntityTypeCode.SitePermission, selectBlock, fromBlock, orderBy, localFilter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetUserGroupPage(DbConnection sqlConnection, List<int> selectedIds, string orderBy, string filter, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = @"G.GROUP_ID AS Id,G.GROUP_NAME AS Name,G.DESCRIPTION AS Description,G.CREATED,G.MODIFIED,G.LAST_MODIFIED_BY AS LastModifiedByUserId,G.LAST_MODIFIED_BY_LOGIN AS LastModifiedByUser,G.shared_content_items AS SharedArticles";
            var fromBlock = @"USER_GROUP_TREE G";

            if (selectedIds != null && selectedIds.Any())
            {
                selectBlock += $" , {SqlQuerySyntaxHelper.CastToBool(dbType, "CASE WHEN (GIS.GROUP_ID IS NOT NULL) THEN 1 else 0 end")} AS IS_SELECTED";
                fromBlock += $" LEFT OUTER JOIN (select GROUP_ID from USER_GROUP_TREE where GROUP_ID in ({string.Join(",", selectedIds)})) AS GIS ON GIS.GROUP_ID = G.GROUP_ID";
                orderBy = string.IsNullOrWhiteSpace(orderBy) ? "IS_SELECTED DESC, Name ASC" : orderBy;
            }
            else
            {
                orderBy = string.IsNullOrWhiteSpace(orderBy) ? "Name ASC" : orderBy;
            }

            return GetSimplePagedList(sqlConnection,
                EntityTypeCode.UserGroup, selectBlock, fromBlock,
                orderBy, filter, startRow, pageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetUserPage(DbConnection sqlConnection, UserPageOptions options, out int totalRecords)
        {
            string strFilter = null;
            string orderBy;
            var filters = new List<string>(4);
            var dbType = GetDbType(sqlConnection);
            var ns = DbSchemaName(dbType);
            if (!string.IsNullOrEmpty(options.Login))
            {
                filters.Add($"U.LOGIN LIKE '%{options.Login}%'");
            }

            if (!string.IsNullOrEmpty(options.Email))
            {
                filters.Add($"U.EMAIL LIKE '%{options.Email}%'");
            }

            if (!string.IsNullOrEmpty(options.FirstName))
            {
                filters.Add($"U.FIRST_NAME LIKE '%{options.FirstName}%'");
            }

            if (!string.IsNullOrEmpty(options.LastName))
            {
                filters.Add($"U.LAST_NAME LIKE '%{options.LastName}%'");
            }

            if (filters.Any())
            {
                strFilter = string.Join(" AND ", filters);
            }

            var selectBlock = @"U.USER_ID as ID, U.LOGIN, U.FIRST_NAME, U.LAST_NAME, U.EMAIL, U.LANGUAGE_ID, L.LANGUAGE_NAME, U.LAST_LOGIN, U.DISABLED, U.CREATED, U.MODIFIED, U.LAST_MODIFIED_BY, MU.LOGIN as LAST_MODIFIED_BY_LOGIN";
            var fromBlock = $@"{ns}.USERS U JOIN {ns}.USERS MU ON MU.USER_ID = U.LAST_MODIFIED_BY JOIN {ns}.LANGUAGES L ON L.LANGUAGE_ID =  U.LANGUAGE_ID";

            if (options.SelectedIDs != null && options.SelectedIDs.Any())
            {
                selectBlock += $" ,{SqlQuerySyntaxHelper.CastToBool(dbType, "CASE WHEN (UIS.USER_ID IS NOT NULL) THEN 1 else 0 end")} AS is_selected";
                fromBlock += $" LEFT OUTER JOIN (select USER_ID from USERS where USER_ID in ({string.Join(",", options.SelectedIDs)})) AS UIS ON UIS.USER_ID = U.USER_ID";
                orderBy = string.IsNullOrWhiteSpace(options.SortExpression) ? "is_selected DESC, LOGIN ASC" : options.SortExpression;
            }
            else
            {
                orderBy = string.IsNullOrWhiteSpace(options.SortExpression) ? "LOGIN ASC" : options.SortExpression;
            }

            return GetSimplePagedList(sqlConnection, EntityTypeCode.User, selectBlock, fromBlock, orderBy, strFilter, options.StartRecord, options.PageSize, out totalRecords);
        }

        public static IEnumerable<DataRow> GetChildContentPermissionsForUser(DbConnection sqlConnection, int siteId, int userId, string orderBy, int startRow, int pageSize, out int totalRecords, int? contentId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var fromBlock = $@"(select C.CONTENT_ID AS ID, C.CONTENT_NAME AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, "case when P2.USER_ID IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, "coalesce(P2.PROPAGATE_TO_ITEMS, 0)")} AS PropagateToItems,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, $"coalesce(P2.HIDE, {SqlQuerySyntaxHelper.ToBoolSql(dbType, false)})")} AS Hide,
                                L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                 LEFT JOIN content_access_permlevel_site P2 ON P1.CONTENT_ID = P2.CONTENT_ID and P1.permission_level = p2.permission_level and P2.USER_ID = {{0}}
                                 RIGHT JOIN CONTENT C ON P1.CONTENT_ID = C.CONTENT_ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE C.SITE_ID = {{1}}) AS TR";

            fromBlock = string.Format(fromBlock, userId, siteId);
            string filter = null;
            if (contentId.HasValue)
            {
                filter = "ID = " + contentId.Value;
            }

            return GetSimplePagedList(sqlConnection, EntityTypeCode.Content, "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords, useSecurity: true, userId: userId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Site, parentEntityId: siteId);
        }

        public static DataRow GetChildContentPermissionForUser(DbConnection sqlConnection, int siteId, int contentId, int userId)
        {
            int totalRecords;
            var rows = GetChildContentPermissionsForUser(sqlConnection, siteId, userId, "ID asc", 1, 1, out totalRecords, contentId);
            return rows.FirstOrDefault();
        }

        public static IEnumerable<DataRow> GetChildContentPermissionsForGroup(DbConnection sqlConnection, int siteId, int groupId, string orderBy, int startRow, int pageSize, out int totalRecords, int? contentId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);
            var fromBlock = $@"(select C.CONTENT_ID AS ID, C.CONTENT_NAME AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, "case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, $"coalesce(P2.PROPAGATE_TO_ITEMS, 0)")} AS PropagateToItems,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, $"coalesce(P2.HIDE, {falseValue})")} AS Hide,
                                L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                 LEFT JOIN content_access_permlevel_site P2 ON P1.CONTENT_ID = P2.CONTENT_ID and P1.permission_level = p2.permission_level and P2.GROUP_ID = {{0}}
                                 RIGHT JOIN CONTENT C ON P1.CONTENT_ID = C.CONTENT_ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE C.SITE_ID = {{1}}) AS TR";

            fromBlock = string.Format(fromBlock, groupId, siteId);

            string filter = null;
            if (contentId.HasValue)
            {
                filter = "ID = " + contentId.Value;
            }

            return GetSimplePagedList(sqlConnection, EntityTypeCode.Content, "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords, useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Site, parentEntityId: siteId);
        }

        public static DataRow GetChildContentPermissionForGroup(DbConnection sqlConnection, int siteId, int contentId, int groupId)
        {
            int totalRecords;
            var rows = GetChildContentPermissionsForGroup(sqlConnection, siteId, groupId, "ID asc", 1, 1, out totalRecords, contentId);
            return rows.FirstOrDefault();
        }

        public static IEnumerable<DataRow> GetChildArticlePermissionsForUser(DbConnection sqlConnection, int contentId, int userId, string titleFieldName, string orderBy, int startRow, int pageSize, out int totalRecords, int? articleId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);
            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);

            var fromBlock = $@"(select CI.CONTENT_ITEM_ID AS ID, {SqlQuerySyntaxHelper.CastToString(dbType, $"CI.{Escape(dbType, titleFieldName)}")} AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, $"case when P2.{Escape(dbType, "USER_ID")} IS NOT NULL THEN {trueValue} ELSE {falseValue} END")}  AS IsExplicit
                                ,{SqlQuerySyntaxHelper.CastToBool(dbType, falseValue)} AS PropagateToItems
                                ,{SqlQuerySyntaxHelper.CastToBool(dbType, falseValue)} AS Hide
                                ,L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                LEFT JOIN content_item_access_permlevel_content P2 ON P1.CONTENT_ITEM_ID = P2.CONTENT_ITEM_ID and P1.PERMISSION_LEVEL = p2.PERMISSION_LEVEL and P2.{Escape(dbType, "USER_ID")} = {userId}
                                LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                RIGHT JOIN CONTENT_{contentId} CI {WithNoLock(dbType)} ON CI.CONTENT_ITEM_ID = P1.CONTENT_ITEM_ID) AS TR";

            string filter = null;
            if (articleId.HasValue)
            {
                filter = "ID = " + articleId.Value;
            }

            return GetSimplePagedList(sqlConnection, "content_item", "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords, useSecurity: true, userId: userId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Content, parentEntityId: contentId);
        }

        public static DataRow GetChildArticlePermissionForUser(DbConnection sqlConnection, int contentId, int articleId, int userId)
        {
            int totalRecords;
            var rows = GetChildArticlePermissionsForUser(sqlConnection, contentId, userId, FieldName.ContentItemId, "ID asc", 1, 1, out totalRecords, articleId);
            return rows.FirstOrDefault();
        }

        public static IEnumerable<DataRow> GetChildArticlePermissionsForGroup(DbConnection sqlConnection, int contentId, int groupId, string titleFieldName, string orderBy, int startRow, int pageSize, out int totalRecords, int? articleId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var trueValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, true);
            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);
            var fromBlock = $@"(select CI.CONTENT_ITEM_ID AS ID, {SqlQuerySyntaxHelper.CastToString(dbType, $"CI.{Escape(dbType, titleFieldName)}")} AS TITLE, L.PERMISSION_LEVEL_NAME as LevelName,
                                {SqlQuerySyntaxHelper.CastToBool(dbType, $"case when P2.GROUP_ID IS NOT NULL THEN {trueValue} ELSE {falseValue} END")}  AS IsExplicit
                                ,{SqlQuerySyntaxHelper.CastToBool(dbType, falseValue)} AS PropagateToItems
                                ,{SqlQuerySyntaxHelper.CastToBool(dbType, falseValue)} AS Hide
                                ,L.PERMISSION_LEVEL_ID as LevelId
                                from
                                (<$_security_insert_$>) P1
                                LEFT JOIN content_item_access_permlevel_content P2 ON P1.CONTENT_ITEM_ID = P2.CONTENT_ITEM_ID and P1.PERMISSION_LEVEL = p2.PERMISSION_LEVEL and P2.GROUP_ID = {groupId}
                                LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                RIGHT JOIN CONTENT_{contentId} CI {WithNoLock(dbType)} ON CI.CONTENT_ITEM_ID = P1.CONTENT_ITEM_ID) AS TR";



            string filter = null;
            if (articleId.HasValue)
            {
                filter = "ID = " + articleId.Value;
            }

            return GetSimplePagedList(sqlConnection, "content_item", "*", fromBlock, orderBy, filter, startRow, pageSize, out totalRecords, useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100, parentEntityTypeCode: EntityTypeCode.Content, parentEntityId: contentId);
        }

        public static DataRow GetChildArticlePermissionForGroup(DbConnection sqlConnection, int contentId, int articleId, int groupId) =>
            GetChildArticlePermissionsForGroup(sqlConnection, contentId, groupId, FieldName.ContentItemId, "ID asc", 1, 1, out int _, articleId).FirstOrDefault();

        public static void InsertChildContentPermissions(DbConnection sqlConnection, List<int> contentIds, int? userId, int? groupId, int permissionLevel, bool propagateToItems, int currentUserId, bool hide)
        {

            var dbType = GetDbType(sqlConnection);
            if (contentIds == null || !contentIds.Any() || !userId.HasValue && !groupId.HasValue)
            {
                return;
            }

            var query = $@"INSERT INTO CONTENT_ACCESS
                                       (CONTENT_ID
                                       ,{Escape(dbType, "USER_ID")}
                                       ,GROUP_ID
                                       ,PERMISSION_LEVEL_ID
                                       ,PROPAGATE_TO_ITEMS
                                       ,HIDE
                                       ,CREATED
                                       ,MODIFIED
                                       ,LAST_MODIFIED_BY)
                            select C.CONTENT_ID, @userId, @groupId, @permissionLevel, @propageteToItems, @hide, {Now(dbType)}, {Now(dbType)}, @modifiedUserId
                            from CONTENT C where C.CONTENT_ID in ({string.Join(",", contentIds)})";


            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId, DbType.Int32);
                cmd.Parameters.AddWithValue("@groupId", groupId, DbType.Int32);



                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@propageteToItems", SqlQuerySyntaxHelper.BooleanToNumeric(dbType, propagateToItems));
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);
                cmd.Parameters.AddWithValue("@hide", hide);

                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildContentPermissions(DbConnection sqlConnection, int siteId, int? userId, int? groupId, int permissionLevel, bool propagateToItems, int currentUserId, bool hide)
        {
            const string query = @"INSERT INTO [CONTENT_ACCESS]
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                if (userId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);
                }

                if (groupId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);
                }

                cmd.Parameters.AddWithValue("@siteId", siteId);
                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@propageteToItems", propagateToItems);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);
                cmd.Parameters.AddWithValue("@hide", hide);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildContentPermissions(DbConnection sqlConnection, List<int> contentIds, int? userId, int? groupId)
        {
            if (contentIds == null || !contentIds.Any() || !userId.HasValue && !groupId.HasValue)
            {
                return;
            }

            var dbType = GetDbType(sqlConnection);

            var query = $"delete from CONTENT_ACCESS where CONTENT_ID in ({string.Join(",", contentIds)}) and (@userId IS NULL OR {Escape(dbType, "USER_ID")} = @userId) and (@groupId IS NULL OR GROUP_ID = @groupId)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@userId", userId, DbType.Int32);
                cmd.Parameters.AddWithValue("@groupId", groupId, DbType.Int32);


                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildContentPermissions(DbConnection sqlConnection, int siteId, int? userId, int? groupId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"delete CONTENT_ACCESS from CONTENT_ACCESS A
                            JOIN CONTENT C ON A.CONTENT_ID = C.CONTENT_ID
                            where C.SITE_ID = @siteId
                            and (@userId IS NULL OR A.{Escape(dbType, "USER_ID")} = @userId) and (@groupId IS NULL OR A.GROUP_ID = @groupId)";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@groupId", groupId);

                cmd.Parameters.AddWithValue("@siteId", siteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildArticlePermissions(DbConnection sqlConnection, IEnumerable<int> articleIDs, int? userId, int? groupId, int permissionLevel, int currentUserId)
        {
            var dbType = GetDbType(sqlConnection);
            var now = SqlQuerySyntaxHelper.Now(dbType);
            var query = $@"INSERT INTO CONTENT_ITEM_ACCESS
                            (CONTENT_ITEM_ID
                            ,USER_ID
                            ,GROUP_ID
                            ,PERMISSION_LEVEL_ID
                            ,CREATED
                            ,MODIFIED
                            ,LAST_MODIFIED_BY)
                            select CI.CONTENT_ITEM_ID, @userId, @groupId, @permissionLevel, {now} , {now}, @modifiedUserId
                            from CONTENT_ITEM CI where CI.CONTENT_ITEM_ID in ({string.Join(",", articleIDs)})";


            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId, DbType.Int32);
                cmd.Parameters.AddWithValue("@groupId", groupId, DbType.Int32);
                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertChildArticlePermissions(DbConnection sqlConnection, int contentId, int? userId, int? groupId, int permissionLevel, int currentUserId)
        {
            const string query = @"INSERT INTO [CONTENT_ITEM_ACCESS]
                            ([CONTENT_ITEM_ID]
                            ,[USER_ID]
                            ,[GROUP_ID]
                            ,[PERMISSION_LEVEL_ID]
                            ,[CREATED]
                            ,[MODIFIED]
                            ,[LAST_MODIFIED_BY])
                            select CI.CONTENT_ITEM_ID, @userId, @groupId, @permissionLevel, GETDATE(), GETDATE(), @modifiedUserId
                            from CONTENT_ITEM CI where CI.CONTENT_ID = @contentId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                if (userId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@userId", DBNull.Value);
                }

                if (groupId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@groupId", DBNull.Value);
                }

                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.AddWithValue("@permissionLevel", permissionLevel);
                cmd.Parameters.AddWithValue("@modifiedUserId", currentUserId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildArticlePermissions(DbConnection sqlConnection, IEnumerable<int> articleIDs, int? userId, int? groupId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"delete from CONTENT_ITEM_ACCESS where CONTENT_ITEM_ID in ({string.Join(",", articleIDs)}) and (@userId IS NULL OR {Escape(dbType, "USER_ID")} = @userId) and (@groupId IS NULL OR GROUP_ID = @groupId)";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId, DbType.Int32);
                cmd.Parameters.AddWithValue("@groupId", groupId, DbType.Int32);

                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveChildArticlePermissions(DbConnection sqlConnection, int contentId, int? userId, int? groupId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"delete CONTENT_ITEM_ACCESS from CONTENT_ITEM_ACCESS A
                            JOIN CONTENT_ITEM CI ON A.CONTENT_ITEM_ID = CI.CONTENT_ITEM_ID
                            where CI.CONTENT_ID = @contentId
                            and (@userId IS NULL OR A.{Escape(dbType, "USER_ID")} = @userId) and (@groupId IS NULL OR A.GROUP_ID = @groupId)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId, DbType.Int32);
                cmd.Parameters.AddWithValue("@groupId", groupId, DbType.Int32);
                cmd.Parameters.AddWithValue("@contentId", contentId, DbType.Int32);

                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsSiteDotNeByObjectFormatId(DbConnection sqlConnection, int objectFormatId)
        {
            const string query = @"select s.script_language from OBJECT_FORMAT obf inner join [OBJECT] o on obf.[OBJECT_ID] = o.[OBJECT_ID]
                            inner join PAGE_TEMPLATE pt on o.PAGE_TEMPLATE_ID = pt.PAGE_TEMPLATE_ID inner join [SITE] s on pt.SITE_ID =  s.SITE_ID
                            where obf.OBJECT_FORMAT_ID = @in_object_format_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@in_object_format_id", objectFormatId);
                return string.Equals(Converter.ToString(cmd.ExecuteScalar()), ".net final");
            }
        }

        public static IEnumerable<DataRow> GetEntityTypePermissionsForGroup(DbConnection sqlConnection, int groupId, int? entityId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var fromBlock = $@"(select T.ID, T.NAME, L.PERMISSION_LEVEL_NAME, {SqlQuerySyntaxHelper.CastToBool(dbType, "case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit from
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.GROUP_ID = {{0}}
                                 RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ACTION_PERMISSION_ENABLE = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)} {{1}}) AS TR";

            fromBlock = string.Format(fromBlock, groupId, entityId.HasValue ? $"AND T.ID = {entityId.Value}" : string.Empty);

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "ENTITY_TYPE", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords, useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetEntityTypePermissionsForUser(DbConnection sqlConnection, int userId, int? entityId = null)
        {
            var dbType = GetDbType(sqlConnection);
            var fromBlock = $@"(select T.ID, T.NAME, L.PERMISSION_LEVEL_NAME, {SqlQuerySyntaxHelper.CastToBool(dbType, $"case when P2.{Escape(dbType, "USER_ID")} IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit, L.PERMISSION_LEVEL
                                 FROM
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN ENTITY_TYPE_ACCESS_PERMLEVEL P2 ON P1.entity_type_id = P2.entity_type_id and P1.permission_level = p2.permission_level and P2.{Escape(dbType, "USER_ID")} = {{0}}
                                 RIGHT JOIN ENTITY_TYPE T ON P1.ENTITY_TYPE_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ACTION_PERMISSION_ENABLE = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)} {{1}}) AS TR";

            fromBlock = string.Format(fromBlock, userId, entityId.HasValue ? $"AND T.ID = {entityId.Value}" : string.Empty);

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "ENTITY_TYPE", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords, useSecurity: true, userId: userId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetActionPermissionsForGroup(QPModelDataContext efContext, DbConnection sqlConnection, int groupId, int entityTypeId, int? actionId)
        {
            var dbType = GetDbType(sqlConnection);
            var refreshActionTypeId = efContext.ActionTypeSet.First(x => x.Code.Equals(ActionTypeCode.Refresh)).Id;
            var fromBlock = $@"(select
                                    T.ID,
                                    T.NAME,
                                    COALESCE(L.PERMISSION_LEVEL_NAME, {{2}}) AS PERMISSION_LEVEL_NAME,
                                   {SqlQuerySyntaxHelper.CastToBool(dbType, "case when P2.GROUP_ID IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit
                                from
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN backend_action_access_permlevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.GROUP_ID = {{0}}
                                 RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ENTITY_TYPE_ID = {{1}} {{3}} AND T.{Escape(dbType, "TYPE_ID")} != {refreshActionTypeId}) AS TR";

            var entityPermissionLevelName = "NULL";
            var entityPermission = GetEntityTypePermissionsForGroup(sqlConnection, groupId, entityTypeId).FirstOrDefault();
            if (entityPermission != null)
            {
                entityPermissionLevelName = $"N'{entityPermission.Field<string>("PERMISSION_LEVEL_NAME")}'";
            }

            fromBlock = string.Format(fromBlock, groupId, entityTypeId, entityPermissionLevelName, actionId.HasValue ? $"AND T.ID = {actionId.Value}" : string.Empty);

            int totalRecords;
            return GetSimplePagedList(sqlConnection, "backend_action", "*", fromBlock, "ID ASC", null, 0, 0, out totalRecords, useSecurity: true, groupId: groupId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetActionPermissionsForUser(QPModelDataContext efContext, DbConnection sqlConnection, int userId, int entityTypeId, int? actionId)
        {
            var dbType = GetDbType(sqlConnection);
            var refreshActionTypeId = efContext.ActionTypeSet.First(x => x.Code.Equals(ActionTypeCode.Refresh)).Id;
            var fromBlock = $@"(
select
T.ID,
T.NAME,
COALESCE(L.PERMISSION_LEVEL_NAME, {{2}}) AS PERMISSION_LEVEL_NAME,
{SqlQuerySyntaxHelper.CastToBool(dbType, $"case when P2.{Escape(dbType, "USER_ID")} IS NOT NULL THEN 1 ELSE 0 END")} AS IsExplicit,
COALESCE(L.PERMISSION_LEVEL, {{4}}) AS PERMISSION_LEVEL
                                 FROM
                                 (<$_security_insert_$>) P1
                                 LEFT JOIN backend_action_access_permlevel P2 ON P1.BACKEND_ACTION_ID = P2.BACKEND_ACTION_ID and P1.permission_level = p2.permission_level and P2.{Escape(dbType, "USER_ID")} = {{0}}
                                 RIGHT JOIN BACKEND_ACTION T ON P1.BACKEND_ACTION_ID = T.ID
                                 LEFT join PERMISSION_LEVEL L ON P1.PERMISSION_LEVEL = L.PERMISSION_LEVEL
                                 WHERE T.ENTITY_TYPE_ID = {{1}} {{3}} AND T.{Escape(dbType, "TYPE_ID")} != {refreshActionTypeId}) AS TR";

            var entityPermissionLevelName = "NULL";
            var entityPermissionLevel = "NULL";
            var entityPermission = GetEntityTypePermissionsForUser(sqlConnection, userId, entityTypeId).FirstOrDefault();
            if (entityPermission != null)
            {
                entityPermissionLevelName = $"N'{entityPermission.Field<string>("PERMISSION_LEVEL_NAME")}'";
                if (!entityPermission.IsNull("PERMISSION_LEVEL"))
                {
                    entityPermissionLevel = entityPermission.Field<decimal>("PERMISSION_LEVEL").ToString(CultureInfo.InvariantCulture);
                }
            }

            fromBlock = string.Format(fromBlock, userId, entityTypeId, entityPermissionLevelName, actionId.HasValue ? $"AND T.ID = {actionId.Value}" : string.Empty, entityPermissionLevel);
            return GetSimplePagedList(sqlConnection, "backend_action", "*", fromBlock, "ID ASC", null, 0, 0, out int _, useSecurity: true, userId: userId, startLevel: 0, endLevel: 100);
        }

        public static IEnumerable<DataRow> GetVisualEditorCommandsBySiteId(DbConnection sqlConnection, int siteId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"
SELECT cmd.{Escape(dbType, "ID")},
cmd.{Escape(dbType, "NAME")},
cmd.{Escape(dbType, "ALIAS")},
cmd.{Escape(dbType, "ROW_ORDER")},
cmd.{Escape(dbType, "TOOLBAR_IN_ROW_ORDER")},
cmd.{Escape(dbType, "GROUP_IN_TOOLBAR_ORDER")},
cmd.{Escape(dbType, "COMMAND_IN_GROUP_ORDER")},
bnd.{Escape(dbType, "ON")},
cmd.{Escape(dbType, "PLUGIN_ID")},
cmd.{Escape(dbType, "CREATED")},
cmd.{Escape(dbType, "MODIFIED")},
cmd.{Escape(dbType, "LAST_MODIFIED_BY")}
from {DbSchemaName(dbType)}.VE_COMMAND_SITE_BIND bnd INNER JOIN {DbSchemaName(dbType)}.VE_COMMAND cmd ON bnd.COMMAND_ID = cmd.ID where bnd.SITE_ID = {siteId} ORDER BY cmd.{Escape(dbType, "ID")}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorCommandsByFieldId(DbConnection sqlConnection, int fieldId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"
SELECT cmd.{Escape(dbType, "ID")},
cmd.{Escape(dbType, "NAME")},
cmd.{Escape(dbType, "ALIAS")},
cmd.{Escape(dbType, "ROW_ORDER")},
cmd.{Escape(dbType, "TOOLBAR_IN_ROW_ORDER")},
cmd.{Escape(dbType, "GROUP_IN_TOOLBAR_ORDER")},
cmd.{Escape(dbType, "COMMAND_IN_GROUP_ORDER")},
bnd.{Escape(dbType, "ON")},
cmd.{Escape(dbType, "PLUGIN_ID")},
cmd.{Escape(dbType, "CREATED")},
cmd.{Escape(dbType, "MODIFIED")},
cmd.{Escape(dbType, "LAST_MODIFIED_BY")}
from {DbSchemaName(dbType)}.VE_COMMAND_FIELD_BIND bnd INNER JOIN {DbSchemaName(dbType)}.VE_COMMAND cmd ON bnd.COMMAND_ID = cmd.ID where bnd.FIELD_ID = {fieldId} ORDER BY cmd.{Escape(dbType, "ID")}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        /// <summary>
        /// Вкл/Выкл команду в.редактора
        /// </summary>
        public static void UpdateOrInsertSiteVeCommandValue(DbConnection sqlConnection, int siteId, int commandId, bool value)
        {
            var dbType = GetDbType(sqlConnection);
            string query;
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    query = @"begin tran
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
                    break;
                case DatabaseType.Postgres:
                    query = $@"
                        INSERT INTO VE_COMMAND_SITE_BIND
                        (command_id, site_id, {Escape(dbType, "on")})
                        VALUES(@cId, @sId, @val)
                        ON CONFLICT(command_id, site_id)
                        DO UPDATE SET {Escape(dbType, "on")} = @val
                        ";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateOrInsertSiteVeStyleValue(DbConnection sqlConnection, int siteId, int styleId, bool value)
        {
            var dbType = GetDbType(sqlConnection);
            string query;
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    query = @"begin tran
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
                    break;
                case DatabaseType.Postgres:
                    query = $@"
    INSERT INTO VE_STYLE_SITE_BIND (style_id, site_id, {Escape(dbType, "on")})
    VALUES( @cId, @sId, @val )
    ON CONFLICT(style_id, site_id)
    DO UPDATE SET {Escape(dbType, "on")} = @val;
";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", styleId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveSiteVeCommand(DbConnection sqlConnection, int siteId, int commandId)
        {

            using (var cmd = DbCommandFactory.Create("DELETE FROM VE_COMMAND_SITE_BIND where COMMAND_ID = @cId and SITE_ID = @sId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveSiteVeStyle(DbConnection sqlConnection, int siteId, int styleId)
        {

            using (var cmd = DbCommandFactory.Create($"DELETE FROM VE_STYLE_SITE_BIND where STYLE_ID = @cId and SITE_ID = @sId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", styleId);
                cmd.Parameters.AddWithValue("@sId", siteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveFieldVeCommand(DbConnection sqlConnection, int fieldId, int commandId)
        {
            using (var cmd = DbCommandFactory.Create("DELETE FROM VE_COMMAND_FIELD_BIND where COMMAND_ID = @cId and FIELD_ID = @fId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveFieldVeStyle(DbConnection sqlConnection, int fieldId, int styleId)
        {
            using (var cmd = DbCommandFactory.Create("DELETE FROM VE_STYLE_FIELD_BIND where STYLE_ID = @sId and FIELD_ID = @fId", sqlConnection))
            {
                cmd.Parameters.AddWithValue("@sId", styleId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateOrInsertFieldVeCommandValue(DbConnection sqlConnection, int fieldId, int commandId, bool value)
        {
            var dbType = GetDbType(sqlConnection);
            string query;
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    query = @"begin tran
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
                    break;
                case DatabaseType.Postgres:
                    query = $@"
 INSERT INTO VE_COMMAND_FIELD_BIND (command_id, field_id, {Escape(dbType, "on")})
    VALUES( @cId, @fId, @val )
    ON CONFLICT(command_id, field_id)
    DO UPDATE SET {Escape(dbType, "on")} = @val;
";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@cId", commandId);
                cmd.Parameters.AddWithValue("@fId", fieldId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsVeCommandNameFree(DbConnection sqlConnection, string name, int pluginId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"select COUNT(*) FROM VE_COMMAND WHERE {Escape(dbType, "NAME")} = @name AND PLUGIN_ID <> @pluginId";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pluginId", pluginId);
                return Converter.ToInt32(cmd.ExecuteScalar()) == 0;
            }
        }

        public static bool IsVeCommandAliasFree(DbConnection sqlConnection, string alias, int pluginId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"select COUNT(*) FROM VE_COMMAND WHERE {Escape(dbType, "ALIAS")} = @alias AND {Escape(dbType, "PLUGIN_ID")} <> @pluginId";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@alias", alias);
                cmd.Parameters.AddWithValue("@pluginId", pluginId);
                return Converter.ToInt32(cmd.ExecuteScalar()) == 0;
            }
        }

        public static void UnlockArticlesLockedByUser(DbConnection sqlConnection, int userId, int[] ids)
        {
            var query = $"update CONTENT_ITEM set locked_by = null, locked = null where locked_by = @user_id and content_item_id in ({string.Join(",", ids)})";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UnlockAllArticlesLockedByUser(DbConnection sqlConnection, int userId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $"update CONTENT_ITEM set locked_by = null, locked = null where permanent_lock = {GetBoolValue(false, dbType)} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }



        public static void UnlockAllSitesLockedByUser(DbConnection sqlConnection, int userId)
        {
            var databaseType = GetDbType(sqlConnection);
            var query = $"update {EscapeObjectName("SITE", databaseType)} set locked_by = null, locked = null where permanent_lock = {GetBoolValue(false, databaseType)} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetBoolValue(bool value, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return value ? "1" : "0";
                case DatabaseType.Postgres:
                    return value ? "TRUE" : "FALSE";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }

        public static int GetVeCommandsMaxRowOrder(DbConnection sqlConnection)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"select MAX({Escape(dbType, "ROW_ORDER")}) FROM VE_COMMAND";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var maxOrder = cmd.ExecuteScalar();
                return DBNull.Value.Equals(maxOrder) ? 0 : Convert.ToInt32(maxOrder);
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesPage(DbConnection sqlConnection, int contentId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);
            var escapedOrderColumnName = Escape(dbType, "ORDER");
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.VisualEditorStyle,
                $"s.ID as Id, s.NAME as Name, s.DESCRIPTION as Description, s.TAG as Tag, s.{escapedOrderColumnName} as {escapedOrderColumnName}," +
                " s.IS_SYSTEM as IsSystem, s.IS_FORMAT as IsFormat, s.CREATED as Created, s.MODIFIED as Modified, u.LOGIN as LastModifiedByLogin",
                "VE_STYLE s inner join users u on s.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : escapedOrderColumnName,
                "",
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesBySiteId(DbConnection sqlConnection, int siteId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"SELECT
s.{Escape(dbType, "ID")},
s.NAME,
s.DESCRIPTION,
s.TAG,
s.{Escape(dbType, "ORDER")},
s.OVERRIDES_TAG,
s.IS_FORMAT,
s.IS_SYSTEM,
bnd.{Escape(dbType, "ON")},
s.ATTRIBUTES,
s.STYLES,
s.CREATED,
s.MODIFIED,
s.LAST_MODIFIED_BY
from VE_STYLE_SITE_BIND bnd INNER JOIN VE_STYLE s ON bnd.STYLE_ID = s.ID where bnd.SITE_ID = {siteId}
ORDER BY {Escape(dbType, "ORDER")}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetVisualEditorStylesByFieldId(DbConnection sqlConnection, int fieldId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"
SELECT s.{Escape(dbType, "ID")},
s.NAME,
s.DESCRIPTION,
s.TAG,
s.{Escape(dbType, "ORDER")},
s.OVERRIDES_TAG,
s.IS_FORMAT,
s.IS_SYSTEM,
bnd.{Escape(dbType, "ON")},
s.ATTRIBUTES,
s.STYLES,
s.CREATED,
s.MODIFIED,
s.LAST_MODIFIED_BY
from VE_STYLE_FIELD_BIND bnd INNER JOIN VE_STYLE s ON bnd.STYLE_ID = s.ID where bnd.FIELD_ID = {fieldId} ORDER BY {Escape(dbType, "ORDER")}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void UpdateOrInsertFieldVeStyleValue(DbConnection sqlConnection, int fieldId, int styleId, bool value)
        {
            var dbType = GetDbType(sqlConnection);
            string query;
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    query = @"begin tran
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
                    break;
                case DatabaseType.Postgres:
                    query = $@"
INSERT INTO VE_STYLE_FIELD_BIND (style_id, field_id, {Escape(dbType, "on")})
    VALUES( @styleId, @fieldId, @val )
    ON CONFLICT(style_id, field_id)
    DO UPDATE SET {Escape(dbType, "on")} = @val;
";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@styleId", styleId);
                cmd.Parameters.AddWithValue("@fieldId", fieldId);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetWorkflowsPage(DbConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Workflow,
                $"w.WORKFLOW_ID AS Id, w.WORKFLOW_NAME as {Escape(dbType, "Name")}, w.DESCRIPTION as Description, w.MODIFIED as Modified, w.CREATED as Created," +
                "w.LAST_MODIFIED_BY as LastModifiedBy, u.LOGIN as LastModifiedByLogin",
                "WORKFLOW w inner join users u on w.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : Escape(dbType, "Name"),
                "w.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetStatusTypePage(DbConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.StatusType,
                $"s.STATUS_TYPE_ID AS Id, s.STATUS_TYPE_NAME as {Escape(dbType, "Name")}, s.DESCRIPTION as Description, s.WEIGHT as Weight, s.MODIFIED as Modified, s.CREATED as Created," +
                " s.LAST_MODIFIED_BY as LastModifiedBy, u.LOGIN as LastModifiedByLogin",
                "STATUS_TYPE s inner join users u on s.LAST_MODIFIED_BY = u.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "Weight",
                "s.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetStatusTypeWeightsBySiteId(DbConnection connection, int siteId, int exceptId)
        {
            var dbType = GetDbType(connection);
            var query = $"select {Escape(dbType, "WEIGHT")} FROM {DbSchemaName(dbType)}.{Escape(dbType, "STATUS_TYPE")} where {Escape(dbType, "SITE_ID")} = @siteId and {Escape(dbType, "STATUS_TYPE_ID")} <> @id";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@siteId", siteId);
                cmd.Parameters.AddWithValue("@id", exceptId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static long GetNumberOfArticlesUsingStatusByStatusId(DbConnection connection, int id)
        {
            using (var cmd = DbCommandFactory.Create("select COUNT(*) from CONTENT_ITEM where STATUS_TYPE_ID = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return value == null ? 0 : long.Parse(value.ToString());
            }
        }

        public static long GetNumberOfWorkflowsUsingStatusByStatusId(DbConnection connection, int id)
        {
            using (var cmd = DbCommandFactory.Create("select COUNT(*) from workflow_rules where SUCCESSOR_STATUS_ID = @id", connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                var value = cmd.ExecuteScalar();
                return value == null ? 0 : long.Parse(value.ToString());
            }
        }

        public static void SetNullAssociatedNotificationsStatusTypesIds(DbConnection connection, int id)
        {
            var dbType = GetDbType(connection);

            var falseValue = SqlQuerySyntaxHelper.ToBoolSql(dbType, false);
            var query = $"UPDATE NOTIFICATIONS SET notify_on_status_type_id = null, for_status_changed = {falseValue}, for_status_partially_changed = {falseValue} WHERE notify_on_status_type_id = {id}";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAssociatedContentItemsStatusHistoryRecords(DbConnection connection, int id)
        {
            var dbType = GetDbType(connection);
            var query = $"DELETE FROM CONTENT_ITEM_STATUS_HISTORY {WithRowLock(dbType)}  where STATUS_TYPE_ID = {id}";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveAssociatedWaitingForApprovalRecords(DbConnection connection, int id)
        {
            var query = $"DELETE FROM WAITING_FOR_APPROVAL where STATUS_TYPE_ID = {id}";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static HashSet<int> GetTreeIdsToLoad(DbConnection sqlConnection, string entityTypeCode, string selectedIds)
        {
            using (var cmd = DbCommandFactory.Create($"select * from ENTITY_TYPE where CODE = '{entityTypeCode}'", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    return new HashSet<int>();
                }

                var row = dt.Rows[0];
                var parentFieldName = row.Field<string>("RECURRING_ID_FIELD");

                if (string.IsNullOrEmpty(parentFieldName))
                {
                    return new HashSet<int>();
                }

                var idFieldName = row.Field<string>("ID_FIELD");
                var tableName = row.Field<string>("SOURCE");
                return GetTreeIdsToLoad(sqlConnection, tableName, parentFieldName, idFieldName, selectedIds);
            }
        }

        public static HashSet<int> GetTreeIdsToLoad(DbConnection sqlConnection, string tableName, string parentFieldName, string idFieldName, string selectedIds)
        {
            var result = new HashSet<int>();
            if (!string.IsNullOrEmpty(selectedIds))
            {
                var dbType = GetDbType(sqlConnection);
                var sb = new StringBuilder();
                sb.AppendLine($"WITH {SqlQuerySyntaxHelper.RecursiveCte(dbType)} IDS (ID)");
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

                using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((int)dr.GetDecimal(0));
                        }
                    }
                }
            }

            return result;
        }

        public static IEnumerable<DataRow> GetCommandBindingBySiteId(DbConnection sqlConnection, int siteId)
        {
            var dbType = GetDbType(sqlConnection);

            var query = $"SELECT COMMAND_ID, {Escape(dbType, "ON")} FROM VE_COMMAND_SITE_BIND WHERE SITE_ID = {siteId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetCommandBindingByFieldId(DbConnection sqlConnection, int fieldId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $"SELECT COMMAND_ID, {Escape(dbType, "ON")} FROM VE_COMMAND_FIELD_BIND WHERE FIELD_ID = {fieldId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetStyleBindingByFieldId(DbConnection sqlConnection, int fieldId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $"SELECT STYLE_ID, {Escape(dbType, "ON")} FROM VE_STYLE_FIELD_BIND WHERE FIELD_ID = {fieldId}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetStyleBindingBySiteId(DbConnection sqlConnection, int siteId)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $"SELECT STYLE_ID, {Escape(dbType, "ON")} FROM VE_STYLE_SITE_BIND WHERE SITE_ID = {siteId}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        /// <summary>
        /// Возвращает id агрегированных статей для статьи контента
        /// </summary>
        public static IEnumerable<decimal> GetAggregatedArticlesIDs(DbConnection connection, int articleId, int[] classifierFields, int[] types, bool isLive)
        {
            var dbType = GetDbType(connection);

            if (dbType == DatabaseType.SqlServer)
            {
                return GetAggregatedArticlesIdsSqlServer(connection, articleId, classifierFields, types, isLive);
            }

            var query = $"select public.qp_get_aggregated_ids(@articleId::integer, @classifierIds, @contentIds, @isLive)";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@articleId", articleId);
                cmd.Parameters.Add(GetIdsDatatableParam("@classifierIds", classifierFields, dbType));
                cmd.Parameters.Add(GetIdsDatatableParam("@contentIds", types, dbType));
                cmd.Parameters.AddWithValue("@isLive", isLive);

                var result = cmd.ExecuteScalar();
                return Array.ConvertAll((int[])result, item => (decimal)item);
            }

        }

        private static IEnumerable<decimal> GetAggregatedArticlesIdsSqlServer(DbConnection connection, int articleId, int[] classifierFields, int[] types, bool isLive)
        {
            string query = $@"
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
                set @sql = @sql + 'select content_item_id from content_' + cast(@content_id as nvarchar(30)) + '{(isLive ? string.Empty : "_united")} where [' + @attribute_name + '] = @article_id'
                delete from @attrIds where attribute_id = @attribute_id
            end
            exec sp_executesql @sql, N'@article_id numeric', @article_id = @article_id";

            var result = new List<decimal>();
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@article_id", articleId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", classifierFields));
                cmd.Parameters.Add(GetIdsDatatableParam("@cids", types));
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
        public static IEnumerable<decimal> GetVariationArticlesIDs(DbConnection connection, int articleId, int contentId, string variationFieldName)
        {
            var query = $@"select c.CONTENT_ITEM_ID from content_{contentId}_united c with(nolock) where c.[{variationFieldName}] = @article_id";
            var result = new List<decimal>();
            using (var cmd = DbCommandFactory.Create(query, connection))
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

        public static void RemoveUserQueryAttrs(DbConnection connection, int contentId)
        {
            const string query = "delete from USER_QUERY_ATTRS WHERE VIRTUAL_CONTENT_ID = @content_id";
            using (var cmd = DbCommandFactory.Create(query, connection))
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
        public static void RemoveUnionAttrsByUnionContent(DbConnection connection, int contentId)
        {
            var query = $@"delete from UNION_ATTRS WHERE virtual_attr_id IN (
                                    select ATTRIBUTE_ID FROM CONTENT_ATTRIBUTE WHERE CONTENT_ID = @content_id
                                )";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("content_id", contentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveUnionAttrsByBaseFields(DbConnection connection, List<int> baseFieldIds)
        {
            if (baseFieldIds.Any())
            {
                var query = $"delete from UNION_ATTRS where union_attr_id in ({string.Join(",", baseFieldIds)})";
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void BatchInsertUserQueryAttrs(DbConnection connection, IEnumerable<UserQueryAttrsDAL> records)
        {
            var qb = new StringBuilder();
            foreach (var r in records)
            {
                qb.Append($@"INSERT INTO user_query_attrs (virtual_content_id, user_query_attr_id) VALUES ({r.VirtualContentId}, {r.UserQueryAttrId});");
            }

            var query = qb.ToString();
            if (!string.IsNullOrEmpty(query))
            {
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void BatchInsertUnionAttrs(DbConnection connection, IEnumerable<UnionAttrDAL> records)
        {
            var qb = new StringBuilder();
            foreach (var r in records)
            {
                qb.Append($@"INSERT INTO union_attrs (virtual_attr_id, union_attr_id) VALUES({r.VirtualFieldId}, {r.UnionFieldId});");
            }

            var query = qb.ToString();
            if (!string.IsNullOrEmpty(query))
            {
                using (var cmd = DbCommandFactory.Create(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SetWorkflowBindedContentId(DbConnection connection, int workflowId, int contentId)
        {
            var dbType = GetDbType(connection);
            var query = $"INSERT INTO content_workflow_bind (CONTENT_ID, WORKFLOW_ID, is_async) VALUES({contentId}, {workflowId}, {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)});";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CleanWorkflowContentBindedIds(DbConnection connection, int workflowId)
        {
            var query = $"DELETE FROM content_workflow_bind where WORKFLOW_ID = {workflowId};";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetWorkflowContentBindedIds(DbConnection connection, int workflowId)
        {
            var query = $"SELECT CONTENT_ID FROM content_workflow_bind where WORKFLOW_ID = {workflowId};";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int[] GetIdsToAutoArchive(DbConnection connection, IEnumerable<int> ids)
        {
            var result = new List<int>();
            const string query = "select content_item_id FROM @ids i inner join content_item ci on i.id = ci.content_item_id inner join content c on c.content_id = ci.content_id where c.auto_archive = 1";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids));
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add((int)dr.GetDecimal(0));
                    }
                }
            }

            return result.ToArray();
        }

        public static IEnumerable<DataRow> GetAcceptableBaseFieldIdsForCloning(DbConnection sqlConnection, string fieldName, string contentIds, int virtualContentId, bool forNew)
        {
            string sql;
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var attrName = dbType == DatabaseType.SqlServer ? "ATTRIBUTE_NAME" : "lower(ATTRIBUTE_NAME)";
            if (!forNew)
            {
                sql = $@"
                    select ATTRIBUTE_ID as id, coalesce(user_query_attr_id, 0) as sort_order from CONTENT_ATTRIBUTE ca
                    left join user_query_attrs uqa on ca.attribute_id = uqa.user_query_attr_id and uqa.virtual_content_id = @virtualContentId
                    where {attrName} = @attrName and CONTENT_ID in ({contentIds}) order by sort_order desc
                ";
            }
            else
            {
                sql = $@"
                    select ATTRIBUTE_ID as id from CONTENT_ATTRIBUTE ca
                    where {attrName} = @attrName and CONTENT_ID in ({contentIds})
                ";
            }

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@attrName", fieldName);
                cmd.Parameters.AddWithValue("@virtualContentId", virtualContentId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static string GetPermittedItemsAsQuery(DbConnection connection, int userId, int groupId, int startLevel, int endLevel, string entityName, string parentEntityName, int parentEntityId, QPModelDataContext context = null)
        {
            if (context == null)
            {
                var dbType = GetDbType(connection);
                switch (dbType)
                {
                    case DatabaseType.SqlServer:
                        context = new SqlServerQPModelDataContext(connection);
                        break;
                    case DatabaseType.Postgres:
                        context = new NpgSqlQPModelDataContext(connection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return PermissionHelper.GetPermittedItemsAsQuery(context, userId, groupId, startLevel, endLevel, entityName, parentEntityName, parentEntityId);
            // using (var cmd = DbCommandFactory.Create("qp_GetPermittedItemsAsQuery", connection))
            // {
            //     cmd.CommandType = CommandType.StoredProcedure;
            //     cmd.Parameters.AddWithValue("@user_id", userId);
            //     cmd.Parameters.AddWithValue("@group_id", groupId);
            //     cmd.Parameters.AddWithValue("@start_level", startLevel);
            //     cmd.Parameters.AddWithValue("@end_level", endLevel);
            //     cmd.Parameters.AddWithValue("@entity_name", entityName);
            //     cmd.Parameters.AddWithValue("@parent_entity_name", parentEntityName);
            //     cmd.Parameters.AddWithValue("@parent_entity_id", parentEntityId);
            //
            //     cmd.Parameters.Add(new SqlParameter("@SQLOut", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
            //     cmd.ExecuteNonQuery();
            //     return (string)cmd.Parameters["@SQLOut"].Value;
            // }
        }

        public static string GetPermittedItemsAsQueryV2(DbConnection connection, int userId, int groupId, int startLevel, int endLevel, string entityName, string parentEntityName, int parentEntityId)
        {
            throw new NotImplementedException();
        }


        public static bool TestM2MValue(DbConnection sqlConnection, int linkId, int articleId, int testArticleId)
        {
            var result =
                GetLinkedArticles(sqlConnection, new[] { linkId }, articleId, false)[linkId]
                    .Split(",".ToCharArray())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Select(int.Parse)
                    .ToDictionary(n => n);

            return result.ContainsKey(testArticleId);
        }

        public static bool TestM2OValue(DbConnection sqlConnection, FieldInfo fi, int articleId, int testArticleId)
        {
            var result =
                GetRelatedArticles(sqlConnection, new[] { fi }, articleId, false)[fi.Id]
                    .Split(",".ToCharArray())
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Select(int.Parse)
                    .ToDictionary(n => n);

            return result.ContainsKey(testArticleId);
        }

        /// <summary>
        /// Обновляет значения поля StringEnum в существующих статьях, в соответствии со составом перечисления
        /// </summary>
        public static void CorrectEnumInContentData(DbConnection sqlConnection, int fieldId, List<string> enumValues, string defValue)
        {
            if (enumValues.Any())
            {
                var query = $@"update content_data set DATA = @def_value, MODIFIED = GETDATE()
                    where ATTRIBUTE_ID = @field_id
                    and DATA not in ({string.Join(",", enumValues.Select(v => $"'{Cleaner.ToSafeSqlString(v)}'"))}) and DATA IS NOT NULL";

                using (var cmd = DbCommandFactory.Create(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@field_id", fieldId);
                    cmd.Parameters.AddWithValue("@def_value", (object)defValue ?? DBNull.Value);
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<DataRow> GetPageTemplatesBySiteId(DbConnection sqlConnection, int siteId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0)
        {
            var dbType = GetDbType(sqlConnection);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.PageTemplate,
                "p.PAGE_TEMPLATE_ID AS Id, p.is_system AS IsSystem, p.LOCKED_BY AS LockedBy, p.TEMPLATE_NAME AS Name, p.TEMPLATE_FOLDER AS Folder, p.DESCRIPTION AS Description, " +
                $"p.CREATED AS Created, p.MODIFIED AS Modified, p.LAST_MODIFIED_BY AS LastModifiedBy, u.LOGIN AS LastModifiedByLogin, {SqlQuerySyntaxHelper.ConcatStrValues(dbType, "u2.FIRST_NAME", "' '", "u2.LAST_NAME")} as LockedByFullName",
                "PAGE_TEMPLATE as p inner join users u on p.LAST_MODIFIED_BY = u.user_id left outer join users u2 on p.LOCKED_BY = u2.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : $"{Escape(dbType, "Name")} ASC",
                "p.SITE_ID = " + siteId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetPagesByTemplateId(DbConnection sqlConnection, int templateId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            var dbType = GetDbType(sqlConnection);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Page,
                "p.PAGE_ID AS Id, p.GENERATE_TRACE AS GenerateTrace, p.LOCKED_BY AS LockedBy, p.REASSEMBLE AS Reassemble, p.PAGE_NAME AS Name, p.DESCRIPTION AS Description, " +
                "p.PAGE_FILENAME as FileName, p.page_folder as Folder, p.CREATED AS Created, p.MODIFIED AS Modified, p.LAST_MODIFIED_BY AS LastModifiedBy, u.LOGIN AS LastModifiedByLogin, " +
                $"p.ASSEMBLED as Assembled, p.LAST_ASSEMBLED_BY as LastAssembledBy, uu.LOGIN as LastAssembledByLogin, {SqlQuerySyntaxHelper.ConcatStrValues(dbType, "u2.FIRST_NAME", "' '", "u2.LAST_NAME")} as LockedByFullName, t.TEMPLATE_NAME as TemplateName",
                $"{DbSchemaName(dbType)}.{Escape(dbType, "PAGE")} as p inner join users u on p.LAST_MODIFIED_BY = u.user_id INNER JOIN page_template as t on p.PAGE_TEMPLATE_ID = t.PAGE_TEMPLATE_ID left outer join users uu on p.LAST_ASSEMBLED_BY " +
                " = uu.user_id left outer join users u2 on p.LAST_ASSEMBLED_BY = u2.user_id",
                !string.IsNullOrEmpty(orderBy) ? orderBy : "Name Asc",
                "p.PAGE_TEMPLATE_ID = " + templateId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetTemplateObjectsByTemplateId(DbConnection sqlConnection, int templateId, string orderBy, out int totalRecords, int startRow, int pageSize)
        {
            var dbType = GetDbType(sqlConnection);

            var selectBlock = $@"
o.parent_object_id as parentId,
o.OBJECT_ID as Id,
o.ICON as Icon,
o.LOCKED_BY AS LockedBy,
o.OBJECT_NAME as Name,
o.DESCRIPTION as Description,
o.CREATED as Created,
o.MODIFIED as Modified,
u.LOGIN as LastModifiedByLogin,
t.TYPE_NAME as TypeName,
o.LAST_MODIFIED_BY AS LastModifiedBy,
{SqlQuerySyntaxHelper.ConcatStrValues(dbType, "u2.first_name", "' '", "u2.last_name")} as LockedByFullName,
{SqlQuerySyntaxHelper.CastToBool(dbType, $"case when oo.object_id is null then {SqlQuerySyntaxHelper.ToBoolSql(dbType, false)} else {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)} end")} as Overriden
 ";
            var fromBlock = $@"
TEMPLATE_OBJECT o inner join USERS u on o.LAST_MODIFIED_BY = u.USER_ID
inner join OBJECT_TYPE t on o.OBJECT_TYPE_ID = t.OBJECT_TYPE_ID
left outer join users u2 on o.LOCKED_BY = u2.user_id
left join (select distinct parent_object_id as object_id from object) oo on o.OBJECT_ID = oo.object_id ";
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.TemplateObject,
                selectBlock,
                fromBlock,
                !string.IsNullOrEmpty(orderBy) ? orderBy : $"{Escape(dbType, "Name")} ASC",
                "o.PAGE_TEMPLATE_ID = " + templateId,
                startRow,
                pageSize,
                out totalRecords
            );
        }

        public static IEnumerable<DataRow> GetPageObjectsByPageId(DbConnection sqlConnection, int pageId, string orderBy, out int totalRecords, int startRow, int pageSize) => GetSimplePagedList(
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

        public static IEnumerable<DataRow> GetObjectFormatsByObjectId(DbConnection sqlConnection, int objectId, string orderBy, out int totalRecords, int startRow, int pageSize, bool pageOrTemplate) => GetSimplePagedList(
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

        public static string GetSiteScriptLanguageByPageId(DbConnection sqlConnection, int pageId)
        {
            const string sql = @"SELECT s.script_language from PAGE p inner join page_template t on p.PAGE_TEMPLATE_ID = t.PAGE_TEMPLATE_ID inner join [SITE] s on t.SITE_ID = s.SITE_ID WHERE p.PAGE_ID = @pageId";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@pageId", pageId);
                var value = cmd.ExecuteScalar();
                return value.ToString();
            }
        }

        public static string GetSiteScriptLanguageByTemplateId(DbConnection sqlConnection, int templateId)
        {
            const string sql = @"SELECT s.script_language from page_template t inner join [SITE] s on t.SITE_ID = s.SITE_ID WHERE t.PAGE_TEMPLATE_ID = @templateId";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@templateId", templateId);
                var value = cmd.ExecuteScalar();
                return value.ToString();
            }
        }

        public static IEnumerable<DataRow> GetPagesBySiteId(DbConnection sqlConnection, int parentId, string orderBy, out int totalRecords, int startRow, int pageSize) => GetSimplePagedList(
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

        public static void CopyContentCustomActions(DbConnection sqlConnection, int sourceId, int destinationId)
        {
            var query = $@"
                INSERT INTO ACTION_CONTENT_BIND (CUSTOM_ACTION_ID, CONTENT_ID)
                SELECT CUSTOM_ACTION_ID, {destinationId} FROM  ACTION_CONTENT_BIND where CONTENT_ID = {sourceId}
            ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void AddHistoryStatus(DbConnection sqlConnection, int contentItemId, int systemStatusTypeId, int userId, string description)
        {
            var query = @"insert into content_item_status_history(CONTENT_ITEM_ID, SYSTEM_STATUS_TYPE_ID, USER_ID, DESCRIPTION) values ({0}, {1}, {2}, N'{3}')";

            query = string.Format(query, contentItemId, systemStatusTypeId, userId, description);
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static List<DataRow> GetStatusHistoryItem(DbConnection sqlConnection, int articleId)
        {
            var databaseType = GetDbType(sqlConnection);
            var query = $@"select
{(databaseType == DatabaseType.SqlServer ? "TOP 1" : string.Empty)}
    h.STATUS_HISTORY_ID as Id
    ,h.STATUS_HISTORY_DATE as ActionDate
    ,COALESCE(h1.DESCRIPTION, h.DESCRIPTION) as Comment
    ,t.STATUS_TYPE_NAME as StatusTypeName
    ,u.LOGIN as ActionMadeBy
    ,s.NAME as SystemStatusTypeName
from CONTENT_ITEM_STATUS_HISTORY as h {WithNoLock(databaseType)}
LEFT JOIN STATUS_TYPE as t {WithNoLock(databaseType)} on t.STATUS_TYPE_ID = h.STATUS_TYPE_ID
LEFT JOIN USERS as u {WithNoLock(databaseType)} on u.USER_ID = h.USER_ID
LEFT JOIN (SELECT DESCRIPTION, SYSTEM_STATUS_TYPE_ID, STATUS_HISTORY_ID FROM CONTENT_ITEM_STATUS_HISTORY {WithNoLock(databaseType)} where SYSTEM_STATUS_TYPE_ID BETWEEN 9 AND 14 AND content_item_id={articleId}) as h1 ON h1.STATUS_HISTORY_ID = (h.STATUS_HISTORY_ID + 1)
LEFT JOIN SYSTEM_STATUS_TYPE as s {WithNoLock(databaseType)} on s.ID = h1.SYSTEM_STATUS_TYPE_ID
where h.CONTENT_ITEM_ID = {articleId} AND h.SYSTEM_STATUS_TYPE_ID IS NULL
order by ActionDate desc
{(databaseType == DatabaseType.Postgres ? "LIMIT 1" : string.Empty)}
";
            // query = string.Format(query, artcileId);
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static List<DataRow> GetAllHistoryStatusesForArticle(DbConnection sqlConnection, int articleId, string orderBy, int startRow, int pageSize, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            var selectBlock = @"h.STATUS_HISTORY_ID as ID
                                            ,h.STATUS_HISTORY_DATE as ActionDate
                                            ,COALESCE(h1.DESCRIPTION, h.DESCRIPTION) as Comment
                                            ,t.STATUS_TYPE_NAME as StatusTypeName
                                            ,u.LOGIN as ActionMadeBy
                                            ,s.NAME as SystemStatusTypeName";

            var formattableString = $@"CONTENT_ITEM_STATUS_HISTORY as h {WithNoLock(dbType)}
                                    LEFT JOIN STATUS_TYPE as t {WithNoLock(dbType)} on t.STATUS_TYPE_ID = h.STATUS_TYPE_ID
                                    LEFT JOIN {Escape(dbType, "USERS")} as u {WithNoLock(dbType)} on u.USER_ID = h.USER_ID
                                    LEFT JOIN (SELECT DESCRIPTION, SYSTEM_STATUS_TYPE_ID, STATUS_HISTORY_ID FROM CONTENT_ITEM_STATUS_HISTORY {WithNoLock(dbType)} where SYSTEM_STATUS_TYPE_ID BETWEEN 9 AND 14 AND content_item_id={articleId}) as h1 ON h1.STATUS_HISTORY_ID = (h.STATUS_HISTORY_ID + 1)
                                    LEFT JOIN SYSTEM_STATUS_TYPE as s {WithNoLock(dbType)} on s.ID = h1.SYSTEM_STATUS_TYPE_ID";
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                selectBlock,
                formattableString,
                !string.IsNullOrEmpty(orderBy) ? orderBy : "ActionDate DESC",
                $"h.CONTENT_ITEM_ID = {articleId}",
                startRow,
                pageSize,
                out totalRecords
            ).ToList();
        }

        public static List<DataRow> GetArticlesForExport(DbConnection sqlConnection, int contentId, string extensions, string columns, string filter, int startRow, int pageSize, string orderBy, out int totalRecords)
        {
            var dbType = GetDbType(sqlConnection);
            return GetSimplePagedList(
                sqlConnection,
                EntityTypeCode.Article,
                $"base.{Escape(dbType, "content_item_id")} {columns}, ci.unique_id, base.created, base.modified",
                $"content_{contentId}_united base {extensions} LEFT JOIN CONTENT_ITEM ci ON base.content_item_id = ci.content_item_id",
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
        public static int[] GetReferencedAggregatedContentIds(QPModelDataContext context, DbConnection sqlConnection, int contentId, int[] articleIds, bool isArchive = false)
        {

            var dbType = GetDbType(sqlConnection);
            if (context == null)
            {
                switch (dbType)
                {
                    case DatabaseType.SqlServer:
                        context = new SqlServerQPModelDataContext(sqlConnection);
                        break;
                    case DatabaseType.Postgres:
                        context = new NpgSqlQPModelDataContext(sqlConnection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var fieldNames = context
                .FieldSet
                .Where(x => x.ContentId == contentId && x.IsClassifier)
                .Select(x => Escape(dbType, x.Name))
                .ToArray();

            if (!fieldNames.Any())
            {
                return new int[0];
            }

            var query = $@"
                    select distinct {string.Join(",", fieldNames)}
                    from content_{contentId}
                    where {SqlQuerySyntaxHelper.CastToBool(dbType, "archive")} = @archive
                    {(articleIds != null && articleIds.Any()
                    ? $"AND content_item_id in (select Id from {IdList(dbType, "@articleIds")})"
                    : string.Empty)}
                    ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.Add(GetIdsDatatableParam("@articleIds", articleIds, dbType));
                cmd.Parameters.AddWithValue("@archive", isArchive);
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<int>();
                    while (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            if (!reader.IsDBNull(i))
                            {
                                result.Add((int)reader.GetDecimal(i));
                            }
                        }
                    }

                    return result.ToArray();
                }
            }
        }

        public static int[] GetReferencedAggregatedContentIds(DbConnection sqlConnection, int contentId)
        {
            var dbType = GetDbType(sqlConnection);

            string query = $@"
                SELECT
                    af.CONTENT_ID
                FROM
                    CONTENT_ATTRIBUTE cf
                    JOIN CONTENT_ATTRIBUTE af ON cf.ATTRIBUTE_ID = af.RELATED_ATTRIBUTE_ID
                WHERE
                    cf.CONTENT_ID = @contentId AND
                    af.AGGREGATED = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static Dictionary<int, string> GetAggregatedFieldNames(DbConnection sqlConnection, int[] contentIds)
        {
            var dbType = GetDbType(sqlConnection);
            using (var cmd = DbCommandFactory.Create($"SELECT CONTENT_ID, ATTRIBUTE_NAME FROM CONTENT_ATTRIBUTE JOIN {IdList(dbType, "@ids")} ON CONTENT_ID = ID WHERE AGGREGATED = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", contentIds, dbType));
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

        public static Dictionary<int, Dictionary<int, int>> GetAggregatedArticleIdsMap(QPModelDataContext efContext, DbConnection sqlConnection, int contentId, int[] articleIds)
        {
            var dbType = GetDbType(sqlConnection);




            // string query = @"
            //     DECLARE @query NVARCHAR(MAX)
            //     SET @query = ''
            //
            //     SELECT
            //         @query = @query + '
            //         SELECT
            //             ids.Id [Id],' +
            //             CONVERT(NVARCHAR(10), f.ATTRIBUTE_ID) +' [FieldId],
            //             a.CONTENT_ITEM_ID [ExtensionId]
            //         FROM
            //             @ids ids
            //             JOIN CONTENT_' + CONVERT(NVARCHAR(10), ef.CONTENT_ID) + ' a ON a.' + ef.ATTRIBUTE_NAME +' = ids.Id
            //         UNION'
            //     FROM
            //         [CONTENT_ATTRIBUTE] f
            //         JOIN [CONTENT_ATTRIBUTE] ef ON ef.CLASSIFIER_ATTRIBUTE_ID = f.ATTRIBUTE_ID
            //     WHERE
            //         f.CONTENT_ID = @contentId
            //
            //     IF @query <> ''
            //     BEGIN
            //         SET @query = LEFT(@query, LEN(@query) - LEN('UNION'))
            //         EXEC sp_executesql @query, N'@ids Ids READONLY', @ids
            //     END";


            var fields = efContext
                .FieldSet
                .Include(x => x.Aggregators)
                .ThenInclude(y => y.Classifier)
                .Where(x => x.Classifier != null && x.Classifier.ContentId == contentId);

            var queryParts = fields.Select(ef => $@"
                SELECT
                    ids.Id,
                    {ef.Classifier.Id} {Escape(dbType, "FieldId")},
                    a.content_item_id {Escape(dbType, "ExtensionId")}
                FROM
                    {IdList(dbType, "@ids", "ids")}
                    JOIN content_{ef.ContentId} a ON a.{Escape(dbType, ef.Name)} = ids.Id
                ").ToList();
            var query = string.Join(" UNION ", queryParts);


            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                // cmd.Parameters.AddWithValue("@contentId", contentId);
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", articleIds, dbType));
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new Dictionary<int, Dictionary<int, int>>();
                    while (reader.Read())
                    {
                        var id = Converter.ToInt32(reader["Id"]);
                        var fieldId = Converter.ToInt32(reader["FieldId"]);
                        var extensionId = Converter.ToInt32(reader["ExtensionId"]);

                        Dictionary<int, int> articleMap;
                        if (result.ContainsKey(id))
                        {
                            articleMap = result[id];
                        }
                        else
                        {
                            articleMap = new Dictionary<int, int>();
                            result[id] = articleMap;
                        }

                        articleMap[fieldId] = extensionId;
                    }

                    return result;
                }
            }
        }

        public static Dictionary<int, string> GetFieldNames(DbConnection sqlConnection, int[] referenceFieldIds)
        {
            using (var cmd = DbCommandFactory.Create("SELECT f.ATTRIBUTE_ID, f.ATTRIBUTE_NAME FROM CONTENT_ATTRIBUTE f JOIN @ids ids ON f.ATTRIBUTE_ID = ids.ID", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(GetIdsDatatableParam("@ids", referenceFieldIds));
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

        public static void SetPagesAndObjectsEnableViewState(DbConnection sqlConnection, int pageTemplateId, bool enableViewState)
        {
            var objectQuery = $"UPDATE [object] SET ENABLE_VIEWSTATE = {(enableViewState ? 1 : 0)} WHERE page_template_id = {pageTemplateId}";
            var pageQuery = $"UPDATE [page] SET ENABLE_VIEWSTATE = {(enableViewState ? 1 : 0)} WHERE page_template_id = {pageTemplateId}";

            using (var cmd = DbCommandFactory.Create(objectQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = DbCommandFactory.Create(pageQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetObjectsDisableDataBinding(DbConnection sqlConnection, int pageTemplateId, bool disableDataBinding)
        {
            var query = $"UPDATE [object] SET DISABLE_DATABIND = {(disableDataBinding ? 1 : 0)} WHERE page_template_id = {pageTemplateId}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetCustomClassForPages(DbConnection sqlConnection, int pageTemplateId, string customClass)
        {
            var query = $"UPDATE [page] SET [page_custom_class] = '{customClass}' WHERE page_template_id = {pageTemplateId}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetObjectTypeId(DbConnection sqlConnection, string typeName)
        {
            var query = $"SELECT [OBJECT_TYPE_ID] FROM [OBJECT_TYPE] WHERE [TYPE_NAME] = '{typeName}'";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (int)(decimal)cmd.ExecuteScalar();
            }
        }

        public static void SetCustomClassForObjects(DbConnection sqlConnection, int pageTemplateId, string customClassForGenerics, string customClassForContainers, string customClassForForms)
        {
            const string generic = "Generic";
            const string container = "Publishing Container";
            const string form = "Publishing Form";

            var genericTypeId = GetObjectTypeId(sqlConnection, generic);
            var containerTypeId = GetObjectTypeId(sqlConnection, container);
            var formTypeId = GetObjectTypeId(sqlConnection, form);

            var genericQuery = $"UPDATE [OBJECT] set control_custom_class = '{customClassForGenerics}' WHERE page_template_id = {pageTemplateId} AND OBJECT_TYPE_ID = {genericTypeId}";
            var containerQuery = $"UPDATE [OBJECT] set control_custom_class = '{customClassForContainers}' WHERE page_template_id = {pageTemplateId} AND OBJECT_TYPE_ID = {containerTypeId}";
            var formQuery = $"UPDATE [OBJECT] set control_custom_class = '{customClassForForms}' WHERE page_template_id = {pageTemplateId} AND OBJECT_TYPE_ID = {formTypeId}";

            using (var cmd = DbCommandFactory.Create(genericQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = DbCommandFactory.Create(containerQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = DbCommandFactory.Create(formQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteObjectContainer(DbConnection sqlConnection, int objectId)
        {
            var query = "Delete FROM CONTAINER WHERE [object_id] = " + objectId;

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteObjectForm(DbConnection sqlConnection, int objectId)
        {
            var query = "Delete FROM CONTENT_FORM WHERE [object_id] = " + objectId;

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static List<DataRow> GetActiveStatusesByObjectId(DbConnection sqlConnection, int objectId)
        {
            const string query = @"SELECT s.STATUS_TYPE_ID, s.WEIGHT, s.SITE_ID, s.STATUS_TYPE_NAME, s.DESCRIPTION, s.Created, s.Modified, s.LAST_MODIFIED_BY, s.BUILT_IN  FROM CONTAINER_STATUSES AS cs INNER JOIN STATUS_TYPE AS s ON cs.[STATUS_TYPE_ID] = s.[STATUS_TYPE_ID] INNER JOIN users as u on u.USER_ID = s.LAST_MODIFIED_BY WHERE cs.[OBJECT_ID] = @objectId";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@objectId", objectId);
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static IEnumerable<DataRow> GetStatusPageForWorkflow(DbConnection sqlConnection, int workflowId, string orderBy, out int totalRecords, int startRow = 0, int pageSize = 0) => GetSimplePagedList(
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

        public static IEnumerable<DataRow> GetAllStatusesForWorkflow(DbConnection sqlConnection, int workflowId)
        {
            var query = @"SELECT s.STATUS_TYPE_ID as Id, s.STATUS_TYPE_NAME as Name, s.DESCRIPTION as Description, s.[WEIGHT] as Weight,
                            s.MODIFIED as Modified, s.[CREATED] as Created, s.LAST_MODIFIED_BY as LastModifiedBy, s.BUILT_IN, u.[LOGIN] as LastModifiedByLogin
                            FROM [workflow_rules] as wr
                            inner join status_type as s on s.[STATUS_TYPE_ID] = wr.[SUCCESSOR_STATUS_ID] inner join USERS as u on u.[USER_ID] = s.LAST_MODIFIED_BY
                            where WORKFLOW_ID = " + workflowId;
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void SetObjectActiveStatuses(DbConnection sqlConnection, int objectId, IEnumerable<int> activeStatusIds)
        {
            foreach (var statusId in activeStatusIds)
            {
                var query = $@"INSERT INTO CONTAINER_STATUSES([OBJECT_ID], [STATUS_TYPE_ID]) values ({objectId}, {statusId})";
                using (var cmd = DbCommandFactory.Create(query, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<int> GetObjectActiveStatusesIds(DbConnection sqlConnection, int objectId)
        {
            var result = new List<int>();
            const string query = "select [STATUS_TYPE_ID] FROM [CONTAINER_STATUSES] WHERE [OBJECT_ID] = @objectId";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static void CleanObjectActiveStatuses(DbConnection sqlConnection, int objectId)
        {
            var query = $@"DELETE FROM CONTAINER_STATUSES where [OBJECT_ID] = {objectId}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UnlockAllTemplatesLockedByUser(DbConnection sqlConnection, int userId)
        {
            var databaseType = GetDbType(sqlConnection);
            var falseValueStr = GetBoolValue(false, databaseType);
            var templateQuery = $"update {EscapeObjectName("PAGE_TEMPLATE", databaseType)} set locked_by = null, locked = null where permanent_lock = {falseValueStr} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(templateQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var pageQuery = $"update {EscapeObjectName("PAGE", databaseType)} set locked_by = null, locked = null where permanent_lock = {falseValueStr} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(pageQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var objectQuery = $"update {EscapeObjectName("OBJECT", databaseType)} set locked_by = null, locked = null where permanent_lock = {falseValueStr} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(objectQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }

            var formatQuery = $"update {EscapeObjectName("OBJECT_FORMAT", databaseType)} set locked_by = null, locked = null where permanent_lock = {falseValueStr} and locked_by = @user_id";
            using (var cmd = DbCommandFactory.Create(formatQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private static string EscapeObjectName(string objectName, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SqlServer:
                    return $"[{objectName}]";
                case DatabaseType.Postgres:
                    return $"\"{objectName.ToLower()}\"";
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null);
            }
        }



        public static IEnumerable<DataRow> GetFormatIdsByTemplateId(DbConnection sqlConnection, int templateId)
        {
            const string query = @"SELECT f.[OBJECT_FORMAT_ID] FROM [OBJECT_FORMAT] AS f " +
                "INNER JOIN [OBJECT] AS o ON f.[OBJECT_ID] = o.[OBJECT_ID] WHERE o.PAGE_TEMPLATE_ID = @templateId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@templateId", templateId);
                cmd.ExecuteNonQuery();
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static IEnumerable<DataRow> AssembleAction_TemplatePages(int templateId, DbConnection sqlConnection)
        {
            const string query = "select P.PAGE_ID Id, T.TEMPLATE_NAME Template, p.PAGE_NAME Name from PAGE P JOIN PAGE_TEMPLATE T ON P.PAGE_TEMPLATE_ID = T.PAGE_TEMPLATE_ID where T.PAGE_TEMPLATE_ID = @template_id order by P.PAGE_ID";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@template_id", templateId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToList();
            }
        }

        public static IEnumerable<int> AssembleAction_GetTemplateFormatIds(int templateId, DbConnection sqlConnection)
        {
            var result = new List<int>();
            const string query = @"SELECT f.[OBJECT_FORMAT_ID] FROM [OBJECT_FORMAT] AS f " +
                "INNER JOIN [OBJECT] AS o ON f.[OBJECT_ID] = o.[OBJECT_ID] INNER JOIN [PAGE_TEMPLATE] AS t on o.[PAGE_TEMPLATE_ID] = t.[PAGE_TEMPLATE_ID] " +
                "INNER JOIN [NOTIFICATIONS] AS n ON n.[FORMAT_ID] = f.[OBJECT_FORMAT_ID] WHERE t.PAGE_TEMPLATE_ID = @template_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static void IterateRows(DbConnection connection, string sqlString, Action<IEnumerable<object>> rowIterator)
        {
            using (var cmd = DbCommandFactory.Create(sqlString, connection))
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

        public static void UpdatePageAndObjectDateModifiedByObjectId(int objectId, int pageId, DbConnection sqlConnection)
        {
            var query = $"declare @date datetime set @date = getdate() Update page set MODIFIED = @date where [PAGE_ID] = {pageId} Update OBJECT set MODIFIED = @date where [OBJECT_ID] = {objectId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateObjectDateModified(int objectId, DbConnection sqlConnection)
        {
            var query = $"Update OBJECT set MODIFIED = getdate() where [OBJECT_ID] = {objectId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetSearchFormatPage(DbConnection sqlConnection, string sortExpression, int siteId, int? templateId, int? pageId, string filter, out int totalRecords, int start, int pageSize)
        {
            var filters = new List<string>
            {
                $"s.SITE_ID = '{Cleaner.ToSafeSqlString(siteId.ToString())}'"
            };

            if (templateId.HasValue)
            {
                filters.Add($"t.[PAGE_TEMPLATE_ID] = '{Cleaner.ToSafeSqlString(templateId.ToString())}'");
            }

            if (pageId.HasValue)
            {
                filters.Add($"p.[PAGE_ID] = '{Cleaner.ToSafeSqlString(pageId.ToString())}'");
            }

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

        public static IEnumerable<DataRow> GetSearchTemplatePage(DbConnection sqlConnection, string sortExpression, int siteId, string filter, out int totalRecords, int start, int pageSize)
        {
            var filters = new List<string>
            {
                $"s.SITE_ID = '{Cleaner.ToSafeSqlString(siteId.ToString())}'"
            };

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filters.Add(string.Format("(t.[TEMPLATE_BODY] like '%{0}%' OR t.[CODE_BEHIND] like '%{0}%')", Cleaner.ToSafeSqlString(filter)));
            }

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

        public static IEnumerable<DataRow> GetSearchObjectPage(DbConnection sqlConnection, string sortExpression, int siteId, int? templateId, int? pageId, string filter, out int totalRecords, int start, int pageSize)
        {
            var primaryFilters = new List<string>
            {
                $"s.[SITE_ID] = '{Cleaner.ToSafeSqlString(siteId.ToString())}'"
            };

            if (templateId.HasValue)
            {
                primaryFilters.Add($"o.[PAGE_TEMPLATE_ID] = '{Cleaner.ToSafeSqlString(templateId.ToString())}'");
            }

            if (pageId.HasValue)
            {
                primaryFilters.Add($"o.[PAGE_ID] = '{Cleaner.ToSafeSqlString(pageId.ToString())}'");
            }

            filter = Cleaner.ToSafeSqlString(filter);
            var secondaryFilters = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                secondaryFilters.Add("[counter] >0");
                secondaryFilters.Add($"cont.[FILTER_VALUE] like '%{filter}%'");
                secondaryFilters.Add($"cont.[ORDER_DYNAMIC] like '%{filter}%'");
                secondaryFilters.Add($"cont.[SELECT_START] = '{filter}'");
                secondaryFilters.Add($"cont.[SELECT_TOTAL] = '{filter}'");
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
                        " where   rowNum between {2} and {3}"
                    })
                , filter, string.Join(" AND ", primaryFilters), start, start + pageSize - 1,
                secondaryFilters.Count > 0 ? "and (" + string.Join(" OR ", secondaryFilters) + ")" : string.Empty,
                string.IsNullOrWhiteSpace(sortExpression) ? string.Empty : "ORDER BY " + sortExpression
            );

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                totalRecords = dt.Rows.Count != 0 ? (int)dt.Rows[0][QPModelDataContext.CountColumn] : 0;
                return dt.AsEnumerable().ToArray();
            }
        }


        public static void ChangeInsertIdentityState(DbConnection sqlConnection, string tableName, bool enable)
        {
            var option = enable ? "ON" : "OFF";
            var query = $"SET IDENTITY_INSERT {tableName} {option}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetFormatVersionsByFormatId(DbConnection sqlConnection, int formatId, string orderBy, out int totalRecords, int startRow, int pageSize, bool pageOrTemplate) => GetSimplePagedList(
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

        public static void RestoreObjectFormatVersion(DbConnection connection, int versionId)
        {
            using (var cmd = DbCommandFactory.Create("restore_object_format_version", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@version_id", versionId);
                cmd.ExecuteNonQuery();
            }
        }
        public static List<int> CheckForArticlesExistence(DbConnection sqlConnection, List<int> relatedIds, string condition, int contentId)
        {
            var result = new List<int>();
            if (!string.IsNullOrEmpty(condition))
            {
                condition = $" AND {condition}";
            }

            if (relatedIds.Count == 0)
            {
                relatedIds.Add(0);
            }

            var dbType = GetDbType(sqlConnection);

            var query = $"select content_item_id from content_{contentId} c {WithNoLock(dbType)} where content_item_id in ({string.Join(",", relatedIds)}) {condition}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static Dictionary<string, int> GetExistingArticleIdsMap(DbConnection sqlConnection, List<string> values, string fieldName, string condition, int contentId)
        {
            var result = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(condition))
            {
                condition = $" AND {condition}";
            }

            if (values.Count > 0)
            {
                var query = string.Format("select content_item_id, [{0}] from content_{1}_united with(nolock) where [{0}] in ({2}) {3}", fieldName, contentId, string.Join(",", values.Select(v => "'" + v + "'")), condition);
                using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static void RemoveLinksFromM2MField(DbConnection sqlConnection, int linkId, List<int> articleIds)
        {
            var query = string.Format(@"DELETE FROM [dbo].[item_to_item] WHERE ([l_item_id] IN ({0}) OR [r_item_id] IN ({0})) and [link_id] = {1}", string.Join(",", articleIds), linkId);
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetDefaultGroupId(DbConnection sqlConnection, int siteId)
        {
            var query = $@"select content_group_id from content_group where site_id = {siteId} and name = 'Default Group'";
            // var query = $@"select [dbo].qp_default_group_id({siteId})";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return DBNull.Value.Equals(result) ? 0 : Convert.ToInt32(result);
            }
        }

        public static string GetNewTemplateObjectName(DbConnection sqlConnection, int contentId)
        {
            string objectName;
            using (var cmd = DbCommandFactory.Create("qp_get_new_template_object_name", sqlConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                objectName = cmd.ExecuteScalar().ToString();
            }

            return objectName;
        }

        public static void CreateContainerStatusBind(DbConnection sqlConnection, int objectId, int contentId)
        {
            var query = $@"INSERT INTO CONTAINER_STATUSES SELECT {objectId}, status_type_id FROM workflow AS w " + "INNER JOIN workflow_rules AS wr ON w.workflow_id = wr.workflow_id " + "INNER JOIN status_type s ON wr.successor_status_id = s.status_type_id " + "INNER JOIN content_workflow_bind cwb ON cwb.workflow_id = w.workflow_id " + "WHERE cwb.content_id = @in_content_id ORDER BY rule_order";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static string FormatBodyNet(int contentId, DbConnection sqlConnection)
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

            const string query = "SELECT attribute_name FROM content_attribute WHERE content_id = @in_content_id order by attribute_order";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        strUserFields.AppendLine(CreateRowNotifTable(dr["attribute_name"].ToString(), "<%#Field(\"" + dr["attribute_name"] + "\")%" + ">", -1));
                    }
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
                .AppendLine("   container.DataSource = Data;")
                .AppendLine("   container.DataBind();")
                .AppendLine("}")
                .Append(Environment.NewLine)
                .AppendLine("override public void InitUserHandlers(EventArgs e)")
                .AppendLine("{")
                .AppendLine("   LoadContainer(this, e);")
                .AppendLine("}")
                .Append(Environment.NewLine)
                .AppendLine("protected void R1_ItemCreated(Object Sender, RepeaterItemEventArgs e)")
                .AppendLine("{")
                .AppendLine("   if(e.Item.ItemType == ListItemType.Item) {")
                .AppendLine("       id = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"content_item_id\"));")
                .AppendLine("           backendLink = String.Format(\"<a href='{0}?actionCode=edit_article&entityTypeCode=article&customerCode={1}&entityId={2}'>Link to the article</a>\",\"" + backendUrl + "\", \"" + currentCustomerCode + "\", id);")
                .AppendLine("       lastModifiedBy = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"last_modified_by\"));")
                .AppendLine("       statusTypeId = Int32.Parse(Field(Data.Rows[e.Item.ItemIndex], \"status_type_id\"));")
                .AppendLine("       switch (Value(\"on\")) {")
                .AppendLine("       case \"for_status_changed\": ")
                .AppendLine("           prevStatus = Status.GetPreviousStatus(id);")
                .AppendLine("           nextStatus = Status.GetStatusName(statusTypeId);")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article status was changed from \"+")
                .AppendLine("\"<strong>{0}</strong> to <strong>{1}</strong></div>\",")
                .AppendLine(" prevStatus, Status.GetStatusName(statusTypeId));")
                .AppendLine("           break;")
                .AppendLine("       case \"for_status_partially_changed\":")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong><br>\"+")
                .AppendLine("\"Already Approved: {1}<br>Waiting For Approval: {2}</div>\",")
                .AppendLine(" Status.GetUserName(lastModifiedBy), Status.GetParallelApproved(id, statusTypeId), Status.GetParallelWaitingForApproval(id));")
                .AppendLine("           break;")
                .AppendLine("       case \"for_create\": ")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was created by <strong>{0}</strong></div>\", ")
                .AppendLine("Status.GetUserName(lastModifiedBy));")
                .AppendLine("           break;")
                .AppendLine("       case \"for_modify\": ")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong></div>\", ")
                .AppendLine("Status.GetUserName(lastModifiedBy));")
                .AppendLine("           break;")
                .AppendLine("       case \"for_remove\": ")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was removed by <strong>{0}</strong></div>\", ")
                .AppendLine("Status.GetUserName(lastModifiedBy));")
                .AppendLine("           break;")
                .AppendLine("       case \"for_frontend\": ")
                .AppendLine("           eventDescription = \"<div style='margin: 5px 0px;'>Article was requested from frontend.</div>\";")
                .AppendLine("           break;")
                .AppendLine("       default: ")
                .AppendLine("           eventDescription = String.Format(\"<div style='margin: 5px 0px;'>Article was modified by <strong>{0}</strong></div>\", ")
                .AppendLine("Status.GetUserName(lastModifiedBy));")
                .AppendLine("           break;")
                .AppendLine("       }")
                .AppendLine("   }")
                .AppendLine("}");

            return codeBehind.ToString();
        }

        public static string FormatBodyVbScript(int contentId, string currentCustomerCode, string backendUrl, DbConnection sqlConnection)
        {
            var formatBody = new StringBuilder();
            formatBody.AppendLine(GetDefaultNotificationStyle())
                .AppendLine("<%")
                .AppendLine("   Dim prevStatus")
                .AppendLine("   prevStatus = GetPreviousStatus(Field(\"content_item_id\"))")
                .AppendLine("   If IsEmpty(prevStatus) Then prevStatus = \"None\" ")
                .AppendLine("%>")
                .AppendLine("<!-- Article Status -->")
                .AppendLine("<div style='margin: 5px 0px;'>Article status was changed from <strong><" + "%=prevStatus%" + "></strong> to <strong><" + "%=GetStatusName(Field(\"status_type_id\"))%" + "></strong></div>")
                .AppendLine("<table cellpadding='0' cellspacing='0' class='my_table'>")
                .AppendLine(CreateRowNotifTable("<strong>Field Name</strong>", "<strong>Field Value</strong>", 0))
                .AppendLine("<!-- User Fields -->");
            var strUserFields = new StringBuilder();
            const string strSql = "SELECT attribute_name FROM content_attribute WHERE content_id = @in_content_id";
            using (var cmd = DbCommandFactory.Create(strSql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@in_content_id", contentId);
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        strUserFields.AppendLine(CreateRowNotifTable(dr["attribute_name"].ToString(), "<%=Field(\"" + dr["attribute_name"] + "\")%>", -1));
                    }
                }
            }

            formatBody.AppendLine(strUserFields.ToString())
                .AppendLine("<!-- System Fields -->")
                .AppendLine(CreateRowNotifTable("Created", "<%=Field(\"created\")%>", -1))
                .AppendLine(CreateRowNotifTable("Modified", "<%=Field(\"modified\")%>", -1))
                .AppendLine(CreateRowNotifTable("Last Modified By", "<%=GetLastModifiedLogin(Field(\"last_modified_by\"))%>", -1))
                .AppendLine(CreateRowNotifTable(string.Empty, "<a href='" + backendUrl + "?actionCode=edit_article&entityTypeCode=article&customerCode=" + currentCustomerCode + "&entityId=<%=Field(\"content_item_id\")%>'>Link to the article</a>", -1))
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
                .AppendLine("   border: 1px solid Black;")
                .AppendLine("}")
                .AppendLine("td.left_td")
                .AppendLine("{")
                .AppendLine("   border-bottom-color:    Black;")
                .AppendLine("   border-bottom-style:    solid;")
                .AppendLine("   border-bottom-width:    1px;")
                .AppendLine("   border-right-style:     solid;")
                .AppendLine("   border-right-width:     1px;")
                .AppendLine("   padding:                4px;")
                .AppendLine("}")
                .AppendLine("td.right_td")
                .AppendLine("{")
                .AppendLine("   border-bottom-color:    Black;")
                .AppendLine("   border-bottom-style:    solid;")
                .AppendLine("   border-bottom-width:    1px;")
                .AppendLine("   padding:                4px;")
                .AppendLine("}")
                .AppendLine("</style>");

            return defaultStyle.ToString();
        }

        public static IEnumerable<DataRow> GetRestTemplateObjects(DbConnection sqlConnection, int templateId, int siteId)
        {
            var query = $@"select t.[TEMPLATE_NAME] as TemplateName, t.PAGE_TEMPLATE_ID, o.[OBJECT_ID], o.PAGE_ID,
                           o.[OBJECT_NAME] as ObjectName, CASE WHEN f.[OBJECT_FORMAT_ID] = o.[OBJECT_FORMAT_ID] THEN null ELSE f.[FORMAT_NAME] END as FormatName
                           from [OBJECT_FORMAT] as f inner join [OBJECT] as o on f.[OBJECT_ID] = o.[OBJECT_ID]
                           inner join [PAGE_TEMPLATE] as t on t.[page_template_id] = o.[page_template_id]
                           where t.[SITE_ID] = {siteId} and t.[PAGE_TEMPLATE_ID] <> {templateId} and o.[PAGE_ID] is null";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void DeleteObjectDefaultValues(DbConnection sqlConnection, int objectId)
        {
            using (var cmd = DbCommandFactory.Create($"delete from OBJECT_VALUES where OBJECT_ID = {objectId}", sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static bool PageFileNameExists(DbConnection sqlConnection, string checkPath, int siteId)
        {
            var query = $@"select COUNT(*)
                                         from PAGE as p left outer join PAGE_TEMPLATE as t on p.PAGE_TEMPLATE_ID = t.PAGE_TEMPLATE_ID
                                         inner join dbo.[SITE] as s on t.SITE_ID = s.[SITE_ID]
                                         where (s.LIVE_DIRECTORY + ISNULL(t.TEMPLATE_FOLDER, '') + ISNULL(p.PAGE_FOLDER, '') + p.PAGE_FILENAME) = '{checkPath}'
                                         and s.SITE_ID = {siteId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return (int)result > 0;
            }
        }

        public static void ClearFieldTreeOrder(DbConnection sqlConnection, int id)
        {
            var query = $@"UPDATE CONTENT_ATTRIBUTE set TREE_ORDER_FIELD = null where TREE_ORDER_FIELD = {id}";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void MoveFieldOrders(DbConnection sqlConnection, int contentId, int newOrder)
        {
            var query = $@"UPDATE CONTENT_ATTRIBUTE SET ATTRIBUTE_ORDER = ATTRIBUTE_ORDER + 1
                WHERE CONTENT_ID = {contentId} and ATTRIBUTE_ORDER >= {newOrder}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<int> GetActiveArticlesIdsForM2MField(DbConnection sqlConnection, int fieldId)
        {
            var query = $"select ARTICLE_ID FROM FIELD_ARTICLE_BIND WHERE FIELD_ID = {fieldId}";
            var result = new List<int>();
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
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

        public static void UpdateContentModification(DbConnection sqlConnection, int contentId)
        {
            var dbType = GetDbType(sqlConnection);
            var sql = $@"
                update content_modification {WithRowLock(dbType)}
                set live_modified = {Now(dbType)}, stage_modified = {Now(dbType)} where content_id = {contentId}
            ";
            ExecuteSql(sqlConnection, sql);
        }

        public static void SetFieldM2MDefValue(DbConnection sqlConnection, int[] defaultArticles, int fieldId)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"DELETE FROM FIELD_ARTICLE_BIND where FIELD_ID = {fieldId};");
            foreach (var artId in defaultArticles)
            {
                sb.AppendLine($@"INSERT INTO FIELD_ARTICLE_BIND(ARTICLE_ID,FIELD_ID) VALUES ({artId},{fieldId});");
            }

            ExecuteSql(sqlConnection, sb.ToString());
        }

        public static IEnumerable<DataRow> GetRelationsBetweenContents(DbConnection sqlConnection, int oldSiteId, int newSiteId, string newContentIds)
        {
            var sb = new StringBuilder();
            sb.Append(@"select c.content_id as source_content_id, nc.content_id as destination_content_id
                                from [dbo].[content] as c (nolock)
                                inner join [dbo].[content] as nc (nolock)
                                    on nc.content_name = c.content_name and nc.site_id = @newSiteId
                                    where c.site_id = @oldSiteId");

            if (!string.IsNullOrEmpty(newContentIds))
            {
                sb.AppendFormat(" and nc.content_id in ({0})", newContentIds);
            }

            using (var cmd = DbCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationshipsBetweenAttributes(DbConnection sqlConnection, int oldSiteId, int newSiteId, string relationsBetweenContentsXml)
        {
            var query = $@" declare @xmlprms xml = '{relationsBetweenContentsXml}'

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
                                where ra.destination_content_id in (select destinationId from relationsBetweenContents)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentItemAccess(DbConnection sqlConnection, string relationsBetweenItems)
        {
            var query = $@"
                                            declare @now DateTime = GetDate()
                                            declare @xmlprmsContentItems xml = '{relationsBetweenItems}'
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
                                                inner join relations_between_content_items as rbci on rbci.source_content_item_id = cia.content_item_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static Dictionary<int, int> GetArticleHierarchy(DbConnection sqlConnection, int contentId, string treeFieldName)
        {
            var result = new Dictionary<int, int>();
            var dbType = GetDbType(sqlConnection);
            var name = Escape(dbType, treeFieldName);
            var parentIdParam = string.IsNullOrEmpty(treeFieldName) ? "cast(0 as numeric)" : $"coalesce({name}, 0)";
            var sql = $"select content_item_id as id, {parentIdParam} as parent_id from content_{contentId}_united {WithNoLock(dbType)} where archive = 0";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
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

        public static void CopyUserQueryContents(DbConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = $@"
                                    declare @xmlprms xml = '{relationsBetweenContentsXml}'

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
                                    inner join relationsBetweenContents as rbc1 on uqc.real_content_id = rbc1.source_content_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyUserQueryAttributes(DbConnection sqlConnection, string relationsBetweenContents, string relationsBetweenAttributes)
        {
            var query = $@"

                                    declare @xmlprmsAttributes xml = '{relationsBetweenAttributes}'

                                    declare @relationsBetweenAttributes table(
                                        source_attribute_id int,
                                        destination_attribute_id int
                                    )

                                    insert into @relationsBetweenAttributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                    declare @xmlprms xml = '{relationsBetweenContents}'

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
                                    inner join @relationsBetweenAttributes as rba on rba.source_attribute_id = uqa.user_query_attr_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyContentItems(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string contentIdsToCopy, int startFrom, int endOn, string relationsBetweenContentsXml, string relationsBetweenStatusesXml)
        {
            var query = $@"
                                if OBJECT_ID('tempdb..#disable_ti_access_content_item') IS NULL begin
                                    select 1 as A into #disable_ti_access_content_item
                                end

                                declare @isVirtual bit = 0
                                declare @todaysDateTime datetime = GetDate();
                                declare @not_for_replication bit  = 1

                                declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'

                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )
                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)

                                declare @xmlprmsStatuses xml = '{relationsBetweenStatusesXml}'

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

                                select source_item_id, destination_item_id from @contentitemstable";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.Parameters.AddWithValue("@contentIdsToCopy", contentIdsToCopy);
                cmd.Parameters.AddWithValue("@startFrom", startFrom);
                cmd.Parameters.AddWithValue("@endOn", endOn);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentItemSchedule(DbConnection sqlConnection, string relationsNewContentItemsIdXml)
        {
            var query = $@"
                                declare @todaysDateTime datetime = GetDate()
                                declare @xmlprms xml = '{relationsNewContentItemsIdXml}'

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
                                            on cis.CONTENT_ITEM_ID = cist.source_content_item_id;";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenStatuses(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @" select st1.STATUS_TYPE_ID as source_status_type_id
                                                    ,st2.STATUS_TYPE_ID as destination_status_type_id
                                            from [dbo].[status_type] as st1 (NOLOCK)
                                            inner join [dbo].[status_type] as st2 (NOLOCK)
                                                on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME and st2.SITE_ID = @newSiteId
                                            where st1.SITE_ID = @oldSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenLinks(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @" select distinct oldvalues.link_id as source_link_id, newvalues.link_id as destination_link_id from (
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void UpdateContentData(DbConnection sqlConnection, string relationsBetweenAttributesXml, string relationsNewContentItemsIdXml)
        {
            var query = $@"

                                select 1 as A into #disable_tu_update_child_content_data;

                                declare @xmlprmsAttributes xml = '{relationsBetweenAttributesXml}'

                                declare @relations_between_attributes table (
                                    source_attribute_id int,
                                    destination_attribute_id int
                                )
                                insert into @relations_between_attributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                declare @xmlprmsContentItems xml = '{relationsNewContentItemsIdXml}'

                                declare @relations_between_content_items table (
                                    source_content_item_id int,
                                    destination_content_item_id int
                                )
                                insert into @relations_between_content_items
                                        select doc.col.value('./@sourceId', 'int') source_content_item_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_item_id
                                        from @xmlprmsContentItems.nodes('/items/item') doc(col)

                                update copydata
                                set [data] =(   case    when at.[TYPE_NAME] = 'Dynamic Image' then replace(cd.data, 'field_' + CAST(cd.ATTRIBUTE_ID as varchar), 'field_' + CAST(ra.destination_attribute_id as varchar))
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
                                    exec qp_replicate_items @ids ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetArticlesCountInSite(DbConnection sqlConnection, int siteId)
        {
            var query = $@"SELECT COUNT(CONTENT_ITEM_ID)
                      FROM [dbo].[CONTENT_ITEM] as it
                      INNER JOIN [dbo].[CONTENT] as c on c.CONTENT_ID = it.CONTENT_ID
                      where c.SITE_ID = {siteId}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var result = cmd.ExecuteScalar();
                return Converter.ToInt32(result);
            }
        }

        public static void UpdateAttributesAfterCopyingArticles(DbConnection sqlConnection, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = $@"declare @xmlprms xml = '{relationsBetweenArticles}'

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
                                        ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateO2MValues(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = $@"
                            declare @xmlprms xml = '{relationsBetweenArticles}'

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
                                where c.SITE_ID = @destinationSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyArticleWorkflowBind(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenArticles)
        {
            var query = $@"
                            declare @relations_between_items table(
                                olditemid numeric,
                                newitemid numeric
                            )
                            declare @xmlprms xml = '{relationsBetweenArticles}'

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
                                inner join relations_between_workflows as rbw on rbw.old_workflow_id = awb.workflow_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyFieldArticleBind(DbConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenAttributes)
        {
            var query = $@"
                                declare @xmlprmsAttributes xml = '{relationsBetweenAttributes}'

                                declare @relations_between_attributes table (
                                    source_attribute_id int,
                                    destination_attribute_id int
                                )
                                insert into @relations_between_attributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{relationsBetweenArticles}'

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
                                inner join @relations_between_attributes as ra on ra.source_attribute_id = fab.field_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentDataAfterCopyingArticles(DbConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = $@"
                                declare @xmlprmsLinks xml = '{relationsBetweenLinks}'

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

                                declare @xmlprmsItems xml = '{relationsBetweenArticles}'

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
                                    on lr1.newitemid = cd.content_item_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyItemToItems(DbConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = $@"

                                if OBJECT_ID('tempdb..#disable_ti_item_to_item') IS NULL begin
                                    select 1 as A into #disable_ti_item_to_item
                                end

                                declare @xmlprmsLinks xml = '{relationsBetweenLinks}'

                                declare @relations_between_links table (
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{relationsBetweenArticles}'

                                ;with relations_between_items as (
                                select doc.col.value('./@sourceId', 'int') olditemid
                                         ,doc.col.value('./@destinationId', 'int') newitemid
                                        from @xmlprmsItems.nodes('/item') doc(col)
                                )
                                insert into [dbo].[item_to_item] (link_id, l_item_id, r_item_id)
                                select r.destination_link_id, i1.l_item_id, i1.r_item_id
                                from [dbo].[item_to_item] as i1 (nolock)
                                inner join @relations_between_links as r
                                    on r.source_link_id = i1.link_id
                                where i1.l_item_id in (select olditemid from relations_between_items)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateItemToItem(DbConnection sqlConnection, string relationsBetweenArticles, string relationsBetweenLinks)
        {
            var query = $@"
                                declare @xmlprmsLinks xml = '{relationsBetweenLinks}'

                                if OBJECT_ID('tempdb..#disable_tu_item_to_item') IS NULL begin
                                    select 1 as A into #disable_tu_item_to_item
                                end

                                declare @relations_between_links table (
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @xmlprmsItems xml = '{relationsBetweenArticles}'

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
                                    on r.destination_link_id = ii.link_id
                                where ii.r_item_id in (select source_item_id from @relations_between_items)
";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void FillLinksTables(DbConnection sqlConnection, string relationsBetweenLinks)
        {
            var query = $@"
                                declare @xmlprmsLinks xml = '{relationsBetweenLinks}'

                                declare @relations_between_links table (
                                    id numeric identity(1,1) primary key,
                                    source_link_id int,
                                    destination_link_id int
                                )
                                insert into @relations_between_links
                                        select doc.col.value('./@sourceId', 'int') source_link_id
                                         ,doc.col.value('./@destinationId', 'int') destination_link_id
                                        from @xmlprmsLinks.nodes('/items/item') doc(col)

                                declare @i int, @count int, @link_id numeric

	                                set @i = 1
	                                select @count = count(id) from @relations_between_links

	                                while @i < @count + 1
	                                begin
		                                select @link_id = destination_link_id from @relations_between_links where id = @i
		                                exec qp_fill_link_table @link_id

		                                set @i = @i + 1
	                                end
                              ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyWorkflow(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"declare @todaysDate datetime
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteAccessRules(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"    declare @todaysDate datetime
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyActionSiteBind(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"insert into [dbo].[action_site_bind]
                                            select [custom_action_id]
                                                  ,@destinationSiteId
                                              from [dbo].[action_site_bind] (nolock)
                                              where site_id = @sourceSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyWorkflowAccess(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"declare @now DateTime = GetDate()
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyWorkflowRules(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @";with relations_between_workflows
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
                                            on wr.WORKFLOW_ID = rbw.old_workflow_id

	                                update [dbo].[workflow_rules] set SUCCESSOR_STATUS_ID = st.new_status_type
	                                    from [dbo].[workflow_rules] wr
	                                        inner join (select st1.STATUS_TYPE_ID as old_status_type,
                                                               st2.STATUS_TYPE_ID as new_status_type
                                                            from  [dbo].[STATUS_TYPE] st1
			                                                    inner join [dbo].[STATUS_TYPE] as st2
			                                                    on st1.STATUS_TYPE_NAME = st2.STATUS_TYPE_NAME
                                                                   and st2.SITE_ID = @destinationSiteId
			                                                where st1.SITE_ID = @sourceSiteId
			                                            ) as st
			                                on wr.successor_status_id = st.old_status_type
	                                        where wr.WORKFLOW_ID in (select WORKFLOW_ID from workflow where site_id = @destinationSiteId)
										";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyFolderAccess(DbConnection sqlConnection, string relationsBetweenFoldersXml)
        {
            var query = $@"
                            declare @now DateTime = GetDate()

                            declare @xmlprmsFolders xml = '{relationsBetweenFoldersXml}'

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
                                inner join relations_between_folders as rbf on rbf.source_folder_id = fa.[folder_id]";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyFolders(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"declare @todaysDate datetime
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyCommandSiteBind(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"delete from [dbo].[VE_COMMAND_SITE_BIND]
                                     where site_id = @destinationSiteId

                                     insert into [dbo].[VE_COMMAND_SITE_BIND]
                                     SELECT command_id
                                            ,@destinationSiteId
                                          ,[ON]
                                      FROM [dbo].[VE_COMMAND_SITE_BIND] (nolock)
                                      where site_id = @sourceSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyStyleSiteBind(int sourceSiteId, int destinationSiteId, DbConnection sqlConnection)
        {
            const string query = @"        delete from [dbo].[VE_STYLE_SITE_BIND]
                                     where site_id = @destinationSiteId

                                     insert into [dbo].[VE_STYLE_SITE_BIND]
                                     SELECT style_id
                                            ,@destinationSiteId
                                            ,[ON]
                                      FROM [dbo].[VE_STYLE_SITE_BIND] (nolock)
                                      where site_id = @sourceSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);

                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyContents(DbConnection sqlConnection, int oldSiteId, int newSiteId, int startFrom, int endOn)
        {
            var excludeColumns = new List<string> { "content_id" };
            var changeValues = new Dictionary<string, string>
            {
                { "site_id", newSiteId.ToString() },
                { "created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) },
                { "modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) }
            };

            var query = string.Format(@"declare @todaysDate datetime = GETDATE()
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
                GetColumnsForTable(sqlConnection, "content", excludeColumns, changeValues),
                GetColumnsForTable(sqlConnection, "content", excludeColumns));

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", oldSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", newSiteId);
                cmd.Parameters.AddWithValue("@startFrom", startFrom);
                cmd.Parameters.AddWithValue("@endOn", endOn);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopyContentConstraints(DbConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = $@"
                                    declare @xmlprms xml = '{relationsBetweenContentsXml}'

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

                                    select source_constraint_id, destination_constraint_id from @relations_between_constrains";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopyContentNotifications(DbConnection sqlConnection, string relationsBetweenContentsXml, string relationsBetweenStatusesXml, string relationsBetweenAttributesXml)
        {
            var changeValues = new Dictionary<string, string>
            {
                { "created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) },
                { "modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) },
                { "notify_on_status_type_id", "{rbs.destination_status_id}" },
                { "email_attribute_id", "{rba.destination_attribute_id}" }
            };

            var query = $@"
                                    declare @xmlprmsStatuses xml = '{relationsBetweenStatusesXml}'

                                    declare @relationsBetweenStatuses table(
                                        source_status_id int,
                                        destination_status_id int
                                    )

                                    insert into @relationsBetweenStatuses
                                        select doc.col.value('./@sourceId', 'int') source_status_id
                                         ,doc.col.value('./@destinationId', 'int') destination_status_id
                                        from @xmlprmsStatuses.nodes('/items/item') doc(col)

                                    declare @xmlprmsAttributes xml = '{relationsBetweenAttributesXml}'

                                    declare @relationsBetweenAttributes table(
                                        source_attribute_id int,
                                        destination_attribute_id int
                                    )

                                    insert into @relationsBetweenAttributes
                                        select doc.col.value('./@sourceId', 'int') source_attribute_id
                                         ,doc.col.value('./@destinationId', 'int') destination_attribute_id
                                        from @xmlprmsAttributes.nodes('/items/item') doc(col)

                                    declare @xmlprms xml = '{relationsBetweenContentsXml}'

                                    ;with relationsBetweenContents as (
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprms.nodes('/items/item') doc(col)
                                    )
                                    insert into [dbo].[notifications] ({GetColumnsForTable(sqlConnection, "notifications", new List<string> { "notification_id" })})
                                    select {GetColumnsForTable(sqlConnection, "notifications", new List<string> { "notification_id", "content_id" }, changeValues, new Dictionary<string, string> { { "rbc.destination_content_id", string.Empty } }, "format_id")}
                                    from [dbo].[notifications] as n
                                    inner join relationsBetweenContents as rbc on rbc.source_content_id = n.content_id
                                    left join @relationsBetweenStatuses as rbs on rbs.source_status_id = n.notify_on_status_type_id
                                    left join @relationsBetweenAttributes as rba on rba.source_attribute_id = n.email_attribute_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyDynamicImageAttributes(DbConnection sqlConnection, string relationsBetweenAttributesXml)
        {
            var query = $@"       declare @xmlprms xml = '{relationsBetweenAttributesXml}'

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
                                        inner join relationsBetweenAttributes as ra on dia.ATTRIBUTE_ID = ra.source_attribute_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateAttributesOrder(DbConnection sqlConnection, int destinationSiteId, string relationsBetweenAttributesXml, string newContentIds)
        {
            var forAttributesOfContents = string.Empty;
            if (!string.IsNullOrEmpty(newContentIds))
            {
                forAttributesOfContents = $"and ca.CONTENT_ID in ({newContentIds})";
            }

            var query = $@"     declare @xmlprms xml = '{relationsBetweenAttributesXml}'

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
                                    where c.SITE_ID = @newsiteid {forAttributesOfContents}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentWorkflowBind(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = $@"

                                    declare @xmlprms xml = '{relationsBetweenContentsXml}'

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
                                        where rbc.destination_content_id in (select destination_content_id from @relationsBetweenContents)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyUnionContents(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string newContentIds)
        {
            if (string.IsNullOrEmpty(newContentIds))
            {
                newContentIds = "0";
            }
            var query = $@"

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
                            where nvc1.content_id_new in ({newContentIds})";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateVirtualContentAttributes(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @"   ;with relationsBetweenContentLinks as (
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsGroups(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @"delete from [dbo].[content_group]
                                where site_id = @newsiteid

                            insert into [dbo].[content_group]
                            select @newsiteid
                                  ,[name]
                              from [dbo].[content_group] (nolock)
                              where site_id = @oldsiteid";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentGroupIds(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @";with relations_between_groups as
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyVirtualContents(DbConnection sqlConnection, int siteId, int newSiteId)
        {
            var excludeColumns = new List<string> { "content_id" };
            var changeValues = new Dictionary<string, string>
            {
                { "site_id", newSiteId.ToString() },
                { "created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) },
                { "modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) }
            };
            var fieldsToAdd = new Dictionary<string, string>
            {
                { "virtual_join_primary_content_id_new", "rc.content_id_new" }
            };

            var query = $@"set nocount on;

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
                                select {GetColumnsForTable(sqlConnection, "content", null, changeValues, fieldsToAdd, "virtual_join_primary_content_id")}
                                  from [dbo].[content] as c (nolock)
                                  left join @relscontents as rc on rc.content_id_old = c.[virtual_join_primary_content_id]
                              where virtual_type != 0 and site_id = @oldsiteid)
                              as src({GetColumnsForTable(sqlConnection, "content", null, null, new Dictionary<string, string> { { "virtual_join_primary_content_id_new", string.Empty } }, "virtual_join_primary_content_id")})
                              on 0 = 1
                              when not matched then
                               insert ({GetColumnsForTable(sqlConnection, "content", excludeColumns)})
                               values ({GetColumnsForTable(sqlConnection, "content", new List<string> { "content_id", "virtual_join_primary_content_id" }, null, new Dictionary<string, string> { { "virtual_join_primary_content_id_new", string.Empty } }, "is_shared")})
                               output src.[content_id], inserted.content_id, inserted.virtual_type, inserted.query, inserted.alt_query
                                into @newvirtualcontents;

                            select  content_id_old
                                , content_id_new
                                , virtual_type
                                , sqlquery
                                , altquery
                            from @newvirtualcontents

                            ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", siteId);
                cmd.Parameters.AddWithValue("@newsiteid", newSiteId);
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int GetSiteRealContentCount(int siteId, DbConnection connection)
        {
            const string query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id AND virtual_type = 0";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int GetSiteContentLinkCount(int siteId, DbConnection connection)
        {
            const string query = "select count(link_id) from SITE_CONTENT_LINK where SITE_ID = @site_id";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static int GetSiteVirtualContentCount(DbConnection connection, int siteId)
        {
            const string query = "select count(CONTENT_ID) from CONTENT where SITE_ID = @site_id AND virtual_type != 0";
            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenContents(DbConnection connection, int oldSiteId, int newSiteId)
        {
            const string query = @"select c.content_id as content_id_old
                                    , nc.content_id as content_id_new
                             from [dbo].[content] as c
                             inner join [dbo].[content] as nc on nc.content_name = c.content_name and nc.site_id = @newsiteid
                             where c.site_id = @oldsiteid";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", oldSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", newSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int CopySiteTemplates(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, int templateNumber)
        {
            const string query = @"
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.Parameters.AddWithValue("@templateNumber", templateNumber);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenTemplates(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, int templateIdNew)
        {
            const string query = @"select pto.page_template_id as source_template_id
                                    ,ptn.page_template_id as destination_template_id
                            from page_template as pto (nolock)
                            inner join page_template as ptn (nolock)
                                  on pto.template_name = ptn.template_name and ptn.site_id = @newSiteId
                            where pto.site_id = @oldSiteId and ptn.PAGE_TEMPLATE_ID = @templateIdNew";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);
                cmd.Parameters.AddWithValue("@templateIdNew", templateIdNew);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenPages(DbConnection sqlConnection, string relationsBetweenTemplates)
        {
            var query = $@" declare @xmlprms xml = '{relationsBetweenTemplates}'

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
                                    on pn.PAGE_TEMPLATE_ID = pt1.destination_page_template_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplatePages(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenTemplates)
        {
            var query = $@"
                            declare @now datetime = GETDATE()
                            declare @new_pages_added table(page_id int, page_template_id int)

                            declare @xmlprms xml = '{relationsBetweenTemplates}'

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

                            select page_id, page_template_id from @new_pages_added";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldsiteid", sourceSiteId);
                cmd.Parameters.AddWithValue("@newsiteid", destinationSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplateObjects(DbConnection sqlConnection, string relationsBetweenTemplates, string relationsBetweenPages)
        {
            var query = $@"
                            declare @now datetime = GETDATE()
                            declare @xmlprmsTemplates xml = '{relationsBetweenTemplates}'

                            declare @relations_between_templates table (
                                source_template_id int,
                                destination_template_id int
                            )
                            insert into @relations_between_templates
                                    select doc.col.value('./@sourceId', 'int') source_template_id
                                     ,doc.col.value('./@destinationId', 'int') destination_template_id
                                    from @xmlprmsTemplates.nodes('/items/item') doc(col)

                            declare @xmlprmsPages xml = '{relationsBetweenPages}'

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

                        select source_object_id, destination_object_id from @relations_between_objects";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> CopySiteTemplateObjectFormats(DbConnection sqlConnection, string relationsBetweenObjects)
        {
            var query = $@"

                            declare @relations_between_object_formats table(
                                source_object_format_id int,
                                destination_object_format_id int
                            )

                            declare @xmlprmsObjects xml = '{relationsBetweenObjects}'

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
                            from @relations_between_object_formats";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static void CopySiteUpdateObjects(DbConnection sqlConnection, string relationsBetweenObjectFormats, string relationsBetweenObjects)
        {
            var query = $@"
                                declare @xmlprmsObjectFormats xml = '{relationsBetweenObjectFormats}'

                                declare @relations_between_object_formats table(
                                    source_object_format_id int,
                                    destination_object_format_id int
                                )

                                insert into @relations_between_object_formats
                                        select doc.col.value('./@sourceId', 'int') source_object_format_id
                                         ,doc.col.value('./@destinationId', 'int') destination_object_format_id
                                        from @xmlprmsObjectFormats.nodes('/items/item') doc(col)

                                declare @xmlprmsObjects xml = '{relationsBetweenObjects}'

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
                                    on o.OBJECT_ID = rbo.destination_object_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteObjectValues(DbConnection sqlConnection, string relationsBetweenObjects)
        {
            var query = $@"

                                    declare @xmlprmsObjects xml = '{relationsBetweenObjects}'

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
                                            on ov.OBJECT_ID = rbo.source_object_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteContainers(DbConnection sqlConnection, string relationsBetweenObjects, string relationsBetweenContents)
        {
            var query = $@"
                                declare @xmlprmsContents xml = '{relationsBetweenContents}'
                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )

                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)

                                declare @xmlprmsObjects xml = '{relationsBetweenObjects}'

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
                                    on c.CONTENT_ID = rbc.source_content_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopySiteUpdateNotifications(DbConnection sqlConnection, string relationsBetweenObjectFormats, string relationsBetweenContents)
        {
            var query = $@"
                                declare @xmlprmsContents xml = '{relationsBetweenContents}'
                                declare @relations_between_contents table (
                                    source_content_id int,
                                    destination_content_id int
                                )

                                insert into @relations_between_contents
                                        select doc.col.value('./@sourceId', 'int') source_content_id
                                         ,doc.col.value('./@destinationId', 'int') destination_content_id
                                        from @xmlprmsContents.nodes('/items/item') doc(col)

                                declare @xmlprmsObjectFormats xml = '{relationsBetweenObjectFormats}'

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
                                where n.content_id in (select destination_content_id from @relations_between_contents)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetTemplatesElementsCountOnSite(DbConnection sqlConnection, int siteId)
        {
            const string query = @"SELECT COUNT(*)
                              from [dbo].[PAGE_TEMPLATE]
                                where SITE_ID = @siteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@siteId", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void UpdateVirtualContent(DbConnection sqlConnection, string newSqlQuery, int newContentId)
        {
            const string query = @"update [dbo].[content]
                                set [query] = @newSqlQuery
                            where [content_id] = @contentId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@newSqlQuery", newSqlQuery);
                cmd.Parameters.AddWithValue("@contentId", newContentId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsAttributes(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string newContentIds, bool isContentsVirtual)
        {
            if (string.IsNullOrEmpty(newContentIds))
            {
                return;
            }

            var excludeColumns = new List<string> { "attribute_id" };
            var changeValues = new Dictionary<string, string>
            {
                { "created", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) },
                { "modified", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-US")) }
            };

            var query = $@" set nocount on;
                if @new_content_ids is not null begin
                    declare @content_ids table
                    (
                        id numeric primary key
                    )

                    insert into @content_ids(id)
                    SELECT convert(numeric, nstr) from dbo.splitNew(@new_content_ids, ',')

                    select 1 as A into #disable_create_new_views

                    delete from content_attribute where content_id in (select id from @content_ids)

                    insert into content_attribute ({GetColumnsForTable(sqlConnection, "content_attribute", excludeColumns)})
                    select {GetColumnsForTable(sqlConnection, "content_attribute", new List<string> { "attribute_id", "content_id" }, changeValues, new Dictionary<string, string> { { "rbc.destination_content_id", string.Empty } }, "ATTRIBUTE_NAME")}
                    from [dbo].[content_attribute] as ca (nolock)
                    inner join (
                        select c.content_id as source_content_id, nc.content_id as destination_content_id
                        from [dbo].[content] as c (nolock)
                        inner join [dbo].[content] as nc (nolock)
                            on nc.content_name = c.content_name and nc.site_id = @destination_site_id
                        where c.site_id = @source_site_id and {(isContentsVirtual ? "c.virtual_type != 0" : "c.virtual_type = 0")}
                    ) as rbc on ca.CONTENT_ID = rbc.source_content_id and rbc.destination_content_id in (select id from @content_ids)

                    if exists(select * from sys.procedures where name = 'qp_content_new_views_recreate')
                    begin
                        declare @content_id numeric
                        while exists(select * from @content_ids)
                        begin
                            select @content_id = id from @content_ids

                            exec qp_content_new_views_recreate @content_id

                            delete from @content_ids where id = @content_id

                        end
                    end

                    drop table #disable_create_new_views

            end";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@source_site_id", sourceSiteId);
                cmd.Parameters.AddWithValue("@destination_site_id", destinationSiteId);
                cmd.Parameters.AddWithValue("@new_content_ids", newContentIds);
                cmd.Parameters.AddWithValue("@is_contents_virtual", isContentsVirtual);

                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyStyleFieldBind(DbConnection sqlConnection, string relationsBetweenAttributes)
        {
            var query = $@"
                                        declare @xmlprmsAttributes xml = '{relationsBetweenAttributes}'
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
                                                      on ra.source_attribute_id = csfb.[field_id]";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyCommandFieldBind(DbConnection sqlConnection, string relationsBetweenAttributes)
        {
            var query = $@"
                                        declare @xmlprmsAttributes xml = '{relationsBetweenAttributes}'
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
                                            on ra.source_attribute_id = csfb.[field_id]";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentConstrainRules(DbConnection sqlConnection, string relationsBetweenConstraints, string relationsBetweenAttributes)
        {
            var query = $@"

                                            declare @xmlprmsConstraints xml = '{relationsBetweenConstraints}'

                                            declare @relations_between_constraints table (
                                                source_constraint_id int,
                                                destination_constraint_id int
                                            )
                                            insert into @relations_between_constraints
                                                select doc.col.value('./@sourceId', 'int') source_constraint_id
                                                 ,doc.col.value('./@destinationId', 'int') destination_constraint_id
                                            from @xmlprmsConstraints.nodes('/items/item') doc(col)

                                            declare @xmlprmsAttributes xml = '{relationsBetweenAttributes}'

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
                                                  inner join @relations_between_constraints as rbc on rbc.source_constraint_id = ccr.constraint_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateAttributes(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenAttributesXml, string contentIds)
        {
            var inContents = string.Empty;
            if (!string.IsNullOrEmpty(contentIds))
            {
                inContents = $"and c.content_id in ({contentIds})";
            }
            var query = $@"
                                    declare @xmlprmsAttributes xml = '{relationsBetweenAttributesXml}'
                                        ;with relsattrs as (
                                            select doc.col.value('./@sourceId', 'int') attr_old
                                            ,doc.col.value('./@destinationId', 'int') attr_new
                                            from @xmlprmsAttributes.nodes('/items/item') doc(col)
                                        )
                                    update [dbo].[content_attribute]
                                    set     [related_attribute_id] = rai.attr_new
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
                                    where c.site_id = @destination_site_id {inContents}";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destination_site_id", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateAttributeLinkIdAndDefaultValue(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenLinksXml)
        {
            var query = $@"

                                    declare @xmlprmsLinks xml = '{relationsBetweenLinksXml}'

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
                                    ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentsParentContentId(DbConnection sqlConnection, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = $@" declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'

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
                                where c.SITE_ID = @destinationSiteId";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentAccess(DbConnection sqlConnection, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = $@"

                                delete FROM [dbo].[CONTENT_ACCESS]
                                where CONTENT_ID in (
                                    select c.CONTENT_ID from content as c (nolock)
                                    inner join [dbo].[content] as c1 (nolock) on c.CONTENT_ID = c1.CONTENT_ID and c.SITE_ID = @destinationSiteId
                                )

                                declare @now datetime = GETDATE()

                                declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'

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
                                    inner join relations_between_contents as rbc on ca.CONTENT_ID = rbc.source_content_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContainerStatuses(DbConnection sqlConnection, string relBetweenStatuses, string relBetweenObjects)
        {
            var query = $@"

                                    declare @xmlprmsObjects xml = '{relBetweenObjects}'

                                    declare @relations_between_objects table(
                                        source_object_id int,
                                        destination_object_id int
                                    )

                                    insert into @relations_between_objects
                                        select doc.col.value('./@sourceId', 'int') source_object_id
                                        ,doc.col.value('./@destinationId', 'int') destination_object_id
                                        from @xmlprmsObjects.nodes('/items/item') doc(col)

                                    declare @xmlprmsStatuses xml = '{relBetweenStatuses}'
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
                                    ";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentsCustomActions(DbConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = $@"
                                declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'

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
                                        on rbc.source_content_id = acb.CONTENT_ID";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void CopyContentFolders(DbConnection sqlConnection, string relationsBetweenContentsXml)
        {
            var query = $@"

                                declare @now datetime = GETDATE()

                                declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'

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
                                        on rbc.source_content_id = cf.content_id";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateContentFolders(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string relationsBetweenContentsXml)
        {
            var query = $@"
                                declare @xmlprmsContents xml = '{relationsBetweenContentsXml}'
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
                                where cf.content_id in (select destination_content_id from @relations_between_contents)";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@destinationSiteId", destinationSiteId);
                cmd.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> CopyContentLinks(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId)
        {
            const string query = @" set nocount on;

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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@oldSiteId", sourceSiteId);
                cmd.Parameters.AddWithValue("@newSiteId", destinationSiteId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> GetRelationsBetweenAttributes(DbConnection sqlConnection, int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
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
                sb.AppendFormat(byNewContents ? " where rbc.destination_content_id in ({0})" : " where rbc.source_content_id in ({0})", contentIds);
            }

            using (var cmd = DbCommandFactory.Create(sb.ToString(), sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int GetArticlesCountToCopy(DbConnection sqlConnection, int noMoreThanNArticles, int siteId)
        {
            const string query = @"declare @articlesCount int
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@noMoreThanNArticles", noMoreThanNArticles);
                cmd.Parameters.AddWithValue("@siteId", siteId);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static string GetContentIdsToCopy(DbConnection sqlConnection, int noMoreThanNArticles, int sourceSiteId)
        {
            const string query = @"   declare @ids varchar(max)
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

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@noMoreThanNArticles", noMoreThanNArticles);
                cmd.Parameters.AddWithValue("@siteId", sourceSiteId);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetContentIdsBySiteId(DbConnection sqlConnection, int sourceSiteId)
        {
            const string query = @"   declare @ids varchar(max)
                                        select @ids = COALESCE(@ids + ', ', '') + CAST(content_id as nvarchar) from (
                                        select distinct(ci.content_id) as content_id from content_item (nolock) as ci
                                        inner join content as c on c.CONTENT_ID = ci.CONTENT_ID and c.virtual_type = 0 and c.SITE_ID = @sourceSiteId
                                    ) as t
                                    select @ids";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@sourceSiteId", sourceSiteId);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetColumnsForTable(DbConnection sqlConnection, string tableName, List<string> excludeColumns, Dictionary<string, string> valuesToChange, Dictionary<string, string> fieldsToAdd, string insertBeforeField)
        {
            const string query = @"select COLUMN_NAME
                            from INFORMATION_SCHEMA.COLUMNS
                            where TABLE_NAME = @tableName";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@tableName", tableName);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                ExcludeColumns(excludeColumns, ref dt);
                ChangeValues(valuesToChange, ref dt);
                AddFields(insertBeforeField, fieldsToAdd, ref dt);

                return string.Join(", ", dt.AsEnumerable().Select(row => row.Field<string>("COLUMN_NAME").ToLower()));
            }
        }

        public static string GetColumnsForTable(DbConnection sqlConnection, string tableName, List<string> excludeColumns, Dictionary<string, string> valuesToChange) => GetColumnsForTable(sqlConnection, tableName, excludeColumns, valuesToChange, null, null);

        public static string GetColumnsForTable(DbConnection sqlConnection, string tableName, List<string> excludeColumns) => GetColumnsForTable(sqlConnection, tableName, excludeColumns, null, null, null);

        public static string GetColumnsForTable(DbConnection sqlConnection, string tableName) => GetColumnsForTable(sqlConnection, tableName, null, null, null, null);

        private static void ChangeValues(Dictionary<string, string> valuesToChange, ref DataTable dt)
        {
            if (valuesToChange == null || valuesToChange.Count == 0)
            {
                return;
            }

            foreach (var keyValue in valuesToChange)
            {
                var row = dt.AsEnumerable().FirstOrDefault(rw => string.Equals(keyValue.Key, rw.Field<string>("COLUMN_NAME"), StringComparison.CurrentCultureIgnoreCase));
                if (row != null)
                {
                    var rowIndex = dt.Rows.IndexOf(row);
                    if (keyValue.Value.IndexOf("{", StringComparison.Ordinal) > -1 && keyValue.Value.IndexOf("}", StringComparison.Ordinal) > -1)
                    {
                        dt.Rows[rowIndex]["COLUMN_NAME"] = $"{keyValue.Value.Replace("{", string.Empty).Replace("}", string.Empty)}";
                    }
                    else
                    {
                        dt.Rows[rowIndex]["COLUMN_NAME"] = $"'{keyValue.Value}' as [{keyValue.Key}]";
                    }
                }
            }
        }

        private static void ExcludeColumns(List<string> excludeColumns, ref DataTable dt)
        {
            if (excludeColumns == null || excludeColumns.Count == 0)
            {
                return;
            }

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
            {
                return;
            }

            foreach (var fieldToAdd in fieldsToAdd)
            {
                var row = dt.NewRow();
                if (!string.IsNullOrEmpty(fieldToAdd.Value))
                {
                    row["COLUMN_NAME"] = $"{fieldToAdd.Value} as {fieldToAdd.Key}";
                }
                else
                {
                    row["COLUMN_NAME"] = $"{fieldToAdd.Key}";
                }
                var insertAfterRow = dt.AsEnumerable().FirstOrDefault(rw => insertBeforeField.ToLower() == rw.Field<string>("COLUMN_NAME").ToLower());
                var insertBeforeRowIndex = dt.Rows.IndexOf(insertAfterRow);

                dt.Rows.InsertAt(row, insertBeforeRowIndex);
            }
        }

        public static void UpdateDefaultFormatId(DbConnection sqlConnection, int objectId, int formatId)
        {
            const string sql = "update [object] set object_format_id = @formatId where [object_id] = @objectId";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@formatId", formatId);
                cmd.Parameters.AddWithValue("@objectId", objectId);

                cmd.ExecuteNonQuery();
            }
        }

        public static string GetDbName(DbConnection sqlConnection)
        {
            var isPostgres = IsPostgresConnection(sqlConnection);
            var query = $"select {(isPostgres ? "current_database()" : "db_name()")} as name";
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetDbServerName(DbConnection sqlConnection)
        {
            var isPostgresConnection = IsPostgresConnection(sqlConnection);
            #warning разобраться, нужно ли в postgres и как получать, сейчас заглушка
            var query = $"select {(isPostgresConnection ? "'SERVER'" : @"@@SERVERNAME")} as server_name";

            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                return (string)cmd.ExecuteScalar();
            }
        }

        private static bool IsPostgresConnection(DbConnection connection) => connection is NpgsqlConnection;

        public static int[] SortIdsByFieldName(DbConnection sqlConnection, int[] ids, int contentId, string fieldName, bool isArchive)
        {
            var dbType = GetDbType(sqlConnection);
            var query = $@"
                select
                    CONTENT_ITEM_ID
                from
                    content_{contentId}_united a {WithNoLock(dbType)}
                    {(ids == null
                        ? string.Empty
                        : $"join {IdList(dbType, "@ids", "ids")} on a.CONTENT_ITEM_ID = ids.Id"
                    )}
                WHERE
                   {SqlQuerySyntaxHelper.CastToBool(dbType, "a.archive")} = @archive
                ORDER BY
                    a.{Escape(dbType, fieldName)}";


            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                if (ids != null)
                {
                    cmd.Parameters.Add(GetIdsDatatableParam("@ids", ids, dbType));
                }
                cmd.Parameters.AddWithValue("@archive", isArchive);
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

        public static IEnumerable<DataRow> MatchContents(DbConnection sqlConnection, int[] contentIds, XDocument fields)
        {
            using (var cmd = DbCommandFactory.Create(MatchContentsQuery, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@contentIds", SqlDbType.Structured) { TypeName = "Ids", Value = IdsToDataTable(contentIds) });
                cmd.Parameters.Add(new SqlParameter("@fieldsXml", SqlDbType.Xml) { Value = fields.ToString() });

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static IEnumerable<DataRow> MatchArticles(DbConnection sqlConnection, Dictionary<string, object> args, string query)
        {
            using (var cmd = DbCommandFactory.Create(query, sqlConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddRange(args.Select(param => new SqlParameter(param.Key, param.Value)).ToArray());

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }


        public static IList<int> GetParentIdsTreeResult(DbConnection cn, IList<int> ids, int fieldId, string fieldName, int? contentId)
        {
            var dbType = GetDbType(cn);
            if (string.IsNullOrWhiteSpace(fieldName) || !contentId.HasValue) return new List<int>();

            var query = $@"
                with {SqlQuerySyntaxHelper.RecursiveCte(dbType)} CTE(Id, ParentId, Lvl)
                        as
                        (
                            select
                                CONTENT_ITEM_ID Id,
                                {fieldName} ParentId,
                                0 Lvl
                            from
                                content_{contentId}_united c {WithNoLock(dbType)}
                            where
                                CONTENT_ITEM_ID IN (select id from {(dbType == DatabaseType.Postgres ? "unnest(@ids) i(id)" : "@ids")})
                            union all
                            select
                                c.CONTENT_ITEM_ID Id,
                                c.{fieldName},
                                r.Lvl + 1
                            from
                                content_{contentId}_united c {WithNoLock(dbType)}
                                join CTE r on c.CONTENT_ITEM_ID = r.ParentId
                        )
            ";
            query += ids.Count > 1
                ? "SELECT DISTINCT Id, ParentId FROM CTE"
                : "SELECT DISTINCT Id, ParentId, Lvl FROM CTE ORDER BY Lvl";

            return GetDatatableResult(cn, query, GetIdsDatatableParam("@ids", ids, dbType)).Select(dr => (int)dr.Field<decimal>(0)).ToList();
        }


        private static IList<DataRow> GetDatatableResult(DbConnection cn, string query, out int totalCount, params DbParameter[] @params)
        {
            using (var cmd = DbCommandFactory.Create(query, cn))
            {
                cmd.CommandType = CommandType.Text;
                if (@params != null && @params.Any())
                {
                    cmd.Parameters.AddRange(@params);
                }

                var ds = new DataSet();
                totalCount = DataAdapterFactory.Create(cmd).Fill(ds);

                return ds.Tables.Count > 0 ? ds.Tables[0].AsEnumerable().ToList() : Enumerable.Empty<DataRow>().ToList();
            }
        }

        private static IList<DataRow> GetDatatableResult(DbConnection cn, string query, params DbParameter[] @params)
        {
            int totalCount;
            return GetDatatableResult(cn, query, out totalCount, @params);
        }

        private static IList<DataRow> GetDatatableResult(DbConnection cn, StringBuilder queryBuilder, params DbParameter[] @params) => GetDatatableResult(cn, queryBuilder.ToString(), @params);

        public static bool NewPasswordMatchCurrentPassword(DbConnection connection, int userId, string password)
        {

            var dbType = GetDbType(connection);
            var q = $@"
            select
                case when {DbSchemaName(dbType)}.qp_get_hash(@password, {Escape(dbType, "salt")}) = {Escape(dbType, "hash")}
                    then {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}
                    else {SqlQuerySyntaxHelper.ToBoolSql(dbType, false)}
                end
            FROM {Escape(dbType, "USERS")}
            WHERE {Escape(dbType, "user_id")} = @userId
";
            // var query = @"declare @salt bigint, @hash binary(20), @old_hash binary(20)
            //               select @salt = salt, @old_hash = hash from users where USER_ID = @userId
            //               set @hash = dbo.qp_get_hash(@password, @salt)
            //               select case when @old_hash = @hash then 1 else 0 end as bit";
            using (var cmd = DbCommandFactory.Create(q, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@password", password);
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        public static int GetArticleIdForCollaborativePublication(DbConnection connection, int childId)
        {
            var sql = @"SELECT id FROM child_delays WHERE child_id = @childId";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@childId", childId);
                var result = cmd.ExecuteScalar();
                return result == null ? 0 : (int)(decimal)result;
            }
        }

        public static void ClearChildDelaysForChild(DbConnection connection, int childId)
        {
            var sql = @"DELETE FROM child_delays WHERE child_id = @childId";
            using (var cmd = DbCommandFactory.Create(sql, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@childId", childId);
                cmd.ExecuteNonQuery();
            }
        }


    }
}
