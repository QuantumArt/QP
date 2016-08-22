using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using Quantumart.QPublishing.Info;


namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class ReadFileFieldFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Field_FileFieldUrl_isGenerated([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                Assert.That(items.FileItemUrl, Is.Not.Null);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Field_FileFieldUploadPath_isGenerated([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                Assert.That(items.FileItemUploadPath, Is.Not.Null);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUrl_isCorrect_Fill([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                var pref = context.Cnn.GetUploadUrlPrefix(context.SiteId);
                var url = ExpectedUploadUrl(pref, items.FileItem);
                Assert.That(items.FileItemUrl, Is.EqualTo(url));
            }
        }

        private static string EXPECTED_UPLOAD_PATH = @"C:\Inetpub\stageroot\tele2\upload\";
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUploadPath_isCorrect_Fill([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                Assert.That(items.FileItemUploadPath, Is.EqualTo(EXPECTED_UPLOAD_PATH));
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUrl_Changed([Values(ContentAccess.Stage)] ContentAccess access, [Values(Mapping.FileDefaultMapping)] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var pref = context.Cnn.GetUploadUrlPrefix(context.SiteId);
                var items = context.FileFieldsItems.FirstOrDefault();
                if (string.IsNullOrEmpty(pref))
                {
                    Assert.That(items.FileItemUrl, Does.StartWith("/upload"));
                }
                else
                {
                    Assert.That(items.FileItemUrl, Does.StartWith("http").Or.StartWith("//"));
                }                
            }
        }


        private static string UploadUrlRel = "/upload/contents/628/";
        private static string UploadUrlAbs = @"http://static.tele2.dev.qsupport.ru/upload/contents/628/";
        private string ExpectedUploadUrl(string pref, string fileName)
        {
            if (string.IsNullOrEmpty(pref))
            {
                return UploadUrlRel + fileName;
            }
            else
            {
                return UploadUrlAbs + fileName;
            }
        }
    }
}
