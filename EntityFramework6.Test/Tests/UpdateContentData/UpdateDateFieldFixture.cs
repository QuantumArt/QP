using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System;

namespace EntityFramework6.Test.Tests.UpdateContentData
{
    [TestFixture]
    public class UpdateDateFieldFixture : DataContextUpdateFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_DateTime_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<DateTimeItemForUpdate>(access, mapping, a => a.DateTimeValueField = DateTime.Now, a => a.DateTimeValueField);
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Date_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<DateItemForUpdate>(access, mapping, a => a.DateValueField = DateTime.Now, a => a.DateValueField);
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Time_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<TimeItemForUpdate>(access, mapping, a => a.TimeValueField = DateTime.Now - DateTime.Today, a => a.TimeValueField);
        }
    }
}