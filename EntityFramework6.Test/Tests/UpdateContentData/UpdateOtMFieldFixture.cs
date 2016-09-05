using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.Tests.UpdateContentData
{
    [TestFixture]
    public class UpdateOtMFieldFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_OtM_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.OtMItemsForUpdate.Take(5).ToArray();
                var dict = context.OtMDictionaryForUpdate.FirstOrDefault();
                Assert.That(dict, Is.Not.Null);

                foreach (var item in items)
                {
                    item.Title = $"{nameof(Check_That_OtM_Field_isUpdated)}_{Guid.NewGuid()}";
                    item.Reference = dict;
                }

                context.SaveChanges();
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_OtM_Field_isCreated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = new OtMItemForUpdate() { Title = $"{nameof(Check_That_OtM_Field_isCreated)}_{Guid.NewGuid()}" };
                var dict = context.OtMDictionaryForUpdate.FirstOrDefault();

                Assert.That(dict, Is.Not.Null);

                item.Reference = dict;

                context.OtMItemsForUpdate.Add(item);
                context.SaveChanges();
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_OtM_Field_And_Dict_isCreated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = new OtMItemForUpdate() { Title = $"{nameof(Check_That_OtM_Field_And_Dict_isCreated)}_{Guid.NewGuid()}" };
                var dict = new OtMDictionaryForUpdate() { Title = $"{nameof(Check_That_OtM_Field_And_Dict_isCreated)}_{Guid.NewGuid()}" };

                item.Reference = dict;

                context.OtMItemsForUpdate.Add(item);
                context.OtMDictionaryForUpdate.Add(dict);

                context.SaveChanges();
            }
        }
    }
}
