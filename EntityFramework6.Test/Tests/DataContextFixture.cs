using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class DataContextFixtureFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("DataContext_AfiellFieldsItems")]
        public void DataContext_AfiellFieldsItems_Read([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.AfiellFieldsItems.ToArray();
                Assert.That(items, Is.Not.Null.And.Not.Empty);
            }
        }
    }
}