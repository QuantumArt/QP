using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Quantumart.QPublishing.Database;

namespace QP8.Integration.Tests
{
    internal partial class Global
    {
        public static int SiteId => 35;

        public static string GetXml(string fileName) => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, fileName));

        public static string DbName => TestContext.Parameters.Get("qp8_test_ci_dbname", $"qp8_test_ci_{Environment.MachineName.ToLowerInvariant()}");

        public static string ConnectionString => $"Initial Catalog={DbName};Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";
        
        public static int[] GetIds(DBConnector dbConnector, int contentId) => dbConnector.GetRealData($"select content_item_id from content_{contentId}_united")
            .AsEnumerable()
            .Select(n => (int)n.Field<decimal>("content_item_id"))
            .OrderBy(n => n)
            .ToArray();

        public static int[] GetIdsFromArchive(DBConnector dbConnector, int[] ids)
        {
            return dbConnector.GetRealData($"select content_item_id from content_item where archive = 1 and content_item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("content_item_id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static Dictionary<string, int> GetIdsWithTitles(DBConnector dbConnector, int contentId) => dbConnector.GetRealData($"select content_item_id, Title from content_{contentId}_united")
            .AsEnumerable()
            .Select(n => new { Id = (int)n.Field<decimal>("content_item_id"), Title = n.Field<string>("Title") })
            .ToDictionary(n => n.Title, n => n.Id);

        public static DateTime[] GetModified(DBConnector dbConnector, int contentId) => dbConnector.GetRealData($"select Modified from content_{contentId}_united")
            .AsEnumerable()
            .Select(n => n.Field<DateTime>("Modified"))
            .ToArray();

        public static int CountLinks(DBConnector dbConnector, int[] ids, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return dbConnector.GetRealData($"select count(*) as cnt from item_link{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int CountEfLinks(DBConnector dbConnector, int[] ids, int contentId, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return dbConnector.GetRealData($"select count(*) as cnt from item_link_{contentId}{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int[] GetLinks(DBConnector dbConnector, int[] ids, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return dbConnector.GetRealData($"select linked_item_id as id from item_link{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static int[] GetEfLinks(DBConnector dbConnector, int[] ids, int contentId, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return dbConnector.GetRealData($"select linked_item_id as id from item_link{contentId}{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static bool EfLinksExists(DBConnector dbConnector, int contentId)
        {
            using (var cmd = new SqlCommand($"select cast(count(null) as bit) cnt from sysobjects where xtype='u' and name='item_link_{contentId}'"))
            {
                return (bool)dbConnector.GetRealScalarData(cmd);
            }
        }

        public static int CountData(DBConnector dbConnector, int[] ids) => dbConnector.GetRealData($"select count(*) as cnt from content_data where content_item_id in ({string.Join(",", ids)})")
            .AsEnumerable()
            .Select(n => n.Field<int>("cnt"))
            .Single();

        public static int CountVersionData(DBConnector dbConnector, int[] ids) => dbConnector.GetRealData($"select count(*) as cnt from version_content_data where content_item_version_id in ({string.Join(",", ids)})")
            .AsEnumerable()
            .Select(n => n.Field<int>("cnt"))
            .Single();

        public static int CountVersionLinks(DBConnector dbConnector, int[] ids) => dbConnector.GetRealData($"select count(*) as cnt from item_to_item_version where content_item_version_id in ({string.Join(",", ids)})")
            .AsEnumerable()
            .Select(n => n.Field<int>("cnt"))
            .Single();

        public static int CountArticles(DBConnector dbConnector, int contentId, int[] ids = null, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : string.Empty;
            return dbConnector.GetRealData($"select count(*) as cnt from content_{contentId}{asyncString} {idsString}")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static string[] GetTitles(DBConnector localdbConnector, int contentId, int[] ids = null, bool isAsync = false) => GetFieldValues<string>(localdbConnector, contentId, "Title", ids, isAsync);

        public static T[] GetFieldValues<T>(DBConnector localdbConnector, int contentId, string fieldName, int[] ids = null, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : string.Empty;
            return localdbConnector.GetRealData($"select [{fieldName}] from content_{contentId}{asyncString} {idsString}")
                .AsEnumerable()
                .Select(n => n.Field<T>(fieldName))
                .ToArray();
        }

        public static decimal[] GetNumbers(DBConnector localdbConnector, int contentId, int[] ids = null, bool isAsync = false) => GetFieldValues<decimal?>(localdbConnector, contentId, "Number", ids, isAsync)
            .Select(n => n ?? 0)
            .ToArray();

        public static int[] GetMaxVersions(DBConnector localdbConnector, int[] ids) => localdbConnector.GetRealData($"select max(content_item_version_id) as id from content_item_version where content_item_id in ({string.Join(",", ids)}) group by content_item_id")
            .AsEnumerable()
            .Select(n => (int)n.Field<decimal>("id"))
            .ToArray();

        public static int GetContentId(DBConnector dbConnector, string contentName) => dbConnector.GetRealData($"select content_id from content where site_id = {SiteId} and content_name = '{contentName}'")
            .AsEnumerable()
            .Select(n => (int)n.Field<decimal>("content_id"))
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
                    .AsEnumerable()
                    .Select(n => new ContentDataItem
                    {
                        FieldId = (int)n.Field<decimal>("ATTRIBUTE_ID"),
                        Data = n.Field<string>("DATA"),
                        BlobData = n.Field<string>("BLOB_DATA")
                    }).ToArray();
            }
        }
    }
}
