using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QP8.Infrastructure.Helpers;
using Quantumart.QPublishing.Database;

namespace QP8.Integration.Tests.Infrastructure
{
    internal static class Global
    {
        public static int SiteId => 35;

        public static string DbName => TestContext.Parameters.Get("qp8_test_ci_dbname", $"qp8_test_ci_{Environment.MachineName.ToLowerInvariant()}");

        public static string ConnectionString => $"Initial Catalog={DbName};Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";

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
            return localdbConnector.GetRealData($"select [{fieldName}] from content_{contentId}{asyncString} {idsString}")
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
            using (var cmd = new SqlCommand("delete from CONTENT_DATA where CONTENT_ITEM_ID = @id"))
            {
                cmd.Parameters.AddWithValue("@id", articleId);
                dbConnector.GetRealData(cmd);
            }
        }

        public static ContentDataItem[] GetContentData(DBConnector dbConnector, int articleId)
        {
            using (var cmd = new SqlCommand("select * from CONTENT_DATA where CONTENT_ITEM_ID = @id"))
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
