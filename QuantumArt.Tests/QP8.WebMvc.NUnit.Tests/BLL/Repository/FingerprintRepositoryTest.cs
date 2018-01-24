using System.Linq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Repository;

namespace QP8.WebMvc.NUnit.Tests.BLL.Repository
{
    [TestFixture]
    public class FingerprintRepositoryTest
    {
        [Test]
        public void AddFilterStatement_ThereAreIdAndParentFieldNamesIncludedAndExceptedIdsAndParentIds()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings
            {
                IncludedIDs = new[] { 1, 2 },
                ExceptedIDs = new[] { 10, 20 },
                IncludedParentIDs = new[] { 3, 4 },
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: WHERE ID IN (1,2) AND PARENT_ID IN (3,4) ", actual[0]);
            Assert.AreEqual("2: WHERE ID IN (1,2) AND PARENT_ID IN (3,4) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreIdAndParentFieldNamesOnlyExceptedIdsAndParentIds()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings
            {
                ExceptedIDs = new[] { 10, 20 },
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: WHERE ID NOT IN (10,20) AND PARENT_ID NOT IN (30,40) ", actual[0]);
            Assert.AreEqual("2: WHERE ID NOT IN (10,20) AND PARENT_ID NOT IN (30,40) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreOnlyIdFieldNamesIncludedAndExceptedIdsAndParentIds()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", null, new FingerprintEntityTypeSettings
            {
                IncludedIDs = new[] { 1, 2 },
                ExceptedIDs = new[] { 10, 20 },
                IncludedParentIDs = new[] { 3, 4 },
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: WHERE ID IN (1,2) ", actual[0]);
            Assert.AreEqual("2: WHERE ID IN (1,2) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreOnlyParentIdFieldNameOnlyExceptedParentIds()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings
            {
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: WHERE PARENT_ID NOT IN (30,40) ", actual[0]);
            Assert.AreEqual("2: WHERE PARENT_ID NOT IN (30,40) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreIdAndParentFieldNamesNoIds()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings(), new[] { "1:{0}", "2:{0}" }).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: ", actual[0]);
            Assert.AreEqual("2: ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreIdAndParentFieldNamesIncludedAndExceptedIdsAndParentIdsAddWereIsFalse()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings
            {
                IncludedIDs = new[] { 1, 2 },
                ExceptedIDs = new[] { 10, 20 },
                IncludedParentIDs = new[] { 3, 4 },
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }, false).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: AND ID IN (1,2) AND PARENT_ID IN (3,4) ", actual[0]);
            Assert.AreEqual("2: AND ID IN (1,2) AND PARENT_ID IN (3,4) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreOnlyIdFieldNamesIncludedAndExceptedIdsAndParentIdsAddWereIsFalse()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", null, new FingerprintEntityTypeSettings
            {
                IncludedIDs = new[] { 1, 2 },
                ExceptedIDs = new[] { 10, 20 },
                IncludedParentIDs = new[] { 3, 4 },
                ExceptedParentIDs = new[] { 30, 40 }
            }, new[] { "1:{0}", "2:{0}" }, false).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: AND ID IN (1,2) ", actual[0]);
            Assert.AreEqual("2: AND ID IN (1,2) ", actual[1]);
        }

        [Test]
        public void AddFilterStatement_ThereAreIdAndParentFieldNamesNoIdsAddWereIsFalse()
        {
            var actual = FingerprintRepository.AddFilterStatement("ID", "PARENT_ID", new FingerprintEntityTypeSettings(), new[] { "1:{0}", "2:{0}" }, false).ToArray();

            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("1: ", actual[0]);
            Assert.AreEqual("2: ", actual[1]);
        }
    }
}
