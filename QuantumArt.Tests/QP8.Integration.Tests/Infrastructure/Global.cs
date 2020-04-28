using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc;
using Quantumart.QPublishing.Database;
using M = QP.ConfigurationService.Models;

namespace QP8.Integration.Tests.Infrastructure
{
    internal static class Global
    {
        public static int SiteId => 35;

        public static WebApplicationFactory<Startup> Factory { get; set; }

        public static string ConnectionString
        {
            get
            {
                var basePart = $"Database={EnvHelpers.DbNameToRunTests};Server={EnvHelpers.DbServerToRunTests};Application Name=UnitTest;";
                if (!String.IsNullOrEmpty(EnvHelpers.PgDbLoginToRunTests))
                {
                    return $"{basePart}User Id={EnvHelpers.PgDbLoginToRunTests};Password={EnvHelpers.PgDbPasswordToRunTests}";
                }

                if (!String.IsNullOrEmpty(EnvHelpers.SqlDbLoginToRunTests))
                {
                    return $"{basePart}User Id={EnvHelpers.SqlDbLoginToRunTests};Password={EnvHelpers.SqlDbPasswordToRunTests}";
                }

                return $"{basePart}Integrated Security=True;Connection Timeout=600";
            }
        }

        public static DatabaseType DbType => !String.IsNullOrEmpty(EnvHelpers.PgDbLoginToRunTests) ? DatabaseType.Postgres : DatabaseType.SqlServer;

        public static M.DatabaseType ClientDbType => (M.DatabaseType)(int)DbType;

        public static QpConnectionInfo ConnectionInfo => new QpConnectionInfo(ConnectionString, DbType);


        public static string GetXml(string fileName) => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, fileName));

        public static int[] GetIds(DBConnector dbConnector, int contentId) => dbConnector.GetRealData($"select content_item_id from content_{contentId}_united")
            .Select()
            .Select(row => Convert.ToInt32(row["content_item_id"]))
            .OrderBy(row => row)
            .ToArray();

        public static Dictionary<string, int> GetIdsWithTitles(DBConnector dbConnector, int contentId) => dbConnector.GetRealData($"select content_item_id, Title from content_{contentId}_united")
            .Select()
            .Select(row => new { Id = Convert.ToInt32(row["content_item_id"]), Title = Convert.ToString(row["Title"]) })
            .ToDictionary(row => row.Title, row => row.Id);

        public static T[] GetFieldValues<T>(DBConnector localdbConnector, int contentId, string fieldName, int[] ids = null, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : string.Empty;
            var field = DbType == DatabaseType.SqlServer ? $@"[{fieldName}]" : $@"""{fieldName.ToLowerInvariant()}""";
            return localdbConnector.GetRealData($"select {field} from content_{contentId}{asyncString} {idsString}")
                .Select()
                .Select(row => ConvertHelpers.ChangeType<T>(row[fieldName]))
                .ToArray();
        }

        public static int GetContentId(DBConnector dbConnector, string contentName) => dbConnector.GetRealData($"select content_id from content where site_id = {SiteId} and content_name = '{contentName}'")
            .Select()
            .Select(row => Convert.ToInt32(row["content_id"]))
            .SingleOrDefault();

        public static int GetFieldId(DBConnector dbConnector, string contentName, string fieldName) => dbConnector.FieldID(SiteId, contentName, fieldName);

        public static void ClearContentData(DBConnector dbConnector, int articleId)
        {
            using (var cmd = dbConnector.CreateDbCommand("delete from CONTENT_DATA where CONTENT_ITEM_ID = @id"))
            {
                cmd.Parameters.AddWithValue("@id", articleId);
                dbConnector.ProcessData(cmd);
            }
        }

        public static ContentDataItem[] GetContentData(DBConnector dbConnector, int articleId)
        {
            using (var cmd = dbConnector.CreateDbCommand("select * from CONTENT_DATA where CONTENT_ITEM_ID = @id"))
            {
                cmd.Parameters.AddWithValue("@id", articleId);
                return dbConnector.GetRealData(cmd)
                    .Select()
                    .Select(row => new ContentDataItem
                    {
                        FieldId = Convert.ToInt32(row["ATTRIBUTE_ID"]),
                        Data = Convert.ToString(row["DATA"]),
                        BlobData = Convert.ToString(row["BLOB_DATA"])
                    }).ToArray();
            }
        }
    }
}
