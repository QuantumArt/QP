using System;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QPublishing.Database;
using ArticleApiService = Quantumart.QP8.BLL.Services.API.ArticleService;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using FieldService = Quantumart.QP8.BLL.Services.API.FieldService;

namespace QP8.Integration.Tests
{
    [TestFixture]
    public class BatchUpdateTests
    {
        public const string ArticlesNotFound = "Статьи для контента не найдены";
        public const string ValuesNotFound = "Значения не найдены";
        public const string BatchUpdateResultIncorrect = "Неверный результат";
        public const string CantReadArticle = "Не удалось прочитать статью";
        public const string NoClassifierField = "Не найдено поле классификатор";
        public const string WrongMtMRelation = "Неверно проставлена связь MtM";
        public const string WrongMtORelation = "Неверно проставлена связь MtO";
        public const string ContentDataIsEmpty = "Не заданы поля в CONTENT_DATA";

        public const string BaseContent = "Test_BatchUpdate_Base";
        public const string BaseFieldEx1 = "Field_Ex1";
        public const string BaseFieldEx2 = "Field_Ex2";
        public const string BaseFieldString = "Field_String";
        public const string BaseFieldMtM = "Field_MtM";
        public const string BaseFieldOtM = "Field_OtM";
        public const string BaseFieldNumericInteger = "Field_Numeric_Integer";
        public const string BaseFieldNumericDecimal = "Field_Numeric_Decimal";
        public const string BaseFieldDate = "Field_Date";
        public const string BaseFieldTime = "Field_Time";
        public const string BaseFieldDateTime = "Field_DateTime";
        public const string BaseFieldFile = "Field_File";
        public const string BaseFieldImage = "Field_Image";
        public const string BaseFieldTextBox = "Field_TextBox";
        public const string BaseFieldVisualEdit = "Field_VisualEdit";
        public const string BaseFieldDynamicImage = "Field_DynamicImage";
        public const string BaseFieldEnum = "Field_Enum";

        public const string DictionaryContent = "Test_BatchUpdate_Dictionary";
        public const string DictionaryKey = "Key";
        public const string DictionaryValue = "Value";
        public const string DictionaryFieldMtMBackward = "Field_MtM_Backward";
        public const string DictionaryFieldMtOBackward = "Field_MtO_Backward";

        public const string Ex11Content = "Test_BatchUpdate_Ex1_1";
        public const string Ex11Parent = "Parent";
        public const string Ex11Field1 = "Field1";
        public const string Ex11Field2 = "Field2";

        public const string Ex12Content = "Test_BatchUpdate_Ex1_2";
        public const string Ex12Parent = "Parent";
        public const string Ex12Field1 = "Field1";

        public const string Ex21Content = "Test_BatchUpdate_Ex2_1";
        public const string Ex21Parent = "Parent";

        public const string Ex22Content = "Test_BatchUpdate_Ex2_2";
        public const string Ex22Parent = "Parent";

        public static DBConnector DbConnector { get; private set; }

        public static ArticleApiService ArticleService { get; private set; }

        public static Random Random { get; private set; }

        public static int BaseContentId { get; private set; }

        public static int BaseFieldEx1Id { get; private set; }

        public static int BaseFieldEx2Id { get; private set; }

        public static int BaseFieldStringId { get; private set; }

        public static int BaseFieldMtMId { get; private set; }

        public static int BaseFieldOtMId { get; private set; }

        public static int BaseFieldNumericIntegerId { get; private set; }

        public static int BaseFieldNumericDecimalId { get; private set; }

        public static int BaseFieldDateId { get; private set; }

        public static int BaseFieldTimeId { get; private set; }

        public static int BaseFieldDateTimeId { get; private set; }

        public static int BaseFieldFileId { get; private set; }

        public static int BaseFieldImageId { get; private set; }

        public static int BaseFieldTextBoxId { get; private set; }

        public static int BaseFieldVisualEditId { get; private set; }

        public static int BaseFieldDynamicImageId { get; private set; }

        public static int BaseFieldEnumId { get; private set; }

        public static int DictionaryContentId { get; private set; }

        public static int DictionaryKeyId { get; private set; }

        public static int DictionaryValueId { get; private set; }

        public static int DictionaryFieldMtMBackwardId { get; private set; }

        public static int DictionaryFieldMtOBackwardId { get; private set; }

        public static int Ex11ContentId { get; private set; }

        public static int Ex11ParentId { get; private set; }

        public static int Ex11Field1Id { get; private set; }

        public static int Ex11Field2Id { get; private set; }

