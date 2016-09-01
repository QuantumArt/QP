using System.Linq;
using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using System;

namespace EntityFramework6.Test.Tests.ReadContentData
{
    [TestFixture]
    class ReadFileFieldFixture : DataContextFixtureBase
    {
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Field_FileFieldUrl_isGenerated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                Assert.That(items.FileItemUrl, Is.Not.Null);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_Field_FileFieldUploadPath_isGenerated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                Assert.That(items.FileItemUploadPath, Is.Not.Null);
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUrl_isCorrect_Fill([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                var pref = context.Cnn.GetUploadUrlPrefix(context.SiteId);
                var url = ExpectedUploadUrl(pref, items.FileItem, mapping);
                Assert.That(items.FileItemUrl, Is.EqualTo(url));
            }
        }

        private static string UPLOAD_PATH = @"C:\Inetpub\stageroot\tele2\upload\contents\";
        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUploadPath_isCorrect_Fill([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            using (var context = GetDataContext(access, mapping))
            {
                var items = context.FileFieldsItems.FirstOrDefault();
                var expectedUploadPath = UPLOAD_PATH + ValuesHelper.GetFileContentId(mapping);
                Assert.That(items.FileItemUploadPath, Is.EqualTo(expectedUploadPath));
            }
        }

        [Test, Combinatorial]
        [Category("ReadContentData")]
        public void Check_That_FileFieldUrl_Changed([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
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


        private static string UploadUrlRel = "/upload/contents/";
        private static string UploadUrlAbs = @"http://static.tele2.dev.qsupport.ru/upload/contents/";
        private string ExpectedUploadUrl(string pref, string fileName, Mapping mapping)
        {
            if (string.IsNullOrEmpty(pref))
            {
                return String.Format("{0}{1}/{2}", UploadUrlRel, ValuesHelper.GetFileContentId(mapping), fileName);
            }
            else
            {
                return String.Format("{0}{1}/{2}", UploadUrlAbs, ValuesHelper.GetFileContentId(mapping), fileName);
            }
        }
    }
}
