using System.Collections.Generic;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QPublishing.Database;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class ContentGroupTests
    {
        private const string GroupName = "dsf";
        private const string NewGroupName = "bsd";
        private const int SpecificGroupId = 999;

        [Test]
        public void ReplayXML_CreateContentGroup_WithSpecifiedIdentity()
        {
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Infrastructure.Global.ConnectionString, new HashSet<string>(new[] { EntityTypeCode.ContentGroup }), 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            Assert.DoesNotThrow(() => service.Process(Infrastructure.Global.GetXml(@"TestData\group.xml")), "Create content group");

            var cnn = new DBConnector(Infrastructure.Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{GroupName}'"));
            Assert.That(id, Is.EqualTo(SpecificGroupId), "Specific id created");

            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{GroupName}'");
        }

        [Test]
        public void ReplayXML_CreateContentGroup_WithGeneratedIdentity()
        {
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Infrastructure.Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            Assert.DoesNotThrow(() => service.Process(Infrastructure.Global.GetXml(@"TestData\group.xml").Replace(GroupName, NewGroupName)), "Create content group");

            var cnn = new DBConnector(Infrastructure.Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{NewGroupName}'"));
            Assert.That(id, Is.Not.EqualTo(SpecificGroupId), "Generated id created");

            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{NewGroupName}'");
        }
    }
}