        public static int Ex12ContentId { get; private set; }

        public static int Ex12ParentId { get; private set; }

        public static int Ex12Field1Id { get; private set; }

        public static int Ex21ContentId { get; private set; }

        public static int Ex21ParentId { get; private set; }

        public static int Ex22ContentId { get; private set; }

        public static int Ex22ParentId { get; private set; }

        [OneTimeSetUp]
        public static void Init()
        {
            TestContext.WriteLine($"Using next database for tests: {Infrastructure.Global.DbName}");

            DbConnector = new DBConnector(Infrastructure.Global.ConnectionString) { ForceLocalCache = true };
            DictionaryContentId = Infrastructure.Global.GetContentId(DbConnector, DictionaryContent);
            BaseContentId = Infrastructure.Global.GetContentId(DbConnector, BaseContent);
            ArticleService = new ArticleApiService(Infrastructure.Global.ConnectionString, 1);
            Clear();

            var dbLogService = new Mock<IXmlDbUpdateLogService>();
            dbLogService.Setup(m => m.IsFileAlreadyReplayed(It.IsAny<string>())).Returns(false);
            dbLogService.Setup(m => m.IsActionAlreadyReplayed(It.IsAny<string>())).Returns(false);

            var service = new XmlDbUpdateNonMvcReplayService(Infrastructure.Global.ConnectionString, 1, false, dbLogService.Object, new ApplicationInfoRepository(), new XmlDbUpdateActionCorrecterService(new ArticleService(new ArticleRepository())), new XmlDbUpdateHttpContextProcessor(), false);
            service.Process(Infrastructure.Global.GetXml(@"TestData\batchupdate.xml"));

            Random = new Random();
            BaseContentId = Infrastructure.Global.GetContentId(DbConnector, BaseContent);
            InitBaseContentFields();
            InitExtensions();

            DictionaryContentId = Infrastructure.Global.GetContentId(DbConnector, DictionaryContent);
            InitDictionaryContentFields();
        }

        private static void InitExtensions()
        {
            Ex11ContentId = Infrastructure.Global.GetContentId(DbConnector, Ex11Content);
            Ex11ParentId = Infrastructure.Global.GetFieldId(DbConnector, Ex11Content, Ex11Parent);
            Ex11Field1Id = Infrastructure.Global.GetFieldId(DbConnector, Ex11Content, Ex11Field1);
            Ex11Field2Id = Infrastructure.Global.GetFieldId(DbConnector, Ex11Content, Ex11Field2);

            Ex12ContentId = Infrastructure.Global.GetContentId(DbConnector, Ex12Content);
            Ex12ParentId = Infrastructure.Global.GetFieldId(DbConnector, Ex12Content, Ex12Parent);
            Ex12Field1Id = Infrastructure.Global.GetFieldId(DbConnector, Ex12Content, Ex12Field1);

            Ex21ContentId = Infrastructure.Global.GetContentId(DbConnector, Ex21Content);
            Ex21ParentId = Infrastructure.Global.GetFieldId(DbConnector, Ex21Content, Ex21Parent);

            Ex22ContentId = Infrastructure.Global.GetContentId(DbConnector, Ex22Content);
            Ex22ParentId = Infrastructure.Global.GetFieldId(DbConnector, Ex22Content, Ex22Parent);
        }

        private static void InitDictionaryContentFields()
        {
            DictionaryKeyId = Infrastructure.Global.GetFieldId(DbConnector, DictionaryContent, DictionaryKey);
            DictionaryValueId = Infrastructure.Global.GetFieldId(DbConnector, DictionaryContent, DictionaryValue);
            DictionaryFieldMtMBackwardId = Infrastructure.Global.GetFieldId(DbConnector, DictionaryContent, DictionaryFieldMtMBackward);
            DictionaryFieldMtOBackwardId = Infrastructure.Global.GetFieldId(DbConnector, DictionaryContent, DictionaryFieldMtOBackward);
        }

