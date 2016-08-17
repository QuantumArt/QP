using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Enums;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CsvReader
    {
        private readonly int _contentId;
        private readonly int _siteId;
        private readonly ImportSettings _importSetts;
        private List<string> _titleHeaders;
        private List<Field> _baseFields;
        private Dictionary<int, List<Field>> _fieldsMap;
        private Dictionary<Field, int> _headersMap;
        private Dictionary<int, Content> _aggregatedContentsMap;
        private ExstendedArticleList _articlesListFromCsv;
        private List<string> _uniqueValuesList;
        private IEnumerable<Line> _csvLines;
        private readonly FileReader _reader;

        public CsvReader(int siteId, int contentId, ImportSettings setts)
        {
            _contentId = contentId;
            _siteId = siteId;
            _importSetts = setts;
            _reader = new FileReader(setts);
        }

        public void Process(int step, int itemsPerStep, out int processedItemsCount)
        {
            _csvLines = GetLinesFromFile(step, itemsPerStep);
            _titleHeaders = MultistepActionHelper.GetFileFields(_importSetts, _reader);
            InitFields();
            ConvertCsvLinesToArticles();
            WriteArticlesToDb();
            processedItemsCount = _csvLines.Count();
        }

        public int ArticleCount
        {
            get
            {
                return _reader.Lines.Count(s => !s.Skip);
            }
        }

        public string LastProcessed
        {
            get
            {
                if (_csvLines == null || !_csvLines.Any())
                {
                    return @"N/A";
                }

                var num = _csvLines.First().Number - 1;
                if (num == 0 || num == 1 && _reader.Lines.First().Skip || num == 2 && _reader.Lines.First().Skip && _reader.Lines.Skip(1).First().Skip)
                {
                    return @"N/A";
                }

                return num.ToString();
            }
        }

        private void WriteArticlesToDb()
        {
            switch (_importSetts.ImportAction)
            {
                case (int)ImportActions.InsertAll:
                    InsertArticles(_articlesListFromCsv);
                    break;
                case (int)ImportActions.InsertAndUpdate:
                    var existingArticles = UpdateArticles(_articlesListFromCsv);
                    SaveUpdatedArticleIdsToSettings(existingArticles.GetBaseArticleIds());
                    var nonExistingArticles = _articlesListFromCsv.Filter(a => !existingArticles.GetBaseArticleIds().Contains<int>(a.Id));
                    if (nonExistingArticles.Count > 0)
                    {
                        InsertArticles(nonExistingArticles);
                        SaveInsertedArticleIdsToSettings(nonExistingArticles.GetBaseArticleIds());
                    }
                    break;
                case (int)ImportActions.Update:
                    UpdateArticles(_articlesListFromCsv);
                    break;
                case (int)ImportActions.UpdateIfChanged:
                    var changedArticles = _articlesListFromCsv.Filter(a => a.Created == DateTime.MinValue);
                    UpdateArticles(changedArticles);
                    SaveUpdatedArticleIdsToSettings(changedArticles.GetBaseArticleIds());
                    break;
                case (int)ImportActions.InsertNew:
                    var nonExistArticles = GetNonExistingArticles(_articlesListFromCsv);
                    if (nonExistArticles.Count > 0)
                    {
                        InsertArticles(nonExistArticles);
                        SaveInsertedArticleIdsToSettings(nonExistArticles.GetBaseArticleIds());
                    }
                    break;
                default:
                    InsertArticles(_articlesListFromCsv);
                    break;
            }
        }

        #region ConvertCsvLinesToArticles
        private void ConvertCsvLinesToArticles()
        {
            foreach (var line in _csvLines)
            {
                if (string.IsNullOrEmpty(line.Value))
                    continue;
                //Parsing line to get field values
                var fieldValues = SplitToValues(_titleHeaders.Count, line.Value);

                //InitializeArticle article with default values

                var baseArticle = InitializeArticle(_contentId);
                ReadLineFields(baseArticle, fieldValues, _contentId, line.Number);

                var article = new ExstendedArticle(baseArticle);

                foreach (var fv in article.BaseArticle.FieldValues)
                {
                    if (fv.Field.IsClassifier)
                    {
                        int classifierContentId;

                        if (int.TryParse(fv.Value, out classifierContentId))
                        {
                            var exstensionArticle = InitializeArticle(classifierContentId);
                            ReadLineFields(exstensionArticle, fieldValues, classifierContentId, line.Number);

                            var content = _aggregatedContentsMap[classifierContentId];
                            var field = content.Fields.First(f => f.Aggregated);
                            exstensionArticle.FieldValues.Add(new FieldValue { Field = field });
                            article.Exstensions[fv.Field] = exstensionArticle;
                            _articlesListFromCsv.ExstensionFields.Add(fv.Field);
                        }
                        else
                        {
                            article.Exstensions[fv.Field] = null;
                        }
                    }
                }

                _articlesListFromCsv.Add(article);
            }
        }

        private void ReadLineFields(Article article, string[] fieldValues, int contentId, int lineNumber)
        {
            List<Field> fields;

            if (_fieldsMap.TryGetValue(contentId, out fields))
            {
                foreach (var field in fields)
                {
                    var titleIndex = _headersMap[field];

                    if (titleIndex == -1)
                    {
                        //Column with a title doesnt exist in first line of file or client didnt map the field, so skip it
                        continue;
                    }

                    var value = PrepareValue(fieldValues[titleIndex]);
                    var fieldDbValue = new FieldValue { Field = field };

                    if (!IsEmpty(value))
                    {
                        try
                        {
                            FormatFieldValue(field, value, ref fieldDbValue);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException(string.Format(ImportStrings.ErrorInColumn, lineNumber, field.Name, ex.Message), ex);
                        }
                    }
                    else if (fieldDbValue.Field.Required)
                        throw new FormatException(string.Format(ImportStrings.ErrorInRequiredColumn, lineNumber, field.Name, fieldDbValue.Field.Name));

                    article.FieldValues.Add(fieldDbValue);
                }
            }

            // Adding values of columns like content_item_id, ischanged, etc.
            ReadAdditionalFields(ref article, fieldValues);
        }

        private static bool IsEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            if (value == "NULL")
                return true;
            if (value.Length >= 2 && value.First() == '"' && value.Last() == '"' && (value.Length == 2 || string.IsNullOrWhiteSpace(value.Substring(1, value.Length - 2))))
                return true;
            return false;
        }

        private string PrepareValue(string inittialValue)
        {
            var value = inittialValue.Replace("\"\"", "\"");
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Remove(0, 1);
                if (value.Length > 0)
                    value = value.Remove(value.Length - 1);
            }
            return value;
        }

        private void ReadAdditionalFields(ref Article article, string[] fieldValues)
        {
            if (_importSetts.ImportAction != (int)ImportActions.InsertAll)
                ReadUniqueField(ref article, fieldValues);

            if (_importSetts.ImportAction == (int)ImportActions.UpdateIfChanged)
                ReadChangedStatus(ref article, fieldValues);
        }

        private void ReadChangedStatus(ref Article article, string[] fieldValues)
        {
            var isChangedindex = GetFieldIndex(ArticleStrings.IsChanged);

            if (isChangedindex == -1)
            {
                throw new ArgumentException($"There is no column {ArticleStrings.IsChanged} in the specified file");
            }

            if (fieldValues[isChangedindex] == "1")
            {
                article.Created = DateTime.MinValue;
            }
            else
            {
                article.Created = DateTime.Now;
            }
        }

        private void ReadUniqueField(ref Article article, string[] fieldValues)
        {
            string key = null;

            if (article.ContentId == _contentId)
            {
                key = _importSetts.UniqueFieldToUpdate;
            }
            else
            {
                if (_importSetts.UniqueAggregatedFieldsToUpdate.ContainsKey(article.ContentId))
                {
                    key = _importSetts.UniqueAggregatedFieldsToUpdate[article.ContentId];
                }
            }

            var uindex = GetFieldIndex(key);
            if (uindex != -1)
            {
                if (_importSetts.UniqueContentField == null || article.ContentId != _contentId)
                {

                    var dbId = 0;
                    //if value is not empty and doesnt contain id then throw an exception. If its empty, just skip it in order to save later
                    if (!string.IsNullOrEmpty(fieldValues[uindex]) && !int.TryParse(fieldValues[uindex], out dbId))
                    {
                        throw new ArgumentException(ImportStrings.InvalidIdentity);
                    }
                    if (dbId != 0)
                    {
                        article.Id = dbId;
                    }
                }
                else
                {
                    var value = PrepareValue(fieldValues[uindex]);

                    if (value != "NULL" && !string.IsNullOrEmpty(value) && value != "\"\"")
                    {
                        var fieldDbValue = new FieldValue { Field = _importSetts.UniqueContentField };
                        FormatFieldValue(_importSetts.UniqueContentField, value, ref fieldDbValue);
                        _uniqueValuesList.Add(fieldDbValue.Value);
                    }
                    else
                    {
                        _uniqueValuesList.Add(null);
                    }
                }
            }
        }

        private int GetFieldIndex(string fieldName)
        {
            var fieldIndex = _titleHeaders.IndexOf(fieldName);

            if (fieldName == "-1")
            {
                throw new ArgumentException(ImportStrings.UniqueNotSpecified);
            }
            return fieldIndex;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void FormatFieldValue(Field field, string value, ref FieldValue fieldDbValue)
        {
            switch (field.ExactType)
            {
                case FieldExactTypes.Numeric:
                    fieldDbValue.Value = MultistepActionHelper.NumericCultureFormat(value, _importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.Date:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, _importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.Time:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, _importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.DateTime:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, _importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.O2MRelation:
                    fieldDbValue.Value = MultistepActionHelper.O2MFormat(value);
                    break;
                case FieldExactTypes.M2MRelation:
                    fieldDbValue.NewRelatedItems = MultistepActionHelper.M2MFormat(value).ToArray();
                    fieldDbValue.Value = field.LinkId.Value.ToString();
                    break;
                case FieldExactTypes.M2ORelation:
                    break;
                default:
                    fieldDbValue.Value = value;
                    break;
            }
        }

        #endregion

        #region SaveAndUpdate articles methods
        private ExstendedArticleList GetExistingArticles(ExstendedArticleList articlesList)
        {
            return GetArticles(articlesList, true);
        }

        private ExstendedArticleList GetNonExistingArticles(ExstendedArticleList articlesList)
        {
            return GetArticles(articlesList, false);
        }

        private ExstendedArticleList GetArticles(ExstendedArticleList articlesList, bool onlyExisting)
        {
            ExstendedArticleList existingArticles;
            var uniqueField = _importSetts.UniqueContentField;

            if (uniqueField == null)
            {
                var existingIds = GetExistingArticleIds(articlesList.GetBaseArticleIds());
                existingArticles = articlesList.Filter(a => !onlyExisting ^ existingIds.Contains(a.Id));
            }
            else
            {
                existingArticles = new ExstendedArticleList(articlesList);
                var existingIdsMap = GetExistingArticleIdsMap(_uniqueValuesList, uniqueField.Name);

                for (var i = 0; i < articlesList.Count; i++)
                {
                    var article = articlesList[i];
                    var uniqueValue = _uniqueValuesList[i];
                    var articleExists = existingIdsMap.ContainsKey(uniqueValue);

                    if (articleExists)
                    {
                        article.BaseArticle.Id = existingIdsMap[uniqueValue];
                    }

                    if (!onlyExisting ^ articleExists)
                    {
                        existingArticles.Add(article);
                    }
                }
            }

            return existingArticles;
        }

        private void InsertArticles(ExstendedArticleList articleList)
        {
            var baseArticles = articleList.GetBaseArticles();
            var idsList = InsertArticlesIds(baseArticles);
            InsertArticleValues(idsList.ToArray(), baseArticles);

            if (_importSetts.ContainsO2MRelationOrM2MRelationFields)
            {
                SaveNewRelationsToFile(baseArticles, idsList);
            }

            for (var i = 0; i < idsList.Count; i++)
            {
                var id = idsList[i];
                var article = articleList[i];

                foreach (var aggregatedArticle in article.Exstensions.Values)
                {
                    var parent = aggregatedArticle.FieldValues.Find(fv => fv.Field.Aggregated);
                    parent.Value = id.ToString();
                }
            }

            var aggregatedArticles = articleList.GetAllAggregatedArticles().ToList();

            foreach (var aggregatedArticleList in aggregatedArticles)
            {
                var aggregatedIdsList = InsertArticlesIds(aggregatedArticleList);
                InsertArticleValues(aggregatedIdsList.ToArray(), aggregatedArticleList);

                if (_importSetts.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(aggregatedArticleList, aggregatedIdsList);
                }
            }
        }

        private ExstendedArticleList UpdateArticles(ExstendedArticleList articlesList)
        {
            var existingArticles = GetExistingArticles(articlesList);
            var exstensionsMap = ContentRepository.GetAggregatedArticleIdsMap(_contentId, existingArticles.GetBaseArticleIds().ToArray());
            InsertArticleValues(existingArticles.GetBaseArticleIds().ToArray(), existingArticles.GetBaseArticles(), true);

            var idsToDelete = new List<int>();
            var articlesToInsert = new List<Article>();
            var idsToUpdate = new List<int>();
            var articlesToUpdate = new List<Article>();

            foreach (var article in existingArticles)
            {
                foreach (var afv in article.Exstensions)
                {
                    var aggregatedArticle = afv.Value;
                    var fieldId = afv.Key.Id;

                    if (exstensionsMap.ContainsKey(article.BaseArticle.Id) && exstensionsMap[article.BaseArticle.Id].ContainsKey(fieldId))
                    {
                        var currentId = exstensionsMap[article.BaseArticle.Id][fieldId];

                        if (currentId == aggregatedArticle.Id)
                        {
                            idsToUpdate.Add(aggregatedArticle.Id);
                            articlesToUpdate.Add(aggregatedArticle);
                        }
                        else
                        {
                            aggregatedArticle.Id = currentId;
                            idsToDelete.Add(aggregatedArticle.Id);
                            articlesToInsert.Add(aggregatedArticle);
                        }
                    }
                    else
                    {
                        articlesToInsert.Add(aggregatedArticle);
                    }

                    var parent = aggregatedArticle.FieldValues.Find(fv => fv.Field.Aggregated);
                    parent.Value = article.BaseArticle.Id.ToString();
                }
            }

            if (idsToDelete.Any())
            {
                ArticleRepository.MultipleDelete(idsToDelete);
            }

            if (articlesToInsert.Any())
            {
                var inserdedIds = InsertArticlesIds(articlesToInsert);

                if (_importSetts.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(articlesToInsert, inserdedIds);
                }

                idsToUpdate.AddRange(inserdedIds);
                articlesToUpdate.AddRange(articlesToInsert);
            }

            InsertArticleValues(idsToUpdate.ToArray(), articlesToUpdate, true);
            return existingArticles;
        }

        private static List<int> InsertArticlesIds(IList<Article> articleList)
        {
            var insertTemplate = @"SELECT {0}, {1}, {2}, {3} {4}";
            var query = new StringBuilder();
            var unionAll = " UNION ALL ";
            var i = 1;
            foreach (var article in articleList)
            {
                if (i == articleList.Count())
                {
                    unionAll = string.Empty;
                }
                var visible = article.Visible ? 1 : 0;
                query.AppendFormat(insertTemplate, visible, article.StatusTypeId, article.ContentId, QPContext.CurrentUserId, unionAll);
                i++;
            }
            var result = query.ToString();
            return ArticleRepository.InsertArticleIds(result);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void InsertArticleValues(int[] idsList, IList<Article> articleList, bool updateArticles = false)
        {
            if (updateArticles)
            {
                UpdateArticlesDateTime(idsList);
            }

            var doc = new XDocument();
            var o2MDoc = new XDocument();
            var parametersXml = new XElement("PARAMETERS");
            doc.Add(parametersXml);
            o2MDoc.Add(parametersXml);

            var k = 0;
            var m2MValues = new List<KeyValuePair<int, FieldValue>>();
            var o2MValues = new List<KeyValuePair<int, FieldValue>>();

            foreach (var articleId in idsList)
            {
                var article = articleList[k];
                if (article != null)
                {
                    foreach (var fieldValue in article.FieldValues)
                    {
                        var fieldValueXml = GetFieldValueElement(fieldValue, articleId);
                        doc.Root.Add(fieldValueXml);

                        if (fieldValue.Field.ExactType == FieldExactTypes.M2MRelation)
                        {
                            m2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                        }
                        else if (fieldValue.Field.ExactType == FieldExactTypes.O2MRelation)
                        {
                            o2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                            o2MDoc.Root.Add(fieldValueXml);
                        }
                    }
                }

                k++;
            }

            ArticleRepository.ValidateO2MValues(o2MDoc.ToString(SaveOptions.None));
            ValidateO2MRelationSecurity(o2MValues);
            ArticleRepository.InsertArticleValues(doc.ToString(SaveOptions.None));
            UpdateM2MValues(m2MValues);
        }

        private static XElement GetFieldValueElement(FieldValue fieldValue, int articleId)
        {
            var fieldValueXml = new XElement("FIELDVALUE");
            var value = fieldValue.Value;
            if (fieldValue.Value == "NULL")
            {
                value = string.Empty;
            }

            fieldValueXml.Add(new XElement("CONTENT_ITEM_ID", articleId));
            fieldValueXml.Add(new XElement("ATTRIBUTE_ID", fieldValue.Field.Id));

            switch (fieldValue.Field.Type.DatabaseType)
            {
                case "NTEXT":
                    fieldValueXml.Add(new XElement("DATA", null));
                    fieldValueXml.Add(new XElement("BLOB_DATA", value));
                    break;
                default:
                    fieldValueXml.Add(new XElement("DATA", value));
                    fieldValueXml.Add(new XElement("BLOB_DATA", null));
                    break;
            }
            return fieldValueXml;
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private static void ValidateO2MRelationSecurity(List<KeyValuePair<int, FieldValue>> o2mValues)
        {
            var fieldsToCheck = o2mValues.Select(n => n.Value.Field).Distinct().Where(n => n.UseRelationSecurity);
            foreach (var field in fieldsToCheck)
            {
                var ids = o2mValues.Where(n => n.Value.Field == field).Select(n => n.Value.Value).Distinct().Select(int.Parse).ToArray();
                var notAccessedData = ArticleRepository.CheckRelationSecurity(field.RelateToContentId.Value, ids, false).Where(n => !n.Value).Select(n => n.Key.ToString()).ToArray();
                var notAccessedHashset = new HashSet<string>(notAccessedData);
                if (notAccessedHashset.Any())
                {
                    var errorItem = o2mValues.First(n => notAccessedHashset.Contains(n.Value.Value));
                    throw new ArgumentException(string.Format(ImportStrings.InaccessibleO2M, errorItem.Key, field.Name,
                        errorItem.Value.Value));
                }
            }
        }

        #region Update M2MRelation And O2MRelation Fields
        public void PostUpdateM2MRelationAndO2MRelationFields()
        {
            if (_importSetts.ContainsO2MRelationOrM2MRelationFields)
            {
                var values = new List<RelSourceDestinationValue>();

                //get all relations between old and new article ids
                GetNewValues(ref values);
                PostUpdateM2MValues(values);
                PostUpdateO2MValues(values);
            }
        }

        // Обновление значений o2m полей
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void PostUpdateO2MValues(List<RelSourceDestinationValue> m2mValues)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var item in m2mValues.Where(s => !s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    var oldId = item.NewRelatedItems[0];
                    var oldElem = m2mValues.FirstOrDefault(s => s.OldId == oldId);
                    if (oldElem != null && item.OldId != oldId)
                    {
                        var itemXml = new XElement("item");
                        itemXml.Add(new XAttribute("id", item.NewId));
                        itemXml.Add(new XAttribute("linked_id", oldElem.NewId));
                        itemXml.Add(new XAttribute("field_id", item.FieldId));
                        doc.Root.Add(itemXml);

                    }
                }
            }

            ArticleRepository.InsertO2MFieldValues(doc.ToString(SaveOptions.None));
        }

        // Обновление значений m2m полей
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void PostUpdateM2MValues(List<RelSourceDestinationValue> m2mValues)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var item in m2mValues.Where(s => s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    var result = GetM2MRelatedArtsWithNewIds(item.NewRelatedItems, m2mValues);
                    var itemXml = new XElement("item");
                    itemXml.Add(new XAttribute("id", item.NewId));
                    itemXml.Add(new XAttribute("linkId", item.FieldId));
                    itemXml.Add(new XAttribute("value", string.Join(",", result)));
                    doc.Root.Add(itemXml);
                }

            }

            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }

        // Получение списка серверных id вместо пользовательских
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static IEnumerable<int> GetM2MRelatedArtsWithNewIds(IEnumerable<int> oldRelatedIds, IEnumerable<RelSourceDestinationValue> m2mValues)
        {
            return (from oldId in oldRelatedIds
                    where m2mValues.FirstOrDefault(s => s.OldId == oldId) != null
                    select m2mValues.FirstOrDefault(s => s.OldId == oldId).NewId).ToList();
        }
        #endregion

        #region Insert M2M relation field values
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private static void UpdateM2MValues(List<KeyValuePair<int, FieldValue>> values)
        {
            var m2mFields = new Dictionary<string, Field>();

            // Getting m2m fields
            foreach (var fieldV in values.Where(fieldV => !m2mFields.Keys.Contains(fieldV.Value.Field.Name)))
            {
                m2mFields.Add(fieldV.Value.Field.Name, fieldV.Value.Field);
            }

            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var field in m2mFields.Values)
            {
                var condition = field.RelationCondition;
                var linkId = field.LinkId ?? 0;
                var contentId = field.RelateToContentId.Value;

                //Filtering values with m2mFields
                var filteredValues = values.Where(f => f.Value.Field.Name == field.Name).ToList();
                var relatedIds = filteredValues
                    .Where(n => n.Value.NewRelatedItems != null)
                    .SelectMany(n => n.Value.NewRelatedItems)
                    .Distinct()
                    .ToList();

                var validatedIds = new HashSet<int>(ArticleRepository.CheckForArticleExistence(relatedIds, condition, contentId));
                var grantedIds = field.UseRelationSecurity
                    ? new HashSet<int>(ArticleRepository.CheckRelationSecurity(contentId, validatedIds.ToArray(), false).Where(n => n.Value).Select(m => m.Key))
                    : validatedIds;

                foreach (var item in filteredValues)
                {
                    var value = string.Empty;
                    if (item.Value.NewRelatedItems != null)
                    {
                        var notValidIds = item.Value.NewRelatedItems.Where(n => !validatedIds.Contains(n)).ToArray();
                        if (notValidIds.Any())
                        {
                            throw new ArgumentException(string.Format(ImportStrings.IncorrectM2M, item.Key, item.Value.Field.Name, string.Join(",", notValidIds)));
                        }

                        var notGrantedIds = item.Value.NewRelatedItems.Where(n => !grantedIds.Contains(n)).ToArray();
                        if (notGrantedIds.Any())
                        {
                            throw new ArgumentException(string.Format(ImportStrings.InaccessibleM2M, item.Key, item.Value.Field.Name, string.Join(",", notGrantedIds)));
                        }

                        value = string.Join(",", item.Value.NewRelatedItems);
                    }

                    var itemXml = new XElement("item");
                    itemXml.Add(new XAttribute("id", item.Key));
                    itemXml.Add(new XAttribute("linkId", linkId));
                    itemXml.Add(new XAttribute("value", value));
                    doc.Root.Add(itemXml);
                }
            }

            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }
        #endregion

        #region File methods
        // Сериализация соответствий новых статей в файл
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void SaveNewRelationsToFile(IList<Article> articles, IEnumerable<int> idsList)
        {
            var k = 0;
            var m2mValues = new List<RelSourceDestinationValue>();

            // idsList - список добавленнынх id, необходимо пройти по каждому из них и обновить данные полей.
            foreach (var id in idsList)
            {
                var art = articles[k];
                foreach (var fv in art.FieldValues)
                {
                    if (fv.Field.ExactType == FieldExactTypes.M2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        var val = new RelSourceDestinationValue
                        {
                            NewId = id,
                            OldId = art.Id,
                            FieldId = fv.Field.LinkId.Value,
                            NewRelatedItems = fv.NewRelatedItems,
                            IsM2M = true
                        };

                        m2mValues.Add(val);
                    }
                    else if (fv.Field.ExactType == FieldExactTypes.O2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        var val = new RelSourceDestinationValue
                        {
                            NewId = id,
                            OldId = art.Id,
                            FieldId = fv.Field.Id,
                            NewRelatedItems = string.IsNullOrEmpty(fv.Value) ? null : new[] { int.Parse(fv.Value) },
                            IsM2M = false
                        };

                        m2mValues.Add(val);
                    }
                }

                k++;
            }

            try
            {
                using (Stream sw = File.Open(_importSetts.TempFileForRelFields, FileMode.Append))
                {
                    var bin = new BinaryFormatter();
                    bin.Serialize(sw, m2mValues);
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        // Десериализация всех соответствий (после добавления статей)
        private void GetNewValues(ref List<RelSourceDestinationValue> m2mValues)
        {
            if (!File.Exists(_importSetts.TempFileForRelFields))
            {
                return;
            }

            try
            {
                using (Stream stream = File.Open(_importSetts.TempFileForRelFields, FileMode.Open))
                {
                    var bin = new BinaryFormatter();

                    while (stream.Position != stream.Length)
                    {
                        var vals = (List<RelSourceDestinationValue>)bin.Deserialize(stream);
                        m2mValues.AddRange(vals);
                    }
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #endregion

        #region Help private methods

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void SaveUpdatedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            var setts = HttpContext.Current.Session["ImportArticlesService.Settings"] as ImportSettings;
            setts.UpdatedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session["ImportArticlesService.Settings"] = setts;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void SaveInsertedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            var setts = HttpContext.Current.Session["ImportArticlesService.Settings"] as ImportSettings;
            setts.InsertedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session["ImportArticlesService.Settings"] = setts;
        }

        private List<int> GetExistingArticleIds(List<int> articlesIdList)
        {
            return ArticleRepository.CheckForArticleExistence(articlesIdList, string.Empty, _contentId);
        }

        private Dictionary<string, int> GetExistingArticleIdsMap(List<string> values, string fieldName)
        {
            return ArticleRepository.GetExistingArticleIdsMap(values, fieldName, string.Empty, _contentId);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void UpdateArticlesDateTime(int[] articlesIds)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var id in articlesIds)
            {
                var itemXml = new XElement("item");
                itemXml.Add(new XAttribute("id", id));
                itemXml.Add(new XAttribute("modifiedBy", QPContext.CurrentUserId));
                doc.Root.Add(itemXml);
            }

            ArticleRepository.UpdateArticlesDateTime(doc.ToString(SaveOptions.None));
        }

        private IEnumerable<Line> GetLinesFromFile(int step, int itemsPerStep)
        {
            return _reader.Lines.Where(s => !s.Skip).Skip(step * itemsPerStep).Take(itemsPerStep);
        }

        private string[] SplitToValues(int countColumns, string line)
        {
            return line.Split(_importSetts.Delimiter).Count() == countColumns ? line.Split(_importSetts.Delimiter) : ParseLine(line);
        }

        private string[] ParseLine(string line)
        {
            const char quote = '"';

            var fieldValues = new List<string>();
            var lineByChar = line.ToCharArray();
            var startIndex = 0;
            var quoteCounter = 0;
            var isFieldQuoted = false;

            for (var i = 0; i < lineByChar.Length; i++)
            {
                quoteCounter = lineByChar[i] == quote ? quoteCounter + 1 : 0;
                if (lineByChar[i] == quote && (i == lineByChar.Length - 1 || lineByChar[i + 1] != quote) && quoteCounter % 2 != 0)
                {
                    isFieldQuoted = !isFieldQuoted;
                }
                else if (lineByChar[i] == _importSetts.Delimiter)
                {
                    if (isFieldQuoted)
                    {
                        continue;
                    }

                    fieldValues.Add(GetStringFromChars(startIndex, i, lineByChar));
                    startIndex = i + 1;
                }

                if (i == lineByChar.Length - 1 && lineByChar[i] != quote && isFieldQuoted)
                {
                    throw new FormatException(string.Format(ImportStrings.LineFormatException, line));
                }
            }

            if (lineByChar.Length != startIndex)
            {
                fieldValues.Add(GetStringFromChars(startIndex, lineByChar.Length, lineByChar));
            }

            return fieldValues.ToArray();
        }

        private static string GetStringFromChars(int startIndex, int endIndex, char[] charLine)
        {
            var sb = new StringBuilder();
            for (var i = startIndex; i < endIndex; i++)
            {
                sb.Append(charLine[i].ToString());
            }

            return sb.ToString();
        }

        private Article InitializeArticle(int contentId)
        {
            var article = new Article
            {
                Status = StatusType.GetPublished(_siteId),
                StatusTypeId = StatusTypeRepository.GetPublishedStatusIdBySiteId(_siteId),
                Visible = true,
                ContentId = contentId,
                FieldValues = new List<FieldValue>()
            };

            return article;
        }
        #endregion

        #region Public static methods
        public static List<string> GetFieldNames(IEnumerable<string> csvLines, char delimiter, bool noHeaders)
        {
            var firstLine = csvLines.First();
            if (!noHeaders)
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    throw new ArgumentException(ImportStrings.FirstLineEmpty);
                }

                return firstLine.Split(delimiter).ToList();
            }

            var columnsCount = firstLine.Split(delimiter).Count();
            var columnIndexes = Enumerable.Range(0, columnsCount).ToArray();
            return columnIndexes.Select(k => k.ToString()).ToList();
        }
        #endregion

        #region extension methods
        private void InitFields()
        {
            _baseFields = _importSetts.FieldsList.Where(f => f.Value.ContentId == _contentId).Select(f => f.Value).ToList();
            _fieldsMap = (from f in _importSetts.FieldsList.Select(f => f.Value)
                         group f by f.ContentId into g
                         select g)
                         .ToDictionary(g => g.Key, g => g.ToList());

            _headersMap = _importSetts.FieldsList.ToDictionary(f => f.Value, f => _titleHeaders.FindIndex(s => s == f.Key));
            _aggregatedContentsMap = ContentRepository.GetAggregatedContents(_contentId).ToDictionary(c => c.Id, c => c);
            _articlesListFromCsv = new ExstendedArticleList();
            _uniqueValuesList = new List<string>();
        }
        #endregion
    }

    public class FileReader
    {
        private readonly ImportSettings _setts;
        private readonly Lazy<IEnumerable<Line>> _lines;

        public FileReader(ImportSettings settings)
        {
            _setts = settings;
            _lines = new Lazy<IEnumerable<Line>>(() => ReadFile(_setts));
        }

        public IEnumerable<Line> Lines => _lines.Value;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void CopyFileToTempDir()
        {
            var fileInfo = new FileInfo(HttpUtility.UrlDecode(_setts.UploadFilePath));
            if (fileInfo.Exists)
            {
                var newFileUploadPath = $"{QPConfiguration.TempDirectory}\\{fileInfo.Name}";
                if (!File.Exists(newFileUploadPath))
                    File.Copy(_setts.UploadFilePath, newFileUploadPath, true);
            }
            else
            {
                throw new FileNotFoundException($"File {_setts.UploadFilePath} was not found.");
            }
        }

        public static IEnumerable<Line> ReadFile(ImportSettings setts)
        {
            using (var reader = new StreamReader(setts.UploadFilePath))
            {
                var rdr = new CustomStreamReader(reader.BaseStream, Encoding.GetEncoding(setts.Encoding), setts.LineSeparator, setts.Delimiter);
                string line;
                var i = 0;
                var headerNum = 1;
                while (!string.IsNullOrEmpty(line = rdr.ReadLine()))
                {
                    i++;
                    var value = line.Trim('\n', '\r');
                    var isSep = value.StartsWith("sep=");
                    if (isSep)
                        headerNum++;
                    var skip = (!setts.NoHeaders && i == headerNum) || string.IsNullOrEmpty(value) || isSep;
                    yield return new Line { Value = value, Number = i, Skip = skip };
                }
            }
        }

        public int RowsCount()
        {
            return Lines.Count(s => !s.Skip);
        }
    }

    public class CustomStreamReader : StreamReader
    {
        private readonly string _lineDelimiter;
        private readonly char _fieldDelimiter;

        public CustomStreamReader(Stream stream, Encoding encoding, string lineDelimiter, char fieldDelimiter)
            : base(stream, encoding)
        {
            _lineDelimiter = lineDelimiter;
            _fieldDelimiter = fieldDelimiter;
        }

        private bool IsEmpty(string line)
        {
            return string.IsNullOrEmpty(line.Trim(_fieldDelimiter, '"', '\r', '\n')) || string.IsNullOrWhiteSpace(line);
        }

        public override string ReadLine()
        {
            var c = Read();
            if (c == -1)
            {
                return null;
            }

            var sb = new StringBuilder();
            var lastCh = char.MinValue;
            var quoteOpen = false;
            do
            {
                if ((char)c == '"' && !quoteOpen)
                {
                    quoteOpen = true;
                }
                else if ((char)c == '"' && quoteOpen)
                {
                    quoteOpen = false;
                }

                var lineSepArr = _lineDelimiter.ToCharArray();
                var sep = lineSepArr[0];
                if (lineSepArr.Length == 2)
                {
                    sep = lineSepArr[1];
                }

                var ch = (char)c;
                if (ch == sep && !quoteOpen && (lineSepArr.Length == 1 || (lineSepArr.Length == 2 && lastCh == lineSepArr[0])))
                {
                    if (!IsEmpty(sb.ToString()))
                    {
                        return sb.ToString();
                    }

                    sb.Remove(0, sb.Length);
                }
                else
                {
                    sb.Append(ch);
                }

                lastCh = (char)c;
            } while ((c = Read()) != -1);

            return sb.ToString();
        }
    }

    [Serializable]
    public class RelSourceDestinationValue
    {
        public int OldId { get; set; }

        public int NewId { get; set; }

        public int FieldId { get; set; }

        public int[] NewRelatedItems { get; set; }

        public bool IsM2M { get; set; }
    }

    public class ExstendedArticle
    {
        public Article BaseArticle { get; }

        public Dictionary<Field, Article> Exstensions { get; }

        public ExstendedArticle(Article baseArticle)
        {
            BaseArticle = baseArticle;
            Exstensions = new Dictionary<Field, Article>();
        }
    }

    public class ExstendedArticleList : List<ExstendedArticle>
    {
        public HashSet<Field> ExstensionFields { get; }

        public ExstendedArticleList()
        {
            ExstensionFields = new HashSet<Field>();
        }

        public ExstendedArticleList(ExstendedArticleList articles)
        {
            ExstensionFields = new HashSet<Field>(articles.ExstensionFields);
        }

        public List<Article> GetBaseArticles()
        {
            return this.Select(a => a.BaseArticle).ToList();
        }

        public List<int> GetBaseArticleIds()
        {
            return this.Select(a => a.BaseArticle.Id).ToList();
        }

        public List<Article> GetAggregatedArticles(Field field)
        {
            return this.Select(a => a.Exstensions[field]).ToList();
        }

        public IEnumerable<List<Article>> GetAllAggregatedArticles()
        {
            return ExstensionFields.Select(GetAggregatedArticles);
        }

        public ExstendedArticleList Filter(Func<Article, bool> predicate)
        {
            var result = new ExstendedArticleList(this);
            result.AddRange(this.Where(article => predicate(article.BaseArticle)));
            return result;
        }
    }

    public class Line
    {
        public int Number { get; set; }

        public string Value { get; set; }

        public bool Skip { get; set; }
    }
}
