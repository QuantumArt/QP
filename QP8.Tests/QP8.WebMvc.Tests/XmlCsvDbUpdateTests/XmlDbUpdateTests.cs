using System.Xml.Linq;
using QP8.WebMvc.Tests.Infrastructure.Attributes;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Xunit;

namespace QP8.WebMvc.Tests.XmlCsvDbUpdateTests
{
    public class XmlDbUpdateTests
    {
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_CR_endings.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_LF_endings.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_with_duplicates.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_14-59-29.xml", "C763A682C267481D897ECB6E95B5469D")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml", "3471C3F5D13C53877D1EB0C90E8061E5")]
        [Theory, Trait("XmlDbUpdate", "XmlHashVerifier")]
        public void GivenXmlData_WhenContainsSpacesDuplicatesAndDifferentLineEndings_ShouldGiveExactGuidData(string xmlString, string expectedResult)
        {
            // Fixture setup
            // Exercise system
            var sutData = XDocument.Parse(xmlString).ToStringWithDeclaration(SaveOptions.DisableFormatting);
            sutData = XmlDbUpdateReplayService.FilterFromSubRootNodeDuplicates(sutData).ToStringWithDeclaration(SaveOptions.DisableFormatting);
            var actualResult = HashHelpers.CalculateMd5Hash(sutData);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
