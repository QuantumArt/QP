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

            var values2 = new List<Dictionary<string, string>>();
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values2.Add(article4);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values2, 1));

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
        public void MassUpdate_InsertSplitAndMergeData_ForM2MAndStatusChanging()
        {
            var values = new List<Dictionary<string, string>>();
            var ints1 = new[] {CategoryIds[1], CategoryIds[3], CategoryIds[5]};
            var ints2 = new[] {CategoryIds[2], CategoryIds[3], CategoryIds[4]};

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints1),
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints2),
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Create");

            var ids1 = new[] { int.Parse(article1[SystemColumnNames.Id]) };
            var ids2 = new[] { int.Parse(article2[SystemColumnNames.Id]) };
            var intsSaved1 = Global.GetLinks(Cnn, ids1);
            var intsSaved2 = Global.GetLinks(Cnn, ids2);

            Assert.That(ints1, Is.EqualTo(intsSaved1), "First article M2M saved");
            Assert.That(ints2, Is.EqualTo(intsSaved2), "Second article M2M saved");

            var titles = new[] {"xnewtest", "xnewtest"};
            var intsNew1 = new[] { CategoryIds[0], CategoryIds[2], CategoryIds[3] };
            var intsNew2 = new[] { CategoryIds[3], CategoryIds[5] };
            article1["Categories"] = string.Join(",", intsNew1);
            article2["Categories"] = string.Join(",", intsNew2);
            article1["Title"] = titles[0];
            article2["Title"] = titles[1];
            article1["STATUS_TYPE_ID"] = NoneId.ToString();
            article2["STATUS_TYPE_ID"] = NoneId.ToString();

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Change and split");

            var intsUpdated1 = Global.GetLinks(Cnn, ids1);
            var intsUpdated2 = Global.GetLinks(Cnn, ids2);
            var intsUpdatedAsync1 = Global.GetLinks(Cnn, ids1, true);
            var intsUpdatedAsync2 = Global.GetLinks(Cnn, ids2, true);

            Assert.That(ints1, Is.EqualTo(intsUpdated1), "First article M2M (main) remains the same");
            Assert.That(ints2, Is.EqualTo(intsUpdated2), "Second article M2M (main) remains the same");
            Assert.That(intsNew1, Is.EqualTo(intsUpdatedAsync1), "First article M2M (async) saved");
            Assert.That(intsNew2, Is.EqualTo(intsUpdatedAsync2), "Second article M2M (async) saved");

            var values2 = new List<Dictionary<string, string>>();
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article1[SystemColumnNames.Id],
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article2[SystemColumnNames.Id],
                ["STATUS_TYPE_ID"] = PublishedId.ToString()
            };
            values2.Add(article4);


            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values2, 1), "Merge");

            var intsMerged1 = Global.GetLinks(Cnn, ids1);
            var intsMerged2 = Global.GetLinks(Cnn, ids2);
            var intsMergedAsync1 = Global.GetLinks(Cnn, ids1, true);
            var intsMergedAsync2 = Global.GetLinks(Cnn, ids2, true);
            var mergedTitles = Global.GetTitles(Cnn, ContentId, ids1.Union(ids2).ToArray());
            var mergedTitlesAsync = Global.GetTitles(Cnn, ContentId, ids1.Union(ids2).ToArray(), true);

            Assert.That(titles, Is.EqualTo(mergedTitles), "Updated articles (main) after merge");
            Assert.That(mergedTitlesAsync, Is.Empty, "Empty articles (async) after merge");
            Assert.That(intsMerged2, Is.EqualTo(intsUpdatedAsync2), "Second article M2M (main) merged");
            Assert.That(intsMerged1, Is.EqualTo(intsUpdatedAsync1), "First article M2M (main) merged");
            Assert.That(intsMergedAsync1, Is.Empty, "First article M2M (async) cleared");
            Assert.That(intsMergedAsync2, Is.Empty, "Second article M2M (async) cleared");

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
