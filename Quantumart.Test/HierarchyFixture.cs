using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using NUnit.Framework;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Resizer;

namespace Quantumart.Test
{
    [TestFixture]
    public class HierarchyFixture
    {
        public static DBConnector Cnn { get; private set; }

        public static int RegionContentId { get; private set; }

        public static int ProductContentId { get; private set; }

        public static string RegionContentName { get; private set; }

        public static string ProductContentName { get; private set; }

        public static Dictionary<string, int> BaseArticlesIds { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new XmlDbUpdateReplayService(Global.ConnectionString);
            service.Process(Global.GetXml(@"xmls\hierarchy.xml"), null);
            RegionContentName = "test regions";
            ProductContentName = "test products";

            Cnn = new DBConnector(Global.ConnectionString)
            {
                DynamicImageCreator = new FakeDynamicImage(),
                FileSystem = new FakeFileSystem(),
                ForceLocalCache = true
            };

            RegionContentId = Global.GetContentId(Cnn, RegionContentName);
            ProductContentId = Global.GetContentId(Cnn, ProductContentName);
            BaseArticlesIds = Global.GetIdsWithTitles(Cnn, RegionContentId);
        }

        [Test]
        public void ArticleService_UpdateArticleWithOptimizedHierarchy_UncheckChildren()
        {
            var ids = new[] { BaseArticlesIds["root"], BaseArticlesIds["macro1"], BaseArticlesIds["macro2"], BaseArticlesIds["region12"], BaseArticlesIds["district113"] };
            var ids2 = new[] { BaseArticlesIds["root"] };
            using (new QPConnectionScope(Global.ConnectionString))
            {

                var articleService = new ArticleService(Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = String.Join(",", ids);

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
            using (new QPConnectionScope(Global.ConnectionString))
            {

                var articleService = new ArticleService(Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = String.Join(",", ids);

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
            using (new QPConnectionScope(Global.ConnectionString))
            {

                var articleService = new ArticleService(Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = String.Join(",", ids);

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
            using (new QPConnectionScope(Global.ConnectionString))
            {

                var articleService = new ArticleService(Global.ConnectionString, 1);
                var article = articleService.New(ProductContentId);
                article.FieldValues.Single(n => n.Field.Name == "Title").Value = "test";
                article.FieldValues.Single(n => n.Field.Name == "Regions").Value = String.Join(",", ids);

                Assert.DoesNotThrow(() => article = articleService.Save(article), "Create article");

                var expected = ids2.OrderBy(n => n).ToArray();
                var actual = article.FieldValues.Single(n => n.Field.Name == "Regions").RelatedItems.OrderBy(n => n).ToArray();

                Assert.That(actual, Is.EqualTo(expected), "M2M equality");
            }
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            var srv = new ContentService(Global.ConnectionString, 1);
            srv.Delete(ProductContentId);
            srv.Delete(RegionContentId);
            QPContext.UseConnectionString = false;
        }
    }
}
