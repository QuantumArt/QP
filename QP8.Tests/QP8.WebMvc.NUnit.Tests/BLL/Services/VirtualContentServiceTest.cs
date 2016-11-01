using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.Results;

namespace QP8.WebMvc.NUnit.Tests.BLL.Services
{
    [TestClass]
    public class VirtualContentServiceTest
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Пустой id -> id = 0
        ///</summary>
        [TestMethod]
        public void EmptyFieldID_ReturnZero_ParseFieldIdTest()
        {
            var entityId = string.Empty;
            const int expected = 0;
            var actual = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// [100] -> 100
        ///</summary>
        [TestMethod]
        public void SingleFakeFieldID_ReturnId_ParseFieldIdTest()
        {
            const string entityId = "[100]";
            const int expected = 100;
            var actual = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// [100] -> 100
        ///</summary>
        [TestMethod]
        public void MultiplyFakeFieldID_ReturnId_ParseFieldIdTest()
        {
            const string entityId = "[100.200.300.400]";
            const int expected = 400;
            var actual = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// 100 -> FormatException
        ///</summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void RealFieldID_ReturnId_ParseFieldIdTest()
        {
            const string entityId = "100";
            const int expected = 100;
            var actual = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// [100 -> FormatException
        ///</summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BadFormatId1_ThrowFormatException_ParseFieldIdTest()
        {
            const string entityId = "[100";
            Content.VirtualFieldNode.ParseFieldTreeId(entityId);
        }

        /// <summary>
        /// dksajdkas -> FormatException
        ///</summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void BadFormatId2_ThrowFormatException_ParseFieldIdTest()
        {
            const string entityId = "dksajdkas";
            Content.VirtualFieldNode.ParseFieldTreeId(entityId);
        }

        [TestMethod]
        public void EmptyParentTreeId_ReturnCorrectZeroLevelTreeId_GetFieldTreeIdTest()
        {
            const string expected = "[100]";
            var actual = Content.VirtualFieldNode.GetFieldTreeId(100);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NotEmptyParentTreeId_ReturnCorrectLevelTreeId_GetFieldTreeIdTest()
        {
            const string expected = "[100.200.300]";
            var actual = Content.VirtualFieldNode.GetFieldTreeId(300, "[100.200]");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NodeHasNotParent_ReturnNull_GetParentFieldTreeIdTest()
        {
            var actual = Content.VirtualFieldNode.GetParentFieldTreeId("[100]");
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void NodeHasParent_ReturnParentTreeId_GetParentFieldTreeIdTest()
        {
            const string expected = "[100.200.3.5.70]";
            var actual = Content.VirtualFieldNode.GetParentFieldTreeId("[100.200.3.5.70.100]");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RootLevelFieldName_ReturnSameName_GetPersistentFieldNameTest()
        {
            const string expected = "Title";
            var helper = new VirtualContentHelper();
            var actual = helper.GetPersistentFieldName("Title");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NotRootLevelFieldName_ReturnLastPointSplitedName_GetPersistentFieldNameTest()
        {
            const string expected = "Title";
            var helper = new VirtualContentHelper();
            var actual = helper.GetPersistentFieldName("m1.m2.m3.m4.Title");
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void RootLevelFieldName_CorrectResult_ReplacePersistentFieldNameTest()
        {
            const string expected = "Header";
            var helper = new VirtualContentHelper();
            var actual = helper.ReplacePersistentFieldName("Title", "Header");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NotRootLevelFieldName_CorrectResult_ReplacePersistentFieldNameTest()
        {
            const string expected = "m1.m2.m3.m4.Header";
            var helper = new VirtualContentHelper();
            var actual = helper.ReplacePersistentFieldName("m1.m2.m3.m4.Title", "Header");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AreNotParentIds_AddRequiredParentIds_NormalizeFieldTreeIdSeqTest()
        {
            var expected = new[] { "[100]", "[100.200]", "[100.200.300]", "[400]", "[400.500]", "[400.500.600]", "[700]" };
            var argument = new[] { "[100.200.300]", "[400.500]", "[400.500.600]", "[700]" };
            var actual = Content.VirtualFieldNode.NormalizeFieldTreeIdSeq(argument);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Count());
            Assert.AreEqual(0, expected.Except(actual).Count());
        }

        [TestMethod]
        public void EveryHasParentIds_DoesntAddAnyIds_NormalizeFieldTreeIdSeqTest()
        {
            var expected = new[] { "[100]", "[100.200]", "[100.200.300]", "[400]", "[400.500]", "[400.500.600]", "[700]" };
            var argument = expected;
            var actual = Content.VirtualFieldNode.NormalizeFieldTreeIdSeq(argument);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Length, actual.Count());
            Assert.AreEqual(0, expected.Except(actual).Count());
        }

        [TestMethod]
        public void ThereIsVirtualFileds_ReturnJoinVirtualContentViewDLLStatment_GenerateCreateJoinViewDDLTest()
        {
            var virtualFieldsData = new[]
            {
                new VirtualFieldData{Id = 1180, Name = "Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = null },
                new VirtualFieldData{Id = 1189, Name = "fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = null },

                    new VirtualFieldData{Id = 1181, Name = "m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = null },
                    new VirtualFieldData{Id = 1182, Name = "m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1181 },
                    new VirtualFieldData{Id = 1188, Name = "m2m.fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = 1181 },

                        new VirtualFieldData{Id = 1183, Name = "m2m.m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = 1181 },
                        new VirtualFieldData{Id = 1184, Name = "m2m.m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1183 },
                        new VirtualFieldData{Id = 1187, Name = "m2m.m2m.fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = 1183 },

                            new VirtualFieldData{Id = 1185, Name = "kdsfjkdsfjk.m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = 1183 },
                            new VirtualFieldData{Id = 1186, Name = "m2m.m2m.m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1185 },

                    new VirtualFieldData{Id = 1190, Name = "o2o", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "o2o", RelateToPersistentContentId = 300, JoinId = null },
                        new VirtualFieldData{Id = 1191, Name = "o2o.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1190 }

            };

            const int joinRootContentId = 295;
            const int virtualContentId = 500;

            const string expected = "CREATE VIEW [dbo].[content_500] AS " +
            "SELECT c_0.CONTENT_ITEM_ID,c_0.STATUS_TYPE_ID,c_0.VISIBLE,c_0.ARCHIVE,c_0.CREATED,c_0.MODIFIED,c_0.LAST_MODIFIED_BY,c_0.[Title] as [Title],c_0.[fff] as [fff],c_0.[m2m] as [m2m]," +
            "c_0_0.[Title] as [m2m.Title],c_0_0.[fff] as [m2m.fff],c_0_0.[m2m] as [m2m.m2m]," +
            "c_0_0_0.[Title] as [m2m.m2m.Title],c_0_0_0.[fff] as [m2m.m2m.fff],c_0_0_0.[m2m] as [kdsfjkdsfjk.m2m]," +
            "c_0_0_0_0.[Title] as [m2m.m2m.m2m.Title]," +
            "c_0.[o2o] as [o2o],c_0_1.[Title] as [o2o.Title] " +
            "FROM dbo.CONTENT_295 AS c_0 " +
            "LEFT OUTER JOIN dbo.CONTENT_295 AS c_0_0 WITH (nolock) ON c_0_0.CONTENT_ITEM_ID = c_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_295 AS c_0_0_0 WITH (nolock) ON c_0_0_0.CONTENT_ITEM_ID = c_0_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_295 AS c_0_0_0_0 WITH (nolock) ON c_0_0_0_0.CONTENT_ITEM_ID = c_0_0_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_300 AS c_0_1 WITH (nolock) ON c_0_1.CONTENT_ITEM_ID = c_0.[o2o] ";

            var helper = new VirtualContentHelper();
            var actual = helper.GenerateCreateJoinViewDdl(virtualContentId, joinRootContentId, virtualFieldsData);

            Assert.IsTrue(actual.Equals(expected, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void ThereIsVirtualFileds_ReturnJoinVirtualContentAsyncViewDLLStatment_GenerateCreateJoinAsyncViewDDLTest()
        {
            var virtualFieldsData = new[]
            {
                new VirtualFieldData{Id = 1180, Name = "Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = null },
                new VirtualFieldData{Id = 1189, Name = "fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = null },

                    new VirtualFieldData{Id = 1181, Name = "m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = null },
                    new VirtualFieldData{Id = 1182, Name = "m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1181 },
                    new VirtualFieldData{Id = 1188, Name = "m2m.fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = 1181 },

                        new VirtualFieldData{Id = 1183, Name = "m2m.m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = 1181 },
                        new VirtualFieldData{Id = 1184, Name = "m2m.m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1183 },
                        new VirtualFieldData{Id = 1187, Name = "m2m.m2m.fff", Type = 1, PersistentContentId = 295, PersistentId = 1159, PersistentName = "fff", RelateToPersistentContentId = null, JoinId = 1183 },

                            new VirtualFieldData{Id = 1185, Name = "kdsfjkdsfjk.m2m", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "m2m", RelateToPersistentContentId = 295, JoinId = 1183 },
                            new VirtualFieldData{Id = 1186, Name = "m2m.m2m.m2m.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1185 },

                    new VirtualFieldData{Id = 1190, Name = "o2o", Type = 11, PersistentContentId = 295, PersistentId = 1158, PersistentName = "o2o", RelateToPersistentContentId = 300, JoinId = null },
                        new VirtualFieldData{Id = 1191, Name = "o2o.Title", Type = 1, PersistentContentId = 295, PersistentId = 1152, PersistentName = "Title", RelateToPersistentContentId = null, JoinId = 1190 }

            };

            const int joinRootContentId = 295;
            const int virtualContentId = 500;

            const string expected = "CREATE VIEW [dbo].[content_500_async] AS " +
            "SELECT c_0.CONTENT_ITEM_ID,c_0.STATUS_TYPE_ID,c_0.VISIBLE,c_0.ARCHIVE,c_0.CREATED,c_0.MODIFIED,c_0.LAST_MODIFIED_BY,c_0.[Title] as [Title],c_0.[fff] as [fff],c_0.[m2m] as [m2m]," +
            "c_0_0.[Title] as [m2m.Title],c_0_0.[fff] as [m2m.fff],c_0_0.[m2m] as [m2m.m2m]," +
            "c_0_0_0.[Title] as [m2m.m2m.Title],c_0_0_0.[fff] as [m2m.m2m.fff],c_0_0_0.[m2m] as [kdsfjkdsfjk.m2m]," +
            "c_0_0_0_0.[Title] as [m2m.m2m.m2m.Title]," +
            "c_0.[o2o] as [o2o],c_0_1.[Title] as [o2o.Title] " +
            "FROM dbo.CONTENT_295_async AS c_0 " +
            "LEFT OUTER JOIN dbo.CONTENT_295_united AS c_0_0 WITH (nolock) ON c_0_0.CONTENT_ITEM_ID = c_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_295_united AS c_0_0_0 WITH (nolock) ON c_0_0_0.CONTENT_ITEM_ID = c_0_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_295_united AS c_0_0_0_0 WITH (nolock) ON c_0_0_0_0.CONTENT_ITEM_ID = c_0_0_0.[m2m] " +
            "LEFT OUTER JOIN dbo.CONTENT_300_united AS c_0_1 WITH (nolock) ON c_0_1.CONTENT_ITEM_ID = c_0.[o2o] ";

            var helper = new VirtualContentHelper();
            var actual = helper.GenerateCreateJoinAsyncViewDdl(virtualContentId, joinRootContentId, virtualFieldsData);

            Assert.IsTrue(actual.Equals(expected, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void GenerateCreateUnionViewDdlTest()
        {
            const int contentId = 10;
            IEnumerable<int> unionSourceContentIDs = new[] { 1, 2 };
            IEnumerable<string> contentFieldNames = new[] { "f1", "f2", "f3", "f4" };
            var fieldNameInSourceContents = new Dictionary<string, HashSet<int>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"f1", new HashSet<int>(new[] {1})},
                {"f2", new HashSet<int>(new[] {1})},
                {"f3", new HashSet<int>(new[] {2})},
                {"f4", new HashSet<int>(new[] {2})}
            };

            const string expected = "CREATE VIEW [dbo].[content_10] AS " +
                                "SELECT 1 content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,[f1] [f1],[f2] [f2],NULL [f3],NULL [f4] FROM dbo.content_1 " +
                                "UNION ALL " +
                                "SELECT 2 content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,NULL [f1],NULL [f2],[f3] [f3],[f4] [f4] FROM dbo.content_2";

            var helper = new VirtualContentHelper();
            var actual = helper.GenerateCreateUnionViewDdl(contentId, unionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);

            Assert.AreEqual(expected, actual, true);
        }

        [TestMethod]
        public void GenerateCreateUnionAsyncViewDdlTest()
        {
            const int contentId = 10;
            IEnumerable<int> unionSourceContentIDs = new[] { 1, 2 };
            IEnumerable<string> contentFieldNames = new[] { "f1", "f2", "f3", "f4" };

            var fieldNameInSourceContents = new Dictionary<string, HashSet<int>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"f1", new HashSet<int>(new[] {1})},
                {"f2", new HashSet<int>(new[] {1})},
                {"f3", new HashSet<int>(new[] {2})},
                {"f4", new HashSet<int>(new[] {2})}
            };

            const string expected = "CREATE VIEW [dbo].[content_10_async] AS " +
                                "SELECT 1 content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,[f1] [f1],[f2] [f2],NULL [f3],NULL [f4] FROM dbo.content_1_async " +
                                "UNION ALL " +
                                "SELECT 2 content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,NULL [f1],NULL [f2],[f3] [f3],[f4] [f4] FROM dbo.content_2_async";

            var helper = new VirtualContentHelper();
            var actual = helper.GenerateCreateUnionAsyncViewDdl(contentId, unionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);

            Assert.AreEqual(expected, actual, true);
        }
    }
}
