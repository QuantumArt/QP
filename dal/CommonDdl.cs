using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL
{
    public static partial class Common
    {
        public static void AddColumn(QPModelDataContext ctx, DbConnection cnn, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var field = ctx.FieldSet.Include(n => n.Type).First(n => n.Id == id);
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

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));

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

        public static void AddIndex(DbConnection cnn, string tableName, string fieldName)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var actualFieldName = (dbType == DatabaseType.Postgres) ? fieldName.ToLower() : fieldName;
            var indexName = $@"{DbSchemaName(dbType)}.{tableName}_{actualFieldName}";
            var sql =  $@" create index {Escape(dbType, indexName)} on {tableName}({Escape(dbType, actualFieldName)})";
            ExecuteSql(cnn, sql);
        }

        public static void DropIndex(DbConnection cnn, string tableName, string fieldName)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var actualFieldName = (dbType == DatabaseType.Postgres) ? fieldName.ToLower() : fieldName;
            var indexName = $@"{DbSchemaName(dbType)}.{tableName}_{actualFieldName}";
            var tablePrefix = (dbType == DatabaseType.SqlServer) ? $@"{DbSchemaName(dbType)}.{tableName}." : "";
            var sql =  $@"
            drop index {tablePrefix}{Escape(dbType, indexName)}
            ";
            ExecuteSql(cnn, sql);
        }

        public static void DropColumn(QPModelDataContext ctx, DbConnection cnn, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var field = ctx.FieldSet.Include(n => n.Type).First(n => n.Id == id);
            var tableName = "content_" + field.ContentId;
            var asyncTableName = tableName + "_async";

            var sql = $"alter table {{0}} drop column {Escape(dbType, field.Name)}";

            if (field.IndexFlag == 1)
            {
                DropIndex(cnn, tableName, field.Name);
                DropIndex(cnn, asyncTableName, field.Name);
            }

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));

        }


        public static void RenameColumn(DbConnection cnn, string tableName, string fieldName, string newFieldName, bool preserveIndex)
        {
            if (preserveIndex)
            {
                DropIndex(cnn, tableName, fieldName);
            }

            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var sql = "";
            if (dbType == DatabaseType.SqlServer)
            {
                sql = $@"exec sp_rename '{tableName}.{fieldName}', '{newFieldName}', 'column' ";
            }
            else
            {
                sql = $@"ALTER TABLE {tableName} RENAME COLUMN {Escape(dbType, fieldName)} TO {Escape(dbType, newFieldName)};";
            }

            ExecuteSql(cnn, sql);

            if (preserveIndex)
            {
                AddIndex(cnn, tableName, fieldName);
            }
        }

        public static void CreateContent(DbConnection cnn, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var tableName = "content_" + id;
            var asyncTableName = tableName + "_async";
            var dtType = (dbType == DatabaseType.Postgres) ? "timestamp without time zone" : "datetime";
            var sql = $@"
            create table {DbSchemaName(dbType)}.{{0}} (
                {Escape(dbType, "CONTENT_ITEM_ID")} numeric NOT NULL PRIMARY KEY,
                {Escape(dbType, "STATUS_TYPE_ID")} numeric NOT NULL,
                {Escape(dbType, "VISIBLE")} numeric NOT NULL,
                {Escape(dbType, "ARCHIVE")} numeric NOT NULL,
                {Escape(dbType, "CREATED")} {dtType} NOT NULL DEFAULT {Now(dbType)},
                {Escape(dbType, "MODIFIED")} {dtType} NOT NULL DEFAULT {Now(dbType)},
                {Escape(dbType, "LAST_MODIFIED_BY")} numeric NOT NULL
             )";

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));

        }

        public static void DropContent(DbConnection cnn, int id)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var tableName = "content_" + id;
            var asyncTableName = tableName + "_async";
            var sql = $@"drop table {DbSchemaName(dbType)}.{{0}}";

            ExecuteSql(cnn, String.Format(sql, tableName));
            ExecuteSql(cnn, String.Format(sql, asyncTableName));
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

        public static int RemovingActions_RemoveContentItems(int contentId, int itemsToDelete, DbConnection connection)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
            string disableTrigger = (dbType == DatabaseType.SqlServer) ? "select 1 as A into #disable_td_delete_item_o2m_nullify;" : "";
            string query = $@"{disableTrigger}
                    delete FROM CONTENT_ITEM WHERE CONTENT_ITEM_ID in
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
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(connection);
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


    }
}
