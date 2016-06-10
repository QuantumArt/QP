using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using NUnit.Framework;
using Quantumart.QP8.Constants;

namespace Quantumart.Test
{
    [TestFixture]
    public class ContentGroupFixture
    {

        private const string groupName = "dsf";
        private const string newGroupName = "bsd";
        private const int specificGroupId = 999;

        [Test]
        public void ReplayXML_CreateContentGroup_WithSpecifiedIdentity()
        {
            QPContext.UseConnectionString = true;

            var identityOptions = new HashSet<string>(new[] {EntityTypeCode.ContentGroup});
            var service = new ReplayService(Global.ConnectionString, 1, null, identityOptions, true);
            Assert.DoesNotThrow(() => service.ReplayXml(Global.GetXml(@"xmls\group.xml")), "Create content group");
            DBConnector cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"select content_group_id from content_group where name = '{groupName}'"));
            Assert.That(id, Is.EqualTo(specificGroupId), "Specific id created");
            cnn.ProcessData($"delete from content_group where name = '{groupName}'");

            QPContext.UseConnectionString = false;
        }

        [Test]
        public void ReplayXML_CreateContentGroup_WithGeneratedIdentity()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(Global.ConnectionString, 1, true);
            Assert.DoesNotThrow(() => service.ReplayXml(Global.GetXml(@"xmls\group.xml").Replace(groupName, newGroupName)), "Create content group");
            DBConnector cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };
            var id = (decimal)cnn.GetRealScalarData(new SqlCommand($"select content_group_id from content_group where name = '{newGroupName}'"));
            Assert.That(id, Is.Not.EqualTo(specificGroupId), "Generated id created");
            cnn.ProcessData($"delete from content_group where name = '{newGroupName}'");

            QPContext.UseConnectionString = false;
        }

    }
}
