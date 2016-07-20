using System.Collections.Generic;
using System.Data.SqlClient;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using NUnit.Framework;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;

namespace Quantumart.Test
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
            QPContext.UseConnectionString = true;

            var identityOptions = new HashSet<string>(new[] {EntityTypeCode.ContentGroup});
            var service = new XmlDbUpdateReplayService(Global.ConnectionString, identityOptions);
            Assert.DoesNotThrow(() => service.Process(Global.GetXml(@"xmls\group.xml")), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{GroupName}'"));
            Assert.That(id, Is.EqualTo(SpecificGroupId), "Specific id created");
            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{GroupName}'");
            QPContext.UseConnectionString = false;
        }

        [Test]
        public void ReplayXML_CreateContentGroup_WithGeneratedIdentity()
        {
            QPContext.UseConnectionString = true;

            var service = new XmlDbUpdateReplayService(Global.ConnectionString);
            Assert.DoesNotThrow(() => service.Process(Global.GetXml(@"xmls\group.xml").Replace(GroupName, NewGroupName)), "Create content group");
            var cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"SELECT content_group_id FROM content_group WHERE name = '{NewGroupName}'"));
            Assert.That(id, Is.Not.EqualTo(SpecificGroupId), "Generated id created");
            cnn.ProcessData($"DELETE FROM content_group WHERE name = '{NewGroupName}'");
            QPContext.UseConnectionString = false;
        }
    }
}
