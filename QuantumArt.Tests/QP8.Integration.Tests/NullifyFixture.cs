using System;
using System.Collections.Generic;
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
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class NullifyFixture
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
            service.Process(Global.GetXml(@"xmls\nullify.xml"));
            ContentId = Global.GetContentId(Cnn, ContentName);
            BaseArticlesIds = Global.GetIds(Cnn, ContentId);
        }


        [Test]
        public void MassUpdate_SaveNull_ForNull()
        {
            var values = new List<Dictionary<string, string>>();
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "Test,Test",
                ["Number"] = "5",
                ["Parent"] = BaseArticlesIds[0].ToString(),
                ["Date"] = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                ["Flag"] = "1"
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));
            var id2 = int.Parse(article2[FieldName.ContentItemId]);
            var ids = new[] { id2 };

            var titleBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var numBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Number", ids)[0];
            var parentBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Parent", ids)[0];
            var dateBefore = Global.GetFieldValues<DateTime?>(Cnn, ContentId, "Date", ids)[0];
            var flagBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Flag", ids)[0];

            Assert.That(titleBefore, Is.Not.Null);
            Assert.That(numBefore, Is.Not.Null);
            Assert.That(parentBefore, Is.Not.Null);
            Assert.That(dateBefore, Is.Not.Null);
            Assert.That(flagBefore, Is.Not.Null);

            values.Clear();
            var article3 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = id2.ToString(),
                ["Title"] = null,
                ["Number"] = null,
                ["Parent"] = null,
                ["Date"] = null,
                ["Flag"] = null
            };
            values.Add(article3);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var titleAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var numAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Number", ids)[0];
            var parentAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Parent", ids)[0];
            var dateAfter = Global.GetFieldValues<DateTime?>(Cnn, ContentId, "Date", ids)[0];
            var flagAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Flag", ids)[0];

            Assert.That(titleAfter, Is.Null);
            Assert.That(numAfter, Is.Null);
            Assert.That(parentAfter, Is.Null);
            Assert.That(dateAfter, Is.Null);
            Assert.That(flagAfter, Is.Null);
        }

        [Test]
        public void ImportToContent_SaveNull_ForNull()
        {
            var values = new List<Dictionary<string, string>>();
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "Test,Test",
                ["Number"] = "5",
                ["Parent"] = BaseArticlesIds[0].ToString(),
                ["Date"] = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                ["Flag"] = "1"
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values, 1));
            var id2 = int.Parse(article2[FieldName.ContentItemId]);
            var ids = new[] { id2 };

            var titleBefore = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var numBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Number", ids)[0];
            var parentBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Parent", ids)[0];
            var dateBefore = Global.GetFieldValues<DateTime?>(Cnn, ContentId, "Date", ids)[0];
            var flagBefore = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Flag", ids)[0];

            Assert.That(titleBefore, Is.Not.Null);
            Assert.That(numBefore, Is.Not.Null);
            Assert.That(parentBefore, Is.Not.Null);
            Assert.That(dateBefore, Is.Not.Null);
            Assert.That(flagBefore, Is.Not.Null);

            values.Clear();
            var article3 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = id2.ToString(),
                ["Title"] = null,
                ["Number"] = null,
                ["Parent"] = null,
                ["Date"] = null,
                ["Flag"] = null
            };
            values.Add(article3);

            Assert.DoesNotThrow(() => Cnn.ImportToContent(ContentId, values, 1));

            var titleAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var numAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Number", ids)[0];
            var parentAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Parent", ids)[0];
            var dateAfter = Global.GetFieldValues<DateTime?>(Cnn, ContentId, "Date", ids)[0];
            var flagAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Flag", ids)[0];

            Assert.That(titleAfter, Is.Null);
            Assert.That(numAfter, Is.Null);
            Assert.That(parentAfter, Is.Null);
            Assert.That(dateAfter, Is.Null);
            Assert.That(flagAfter, Is.Null);
        }

        [Test]
        public void MassUpdate_SaveNull_ForEmpty()
        {
            var values = new List<Dictionary<string, string>>();
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Title"] = "Test,Test",
                ["Number"] = "5"
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));
            var id2 = int.Parse(article2[FieldName.ContentItemId]);
            var ids = new[] { id2 };

            values.Clear();
            var article3 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = id2.ToString(),
                ["Title"] = "",
                ["Number"] = ""
            };
            values.Add(article3);

            Assert.DoesNotThrow(() => Cnn.MassUpdate(ContentId, values, 1));

            var titleAfter = Global.GetFieldValues<string>(Cnn, ContentId, "Title", ids)[0];
            var numAfter = Global.GetFieldValues<decimal?>(Cnn, ContentId, "Number", ids)[0];

            Assert.That(titleAfter, Is.Null);
            Assert.That(numAfter, Is.Null);


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
