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
    public class UpdateMtMFieldFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_MtM_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.MtMItemsForUpdate.ToArray();
                var dict = context.MtMDictionaryForUpdate.Take(2).ToArray();

                foreach (var item in items)
                {
                    item.Title = $"{nameof(Check_That_MtM_Field_isUpdated)}_{Guid.NewGuid()}";

                    foreach (var d in dict)
                    {
                        item.Reference.Add(d);
                    }
                }

                context.SaveChanges();
            }
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_MtM_Field_isCreated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var item = new MtMItemForUpdate() { Title = $"{nameof(Check_That_MtM_Field_isCreated)}_{Guid.NewGuid()}" };

                var dict = context.MtMDictionaryForUpdate.Take(2).ToArray();

                foreach (var d in dict)
                {
                    item.Reference.Add(d);
                }

                context.MtMItemsForUpdate.Add(item);

                context.SaveChanges();
            }
        }
    }
}
