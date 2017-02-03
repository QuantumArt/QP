using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class UniqueValidationFixture
    {
        public static DBConnector Cnn { get; private set; }

        public static int ContentId { get; private set; }

        public static string ContentName { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            Cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            ContentName = "Test unique";
            Clear();

            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            service.Process(Global.GetXml(@"xmls\unique.xml"));
            ContentId = Global.GetContentId(Cnn, ContentName);
            BaseArticlesIds = Global.GetIds(Cnn, ContentId);
        }

        [Test]
        public void MassUpdate_DoesntThrowException_SelfValidateForNonExistingFields()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["abc"] = "Name3"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                ["abc"] = "Name3"
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Non-existing fields shouldn't violate rules");
        }

        [Test]
        public void MassUpdate_ThrowsException_SelfValidateForDataConflict()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "Name3"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "Name3"
            };

            values.Add(article2);
            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("between articles being added/updated"),
                "Field Title should violate rules"
            );
        }

        [Test]
        public void MassUpdate_ThrowsException_ValidateConstraintForDataConflict()
        {
            var values = new List<Dictionary<string, string>>();
            var id = (decimal)Cnn.GetRealScalarData(new SqlCommand($"select content_item_id from content_{ContentId}_united where [Title] <> 'Name2'"));
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = id.ToString(CultureInfo.InvariantCulture),
                ["Title"] = "Name2",
                ["Number"] = "9,5"
            };

            values.Add(article1);
            Assert.That(() => Cnn.MassUpdate(ContentId, values, 1), Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("for content articles"), "Duplicate of test data should violate rules");
        }

        [Test]
        public void AddFormToContent_ThrowsException_ValidateConstraintForDataConflict()
        {
            var titleName = Cnn.FieldName(Global.SiteId, ContentName, "Title");
            var numberName = Cnn.FieldName(Global.SiteId, ContentName, "Number");
            var id = (int)(decimal)Cnn.GetRealScalarData(new SqlCommand($"select content_item_id from content_{ContentId}_united where [Title] <> 'Name2'"));
            var article1 = new Hashtable
            {
                [titleName] = "Name2",
                [numberName] = "9,5"
            };

            Assert.That(() => { id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("Unique constraint violation"), "Duplicate of test data should violate rules");
        }

        [Test]
        public void MassUpdate_UpdateNothing_InCaseOfAnyError()
        {
            var maxId = (decimal)Cnn.GetRealScalarData(new SqlCommand("select max(content_item_id) from content_item") { CommandType = CommandType.Text });
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["Title"] = "Name5"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                ["Title"] = "Name5"
            };

            values.Add(article2);
            Assert.That(() => { Cnn.MassUpdate(ContentId, values, 1); }, Throws.Exception);

            var maxIdAfter =
            (decimal)Cnn.GetRealScalarData(new SqlCommand("select max(content_item_id) from content_item")
            {
                CommandType = CommandType.Text
            });

            var titles = Global.GetTitles(Cnn, ContentId);
            Assert.That(titles, Does.Not.Contain("Name5"), "In case of any error the internal transaction should be rolled back");
            Assert.That(maxId, Is.EqualTo(maxIdAfter), "No new content items");
        }

        [Test]
        public void MassUpdate_UpdateNothing_InCaseOfAnyErrorAndExternalTransaction()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["Title"] = "Name5",
                ["Number"] = "10"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                ["Title"] = "Name5"
            };

            values.Add(article2);
            var values2 = new List<Dictionary<string, string>>();
            var article3 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0"
            };

            values2.Add(article3);
            using (var conn = new SqlConnection(Global.ConnectionString))
            {
                conn.Open();
                var tr = conn.BeginTransaction();
                var localCnn = new DBConnector(conn, tr);

                Assert.DoesNotThrow(() => localCnn.MassUpdate(ContentId, values, 1));
                Assert.That(() => { localCnn.MassUpdate(ContentId, values2, 1); }, Throws.Exception);

                tr.Rollback();
                var titles = Global.GetTitles(localCnn, ContentId);
                Assert.That(titles, Does.Not.Contain("Name5"), "In case of any error the external transaction should be rolled back");
            }
        }

        [Test]
        public void AddFormToContent_UpdateNothing_InCaseOfAnyErrorAndExternalTransaction()
        {
            var titlesBefore = Global.GetTitles(Cnn, ContentId);
            Assert.That(titlesBefore, Does.Not.Contain("Name5"), "correct state");

            var titleName = Cnn.FieldName(Global.SiteId, ContentName, "Title");
            var numberName = Cnn.FieldName(Global.SiteId, ContentName, "Number");
            var article1 = new Hashtable
            {
                [titleName] = "Name5",
                [numberName] = "10"
            };

            var article3 = new Hashtable
            {
                [FieldName.ContentItemId] = "0"
            };

            using (var conn = new SqlConnection(Global.ConnectionString))
            {
                conn.Open();
                var tr = conn.BeginTransaction();
                var localCnn = new DBConnector(conn, tr);

                Assert.DoesNotThrow(() =>
                {
                    localCnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]);
                }, "Update existing data");

                Assert.That(() =>
                {
                    localCnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article3, 0);
                }, Throws.Exception, "Invalid data");

                tr.Rollback();
                var titles = Global.GetTitles(localCnn, ContentId);
                Assert.That(titles, Does.Not.Contain("Name5"), "In case of any error the external transaction should be rolled back");
            }
        }

        [Test]
        public void MassUpdate_IsValid_ValidateConstraintSameData()
        {
            var values = new List<Dictionary<string, string>>();
            var first = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second = ContentItem.Read(BaseArticlesIds[1], Cnn);
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["Title"] = first.FieldValues["Title"].Data,
                ["Number"] = first.FieldValues["Number"].Data
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                ["Title"] = second.FieldValues["Title"].Data,
                ["Number"] = second.FieldValues["Number"].Data
            };

            values.Add(article2);
            var modified = Global.GetModified(Cnn, ContentId);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Update existing data");

            var modified2 = Global.GetModified(Cnn, ContentId);
            var first2 = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second2 = ContentItem.Read(BaseArticlesIds[1], Cnn);

            Assert.That(modified, Is.Not.EqualTo(modified2), "Modification dates should be changed");
            Assert.That(first2.FieldValues["Title"].Data, Is.EqualTo(first.FieldValues["Title"].Data), "Data should remain the same");
            Assert.That(second2.FieldValues["Title"].Data, Is.EqualTo(second.FieldValues["Title"].Data), "Data should remain the same");
        }

        [Test]
        public void AddFormToContent_IsValid_ValidateConstraintSameData()
        {
            var first = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var titleName = Cnn.FieldName(Global.SiteId, ContentName, "Title");
            var numberName = Cnn.FieldName(Global.SiteId, ContentName, "Number");
            var article1 = new Hashtable
            {
                [titleName] = first.FieldValues["Title"].Data,
                [numberName] = first.FieldValues["Number"].Data
            };

            var modified = Global.GetModified(Cnn, ContentId);
            Assert.DoesNotThrow(() =>
            {
                Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]);
            }, "Update existing data");

            var modified2 = Global.GetModified(Cnn, ContentId);
            var first2 = ContentItem.Read(BaseArticlesIds[0], Cnn);

            Assert.That(modified, Is.Not.EqualTo(modified2), "Modification dates should be changed");
            Assert.That(first2.FieldValues["Title"].Data, Is.EqualTo(first.FieldValues["Title"].Data), "Data should remain the same");

        }

        [Test]
        public void MassUpdate_IsValid_ValidateConstraintSwapData()
        {
            var values = new List<Dictionary<string, string>>();
            var first = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second = ContentItem.Read(BaseArticlesIds[1], Cnn);
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["Title"] = second.FieldValues["Title"].Data,
                ["Number"] = second.FieldValues["Number"].Data
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                ["Title"] = first.FieldValues["Title"].Data,
                ["Number"] = first.FieldValues["Number"].Data

            };

            values.Add(article2);
            var modified = Global.GetModified(Cnn, ContentId);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Swap existing data");

            var modified2 = Global.GetModified(Cnn, ContentId);
            var first2 = ContentItem.Read(BaseArticlesIds[0], Cnn);
            var second2 = ContentItem.Read(BaseArticlesIds[1], Cnn);

            Assert.That(modified, Is.Not.EqualTo(modified2), "Modification dates should be changed");
            Assert.That(first2.FieldValues["Title"].Data, Is.EqualTo(second.FieldValues["Title"].Data), "Data should be swapped");
            Assert.That(second2.FieldValues["Title"].Data, Is.EqualTo(first.FieldValues["Title"].Data), "Data should be swapped");
        }

        [Test]
        public void MassUpdate_ThrowsException_ValidateAttributeValueMissedData()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0"
            };

            values.Add(article1);
            Assert.That(() => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("is required"),
                "Validate required fields"
            );
        }

        [Test]
        public void AddFormToContent_ThrowsException_ValidateAttributeValueMissedData()
        {
            var article1 = new Hashtable
            {
                [FieldName.ContentItemId] = "0"
            };

            Assert.That(() =>
            {
                Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]);
            }, Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("is required"), "Validate required fields");
        }

        [Test]
        public void MassUpdate_ThrowsException_ValidateAttributeValueStringSizeExceeded()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                ["Title"] = new string('*', 1000)
            };

            values.Add(article1);
            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("too long"),
                "Validate string size"
            );
        }

        [Test]
        public void AddFormToContent_ThrowsException_ValidateAttributeValueStringSizeExceeded()
        {
            var titleName = Cnn.FieldName(Global.SiteId, ContentName, "Title");
            var article1 = new Hashtable
            {
                [titleName] = new string('*', 1000)
            };

            Assert.That(() => { Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]); }, Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("too long"), "Validate string size");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            ContentId = Global.GetContentId(Cnn, ContentName);
            var srv = new ContentService(Global.ConnectionString, 1);
            if (srv.Exists(ContentId))
            {
                srv.Delete(ContentId);
            }
        }
    }
}
