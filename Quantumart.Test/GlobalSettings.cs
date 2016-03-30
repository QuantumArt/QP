using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Quantumart.Test
{
    internal class GlobalSettings
    {
        public static string ConnectionString =
            @"Initial Catalog=mts_catalog;Data Source=mscsql01;Integrated Security=True;Application Name=UnitTest";

        public static string GetXml(string fileName)
        {
            var path = TestContext.CurrentContext.TestDirectory;
            return File.ReadAllText(Path.Combine(path, fileName));
        }

        public static int SiteId => 35;
    }
}
