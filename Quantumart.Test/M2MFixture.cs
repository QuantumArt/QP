using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Factories.Logging;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Info;

namespace Quantumart.Test
{
    [TestFixture]
    public class M2MFixture
    {
        public static int NoneId { get; private set; }

        public static int PublishedId { get; private set; }

        public static DBConnector Cnn { get; private set; }

        public static int ContentId { get; private set; }

        public static string ContentName { get; private set; }

        public static bool EFLinksExists { get; private set; }        

        public static string TitleName { get; private set; }

        public static string MainCategoryName { get; private set; }

        public static string NumberName { get; private set; }

        public static string CategoryName { get; private set; }

        public static int DictionaryContentId { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        public static int[] CategoryIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            Cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            ContentName = "Test M2M";
            Clear();

            LogProvider.LogFactory = new DiagnosticsDebugLogFactory();
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, 1, dbLogService.Object, false);
            service.Process(Global.GetXml(@"xmls\m2m.xml"));


            ContentId = Global.GetContentId(Cnn, ContentName);
            EFLinksExists = Global.EFLinksExists(Cnn, ContentId);
            TitleName = Cnn.FieldName(Global.SiteId, ContentName, "Title");
            MainCategoryName = Cnn.FieldName(Global.SiteId, ContentName, "MainCategory");
            NumberName = Cnn.FieldName(Global.SiteId, ContentName, "Number");
            CategoryName = Cnn.FieldName(Global.SiteId, ContentName, "Categories");
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

            var ints = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };

            var cntAsyncBefore = Global.CountLinks(Cnn, ints, true);
            var cntBefore = Global.CountLinks(Cnn, ints);
            var titlesBefore = Global.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncBefore = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesBefore = Global.CountArticles(Cnn, ContentId, ints);


            Assert.That(cntAsyncBefore, Is.EqualTo(0));
            Assert.That(cntBefore, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncBefore, Is.EqualTo(0));
            Assert.That(cntArticlesBefore, Is.Not.EqualTo(0));

            if (EFLinksExists)
            {
                var cntEFAsyncBefore = Global.CountEFLinks(Cnn, ints, ContentId, true);
                var cntEFBefore = Global.CountEFLinks(Cnn, ints, ContentId, false);
                Assert.That(cntEFAsyncBefore, Is.EqualTo(cntAsyncBefore));
                Assert.That(cntEFBefore, Is.EqualTo(cntBefore));
            }

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var cntAsyncAfterSplit = Global.CountLinks(Cnn, ints, true);
            var cntAfterSplit = Global.CountLinks(Cnn, ints);
            var asyncTitlesAfterSplit = Global.GetTitles(Cnn, ContentId, ints, true);
            var cntArticlesAsyncAfterSplit = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterSplit = Global.CountArticles(Cnn, ContentId, ints);

            Assert.That(cntAsyncAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntAfterSplit, Is.EqualTo(cntAsyncAfterSplit));
            Assert.That(cntArticlesAfterSplit, Is.Not.EqualTo(0));
            Assert.That(cntArticlesAsyncAfterSplit, Is.EqualTo(cntArticlesAfterSplit));

            if (EFLinksExists)
            {
                var cntEFAsyncAfterSplit = Global.CountEFLinks(Cnn, ints, ContentId, true);
                var cntEFAfterSplit = Global.CountEFLinks(Cnn, ints, ContentId, false);
                Assert.That(cntEFAsyncAfterSplit, Is.EqualTo(cntAsyncAfterSplit));
                Assert.That(cntEFAfterSplit, Is.EqualTo(cntAfterSplit));
            }

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
            var cntAfterMerge = Global.CountLinks(Cnn, ints);
            var titlesAfterMerge = Global.GetTitles(Cnn, ContentId, ints);
            var cntArticlesAsyncAfterMerge = Global.CountArticles(Cnn, ContentId, ints, true);
            var cntArticlesAfterMerge = Global.CountArticles(Cnn, ContentId, ints);

            Assert.That(cntAsyncAfterMerge, Is.EqualTo(0));
            Assert.That(cntAfterMerge, Is.Not.EqualTo(0));
            Assert.That(cntAfterMerge, Is.EqualTo(cntBefore));

            Assert.That(cntArticlesAsyncAfterMerge, Is.EqualTo(0));
            Assert.That(cntArticlesAfterMerge, Is.EqualTo(cntArticlesBefore));

