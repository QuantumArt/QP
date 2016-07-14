using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.EF.CodeFirstV6.Data;

namespace EFCodeFirstV6Tests
{
    [TestClass]
    public class CodeFirstV6Tests
    {
        [TestMethod]
        public void TestMethod1_O2M()
        {
            using (var model = new Model1())
            {
                var result1 = model.Products
                    .Where(x => x.MarketingProduct_ID != null)
                    .FirstOrDefault();

                var result2 = model.Products
                    .Include(x => x.MarketingProduct)
                    .Where(x => x.MarketingProduct_ID != null)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result2);
                Assert.IsNotNull(result2.MarketingProduct);
            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_M2O()
        {
            using (var model = new Model1())
            {
                var result1 = model.MarketingProducts
                    .Include(x => x.Products)
                    .Where(x => x.Products.Count() > 0)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_M2M()
        {
            using (var model = new Model1())
            {
                var result1 = model.Products
                    .Include(x => x.Regions)
                    .Where(x => x.Regions.Count() > 0)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.Regions);
                Assert.IsTrue(result1.Regions.Count > 0);
            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_M2M_reversed()
        {
            using (var model = new Model1())
            {
                var result1 = model.Regions.Select(x => new
                {
                    x.Id,
                    x.Alias,
                    Count = x.Products.Count()
                }).ToList();

                Assert.IsTrue(result1.Any(x => x.Count > 0));
            }
        }


        [TestMethod]
        public void Test_HandWrittenModel_Self_O2M()
        {
            using (var model = new Model1())
            {
                var result1 = model.Regions
                    .Include(x => x.Parent)
                    .Where(x => x.Parent != null)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.Parent);
            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_Self_M2O()
        {
            using (var model = new Model1())
            {
                var result1 = model.Regions
                    .Include(x => x.Children)
                    .Where(x => x.Children.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.Children);
                Assert.IsTrue(result1.Children.Count > 0);
            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_M2M_self_symmetric()
        {
            using (var model = new Model1())
            {
                var result1 = model.Settings
                    .Include(x => x.RelatedSettings)
                    .Include(x => x.RelatedSettings.Select(y => y.RelatedSettings))
                    .Where(x => x.RelatedSettings.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.RelatedSettings);
                Assert.IsTrue(result1.RelatedSettings.Count > 0);

                Assert.IsNotNull(result1.RelatedSettings.First().RelatedSettingsBackward);
                Assert.IsTrue(result1.RelatedSettings.First().RelatedSettingsBackward.Count > 0);


                Assert.IsNotNull(result1.RelatedSettings.First().RelatedSettings);
                Assert.IsTrue(result1.RelatedSettings.First().RelatedSettings.Count > 0);

            }
        }

        [TestMethod]
        public void Test_HandWrittenModel_M2M_self_asymmetric()
        {
            using (var model = new Model1())
            {
                var result1 = model.Regions
                    .Include(x => x.AllowedRegions)
                    .Include(x => x.AllowedRegions.Select(y => y.AllowedRegionsBackward))
                    .Include(x => x.AllowedRegions.Select(y => y.AllowedRegions))
                    .Where(x => x.AllowedRegions.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.AllowedRegions);
                Assert.IsTrue(result1.AllowedRegions.Count > 0);

                Assert.IsNotNull(result1.AllowedRegions.First().AllowedRegionsBackward);
                Assert.IsTrue(result1.AllowedRegions.First().AllowedRegionsBackward.Count == 1);

                Assert.AreEqual(result1, result1.AllowedRegions.First().AllowedRegionsBackward.Single());

                Assert.IsNotNull(result1.AllowedRegions.First().AllowedRegions);
                Assert.IsTrue(result1.AllowedRegions.First().AllowedRegions.Count == 0);
            }
        }



        //[TestMethod]
        // развязочные сущности пока не используются.  
        public void Test_HandWrittenModel_M2M_Junction()
        {
            using (var model = new Model1())
            {
                var result1 = model.Products
                    .Include(x => x.ProduktyRegionyArticles)
                    .Where(x => x.ProduktyRegionyArticles.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.ProduktyRegionyArticles);
            }
        }

        [TestMethod]
        public void Test_System_Information()
        {
            using (var m = new Model1())
            {
                var r = m.StatusTypes.ToList();

                var r2 = m.Users
                    .Include(x => x.UserGroups)
                    .Take(10)
                    .ToList();
            }
        }

        [TestMethod]
        public void Test_split()
        {
            using (var m = new Model1())
            {
                var s = m.Settings
                    .Include(x => x.ValueExtended)
                    .Where(z => z.ValueExtended.ValueExtended.Length > 10)
                    .FirstOrDefault();
                Assert.IsNotNull(s);
            }
        }

    }
}
