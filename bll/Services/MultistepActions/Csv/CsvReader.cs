using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using QP8.Infrastructure.Web.Extensions;
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
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _siteId;
        private readonly int _contentId;
        private readonly FileReader _reader;
        private readonly ImportSettings _importSettings;
        private readonly NotificationPushRepository _notificationRepository;
        private readonly bool _skipFirstLine;
        private readonly bool _skipSecondLine;
        private int _initNumber;
        private List<string> _titleHeaders;
        private Dictionary<int, List<Field>> _fieldsMap;
        private Dictionary<Field, int> _headersMap;
        private Dictionary<int, Content> _aggregatedContentsMap;
        private ExtendedArticleList _articlesListFromCsv;
        private List<string> _uniqueValuesList;
        private IEnumerable<Line> _csvLines;
        private readonly JObject _jObject;
        private readonly Dictionary<int, Field> _traceFields;
        private readonly PathHelper _pathHelper;
        private readonly IBackendActionLogRepository _logRepository;

        public CsvReader(int siteId, int contentId, ImportSettings settings, PathHelper pathHelper)
        {
            _siteId = siteId;
            _contentId = contentId;
            _importSettings = settings;
            _reader = new FileReader(settings);
            _skipFirstLine = _reader.Lines.First().Skip;
            _skipSecondLine = _skipFirstLine && _reader.Lines.Skip(1).First().Skip;
            _notificationRepository = new() { IgnoreInternal = false };
            _traceFields = GetTraceFields(contentId);
            _jObject = InitJObject(_traceFields);
            _pathHelper = pathHelper;
            _logRepository = new AuditRepository();
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
            _csvLines = _reader.Lines.Where(s => !s.Skip).Skip(step * itemsPerStep).Take(itemsPerStep).ToArray();
            _initNumber = _csvLines.First().Number - 1;
            _titleHeaders = MultistepActionHelper.GetFileFields(_importSettings, _reader);
            var fields = FieldRepository.GetList(_importSettings.FieldsList.Select(n => n.Value))
                .ToDictionary(n => n.Id.ToString(), m => m);
            InitFields(_importSettings, fields);
            ConvertCsvLinesToArticles(fields);
            WriteArticlesToDb(step * itemsPerStep >= ArticleCount - itemsPerStep);
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
                if (_initNumber == 0 || _initNumber == 1 && _skipFirstLine || _initNumber == 2 && _skipSecondLine)
                {
                    return @"N/A";
                }
                return _initNumber.ToString();
            }
        }

        private void WriteArticlesToDb(bool lastStep = false)
        {
            List<NotificationArticles> notificationList = new();

            using (TransactionScope ts = QPConfiguration.CreateTransactionScope(IsolationLevel.ReadCommitted))
            {
                switch (_importSettings.ImportAction)
                {
                    case (int)CsvImportMode.InsertAll:
                        InsertArticles(_articlesListFromCsv, ref notificationList);

                        break;
                    case (int)CsvImportMode.InsertAndUpdate:
                        var existingArticles = UpdateArticles(_articlesListFromCsv, ref notificationList);
                        var remainingArticles = _articlesListFromCsv.Filter(a => !existingArticles.GetBaseArticleIds().Contains<int>(a.Id));
                        InsertArticles(remainingArticles, ref notificationList);

                        break;
                    case (int)CsvImportMode.Update:
                        UpdateArticles(_articlesListFromCsv, ref notificationList);

                        break;
                    case (int)CsvImportMode.UpdateIfChanged:
                        var changedArticles = _articlesListFromCsv.Filter(a => a.Created == DateTime.MinValue);
                        UpdateArticles(changedArticles, ref notificationList);

                        break;
                    case (int)CsvImportMode.InsertNew:
                        var nonExistingArticles = GetArticles(_articlesListFromCsv, false);
                        InsertArticles(nonExistingArticles, ref notificationList);

                        break;
                    default:
                        throw new NotImplementedException($"Неизвестный режим импорта: {_importSettings.ImportAction}");
                }

                if (lastStep)
                {
                    PostUpdateM2MRelationAndO2MRelationFields(ref notificationList);
                    ContentRepository.UpdateContentModification(_contentId);
                }

                ts.Complete();
            }

            NotifyProcessedArticles(notificationList);
        }

        private void NotifyProcessedArticles(List<NotificationArticles> notificationList)
        {
            foreach (NotificationArticles notificationArticles in notificationList)
            {
                var code = notificationArticles.NotificationCode == NotificationCode.Create ?
                    ActionCode.MultipleSaveArticles : ActionCode.MultipleUpdateArticles;
                BackendActionContext.CreateLogs(code, notificationArticles.ArticleIds, _contentId, _logRepository);
                try
                {
                    _notificationRepository.PrepareNotifications(_contentId, notificationArticles.ArticleIds.ToArray(), notificationArticles.NotificationCode);
                    _notificationRepository.SendBatchNotification();
                }
                catch (Exception e)
                {
                    Logger.ForErrorEvent()
                       .Exception(e)
                       .Message("Error while sending notifications for articles {Articles} with code {Code}",
                            string.Join(", ", notificationArticles.ArticleIds),
                            notificationArticles.NotificationCode)
                       .Log();
                }
            }
        }

        private void InsertArticles(ExtendedArticleList articlesToInsert, ref List<NotificationArticles> notificationArticlesList)
        {
            if (articlesToInsert.Any())
            {
                var insertingArticles = InsertArticlesToDb(articlesToInsert);
                var insertedArticles = insertingArticles.GetBaseArticleIds();
                SaveInsertedArticleIdsToSettings(insertedArticles);
                notificationArticlesList.Add(new(insertedArticles, NotificationCode.Create));
            }
        }

        private ExtendedArticleList UpdateArticles(ExtendedArticleList articlesToUpdate, ref List<NotificationArticles> notificationArticlesList)
        {
            if (articlesToUpdate.Any())
            {
                var updatingArticles = UpdateArticlesToDb(articlesToUpdate);
                var updatedArticles = updatingArticles.GetBaseArticleIds();
                SaveUpdatedArticleIdsToSettings(updatedArticles);
                notificationArticlesList.Add(new(updatedArticles, NotificationCode.Update));
                return updatingArticles;
            }

            return articlesToUpdate;
        }

        private void ConvertCsvLinesToArticles(Dictionary<string, Field> fields)
        {
            foreach (var line in _csvLines)
            {
                if (string.IsNullOrEmpty(line.Value))
                {
                    continue;
                }

                var fieldValues = SplitToValues(_titleHeaders.Count, line.Value);
                var baseArticle = InitializeArticle(_contentId);

                var index = _titleHeaders.IndexOf(_importSettings.UniqueFieldToUpdate);
                index = index == -1 ? 0 : index;

                if (int.TryParse(fieldValues[index], out var articleId))
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
                    var classifierFields = fields.Where(w => w.Value.IsClassifier).Select(n => n.Value).ToArray();

                    foreach (var classifier in classifierFields)
                    {
                        foreach (var classifierContentId in _aggregatedContentsMap.Where(w => w.Key != classifier.ContentId).Select(s => s.Key))
                        {
                            if (_importSettings.FieldsList.Where(w => w.Key != "-1" && fields[w.Key].ContentId == classifierContentId).Any())
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
            var field = (_importSettings.UniqueContentFieldId != 0) ? FieldRepository.GetById(_importSettings.UniqueContentFieldId) : null;

            if (field == null || article.ContentId != _contentId)
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

                    var fieldDbValue = new FieldValue { Field = field };
                    FormatFieldValue(field, value, ref fieldDbValue);
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
            var uniqueField = FieldRepository.GetById(_importSettings.UniqueContentFieldId);
            if (uniqueField == null)
            {
                var existingIds = GetExistingArticleIds(articlesList.GetExistingBaseArticleIds());
                existingArticles = articlesList.Filter(a => !onlyExisting ^ existingIds.Contains(a.Id));
            }
            else
            {
                existingArticles = new ExtendedArticleList(articlesList);
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

        private ExtendedArticleList InsertArticlesToDb(ExtendedArticleList articleList)
        {
            var baseArticles = articleList.GetBaseArticles();
            var idsList = InsertArticlesIds(baseArticles, baseArticles.All(a => a.UniqueId.HasValue)).ToArray();
            var defaultValues = Article.CreateNew(_contentId).FieldValues;
            var notMappedDefaultValues = defaultValues
                .Where(
                    n => !String.IsNullOrEmpty(n.Field.DefaultValue)
                    && _headersMap.ContainsKey(n.Field)
                    && _headersMap[n.Field] == -1
                ).ToList();
            InsertArticleValues(idsList.ToArray(), baseArticles, notMappedDefaultValues);

            if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
            {
                SaveNewRelationsToFile(baseArticles, idsList);
            }

            for (var i = 0; i < idsList.Length; i++)
            {
                var id = idsList[i];
                var article = articleList[i];
                article.BaseArticle.Id = id;
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

            return articleList;
        }

        private ExtendedArticleList UpdateArticlesToDb(ExtendedArticleList articlesList)
        {
            var existingArticles = GetArticles(articlesList, true);
            var extensionsMap = ContentRepository.GetAggregatedArticleIdsMap(_contentId, existingArticles.GetBaseArticleIds().ToArray());
            var idsList = existingArticles.GetBaseArticleIds().ToArray();

            if (_importSettings.CreateVersions)
            {
                var versionsToDelete = ArticleVersionRepository.GetVersionsToDelete(idsList);
                ArticleVersionRepository.Create(idsList);
                var content = ContentRepository.GetById(_contentId);
                DeleteVersionFolders(versionsToDelete.Keys.ToArray(), content);
                CreateVersionFolders(idsList, content);
            }

            var articlesInDb = _traceFields.Any() ?
                ArticleRepository.GetList(idsList, true).ToDictionary(n => n.Id, m => m) : new Dictionary<int, Article>();

            InsertArticleValues(idsList, existingArticles.GetBaseArticles(), updateArticles: true);

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

            InsertArticleValues(idsToUpdate.ToArray(), articlesToUpdate, updateArticles: true);
            return existingArticles;
        }

        private void CreateVersionFolders(int[] idsList, Content content)
        {
            var newVersions = ArticleVersionRepository.GetLatestVersions(idsList);
            var files = ArticleVersionRepository.GetFilesForVersions(newVersions.Keys.ToArray());
            var currentVersionFolders = new Dictionary<int, string> {{
                content.Id,
                content.GetVersionPathInfo(ArticleVersion.CurrentVersionId).Path
            }};

            foreach (var newVersion in newVersions.Keys)
            {
                var versionFolder = content.GetVersionPathInfo(newVersion).Path;
                if (files.TryGetValue(newVersion, out var versionFiles))
                {
                    if (!_pathHelper.UseS3)
                    {
                        Directory.CreateDirectory(versionFolder);
                    }
                    foreach (var filePair in versionFiles)
                    {
                        if (!currentVersionFolders.TryGetValue(filePair.Value, out var currentVersionFolder))
                        {
                            currentVersionFolder = ContentRepository.GetById(filePair.Value).GetVersionPathInfo(ArticleVersion.CurrentVersionId).Path;
                            currentVersionFolders.Add(filePair.Value, currentVersionFolder);
                        }
                        var fileName = Path.GetFileName(filePair.Key);
                        var src = _pathHelper.CombinePath(currentVersionFolder, fileName);
                        var dest = _pathHelper.CombinePath(versionFolder, fileName);
                        if (_pathHelper.FileExists(src))
                        {
                            _pathHelper.Copy(src, dest);
                        }
                    }
                }
            }
        }

        private void DeleteVersionFolders(int[] versions, Content content)
        {
            foreach (var version in versions)
            {
                var versionFolder = content.GetVersionPathInfo(version).Path;
                _pathHelper.RemoveFolder(versionFolder);
            }
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
            var doc = new XDocument();
            doc.Add(new XElement("items"));
            foreach (var article in articleList)
            {
                var elem = new XElement("item");
                elem.Add(new XAttribute("visible", Convert.ToInt32(article.Visible)));
                elem.Add(new XAttribute("contentId", article.ContentId));
                elem.Add(new XAttribute("statusId", article.StatusTypeId));
                elem.Add(new XAttribute("userId", QPContext.CurrentUserId));
                if (preserveGuids)
                {
                    elem.Add(new XAttribute("guid", article.UniqueId));
                }
                doc.Root?.Add(elem);
            }

            return ArticleRepository.InsertArticleIds(doc.ToString(), preserveGuids);
        }

        private void InsertArticleValues(int[] idsList, IList<Article> articleList, IList<FieldValue> defaultFieldValues = null, bool updateArticles = false)
        {
            var guidsByIdToUpdate = new List<Tuple<int, Guid>>();
            if (updateArticles)
            {
                ValidateWorkflowStatus(idsList);
                UpdateArticlesDateTime(idsList);
            }

            var defaultFieldValuesList = updateArticles == false && defaultFieldValues != null ? defaultFieldValues : new List<FieldValue>();
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
                    var fieldValues = article.FieldValues.Concat(defaultFieldValuesList);
                    foreach (var fieldValue in fieldValues)
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

        private void ValidateWorkflowStatus(int[] idsList)
        {
            if (QPContext.IsAdmin) return;
            var workflow = ContentRepository.GetById(_contentId).WorkflowBinding;
            if (workflow.IsAssigned)
            {
                var statusList = workflow.AvailableStatuses.Select(n => n.Id).ToArray();
                var wrongIds = ArticleRepository.GetArticleIdsWithWrongStatuses(idsList, statusList);
                if (wrongIds.Any())
                {
                    throw new ArgumentException(String.Format(ImportStrings.InaccessibleByWorkflow, string.Join(",", wrongIds)));
                }
            }
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
        public void PostUpdateM2MRelationAndO2MRelationFields(ref List<NotificationArticles> notificationArticlesList)
        {
            if (_importSettings.ContainsO2MRelationOrM2MRelationFields)
            {
                //get all relations between old and new article ids
                var values = GetNewValues();
                if (values.Any())
                {
                    PostUpdateM2MValues(values);
                    PostUpdateO2MValues(values);
                    notificationArticlesList.Add(new(values.Select(x => x.NewId).Distinct().ToList(), NotificationCode.Update));
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

            using TextWriter textWriter = new StreamWriter(_importSettings.TempFileForRelFields, true, Encoding.UTF8);

            foreach(var relation in m2MValues)
            {
                textWriter.WriteLine(relation.ToString());
            }

            textWriter.Flush();
        }

        // Десериализация всех соответствий (после добавления статей)
        private List<RelSourceDestinationValue> GetNewValues()
        {
            var m2MValues = new List<RelSourceDestinationValue>();
            if (File.Exists(_importSettings.TempFileForRelFields))
            {
                try
                {
                    using TextReader textReader = new StreamReader(_importSettings.TempFileForRelFields, Encoding.UTF8);

                    string line;
                    while (!string.IsNullOrWhiteSpace(line = textReader.ReadLine()))
                    {
                        m2MValues.Add(new RelSourceDestinationValue(line));
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
            var settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            settings?.UpdatedArticleIds.AddRange(articleIds);
            HttpContext.Session.SetValue(HttpContextSession.ImportSettingsSessionKey, settings);
        }

        private static void SaveInsertedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            var settings = HttpContext.Session.GetValue<ImportSettings>(HttpContextSession.ImportSettingsSessionKey);
            settings?.InsertedArticleIds.AddRange(articleIds);
            HttpContext.Session.SetValue(HttpContextSession.ImportSettingsSessionKey, settings);
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

        private void InitFields(ImportSettings importSettings, Dictionary<string, Field> fields)
        {
            _fieldsMap = fields.Select(f => f.Value)
                .GroupBy(f => f.ContentId)
                .Select(g => g)
                .ToDictionary(g => g.Key, g => g.ToList());
            _headersMap = importSettings.FieldsList.ToDictionary(
                k => fields[k.Value.ToString()],
                v => _titleHeaders.FindIndex(s => s == v.Key));
            _aggregatedContentsMap = ContentRepository.GetAggregatedContents(_contentId).ToDictionary(c => c.Id, c => c);
            _articlesListFromCsv = new ExtendedArticleList();
            _uniqueValuesList = new List<string>();
        }
    }
}
