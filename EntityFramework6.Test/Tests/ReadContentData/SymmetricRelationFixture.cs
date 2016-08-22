using System.Linq;
using System.Data.Entity;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;

namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class SymmetricRelationFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Symmetric_Relation_Field_isLoaded([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.SymmetricRelationItems.Include(x => x.M2MSymmField).FirstOrDefault();
                Assert.That(items.M2MSymmField.Count, Is.Not.EqualTo(0));
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_BackSymmetric_Relation_Field_isLoaded([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.StaticMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.SymmetricRelationItems.Include(x => x.M2MSymmField).FirstOrDefault();
                var relat = context.SymmetricToItems.Include(x => x.M2MSymmField).ToList();
                foreach (var item in items.M2MSymmField)
                {
                    if (item.M2MSymmField.Count > 0)
                    {
                        var ids = item.M2MSymmField.Select(s => s.Id).ToList();
                        if (!ids.Contains(items.Id))
                        {
                            Assert.Fail("Not filled symmetric M2M filed");
                        }
                    }
                    else Assert.Fail("Not filled symmetric M2M filed");
                }
                Assert.Pass();
            }
        }
    }
}
