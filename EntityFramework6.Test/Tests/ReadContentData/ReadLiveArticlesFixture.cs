using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    class ReadLiveArticlesFixture : DataContextFixtureBase
    {

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_That_Published_Article_isLoaded([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_That_Published_Article_isLoaded([Values(ContentAccess.Live)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.PublishedNotPublishedItems.ToArray();
                Assert.That(items, Is.Not.Null.And.Not.Empty);
            }
        }

        //Для расщепленной статьи в режиме Live должна загружаться опубликованная статья
        private static string ALIAS_FOR_SPLITTED_ARTICLES = "SplittedItem";
        private static string TITLE_FOR_SPLITTED_PPUBLISHED_ARTICLES = "PublishArticle";
        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_That_Splitted_Article_isLoaded_Published_Version([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_That_Splitted_Article_isLoaded_Published_Version([Values(ContentAccess.Live)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = context.PublishedNotPublishedItems.Where(x => x.Alias.Equals(ALIAS_FOR_SPLITTED_ARTICLES)).FirstOrDefault();
                Assert.That(item.Title, Is.EqualTo(TITLE_FOR_SPLITTED_PPUBLISHED_ARTICLES));
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_That_nonPublished_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_That_nonPublished_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var nonpublishedItems = context.PublishedNotPublishedItems.Where(x => x.StatusTypeId == GetNonPublishedStatus(mapping)).ToArray();
                Assert.That(nonpublishedItems, Is.Null.Or.Empty);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_That_Archive_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_That_Archive_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var archivedItems = context.PublishedNotPublishedItems.Where(x => x.Archive).ToArray();
                Assert.That(archivedItems, Is.Null.Or.Empty);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_That_Invisible_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_That_Invisible_Article_notLoaded([Values(ContentAccess.Live)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var inVisibleItems = context.PublishedNotPublishedItems.Where(x => !x.Visible).ToArray();
                Assert.That(inVisibleItems, Is.Null.Or.Empty);
            }
        }

        private int GetNonPublishedStatus(Mapping mapping)
        {
            if (new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping))
            {
                return 148;
            }
            else
            {
                return 144;
            }
        }
    }
}

