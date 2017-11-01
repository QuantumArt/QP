using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Resizer;
using ArticleApiService = Quantumart.QP8.BLL.Services.API.ArticleService;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class HierarchyTests
    {
        public static DBConnector DbConnector { get; private set; }

        public static int RegionContentId { get; private set; }

        public static int ProductContentId { get; private set; }

        public static string RegionContentName { get; private set; }

        public static string ProductContentName { get; private set; }

        public static Dictionary<string, int> BaseArticlesIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            DbConnector = new DBConnector(Infrastructure.Global.ConnectionString)
            {
                DynamicImageCreator = new FakeDynamicImage(),
                FileSystem = new FakeFileSystem(),
                ForceLocalCache = true
            };

            RegionContentName = "test regions";
            ProductContentName = "test products";
            Clear();

            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Infrastructure.Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            service.Process(Infrastructure.Global.GetXml(@"TestData\hierarchy.xml"));

            RegionContentId = Infrastructure.Global.GetContentId(DbConnector, RegionContentName);
            ProductContentId = Infrastructure.Global.GetContentId(DbConnector, ProductContentName);
            BaseArticlesIds = Infrastructure.Global.GetIdsWithTitles(DbConnector, RegionContentId);
        }

        [Test]
        public void ArticleService_UpdateArticleWithOptimizedHierarchy_UncheckChildren()
        {
            var ids = new[] { BaseArticlesIds["root"], BaseArticlesIds["macro1"], BaseArticlesIds["macro2"], BaseArticlesIds["region12"], BaseArticlesIds["district113"] };
            var ids2 = new[] { BaseArticlesIds["root"] };
            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var articleService = new ArticleApiService(Infrastructure.Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = string.Join(",", ids);
                Assert.DoesNotThrow(() => article = articleService.Save(article), "Create article");

                var expected = ids2.OrderBy(n => n).ToArray();
                var actual = article.FieldValues.Single(n => n.Field.Name == "Regions").RelatedItems.OrderBy(n => n).ToArray();
                Assert.That(actual, Is.EqualTo(expected), "M2M equality");
            }
        }

        [Test]
        public void ArticleService_UpdateArticleWithOptimizedHierarchy_UncheckOneChild()
        {
            var ids = new[] { BaseArticlesIds["region12"], BaseArticlesIds["macro1"] };
            var ids2 = new[] { BaseArticlesIds["macro1"] };
            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var articleService = new ArticleApiService(Infrastructure.Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = string.Join(",", ids);
                Assert.DoesNotThrow(() => article = articleService.Save(article), "Create article");

                var expected = ids2.OrderBy(n => n).ToArray();
                var actual = article.FieldValues.Single(n => n.Field.Name == "Regions").RelatedItems.OrderBy(n => n).ToArray();
                Assert.That(actual, Is.EqualTo(expected), "M2M equality");
            }
        }

        [Test]
        public void ArticleService_UpdateArticleWithOptimizedHierarchy_CheckParentToRoot()
        {
            var ids = new[] { BaseArticlesIds["city111"], BaseArticlesIds["city112"], BaseArticlesIds["district113"], BaseArticlesIds["region12"], BaseArticlesIds["macro2"] };
            var ids2 = new[] { BaseArticlesIds["root"] };
            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var articleService = new ArticleApiService(Infrastructure.Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = string.Join(",", ids);
                Assert.DoesNotThrow(() => article = articleService.Save(article), "Create article");

                var expected = ids2.OrderBy(n => n).ToArray();
                var actual = article.FieldValues.Single(n => n.Field.Name == "Regions").RelatedItems.OrderBy(n => n).ToArray();
                Assert.That(actual, Is.EqualTo(expected), "M2M equality");
            }
        }

        [Test]
        public void ArticleService_UpdateArticleWithOptimizedHierarchy_CheckParentToMacro()
        {
            var ids = new[] { BaseArticlesIds["city111"], BaseArticlesIds["city112"], BaseArticlesIds["district113"], BaseArticlesIds["region12"] };
            var ids2 = new[] { BaseArticlesIds["macro1"] };
            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var articleService = new ArticleApiService(Infrastructure.Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = string.Join(",", ids);
                Assert.DoesNotThrow(() => article = articleService.Save(article), "Create article");

                var expected = ids2.OrderBy(n => n).ToArray();
                var actual = article.FieldValues.Single(n => n.Field.Name == "Regions").RelatedItems.OrderBy(n => n).ToArray();
                Assert.That(actual, Is.EqualTo(expected), "M2M equality");
            }
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            var srv = new ContentService(Infrastructure.Global.ConnectionString, 1);
            RegionContentId = Infrastructure.Global.GetContentId(DbConnector, RegionContentName);
            ProductContentId = Infrastructure.Global.GetContentId(DbConnector, ProductContentName);

            if (srv.Exists(ProductContentId))
            {
                srv.Delete(ProductContentId);
            }

            if (srv.Exists(RegionContentId))
            {
                srv.Delete(RegionContentId);
            }
        }
    }
}
