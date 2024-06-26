using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using QP8.Integration.Tests.Infrastructure;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
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

            var service = new XmlDbUpdateNonMvcReplayService(
                Global.ConnectionString,
                Global.DbType,
                new HashSet<string>(new[] { EntityTypeCode.ContentGroup }),
                1,
                false,
                dbLogService.Object,
                new ApplicationInfoRepository(),
                new XmlDbUpdateActionCorrecterService(
                    new ArticleService(new ArticleRepository(), new PathHelper(new DbService(new S3Options()))),
                    new ContentService(new ContentRepository(), new PathHelper(new DbService(new S3Options()))),
                    new ModelExpressionProvider(new EmptyModelMetadataProvider())
                ),
                new XmlDbUpdateHttpContextProcessor(),
                Global.Factory.Server.Host.Services,
                false
            );

            Assert.DoesNotThrow(() => service.Process(Global.GetXml($"TestData{Path.DirectorySeparatorChar}group.xml")), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString, Global.ClientDbType) { ForceLocalCache = true };
            var id = cnn.GetRealScalarData(cnn.CreateDbCommand($"SELECT content_group_id FROM content_group WHERE name = '{GroupName}'"));
            Assert.That(id, Is.EqualTo(SpecificGroupId), "Specific id created");

            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{GroupName}'");
        }

        [Test]
        public void ReplayXML_CreateContentGroup_WithGeneratedIdentity()
        {
            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(
                Global.ConnectionString,
                Global.DbType,
                null,
                1,
                false,
                dbLogService.Object,
                new ApplicationInfoRepository(),
                new XmlDbUpdateActionCorrecterService(
                    new ArticleService(new ArticleRepository(), new PathHelper(new DbService(new S3Options()))),
                    new ContentService(new ContentRepository(), new PathHelper(new DbService(new S3Options()))),
                    new ModelExpressionProvider(new EmptyModelMetadataProvider())
                ),
                new XmlDbUpdateHttpContextProcessor(),
                Global.Factory.Server.Host.Services,
                false
            );

            Assert.DoesNotThrow(() => service.Process(Global.GetXml($"TestData{Path.DirectorySeparatorChar}group.xml").Replace(GroupName, NewGroupName)), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString, Global.ClientDbType) { ForceLocalCache = true };
            var id = cnn.GetRealScalarData(cnn.CreateDbCommand($"SELECT content_group_id FROM content_group WHERE name = '{NewGroupName}'"));
            Assert.That(id, Is.Not.EqualTo(SpecificGroupId), "Generated id created");

            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{NewGroupName}'");
        }
    }
}
