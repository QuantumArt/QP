using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using Quantumart.QP8.CodeGeneration.Services;
using Quantumart.QPublishing.Info;
using System.IO;
using System.Text.RegularExpressions;

namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class ReplacingPlaceholdersFixture : DataContextFixtureBase
    {
        private static Regex UPLOAD_URL_PLACEHOLDER = new Regex(@"<%=upload_url%>");
        private static Regex SITE_URL_PLACEHOLDER = new Regex(@"<%=site_url%>");

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_Replacing_Placeholder_IF_True([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var model = GetModel();
                var item = context.ReplacingPlaceholdersItems.FirstOrDefault();
                Match match = UPLOAD_URL_PLACEHOLDER.Match(item.Title);
                if (model.Schema.ReplaceUrls)
                {                   
                    Assert.That(match.Length, Is.EqualTo(0));
                }
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_Replacing_Placeholder_IF_False([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var model = GetModel();
                var item = context.ReplacingPlaceholdersItems.FirstOrDefault();
                Match match = UPLOAD_URL_PLACEHOLDER.Match(item.Title);
                if (!model.Schema.ReplaceUrls)
                {               
                    Assert.That(match.Length, Is.Not.EqualTo(0));
                }
            }
        }

        private ModelReader GetModel()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"DataContext\ModelMappingResult.xml");
            return new ModelReader(path, _ => { }, true);
        }
    }
}
