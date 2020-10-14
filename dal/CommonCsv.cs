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
    public class CommonCsv
    {

        private static string PgTableValuesBlock => $@"
            WITH new AS
            (
                select x.content_item_id, x.attribute_id,
                case when text_data <> '' then text_data else null end ""data"",
                case when blob_data <> '' then blob_data else null end blob_data
                from XMLTABLE('PARAMETERS/FIELDVALUE' passing @xmlParameter COLUMNS
                    content_item_id int PATH 'CONTENT_ITEM_ID',
                    attribute_id int PATH 'ATTRIBUTE_ID',
                    text_data text PATH 'DATA',
                    blob_data text PATH 'BLOB_DATA'
                ) x
            )
        ";

        public static void ValidateO2MValues(DbConnection sqlConnection, string xmlParameter, string message)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            var tableDefintionBlock = dbType == DatabaseType.SqlServer ? $@"
                DECLARE @NewArticles TABLE (CONTENT_ITEM_ID int, ATTRIBUTE_ID int, DATA nvarchar(3500), BLOB_DATA nvarchar(max))
                INSERT INTO @NewArticles
                    SELECT
                     doc.col.value('(CONTENT_ITEM_ID)[1]', 'int') CONTENT_ITEM_ID
                    ,doc.col.value('(ATTRIBUTE_ID)[1]', 'int') ATTRIBUTE_ID
                    ,doc.col.value('(DATA)[1]', 'nvarchar(3500)') DATA
                    ,doc.col.value('(BLOB_DATA)[1]', 'nvarchar(max)') BLOB_DATA
                    FROM @xmlParameter.nodes('/PARAMETERS/FIELDVALUE') doc(col)
            " : PgTableValuesBlock;

            var tableUsing = dbType == DatabaseType.SqlServer ? "@NewArticles a" : "new a";

            string sql = $@"{tableDefintionBlock}
                select * from
                (
                    select a.*, ca.ATTRIBUTE_NAME, rca.CONTENT_ID as RELATED_CONTENT_ID from {tableUsing}
                    inner join CONTENT_ATTRIBUTE ca on a.ATTRIBUTE_ID = ca.ATTRIBUTE_ID
                    inner join CONTENT_ATTRIBUTE rca on ca.RELATED_ATTRIBUTE_ID = rca.ATTRIBUTE_ID and ca.CONTENT_ID <> rca.CONTENT_ID
                    inner join CONTENT rc on rc.CONTENT_ID = rca.CONTENT_ID and rc.VIRTUAL_TYPE <> 3
                    where a.data != ''
                ) as a
                left join CONTENT_ITEM ci on ci.CONTENT_ITEM_ID = cast(a.data as numeric)
                where ci.CONTENT_ID is null or ci.CONTENT_ID <> a.RELATED_CONTENT_ID";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xmlParameter", xmlParameter, dbType));
                var resultDt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(resultDt);
                if (resultDt.AsEnumerable().Any())
                {
                    var dr = resultDt.Rows[0];
                    var title = dr.Field<string>("ATTRIBUTE_NAME");
                    var data = dr.Field<string>("DATA");
                    var id = dr.Field<int>(FieldName.ContentItemId).ToString();
                    throw new ArgumentException(string.Format(message, id, title, data));
                }
            }
        }

        public static List<int> InsertArticleIds(DbConnection sqlConnection, string xml, bool withGuids)
        {
            var result = new List<int>();
            var withGuidsStr = withGuids ? ", UNIQUE_ID" : "";
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            string baseInsert = $@"INSERT INTO CONTENT_ITEM (VISIBLE, STATUS_TYPE_ID, CONTENT_ID, LAST_MODIFIED_BY {withGuidsStr}) ";
            string baseSelect = $@"SELECT VISIBLE, STATUS_TYPE_ID, CONTENT_ID, LAST_MODIFIED_BY {withGuidsStr} ";
            string sql = dbType == DatabaseType.SqlServer ? $@"
                    DECLARE @NewArticles TABLE (CONTENT_ID int, STATUS_TYPE_ID int, VISIBLE int, LAST_MODIFIED_BY int, UNIQUE_ID uniqueidentifier)
                    DECLARE @NewIds TABLE ([ID] INT);

                    INSERT INTO @NewArticles
                    SELECT
                        doc.col.value('./@contentId', 'int') CONTENT_ID
                        ,doc.col.value('./@statusId', 'int') STATUS_TYPE_ID
                        ,doc.col.value('./@visible', 'int') VISIBLE
                        ,doc.col.value('./@userId', 'int') LAST_MODIFIED_BY
                        ,doc.col.value('./@guid', 'uniqueidentifier') UNIQUE_ID
                        FROM @xml.nodes('ITEMS/ITEM') doc(col)

                    {baseInsert}
                    OUTPUT inserted.CONTENT_ITEM_ID INTO @NewIds
                    {baseSelect}
                    from @NewArticles
                    SELECT ID FROM @NewIds;
                " : $@"
                WITH inserted(id) AS
                (
                    {baseInsert}
                    {baseSelect}
                    from XMLTABLE('ITEMS/ITEM' passing @xml COLUMNS
                        CONTENT_ID int PATH '@contentId',
                        STATUS_TYPE_ID int PATH '@statusId',
                        VISIBLE int PATH '@visible',
                        LAST_MODIFIED_BY int PATH '@userId',
                        UNIQUE_ID uuid PATH '@guid'
                    ) x returning CONTENT_ITEM_ID
                ) select id from inserted;";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xml", xml, dbType));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id;
                        if (int.TryParse(reader.GetValue(0).ToString(), out id))
                        {
                            result.Add(id);
                        }
                    }
                }
                return result;
            }
        }

        public static void UpdateArticleGuids(DbConnection sqlConnection, List<Tuple<int, Guid>> guidsByIdToUpdate)
        {
            if (guidsByIdToUpdate.Any())
            {
                var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
                var doc = new XDocument();
                doc.Add(new XElement("ITEMS"));
                foreach (var tuple in guidsByIdToUpdate)
                {
                    var elem = new XElement("ITEM");
                    elem.Add(new XAttribute("id", tuple.Item1));
                    elem.Add(new XAttribute("guid", tuple.Item2));
                    doc.Root?.Add(elem);
                }

                string sql = dbType == DatabaseType.SqlServer ? $@"
                    DECLARE @NewIds TABLE (CONTENT_ITEM_ID int, UNIQUE_ID uniqueidentifier)

                    INSERT INTO @NewIds
                    SELECT
                        doc.col.value('./@id', 'int') CONTENT_ITEM_ID
                        ,doc.col.value('./@guid', 'uniqueidentifier') UNIQUE_ID
                        FROM @xml.nodes('ITEMS/ITEM') doc(col)

                    UPDATE CONTENT_ITEM SET UNIQUE_ID = i.UNIQUE_ID FROM CONTENT_ITEM ci INNER JOIN @NewIds i
                    on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                " : $@"
                WITH new AS
                (
                    select x.* from XMLTABLE('ITEMS/ITEM' passing @xml COLUMNS
                        content_item_id int PATH '@id',
                        unique_id uuid PATH '@guid'
                    ) x
                )
                update content_item ci set unique_id = new.unique_id from new
                where ci.content_item_id = new.content_item_id;";


                using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
                {
                    cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xml", doc.ToString(), dbType));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void InsertArticleValues(DbConnection sqlConnection, string xmlParameter)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            string sql = dbType == DatabaseType.SqlServer ? "qp_insertArticleValues" : PgTableValuesBlock + $@"
                update content_data cd set data = coalesce(new.blob_data, new.data) from new
                where cd.content_item_id = new.content_item_id and cd.attribute_id = new.attribute_id;
            ";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xmlParameter", xmlParameter, dbType));
                var resultDt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(resultDt);
            }
        }

        public static void InsertO2MFieldValues(DbConnection sqlConnection, string xmlParameter)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            string sql = dbType == DatabaseType.SqlServer ? "qp_update_o2mfieldvalues" : $@"
                WITH new AS
                (
                    select x.* from XMLTABLE('ITEMS/ITEM' passing @xmlParameter COLUMNS
                        content_item_id int PATH '@id',
                        linked_id int PATH '@linked_id',
                        attribute_id int PATH '@field_id'
                    ) x
                )
                update content_data cd set data = new.linked_id from new
                where cd.content_item_id = new.CONTENT_ITEM_ID and cd.attribute_id = new.attribute_id;
                ";
            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xmlParameter", xmlParameter, dbType));
                var resultDt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(resultDt);
            }
        }

        public static void UpdateArticlesDateTime(DbConnection sqlConnection, string xmlParameter)
        {
            var dbType = DatabaseTypeHelper.ResolveDatabaseType(sqlConnection);
            string sql = dbType == DatabaseType.SqlServer ? "qp_update_acrticle_modification_date" : $@"
                WITH new AS
                (
                    select x.* from XMLTABLE('ITEMS/ITEM' passing @xmlParameter COLUMNS
                        content_item_id int PATH '@id',
                        last_modified_by int PATH '@modifiedBy'
                    ) x
                )
                update content_item ci set modified = now(), last_modified_by = new.last_modified_by from new
                where ci.content_item_id = new.content_item_id;
                ";

            using (var cmd = DbCommandFactory.Create(sql, sqlConnection))
            {
                cmd.CommandType = dbType == DatabaseType.SqlServer ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.Add(SqlQuerySyntaxHelper.GetXmlParameter("@xmlParameter", xmlParameter, dbType));
                var resultDt = new DataTable();
                DataAdapterFactory.Create(cmd).Fill(resultDt);
            }
        }
    }
}
