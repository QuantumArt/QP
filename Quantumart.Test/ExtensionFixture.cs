using System;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;
using Quantumart.QPublishing.Database;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using NUnit.Framework;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Resizer;

namespace Quantumart.Test
{
    [TestFixture]
    public class ExtensionFixture
    {
        public static DBConnector Cnn { get; private set; }

        public const string BaseContent = "Test_BatchUpdate_Base";
        public const string ExContent11 = "Test_BatchUpdate_Ex1_1";
        public const string ExContent12 = "Test_BatchUpdate_Ex1_2";
        public const string ExContent21 = "Test_BatchUpdate_Ex2_1";
        public const string ExContent22 = "Test_BatchUpdate_Ex2_2";
        public const string DictionaryContent = "Test_BatchUpdate_Dictionary";


        public const string Classifier1 = "Field_Ex1";
        public const string Classifier2 = "Field_Ex2";
        public const string M2M = "Field_MtM";
        public const string O2M = "Field_OtM";
        public const string Parent = "Parent";

        public static int BaseContentId { get; private set; }

        public static int Ext11ContentId { get; private set; }

        public static int Ext12ContentId { get; private set; }

        public static int Ext21ContentId { get; private set; }

        public static int Ext22ContentId { get; private set; }

        public static int DictionaryContentId { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        public static int[] ExtArticlesIds1 { get; private set; }

        public static int[] ExtArticlesIds2 { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(Global.ConnectionString, 1, true);
            service.ReplayXml(Global.GetXml(@"xmls\batchupdate.xml"));

            Cnn = new DBConnector(Global.ConnectionString)
            {
                DynamicImageCreator = new FakeDynamicImage(),
                FileSystem = new FakeFileSystem(),
                ForceLocalCache = true
            };

            BaseContentId = Global.GetContentId(Cnn, BaseContent);
            Ext11ContentId = Global.GetContentId(Cnn, ExContent11);
            Ext12ContentId = Global.GetContentId(Cnn, ExContent12);
            Ext21ContentId = Global.GetContentId(Cnn, ExContent21);
            Ext22ContentId = Global.GetContentId(Cnn, ExContent22);
            DictionaryContentId = Global.GetContentId(Cnn, DictionaryContent);
            BaseArticlesIds = Global.GetIds(Cnn, BaseContentId);
            ExtArticlesIds1 = Global.GetIds(Cnn, Ext11ContentId);
            ExtArticlesIds2 = Global.GetIds(Cnn, Ext21ContentId);
        }

        [Test]
        public void ContentItem_SetArchive_MoveToArchive()
        {
            var id = BaseArticlesIds[0];
            var extId1 = ExtArticlesIds1[0];
            var extId2 = ExtArticlesIds2[0];
            var ids = new[] {id, extId1, extId2};

            Assert.That(() => SetArchive(ids, Cnn, true), Throws.Nothing);
            Assert.That(() => SetArchive(ids, Cnn, true), Throws.Nothing);
            Assert.That(Global.GetIdsFromArchive(Cnn, ids), Is.EqualTo(ids));

            Assert.That(() => SetArchive(ids, Cnn, false), Throws.Nothing);
            Assert.That(Global.GetIdsFromArchive(Cnn, ids), Is.Empty);

        }

        [Test]
        public void ContentItem_SetClassifier_ThrowsException()
        {
            var id = BaseArticlesIds[0];

            var ci = ContentItem.Read(id, Cnn);
            ci.FieldValues[Classifier1].Data = Ext12ContentId.ToString();

            Assert.That(() => ci.Save(), Throws.Exception.TypeOf<Exception>().And.Message.Contains("are not supported"));

        }

        [Test]
        public void ContentItem_SetAggrerated_ThrowsException()
        {
            var id = ExtArticlesIds1[0];

            var ci = ContentItem.Read(id, Cnn);
            ci.FieldValues[Parent].Data = string.Empty;

            Assert.That(() => ci.Save(), Throws.Exception.TypeOf<Exception>().And.Message.Contains("are not supported"));

        }

        public void SetArchive(int[] ids, DBConnector cnn, bool flag)
        {
            foreach (var id in ids)
            {
                var ci = ContentItem.Read(id, Cnn);
                ci.Archive = flag;
                ci.Save();
            }

        }


        [OneTimeTearDown]
        public static void TearDown()
        {
            var contentService = new ContentService(Global.ConnectionString, 1);
            var fieldService = new FieldService(Global.ConnectionString, 1);

            var articleService = new ArticleService(Global.ConnectionString, 1);
            articleService.Delete(BaseContentId, BaseArticlesIds);

            contentService.Delete(Ext11ContentId);
            contentService.Delete(Ext12ContentId);
            contentService.Delete(Ext21ContentId);
            contentService.Delete(Ext22ContentId);

            fieldService.Delete(Global.GetFieldId(Cnn, BaseContent, M2M));
            fieldService.Delete(Global.GetFieldId(Cnn, BaseContent, O2M));

            contentService.Delete(DictionaryContentId);
            contentService.Delete(BaseContentId);

            QPContext.UseConnectionString = false;
        }
    }
}
