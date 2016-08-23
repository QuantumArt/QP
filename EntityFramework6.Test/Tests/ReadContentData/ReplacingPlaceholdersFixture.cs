using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using Quantumart.QP8.CodeGeneration.Services;
using Quantumart.QPublishing.Info;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class ReplacingPlaceholdersFixture : DataContextFixtureBase
    {
        private static Regex URL_PLACEHOLDER = new Regex(@"<%=upload_url%>|<%=site_url%>");

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_Replacing_Placeholder_IF_True([Values(ContentAccess.Stage)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_Replacing_Placeholder_IF_True([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping, Mapping.StaticMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                bool ReplaceUrls = context.Database.SqlQuery<bool>("SELECT TOP 1 REPLACE_URLS FROM SITE WHERE SITE_ID = @SiteId", new SqlParameter("SiteId", context.SiteId)).FirstOrDefault();
                if (ReplaceUrls)
                {
                    var item = context.ReplacingPlaceholdersItems.FirstOrDefault();
                    Match match = URL_PLACEHOLDER.Match(item.Title);

                    Assert.That(match.Length, Is.EqualTo(0));
                }
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        //public void Check_Replacing_Placeholder_IF_False([Values(ContentAccess.Stage)] ContentAccess access, [MappingValues] Mapping mapping)
        public void Check_Replacing_Placeholder_IF_False([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping, Mapping.StaticMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                bool ReplaceUrls = context.Database.SqlQuery<bool>("SELECT TOP 1 REPLACE_URLS FROM SITE WHERE SITE_ID = @SiteId", new SqlParameter("SiteId", context.SiteId)).FirstOrDefault();
                if (!ReplaceUrls)
                {
                    var item = context.ReplacingPlaceholdersItems.FirstOrDefault();
                    Match match = URL_PLACEHOLDER.Match(item.Title);
                    Assert.That(match.Length, Is.Not.EqualTo(0));
                }
            }
        }       
    }
}
