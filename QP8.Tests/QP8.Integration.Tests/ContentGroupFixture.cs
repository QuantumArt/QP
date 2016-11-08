using System.Collections.Generic;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QPublishing.Database;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class ContentGroupFixture
    {
        private const string GroupName = "dsf";
        private const string NewGroupName = "bsd";
        private const int SpecificGroupId = 999;

        [Test]
        public void ReplayXML_CreateContentGroup_WithSpecifiedIdentity()
        {
            var appInfoRepository = new Mock<IApplicationInfoRepository>();
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            var actionsCorrecterService = new Mock<IXmlDbUpdateActionCorrecterService>();
            var httpContextProcessor = new Mock<IXmlDbUpdateHttpContextProcessor>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, new HashSet<string>(new[] { EntityTypeCode.ContentGroup }), 1, false, dbLogService.Object, appInfoRepository.Object, actionsCorrecterService.Object, httpContextProcessor.Object, false);
            Assert.DoesNotThrow(() => service.Process(Global.GetXml(@"xmls\group.xml")), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{GroupName}'"));
            Assert.That(id, Is.EqualTo(SpecificGroupId), "Specific id created");
            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{GroupName}'");
        }

        [Test]
        public void ReplayXML_CreateContentGroup_WithGeneratedIdentity()
        {
            var appInfoRepository = new Mock<IApplicationInfoRepository>();
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            var actionsCorrecterService = new Mock<IXmlDbUpdateActionCorrecterService>();
            var httpContextProcessor = new Mock<IXmlDbUpdateHttpContextProcessor>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, 1, false, dbLogService.Object, appInfoRepository.Object, actionsCorrecterService.Object, httpContextProcessor.Object, false);
            Assert.DoesNotThrow(() => service.Process(Global.GetXml(@"xmls\group.xml").Replace(GroupName, NewGroupName)), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{NewGroupName}'"));
            Assert.That(id, Is.Not.EqualTo(SpecificGroupId), "Generated id created");
            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{NewGroupName}'");
        }
    }
}
