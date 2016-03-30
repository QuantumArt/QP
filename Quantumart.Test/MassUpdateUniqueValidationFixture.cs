using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using NUnit.Framework;

namespace Quantumart.Test
{
    [TestFixture]
    public class MassUpdateUniqueValidationFixture
    {
        public static DBConnector Cnn { get; private set; }

        public static int ContentId { get; private set; }

        public static Dictionary<int, DateTime> BaseArticles { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(GlobalSettings.ConnectionString, 1, true);
            service.ReplayXml(GlobalSettings.GetXml(@"xmls\unique.xml"));
            Cnn = new DBConnector(GlobalSettings.ConnectionString);
            ContentId = Cnn.GetContentId(GlobalSettings.SiteId, "Test unique");
            BaseArticles = Cnn.GetRealData($"select content_item_id, modified from content_{ContentId}_united")
                .AsEnumerable()
                .ToDictionary(n => (int) n.Field<decimal>("content_item_id"), m => m.Field<DateTime>("modified"));
            BaseArticlesIds = BaseArticles.Keys.ToArray();
        }


        [Test]
        public void SelfValidate_DoesntThrowException_ForNonExistingFields()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["abc"] = "Name3"
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["abc"] = "Name3",
            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Non-existing fields shouldn't violate rules");
        }

        [Test]
        public void SelfValidate_ThrowsException_ForDataConflict()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "Name3"
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "Name3",
            };
            values.Add(article2);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1), 
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("between articles being added/updated"), 
                "Field Title should violate rules"
            );
        }

        [Test]
        public void ValidateConstraint_ThrowsException_ForDataConflict()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = "Name2",
                ["Number"] = "9,5",
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1), 
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("for content articles"),
                "Duplicate of test data should violate rules");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            var srv = new ContentService(GlobalSettings.ConnectionString, 1);
            srv.Delete(ContentId);
            QPContext.UseConnectionString = false;
        }
    }
}
