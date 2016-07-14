using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.EF.CodeFirstV6.Codegen.Data;


namespace EFCodeFirstV6Tests
{
    [TestClass]
    public class CodeFirstV6GenTests
    {
        [TestInitialize]
        public void TestInit()
        {
            // Устанавливаем режим чтения статей по умолчанию как чтение расщепленных.
            EF6ModelMappingConfigurator.DefaultContentAccess =
                EF6ModelMappingConfigurator.ContentAccess.Stage;
        }
        [TestMethod]
        public void Test_CodeGen_O2M()
        {
            using (var model = EF6Model.Create())
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
        public void Test_CodeGen_M2O()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.MarketingProducts
                    .Include(x => x.Products)
                    .Where(x => x.Products.Count() > 0)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
            }
        }

        [TestMethod]
        public void Test_CodeGen_M2M()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Products.AsNoTracking()
                    .Include(x => x.Regions)
                    .Where(x => x.Regions.Count() > 0)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
                Assert.IsNotNull(result1.Regions);
                Assert.IsTrue(result1.Regions.Count > 0);
            }
        }




        [TestMethod]
        public void Test_CodeGen_M2M_With_Related_Condition()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Products
                    .Where(x => x.Regions.Any(y => y.Alias == "moskovskaya-obl"))
                    .Select(x => new { x.Id, x.MarketingProduct.Title })
                    .ToList();

                Assert.IsNotNull(result1);
            }
        }


        [TestMethod]
        public void Test_CodeGen_Self_O2M()
        {
            using (var model = EF6Model.Create())
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
        public void Test_CodeGen_Self_M2O()
        {
            using (var model = EF6Model.Create())
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
        public void Test_CodeGen_M2M_self_symmetric()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Settings
                    .Include(x => x.RelatedSettings)
                    .Include(x => x.RelatedSettings.Select(y => y.RelatedSettings))
                    .Where(x => x.RelatedSettings.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1, "result");
                Assert.IsNotNull(result1.RelatedSettings, "RS");
                Assert.IsTrue(result1.RelatedSettings.Count > 0);

                Assert.IsNotNull(result1.RelatedSettings.First().BackwardForRelatedSettings, "BK");
                Assert.IsTrue(result1.RelatedSettings.First().BackwardForRelatedSettings.Count > 0);


                Assert.IsNotNull(result1.RelatedSettings.First().RelatedSettings);
                Assert.IsTrue(result1.RelatedSettings.First().RelatedSettings.Count > 0);

            }
        }

        [TestMethod]
        public void Test_CodeGen_M2M_self_asymmetric()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Regions
                    .Include(x => x.AllowedRegions)
                    .Include(x => x.AllowedRegions.Select(y => y.BackwardForAllowedRegions))
                    .Include(x => x.AllowedRegions.Select(y => y.AllowedRegions))
                    .Where(x => x.AllowedRegions.Any())
                    .FirstOrDefault();

                Assert.IsNotNull(result1, "result");
                Assert.IsNotNull(result1.AllowedRegions, "allowed regions");
                Assert.IsTrue(result1.AllowedRegions.Count > 0, "count > 0");

                Assert.IsNotNull(result1.AllowedRegions.First().BackwardForAllowedRegions, "BackwardForAllowedRegions");
                Assert.AreEqual(1, result1.AllowedRegions.First().BackwardForAllowedRegions.Count, "BackwardForAllowedRegions.Count == 1");

                Assert.AreEqual(result1, result1.AllowedRegions.First().BackwardForAllowedRegions.Single());

                Assert.IsNotNull(result1.AllowedRegions.First().AllowedRegions);
                Assert.IsTrue(result1.AllowedRegions.First().AllowedRegions.Count == 0);
            }
        }

        [TestMethod]
        public void Test_M2M_reversed_not_self()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Regions.Select(x => new
                {
                    x.Id,
                    x.Alias,
                    Count = x.BackwardForRegions.Count()
                }).ToList();

                Assert.IsTrue(result1.Any(x => x.Count > 0));
            }
        }

        [TestMethod]
        public void Test_M2M_reversed_self()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model.Settings.Select(x => new
                {
                    x.Id,
                    Count = x.BackwardForRelatedSettings.Count()
                }).ToList();

                Assert.IsTrue(result1.Any(x => x.Count > 0));
            }
        }

        [TestMethod]
        public void Test_CodeGen_Test_Live_Access()
        {
            // Читаем опубликованные статьи
            using (var model = EF6Model.Create(new EF6ModelMappingConfigurator(EF6ModelMappingConfigurator.ContentAccess.Live)))
            {
                var result1 = model.Products.Count(x => x.StatusTypeId != 125 // 125 - ид статуса Published
                    || x.Visible == false
                    || x.Archive == true);
                Assert.AreEqual(0, result1);
            }
        }

        [TestMethod]
        public void Test_CodeGen_Test_Stage_Access()
        {
            // Читаем расщепленные статьи
            using (var model = EF6Model.Create(new EF6ModelMappingConfigurator(EF6ModelMappingConfigurator.ContentAccess.Stage)))
            {
                var stage = model.Products.Count(x => x.StatusTypeId != 125  /* 125 - ид статуса Published*/);
                Assert.AreNotEqual(0, stage, "Нет продуктов в стейдже!");

                var invisibleOrArchived = model.Products.Count(x => x.Visible == false || x.Archive == true);
                Assert.AreEqual(0, invisibleOrArchived);
            }
        }

        [TestMethod]
        public void Test_CodeGen_Test_Invisible_Or_Archived()
        {
            // Читаем расщепленные, невидимые и архивные статьи
            using (var model = EF6Model.Create(new EF6ModelMappingConfigurator(EF6ModelMappingConfigurator.ContentAccess.InvisibleOrArchived)))
            {
                var invisibleOrArchived = model.Regions.Count(x => x.Visible == false || x.Archive == true);
                Assert.AreNotEqual(0, invisibleOrArchived);
            }
        }

        [TestMethod]
        public void Test_CodeGen_StatusType()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Products
                    .Include(x => x.StatusType)
                    .Take(10)
                    .ToList();
            }
        }
        [TestMethod]
        public void Test_System_Information()
        {
            using (var m = new EF6Model())
            {
                var r = m.StatusTypes.ToList();

                var r2 = m.Users
                    .Include(x => x.UserGroups)
                    .Take(10)
                    .ToList();
            }
        }

        [TestMethod]
        public void Test_CodeGen_decimal()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Settings.AsNoTracking()
                    .Where(z => z.DecimalValue > 0)
                    .FirstOrDefault();

                Assert.IsNotNull(result1);
            }
        }



        //[TestMethod]
        public void Test_CodeGen_add_scalar()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Settings.Add(new Setting { Title = "test title" });

                model.SaveChanges();
            }
        }

        //  [TestMethod]
        public void Test_CodeGen_update_scalar()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Settings.FirstOrDefault();

                result1.Title += "test!";
                result1.ValueMapped += "_1";

                result1.StatusType = model.StatusTypes.FirstOrDefault(x => x.Id != result1.StatusTypeId);

                model.SaveChanges();
            }
        }

        // [TestMethod]
        public void Test_CodeGen_update_O2M()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Settings.FirstOrDefault();

                result1.StatusType = model.StatusTypes.FirstOrDefault(x => x.Id != result1.StatusTypeId);

                model.SaveChanges();
            }
        }

        //[TestMethod]
        public void Test_CodeGen_update_M2M()
        {
            using (var model = EF6Model.Create())
            {
                var result1 = model
                    .Settings.FirstOrDefault(x => !x.RelatedSettings.Any());

                result1.RelatedSettings.Add(result1);

                model.SaveChanges();
            }
        }
    }
}
