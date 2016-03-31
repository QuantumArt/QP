using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Quantumart.QPublishing.Database;

namespace Quantumart.Test
{
    internal class GlobalSettings
    {
        public static string ConnectionString =
            @"Initial Catalog=mts_catalog;Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";

        public static string GetXml(string fileName)
        {
            var path = TestContext.CurrentContext.TestDirectory;
            return File.ReadAllText(Path.Combine(path, fileName));
        }

        public static int SiteId => 35;

        public static int[] GetIds(DBConnector cnn, int contentId)
        {
            return cnn.GetRealData($"select content_item_id from content_{contentId}_united")
                .AsEnumerable()
                .Select(n => (int) n.Field<decimal>("content_item_id"))
                .ToArray();
        }


        public static DateTime[] GetModified(DBConnector cnn, int contentId)
        {
            return cnn.GetRealData($"select Modified from content_{contentId}_united")
                .AsEnumerable()
                .Select(n => n.Field<DateTime>("Modified"))
                .ToArray();
        }

        public static int CountLinks(DBConnector cnn, int[] ids, bool isAsync)
        {
            var asyncString = isAsync ? "_async" : "";
            return cnn.GetRealData(
                $"select count(*) as cnt from item_link{asyncString} where item_id in ({string.Join(",", ids)})")
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
            var asyncString = isAsync ? "_async" : "";
            var idsString = ids != null ? $"where content_item_id in ({string.Join(",", ids)})" : "";
            ;
            return localCnn.GetRealData($"select Title from content_{contentId}{asyncString} {idsString}")
                .AsEnumerable()
                .Select(n => n.Field<string>("Title"))
                .ToArray();
        }

        public static int GetContentId(DBConnector cnn, string contentName)
        {
            return cnn.GetRealData(
                $"select content_id from content where site_id = { SiteId } and content_name = '{contentName}'")
                .AsEnumerable()
                .Select(n => (int)n.Field<decimal>("content_id"))
                .Single();
        }
    }
}
