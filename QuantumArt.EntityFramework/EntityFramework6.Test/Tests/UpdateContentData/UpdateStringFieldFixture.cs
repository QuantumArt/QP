using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System;
using Quantumart.QP8.EntityFramework.Services;

namespace EntityFramework6.Test.Tests.UpdateContentData
{
    [TestFixture]
    public class UpdateStringFieldFixture : DataContextUpdateFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_String_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<StringItemForUpdate>(access, mapping, a => a.StringValue = Guid.NewGuid().ToString(), a => a.StringValue);
        }
    }
}
