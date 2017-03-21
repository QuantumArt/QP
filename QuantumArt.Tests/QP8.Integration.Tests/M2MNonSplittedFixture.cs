using System.Collections.Generic;
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
    public class M2MNonSplittedFixture
    {
        public static int NoneId { get; private set; }

        public static int PublishedId { get; private set; }

        public static DBConnector Cnn { get; private set; }

        public static int ContentId { get; private set; }

        public static int DictionaryContentId { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        public static int[] CategoryIds { get; private set; }

        public static bool EfLinksExists { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            Cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            Clear();

            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            service.Process(Global.GetXml(@"xmls\m2m_nonsplitted.xml"));

            ContentId = Global.GetContentId(Cnn, "Test M2M");
            EfLinksExists = Global.EfLinksExists(Cnn, ContentId);
            DictionaryContentId = Global.GetContentId(Cnn, "Test Category");
            BaseArticlesIds = Global.GetIds(Cnn, ContentId);
            CategoryIds = Global.GetIds(Cnn, DictionaryContentId);
            NoneId = Cnn.GetStatusTypeId(Global.SiteId, "None");
            PublishedId = Cnn.GetStatusTypeId(Global.SiteId, "Published");
        }

        [Test]
        public void MassUpdate_NoSplitAndMerge_ForSynWorkflow()
        {
            var values = new List<Dictionary<string, string>>();
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };
            var ints2 = new[] { CategoryIds[2], CategoryIds[3], CategoryIds[4] };
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints1),
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints2),
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Create");

            var ids1 = new[] { int.Parse(article1[FieldName.ContentItemId]) };
            var ids2 = new[] { int.Parse(article2[FieldName.ContentItemId]) };
            var intsSaved1 = Global.GetLinks(Cnn, ids1);
            var intsSaved2 = Global.GetLinks(Cnn, ids2);

            Assert.That(ints1, Is.EqualTo(intsSaved1), "First article M2M saved");
            Assert.That(ints2, Is.EqualTo(intsSaved2), "Second article M2M saved");
            if (EfLinksExists)
            {
                var intsEfSaved1 = Global.GetEfLinks(Cnn, ids1, ContentId);
                var intsEfSaved2 = Global.GetEfLinks(Cnn, ids2, ContentId);
                Assert.That(ints1, Is.EqualTo(intsEfSaved1), "First article EF M2M saved");
                Assert.That(ints2, Is.EqualTo(intsEfSaved2), "Second article EF M2M saved");
            }

            var titles = new[] { "xnewtest", "xnewtest" };
            var intsNew1 = new[] { CategoryIds[0], CategoryIds[2], CategoryIds[3] };
            var intsNew2 = new[] { CategoryIds[3], CategoryIds[5] };
            article1["Categories"] = string.Join(",", intsNew1);
            article2["Categories"] = string.Join(",", intsNew2);
            article1["Title"] = titles[0];
            article2["Title"] = titles[1];
            article1[FieldName.StatusTypeId] = NoneId.ToString();
            article2[FieldName.StatusTypeId] = NoneId.ToString();

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Change to none");
            var intsUpdated1 = Global.GetLinks(Cnn, ids1);
            var intsUpdated2 = Global.GetLinks(Cnn, ids2);
            var intsUpdatedAsync1 = Global.GetLinks(Cnn, ids1, true);
            var intsUpdatedAsync2 = Global.GetLinks(Cnn, ids2, true);

            Assert.That(intsNew1, Is.EqualTo(intsUpdated1), "First article M2M (main) saved");
            Assert.That(intsNew2, Is.EqualTo(intsUpdated2), "Second article M2M (main) saved");
            Assert.That(intsUpdatedAsync1, Is.Empty, "No first async M2M ");
            Assert.That(intsUpdatedAsync2, Is.Empty, "No second async M2M ");

            if (EfLinksExists)
            {
                var intsEfUpdated2 = Global.GetEfLinks(Cnn, ids2, ContentId);
                var intsEfUpdated1 = Global.GetEfLinks(Cnn, ids1, ContentId);
                var intsEfUpdatedAsync1 = Global.GetEfLinks(Cnn, ids1, ContentId, true);
                var intsEfUpdatedAsync2 = Global.GetEfLinks(Cnn, ids2, ContentId, true);

                Assert.That(intsNew1, Is.EqualTo(intsEfUpdated1), "First article EF M2M (main) saved");
                Assert.That(intsNew2, Is.EqualTo(intsEfUpdated2), "Second article EF M2M (main) saved");
                Assert.That(intsEfUpdatedAsync1, Is.Empty, "No first async EF M2M ");
                Assert.That(intsEfUpdatedAsync2, Is.Empty, "No second async EF M2M ");
            }

            var values2 = new List<Dictionary<string, string>>();
            var article3 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = article1[FieldName.ContentItemId],
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = article2[FieldName.ContentItemId],
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values2.Add(article4);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values2, 1), "Change to published");

            var intsPublished1 = Global.GetLinks(Cnn, ids1);
            var intsPublished2 = Global.GetLinks(Cnn, ids2);

            Assert.That(intsPublished1, Is.EqualTo(intsUpdated1), "First article same");
            Assert.That(intsPublished2, Is.EqualTo(intsUpdated2), "Second article same");
            if (EfLinksExists)
            {
                var intsEfPublished1 = Global.GetEfLinks(Cnn, ids1, ContentId);
                var intsEfPublished2 = Global.GetEfLinks(Cnn, ids2, ContentId);
                Assert.That(intsEfPublished1, Is.EqualTo(intsUpdated1), "First EF article same");
                Assert.That(intsEfPublished2, Is.EqualTo(intsUpdated2), "Second EF article same");
            }
        }

        [Test]
        public void MassUpdate_NoVersions_ForDisabledVersionControl()
        {
            var values = new List<Dictionary<string, string>>();
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };
            var ints2 = new[] { CategoryIds[2], CategoryIds[3], CategoryIds[4] };
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints1),
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "newtest",
                ["Categories"] = string.Join(",", ints2),
                [FieldName.StatusTypeId] = PublishedId.ToString()
            };

            values.Add(article2);
            var ids = new[] { int.Parse(article1[FieldName.ContentItemId]), int.Parse(article2[FieldName.ContentItemId]) };
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Create");
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Change");

            var versions = Global.GetMaxVersions(Cnn, ids);
            Assert.That(versions, Is.Empty, "Versions created");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            ContentId = Global.GetContentId(Cnn, "Test M2M");
            DictionaryContentId = Global.GetContentId(Cnn, "Test Category");
            var srv = new ContentService(Global.ConnectionString, 1);

            if (srv.Exists(ContentId))
            {
                srv.Delete(ContentId);
            }

            if (srv.Exists(DictionaryContentId))
            {
                srv.Delete(DictionaryContentId);
            }
        }
    }
}