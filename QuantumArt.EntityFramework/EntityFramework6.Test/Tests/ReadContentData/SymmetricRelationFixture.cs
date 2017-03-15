using System.Data.Entity;
using System.Linq;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;

namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class SymmetricRelationFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Symmetric_Relation_Field_isLoaded([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.SymmetricRelationArticles.Include(x => x.SymmetricRelation).FirstOrDefault();
                Assert.That(items.SymmetricRelation.Count, Is.Not.EqualTo(0));
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_BackSymmetric_Relation_Field_isLoaded([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.SymmetricRelationArticles.Include(x => x.SymmetricRelation.Select(y => y.ToSymmetricRelation)).FirstOrDefault();
                if (items.SymmetricRelation.Count == 0)
                {
                    Assert.Fail("SymmerticRelation field not filled");
                }
                else
                {
                    foreach (var item in items.SymmetricRelation)
                    {
                        if (item.ToSymmetricRelation.Count > 0)
                        {
                            var ids = item.ToSymmetricRelation.Select(s => s.Id).ToList();
                            if (!ids.Contains(items.Id))
                            {
                                Assert.Fail("SymmerticRelation field not filled");
                            }
                        }
                        else Assert.Fail("SymmerticRelation field not filled");
                    }
                }
            }
        }
    }
}
