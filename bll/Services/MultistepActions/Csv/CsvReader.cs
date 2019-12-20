using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Enums.Csv;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CsvReader
    {
        private readonly int _siteId;
        private readonly int _contentId;
        private readonly FileReader _reader;
        private readonly ImportSettings _importSettings;
        private readonly NotificationPushRepository _notificationRepository;

        private List<string> _titleHeaders;
        private Dictionary<int, List<Field>> _fieldsMap;
        private Dictionary<Field, int> _headersMap;
        private Dictionary<int, Content> _aggregatedContentsMap;
        private ExtendedArticleList _articlesListFromCsv;
        private List<string> _uniqueValuesList;
        private IEnumerable<Line> _csvLines;
        private JObject _jObject;
        private Dictionary<int, Field> _traceFields;

        public CsvReader(int siteId, int contentId, ImportSettings settings)
        {
            _siteId = siteId;
            _contentId = contentId;
            _importSettings = settings;
            _reader = new FileReader(settings);
            _notificationRepository = new NotificationPushRepository { IgnoreInternal = true };
            _traceFields = GetTraceFields(contentId);
            _jObject = InitJObject(_traceFields);

        }

        public string GetTraceResult()
        {
            return _traceFields.Any() ? _jObject.ToString() : "";
        }

        private static JObject InitJObject(Dictionary<int, Field> traceFields)
        {
            var obj = new JObject();

            foreach (var field in traceFields.Values)
            {
                obj.Add(field.Name, new JArray());
            }

            return obj;
        }

        private static Dictionary<int, Field> GetTraceFields(int contentId)
        {
            return FieldRepository.GetList(contentId, false).Where(n => n.TraceImport).ToDictionary(n => n.Id, m => m);
        }


        public void Process(int step, int itemsPerStep, out int processedItemsCount)
        {
            _csvLines = _reader.Lines.Where(s => !s.Skip).Skip(step * itemsPerStep).Take(itemsPerStep);
            _titleHeaders = MultistepActionHelper.GetFileFields(_importSettings, _reader);

            InitFields();
            ConvertCsvLinesToArticles();
            WriteArticlesToDb();
            processedItemsCount = _csvLines.Count();
        }

        public int ArticleCount
        {
            get { return _reader.Lines.Count(s => !s.Skip); }
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
            switch (_importSettings.ImportAction)
            {
                case (int)CsvImportMode.InsertAll:
                    InsertArticles(_articlesListFromCsv);
                    break;
                case (int)CsvImportMode.InsertAndUpdate:
                    var existingArticles = UpdateArticles(_articlesListFromCsv);
                    var remainingArticles = _articlesListFromCsv.Filter(a => !existingArticles.GetBaseArticleIds().Contains<int>(a.Id));
                    InsertArticles(remainingArticles);
                    break;
                case (int)CsvImportMode.Update:
                    UpdateArticles(_articlesListFromCsv);
                    break;
                case (int)CsvImportMode.UpdateIfChanged:
                    var changedArticles = _articlesListFromCsv.Filter(a => a.Created == DateTime.MinValue);
                    UpdateArticles(changedArticles);
                    break;
                case (int)CsvImportMode.InsertNew:
                    var nonExistingArticles = GetArticles(_articlesListFromCsv, false);
                    InsertArticles(nonExistingArticles);
                    break;
                default:
                    throw new NotImplementedException($"Неизвестный режим импорта: {_importSettings.ImportAction}");
            }
        }

        private void InsertArticles(ExtendedArticleList articlesToInsert)
        {
            if (articlesToInsert.Any())
            {
                var insertingArticles = InsertArticlesToDb(articlesToInsert);
                var insertedArticles = insertingArticles.GetBaseArticleIds();
                SaveInsertedArticleIdsToSettings(insertedArticles);
            }
        }

        private ExtendedArticleList UpdateArticles(ExtendedArticleList articlesToUpdate)
        {
            if (articlesToUpdate.Any())
            {
                var updatingArticles = UpdateArticlesToDb(articlesToUpdate);
                var updatedArticles = updatingArticles.GetBaseArticleIds();
                SaveUpdatedArticleIdsToSettings(updatedArticles);
                return updatingArticles;
            }

            return articlesToUpdate;
        }

        private void ConvertCsvLinesToArticles()
        {
            foreach (var line in _csvLines)
            {
                if (string.IsNullOrEmpty(line.Value))
                {
                    continue;
                }

                var fieldValues = SplitToValues(_titleHeaders.Count, line.Value);
                var baseArticle = InitializeArticle(_contentId);

                if (int.TryParse(fieldValues.First(), out var articleId))
                {
                    baseArticle.Id = articleId;
                }

                ReadLineFields(baseArticle, fieldValues, _contentId, line.Number);

                var article = new ExtendedArticle(baseArticle);
                foreach (var fv in article.BaseArticle.FieldValues)
                {
                    if (fv.Field.IsClassifier)
                    {
                        if (int.TryParse(fv.Value, out var classifierContentId))
                        {
                            AddExtensionArticle(article, fv.Field, classifierContentId, fieldValues, line);
                        }
                        else
                        {
                            article.Extensions[fv.Field] = null;
                        }
                    }
                }

                if (!article.BaseArticle.FieldValues.Any() && _aggregatedContentsMap.Where(w => w.Key != article.BaseArticle.Id).Any())
                {
                    var classifierFields = _importSettings.FieldsList.Where(w => w.Value.IsClassifier).Select(s => s.Value);

                    foreach (var classifier in classifierFields)
                    {
                        foreach (var classifierContentId in _aggregatedContentsMap.Where(w => w.Key != classifier.ContentId).Select(s => s.Key))
                        {
                            if (_importSettings.FieldsList.Where(w => w.Key != "-1" && w.Value.ContentId == classifierContentId).Any())
                            {
                                AddExtensionArticle(article, classifier, classifierContentId, fieldValues, line);
                            }
                        }
                    }
                }

                _articlesListFromCsv.Add(article);
            }
        }

        private void AddExtensionArticle(ExtendedArticle article, Field fv, int classifierContentId, string[] fieldValues, Line line)
        {
            var extensionArticle = InitializeArticle(classifierContentId);
            ReadLineFields(extensionArticle, fieldValues, classifierContentId, line.Number, true);

            var content = _aggregatedContentsMap[classifierContentId];
            var field = content.Fields.First(f => f.Aggregated);
            extensionArticle.FieldValues.Add(new FieldValue { Field = field });
            article.Extensions[fv] = extensionArticle;
            _articlesListFromCsv.ExtensionFields.Add(fv);
        }

        private void ReadLineFields(Article article, IReadOnlyList<string> fieldValues, int contentId, int lineNumber, bool isExtension = false)
        {
            if (_fieldsMap.TryGetValue(contentId, out var fields))
            {
                foreach (var field in fields)
                {
                    var titleIndex = _headersMap[field];
                    if (titleIndex == -1)
                    {
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
                    {
                        throw new FormatException(string.Format(ImportStrings.ErrorInRequiredColumn, lineNumber, field.Name, fieldDbValue.Field.Name));
                    }

                    article.FieldValues.Add(fieldDbValue);
                }
            }

            ReadServiceFields(ref article, fieldValues, isExtension);
        }

        private static bool IsEmpty(string value) => string.IsNullOrWhiteSpace(value)
            || value == "NULL"
            || value.Length >= 2
            && value.First() == '"'
            && value.Last() == '"'
            && (value.Length == 2 || string.IsNullOrWhiteSpace(value.Substring(1, value.Length - 2)));

        private static string PrepareValue(string inittialValue)
        {
            var value = inittialValue.Replace("\"\"", "\"");
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Remove(0, 1);
                if (value.Length > 0)
                {
                    value = value.Remove(value.Length - 1);
                }
            }

            return value;
        }

        private void ReadServiceFields(ref Article article, IReadOnlyList<string> fieldValues, bool isExtension)
        {
            if (!isExtension)
            {
                article = ReadUniqueIdField(article, fieldValues);
            }

            if (_importSettings.ImportAction != (int)CsvImportMode.InsertAll)
            {
                article = ReadIsUniqueField(article, fieldValues);
            }

            if (_importSettings.ImportAction == (int)CsvImportMode.UpdateIfChanged)
            {
                article = ReadIsChangedStatus(article, fieldValues);
            }
        }

        private Article ReadUniqueIdField(Article article, IReadOnlyList<string> fieldValues)
        {
            var fieldIndex = _titleHeaders.IndexOf(FieldName.UniqueId);
            var guidString = fieldIndex == -1 ? null : fieldValues[fieldIndex];
            if (!string.IsNullOrWhiteSpace(guidString))
            {
                article.UniqueId = Guid.Parse(guidString);
            }

            return article;
        }

        private Article ReadIsChangedStatus(Article article, IReadOnlyList<string> fieldValues)
        {
            var fieldIndex = _titleHeaders.IndexOf(FieldName.IsChanged);
            article.Created = fieldValues[fieldIndex] == "1" ? DateTime.MinValue : DateTime.Now;
            return article;
        }

        private Article ReadIsUniqueField(Article article, IReadOnlyList<string> fieldValues)
        {
            string fieldName = null;
            if (article.ContentId == _contentId)
            {
                fieldName = _importSettings.UniqueFieldToUpdate;
            }
            else
            {
                if (_importSettings.UniqueAggregatedFieldsToUpdate.ContainsKey(article.ContentId))
                {
                    fieldName = _importSettings.UniqueAggregatedFieldsToUpdate[article.ContentId];
                }
            }

            var fieldIndex = _titleHeaders.IndexOf(fieldName);
            Ensure.NotEqual(fieldIndex, -1, ImportStrings.UniqueNotSpecified);

            if (_importSettings.UniqueContentField == null || article.ContentId != _contentId)
            {
                var dbId = 0;
                if (!string.IsNullOrEmpty(fieldValues[fieldIndex]) && !int.TryParse(fieldValues[fieldIndex], out dbId))
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
                var value = PrepareValue(fieldValues[fieldIndex]);
                if (value != "NULL" && !string.IsNullOrEmpty(value) && value != "\"\"")
                {
                    var fieldDbValue = new FieldValue { Field = _importSettings.UniqueContentField };
                    FormatFieldValue(_importSettings.UniqueContentField, value, ref fieldDbValue);
                    _uniqueValuesList.Add(fieldDbValue.Value);
                }
                else
                {
                    _uniqueValuesList.Add(null);
                }
            }

            return article;
        }

        private string GetFieldValueForTracing(Field field, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            switch (field.ExactType)
            {
                case FieldExactTypes.Numeric:
                    return MultistepActionHelper.NumericCultureFormat(
                        value, CultureInfo.CurrentCulture.ToString(), "en-US"
                    );
                case FieldExactTypes.Date:
                case FieldExactTypes.Time:
                case FieldExactTypes.DateTime:
                    return MultistepActionHelper.DateCultureFormat(
                        value, CultureInfo.CurrentCulture.ToString(), "en-US"
                    );
                default:
                    return value;

            }
        }

        private void FormatFieldValue(Field field, string value, ref FieldValue fieldDbValue)
        {
            switch (field.ExactType)
            {
                case FieldExactTypes.Numeric:
                    fieldDbValue.Value = MultistepActionHelper.NumericCultureFormat(
                        value, _importSettings.Culture, "en-US"
                    );
                    break;
                case FieldExactTypes.Date:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(
                        value, _importSettings.Culture, "en-US", field.DenyPastDates
                    );
                    break;
                case FieldExactTypes.Time:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(
                        value, _importSettings.Culture, "en-US"
                    );
                    break;
                case FieldExactTypes.DateTime:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(
                        value, _importSettings.Culture, "en-US", field.DenyPastDates
                    );
                    break;
                case FieldExactTypes.O2MRelation:
                    fieldDbValue.Value = MultistepActionHelper.O2MFormat(value);
                    break;
                case FieldExactTypes.M2MRelation:
                    fieldDbValue.NewRelatedItems = MultistepActionHelper.M2MFormat(value).ToArray();
                    fieldDbValue.Value = field.LinkId?.ToString();
                    break;
                case FieldExactTypes.M2ORelation:
                    break;
                default:
                    fieldDbValue.Value = value;
                    break;
            }
        }

        private ExtendedArticleList GetArticles(ExtendedArticleList articlesList, bool onlyExisting)
        {
            ExtendedArticleList existingArticles;
            if (_importSettings.UniqueContentField == null)
            {
                var existingIds = GetExistingArticleIds(articlesList.GetBaseArticleIds());
                existingArticles = articlesList.Filter(a => !onlyExisting ^ existingIds.Contains(a.Id));
            }
            else
            {
                existingArticles = new ExtendedArticleList(articlesList);
                var existingIdsMap = GetExistingArticleIdsMap(_uniqueValuesList, _importSettings.UniqueContentField.Name);
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

        private ExtendedArticleList InsertArticlesToDb(ExtendedArticleList articleList)
        {
            var baseArticles = articleList.GetBaseArticles();
            var idsList = InsertArticlesIds(baseArticles, baseArticles.All(a => a.UniqueId.HasValue)).ToArray();
            _notificationRepository.PrepareNotifications(_contentId, idsList, NotificationCode.Create);
            InsertArticleValues(idsList.ToArray(), baseArticles);

            if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
            {
                SaveNewRelationsToFile(baseArticles, idsList);
            }

            for (var i = 0; i < idsList.Length; i++)
            {
                var id = idsList[i];
                var article = articleList[i];
                foreach (var aggregatedArticle in article.Extensions.Values.Where(ex => ex != null))
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
                if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(aggregatedArticleList, aggregatedIdsList);
                }
            }

            _notificationRepository.SendNotifications();
            return articleList;
        }

        private ExtendedArticleList UpdateArticlesToDb(ExtendedArticleList articlesList)
        {
            var existingArticles = GetArticles(articlesList, true);
            var extensionsMap = ContentRepository.GetAggregatedArticleIdsMap(_contentId, existingArticles.GetBaseArticleIds().ToArray());
            var idsList = existingArticles.GetBaseArticleIds().ToArray();
            _notificationRepository.PrepareNotifications(_contentId, idsList, NotificationCode.Update);
            var articlesInDb = _traceFields.Any() ?
                ArticleRepository.GetList(idsList, true).ToDictionary(n => n.Id, m => m) : new Dictionary<int, Article>();


            InsertArticleValues(idsList, existingArticles.GetBaseArticles(), true);

            var idsToDelete = new List<int>();
            var articlesToInsert = new List<Article>();
            var idsToUpdate = new List<int>();
            var articlesToUpdate = new List<Article>();
            foreach (var article in existingArticles)
            {
                if (_traceFields.Any())
                {
                    var newArticle = article.BaseArticle;
                    var oldArticle = articlesInDb[article.BaseArticle.Id];
                    RegisterTraceFieldValues(newArticle, oldArticle);
                }

                foreach (var afv in article.Extensions.Where(ex => ex.Value != null))
                {
                    var aggregatedArticle = afv.Value;
                    var fieldId = afv.Key.Id;
                    if (extensionsMap.ContainsKey(article.BaseArticle.Id) && extensionsMap[article.BaseArticle.Id].ContainsKey(fieldId))
                    {
                        var currentId = extensionsMap[article.BaseArticle.Id][fieldId];
                        if (currentId != aggregatedArticle.Id)
                        {
                            aggregatedArticle.Id = currentId;
                        }
                        idsToUpdate.Add(aggregatedArticle.Id);
                        articlesToUpdate.Add(aggregatedArticle);
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
                if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(articlesToInsert, inserdedIds);
                }

                idsToUpdate.AddRange(inserdedIds);
                articlesToUpdate.AddRange(articlesToInsert);
            }

            InsertArticleValues(idsToUpdate.ToArray(), articlesToUpdate, true);
            _notificationRepository.SendNotifications();
            return existingArticles;
        }

        private void RegisterTraceFieldValues(Article newArticle, Article oldArticle)
        {
            if (!_traceFields.Any())
            {
                return;
            }

            var newTraceFieldValues = newArticle.FieldValues.Where(n => n.Field.TraceImport).ToDictionary(n => n.Field.Id, m => m.Value);
            var oldTraceFieldValues = oldArticle.FieldValues.Where(n => n.Field.TraceImport).ToDictionary(n => n.Field.Id, m => m.Value);

            foreach (var field in _traceFields.Values)
            {
                var localObj = new JObject();
                localObj.Add("id", newArticle.Id);
                localObj.Add("new", newTraceFieldValues.TryGetValue(field.Id, out var newFvData) ? newFvData : "");
                localObj.Add("old", GetFieldValueForTracing(
                    field, oldTraceFieldValues.TryGetValue(field.Id, out var oldFvData) ? oldFvData : ""
                ));
                ((JArray)_jObject[field.Name]).Add(localObj);
            }
        }

        private static List<int> InsertArticlesIds(ICollection<Article> articleList, bool preserveGuids = false)
        {
            var i = 0;
            var query = new StringBuilder();
            foreach (var article in articleList)
            {
                var unionAll = ++i == articleList.Count ? string.Empty : " UNION ALL ";
                query.AppendLine(preserveGuids
                    ? $"SELECT {Convert.ToInt32(article.Visible)}, {article.StatusTypeId}, {article.ContentId}, {QPContext.CurrentUserId}, '{article.UniqueId}' {unionAll}"
                    : $"SELECT {Convert.ToInt32(article.Visible)}, {article.StatusTypeId}, {article.ContentId}, {QPContext.CurrentUserId} {unionAll}"
                );
            }

            return ArticleRepository.InsertArticleIds(query.ToString(), preserveGuids);
        }

        private static void InsertArticleValues(int[] idsList, IList<Article> articleList, bool updateArticles = false)
        {
            var guidsByIdToUpdate = new List<Tuple<int, Guid>>();
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
                var article = articleList[k++];
                if (article != null)
                {
                    foreach (var fieldValue in article.FieldValues)
                    {
                        var fieldValueXml = GetFieldValueElement(fieldValue, articleId);
                        doc.Root?.Add(fieldValueXml);
                        switch (fieldValue.Field.ExactType)
                        {
                            case FieldExactTypes.M2MRelation:
                                m2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                                break;
                            case FieldExactTypes.O2MRelation:
                                o2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                                o2MDoc.Root?.Add(fieldValueXml);
                                break;
                        }
                    }
                }

                if (updateArticles && article.UniqueId.HasValue)
                {
                    guidsByIdToUpdate.Add(new Tuple<int, Guid>(article.Id, article.UniqueId.Value));
                }
            }

            ArticleRepository.ValidateO2MValues(o2MDoc.ToString(SaveOptions.None));
            ValidateO2MRelationSecurity(o2MValues);
            ArticleRepository.InsertArticleValues(doc.ToString(SaveOptions.None));
            UpdateM2MValues(m2MValues);

            ArticleRepository.UpdateArticleGuids(guidsByIdToUpdate);
        }

        private static XElement GetFieldValueElement(FieldValue fieldValue, int articleId)
        {
            var fieldValueXml = new XElement("FIELDVALUE");
            var value = fieldValue.Value;
            if (fieldValue.Value == "NULL")
            {
                value = string.Empty;
            }

            fieldValueXml.Add(new XElement(FieldName.ContentItemId, articleId));
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

        private static void ValidateO2MRelationSecurity(List<KeyValuePair<int, FieldValue>> o2MValues)
        {
            var fieldsToCheck = o2MValues.Select(n => n.Value.Field).Distinct().Where(n => n.UseRelationSecurity);
            foreach (var field in fieldsToCheck)
            {
                var ids = o2MValues.Where(n => Equals(n.Value.Field, field))
                    .Select(n => n.Value.Value)
                    .Distinct()
                    .Select(int.Parse)
                    .ToArray();

                if (field.RelateToContentId != null)
                {
                    var notAccessed = new HashSet<string>(ArticleRepository.CheckRelationSecurity(field.RelateToContentId.Value, ids, false)
                        .Where(n => !n.Value)
                        .Select(n => n.Key.ToString())
                        .ToArray());

                    if (notAccessed.Any())
                    {
                        var errorItem = o2MValues.First(n => notAccessed.Contains(n.Value.Value));
                        throw new ArgumentException(string.Format(ImportStrings.InaccessibleO2M, errorItem.Key, field.Name, errorItem.Value.Value));
                    }
                }
            }
        }

        // Добавление значений m2m и o2m полей
        public void PostUpdateM2MRelationAndO2MRelationFields()
        {
            if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
            {
                //get all relations between old and new article ids
                var values = GetNewValues();
                if (values.Any())
                {
                    var repo = new NotificationPushRepository();
                    repo.PrepareNotifications(_contentId, values.Select(n => n.NewId).Distinct().ToArray(), NotificationCode.Update);
                    PostUpdateM2MValues(values);
                    PostUpdateO2MValues(values);
                    repo.SendNotifications();
                }
            }
        }

        // Обновление значений o2m полей
        private static void PostUpdateO2MValues(List<RelSourceDestinationValue> m2MValues)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var item in m2MValues.Where(s => !s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    var oldId = item.NewRelatedItems[0];
                    var oldElem = m2MValues.FirstOrDefault(s => s.OldId == oldId);
                    if (oldElem != null && item.OldId != oldId)
                    {
                        var itemXml = new XElement("item");
                        itemXml.Add(new XAttribute("id", item.NewId));
                        itemXml.Add(new XAttribute("linked_id", oldElem.NewId));
                        itemXml.Add(new XAttribute("field_id", item.FieldId));
                        doc.Root?.Add(itemXml);
                    }
                }
            }

            ArticleRepository.InsertO2MFieldValues(doc.ToString(SaveOptions.None));
        }

        // Обновление значений m2m полей
        private static void PostUpdateM2MValues(List<RelSourceDestinationValue> m2MValues)
        {
            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var item in m2MValues.Where(s => s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    var result = GetM2MRelatedArtsWithNewIds(item.NewRelatedItems, m2MValues);
                    if (result.Any())
                    {
                        var itemXml = new XElement("item");
                        itemXml.Add(new XAttribute("id", item.NewId));
                        itemXml.Add(new XAttribute("linkId", item.FieldId));
                        itemXml.Add(new XAttribute("value", string.Join(",", result)));
                        doc.Root?.Add(itemXml);
                    }
                }
            }

            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }

        // Получение списка серверных id вместо пользовательских
        private static IEnumerable<int> GetM2MRelatedArtsWithNewIds(IEnumerable<int> oldRelatedIds, List<RelSourceDestinationValue> m2MValues)
        {
            var result = new List<int>();
            foreach (var oldId in oldRelatedIds)
            {
                var value = m2MValues.FirstOrDefault(s => s.OldId == oldId);
                if (value != null)
                {
                    result.Add(value.NewId);
                }
            }

            return result;
        }

        private static void UpdateM2MValues(List<KeyValuePair<int, FieldValue>> values)
        {
            var m2MFields = new Dictionary<string, Field>();
            foreach (var fieldV in values)
            {
                if (!m2MFields.Keys.Contains(fieldV.Value.Field.Name))
                {
                    m2MFields.Add(fieldV.Value.Field.Name, fieldV.Value.Field);
                }
            }

            var doc = new XDocument();
            var items = new XElement("items");
            doc.Add(items);

            foreach (var field in m2MFields.Values)
            {
                var m2MField = field;
                var condition = m2MField.RelationCondition;
                var linkId = m2MField.LinkId ?? 0;
                if (m2MField.RelateToContentId != null)
                {
                    var contentId = m2MField.RelateToContentId.Value;
                    var filteredValues = values.Where(f => f.Value.Field.Name == m2MField.Name).ToArray();
                    var relatedIds = filteredValues
                        .Where(n => n.Value.NewRelatedItems != null)
                        .SelectMany(n => n.Value.NewRelatedItems)
                        .Distinct()
                        .ToList();

                    var validatedIds = new HashSet<int>(ArticleRepository.CheckForArticleExistence(relatedIds, condition, contentId));
                    var grantedIds = field.UseRelationSecurity
                        ? new HashSet<int>(ArticleRepository.CheckRelationSecurity(contentId, validatedIds.ToArray(), false).Where(n => n.Value).Select(m => m.Key))
                        : validatedIds;

                    var archiveArticles = ArticleRepository.CheckArchiveArticles(relatedIds.ToArray());

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
                            item.Value.NewRelatedItems = item.Value.NewRelatedItems.Where(w=> !archiveArticles.Contains(w)).ToArray();
                            value = string.Join(",", item.Value.NewRelatedItems);
                        }

                        var itemXml = new XElement("item");
                        itemXml.Add(new XAttribute("id", item.Key));
                        itemXml.Add(new XAttribute("linkId", linkId));
                        itemXml.Add(new XAttribute("value", value));
                        doc.Root?.Add(itemXml);
                    }
                }
            }

            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }

        // Сериализация соответствий новых статей в файл
        private void SaveNewRelationsToFile(IList<Article> articles, IEnumerable<int> idsList)
        {
            var k = 0;
            var m2MValues = new List<RelSourceDestinationValue>();
            foreach (var id in idsList)
            {
                var art = articles[k];
                foreach (var fv in art.FieldValues)
                {
                    if (fv.Field.RelateToContentId != null && fv.Field.ExactType == FieldExactTypes.M2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        if (fv.Field.LinkId != null)
                        {
                            var val = new RelSourceDestinationValue
                            {
                                NewId = id,
                                OldId = art.Id,
                                FieldId = fv.Field.LinkId.Value,
                                NewRelatedItems = fv.NewRelatedItems,
                                IsM2M = true
                            };

                            m2MValues.Add(val);
                        }
                    }
                    else if (fv.Field.RelateToContentId != null && fv.Field.ExactType == FieldExactTypes.O2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        var val = new RelSourceDestinationValue
                        {
                            NewId = id,
                            OldId = art.Id,
                            FieldId = fv.Field.Id,
                            NewRelatedItems = string.IsNullOrEmpty(fv.Value) ? null : new[] { int.Parse(fv.Value) },
                            IsM2M = false
                        };

                        m2MValues.Add(val);
                    }
                }

                k++;
            }

            try
            {
                using (Stream sw = File.Open(_importSettings.TempFileForRelFields, FileMode.Append))
                {
                    var bin = new BinaryFormatter();
                    bin.Serialize(sw, m2MValues);
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        // Десериализация всех соответствий (после добавления статей)
        private List<RelSourceDestinationValue> GetNewValues()
        {
            var m2MValues = new List<RelSourceDestinationValue>();
            if (File.Exists(_importSettings.TempFileForRelFields))
            {
                try
                {
                    using (Stream stream = File.Open(_importSettings.TempFileForRelFields, FileMode.Open))
                    {
                        var bin = new BinaryFormatter();

                        while (stream.Position != stream.Length)
                        {
                            var vals = (List<RelSourceDestinationValue>)bin.Deserialize(stream);
                            m2MValues.AddRange(vals);
                        }
                    }
                }
                catch
                {
                    throw new ArgumentException();
                }
            }

            return m2MValues;
        }

        private static void SaveUpdatedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
            settings?.UpdatedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] = settings;
        }

        private static void SaveInsertedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] as ImportSettings;
            settings?.InsertedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session[HttpContextSession.ImportSettingsSessionKey] = settings;
        }

        private List<int> GetExistingArticleIds(List<int> articlesIdList) => ArticleRepository.CheckForArticleExistence(articlesIdList, string.Empty, _contentId);

        private Dictionary<string, int> GetExistingArticleIdsMap(List<string> values, string fieldName) => ArticleRepository.GetExistingArticleIdsMap(values, fieldName, string.Empty, _contentId);

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
                doc.Root?.Add(itemXml);
            }

            ArticleRepository.UpdateArticlesDateTime(doc.ToString(SaveOptions.None));
        }

        private string[] SplitToValues(int countColumns, string line) => line.Split(_importSettings.Delimiter).Length == countColumns ? line.Split(_importSettings.Delimiter) : ParseLine(line);

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
                else if (lineByChar[i] == _importSettings.Delimiter)
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

        private Article InitializeArticle(int contentId) => new Article
        {
            Status = _importSettings.IsWorkflowAssigned ? StatusType.GetNone(_siteId) : StatusType.GetPublished(_siteId),
            StatusTypeId = _importSettings.IsWorkflowAssigned
                ? StatusTypeRepository.GetNoneStatusIdBySiteId(_siteId)
                : StatusTypeRepository.GetPublishedStatusIdBySiteId(_siteId),
            Visible = true,
            ContentId = contentId,
            FieldValues = new List<FieldValue>()
        };

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

            var columnsCount = firstLine.Split(delimiter).Length;
            var columnIndexes = Enumerable.Range(0, columnsCount).ToArray();
            return columnIndexes.Select(k => k.ToString()).ToList();
        }

        private void InitFields()
        {
            _fieldsMap = _importSettings
                .FieldsList.Select(f => f.Value)
                .GroupBy(f => f.ContentId)
                .Select(g => g)
                .ToDictionary(g => g.Key, g => g.ToList());

            _headersMap = _importSettings.FieldsList.ToDictionary(f => f.Value, f => _titleHeaders.FindIndex(s => s == f.Key));
            _aggregatedContentsMap = ContentRepository.GetAggregatedContents(_contentId).ToDictionary(c => c.Id, c => c);
            _articlesListFromCsv = new ExtendedArticleList();
            _uniqueValuesList = new List<string>();
        }
    }

    public class FileReader
    {
        private readonly ImportSettings _settings;
        private readonly Lazy<IEnumerable<Line>> _lines;

        public FileReader(ImportSettings settings)
        {
            _settings = settings;
            _lines = new Lazy<IEnumerable<Line>>(() => ReadFile(_settings));
        }

        public IEnumerable<Line> Lines => _lines.Value;

        public void CopyFileToTempDir()
        {
            var fileInfo = new FileInfo(HttpUtility.UrlDecode(_settings.UploadFilePath) ?? string.Empty);
            if (fileInfo.Exists)
            {
                var newFileUploadPath = $"{QPConfiguration.TempDirectory}\\{fileInfo.Name}";
                if (!File.Exists(newFileUploadPath))
                {
                    File.Copy(_settings.UploadFilePath, newFileUploadPath, true);
                }
            }
            else
            {
                throw new FileNotFoundException($"File {_settings.UploadFilePath} was not found.");
            }
        }

        public static IEnumerable<Line> ReadFile(ImportSettings setts)
        {
            using (var sr = new StreamReader(setts.UploadFilePath))
            {
                string line;
                var rdr = new CustomStreamReader(sr.BaseStream, Encoding.GetEncoding(setts.Encoding), setts.LineSeparator, setts.Delimiter);
                var i = 0;
                var headerNum = 1;
                while (!string.IsNullOrEmpty(line = rdr.ReadLine()))
                {
                    i++;
                    var value = line.Trim('\r', '\n');
                    var isSep = value.StartsWith("sep=");
                    if (isSep)
                    {
                        headerNum++;
                    }

                    var skip = !setts.NoHeaders && i == headerNum || string.IsNullOrEmpty(value) || isSep;
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
            var res = line.Trim(_fieldDelimiter, '"', '\r', '\n');
            return string.IsNullOrEmpty(res) || string.IsNullOrWhiteSpace(line);
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
                if (ch == sep && !quoteOpen && (lineSepArr.Length == 1 || lineSepArr.Length == 2 && lastCh == lineSepArr[0]))
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

    public class ExtendedArticle
    {
        public Article BaseArticle { get; }

        public Dictionary<Field, Article> Extensions { get; }

        public ExtendedArticle(Article baseArticle)
        {
            BaseArticle = baseArticle;
            Extensions = new Dictionary<Field, Article>();
        }
    }

    public class ExtendedArticleList : List<ExtendedArticle>
    {
        public HashSet<Field> ExtensionFields { get; }

        public ExtendedArticleList()
        {
            ExtensionFields = new HashSet<Field>();
        }

        public ExtendedArticleList(ExtendedArticleList articles)
        {
            ExtensionFields = new HashSet<Field>(articles.ExtensionFields);
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
            return this.Select(a => a.Extensions[field]).ToList();
        }

        public IEnumerable<List<Article>> GetAllAggregatedArticles() => ExtensionFields.Select(GetAggregatedArticles);

        public ExtendedArticleList Filter(Func<Article, bool> predicate)
        {
            var result = new ExtendedArticleList(this);
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
