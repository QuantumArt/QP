using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace EntityFramework6.Test.Tests.UpdateContentData
{
    [TestFixture]
    public class UpdateArticleFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_IsInserted([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var status = context.StatusTypes.FirstOrDefault(s => s.StatusTypeName == "Published");
                Assert.That(status, Is.Not.Null);

                var title = Guid.NewGuid().ToString();
                var item = new ItemForInsert() { Title = title, StatusType = status };
                var id = item.Id;
                var created = item.Created;
                var modified = item.Modified;


                context.ItemsForInsert.Add(item);
                context.SaveChanges();

                Assert.That(item.Id, Is.Not.EqualTo(id));
                Assert.That(item.Title, Is.EqualTo(title));
                Assert.That(item.Created, Is.Not.EqualTo(created));
                Assert.That(item.Modified, Is.Not.EqualTo(modified));

                var insertedItem = context.ItemsForInsert.FirstOrDefault(itm => itm.Id == item.Id);

                Assert.That(insertedItem, Is.Not.Null);
                Assert.That(insertedItem.Id, Is.EqualTo(item.Id));
                Assert.That(insertedItem.Title, Is.EqualTo(item.Title));
                Assert.That(insertedItem.Created, Is.EqualTo(item.Created));
                Assert.That(insertedItem.Modified, Is.EqualTo(item.Modified));
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_Status_IsUpdated([Values(ContentAccess.Live)] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var none = context.StatusTypes.FirstOrDefault(s => s.StatusTypeName == "None");
                var published = context.StatusTypes.FirstOrDefault(s => s.StatusTypeName == "Published");
                
                Assert.That(published, Is.Not.Null);
                Assert.That(none, Is.Not.Null);

                var item = context.ItemsForUpdate.FirstOrDefault();
                Assert.That(item, Is.Not.Null);

                foreach (var status in new[] { none, published })
                {
                    item.StatusType = published;
                    context.SaveChanges();

                    var updatedItem = context.ItemsForUpdate.Where(itm => itm.Id == item.Id).FirstOrDefault();
                    Assert.That(updatedItem, Is.Not.Null);
                    Assert.That(updatedItem.StatusTypeId, Is.EqualTo(published.Id));
                }
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_IsUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = context.ItemsForUpdate.FirstOrDefault();
                Assert.That(item, Is.Not.Null);

                var id = item.Id;
                var title = Guid.NewGuid().ToString();
                var modified = item.Modified;

                item.Title = title;

                context.SaveChanges();

                Assert.That(item.Id, Is.EqualTo(id));
                Assert.That(item.Title, Is.EqualTo(title));
                Assert.That(item.Modified, Is.Not.EqualTo(modified));

                var updatedItem = context.ItemsForUpdate.FirstOrDefault(itm => itm.Id == id);

                Assert.That(updatedItem, Is.Not.Null);
                Assert.That(updatedItem.Id, Is.EqualTo(id));
                Assert.That(updatedItem.Title, Is.EqualTo(title));
                Assert.That(updatedItem.Modified, Is.EqualTo(item.Modified));
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_Transaction_IsCommited([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            int id = 0;
            var title = Guid.NewGuid().ToString();

            using (var scope = new TransactionScope())
            using (var context = GetDataContext(access, mapping))
            {
                var item = context.ItemsForUpdate.FirstOrDefault();
                Assert.That(item, Is.Not.Null);

                id = item.Id;
                item.Title = title;
                context.SaveChanges();
                scope.Complete();
            }

            using (var context = GetDataContext(access, mapping))
            {
                var commitedItem = context.ItemsForUpdate.FirstOrDefault(itm => itm.Id == id);

                Assert.That(commitedItem, Is.Not.Null);
                Assert.That(commitedItem.Title, Is.EqualTo(title));
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_Transaction_IsRollbacked([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            int id = 0;
            var title = Guid.NewGuid().ToString();

            using (var scope = new TransactionScope())
            using (var context = GetDataContext(access, mapping))
            {
                var item = context.ItemsForUpdate.FirstOrDefault();
                Assert.That(item, Is.Not.Null);

                id = item.Id;
                item.Title = title;
                context.SaveChanges();
            }

            using (var context = GetDataContext(access, mapping))
            {
                var commitedItem = context.ItemsForUpdate.FirstOrDefault(itm => itm.Id == id);

                Assert.That(commitedItem, Is.Not.Null);
                Assert.That(commitedItem.Title, Is.Not.EqualTo(title));

            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Article_IsDeleted([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = context.ItemsForInsert.FirstOrDefault();
                Assert.That(item, Is.Not.Null);

                context.ItemsForInsert.Remove(item);
                context.SaveChanges();

                var deletedItem = context.ItemsForInsert.FirstOrDefault(itm => itm.Id == item.Id);
                Assert.That(deletedItem, Is.Null);
            }
        }
    }
}
