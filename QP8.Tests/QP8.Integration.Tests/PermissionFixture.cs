using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using System.Configuration;
using System.Reflection;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class PermissionFixture
    {
        private const string username = "test";
        private const string password = "1Qaz2Wsx";
        private const string firstName = "testFirstName";
        private const string lastName = "testLastName";
        private const string email = "testEmail@test.ru";
        private const string columnIdName = "user_id";
        private static int groupId = 1;

        public static DBConnector Cnn { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            var settings = typeof(ConfigurationElementCollection)
                    .GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            settings.SetValue(ConfigurationManager.ConnectionStrings, false);
            ConfigurationManager.ConnectionStrings.Add(new ConnectionStringSettings("qp_database", Global.ConnectionString, "System.Data.SqlClient"));
        }

        [Test]
        public void AuthenticateUser()
        {
            var id = 0;
            Assert.DoesNotThrow(() =>
            {
                id = Permissions.AddUser(username, password, 0, firstName, lastName, email);
            }, "Add User");

            //authenticate user
            var auth = 0;
            Assert.DoesNotThrow(() =>
            {
                auth = Permissions.AuthenticateUser(username, password);
            }, "Authenticate");
            Assert.That(auth, Is.Not.EqualTo(0));

            //add user to group
            Assert.DoesNotThrow(() =>
            {
                Permissions.AddUserToGroup(id, groupId);
            }, "Add user to group");

            //remove user from group
            Assert.DoesNotThrow(() =>
            {
                Permissions.RemoveUserFromGroup(id, groupId);
            }, "Remove user from group");
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            var id = 0;
            var userInfo = Permissions.GetUserInfo(username);
            if (userInfo.Rows.Count > 0)
            {
                id = Convert.ToInt32(userInfo.Rows[0][columnIdName]);
                Permissions.RemoveUser(id);
            }
        }
    }
}
