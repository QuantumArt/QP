using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.EF.CodeFirstV6.Codegen.Catalog;

namespace EFCodeFirstV6Tests
{
    [TestClass]
    public class CodeFirstV6Gen_Beeline_Model_Tests
    {
        [TestInitialize]
        public void TestInit()
        {
            // Устанавливаем режим чтения статей по умолчанию как чтение расщепленных.
            QPDataContextMappingConfigurator.DefaultContentAccess =
                QPDataContextMappingConfigurator.ContentAccess.Stage;
        }
        [TestMethod]
        public void Test_Bee_CodeGen_M2M_Include_with_Single()
        {
            using (var model = QPDataContext.Create())
            {
                var result1 = model.InternetTariffs
                    .AsNoTracking()
                    .Include(x => x.Params)
                    .Where(x => x.Params.Any() && x.Price > 0)
                    .OrderBy(x => x.Price)
                    .Take(10)
                    .Select(x => x.Price)
                    .ToList();

            }
        }


        [TestMethod]
        public void Test_Bee_CodeGen_M2M_Include_with_Double()
        {
            using (var model = QPDataContext.Create())
            {
                var result1 = model.InternetTariffs
                    .Where(x => x.TransferPrice!= null)
                    .OrderByDescending(x => x.TransferPrice)
                    .Take(10)
                    .Select(x => x.TransferPrice)
                    .ToList();

            }
        }

        [TestMethod]
        public void Test_Bee_CodeGen_M2M_Include_with_Double_skip()
        {
            using (var model = QPDataContext.Create())
            {
                var result1 = model.InternetTariffs
                    .Where(x => x.TransferPrice != null)
                    .OrderByDescending(x => x.TransferPrice)
                    .Take(10)
                    .Skip(12)
                    .Select(x => x.TransferPrice)
                    .ToList();

            }
        }


        [TestMethod]
        public void Test_Bee_CodeGen_Time_Data_Type()
        {
            using (var model = QPDataContext.Create())
            {
                var result1 = model.InternetTariffs
                    .Where(x => x.Time != null)
                    .OrderByDescending(x => x.Time)
                    .Take(10)
                    .Select(x => x.Time)
                    .ToList();
            }
        }

        [TestMethod]
        public void Test_Bee_StatusTypee()
        {
            using (var model = QPDataContext.Create())
            {
                var result1 = model.Actions
                    .Include(x => x.StatusType)
                    .Take(2)
                    .ToList();
            }
        }

        [TestMethod]
        public void Test_Bee_User_With_Groups()
        {
            using (var model = QPDataContext.Create())
            {
                var a = model.Users
                    .Include(x => x.UserGroups)
                    .ToList();
            }
        }
    }
}