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
    public class MassUpdateM2MValidationFixture
    {
        public static int NoneId { get; private set; }
        public static int PublishedId { get; private set; }
        public static DBConnector Cnn { get; private set; }

        public static int ContentId { get; private set; }

        public static int DictionaryContentId { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        public static int[] CategoryIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(Global.ConnectionString, 1, true);
            service.ReplayXml(Global.GetXml(@"xmls\m2m.xml"));
            Cnn = new DBConnector(Global.ConnectionString);
            ContentId = Global.GetContentId(Cnn, "Test M2M");
            DictionaryContentId = Global.GetContentId(Cnn, "Test Category");
            BaseArticlesIds = Global.GetIds(Cnn, ContentId);
            CategoryIds = Global.GetIds(Cnn, DictionaryContentId);
            NoneId = Cnn.GetStatusTypeId(Global.SiteId, "None");
            PublishedId = Cnn.GetStatusTypeId(Global.SiteId, "Published");
        }


        [Test]
        public void MassUpdate_SplitAndMergeData_ForStatusChanging()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["STATUS_TYPE_ID"] = NoneId.ToString()
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["STATUS_TYPE_ID"] = NoneId.ToString()
            };
            values.Add(article2);

            var ints = new[] {BaseArticlesIds[0], BaseArticlesIds[1]};

            var cntAsyncBefore = Global.CountLinks(Cnn, ints, true);
            var cntBefore = Global.CountLinks(Cnn, ints, false);
            var titlesBefore = Global.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncBefore = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesBefore = Global.CountArticles(Cnn, ContentId, ints, false);


            Assert.That(cntAsyncBefore, Is.EqualTo(0));
            Assert.That(cntBefore, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncBefore, Is.EqualTo(0));
            Assert.That(cntArticlesBefore, Is.Not.EqualTo(0));
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var cntAsyncAfterSplit = Global.CountLinks(Cnn, ints, true);
            var cntAfterSplit = Global.CountLinks(Cnn, ints, false);
            var asyncTitlesAfterSplit = Global.GetTitles(Cnn, ContentId, ints, true);
            var cntArticlesAsyncAfterSplit = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterSplit = Global.CountArticles(Cnn, ContentId, ints, false);

            Assert.That(cntAsyncAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntAfterSplit, Is.EqualTo(cntAsyncAfterSplit));
            Assert.That(cntArticlesAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncAfterSplit, Is.EqualTo(cntArticlesAfterSplit));

            article1["STATUS_TYPE_ID"] = PublishedId.ToString();
            article2["STATUS_TYPE_ID"] = PublishedId.ToString();

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var cntAsyncAfterMerge = Global.CountLinks(Cnn, ints, true);
            var cntAfterMerge = Global.CountLinks(Cnn, ints, false);
            var titlesAfterMerge = Global.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncAfterMerge = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterMerge = Global.CountArticles(Cnn, ContentId, ints, false);

            Assert.That(cntAsyncAfterMerge, Is.EqualTo(0));
            Assert.That(cntAfterMerge, Is.Not.EqualTo(0));
            Assert.That(cntAfterMerge, Is.EqualTo(cntBefore));

            Assert.That(cntArticlesAsyncAfterMerge, Is.EqualTo(0));
            Assert.That(cntArticlesAfterMerge, Is.EqualTo(cntArticlesBefore));

            Assert.That(titlesBefore, Is.EqualTo(titlesAfterMerge));
            Assert.That(titlesBefore, Is.EqualTo(asyncTitlesAfterSplit));


        }

        [Test]
        public void ValidateAttributeValue_ThrowsException_InvalidNumericData()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Number"] = "test"
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("type is incorrect"),
                "Validate numeric data"
            );
        }

        [Test]
        public void ValidateAttributeValue_ThrowsException_StringDoesNotComplyInputMask()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = "test123"
            };
            values.Add(article1);

            Assert.That(
                () => Cnn.MassUpdate(ContentId, values, 1),
                Throws.Exception.TypeOf<QPInvalidAttributeException>().And.Message.Contains("input mask"),
                "Validate input mask"
            );
        }

        [Test]
        public void ValidateAttributeValue_ArticleAddedWithDefaultValues_NewArticleWithMissedData()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "newtest",
            };
            values.Add(article1);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Add article");

            int id = int.Parse(values[0][SystemColumnNames.Id]);
            var ids = new[] {id};

            Assert.That(id, Is.Not.EqualTo(0), "Return id");

            var desc = Global.GetFieldValues(Cnn, ContentId, "Description", ids)[0];
            var num = (int)Global.GetNumbers(Cnn, ContentId, ids)[0];
            var cnt = Global.CountLinks(Cnn, ids);

            Assert.That(num, Is.Not.EqualTo(0), "Default number");
            Assert.That(desc, Is.Not.Null.Or.Empty, "Default description");
            Assert.That(cnt, Is.EqualTo(2), "Default M2M");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            var srv = new ContentService(Global.ConnectionString, 1);
            srv.Delete(ContentId);
            srv.Delete(DictionaryContentId);
            QPContext.UseConnectionString = false;
        }
    }
}
