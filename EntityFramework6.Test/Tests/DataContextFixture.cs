using System;
using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class DataContextFixture
    {
        [OneTimeSetUp]
        public static void Init()
        {
            EF6ModelMappingConfigurator.DefaultContentAccess = EF6ModelMappingConfigurator.ContentAccess.Stage;
        }

        [Test]
        public void DataContext_Read_Products()
        {
            using (var model = EF6Model.Create())
            {
                var count = model.Products.Count();

                Assert.That(count, Is.GreaterThan(0));
            }
        }

        [Test]
        public void DataContext_Query_Data()
        {
            using (var model = EF6Model.Create())
            {
                var data = model.UserGroups.Take(100).ToArray();
                Assert.That(data, Is.Not.Null.And.Not.Empty);
            }
        }
    }
}