        private static void InitBaseContentFields()
        {
            BaseFieldEx1Id = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldEx1);
            BaseFieldEx2Id = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldEx2);
            BaseFieldStringId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldString);
            BaseFieldMtMId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldMtM);
            BaseFieldOtMId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldOtM);
            BaseFieldNumericIntegerId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldNumericInteger);
            BaseFieldNumericDecimalId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldNumericDecimal);
            BaseFieldDateId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldDate);
            BaseFieldTimeId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldTime);
            BaseFieldDateTimeId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldDateTime);
            BaseFieldFileId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldFile);
            BaseFieldImageId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldImage);
            BaseFieldTextBoxId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldTextBox);
            BaseFieldVisualEditId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldVisualEdit);
            BaseFieldDynamicImageId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldDynamicImage);
            BaseFieldEnumId = Infrastructure.Global.GetFieldId(DbConnector, BaseContent, BaseFieldEnum);
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            Clear();
        }

        private static void Clear()
        {
            var contentService = new ContentService(Infrastructure.Global.ConnectionString, 1);
            var baseContentExists = contentService.Exists(BaseContentId);
            var dictionaryContentExists = contentService.Exists(DictionaryContentId);
            var fieldService = new FieldService(Infrastructure.Global.ConnectionString, 1);

            if (dictionaryContentExists)
            {
                InitDictionaryContentFields();
                var dictionaryIds = Infrastructure.Global.GetIds(DbConnector, DictionaryContentId);
                ArticleService.Delete(DictionaryContentId, dictionaryIds);
            }

            if (baseContentExists)
            {
                InitBaseContentFields();
                InitExtensions();

                var baseIds = Infrastructure.Global.GetIds(DbConnector, BaseContentId);
                ArticleService.Delete(BaseContentId, baseIds);
                contentService.Delete(Ex11ContentId);
                contentService.Delete(Ex12ContentId);
                contentService.Delete(Ex21ContentId);
                contentService.Delete(Ex22ContentId);
            }

            if (dictionaryContentExists)
            {
                fieldService.Delete(DictionaryFieldMtMBackwardId);
                fieldService.Delete(DictionaryFieldMtOBackwardId);
            }

            if (baseContentExists)
            {
                fieldService.Delete(BaseFieldMtMId);
                fieldService.Delete(BaseFieldOtMId);
            }

            if (dictionaryContentExists)
            {
                contentService.Delete(DictionaryContentId);
            }

            if (baseContentExists)
            {
                contentService.Delete(BaseContentId);
            }
        }

        [Test]
        public void BatchUpdate_DictionaryContent_RecoverContentData()
        {
            var random = Guid.NewGuid().ToString();
            var key = "key_" + random;
            var value = "value_" + random;
            var data = new[]
            {
                new ArticleData
                {
                    ContentId = DictionaryContentId,
                    Id = -1,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = DictionaryKeyId,
                            Value = string.Empty
                        },
                        new FieldData
                        {
                            Id = DictionaryValueId,
                            Value = string.Empty
                        },
                        new FieldData
                        {
                            Id = DictionaryFieldMtMBackwardId
                        },
                        new FieldData
                        {
                            Id = DictionaryFieldMtOBackwardId
                        }
                    }.ToList()
                }
            };

            var result = ArticleService.BatchUpdate(data);
            Assert.That(result, Is.Not.Null, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));

            var articleResult = result[0];
            Assert.That(articleResult.ContentId, Is.EqualTo(DictionaryContentId));
            Assert.That(articleResult.CreatedArticleId, Is.Not.EqualTo(articleResult.OriginalArticleId));

            data[0].Id = articleResult.CreatedArticleId;
            data[0].Fields[0].Value = key;
            data[0].Fields[1].Value = value;

            Infrastructure.Global.ClearContentData(DbConnector, articleResult.CreatedArticleId);
            result = ArticleService.BatchUpdate(data);
            Assert.That(result, Is.Not.Null.And.Empty, BatchUpdateResultIncorrect);

            var contentData = Infrastructure.Global.GetContentData(DbConnector, articleResult.CreatedArticleId);
            Assert.That(contentData, Is.Not.Null);
            Assert.That(contentData, Has.Length.EqualTo(4), ContentDataIsEmpty);

            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var article = ArticleService.Read(articleResult.CreatedArticleId);
                Assert.That(article, Is.Not.Null, CantReadArticle);

                var keyFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryKey);
                Assert.That(keyFv, Is.Not.Null);
                Assert.That(key, Is.EqualTo(keyFv.Value));
                Assert.That(contentData.Any(itm => itm.FieldId == DictionaryKeyId && itm.Data == key));

                var valueFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryValue);
                Assert.That(valueFv, Is.Not.Null);
                Assert.That(value, Is.EqualTo(valueFv.Value));
                Assert.That(contentData.Any(itm => itm.FieldId == DictionaryValueId && itm.Data == value));

                var mtoValue = GetFieldValue<decimal?>(DictionaryContentId, DictionaryFieldMtOBackward, articleResult.CreatedArticleId);
                Assert.That(mtoValue, Is.EqualTo(BaseFieldOtMId), WrongMtORelation);
                Assert.That(contentData.Any(itm => itm.FieldId == DictionaryFieldMtOBackwardId && itm.Data == BaseFieldOtMId.ToString(CultureInfo.InvariantCulture)), WrongMtORelation);

                var mtmValue = GetFieldValue<decimal?>(DictionaryContentId, DictionaryFieldMtMBackward, articleResult.CreatedArticleId);
                Assert.That(mtmValue, Is.Not.Null, WrongMtMRelation);

                var mtmFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryFieldMtMBackward);
                Assert.That(mtmFv, Is.Not.Null);
                Assert.That(mtmFv.Field.Default, Is.EqualTo(mtmValue.Value.ToString(CultureInfo.InvariantCulture)), WrongMtMRelation);
                Assert.That(contentData.Any(itm => itm.FieldId == DictionaryFieldMtMBackwardId && itm.Data == mtmFv.Field.Default), WrongMtMRelation);

                ArticleService.Delete(article.Id);
            }
        }

        [Test]
        public void BatchUpdate_DictionaryContent_InsertArticle()
        {
            var random = Guid.NewGuid().ToString();
            var key = "key_" + random;
            var value = "value_" + random;
            var data = new[]
            {
                new ArticleData
                {
                    ContentId = DictionaryContentId,
                    Id = -1,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = DictionaryKeyId,
                            Value = key
                        },
                        new FieldData
                        {
                            Id = DictionaryValueId,
                            Value = value
                        },
                        new FieldData
                        {
                            Id = DictionaryFieldMtMBackwardId
                        },
                        new FieldData
                        {
                            Id = DictionaryFieldMtOBackwardId
                        }
                    }.ToList()
                }
            };

            var result = ArticleService.BatchUpdate(data);

            Assert.That(result, Is.Not.Null, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));
            var articleResult = result[0];
            Assert.That(DictionaryContentId, Is.EqualTo(articleResult.ContentId));
            Assert.That(articleResult.OriginalArticleId, Is.Not.EqualTo(articleResult.CreatedArticleId));

            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var article = ArticleService.Read(articleResult.CreatedArticleId);
                Assert.That(article, Is.Not.Null, CantReadArticle);

                var keyFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryKey);
                Assert.That(keyFv, Is.Not.Null);
                Assert.That(key, Is.EqualTo(keyFv.Value));

                var valueFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryValue);
                Assert.That(valueFv, Is.Not.Null);
                Assert.That(value, Is.EqualTo(valueFv.Value));

                var mtoValue = GetFieldValue<decimal?>(DictionaryContentId, DictionaryFieldMtOBackward, articleResult.CreatedArticleId);
                Assert.That(mtoValue, Is.EqualTo(BaseFieldOtMId), WrongMtORelation);

                var mtmValue = GetFieldValue<decimal?>(DictionaryContentId, DictionaryFieldMtMBackward, articleResult.CreatedArticleId);
                Assert.That(mtmValue, Is.Not.Null, WrongMtMRelation);
                var mtmFv = article.FieldValues.Single(fv => fv.Field.Name == DictionaryFieldMtMBackward);
                Assert.That(mtmFv, Is.Not.Null);
                Assert.That(mtmFv.Field.Default, Is.EqualTo(mtmValue.Value.ToString(CultureInfo.InvariantCulture)), WrongMtMRelation);

                ArticleService.Delete(article.Id);
            }
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateExtensionField()
        {
            var articleId = GetArticleId(BaseContentId);
            ClearClassifierField(articleId);

            var data = new[]
            {
                new ArticleData
                {
                    ContentId = BaseContentId,
                    Id = articleId,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = BaseFieldEx2Id,
                            Value = Ex21ContentId.ToString(),
                            ArticleIds = new[] { -1 }
                        }
                    }.ToList()
                },
                new ArticleData
                {
                    ContentId = Ex21ContentId,
                    Id = -1
                }
            };

            var result = ArticleService.BatchUpdate(data);

            Assert.That(result, Is.Not.Null, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));

            var extensionResult = result[0];
            Assert.That(extensionResult.ContentId, Is.EqualTo(Ex21ContentId));
            Assert.That(extensionResult.OriginalArticleId, Is.Not.EqualTo(extensionResult.CreatedArticleId));

            var parentValues = Infrastructure.Global.GetFieldValues<decimal>(DbConnector, Ex21ContentId, Ex21Parent, new[] { extensionResult.CreatedArticleId });
            Assert.That(parentValues, Is.Not.Null, ValuesNotFound);
            Assert.That(parentValues, Has.Length.EqualTo(1));

            var parentValue = parentValues[0];
            Assert.That(articleId, Is.EqualTo(parentValue));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateStringField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(BaseContentId, BaseFieldStringId, BaseFieldString, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateEnumField()
        {
            var value = "Key1";
            UpdateField(BaseContentId, BaseFieldEnumId, BaseFieldEnum, value, value);

            value = "Key2";
            UpdateField(BaseContentId, BaseFieldEnumId, BaseFieldEnum, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateFileField()
        {
            var value = Guid.NewGuid() + ".txt";
            UpdateField(BaseContentId, BaseFieldFileId, BaseFieldFile, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateImageField()
        {
            var value = Guid.NewGuid() + ".png";
            UpdateField(BaseContentId, BaseFieldImageId, BaseFieldImage, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDynamicImageField()
        {
            var value = Guid.NewGuid() + ".png";
            UpdateField(BaseContentId, BaseFieldDynamicImageId, BaseFieldDynamicImage, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateTextBoxField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(BaseContentId, BaseFieldTextBoxId, BaseFieldTextBox, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateVisualEditField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(BaseContentId, BaseFieldVisualEditId, BaseFieldVisualEdit, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateNumericIntegerField()
        {
            const int range = 1000;
            decimal value = Random.Next(-range, range);
            UpdateField<object>(BaseContentId, BaseFieldNumericIntegerId, BaseFieldNumericInteger, value, value.ToString(CultureInfo.GetCultureInfo(1049)));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateNumericDecimalField()
        {
            const int range = 100000;
            decimal value = Random.Next(-range, range);
            value /= 100;
            UpdateField<object>(BaseContentId, BaseFieldNumericDecimalId, BaseFieldNumericDecimal, value, value.ToString(CultureInfo.GetCultureInfo(1049)));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDateTimeField()
        {
            var value = DateTime.Now;
            value = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
            UpdateField(BaseContentId, BaseFieldDateTimeId, BaseFieldDateTime, value, value.ToString(CultureInfo.GetCultureInfo(1049)));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDateField()
        {
            var value = DateTime.Today;
            UpdateField(BaseContentId, BaseFieldDateId, BaseFieldDate, value, value.ToString(CultureInfo.GetCultureInfo(1049)));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateTimeField()
        {
            var value = DateTime.Now;
            value = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
            UpdateField(BaseContentId, BaseFieldTimeId, BaseFieldTime, value, value.ToString(CultureInfo.GetCultureInfo(1049)));
        }

        private static void UpdateField<T>(int contentId, int fieldId, string fieldName, T value, string stringValue)
        {
            var articleId = GetArticleId(contentId);
            var data = new[]
            {
                new ArticleData
                {
                    ContentId = contentId,
                    Id = articleId,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = fieldId,
                            Value = stringValue
                        }
                    }.ToList()
                }
            };

            var result = ArticleService.BatchUpdate(data);
            Assert.That(result, Is.Not.Null.And.Empty, BatchUpdateResultIncorrect);

            var newValue = GetFieldValue<T>(contentId, fieldName, articleId);
            Assert.That(value, Is.EqualTo(newValue));
        }

        private static void ClearClassifierField(int articleId)
        {
            using (new QPConnectionScope(Infrastructure.Global.ConnectionString))
            {
                var article = ArticleService.Read(articleId);
                Assert.That(article, Is.Not.Null, CantReadArticle);

                var fv = article.FieldValues.Find(itm => itm.Field.IsClassifier && itm.Field.Id == BaseFieldEx2Id);
                Assert.That(fv, Is.Not.Null, NoClassifierField);
                fv.Value = null;

                ArticleService.Save(article, true);
            }
        }

        private static T GetFieldValue<T>(int contentId, string fieldName, int articleId)
        {
            var values = Infrastructure.Global.GetFieldValues<T>(DbConnector, contentId, fieldName, new[] { articleId });
            Assert.That(values, Is.Not.Null, ValuesNotFound);
            Assert.That(values, Has.Length.EqualTo(1), ValuesNotFound);
            return values[0];
        }

        private static int GetArticleId(int contentId)
        {
            var ids = Infrastructure.Global.GetIds(DbConnector, contentId);
            Assert.That(ids, Is.Not.Null.And.Not.Empty, ArticlesNotFound);
            return ids[0];
        }
    }
}
