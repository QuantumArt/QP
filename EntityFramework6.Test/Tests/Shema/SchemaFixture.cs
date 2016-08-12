using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System.Linq;

namespace EntityFramework6.Test.Tests.Shema
{
    [TestFixture]
    public class SchemaFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("Shema")]
        public void DataContext_Schema_GetContentInfo([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var contentId = context.GetInfo<AfiellFieldsItem>().Id;
                var expectedContentId = GetContentId(mapping);

                Assert.That(contentId, Is.EqualTo(expectedContentId));
            }
        }

        [Test, Combinatorial]
        [Category("Shema")]
        public void DataContext_Schema_GetAttributeInfo([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var attributeId = context.GetInfo<AfiellFieldsItem>(a => a.TextBox).Id;
                var expectedattributeId = GetFieldId(mapping);

                Assert.That(attributeId, Is.EqualTo(expectedattributeId));
            }
        }

        private int GetContentId(Mapping mapping)
        {
            if (new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping))
            {
                return 619;
            }
            else
            {
                return 618;
            }
        }

        private int GetFieldId(Mapping mapping)
        {
            if (new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping))
            {
                return 38027;
            }
            else
            {
                return 38013;
            }
        }
    }
}
