﻿using System;
using System.Linq;
using System.Data.Entity;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using System.IO;
//using Quantumart.QP8.EntityFramework.Services;

namespace EntityFramework6.Test.Tests
{
    [TestFixture]
    public class SystemInformationFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("Combinatorial")]
        public void DataContext_Users_Read([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var users = context.Users.Include(u => u.UserGroups).ToArray();
                Assert.That(users, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test, Combinatorial]
        [Category("Combinatorial")]
        public void DataContext_UserGroups_Read([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var groups = context.UserGroups.Include(g => g.Users).ToArray();
                Assert.That(groups, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test, Combinatorial]
        [Category("Combinatorial")]
        public void DataContext_StatusTypes_Read([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var statusTypes = context.StatusTypes.ToArray();
                Assert.That(statusTypes, Is.Not.Null.And.Not.Empty);
            }
        }
    }
}
