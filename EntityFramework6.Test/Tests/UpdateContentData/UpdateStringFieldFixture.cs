using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using System;
using System.Transactions;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class UpdateFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("DataContext_Update")]
        //public void DataContext_AfiellFieldsItems_UpdateString([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        public void DataContext_AfiellFieldsItems_UpdateString([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.StaticMapping)] Mapping mapping)
        {
            UpdateProperty<AfiellFieldsItem>(access, mapping, a => a.String = Guid.NewGuid().ToString(), a => a.String);
        }

        [Test, Combinatorial]
        [Category("DataContext_Update")]
        public void DataContext_AfiellFieldsItems_UpdateDateTime([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.StaticMapping)] Mapping mapping)
        //public void DataContext_AfiellFieldsItems_UpdateDateTime([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<AfiellFieldsItem>(access, mapping, a => a.DateTime = DateTime.Now, a => a.DateTime);
        }

        [Test, Combinatorial]
        [Category("DataContext_Update")]
        public void DataContext_AfiellFieldsItems_Insert([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.StaticMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {                
                var article = new AfiellFieldsItem()
                {
                    String = "str",
                    TextBox = "tbx",
                    DateTime = DateTime.Now,
                    Image = "users.gif",
                    Integer = 90,
                    Created = DateTime.Today
                };

                context.AfiellFieldsItems.Add(article);
                context.SaveChanges();

                Assert.That(article.Id, Is.Not.EqualTo(0));
                Assert.That(context.AfiellFieldsItems.Any(a => a.Id == article.Id), Is.True);
            }
        }

        private void UpdateProperty<TArticle>(ContentAccess access, Mapping mapping, Action<TArticle> setField, Func<TArticle, object> getField)
            where TArticle : class
        {
            using (var context = GetDataContext(access, mapping))
            using (var scope = new TransactionScope())
            {
                var oldItem = context.Set<TArticle>().FirstOrDefault();
                Assert.That(oldItem, Is.Not.Null);

                setField(oldItem);
                context.SaveChanges();

                var newItem = context.Set<TArticle>().FirstOrDefault();
                Assert.That(newItem, Is.Not.Null);

                Assert.That(getField(newItem), Is.EqualTo(getField(oldItem)));
            }
        }
    }
}