using System.Linq;
using NUnit.Framework;
using QP8.Integration.Tests.Infrastructure;
using Quantumart.QP8.BLL;
using ArticleApiService = Quantumart.QP8.BLL.Services.API.ArticleService;
using ContentApiService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class ListTests
    {
        private static int _newsContentId = 271;
        private static int _archiveId = 0;

        [OneTimeSetUp]
        public static void Init()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var article0 = articleService.Read(1512);
                var copyResult = articleService.Copy(article0);
                _archiveId = copyResult.Id;
                articleService.SetArchiveFlag(_newsContentId, new[] { _archiveId }, true);
            }
        }

        [Test]
        public void ArticleService_GetFebNews_Got2()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, null, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(2));
                var ra = articles.Single(n => n.Id == 1512).FieldValues.Single(n => n.Field.Name == "Related Articles");
                Assert.That(ra.RelatedItems.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void ArticleService_GetFebIds_Got2()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, null, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(2));
            }
        }

        [Test]
        public void ArticleService_GetFebNewsWithIds_Got1()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, new []{1513, 1514, 1515, _archiveId}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsWithIds_Got1()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, new []{1513, 1514, 1515, _archiveId}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void ArticleService_GetFebNewsWithOnlyIds_Got1()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(0, new []{1513, 1514, 1515, _archiveId}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsWithOnlyIds_Got1()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(0, new []{1513, 1514, 1515, _archiveId}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(1));
            }
        }


        [Test]
        public void ArticleService_GetFebNewsIncludingArchive_Got3()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, null, false, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(3));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsIncludingArchive_Got3()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, null, false, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(3));
            }
        }

        [Test]
        public void ArticleService_GetFebNewsWithIdsIncludingArchive_Got2()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, new []{1513, 1514, 1515, _archiveId}, false, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(2));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsWithIdsIncludingArchive_Got2()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, new []{1513, 1514, 1515, _archiveId}, false, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(2));
            }
        }


        [Test]
        public void ArticleService_GetFebArticlesWithOnlyEmptyIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(0, new int[]{}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetFebArticlesWithEmptyIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, new int[]{}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsWithEmptyIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, new int[]{}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetNoFebArticlesWithNullIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(_newsContentId, null, true, "c.title like '%ффф%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetNoFebIdsWithNullIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(_newsContentId, null, true, "c.title like '%ффф%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }


        [Test]
        public void ArticleService_GetFebIdsWithOnlyEmptyIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(0, new int[]{}, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetFebArticlesWithOnlyNullIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.List(0, null, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [Test]
        public void ArticleService_GetFebIdsWithOnlyNullIds_Got0()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                var articles = articleService.Ids(0, null, true, "c.title like '%feb%'").ToArray();
                Assert.That(articles.Length, Is.EqualTo(0));
            }
        }

        [OneTimeTearDown]
        public static void Destroy()
        {
            using (new QPConnectionScope(Global.ConnectionInfo))
            {
                var articleService = new ArticleApiService(Global.ConnectionInfo, 1);
                articleService.Delete(_archiveId);
            }
        }
    }
}
