using System;
using System.Linq;
using System.Data.Entity;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
//using Quantumart.QP8.EntityFramework.Services;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class DataContextFixture
    {
        [OneTimeSetUp]
        public static void Init()
        {
            EF6ModelMappingConfigurator.DefaultContentAccess = ContentAccess.Stage;
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
            using (var model = new EF6Model())
            {
                var data = model.Products.Take(100).ToArray();
                Assert.That(data, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void Test_System_Information()
        {
            using (var m = new EF6Model())
            {
                var r = m.StatusTypes.AsNoTracking().ToList();

                var r2 = m.Users
                    .Include(x => x.UserGroups)
                    .Take(10)
                    .ToList();
            }
        }
    }
}
