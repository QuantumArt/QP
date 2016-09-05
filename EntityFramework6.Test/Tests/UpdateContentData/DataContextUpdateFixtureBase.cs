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
    public class DataContextUpdateFixtureBase : DataContextFixtureBase
    {
        protected void UpdateProperty<TArticle>(ContentAccess access, Mapping mapping, Action<TArticle> setField, Func<TArticle, object> getField)
           where TArticle : class
        {
            using (var context = GetDataContext(access, mapping))
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
