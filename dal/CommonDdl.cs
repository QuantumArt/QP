using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {
        public static void AddColumn(DbConnection cnn, FieldDAL field)
        {
            var dbType = GetDbType(cnn);
            var tableName = "content_" + field.ContentId;
            var asyncTableName = tableName + "_async";
            var columnType = (dbType == DatabaseType.SqlServer) ? field.Type.DatabaseType : PgColumnType(field.Type.DatabaseType);
            var fieldDef = $@"{Escape(dbType, field.Name)} {columnType}";
            if (field.Type.DatabaseType == "NVARCHAR")
            {
                fieldDef += $"({field.Size})";
            }
            else if (field.Type.DatabaseType == "NUMERIC")
            {
                var firstSize = (field.Type.Name == FieldTypeName.Numeric) ? 38 : 18;
                fieldDef += $"({firstSize}, {field.Size})";
            }

            var sql = $"alter table {{0}} add {fieldDef}";

            var contentId = (int)field.ContentId;
            DropContentViews(cnn, contentId);
            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));
            CreateContentViews(cnn, contentId);

            if (field.IndexFlag == 1)
            {
                AddIndex(cnn, tableName, field.Name);
                AddIndex(cnn, asyncTableName, field.Name);
            }
        }

        private static string PgColumnType(string type)
        {
            switch (type)
            {
                case "NUMERIC":
                    return "numeric";
                case "DATETIME":
                    return "timestamp without time zone";
                case "NVARCHAR":
                    return "character varying";
                default:
                    return "text";
            }
        }

        public static string IndexName(DatabaseType dbType, string tableName, string fieldName)
        {
            var indexName = $@"{DbSchemaName(dbType)}.{tableName}_{fieldName}";
            return Escape(dbType, indexName);
        }

        public static void AddIndex(DbConnection cnn, string tableName, string fieldName)
        {
            var dbType = GetDbType(cnn);
            var actualFieldName = (dbType == DatabaseType.Postgres) ? fieldName.ToLower() : fieldName;
            var indexName = IndexName(dbType, tableName, actualFieldName);
            var sql =  $@" create index {indexName} on {tableName}({Escape(dbType, actualFieldName)})";
            ExecuteSql(cnn, sql);
        }

        public static void DropIndex(DbConnection cnn, string tableName, string fieldName)
        {
            var dbType = GetDbType(cnn);
            var actualFieldName = (dbType == DatabaseType.Postgres) ? fieldName.ToLower() : fieldName;
            var indexName = IndexName(dbType, tableName, actualFieldName);
            var tablePrefix = (dbType == DatabaseType.SqlServer) ? $@"{DbSchemaName(dbType)}.{tableName}." : "";
            var sql = $@"
            drop index {tablePrefix}{indexName}
            ";
            ExecuteSql(cnn, sql);
        }

        public static void DropColumn(DbConnection cnn, FieldDAL field)
        {
            var dbType = GetDbType(cnn);
            var tableName = "content_" + field.ContentId;
            var asyncTableName = tableName + "_async";

            var sql = $"alter table {{0}} drop column {Escape(dbType, field.Name)}";

            if (field.IndexFlag == 1)
            {
                DropIndex(cnn, tableName, field.Name);
                DropIndex(cnn, asyncTableName, field.Name);
            }

            var contentId = (int)field.ContentId;
            DropContentViews(cnn, contentId);
            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));
            CreateContentViews(cnn, contentId);

        }


        public static void CreateContentTables(DbConnection cnn, int id)
        {
            var dbType = GetDbType(cnn);
            var tableName = "content_" + id;
            var asyncTableName = tableName + "_async";
            var dtType = (dbType == DatabaseType.Postgres) ? "timestamp without time zone" : "datetime";
            var sql = $@"
            create table {DbSchemaName(dbType)}.{{0}} (
                {Escape(dbType, "CONTENT_ITEM_ID")} numeric(18,0) NOT NULL PRIMARY KEY,
                {Escape(dbType, "STATUS_TYPE_ID")} numeric(18,0) NOT NULL,
                {Escape(dbType, "VISIBLE")} numeric(18,0) NOT NULL,
                {Escape(dbType, "ARCHIVE")} numeric(18,0) NOT NULL,
                {Escape(dbType, "CREATED")} {dtType} NOT NULL DEFAULT {Now(dbType)},
                {Escape(dbType, "MODIFIED")} {dtType} NOT NULL DEFAULT {Now(dbType)},
                {Escape(dbType, "LAST_MODIFIED_BY")} numeric(18,0) NOT NULL
             )";

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));
        }

        public static void CreateContentViews(DbConnection cnn, int id, bool withUnited = true)
        {
            var dbType = GetDbType(cnn);
            var idStr = id.ToString();

            if (withUnited)
            {
                var unitedSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_united_view_create", idStr);
                ExecuteSql(cnn, unitedSql);
            }

            var feSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_frontend_views_create", idStr);
            ExecuteSql(cnn, feSql);

            var newSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_new_views_create", idStr);
            ExecuteSql(cnn, newSql);

        }

        public static void DropContentTables(DbConnection cnn, int id)
        {
            var dbType = GetDbType(cnn);
            var tableName = "content_" + id;
            var asyncTableName = tableName + "_async";
            var sql = $@"drop table if exists {DbSchemaName(dbType)}.{{0}}";

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));
        }

        public static void DropContentViews(DbConnection cnn, int id)
        {
            var dbType = GetDbType(cnn);
            var idStr = id.ToString();

            var newSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_new_views_drop", idStr);
            ExecuteSql(cnn, newSql);

            var feSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_frontend_views_drop", idStr);
            ExecuteSql(cnn, feSql);

            var unitedSql = SqlQuerySyntaxHelper.SpCall(dbType, "qp_content_united_view_drop", idStr);
            ExecuteSql(cnn, unitedSql);
        }

        public static IEnumerable<DataRow> RemovingActions_GetContentsItemInfo(int? siteId, int? contentId, DbConnection connection)
        {
            var query = $@"select S.SITE_ID, S.SITE_NAME, C.CONTENT_ID, C.CONTENT_NAME, cast(COALESCE(I.ITEMS_COUNT, 0) as int) AS ITEMS_COUNT from
                (select  content_id, count(CONTENT_ITEM_ID) ITEMS_COUNT from content_item group by content_id) I
                RIGHT JOIN CONTENT C ON C.CONTENT_ID = I.CONTENT_ID
                JOIN SITE S ON S.SITE_ID = C.SITE_ID
                where (c.content_id = @content_id OR @content_id is null)
                and (s.site_id = @site_id OR @site_id is null)";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", (object)contentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@site_id", (object)siteId ?? DBNull.Value );
                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static int RemovingActions_RemoveSiteContentItems(int siteId, int itemsToDelete, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            string disableTrigger = (dbType == DatabaseType.SqlServer) ? $@"
                select 1 as A into #disable_td_delete_item_o2m_nullify;
                select 1 as A into #disable_td_item_to_item;
            " : "";
            string query = $@"{disableTrigger}
                    delete FROM CONTENT_ITEM WHERE CONTENT_ITEM_ID in (
                        select {Top(dbType, itemsToDelete)} CONTENT_ITEM_ID from CONTENT_ITEM ci
                        inner join CONTENT c on c.content_id = ci.content_id
                        where c.site_id = @site_id order by CONTENT_ITEM_ID {Limit(dbType, itemsToDelete)}
                    );";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@site_id", siteId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static int RemovingActions_RemoveContentItems(int contentId, int itemsToDelete, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            string disableTrigger = (dbType == DatabaseType.SqlServer) ? "select 1 as A into #disable_td_delete_item_o2m_nullify;" : "";
            string query = $@"{disableTrigger}
                    delete FROM CONTENT_ITEM WHERE CONTENT_ITEM_ID in (
                        select {Top(dbType, itemsToDelete)} CONTENT_ITEM_ID from CONTENT_ITEM
                        where CONTENT_ID = @content_id order by CONTENT_ITEM_ID {Limit(dbType, itemsToDelete)}
                    );";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@content_id", contentId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static int RemovingActions_ClearO2MRelations(int contentId, DbConnection connection)
        {
            var dbType = GetDbType(connection);
            if (dbType != DatabaseType.SqlServer)
            {
                return 0;
            }
            using (var cmd = DbCommandFactory.Create("qp_clear_relations", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@parent_id", contentId);
                return cmd.ExecuteNonQuery();
            }
        }

        public static void DropView(string viewName, DbConnection connection)
        {
            var sql = "";
            var dbType = GetDbType(connection);
            if (dbType == DatabaseType.SqlServer)
            {
                sql = $"exec qp_drop_existing '{viewName}', 'IsView'";
            }
            else
            {
                sql = $"DROP VIEW IF EXISTS {viewName}";
            }

            ExecuteSql(connection, sql);
        }

        public static void UpdateColumn(DbConnection connection, FieldDAL oldField, FieldDAL newField)
        {
            var tableName = "content_" + newField.ContentId;
            var asyncTableName = tableName + "_async";
            if (oldField.IndexFlag == 1 && newField.IndexFlag == 0)
            {
                DropIndex(connection, tableName, oldField.Name);
                DropIndex(connection, asyncTableName, oldField.Name);
            }

            if (oldField.Name != newField.Name)
            {
                DropContentViews(connection, (int)newField.ContentId);
                RenameColumn(connection, tableName, oldField.Name, newField.Name);
                RenameColumn(connection, asyncTableName, oldField.Name, newField.Name);
                CreateContentViews(connection, (int)newField.ContentId);;

                if (oldField.IndexFlag == 1 && newField.IndexFlag == 1)
                {
                    RenameIndex(connection, tableName, oldField.Name, newField.Name);
                    RenameIndex(connection, asyncTableName, oldField.Name, newField.Name);
                }
            }

            if (oldField.IndexFlag == 0 && newField.IndexFlag == 1)
            {
                AddIndex(connection, tableName, newField.Name);
                AddIndex(connection, asyncTableName, newField.Name);
            }
        }

        public static void RenameColumn(DbConnection connection, string tableName, string oldFieldName, string newFieldName)
        {
            var dbType = GetDbType(connection);
            var sql = "";
            if (dbType == DatabaseType.SqlServer)
            {
                sql = $"exec sp_rename '{tableName}.{oldFieldName}', '{newFieldName}', 'column' ";
            }
            else
            {
                sql = $"alter table {tableName} rename {Escape(dbType, oldFieldName)} to {Escape(dbType, newFieldName)}";
            }

            ExecuteSql(connection, sql);
        }

        public static void RenameIndex(DbConnection connection, string tableName, string oldFieldName, string newFieldName)
        {
            var dbType = GetDbType(connection);
            var oldIndexName = IndexName(dbType, tableName, oldFieldName);
            var newIndexName = IndexName(dbType, tableName, newFieldName);
            var sql = "";

            if (dbType == DatabaseType.SqlServer)
            {
                sql = $"exec sp_rename '{tableName}.{oldIndexName}', '{newIndexName}', 'index' ";
            }
            else
            {
                sql = $"ALTER INDEX {oldIndexName} RENAME TO {newIndexName}";
            }

            ExecuteSql(connection, sql);
        }

        public static void CreateContentModification(DbConnection connection, int id)
        {
            var dbType = GetDbType(connection);
            var sql = $@"INSERT INTO CONTENT_MODIFICATION SELECT {id}, {Now(dbType)}, {Now(dbType)}";
            ExecuteSql(connection, sql);

        }

        public static void CreateLinkTables(DbConnection connection, ContentToContentDAL item)
        {
            var dbType = GetDbType(connection);
            if (dbType != DatabaseType.SqlServer)
            {
                var tableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}";
                var revTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_rev";
                var asyncTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_async";
                var asyncRevTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_async_rev";
                var sql = $@"CREATE TABLE {{0}} (id int NOT NULL, linked_id int NOT NULL, PRIMARY KEY (id, linked_id))";

                ExecuteSql(connection, String.Format(sql, tableName));
                ExecuteSql(connection, String.Format(sql, revTableName));
                ExecuteSql(connection, String.Format(sql, asyncTableName));
                ExecuteSql(connection, String.Format(sql, asyncRevTableName));
            }
        }

        public static void DropLinkTables(DbConnection connection, ContentToContentDAL item)
        {
            var dbType = GetDbType(connection);
            var tableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}";
            var revTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_rev";
            var asyncTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_async";
            var asyncRevTableName = $@"{DbSchemaName(dbType)}.item_link_{item.LinkId}_async_rev";
            var sql = $@"DROP TABLE {{0}}";

            ExecuteSql(connection, String.Format(sql, tableName));
            ExecuteSql(connection, String.Format(sql, revTableName));
            ExecuteSql(connection, String.Format(sql, asyncTableName));
            ExecuteSql(connection, String.Format(sql, asyncRevTableName));
        }

        public static void CreateLinkView(DbConnection connection, ContentToContentDAL item)
        {
            var dbType = GetDbType(connection);
            var spName = dbType == DatabaseType.SqlServer ? "qp_build_link_view" : "qp_link_view_create";
            var sql = SqlQuerySyntaxHelper.SpCall(dbType, spName , item.LinkId.ToString(CultureInfo.InvariantCulture));
            ExecuteSql(connection, sql);
        }

        public static void DropLinkView(DbConnection connection, ContentToContentDAL item)
        {
            var dbType = GetDbType(connection);
            var spName = dbType == DatabaseType.SqlServer ? "qp_drop_link_view" : "qp_link_view_drop";
            var sql = SqlQuerySyntaxHelper.SpCall(dbType, spName, item.LinkId.ToString(CultureInfo.InvariantCulture));
            ExecuteSql(connection, sql);
        }

        public static void PostgresUpdateSequence(DbConnection connection, string tableName, string keyName)
        {
            var sql = $@"
                LOCK TABLE {tableName} IN EXCLUSIVE MODE;
                SELECT setval('{tableName}_seq', cast(COALESCE((SELECT MAX({keyName})+1 FROM {tableName}), 1) as int), false);
            ";
            ExecuteSql(connection, sql);
        }

    }
}
