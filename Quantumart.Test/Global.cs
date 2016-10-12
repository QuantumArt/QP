using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Quantumart.QPublishing.Database;

namespace Quantumart.Test
{
    internal class Global
    {
        public static string ConnectionString = @"Initial Catalog=mts_catalog;Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";

        public static string GetXml(string fileName)
        {
            return File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, fileName));
        }

        public static int SiteId => 35;

        public static int[] GetIds(DBConnector cnn, int contentId)
        {
            return cnn.GetRealData($"select content_item_id from content_{contentId}_united")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("content_item_id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static int[] GetIdsFromArchive(DBConnector cnn, int[] ids)
        {
            return cnn.GetRealData($"select content_item_id from content_item where archive = 1 and content_item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("content_item_id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static Dictionary<string, int> GetIdsWithTitles(DBConnector cnn, int contentId)
        {
            return cnn.GetRealData($"select content_item_id, Title from content_{contentId}_united")
                .AsEnumerable()
                .Select(n => new { Id = (int)n.Field<decimal>("content_item_id"), Title = n.Field<string>("Title") })
                .ToDictionary(n => n.Title, n => n.Id);
        }

        public static DateTime[] GetModified(DBConnector cnn, int contentId)
        {
            return cnn.GetRealData($"select Modified from content_{contentId}_united")
                .AsEnumerable()
                .Select(n => n.Field<DateTime>("Modified"))
                .ToArray();
        }

        public static int CountLinks(DBConnector cnn, int[] ids, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return cnn.GetRealData(
                $"select count(*) as cnt from item_link{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int CountEFLinks(DBConnector cnn, int[] ids, int contentId, bool isAsync = false)
        {      
            var asyncString = isAsync ? "_async" : string.Empty;
            return cnn.GetRealData(
                $"select count(*) as cnt from item_link_{contentId}{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int[] GetLinks(DBConnector cnn, int[] ids, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return cnn.GetRealData(
                $"select linked_item_id as id from item_link{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static int[] GetEFLinks(DBConnector cnn, int[] ids, int contentId, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : string.Empty;
            return cnn.GetRealData(
                $"select linked_item_id as id from item_link{contentId}{asyncString} where item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("id"))
                .OrderBy(n => n)
                .ToArray();
        }

        public static bool EFLinksExists(DBConnector cnn, int contentId)
        {
            var cmd = new SqlCommand($"select cast(count(null) as bit) cnt from sysobjects where xtype='u' and name='item_link_{contentId}'");
            return (bool)cnn.GetRealScalarData(cmd);
        }

        public static int CountData(DBConnector cnn, int[] ids)
        {
            return cnn.GetRealData(
                $"select count(*) as cnt from content_data where content_item_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int CountVersionData(DBConnector cnn, int[] ids)
        {
            return cnn.GetRealData(
                $"select count(*) as cnt from version_content_data where content_item_version_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int CountVersionLinks(DBConnector cnn, int[] ids)
        {
            return cnn.GetRealData(
                $"select count(*) as cnt from item_to_item_version where content_item_version_id in ({string.Join(",", ids)})")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static int CountArticles(DBConnector cnn, int contentId, int[] ids = null, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : "";
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : "";
            return cnn.GetRealData(
                $"select count(*) as cnt from content_{contentId}{asyncString} {idsString}")
                .AsEnumerable()
                .Select(n => n.Field<int>("cnt"))
                .Single();
        }

        public static string[] GetTitles(DBConnector localCnn, int contentId, int[] ids = null, bool isAsync = false)
        {
            return GetFieldValues<string>(localCnn, contentId, "Title", ids, isAsync);
        }

        public static T[] GetFieldValues<T>(DBConnector localCnn, int contentId, string fieldName, int[] ids = null, bool isAsync = false)
        {
            var asyncString = isAsync ? "_async" : "";
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : "";
            return localCnn.GetRealData($"select [{fieldName}] from content_{contentId}{asyncString} {idsString}")
                .AsEnumerable()
                .Select(n => n.Field<T>(fieldName))
                .ToArray();
        }

        public static decimal[] GetNumbers(DBConnector localCnn, int contentId, int[] ids = null, bool isAsync = false)
        {
            return GetFieldValues<decimal?>(localCnn, contentId, "Number", ids, isAsync)
                .Select(n => n ?? 0)
                .ToArray();
        }

        public static int[] GetMaxVersions(DBConnector localCnn, int[] ids)
        {
            return localCnn.GetRealData($"select max(content_item_version_id) as id from content_item_version where content_item_id in ({string.Join(",", ids)}) group by content_item_id")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("id"))
                .ToArray();
        }

        public static int GetContentId(DBConnector cnn, string contentName)
        {
            return cnn.GetRealData(
                $"select content_id from content where site_id = { SiteId } and content_name = '{contentName}'")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("content_id"))
                .SingleOrDefault();
        }

        public static int GetFieldId(DBConnector cnn, string contentName, string fieldName)
        {
            return cnn.FieldID(SiteId, contentName, fieldName);
        }

        public static void ClearContentData(DBConnector cnn, int articleId)
        {
            const string query = "delete from CONTENT_DATA where CONTENT_ITEM_ID = @id";
            var cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@id", articleId);

            cnn.GetRealData(cmd);
        }

        public static ContentDataItem[] GetContentData(DBConnector cnn, int articleId)
        {
            const string query = "select * from CONTENT_DATA where CONTENT_ITEM_ID = @id";
            var cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@id", articleId);

            return cnn.GetRealData(cmd)
              .AsEnumerable()
              .Select(n => new ContentDataItem
              {
                  FieldId = (int)n.Field<decimal>("ATTRIBUTE_ID"),
                  Data = n.Field<string>("DATA"),
                  BlobData = n.Field<string>("BLOB_DATA")
              })
              .ToArray();
        }

        public class ContentDataItem
        {
            public int FieldId { get; set; }

            public string Data { get; set; }

            public string BlobData { get; set; }

            public override string ToString()
            {
                return new { FieldId, Data, BlobData }.ToString();
            }
        }
    }
}
