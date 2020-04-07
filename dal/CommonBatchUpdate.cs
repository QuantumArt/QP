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
    public class CommonBatchUpdate
    {
        #region BatchUpdate

        #region BatchInsertQuery

        private static string BatchInsertQuery = $@"
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

        public static IEnumerable<DataRow> BatchInsert(DbConnection sqlConnection, DataTable articles, bool visible, int userId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            using (var cmd = DbCommandFactory.Create(BatchInsertQuery, sqlConnection))
            {
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });
                }
                else
                {
                    cmd.CommandText = $@"select * from qp_batch_insert(@values, @visible, @userId);";
                    var xml = GetValuesDoc(articles).ToString();
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@values", xml, dbType));
                }
                cmd.Parameters.AddWithValue("@visible", visible ? 1 : 0);
                cmd.Parameters.AddWithValue("@userId", userId);

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        #region BatchUpdateQuery

        private static string PgBatchUpdateQuery = $@"

            WITH v AS (
                SELECT ArticleId, FieldId, ContentId, case when TextData <> '' then TextData else null end TextData
                FROM XMLTABLE('ITEMS/ITEM' passing @values COLUMNS
                    ArticleId int PATH '@id',
                    FieldId int PATH '@fieldId',
                    ContentId int PATH '@contentId',
                    TextData text PATH 'DATA'
                ) x
            )
            INSERT INTO CONTENT_DATA (ATTRIBUTE_ID, CONTENT_ITEM_ID, CREATED, MODIFIED, DATA, BLOB_DATA, not_for_replication)
            SELECT v.FieldId, v.ArticleId, now(), now(), v.TextData, null, True FROM v
            ON CONFLICT (ATTRIBUTE_ID, CONTENT_ITEM_ID) DO UPDATE
            SET DATA = EXCLUDED.DATA, BLOB_DATA = null";

        private static string SqlBatchUpdateQuery = $@"
            INSERT INTO
                CONTENT_DATA (ATTRIBUTE_ID, CONTENT_ITEM_ID, CREATED, MODIFIED, [DATA], BLOB_DATA, not_for_replication)
            SELECT
                v.FieldId ATTRIBUTE_ID,
                v.ArticleId CONTENT_ITEM_ID,
                GETDATE() CREATED,
                GETDATE() MODIFIED,
                CASE
                    WHEN t.DATABASE_TYPE != 'NTEXT' AND v.Value != '' THEN v.Value
                    ELSE NULL
                END [DATA],
                CASE
                    WHEN t.DATABASE_TYPE = 'NTEXT' AND v.Value != '' THEN v.Value
                    ELSE NULL
                END [BLOB_DATA],
                1 not_for_replication
            FROM
                CONTENT_DATA d
                RIGHT JOIN @values v ON v.ArticleId = d.CONTENT_ITEM_ID AND v.FieldId = d.ATTRIBUTE_ID
                JOIN CONTENT_ATTRIBUTE a ON v.FieldId = a.ATTRIBUTE_ID AND v.ContentId = a.CONTENT_ID
                JOIN ATTRIBUTE_TYPE t ON a.ATTRIBUTE_TYPE_ID = t.ATTRIBUTE_TYPE_ID
            WHERE
                d.ATTRIBUTE_ID IS NULL

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

        public static void UpdateNotForReplication(DbConnection sqlConnection, int[] ids, int userId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var setTrue = dbType == DatabaseType.SqlServer ? "1" : "true";
            var sql = $@"
            UPDATE CONTENT_ITEM SET NOT_FOR_REPLICATION = {setTrue}, MODIFIED = {SqlQuerySyntaxHelper.Now(dbType)}, LAST_MODIFIED_BY = @userId
            WHERE {SqlQuerySyntaxHelper.IsFalse(dbType, "NOT_FOR_REPLICATION")} AND CONTENT_ITEM_ID IN (select id from {Common.IdList(dbType, "@ids")});
            ";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", ids, dbType));
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void BatchUpdate(DbConnection sqlConnection,  DataTable articles, int userId)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = dbType == DatabaseType.SqlServer ? SqlBatchUpdateQuery : PgBatchUpdateQuery;
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });
                }
                else
                {
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@values", GetValuesDoc(articles).ToString(), dbType));
                }
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        #region GetRelationsQuery

        private static string GetRelationsQuery(DatabaseType dbType)
        {
            var textType = dbType == DatabaseType.SqlServer ? "nvarchar(max)" : "text";
            return $@"
            SELECT
                f.ArticleId ""ArticleId"",
                a.CONTENT_ID ""ContentId"",
                a.ATTRIBUTE_ID ""FieldId"",
                a.ATTRIBUTE_NAME ""FieldName"",
                f.Value ""FieldValue"",
                CASE
                    WHEN otm.CONTENT_ID IS NOT NULL THEN a.CONTENT_ID
                    WHEN mto.CONTENT_ID IS NOT NULL THEN mto.CONTENT_ID
                    WHEN mtm.r_content_id IS NOT NULL THEN mtm.r_content_id
                    WHEN classifier.CONTENT_ID IS NOT NULL THEN classifier.CONTENT_ID
                    ELSE NULL
                END ""RefContentId"",
                CASE
                    WHEN otm.CONTENT_ID IS NOT NULL THEN a.ATTRIBUTE_ID
                    WHEN mto.CONTENT_ID IS NOT NULL THEN mto.ATTRIBUTE_ID
                    WHEN classifier.CONTENT_ID IS NOT NULL THEN classifier.ATTRIBUTE_ID
                    ELSE NULL
                END ""RefFieldId"",
                CASE
                    WHEN mtm.link_id IS NOT NULL THEN mtm.link_id
                    ELSE NULL
                END ""LinkId""
            FROM
                {ValuesTable(dbType)} f
                JOIN CONTENT_ATTRIBUTE a ON f.FieldId = a.ATTRIBUTE_ID AND f.ContentId = a.CONTENT_ID
                LEFT JOIN CONTENT_ATTRIBUTE otm ON a.RELATED_ATTRIBUTE_ID = otm.ATTRIBUTE_ID
                LEFT JOIN CONTENT_ATTRIBUTE mto ON a.BACK_RELATED_ATTRIBUTE_ID = mto.ATTRIBUTE_ID
                LEFT JOIN CONTENT_TO_CONTENT mtm ON a.LINK_ID = mtm.LINK_ID AND a.CONTENT_ID = mtm.l_content_id
                LEFT JOIN CONTENT_ATTRIBUTE classifier ON a.ATTRIBUTE_ID = classifier.CLASSIFIER_ATTRIBUTE_ID AND CAST(classifier.CONTENT_ID AS {textType}) = f.Value
            WHERE
                otm.CONTENT_ID IS NOT NULL OR
                mto.CONTENT_ID IS NOT NULL OR
                mtm.r_content_id IS NOT NULL OR
                classifier.CONTENT_ID IS NOT NULL";
        }

        #endregion

        public static string ValuesTable(DatabaseType dbType) => dbType == DatabaseType.SqlServer ? "@values" : $@"
            XMLTABLE('ITEMS/ITEM' passing @values COLUMNS
                ArticleId int PATH '@id',
                FieldId int PATH '@fieldId',
                ContentId int PATH '@contentId',
                Value text PATH 'DATA'
            )";

        public static IEnumerable<DataRow> GetRelations(DbConnection sqlConnection, DataTable articles)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            using (var cmd = DbCommandFactory.Create(GetRelationsQuery(dbType), sqlConnection))
            {
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.Parameters.Add(new SqlParameter("@values", SqlDbType.Structured) { TypeName = "Values", Value = articles });
                }
                else
                {
                    var xml = GetValuesDoc(articles).ToString();
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@values", xml, dbType));
                }

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);
                return dt.AsEnumerable().ToArray();
            }
        }

        public static XDocument GetValuesDoc(DataTable articles)
        {
            var dataDoc = new XDocument();
            dataDoc.Add(new XElement("ITEMS"));

            foreach (DataRow row in articles.Rows)
            {
                    var elem = new XElement("ITEM");
                    elem.Add(new XAttribute("id", row["ArticleId"].ToString()));
                    elem.Add(new XAttribute("fieldId", row["FieldId"].ToString()));
                    elem.Add(new XAttribute("contentId", row["ContentId"].ToString()));
                    elem.Add(new XElement("DATA", row["Value"].ToString()));
                    dataDoc.Root?.Add(elem);
            }

            return dataDoc;
        }

        public static void ReplicateItems(DbConnection sqlConnection, int[] articleIds, int[] fieldIds)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var sql = (dbType == DatabaseType.SqlServer) ? "qp_replicate_items" : "call qp_replicate_items(@ids, @attr_ids);";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                if (dbType == DatabaseType.SqlServer)
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ids", SqlDbType.NVarChar, -1) { Value = string.Join(",", articleIds) });
                    cmd.Parameters.Add(new SqlParameter("@attr_ids", SqlDbType.NVarChar, -1) { Value = string.Join(",", fieldIds) });

                }
                else
                {
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@ids", articleIds, dbType));
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetIdsDatatableParam("@attr_ids", fieldIds, dbType));
                }

                cmd.Parameters.AddWithValue("@modification_update_interval", -1);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        public static DataTable GetFieldTypes(DbConnection cnn, int[] ids)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(cnn);
            var textType = dbType == DatabaseType.SqlServer ? "nvarchar(max)" : "text";
            var text = $@"select attribute_id, BACK_RELATED_ATTRIBUTE_ID, attribute_type_id, link_id, is_classifier,
                cast(case when coalesce(cast(enum_values as {textType}), '') <> '' then 1 else 0 end as bit) as is_string_enum
                from content_attribute where attribute_id in (select id from {Common.IdList(dbType, "@ids")})";

            using (var cmd = DbCommandFactory.Create(text, cnn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(Common.GetIdsDatatableParam("@ids", ids, dbType));

                var dt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(dt);

                return dt;
            }
        }
    }
}
