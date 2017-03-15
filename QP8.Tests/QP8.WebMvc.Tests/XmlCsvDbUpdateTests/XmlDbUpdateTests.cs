using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using QP8.Infrastructure.Logging.Factories;
using QP8.WebMvc.Tests.Infrastructure.Attributes;
using QP8.WebMvc.Tests.Infrastructure.Specimens;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Adapters;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Xunit;

namespace QP8.WebMvc.Tests.XmlCsvDbUpdateTests
{
    public class XmlDbUpdateTests
    {
        private readonly IFixture _fixture;

        public XmlDbUpdateTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization()).Customize(new MultipleCustomization());
            _fixture.Customizations.Add(new NameValueSpecimenBuilder());
            _fixture.Customize<XmlDbUpdateRecordedAction>(m => m.Without(action => action.Lcid));

            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
            LogProvider.LogFactory = new NullLogFactory();
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_CR_endings.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_LF_endings.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\test_sample_for_hash_with_duplicates.xml", "ACADB78E0CE686C60564A71F1F595993")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_guid_rename.xml", "C763A682C267481D897ECB6E95B5469D")]
        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml", "3471C3F5D13C53877D1EB0C90E8061E5")]
        [Theory, Trait("XmlDbUpdate", "XmlHashVerifier")]
        public void GivenXmlData_WhenContainsSpacesDuplicatesAndDifferentLineEndings_ShouldCorrectlyCalculateHash(string xmlString, string expectedHash)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var dbLogService = _fixture.Freeze<Mock<IXmlDbUpdateLogService>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false);
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml);
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(expectedHash)).Verifiable();

            // Exercise system
            sut.Process(xmlString);

            // Verify outcome
            dbLogService.Verify();
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml", "3471C3F5D13C53877D1EB0C90E8061E5")]
        [Theory, Trait("XmlDbUpdate", "XmlHashVerifier")]
        public void GivenXmlData_WhenContainsCorrectData_ShouldCallAllServices(string xmlString, string expectedHash)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var recordedAction = _fixture.Create<XmlDbUpdateRecordedAction>();
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var dbLogService = _fixture.Freeze<Mock<IXmlDbUpdateLogService>>();
            var actionsCorrecterService = _fixture.Freeze<Mock<IXmlDbUpdateActionCorrecterService>>();
            var httpContextProcessor = _fixture.Freeze<Mock<IXmlDbUpdateHttpContextProcessor>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false).Verifiable();
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml).Verifiable();

            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false).Verifiable();
            dbLogService.Setup(m => m.InsertFileLogEntry(It.IsAny<XmlDbUpdateLogModel>())).Verifiable();

            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false).Verifiable();
            actionsCorrecterService.Setup(m => m.PreActionCorrections(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<bool>())).Verifiable();
            httpContextProcessor.Setup(m => m.PostAction(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Verifiable();
            actionsCorrecterService.Setup(m => m.PostActionCorrections(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<HttpContextBase>())).Returns(recordedAction).Verifiable();
            dbLogService.Setup(m => m.InsertActionLogEntry(It.IsAny<XmlDbUpdateActionsLogModel>())).Verifiable();

            // Exercise system
            sut.Process(xmlString);

            // Verify outcome
            dbLogService.Verify();
            appInfoRepository.Verify();
            httpContextProcessor.Verify();
            actionsCorrecterService.Verify();
        }


        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml")]
        [Theory, Trait("XmlDbUpdate", "DbValidation")]
        public void GivenXmlData_WhenDbRecordingIsOn_ShouldThrowException(string xmlString)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(true).Verifiable();
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml);

            // Exercise system
            Action sutAction = () => sut.Process(xmlString);

            // Verify outcome
            Assert.Throws<InvalidOperationException>(sutAction);
            appInfoRepository.Verify();
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml")]
        [Theory, Trait("XmlDbUpdate", "DbValidation")]
        public void GivenXmlData_WhenDbVersionDifferentFromXml_ShouldThrowException(string xmlString)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.1";
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false);
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml).Verifiable();

            // Exercise system
            Action sutAction = () => sut.Process(xmlString);

            // Verify outcome
            Assert.Throws<InvalidOperationException>(sutAction);
            appInfoRepository.Verify();
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml")]
        [Theory, Trait("XmlDbUpdate", "DbLogging")]
        public void GivenXmlData_WhenFileWasAlreadyReplayed_ShouldThrowException(string xmlString)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var dbLogService = _fixture.Freeze<Mock<IXmlDbUpdateLogService>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false);
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml);
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(true);

            // Exercise system
            Action sutAction = () => sut.Process(xmlString);

            // Verify outcome
            Assert.Throws<XmlDbUpdateLoggingException>(sutAction);
            dbLogService.Verify(m => m.InsertFileLogEntry(It.IsAny<XmlDbUpdateLogModel>()), Times.Never());
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog2_2016-10-27_17-00-16.xml", "3471C3F5D13C53877D1EB0C90E8061E5")]
        [Theory, Trait("XmlDbUpdate", "DbLogging")]
        public void GivenXmlData_WhenFileWasNotReplayedBefore_ShouldInsertDbEntry(string xmlString, string expectedHash)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var dbLogService = _fixture.Freeze<Mock<IXmlDbUpdateLogService>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false);
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml);
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.InsertFileLogEntry(It.Is<XmlDbUpdateLogModel>(entry => entry.Hash == expectedHash))).Verifiable();

            // Exercise system
            sut.Process(xmlString);

            // Verify outcome
            dbLogService.Verify();
        }

        [XmlDbUpdateDataReader(@"TestData\ConsoleDbUpdate\XmlData\mts_catalog_with_five_actions.xml")]
        [Theory, Trait("XmlDbUpdate", "DbLogging")]
        public void GivenXmlData_WhenActionsWasNotReplayedBefore_ShouldSkipAndProcessOtherActions(string xmlString)
        {
            // Fixture setup
            const string dbVersionFromXml = "7.9.9.0";
            var recordedAction = _fixture.Create<XmlDbUpdateRecordedAction>();
            var appInfoRepository = _fixture.Freeze<Mock<IApplicationInfoRepository>>();
            var dbLogService = _fixture.Freeze<Mock<IXmlDbUpdateLogService>>();
            var actionsCorrecterService = _fixture.Freeze<Mock<IXmlDbUpdateActionCorrecterService>>();
            var httpContextProcessor = _fixture.Freeze<Mock<IXmlDbUpdateHttpContextProcessor>>();
            var sut = _fixture.Create<XmlDbUpdateNonMvcReplayService>();

            appInfoRepository.Setup(m => m.RecordActions()).Returns(false);
            appInfoRepository.Setup(m => m.GetCurrentDbVersion()).Returns(dbVersionFromXml);
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.SetupSequence(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(true).Returns(false).Returns(false).Returns(false).Returns(false);
            dbLogService.Setup(m => m.InsertActionLogEntry(It.IsAny<XmlDbUpdateActionsLogModel>()));
            actionsCorrecterService.Setup(m => m.PreActionCorrections(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<bool>()));
            httpContextProcessor.Setup(m => m.PostAction(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()));
            actionsCorrecterService.Setup(m => m.PostActionCorrections(It.IsAny<XmlDbUpdateRecordedAction>(), It.IsAny<HttpContextBase>())).Returns(recordedAction);

            // Exercise system
            sut.Process(xmlString);

            // Verify outcome
            dbLogService.Verify(m => m.InsertActionLogEntry(It.IsAny<XmlDbUpdateActionsLogModel>()), Times.Exactly(4));
        }

        [Theory, AutoData, Trait("XmlDbUpdate", "ActionSerializing")]
        public void GetXmlDbUpdateRecordedAction_WhenSerializingForRecord_ShouldIncludeRootElement(string currentDbVersion, string backendUrl)
        {
            // Fixture setup
            var actionToRecord = _fixture.Create<XmlDbUpdateRecordedAction>();

            // Exercise system
            IEnumerable<XElement> actualElements;
            using (new NonQpEnvironmentContext(null))
            {
                var resultXmlDocument = XmlDbUpdateSerializerHelpers.SerializeAction(actionToRecord, currentDbVersion, backendUrl);
                actualElements = resultXmlDocument.XPathSelectElements("/actions");
            }

            // Verify outcome
            actualElements.Should().NotBeNullOrEmpty();
        }

        [Theory, AutoData, Trait("XmlDbUpdate", "ActionSerializing")]
        public void GetXmlDbUpdateRecordedAction_WhenSerializingForLogging_ShouldNotIncludeRootElement(string currentDbVersion, string backendUrl)
        {
            // Fixture setup
            var actionToRecord = _fixture.Create<XmlDbUpdateRecordedAction>();

            // Exercise system
            IEnumerable<XElement> actualElements;
            using (new NonQpEnvironmentContext(null))
            {
                var resultXmlDocument = XmlDbUpdateSerializerHelpers.SerializeAction(actionToRecord, currentDbVersion, backendUrl, true);
                actualElements = resultXmlDocument.XPathSelectElements("/actions");
            }

            // Verify outcome
            actualElements.Should().BeNullOrEmpty();
        }

        [Theory, AutoData, Trait("XmlDbUpdate", "ActionSerializing")]
        public void GetXmlDbUpdateRecordedAction_WhenSerializingForLogging_ShouldNotIncludeXmlDeclaration(string currentDbVersion, string backendUrl)
        {
            // Fixture setup
            var actionToRecord = _fixture.Create<XmlDbUpdateRecordedAction>();

            // Exercise system
            string resultXml;
            using (new NonQpEnvironmentContext(null))
            {
                resultXml = XmlDbUpdateSerializerHelpers.SerializeAction(actionToRecord, currentDbVersion, backendUrl, true).ToNormalizedString(true);
            }

            // Verify outcome
            Assert.False(resultXml.StartsWith("<?"));
        }

        [Theory, AutoData, Trait("XmlDbUpdate", "ActionSerializing")]
        public void GetXmlDbUpdateRecordedAction_WhenSerializing_ShouldIncludeFormElements(string currentDbVersion, string backendUrl)
        {
            // Fixture setup
            var actionToRecord = _fixture.Create<XmlDbUpdateRecordedAction>();
            var expectedElementsCount = actionToRecord.Form.Count;

            // Exercise system
            int actualElementsCount;
            using (new NonQpEnvironmentContext(null))
            {
                var resultXmlDocument = XmlDbUpdateSerializerHelpers.SerializeAction(actionToRecord, currentDbVersion, backendUrl);
                actualElementsCount = resultXmlDocument.XPathSelectElements("//action/field").Count();
            }

            // Verify outcome
            Assert.Equal(expectedElementsCount, actualElementsCount);
        }
    }
}
