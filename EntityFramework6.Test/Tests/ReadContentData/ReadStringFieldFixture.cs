using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class ReadStringFieldFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_StringItem_StringValue_NotEmpty([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.StringItems.ToArray();
                Assert.That(items, Is.Not.Null.And.Not.Empty);
                Assert.That(items, Is.All.Matches<StringItem>(itm => !string.IsNullOrEmpty(itm.StringValue)));
            }
        }
    }
}