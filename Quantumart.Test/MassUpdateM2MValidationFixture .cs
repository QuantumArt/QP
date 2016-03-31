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

            var service = new ReplayService(GlobalSettings.ConnectionString, 1, true);
            service.ReplayXml(GlobalSettings.GetXml(@"xmls\m2m.xml"));
            Cnn = new DBConnector(GlobalSettings.ConnectionString);
            ContentId = GlobalSettings.GetContentId(Cnn, "Test M2M");
            DictionaryContentId = GlobalSettings.GetContentId(Cnn, "Test Category");
            BaseArticlesIds = GlobalSettings.GetIds(Cnn, ContentId);
            CategoryIds = GlobalSettings.GetIds(Cnn, DictionaryContentId);
            NoneId = Cnn.GetStatusTypeId(GlobalSettings.SiteId, "None");
            PublishedId = Cnn.GetStatusTypeId(GlobalSettings.SiteId, "Published");
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

            var cntAsyncBefore = GlobalSettings.CountLinks(Cnn, ints, true);
            var cntBefore = GlobalSettings.CountLinks(Cnn, ints, false);
            var titlesBefore = GlobalSettings.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncBefore = GlobalSettings.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesBefore = GlobalSettings.CountArticles(Cnn, ContentId, ints, false);


            Assert.That(cntAsyncBefore, Is.EqualTo(0));
            Assert.That(cntBefore, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncBefore, Is.EqualTo(0));
            Assert.That(cntArticlesBefore, Is.Not.EqualTo(0));
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var cntAsyncAfterSplit = GlobalSettings.CountLinks(Cnn, ints, true);
            var cntAfterSplit = GlobalSettings.CountLinks(Cnn, ints, false);
            var asyncTitlesAfterSplit = GlobalSettings.GetTitles(Cnn, ContentId, ints, true);
            var cntArticlesAsyncAfterSplit = GlobalSettings.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterSplit = GlobalSettings.CountArticles(Cnn, ContentId, ints, false);

            Assert.That(cntAsyncAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntAfterSplit, Is.EqualTo(cntAsyncAfterSplit));
            Assert.That(cntArticlesAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncAfterSplit, Is.EqualTo(cntArticlesAfterSplit));

            article1["STATUS_TYPE_ID"] = PublishedId.ToString();
            article2["STATUS_TYPE_ID"] = PublishedId.ToString();

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var cntAsyncAfterMerge = GlobalSettings.CountLinks(Cnn, ints, true);
            var cntAfterMerge = GlobalSettings.CountLinks(Cnn, ints, false);
            var titlesAfterMerge = GlobalSettings.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncAfterMerge = GlobalSettings.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterMerge = GlobalSettings.CountArticles(Cnn, ContentId, ints, false);

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

        [OneTimeTearDown]
        public static void TearDown()
        {
            var srv = new ContentService(GlobalSettings.ConnectionString, 1);
            srv.Delete(ContentId);
            srv.Delete(DictionaryContentId);
            QPContext.UseConnectionString = false;
        }
    }
}