            Assert.That(titlesBefore, Is.EqualTo(titlesAfterMerge));
            Assert.That(titlesBefore, Is.EqualTo(asyncTitlesAfterSplit));

            if (EFLinksExists)
            {
                var cntEFAsyncAfterMerge = Global.CountEFLinks(Cnn, ints, ContentId, true);
                var cntEFAfterMerge = Global.CountEFLinks(Cnn, ints, ContentId, false);
                Assert.That(cntEFAsyncAfterMerge, Is.EqualTo(cntAsyncAfterMerge));
                Assert.That(cntEFAfterMerge, Is.EqualTo(cntAfterMerge));
            }
        }


        [Test]
        public void MassUpdate_InsertSplitAndMergeData_ForM2MAndStatusChanging()
        {
            var values = new List<Dictionary<string, string>>();
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };
            var ints2 = new[] { CategoryIds[2], CategoryIds[3], CategoryIds[4] };

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

            if (EFLinksExists)
            {
                var intsEFSaved1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFSaved2 = Global.GetEFLinks(Cnn, ids2, ContentId, false);
                Assert.That(intsEFSaved1, Is.EquivalentTo(intsSaved1));
                Assert.That(intsEFSaved2, Is.EquivalentTo(intsSaved2));
            }

            var titles = new[] { "xnewtest", "xnewtest" };
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

            if (EFLinksExists)
            {
                var intsEFUpdated1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFUpdated2 = Global.GetEFLinks(Cnn, ids2, ContentId, false);
                var intsEFUpdatedAsync1 = Global.GetEFLinks(Cnn, ids1, ContentId, true);
                var intsEFUpdatedAsync2 = Global.GetEFLinks(Cnn, ids2, ContentId, true);
                Assert.That(intsEFUpdated1, Is.EquivalentTo(intsUpdated1));
                Assert.That(intsEFUpdated2, Is.EquivalentTo(intsUpdated2));
                Assert.That(intsEFUpdatedAsync1, Is.EquivalentTo(intsUpdatedAsync1));
                Assert.That(intsEFUpdatedAsync2, Is.EquivalentTo(intsUpdatedAsync2));
            }

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

            if (EFLinksExists)
            {
                var intsEFMerged1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFMerged2 = Global.GetEFLinks(Cnn, ids2, ContentId, false);
                var intsEFMergedAsync1 = Global.GetEFLinks(Cnn, ids1, ContentId, true);
                var intsEFMergedAsync2 = Global.GetEFLinks(Cnn, ids2, ContentId, true);
                Assert.That(intsEFMerged1, Is.EquivalentTo(intsMerged1));
                Assert.That(intsEFMerged2, Is.EquivalentTo(intsMerged2));
                Assert.That(intsEFMergedAsync1, Is.EquivalentTo(intsMergedAsync1));
                Assert.That(intsEFMergedAsync2, Is.EquivalentTo(intsMergedAsync2));
            }
        }

        [Test]
        public void AddFormToContent_InsertSplitAndMergeData_ForM2MAndStatusChanging()
        {
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };

            var titles1 = new[] { "newtest" };
            var article1 = new Hashtable
            {
                [TitleName] = titles1[0],
                [CategoryName] = string.Join(",", ints1),
                [MainCategoryName] = CategoryIds[0]

            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Create");

            var ids1 = new[] { id };

            var intsSaved1 = Global.GetLinks(Cnn, ids1);

            Assert.That(ints1, Is.EqualTo(intsSaved1), "article M2M saved");

            if (EFLinksExists)
            {
                var intsEFSaved1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                Assert.That(ints1, Is.EqualTo(intsEFSaved1), "article EF M2M saved");
            }


            var titles2 = new[] { "xnewtest" };
            var intsNew1 = new[] { CategoryIds[0], CategoryIds[2], CategoryIds[3] };
            article1[CategoryName] = string.Join(",", intsNew1);
            article1[TitleName] = titles2[0];

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "None", ref article1, id);
            }, "Change and split");

            var intsUpdated1 = Global.GetLinks(Cnn, ids1);
            var intsUpdatedAsync1 = Global.GetLinks(Cnn, ids1, true);
            var updatedTitlesAsync = Global.GetTitles(Cnn, ContentId, ids1, true);
            var updatedTitles = Global.GetTitles(Cnn, ContentId, ids1);

            Assert.That(titles1, Is.EqualTo(updatedTitles), "Article (main) remains the same");
            Assert.That(titles2, Is.EqualTo(updatedTitlesAsync), "Article (async) saved");
            Assert.That(ints1, Is.EqualTo(intsUpdated1), "Article M2M (main) remains the same");
            Assert.That(intsNew1, Is.EqualTo(intsUpdatedAsync1), "Article M2M (async) saved");

            if (EFLinksExists)
            {
                var intsEFUpdated1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFUpdatedAsync1 = Global.GetEFLinks(Cnn, ids1, ContentId, true);
                Assert.That(ints1, Is.EqualTo(intsEFUpdated1), "Article EF M2M (main) remains the same");
                Assert.That(intsNew1, Is.EqualTo(intsEFUpdatedAsync1), "Article EF M2M (async) saved");
            }

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id);
            }, "Merge with values");


            var intsMerged1 = Global.GetLinks(Cnn, ids1);
            var intsMergedAsync1 = Global.GetLinks(Cnn, ids1, true);
            var mergedTitles = Global.GetTitles(Cnn, ContentId, ids1);
            var mergedTitlesAsync = Global.GetTitles(Cnn, ContentId, ids1, true);

            Assert.That(titles2, Is.EqualTo(mergedTitles), "Updated article (main) after merge");
            Assert.That(mergedTitlesAsync, Is.Empty, "Empty article (async) after merge");
            Assert.That(intsMerged1, Is.EqualTo(intsUpdatedAsync1), "Article M2M (main) merged");
            Assert.That(intsMergedAsync1, Is.Empty, "Article M2M (async) cleared");

            if (EFLinksExists)
            {
                var intsEFMerged1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFMergedAsync1 = Global.GetEFLinks(Cnn, ids1, ContentId, true);
                Assert.That(intsEFMerged1, Is.EqualTo(intsUpdatedAsync1), "Article EF M2M (main) merged");
                Assert.That(intsEFMergedAsync1, Is.Empty, "Article EF M2M (async) cleared");
            }
        }

        [Test]
        public void MassUpdate_SaveAndUpdateOK_ForM2MData()
        {
            var values = new List<Dictionary<string, string>>();
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };
            var ints2 = new[] { CategoryIds[2], CategoryIds[3], CategoryIds[4] };

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
            var ids = ids1.Union(ids2).ToArray();
            var intsSaved1 = Global.GetLinks(Cnn, ids1);
            var intsSaved2 = Global.GetLinks(Cnn, ids2);

            Assert.That(ints1, Is.EqualTo(intsSaved1), "First article M2M saved");
            Assert.That(ints2, Is.EqualTo(intsSaved2), "Second article M2M saved");

            if (EFLinksExists)
            {
                var intsEFSaved1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFSaved2 = Global.GetEFLinks(Cnn, ids2, ContentId, false);
                Assert.That(ints1, Is.EqualTo(intsEFSaved1), "First article EF M2M saved");
                Assert.That(ints2, Is.EqualTo(intsEFSaved2), "Second article EF M2M saved");
            }

            var titles = new[] { "xnewtest", "xnewtest" };
            var intsNew1 = new[] { CategoryIds[0], CategoryIds[2], CategoryIds[3] };
            var intsNew2 = new[] { CategoryIds[3], CategoryIds[5] };
            article1["Categories"] = string.Join(",", intsNew1);
            article2["Categories"] = string.Join(",", intsNew2);
            article1["Title"] = titles[0];
            article2["Title"] = titles[1];

            var cntData = Global.CountData(Cnn, ids);
            var cntLinks = Global.CountLinks(Cnn, ids);

            if (EFLinksExists)
            {
                var cntEFLinks = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEFLinks, Is.EqualTo(cntLinks), "EF links");
            }


            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Change");

            var intsUpdated1 = Global.GetLinks(Cnn, ids1);
            var intsUpdated2 = Global.GetLinks(Cnn, ids2);

            Assert.That(intsNew1, Is.EqualTo(intsUpdated1), "First article M2M updated");
            Assert.That(intsNew2, Is.EqualTo(intsUpdated2), "Second article M2M updated");

            if (EFLinksExists)
            {
                var intsEFUpdated1 = Global.GetEFLinks(Cnn, ids1, ContentId, false);
                var intsEFUpdated2 = Global.GetEFLinks(Cnn, ids2, ContentId, false);
                Assert.That(intsNew1, Is.EqualTo(intsEFUpdated1), "First article EF M2M updated");
                Assert.That(intsNew2, Is.EqualTo(intsEFUpdated2), "Second article EF M2M updated");
            }

            var versions = Global.GetMaxVersions(Cnn, ids);
            var cntVersionData = Global.CountVersionData(Cnn, versions);
            var cntVersionLinks = Global.CountVersionLinks(Cnn, versions);

            Assert.That(versions.Length, Is.EqualTo(2), "Versions created");
            Assert.That(cntData, Is.EqualTo(cntVersionData), "Data moved to versions");
            Assert.That(cntLinks, Is.EqualTo(cntVersionLinks), "Links moved to versions");
        }

        [Test]
        public void AddFormToContent_SaveAndUpdateOK_ForM2MData()
        {
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };

            var article1 = new Hashtable
            {
                [TitleName] = "newtest",
                [CategoryName] = string.Join(",", ints1),
                [MainCategoryName] = CategoryIds[0]
            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Create");

            var ids = new[] { id };
            var intsSaved1 = Global.GetLinks(Cnn, ids);

            Assert.That(id, Is.Not.EqualTo(0), "Saved");
            Assert.That(ints1, Is.EqualTo(intsSaved1), "Article M2M saved");

            if (EFLinksExists)
            {
                var intsEFSaved1 = Global.GetEFLinks(Cnn, ids, ContentId, false);
                Assert.That(ints1, Is.EqualTo(intsEFSaved1), "Article EF M2M saved");
            }

            const string title1 = "xnewtest";
            var intsNew1 = new[] { CategoryIds[0], CategoryIds[2], CategoryIds[3] };
            article1[CategoryName] = string.Join(",", intsNew1);
            article1[TitleName] = title1;

            var cntData = Global.CountData(Cnn, ids);
            var cntLinks = Global.CountLinks(Cnn, ids);

            if (EFLinksExists)
            {
                var cntEFLinks = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEFLinks, Is.EqualTo(cntLinks), "EF links");
            }

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id);
            }, "Update");

            var intsUpdated1 = Global.GetLinks(Cnn, ids);
        
            Assert.That(intsNew1, Is.EqualTo(intsUpdated1), "Article M2M updated");

            if (EFLinksExists)
            {
                var intsEFUpdated1 = Global.GetEFLinks(Cnn, ids, ContentId, false);
                Assert.That(intsNew1, Is.EqualTo(intsEFUpdated1), "Article EF M2M updated");
            }

            var versions = Global.GetMaxVersions(Cnn, ids);
            var cntVersionData = Global.CountVersionData(Cnn, versions);
            var cntVersionLinks = Global.CountVersionLinks(Cnn, versions);

            Assert.That(versions.Length, Is.EqualTo(1), "Versions created");
            Assert.That(cntData, Is.EqualTo(cntVersionData), "Data moved to versions");
            Assert.That(cntLinks, Is.EqualTo(cntVersionLinks), "Links moved to versions");
        }


        [Test]
        public void MassUpdate_UpdateOK_ForAsymmetricData()
        {

            var ids = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };
            var descriptionsBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var numbersBefore = Global.GetNumbers(Cnn, ContentId, ids);
            var values = new List<Dictionary<string, string>>();

            const string title1 = "newtestxx";
            const string title2 = "newtestxxx";
            const int num = 30;
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString(),
                ["Title"] = title1,
                ["Number"] = num.ToString()

            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString(),
                ["Title"] = title2
            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Update");

            var descriptionsAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var titlesAfter = Global.GetTitles(Cnn, ContentId, ids);
            var numbersAfter = Global.GetNumbers(Cnn, ContentId, ids);

            Assert.That(numbersAfter[1], Is.EqualTo(numbersBefore[1]), "Number 2 remains the same");
            Assert.That(numbersAfter[0], Is.EqualTo(num), "Number 1 is changed");
            Assert.That(titlesAfter[1], Is.EqualTo(title2), "Title 2 is changed");
            Assert.That(titlesAfter[0], Is.EqualTo(title1), "Title 1 is changed");
            Assert.That(descriptionsAfter[1], Is.EqualTo(descriptionsBefore[1]), "Description 2 remains the same");
            Assert.That(descriptionsAfter[0], Is.EqualTo(descriptionsBefore[0]), "Description 1 remains the same");
        }

        [Test]
        public void ImportToContent_UpdateOK_ForAsymmetricData()
        {

            var values = new List<Dictionary<string, string>>();

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testa",
                ["Number"] = "20",
                ["Description"] = "abc"

            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testb",
                ["Number"] = "30",
                ["Description"] = "def"

            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values), "Create");

            var ids = new[] { int.Parse(article1[SystemColumnNames.Id]), int.Parse(article2[SystemColumnNames.Id]) };
            var descriptionsBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var numbersBefore = Global.GetNumbers(Cnn, ContentId, ids);

            var values2 = new List<Dictionary<string, string>>();
            const string title1 = "newtestab";
            const string title2 = "newtestabc";
            const int num = 40;
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article1[SystemColumnNames.Id],
                ["Title"] = title1,
                ["Number"] = num.ToString()

            };
            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article2[SystemColumnNames.Id],
                ["Title"] = title2

            };
            values2.Add(article4);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values2), "Update");

            var descriptionsAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var titlesAfter = Global.GetTitles(Cnn, ContentId, ids);
            var numbersAfter = Global.GetNumbers(Cnn, ContentId, ids);

            Assert.That(numbersAfter[1], Is.EqualTo(numbersBefore[1]), "Number 2 remains the same");
            Assert.That(numbersAfter[0], Is.EqualTo(num), "Number 1 is changed");
            Assert.That(titlesAfter[1], Is.EqualTo(title2), "Title 2 is changed");
            Assert.That(titlesAfter[0], Is.EqualTo(title1), "Title 1 is changed");
            Assert.That(descriptionsAfter[1], Is.EqualTo(descriptionsBefore[1]), "Description 2 remains the same");
            Assert.That(descriptionsAfter[0], Is.EqualTo(descriptionsBefore[0]), "Description 1 remains the same");
        }


        [Test]
        public void ImportToContent_UpdateOK_ForAsymmetricDataWithOverrideMissedFields()
        {

            var values = new List<Dictionary<string, string>>();

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testa",
                ["Number"] = "20",
                ["Description"] = "abc"

            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testb",
                ["Number"] = "30",
                ["Description"] = "def"

            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values), "Create");

            var ids = new[] { int.Parse(article1[SystemColumnNames.Id]), int.Parse(article2[SystemColumnNames.Id]) };

            var values2 = new List<Dictionary<string, string>>();
            const string title1 = "newtestab";
            const string title2 = "newtestabc";
            const int num = 40;
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article1[SystemColumnNames.Id],
                ["Title"] = title1,
                ["Number"] = num.ToString()

            };
            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article2[SystemColumnNames.Id],
                ["Title"] = title2

            };
            values2.Add(article4);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values2, 1, null, true), "Update");

            var descriptionsAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var titlesAfter = Global.GetTitles(Cnn, ContentId, ids);
            var numbersAfter = Global.GetNumbers(Cnn, ContentId, ids);

            Assert.That(numbersAfter[1], Is.EqualTo(0), "Number 2 is cleared");
            Assert.That(numbersAfter[0], Is.EqualTo(num), "Number 1 is changed");
            Assert.That(titlesAfter[1], Is.EqualTo(title2), "Title 2 is changed");
            Assert.That(titlesAfter[0], Is.EqualTo(title1), "Title 1 is changed");
            Assert.That(descriptionsAfter[1], Is.Null.Or.Empty, "Description 2 is cleared");
            Assert.That(descriptionsAfter[0], Is.Null.Or.Empty, "Description 1 is cleared");
        }

        [Test]
        public void ImportToContent_UpdateOK_ForAsymmetricDataWithOverrideMissedFieldsAndAttrIds()
        {

            var values = new List<Dictionary<string, string>>();

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testa",
                ["Number"] = "20",
                ["Description"] = "abc"

            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "testb",
                ["Number"] = "30",
                ["Description"] = "def"

            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values), "Create");

            var ids = new[] { int.Parse(article1[SystemColumnNames.Id]), int.Parse(article2[SystemColumnNames.Id]) };
            var descriptionsBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);

            var values2 = new List<Dictionary<string, string>>();
            const string title1 = "newtestab";
            const string title2 = "newtestabc";
            const int num = 40;
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article1[SystemColumnNames.Id],
                ["Title"] = title1,
                ["Number"] = num.ToString()

            };
            values2.Add(article3);
            var article4 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = article2[SystemColumnNames.Id],
                ["Title"] = title2

            };
            values2.Add(article4);

            var titleId = Cnn.FieldID(Global.SiteId, Cnn.GetContentName(ContentId), "Title");
            var numberId = Cnn.FieldID(Global.SiteId, Cnn.GetContentName(ContentId), "Number");
            var attrIds = new[] { titleId, numberId };


            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values2, 1, attrIds, true), "Update");

            var descriptionsAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids);
            var titlesAfter = Global.GetTitles(Cnn, ContentId, ids);
            var numbersAfter = Global.GetNumbers(Cnn, ContentId, ids);

            Assert.That(numbersAfter[1], Is.EqualTo(0), "Number 2 is cleared");
            Assert.That(numbersAfter[0], Is.EqualTo(num), "Number 1 is changed");
            Assert.That(titlesAfter[1], Is.EqualTo(title2), "Title 2 is changed");
            Assert.That(titlesAfter[0], Is.EqualTo(title1), "Title 1 is changed");
            Assert.That(descriptionsAfter[1], Is.EqualTo(descriptionsBefore[1]), "Description 2 remains the same");
            Assert.That(descriptionsAfter[0], Is.EqualTo(descriptionsBefore[0]), "Description 1 remains the same");

        }



        [Test]
        public void MassUpdate_ReturnModified_ForInsertingAndUpdatingData()
        {

            var ids = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };
            var modifiedBefore = Global.GetFieldValues<DateTime>(Cnn, ContentId, "Modified", ids);

            var values = new List<Dictionary<string, string>>();

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString()
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString()
            };
            values.Add(article2);
            var article3 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0"
            };
            values.Add(article3);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Update and Insert");
            var afterIds = values.Select(n => n[SystemColumnNames.Id]).Select(int.Parse).ToArray();
            var newId = afterIds.Except(ids).Single();

            var modifiedAfter = Global.GetFieldValues<DateTime>(Cnn, ContentId, "Modified", afterIds);
            var everyOneHasModified = values.All(n => n.ContainsKey(SystemColumnNames.Modified));
            var createdItemId = int.Parse(values.Single(n => n.ContainsKey(SystemColumnNames.Created))[SystemColumnNames.Id]);

            Assert.That(newId, Is.EqualTo(createdItemId), "New item has created");
            Assert.That(modifiedBefore, Does.Not.EqualTo(modifiedAfter.Take(2).ToArray()), "Modified changed");
            Assert.That(everyOneHasModified, Is.True, "All articles has Modified");

            var modifiedReturned = values.Select(n => DateTime.Parse(n[SystemColumnNames.Modified], CultureInfo.InvariantCulture)).ToArray();

            Assert.That(modifiedAfter, Is.EqualTo(modifiedReturned), "Return modified");

        }


        [Test]
        public void AddFormToContent_ReturnModified_ReturnModifiedTrue()
        {

            var article1 = new Hashtable
            {
                [TitleName] = "abc",
                [MainCategoryName] = CategoryIds[0]
            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Add article");

            var ids = new[] { id };

            var modified = DateTime.MinValue;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentId, "Published", ref article1, id, true, 0, true, false, true, ref modified);
            }, "Update article");

            var modifiedAfter = Global.GetFieldValues<DateTime>(Cnn, ContentId, "Modified", ids)[0];

            Assert.That(modified, Is.EqualTo(modifiedAfter), "Modified changed");

        }

        [Test]
        public void MassUpdate_DoesntReturnModified_ReturnModifiedFalse()
        {
            var values = new List<Dictionary<string, string>>();

            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString()
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString()
            };
            values.Add(article2);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1, new MassUpdateOptions { ReturnModified = false }), "Update");

            var noOneHasModified = values.All(n => !n.ContainsKey(SystemColumnNames.Modified));
            Assert.That(noOneHasModified, Is.EqualTo(true), "All articles has Modified");

        }


        [Test]
        public void MassUpdate_ThrowsException_ValidateAttributeValueInvalidNumericData()
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
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("type is incorrect"),
                "Validate numeric data"
            );
        }

        [Test]
        public void AddFormToContent_ThrowsException_ValidateAttributeValueInvalidNumericData()
        {
            var article1 = new Hashtable
            {
                [NumberName] = "test",
                [MainCategoryName] = CategoryIds[0]
            };

            Assert.That(
                () =>
                {
                    Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]);
                },
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("type is incorrect"),
                "Validate numeric data"
            );
        }

        [Test]
        public void MassUpdate_ThrowsException_ValidateAttributeValueStringDoesNotComplyInputMask()
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
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("input mask"),
                "Validate input mask"
            );
        }

        [Test]
        public void AddFormToContent_ThrowsException_ValidateAttributeValueStringDoesNotComplyInputMask()
        {
            var article1 = new Hashtable
            {
                [TitleName] = "test123",
                [MainCategoryName] = CategoryIds[0]
            };

            Assert.That(
                () =>
                {
                    Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, BaseArticlesIds[0]);
                },
                Throws.Exception.TypeOf<QpInvalidAttributeException>().And.Message.Contains("input mask"),
                "Validate input mask"
            );
        }

        [Test]
        public void MassUpdate_ArticleAddedWithDefaultValues_ValidateAttributeValueNewArticleWithMissedData()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Title"] = "newtest"
            };
            values.Add(article1);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Add article");

            var id = int.Parse(values[0][SystemColumnNames.Id]);
            var ids = new[] { id };

            Assert.That(id, Is.Not.EqualTo(0), "Return id");

            var desc = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids)[0];
            var num = (int)Global.GetNumbers(Cnn, ContentId, ids)[0];
            var cnt = Global.CountLinks(Cnn, ids);

            Assert.That(num, Is.Not.EqualTo(0), "Default number");
            Assert.That(desc, Is.Not.Null.Or.Empty, "Default description");
            Assert.That(cnt, Is.EqualTo(2), "Default M2M");

            if (EFLinksExists)
            {
                var cntEF = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEF, Is.EqualTo(2), "Default EF M2M");
            }
        }

        [Test]
        public void MassUpdate_WithUrlsAndReplaceUrlsDefault_ReplaceUrls()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Description"] = $@"<a href=""{ Cnn.GetImagesUploadUrl(Global.SiteId) }"">test</a>"
            };
            values.Add(article1);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Add article");

            var id = int.Parse(values[0][SystemColumnNames.Id]);
            var ids = new[] { id };

            var desc = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids)[0];

            Assert.That(desc, Does.Contain(@"<%=upload_url%>"));
        }

        [Test]
        public void MassUpdate_WithUrlsAndReplaceUrlsFalse_DoesNotReplaceUrls()
        {
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = "0",
                ["Description"] = $@"<a href=""{ Cnn.GetImagesUploadUrl(Global.SiteId) }"">test</a>"
            };
            values.Add(article1);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1, new MassUpdateOptions { ReplaceUrls = false }), "Add article");

            var id = int.Parse(values[0][SystemColumnNames.Id]);
            var ids = new[] { id };

            var desc = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids)[0];

            Assert.That(desc, Does.Not.Contain(@"<%=upload_url%>"));
        }

        [Test]
        public void AddFormToContent_ArticleAddedWithDefaultValues_ValidateAttributeValueNewArticleWithMissedData()
        {
            var article1 = new Hashtable
            {
                [TitleName] = "newtest"
            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Add article");

            var ids = new[] { id };

            Assert.That(id, Is.Not.EqualTo(0), "Return id");

            var desc = Global.GetFieldValues<string>(Cnn, ContentId, "Description", ids)[0];
            var num = (int)Global.GetNumbers(Cnn, ContentId, ids)[0];
            var cnt = Global.CountLinks(Cnn, ids);

            Assert.That(num, Is.Not.EqualTo(0), "Default number");
            Assert.That(desc, Is.Not.Null.Or.Empty, "Default description");
            Assert.That(cnt, Is.EqualTo(2), "Default M2M");

            if (EFLinksExists)
            {
                var cntEF = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEF, Is.EqualTo(2), "Default EF M2M");
            }
        }

        [Test]
        public void MassUpdate_DoesntCreateVersionDirectory_ContentDoesntHaveFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            Cnn.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[0].ToString()
            };
            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [SystemColumnNames.Id] = BaseArticlesIds[1].ToString()
            };
            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1), "Update");

            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "Shouldn't be called");
        }

        [Test]
        public void AddFormToContent_UpdateArchiveVisible_UpdateFlagsTrue()
        {

            var article1 = new Hashtable
            {
                [TitleName] = "abc",
                [MainCategoryName] = CategoryIds[0]
            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Add article");

            var ids = new[] { id };

            var visibleBefore = Global.GetFieldValues<decimal>(Cnn, ContentId, "Visible", ids)[0];
            var archiveBefore = Global.GetFieldValues<decimal>(Cnn, ContentId, "Archive", ids)[0];

            var modified = DateTime.MinValue;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentId, "Published", ref article1, id, true, 0, false, true, true, ref modified);
            }, "Update article");

            var visibleAfter = Global.GetFieldValues<decimal>(Cnn, ContentId, "Visible", ids)[0];
            var archiveAfter = Global.GetFieldValues<decimal>(Cnn, ContentId, "Archive", ids)[0];

            Assert.That(visibleBefore, Is.Not.EqualTo(visibleAfter), "Visible changed");
            Assert.That(archiveBefore, Is.Not.EqualTo(archiveAfter), "Archive changed");
            Assert.That(visibleAfter, Is.EqualTo(0), "Visible updated");
            Assert.That(archiveAfter, Is.EqualTo(1), "Archive updated");
        }

        [Test]
        public void AddFormToContent_UpdateOnlyOneField_AttrIdProvided()
        {

            var article1 = new Hashtable
            {
                [TitleName] = "txt",
                [MainCategoryName] = CategoryIds[0]
            };

            var article2 = new Hashtable
            {
                [MainCategoryName] = CategoryIds[1]
            };

            var mainCatId = int.Parse(MainCategoryName.Replace("field_", ""));

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Add article");

            var ids = new[] { id };

            var titleBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var catBefore = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "MainCategory", ids)[0];

            var files = (HttpFileCollection)null;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentId, "Published", ref article2, ref files, id, true, mainCatId);
            }, "Update article");

            var titleAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var catAfter = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "MainCategory", ids)[0];

            Assert.That(titleBefore, Is.EqualTo(titleAfter), "Same Title");
            Assert.That(catBefore, Is.Not.EqualTo(catAfter), "Category changed");
            Assert.That(catAfter, Is.EqualTo(CategoryIds[1]), "Category updated");
        }

        [Test]
        public void AddFormToContent_UpdateOnlyNonEmpty_UpdateEmptyTrue()
        {

            var article1 = new Hashtable
            {
                [TitleName] = "pdf",
                [NumberName] = "10",
                [MainCategoryName] = CategoryIds[0]
            };

            var article2 = new Hashtable
            {
                [TitleName] = "docx",
                [NumberName] = ""
            };


            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Add article");

            var ids = new[] { id };

            var titleBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var catBefore = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "MainCategory", ids)[0];
            var numBefore = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "Number", ids)[0];

            var files = (HttpFileCollection)null;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article2, ref files, id, false);
            }, "Update article");

            var titleAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var catAfter = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "MainCategory", ids)[0];
            var numAfter = (int)Global.GetFieldValues<decimal>(Cnn, ContentId, "Number", ids)[0];

            Assert.That(titleBefore, Is.Not.EqualTo(titleAfter), "Changed Title");
            Assert.That(catBefore, Is.EqualTo(catAfter), "Same Category");
            Assert.That(numBefore, Is.EqualTo(numAfter), "Same Number");
            Assert.That(titleAfter, Is.EqualTo(article2[TitleName]), "Category updated");
        }

        [Test]
        public void AddFormToContent_NullifyM2M_ForEmptyM2MData()
        {
            var ints1 = new[] { CategoryIds[1], CategoryIds[3], CategoryIds[5] };

            var article1 = new Hashtable
            {
                [TitleName] = "newtest",
                [CategoryName] = string.Join(",", ints1),
                [MainCategoryName] = CategoryIds[0]

            };

            var article2 = new Hashtable
            {
                [TitleName] = "newtest",
                [CategoryName] = "",
                [MainCategoryName] = CategoryIds[0]
            };

            var id = 0;

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0);
            }, "Create");

            var ids = new[] { id };
            var cntLinks = Global.CountLinks(Cnn, ids);
            var cntEFLinks = 0;

            Assert.That(cntLinks, Is.Not.EqualTo(0), "Links saved");

            if (EFLinksExists)
            {
                cntEFLinks = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEFLinks, Is.EqualTo(cntLinks), "EF links saved");
            }

            Assert.DoesNotThrow(() =>
            {
                id = Cnn.AddFormToContent(Global.SiteId, ContentName, "Published", ref article2, id);
            }, "Update");

            var cntLinksAfter = Global.CountLinks(Cnn, ids);            
            Assert.That(cntLinksAfter, Is.EqualTo(0), "Links nullified");

            if (EFLinksExists)
            {
                var cntEFLinksAfter = Global.CountEFLinks(Cnn, ids, ContentId, false);
                Assert.That(cntEFLinksAfter, Is.EqualTo(0), "EF links nullified");
            }
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
