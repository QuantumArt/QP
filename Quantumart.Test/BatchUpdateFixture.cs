using NUnit.Framework;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.Test
{
    [TestFixture]
    public class BatchUpdateFixture
    {
        #region Constants
        #region Assert messages
        public const string ArticlesNotFound = "Статьи для контента не найдены";
        public const string ValuesNotFound = "Значения не найдены";
        public const string BatchUpdateResultIncorrect = "Неверный результат";
        public const string CantReadArticle = "Не удалось прочитать статью";
        public const string NoClassifierField = "Не найдено поле классификатор";
        public const string WrongMtMRelation = "Неверно проставлена связь MtM";
        public const string WrongMtORelation = "Неверно проставлена связь MtO";
        public const string ContentDataIsEmpty = "Не заданы поля в CONTENT_DATA";
        #endregion

        #region Base content
        public const string Base_Content = "Test_BatchUpdate_Base";

        public const string Base_Field_Ex1 = "Field_Ex1";
        public const string Base_Field_Ex2 = "Field_Ex2";
        public const string Base_Field_String = "Field_String";
        public const string Base_Field_MtM = "Field_MtM";
        public const string Base_Field_OtM = "Field_OtM";
        public const string Base_Field_Numeric_Integer = "Field_Numeric_Integer";
        public const string Base_Field_Numeric_Decimal = "Field_Numeric_Decimal";
        public const string Base_Field_Date = "Field_Date";
        public const string Base_Field_Time = "Field_Time";
        public const string Base_Field_DateTime = "Field_DateTime";
        public const string Base_Field_File = "Field_File";
        public const string Base_Field_Image = "Field_Image";
        public const string Base_Field_TextBox = "Field_TextBox";
        public const string Base_Field_VisualEdit = "Field_VisualEdit";
        public const string Base_Field_DynamicImage = "Field_DynamicImage";
        public const string Base_Field_Enum = "Field_Enum";
        #endregion

        #region Dictionary content
        public const string Dictionary_Content = "Test_BatchUpdate_Dictionary";

        public const string Dictionary_Key = "Key";
        public const string Dictionary_Value = "Value";
        public const string Dictionary_Field_MtM_Backward = "Field_MtM_Backward";
        public const string Dictionary_Field_MtO_Backward = "Field_MtO_Backward";
        #endregion

        #region Ex1_1 content
        public const string Ex1_1_Content = "Test_BatchUpdate_Ex1_1";

        public const string Ex1_1_Parent = "Parent";
        public const string Ex1_1_Field1 = "Field1";
        public const string Ex1_1_Field2 = "Field2";
        #endregion

        #region Ex1_2 content
        public const string Ex1_2_Content = "Test_BatchUpdate_Ex1_2";

        public const string Ex1_2_Parent = "Parent";
        public const string Ex1_2_Field1 = "Field1";
        #endregion

        #region Ex2_1 content
        public const string Ex2_1_Content = "Test_BatchUpdate_Ex2_1";
        public const string Ex2_1_Parent = "Parent";
        #endregion

        #region Ex2_2 content
        public const string Ex2_2_Content = "Test_BatchUpdate_Ex2_2";
        public const string Ex2_2_Parent = "Parent";
        #endregion
        #endregion

        #region Properties
        public static DBConnector Cnn { get; private set; }
        public static ArticleService ArticleService { get; private set; }
        public static Random Random { get; private set; }

        #region Base content
        public static int Base_ContentId { get; private set; }

        public static int Base_Field_Ex1Id { get; private set; }
        public static int Base_Field_Ex2Id { get; private set; }
        public static int Base_Field_StringId { get; private set; }
        public static int Base_Field_MtMId { get; private set; }
        public static int Base_Field_OtMId { get; private set; }
        public static int Base_Field_Numeric_IntegerId { get; private set; }
        public static int Base_Field_Numeric_DecimalId { get; private set; }
        public static int Base_Field_DateId { get; private set; }
        public static int Base_Field_TimeId { get; private set; }
        public static int Base_Field_DateTimeId { get; private set; }
        public static int Base_Field_FileId { get; private set; }
        public static int Base_Field_ImageId { get; private set; }
        public static int Base_Field_TextBoxId { get; private set; }
        public static int Base_Field_VisualEditId { get; private set; }
        public static int Base_Field_DynamicImageId { get; private set; }
        public static int Base_Field_EnumId { get; private set; }
        #endregion

        #region Dictionary content
        public static int Dictionary_ContentId { get; private set; }

        public static int Dictionary_KeyId { get; private set; }
        public static int Dictionary_ValueId { get; private set; }
        public static int Dictionary_Field_MtM_BackwardId { get; private set; }
        public static int Dictionary_Field_MtO_BackwardId { get; private set; }
        #endregion

        #region Ex1_1 content
        public static int Ex1_1_ContentId { get; private set; }

        public static int Ex1_1_ParentId { get; private set; }
        public static int Ex1_1_Field1Id { get; private set; }
        public static int Ex1_1_Field2Id { get; private set; }
        #endregion

        #region Ex1_2 content
        public static int Ex1_2_ContentId { get; private set; }
        public static int Ex1_2_ParentId { get; private set; }
        public static int Ex1_2_Field1Id { get; private set; }
        #endregion

        #region Ex2_1 content
        public static int Ex2_1_ContentId { get; private set; }
        public static int Ex2_1_ParentId { get; private set; }
        #endregion

        #region Ex2_2 content
        public static int Ex2_2_ContentId { get; private set; }
        public static int Ex2_2_ParentId { get; private set; }
        #endregion

        #endregion

        #region Test actions
        [OneTimeSetUp]
        public static void Init()
        {
            QPContext.UseConnectionString = true;

            var service = new ReplayService(Global.ConnectionString, 1, true);
            service.ReplayXml(Global.GetXml(@"xmls\batchupdate.xml"));
            Cnn = new DBConnector(Global.ConnectionString) { ForceLocalCache = true };            
            ArticleService = new ArticleService(Global.ConnectionString, 1);
            Random = new Random();

            #region Base content
            Base_ContentId = Global.GetContentId(Cnn, Base_Content);

            Base_Field_Ex1Id = Global.GetFieldId(Cnn, Base_Content, Base_Field_Ex1);
            Base_Field_Ex2Id = Global.GetFieldId(Cnn, Base_Content, Base_Field_Ex2);
            Base_Field_StringId = Global.GetFieldId(Cnn, Base_Content, Base_Field_String);
            Base_Field_MtMId = Global.GetFieldId(Cnn, Base_Content, Base_Field_MtM);
            Base_Field_OtMId = Global.GetFieldId(Cnn, Base_Content, Base_Field_OtM);
            Base_Field_Numeric_IntegerId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Numeric_Integer);
            Base_Field_Numeric_DecimalId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Numeric_Decimal);
            Base_Field_DateId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Date);
            Base_Field_TimeId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Time);
            Base_Field_DateTimeId = Global.GetFieldId(Cnn, Base_Content, Base_Field_DateTime);
            Base_Field_FileId = Global.GetFieldId(Cnn, Base_Content, Base_Field_File);
            Base_Field_ImageId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Image);
            Base_Field_TextBoxId = Global.GetFieldId(Cnn, Base_Content, Base_Field_TextBox);
            Base_Field_VisualEditId = Global.GetFieldId(Cnn, Base_Content, Base_Field_VisualEdit);
            Base_Field_DynamicImageId = Global.GetFieldId(Cnn, Base_Content, Base_Field_DynamicImage);
            Base_Field_EnumId = Global.GetFieldId(Cnn, Base_Content, Base_Field_Enum);
            #endregion

            #region Dictionary content
            Dictionary_ContentId = Global.GetContentId(Cnn, Dictionary_Content);

            Dictionary_KeyId = Global.GetFieldId(Cnn, Dictionary_Content, Dictionary_Key);
            Dictionary_ValueId = Global.GetFieldId(Cnn, Dictionary_Content, Dictionary_Value);
            Dictionary_Field_MtM_BackwardId = Global.GetFieldId(Cnn, Dictionary_Content, Dictionary_Field_MtM_Backward);
            Dictionary_Field_MtO_BackwardId = Global.GetFieldId(Cnn, Dictionary_Content, Dictionary_Field_MtO_Backward);
            #endregion

            #region Ex1_1 content
            Ex1_1_ContentId = Global.GetContentId(Cnn, Ex1_1_Content);

            Ex1_1_ParentId = Global.GetFieldId(Cnn, Ex1_1_Content, Ex1_1_Parent);
            Ex1_1_Field1Id = Global.GetFieldId(Cnn, Ex1_1_Content, Ex1_1_Field1);
            Ex1_1_Field2Id = Global.GetFieldId(Cnn, Ex1_1_Content, Ex1_1_Field2);
            #endregion

            #region Ex1_2 content
            Ex1_2_ContentId = Global.GetContentId(Cnn, Ex1_2_Content);
            Ex1_2_ParentId = Global.GetFieldId(Cnn, Ex1_2_Content, Ex1_2_Parent);
            Ex1_2_Field1Id = Global.GetFieldId(Cnn, Ex1_2_Content, Ex1_2_Field1);
            #endregion

            #region Ex2_1 content
            Ex2_1_ContentId = Global.GetContentId(Cnn, Ex2_1_Content);
            Ex2_1_ParentId = Global.GetFieldId(Cnn, Ex2_1_Content, Ex2_1_Parent);
            #endregion

            #region Ex2_2 content
            Ex2_2_ContentId = Global.GetContentId(Cnn, Ex2_2_Content);
            Ex2_2_ParentId = Global.GetFieldId(Cnn, Ex2_2_Content, Ex2_2_Parent);
            #endregion

        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            var contentService = new ContentService(Global.ConnectionString, 1);
            var fieldService = new FieldService(Global.ConnectionString, 1);

            var dictionaryIds = Global.GetIds(Cnn, Dictionary_ContentId);
            var baseIds = Global.GetIds(Cnn, Base_ContentId);
            
            ArticleService.Delete(Dictionary_ContentId, dictionaryIds);
            ArticleService.Delete(Base_ContentId, baseIds);

            contentService.Delete(Ex1_1_ContentId);
            contentService.Delete(Ex1_2_ContentId);
            contentService.Delete(Ex2_1_ContentId);
            contentService.Delete(Ex2_2_ContentId);

            fieldService.Delete(Dictionary_Field_MtM_BackwardId);
            fieldService.Delete(Dictionary_Field_MtO_BackwardId);
            fieldService.Delete(Base_Field_MtMId);
            fieldService.Delete(Base_Field_OtMId);

            contentService.Delete(Dictionary_ContentId);
            contentService.Delete(Base_ContentId);

            QPContext.UseConnectionString = false;
        }
        #endregion

        #region Tests
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
                    ContentId = Dictionary_ContentId,
                    Id = -1,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = Dictionary_KeyId,
                            Value = ""
                        },
                        new FieldData
                        {
                            Id = Dictionary_ValueId,
                            Value = ""
                        },
                        new FieldData
                        {
                            Id = Dictionary_Field_MtM_BackwardId
                        },
                        new FieldData
                        {
                            Id = Dictionary_Field_MtO_BackwardId
                        }
                    }.ToList()
                }
            };

            var result = ArticleService.BatchUpdate(data);            

            Assert.IsNotNull(result, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));
            var articleResult = result[0];
            Assert.AreEqual(Dictionary_ContentId, articleResult.ContentId);
            Assert.AreNotEqual(articleResult.OriginalArticleId, articleResult.CreatedArticleId);

         
            data[0].Id = articleResult.CreatedArticleId;
            data[0].Fields[0].Value = key;
            data[0].Fields[1].Value = value;

            Global.ClearContentData(Cnn, articleResult.CreatedArticleId);

            result = ArticleService.BatchUpdate(data);

            Assert.IsNotNull(result, BatchUpdateResultIncorrect);
            Assert.IsEmpty(result);

            var contentData = Global.GetContentData(Cnn, articleResult.CreatedArticleId);

            Assert.IsNotNull(contentData);
            Assert.That(contentData, Has.Length.EqualTo(4), ContentDataIsEmpty);            

            using (var scope = new QPConnectionScope(Global.ConnectionString))
            {
                var article = ArticleService.Read(articleResult.CreatedArticleId);

                Assert.IsNotNull(article, CantReadArticle);

                var keyFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Key);
                Assert.IsNotNull(keyFv);
                Assert.AreEqual(key, keyFv.Value);
                Assert.True(contentData.Any(itm => itm.FieldId == Dictionary_KeyId && itm.Data == key));

                var valueFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Value);
                Assert.IsNotNull(valueFv);
                Assert.AreEqual(value, valueFv.Value);
                Assert.True(contentData.Any(itm => itm.FieldId == Dictionary_ValueId && itm.Data == value));

                var mtoValue = GetFieldValue<decimal?>(Dictionary_ContentId, Dictionary_Field_MtO_Backward, articleResult.CreatedArticleId);
                Assert.AreEqual(mtoValue, Base_Field_OtMId, WrongMtORelation);
                Assert.True(contentData.Any(itm => itm.FieldId == Dictionary_Field_MtO_BackwardId && itm.Data == Base_Field_OtMId.ToString(CultureInfo.InvariantCulture)), WrongMtORelation);

                var mtmValue = GetFieldValue<decimal?>(Dictionary_ContentId, Dictionary_Field_MtM_Backward, articleResult.CreatedArticleId);
                Assert.IsNotNull(mtmValue, WrongMtMRelation);
                var mtmFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Field_MtM_Backward);
                Assert.IsNotNull(mtmFv);
                Assert.AreEqual(mtmFv.Field.Default, mtmValue.Value.ToString(CultureInfo.InvariantCulture), WrongMtMRelation);
                Assert.True(contentData.Any(itm => itm.FieldId == Dictionary_Field_MtM_BackwardId && itm.Data == mtmFv.Field.Default), WrongMtMRelation);

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
                    ContentId = Dictionary_ContentId,
                    Id = -1,
                    Fields = new[]
                    {
                        new FieldData
                        {
                            Id = Dictionary_KeyId,
                            Value = key
                        },
                        new FieldData
                        {
                            Id = Dictionary_ValueId,
                            Value = value
                        },
                        new FieldData
                        {
                            Id = Dictionary_Field_MtM_BackwardId
                        },
                        new FieldData
                        {
                            Id = Dictionary_Field_MtO_BackwardId
                        }
                    }.ToList()
                }
            };

            var result = ArticleService.BatchUpdate(data);

            Assert.IsNotNull(result, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));
            var articleResult = result[0];
            Assert.AreEqual(Dictionary_ContentId, articleResult.ContentId);
            Assert.AreNotEqual(articleResult.OriginalArticleId, articleResult.CreatedArticleId);

            using (var scope = new QPConnectionScope(Global.ConnectionString))
            {
                var article = ArticleService.Read(articleResult.CreatedArticleId);

                Assert.IsNotNull(article, CantReadArticle);

                var keyFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Key);
                Assert.IsNotNull(keyFv);
                Assert.AreEqual(key, keyFv.Value);

                var valueFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Value);
                Assert.IsNotNull(valueFv);
                Assert.AreEqual(value, valueFv.Value);

                var mtoValue = GetFieldValue<decimal?>(Dictionary_ContentId, Dictionary_Field_MtO_Backward, articleResult.CreatedArticleId);                
                Assert.AreEqual(mtoValue, Base_Field_OtMId, WrongMtORelation);

                var mtmValue = GetFieldValue<decimal?>(Dictionary_ContentId, Dictionary_Field_MtM_Backward, articleResult.CreatedArticleId);
                Assert.IsNotNull(mtmValue, WrongMtMRelation);
                var mtmFv = article.FieldValues.Single(fv => fv.Field.Name == Dictionary_Field_MtM_Backward);
                Assert.IsNotNull(mtmFv);
                Assert.AreEqual(mtmFv.Field.Default, mtmValue.Value.ToString(CultureInfo.InvariantCulture), WrongMtMRelation);                

                ArticleService.Delete(article.Id);
            }
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateExstensionField()
        {
            var articleId = GetArticleId(Base_ContentId);

            ClearClassifierField(articleId, Base_Field_Ex2Id);


            var data = new[]
            {
                new ArticleData
                {
                    ContentId = Base_ContentId,
                    Id = articleId,
                    Fields = new []
                    {
                        new FieldData
                        {
                            Id = Base_Field_Ex2Id,
                            Value = Ex2_1_ContentId.ToString(CultureInfo.InvariantCulture),
                            ArticleIds = new[] { -1 }

                        }
                    }.ToList()
                },
                new ArticleData
                {
                    ContentId = Ex2_1_ContentId,
                    Id = -1
                }
            };

            var result = ArticleService.BatchUpdate(data);

            Assert.IsNotNull(result, BatchUpdateResultIncorrect);
            Assert.That(result, Has.Length.EqualTo(1));
            var exstensionResult = result[0];
            Assert.AreEqual(exstensionResult.ContentId, Ex2_1_ContentId);
            Assert.AreNotEqual(exstensionResult.OriginalArticleId, exstensionResult.CreatedArticleId);


            var parentValues = Global.GetFieldValues<decimal>(Cnn, Ex2_1_ContentId, Ex2_1_Parent, new[] { exstensionResult.CreatedArticleId });
            Assert.IsNotNull(parentValues, ValuesNotFound);
            Assert.That(parentValues, Has.Length.EqualTo(1));
            var parentValue = parentValues[0];

            Assert.AreEqual(articleId, parentValue);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateStringField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(Base_ContentId, Base_Field_StringId, Base_Field_String, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateEnumField()
        {
            var value = "Key1";
            UpdateField(Base_ContentId, Base_Field_EnumId, Base_Field_Enum, value, value);
            value = "Key2";
            UpdateField(Base_ContentId, Base_Field_EnumId, Base_Field_Enum, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateFileField()
        {
            var value = Guid.NewGuid().ToString() + ".txt";
            UpdateField(Base_ContentId, Base_Field_FileId, Base_Field_File, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateImageField()
        {
            var value = Guid.NewGuid().ToString() + ".png";
            UpdateField(Base_ContentId, Base_Field_ImageId, Base_Field_Image, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDynamicImageField()
        {
            var value = Guid.NewGuid().ToString() + ".png";
            UpdateField(Base_ContentId, Base_Field_DynamicImageId, Base_Field_DynamicImage, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateTextBoxField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(Base_ContentId, Base_Field_TextBoxId, Base_Field_TextBox, value, value);
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateVisualEditField()
        {
            var value = Guid.NewGuid().ToString();
            UpdateField(Base_ContentId, Base_Field_VisualEditId, Base_Field_VisualEdit, value, value);
        }      

        [Test]
        public void BatchUpdate_BaseContent_UpdateNumericIntegerField()
        {
            int range = 1000;
            decimal value = Random.Next(-range, range);
            UpdateField<object>(Base_ContentId, Base_Field_Numeric_IntegerId, Base_Field_Numeric_Integer, value, value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateNumericDecimalField()
        {
            int range = 100000;
            decimal value = Random.Next(-range, range);
            value /= 100;
            UpdateField<object>(Base_ContentId, Base_Field_Numeric_DecimalId, Base_Field_Numeric_Decimal, value, value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDateTimeField()
        {
            var value = DateTime.Now;
            value = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
            UpdateField(Base_ContentId, Base_Field_DateTimeId, Base_Field_DateTime, value, value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateDateField()
        {
            var value = DateTime.Today;
            UpdateField(Base_ContentId, Base_Field_DateId, Base_Field_Date, value, value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void BatchUpdate_BaseContent_UpdateTimeField()
        {
            var value = DateTime.Now;
            value = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
            UpdateField(Base_ContentId, Base_Field_TimeId, Base_Field_Time, value, value.ToString(CultureInfo.InvariantCulture));
        }
        #endregion          

        #region Private methods
        private void UpdateField<T>(int contentId, int fieldId, string fieldName, T value, string stringValue)
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

            Assert.IsNotNull(result, BatchUpdateResultIncorrect);
            Assert.IsEmpty(result, BatchUpdateResultIncorrect);

            var newValue = GetFieldValue<T>(contentId, fieldName, articleId);

            Assert.AreEqual(value, newValue);
        }
    
        private void ClearClassifierField(int articleId, int fieldId)
        {
            using (var scope = new QPConnectionScope(Global.ConnectionString))
            {
                var article = ArticleService.Read(articleId);
                Assert.IsNotNull(article, CantReadArticle);

                var fv = article.FieldValues.Find(itm => itm.Field.IsClassifier && itm.Field.Id == Base_Field_Ex2Id);
                Assert.IsNotNull(fv, NoClassifierField);
                fv.Value = null;

                ArticleService.Save(article);
            }
        }

        T GetFieldValue<T>(int contentId, string fieldName, int articleId)
        {
            var values = Global.GetFieldValues<T>(Cnn, contentId, fieldName, new[] { articleId });
            Assert.IsNotNull(values, ValuesNotFound);
            Assert.That(values, Has.Length.EqualTo(1), ValuesNotFound);
            return values[0];
        }

        private int GetArticleId(int contentId)
        {
            var ids = Global.GetIds(Cnn, contentId);
            Assert.IsNotNull(ids, ArticlesNotFound);
            Assert.IsNotEmpty(ids, ArticlesNotFound);
            return ids[0];
        }        
        #endregion
    }
}