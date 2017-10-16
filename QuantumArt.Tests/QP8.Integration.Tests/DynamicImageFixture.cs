using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Resizer;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class DynamicImageFixture
    {
        private const string ImageName = "BaseImage";

        public static int NoneId { get; private set; }

        public static int PublishedId { get; private set; }

        public static DBConnector DbConnector { get; private set; }

        public static int ContentId { get; private set; }

        public static string ContentName { get; private set; }

        public static int[] BaseArticlesIds { get; private set; }

        private class CopyFile : IEquatable<CopyFile>
        {
            private string From { get; }

            private string To { get; }

            public CopyFile(string from, string to)
            {
                From = from;
                To = to;
            }

            public bool Equals(CopyFile other) => From == other?.From && To == other?.To;

            public override bool Equals(object other) => other is CopyFile && Equals(other);

            public override int GetHashCode() => From.GetHashCode() + To.GetHashCode();

            public override string ToString() => $"From: {From}, To: {To}";
        }

        [OneTimeSetUp]
        public static void Init()
        {
            DbConnector = new DBConnector(Global.ConnectionString)
            {
                DynamicImageCreator = new FakeDynamicImage(),
                FileSystem = new FakeFileSystem(),
                ForceLocalCache = true
            };

            ContentName = "Test files";
            Clear();

            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            service.Process(Global.GetXml(@"xmls\files.xml"));
            ContentId = Global.GetContentId(DbConnector, ContentName);
            BaseArticlesIds = Global.GetIds(DbConnector, ContentId);
            NoneId = DbConnector.GetStatusTypeId(Global.SiteId, "None");
            PublishedId = DbConnector.GetStatusTypeId(Global.SiteId, "Published");
        }

        [Test]
        public void MassUpdate_CreateVersionDirectory_ContentHasFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;

            var actualPathes = new List<string>();
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>())).Callback<string>(path => { actualPathes.Add(path); });

            var ids = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };
            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString()
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString()
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");

            var paths = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            Assert.That(paths, Is.SubsetOf(actualPathes), "CreateDirectory calls");
        }

        [Test]
        public void AddFormToContent_CreateVersionDirectory_ContentHasFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;

            var actualPathes = new List<string>();
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>())).Callback<string>(path => { actualPathes.Add(path); });

            var id = 0;
            var imageName = DbConnector.FieldName(Global.SiteId, ContentName, "BaseImage");
            var article1 = new Hashtable
            {
                [imageName] = "testxx.jpg"
            };

            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0); }, "Create");

            var ids = new[] { id };
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, "Update");

            var paths = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            Assert.That(paths, Is.SubsetOf(actualPathes), "CreateDirectory calls");
        }

        [Test]
        public void MassUpdate_DoesntCreateVersions_CreateVersionsFalse()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                [ImageName] = "test44.jpg"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                [ImageName] = "test55.jpg"
            };

            values.Add(article2);
            var ids = new[] { int.Parse(article1[FieldName.ContentItemId]), int.Parse(article2[FieldName.ContentItemId]) };
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1, new MassUpdateOptions { CreateVersions = false }), "Update");
            var versions = Global.GetMaxVersions(DbConnector, ids).ToArray();

            Assert.That(versions, Is.Empty, "No new versions");
            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "No new folders");
        }

        [Test]
        public void MassUpdate_CopyFiles_ContentHasFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;

            var list = new List<CopyFile>();
            mockFileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((from, to) => { list.Add(new CopyFile(from, to)); });

            var values = new List<Dictionary<string, string>>();
            const string name1 = "test234";
            const string name2 = "test456";
            const string ext1 = "jpg";
            const string ext2 = "png";
            const string folder2 = "center";

            var ids = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                [ImageName] = $"{name1}.{ext1}"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                [ImageName] = $"{folder2}/{name2}.{ext2}"
            };

            values.Add(article2);
            var attrFolder = DbConnector.GetDirectoryForFileAttribute(DbConnector.FieldID(Global.SiteId, ContentName, ImageName));
            var currentVersionFolder = DbConnector.GetCurrentVersionFolderForContent(ContentId);
            var fileValuesBefore = Global.GetFieldValues<string>(DbConnector, ContentId, ImageName, ids);

            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            var paths = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            var file1 = new CopyFile(
                Path.Combine(currentVersionFolder, fileValuesBefore[0]),
                Path.Combine(paths[0], fileValuesBefore[0])
            );

            var file2 = new CopyFile(
                Path.Combine(currentVersionFolder, fileValuesBefore[1]),
                Path.Combine(paths[1], fileValuesBefore[1])
            );

            var file3 = new CopyFile(
                Path.Combine(attrFolder, article1[ImageName]),
                Path.Combine(currentVersionFolder, article1[ImageName])
            );

            var file4 = new CopyFile(
                Path.Combine(attrFolder, article2[ImageName].Replace(@"/", @"\")),
                Path.Combine(currentVersionFolder, $"{name2}.{ext2}")
            );

            Assert.That(list, Has.Member(file1), "Copy old file 1 to version dir");
            Assert.That(list, Has.Member(file2), "Copy old file 2 to version dir");
            Assert.That(list, Has.Member(file3), "Copy new file 1 to current dir");
            Assert.That(list, Has.Member(file4), "Copy new file 2 to current dir without subfolders");

            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            var paths2 = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            var file5 = new CopyFile(
                Path.Combine(currentVersionFolder, $"{name2}.{ext2}"),
                Path.Combine(paths2[1], $"{name2}.{ext2}")
            );

            Assert.That(list, Has.Member(file5), "Copy new file 2 to version dir without subfolders");
        }

        [Test]
        public void AddFormToContent_CopyFiles_ContentHasFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;

            var list = new List<CopyFile>();
            mockFileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((from, to) => { list.Add(new CopyFile(from, to)); });

            const string name1 = "test123";
            const string ext1 = "jpg";
            const string name2 = "test456";
            const string ext2 = "png";
            const string folder2 = "center";

            var imageName = DbConnector.FieldName(Global.SiteId, ContentName, "BaseImage");
            var article1 = new Hashtable
            {
                [imageName] = $"{name1}.{ext1}"
            };

            var article2 = new Hashtable
            {
                [imageName] = $"{folder2}/{name2}.{ext2}"
            };

            var id = 0;
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0); }, "Create");

            var ids = new[] { id };
            var attrFolder = DbConnector.GetDirectoryForFileAttribute(DbConnector.FieldID(Global.SiteId, ContentName, ImageName));
            var currentVersionFolder = DbConnector.GetCurrentVersionFolderForContent(ContentId);
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article2, id); }, "Update");

            var paths = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article2, id); }, "Update");

            var paths2 = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            var file1 = new CopyFile(
                Path.Combine(attrFolder, article1[imageName].ToString()),
                Path.Combine(currentVersionFolder, article1[imageName].ToString())
            );

            var file2 = new CopyFile(
                Path.Combine(currentVersionFolder, article1[imageName].ToString()),
                Path.Combine(paths[0], article1[imageName].ToString())
            );

            var file3 = new CopyFile(
                Path.Combine(attrFolder, article2[imageName].ToString().Replace(@"/", @"\")),
                Path.Combine(currentVersionFolder, $"{name2}.{ext2}")
            );

            var file4 = new CopyFile(
                Path.Combine(currentVersionFolder, $"{name2}.{ext2}"),
                Path.Combine(paths2[0], $"{name2}.{ext2}")
            );

            Assert.That(list, Has.Member(file1), "Copy old file to current dir");
            Assert.That(list, Has.Member(file2), "Copy old file to version dir");
            Assert.That(list, Has.Member(file3), "Copy new file to current dir without subfolders");
            Assert.That(list, Has.Member(file4), "Copy new file to version dir without subfolders");
        }

        [Test]
        public void MassUpdate_DoesntCreateVersionDirectory_EmptyFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0"
            };

            values.Add(article1);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Create");
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "Shouldn't be called for empty file fields");
        }

        [Test]
        public void AddFormToContent_DoesntCreateVersionDirectory_EmptyFileFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0"
            };

            values.Add(article1);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Create");
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "Shouldn't be called for empty file fields");
        }

        [Test]
        public void MassUpdate_DoesntCreateVersionDirectory_DisableVersionControlFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));
            mockFileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                ["Video"] = "newtest.mp4"
            };

            values.Add(article1);

            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Create");
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");

            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "Shouldn't be called for disabled versions control fields");
            mockFileSystem.Verify(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never(), "Shouldn't be called for disabled versions control fields");
        }

        [Test]
        public void AddFormToContent_DoesntCreateVersionDirectory_DisableVersionControlFields()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            mockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));
            mockFileSystem.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()));

            var article1 = new Hashtable
            {
                ["Video"] = "newtest.mp4"
            };

            var id = 0;
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0); }, "Create");

            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, "Update");

            mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never(), "Shouldn't be called for disabled versions control fields");
            mockFileSystem.Verify(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never(), "Shouldn't be called for disabled versions control fields");
        }

        [Test]
        public void MassUpdate_RemoveVersionDirectory_VersionOverflow()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;

            var actualPathes = new List<string>();
            mockFileSystem.Setup(x => x.RemoveDirectory(It.IsAny<string>())).Callback<string>(path => { actualPathes.Add(path); });

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0",
                [ImageName] = "testxx.jpg"
            };

            values.Add(article1);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Create");

            var id = int.Parse(values[0][FieldName.ContentItemId]);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");

            var paths = Global.GetMaxVersions(DbConnector, new[] { id }).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            Assert.That(paths, Is.EqualTo(actualPathes), "RemoveDirectory calls");
        }

        [Test]
        public void AddFormToContent_RemoveVersionDirectory_VersionOverflow()
        {
            var mockFileSystem = new Mock<IFileSystem>();
            DbConnector.FileSystem = mockFileSystem.Object;
            var actualPathes = new List<string>();
            mockFileSystem.Setup(x => x.RemoveDirectory(It.IsAny<string>())).Callback<string>(path => { actualPathes.Add(path); });

            var imageName = DbConnector.FieldName(Global.SiteId, ContentName, "BaseImage");
            var article1 = new Hashtable
            {
                [imageName] = "testxx.jpg"
            };

            var id = 0;
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0); }, "Create");

            var ids = new[] { id };
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, "Update");

            var paths = Global.GetMaxVersions(DbConnector, ids).Select(n => DbConnector.GetVersionFolderForContent(ContentId, n)).ToArray();
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, "Update");

            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, id); }, "Update");

            Assert.That(paths, Is.EqualTo(actualPathes), "RemoveDirectory calls");
        }

        [Test]
        public void MassUpdate_CreateDynamicImages_UpdateBaseImage()
        {
            var mockDynamicImage = new Mock<IDynamicImage>();
            DbConnector.DynamicImageCreator = mockDynamicImage.Object;

            var actualImages = new List<DynamicImageInfo>();
            mockDynamicImage.Setup(x => x.CreateDynamicImage(It.IsAny<DynamicImageInfo>())).Callback<DynamicImageInfo>(info => { actualImages.Add(info); });

            var values = new List<Dictionary<string, string>>();
            const string name1 = "test789";
            const string name2 = "test321";
            const string ext1 = "jpg";
            const string ext2 = "png";
            const string folder2 = "cnt";

            var ids = new[] { BaseArticlesIds[0], BaseArticlesIds[1] };
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[0].ToString(),
                [ImageName] = $"{name1}.{ext1}",
                ["File"] = "test.docx"
            };

            values.Add(article1);
            var article2 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = BaseArticlesIds[1].ToString(),
                [ImageName] = $"{folder2}/{name2}.{ext2}"
            };

            values.Add(article2);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");

            const string imagePng = "PngImage";
            const string imageJpg = "JpgImage";
            const string imageGif = "GifImage";

            var pngs = Global.GetFieldValues<string>(DbConnector, ContentId, imagePng, ids);
            var jpgs = Global.GetFieldValues<string>(DbConnector, ContentId, imageJpg, ids);
            var gifs = Global.GetFieldValues<string>(DbConnector, ContentId, imageGif, ids);

            var pngId = DbConnector.FieldID(Global.SiteId, ContentName, imagePng);
            var jpgId = DbConnector.FieldID(Global.SiteId, ContentName, imageJpg);
            var gifId = DbConnector.FieldID(Global.SiteId, ContentName, imageGif);
            var fileId = DbConnector.FieldID(Global.SiteId, ContentName, "File");

            var dbPng1 = $"field_{pngId}/{name1}.PNG";
            var dbPng2 = $"field_{pngId}/{folder2}/{name2}.PNG";
            var dbJpg1 = $"field_{jpgId}/{name1}.JPG";
            var dbJpg2 = $"field_{jpgId}/{folder2}/{name2}.JPG";
            var dbGif1 = $"field_{gifId}/{name1}.GIF";
            var dbGif2 = $"field_{gifId}/{folder2}/{name2}.GIF";

            Assert.That(actualImages.Any(n => n.ImageName == article1[ImageName] && n.AttrId == pngId), Is.True, "Create png for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[ImageName] && n.AttrId == pngId), Is.True, "Create png for article 2");
            Assert.That(actualImages.Any(n => n.ImageName == article1[ImageName] && n.AttrId == jpgId), Is.True, "Create jpg for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[ImageName] && n.AttrId == jpgId), Is.True, "Create jpg for article 2");
            Assert.That(actualImages.Any(n => n.ImageName == article1[ImageName] && n.AttrId == gifId), Is.True, "Create gif for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[ImageName] && n.AttrId == gifId), Is.True, "Create gif for article 2");
            Assert.That(actualImages.Any(n => n.ImageName == article1["File"] && n.AttrId == fileId), Is.False, "File field is ignored");

            Assert.That(pngs[0], Is.EqualTo(dbPng1), "Dynamic image value for png in article 1");
            Assert.That(pngs[1], Is.EqualTo(dbPng2), "Dynamic image value for png in article 2");
            Assert.That(jpgs[0], Is.EqualTo(dbJpg1), "Dynamic image value for jpg in article 1");
            Assert.That(jpgs[1], Is.EqualTo(dbJpg2), "Dynamic image value for jpg in article 2");
            Assert.That(gifs[0], Is.EqualTo(dbGif1), "Dynamic image value for gif in article 1");
            Assert.That(gifs[1], Is.EqualTo(dbGif2), "Dynamic image value for gif in article 2");
        }

        [Test]
        public void AddFormToContent_CreateDynamicImages_ContentHasDynamicImages()
        {
            var mockDynamicImage = new Mock<IDynamicImage>();
            DbConnector.DynamicImageCreator = mockDynamicImage.Object;

            var actualImages = new List<DynamicImageInfo>();
            mockDynamicImage.Setup(x => x.CreateDynamicImage(It.IsAny<DynamicImageInfo>())).Callback<DynamicImageInfo>(info => { actualImages.Add(info); });

            const string name1 = "test789";
            const string name2 = "test321";
            const string ext1 = "jpg";
            const string ext2 = "png";
            const string folder2 = "cnt";

            var imageName = DbConnector.FieldName(Global.SiteId, ContentName, "BaseImage");
            var article1 = new Hashtable
            {
                [imageName] = $"{name1}.{ext1}"
            };

            var article2 = new Hashtable
            {
                [imageName] = $"{folder2}/{name2}.{ext2}"
            };

            var id = 0;
            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article1, 0); }, "Create");

            var ids = new[] { id };
            const string imagePng = "PngImage";
            const string imageJpg = "JpgImage";
            const string imageGif = "GifImage";

            var pngs1 = Global.GetFieldValues<string>(DbConnector, ContentId, imagePng, ids);
            var jpgs1 = Global.GetFieldValues<string>(DbConnector, ContentId, imageJpg, ids);
            var gifs1 = Global.GetFieldValues<string>(DbConnector, ContentId, imageGif, ids);

            Assert.DoesNotThrow(() => { id = DbConnector.AddFormToContent(Global.SiteId, ContentName, "Published", ref article2, id); }, "Update");

            var pngs2 = Global.GetFieldValues<string>(DbConnector, ContentId, imagePng, ids);
            var jpgs2 = Global.GetFieldValues<string>(DbConnector, ContentId, imageJpg, ids);
            var gifs2 = Global.GetFieldValues<string>(DbConnector, ContentId, imageGif, ids);

            var pngId = DbConnector.FieldID(Global.SiteId, ContentName, imagePng);
            var jpgId = DbConnector.FieldID(Global.SiteId, ContentName, imageJpg);
            var gifId = DbConnector.FieldID(Global.SiteId, ContentName, imageGif);

            var dbPng1 = $"field_{pngId}/{name1}.PNG";
            var dbPng2 = $"field_{pngId}/{folder2}/{name2}.PNG";
            var dbJpg1 = $"field_{jpgId}/{name1}.JPG";
            var dbJpg2 = $"field_{jpgId}/{folder2}/{name2}.JPG";
            var dbGif1 = $"field_{gifId}/{name1}.GIF";
            var dbGif2 = $"field_{gifId}/{folder2}/{name2}.GIF";

            Assert.That(actualImages.Any(n => n.ImageName == article1[imageName].ToString() && n.AttrId == pngId), Is.True, "Create png for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[imageName].ToString() && n.AttrId == pngId), Is.True, "Create png for article 2");
            Assert.That(actualImages.Any(n => n.ImageName == article1[imageName].ToString() && n.AttrId == jpgId), Is.True, "Create jpg for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[imageName].ToString() && n.AttrId == jpgId), Is.True, "Create jpg for article 2");
            Assert.That(actualImages.Any(n => n.ImageName == article1[imageName].ToString() && n.AttrId == gifId), Is.True, "Create gif for article 1");
            Assert.That(actualImages.Any(n => n.ImageName == article2[imageName].ToString() && n.AttrId == gifId), Is.True, "Create gif for article 2");

            Assert.That(pngs1[0], Is.EqualTo(dbPng1), "Dynamic image value for png in article 1");
            Assert.That(pngs2[0], Is.EqualTo(dbPng2), "Dynamic image value for png in article 2");
            Assert.That(jpgs1[0], Is.EqualTo(dbJpg1), "Dynamic image value for jpg in article 1");
            Assert.That(jpgs2[0], Is.EqualTo(dbJpg2), "Dynamic image value for jpg in article 2");
            Assert.That(gifs1[0], Is.EqualTo(dbGif1), "Dynamic image value for gif in article 1");
            Assert.That(gifs2[0], Is.EqualTo(dbGif2), "Dynamic image value for gif in article 2");
        }

        [Test]
        public void MassUpdate_DoesntCreateDynamicImages_EmptyBaseImage()
        {
            var mockDynamicImage = new Mock<IDynamicImage>();
            DbConnector.DynamicImageCreator = mockDynamicImage.Object;
            mockDynamicImage.Setup(x => x.CreateDynamicImage(It.IsAny<DynamicImageInfo>()));

            var values = new List<Dictionary<string, string>>();
            var article1 = new Dictionary<string, string>
            {
                [FieldName.ContentItemId] = "0"
            };

            values.Add(article1);
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Create");
            Assert.DoesNotThrow(() => DbConnector.MassUpdate(ContentId, values, 1), "Update");
            mockDynamicImage.Verify(x => x.CreateDynamicImage(It.IsAny<DynamicImageInfo>()), Times.Never(), "Shouldn't be called for empty image fields");
        }

        [TearDown]
        public static void TestTearDown()
        {
            DbConnector.FileSystem = new FakeFileSystem();
            DbConnector.DynamicImageCreator = new FakeDynamicImage();
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            ContentId = Global.GetContentId(DbConnector, ContentName);
            var srv = new ContentService(Global.ConnectionString, 1);
            if (srv.Exists(ContentId))
            {
                srv.Delete(ContentId);
            }
        }
    }
}
