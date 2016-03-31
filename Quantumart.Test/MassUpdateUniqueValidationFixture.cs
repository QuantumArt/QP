using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

        public static int[] BaseArticlesIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(GlobalSettings.ConnectionString, 1, true);
            service.ReplayXml(GlobalSettings.GetXml(@"xmls\unique.xml"));
            Cnn = new DBConnector(GlobalSettings.ConnectionString);
            ContentId = GlobalSettings.GetContentId(Cnn, "Test unique");
            BaseArticlesIds = GlobalSettings.GetIds(Cnn, ContentId);
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
            var id = (decimal)Cnn.GetRealScalarData(new SqlCommand($"select content_item_id from content_{ContentId}_united where [Title] <> 'Name2'"));
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = id.ToString(CultureInfo.InvariantCulture),
                ["Title"] = "Name2",
                ["Number"] = "9,5",
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1), 
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("for content articles"),
                "Duplicate of test data should violate rules");
        }

        [Test]
        public void MassUpdate_UpdateNothing_InCaseOfAnyError()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = "Name5"
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["Title"] = "Name5",
            };
            values.Add(article2);

            Assert.That(() => { Cnn.MassUpdate(ContentId, values, 1); }, Throws.Exception);

            var titles = GlobalSettings.GetTitles(Cnn, ContentId);

            Assert.That(titles, Does.Not.Contain("Name5"), "In case of any error the internal transaction should be rolled back");
        }

        [Test]
        public void MassUpdateWithExternalTransaction_UpdateNothing_InCaseOfAnyError()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = "Name5",
                ["Number"] = "10"
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["Title"] = "Name5",
            };
            values.Add(article2);

            var values2 = new List<Dictionary<string, string>>();
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0"
            };
            values2.Add(article3);

            using (var conn = new SqlConnection(GlobalSettings.ConnectionString))
            {
                conn.Open();
                var tr = conn.BeginTransaction();
                DBConnector localCnn = new DBConnector(conn, tr);

                Assert.DoesNotThrow(() => localCnn.MassUpdate(ContentId, values, 1));
                Assert.That(() => { localCnn.MassUpdate(ContentId, values2, 1); }, Throws.Exception);

                tr.Rollback();

                var titles = GlobalSettings.GetTitles(localCnn, ContentId);

                Assert.That(titles, Does.Not.Contain("Name5"), "In case of any error the external transaction should be rolled back");
            }

        }

        [Test]
        public void ValidateConstraint_IsValid_SameData()
        {
            var values = new List<Dictionary<string, string>>();
            var first = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second = ContentItem.Read(BaseArticlesIds[1], Cnn);

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = first.FieldValues["Title"].Data,
                ["Number"] = first.FieldValues["Number"].Data
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["Title"] = second.FieldValues["Title"].Data,
                ["Number"] = second.FieldValues["Number"].Data
            };
            values.Add(article2);

            var modified = GlobalSettings.GetModified(Cnn, ContentId);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Update existing data");

            var modified2 = GlobalSettings.GetModified(Cnn, ContentId);
            var first2 = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second2 = ContentItem.Read(BaseArticlesIds[1], Cnn);

            Assert.That(modified, Is.Not.EqualTo(modified2), "Modification dates should be changed");
            Assert.That(first2.FieldValues["Title"].Data, Is.EqualTo(first.FieldValues["Title"].Data), "Data should remain the same");
            Assert.That(second2.FieldValues["Title"].Data, Is.EqualTo(second.FieldValues["Title"].Data), "Data should remain the same");

        }


        [Test]
        public void ValidateConstraint_IsValid_SwapData()
        {
            var values = new List<Dictionary<string, string>>();
            var first = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second = ContentItem.Read(BaseArticlesIds[1], Cnn);

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = second.FieldValues["Title"].Data,
                ["Number"] = second.FieldValues["Number"].Data
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["Title"] = first.FieldValues["Title"].Data,
                ["Number"] = first.FieldValues["Number"].Data

            };
            values.Add(article2);

            var modified = GlobalSettings.GetModified(Cnn, ContentId);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Swap existing data");

            var modified2 = GlobalSettings.GetModified(Cnn, ContentId);
            var first2 = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second2 = ContentItem.Read(BaseArticlesIds[1], Cnn);

            Assert.That(modified, Is.Not.EqualTo(modified2), "Modification dates should be changed");
            Assert.That(first2.FieldValues["Title"].Data, Is.EqualTo(second.FieldValues["Title"].Data), "Data should be swapped");
            Assert.That(second2.FieldValues["Title"].Data, Is.EqualTo(first.FieldValues["Title"].Data), "Data should be swapped");
        }


        [Test]
        public void ValidateAttributeValue_ThrowsException_MissedData()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0"
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1), 
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("is required"),
                "Validate required fields"
            );
        }

        [Test]
        public void ValidateAttributeValue_ThrowsException_StringSizeExceeded()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = new string('*', 1000)
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("too long"),
                "Validate string size"
            );
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
