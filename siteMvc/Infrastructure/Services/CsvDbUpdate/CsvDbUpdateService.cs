using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using ILogger = NLog.ILogger;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate
{
    public class CsvDbUpdateService : ICsvDbUpdateService
    {
        private const char CsvRelationSeparator = ';';

        private readonly IContentRepository _contentRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly IBatchUpdateService _articleService;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public CsvDbUpdateService(IBatchUpdateService articleService, IFieldRepository fieldRepository, IContentRepository contentRepository, IArticleRepository articleRepository)
        {
            _contentRepository = contentRepository;
            _articleRepository = articleRepository;
            _fieldRepository = fieldRepository;
            _articleService = articleService;
        }

        public void Process(IEnumerable<CsvDbUpdateModel> data, bool updateExisting)
        {
            using (var ts = QPConfiguration.CreateTransactionScope(IsolationLevel.ReadCommitted))
            using (new QPConnectionScope())
            {
                var articlesData = new List<ArticleData>();
                foreach (var csvFileData in data)
                {
                    foreach (var csvRowFields in csvFileData.Fields.Values)
                    {
                        var dbFields = GetFieldsByNames(csvFileData.ContentId, csvRowFields);
                        var dataToAdd = CreateArticleDatas(csvFileData.ContentId, csvRowFields, dbFields, updateExisting);
                        dataToAdd = InsertFields(csvFileData.ContentId, dataToAdd, dbFields, csvRowFields);
                        foreach (var extensionContentId in dataToAdd.Where(ad => ad.ContentId != csvFileData.ContentId).Select(ad => ad.ContentId))
                        {
                            var contentNamePrefix = $"{_contentRepository.GetById(extensionContentId).Name}.";
                            var extensionDbFields = GetFieldsByNames(extensionContentId, csvRowFields, contentNamePrefix);
                            dataToAdd = InsertFields(extensionContentId, dataToAdd, extensionDbFields, csvRowFields, contentNamePrefix);
                        }

                        articlesData.AddRange(dataToAdd);
                    }
                }

                articlesData = FilterNotExistedReferences(articlesData);
                _articleService.BatchUpdate(articlesData);
                ts.Complete();
            }
        }

        private List<ArticleData> FilterNotExistedReferences(List<ArticleData> articlesData)
        {
            var listOfIds = articlesData.Select(ad => ad.Id).ToList();
            foreach (var article in articlesData)
            {
                foreach (var articleField in article.Fields)
                {
                    var result = new List<int>();
                    foreach (var relatedId in articleField.ArticleIds)
                    {
                        if (listOfIds.Contains(relatedId))
                        {
                            result.Add(relatedId);
                            continue;
                        }

                        if (listOfIds.Contains(-relatedId))
                        {
                            result.Add(-relatedId);
                            continue;
                        }

                        if (_articleRepository.IsExist(-relatedId))
                        {
                            result.Add(-relatedId);
                            continue;
                        }

                        var msg = "Ignore related article CIID: {relatedArticleId}. " +
                            "Can't find any related article at csv data or db. " +
                            "FID: {fieldId}, CID: {contentId}, CIID: {articleId}";
                        Logger.ForWarnEvent()
                            .Message(msg,-relatedId, articleField.Id, articleField.Id, -article.Id)
                            .Log();
                    }

                    articleField.ArticleIds = result.ToArray();
                }
            }

            return articlesData;
        }

        private IList<ArticleData> CreateArticleDatas(int contentId, IList<CsvDbUpdateFieldModel> csvRowFields, IEnumerable<Field> dbFields, bool updateExisting)
        {
            var result = new List<ArticleData>
            {
                new ArticleData
                {
                    Id = CreateSimpleArticleId(contentId, csvRowFields, updateExisting),
                    ContentId = contentId
                }
            };

            foreach (var dbf in dbFields.Where(dbf => dbf.IsClassifier))
            {
                var fieldValue = csvRowFields.Single(StringFilter(dbf.Name)).Value;
                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    var extensionContentId = Convert.ToInt32(fieldValue);
                    result.Add(new ArticleData
                    {
                        Id = CreateExtensionArticleId(extensionContentId, csvRowFields, updateExisting),
                        ContentId = extensionContentId
                    });
                }
            }

            return result;
        }

        private IList<Field> GetFieldsByNames(int contentId, IEnumerable<CsvDbUpdateFieldModel> csvRowFields, string contentNameFieldPrefix = "")
        {
            var fieldNames = csvRowFields.Where(FilterByPrefix(contentNameFieldPrefix)).Select(f => f.Name.Substring(contentNameFieldPrefix.Length)).ToList();
            return _fieldRepository.GetByNames(contentId, fieldNames);
        }

        private int CreateExtensionArticleId(int extensionContentId, IEnumerable<CsvDbUpdateFieldModel> csvRowFields, bool useExisting)
        {
            var contentName = _contentRepository.GetById(extensionContentId).Name;
            var getExtensionContentId = StringFilter($"{contentName}.{FieldName.ContentItemId}");
            var result = Convert.ToInt32(csvRowFields.Single(getExtensionContentId).Value);
            return useExisting && _articleRepository.GetContentId(result) == extensionContentId ? result : -result;
        }

        private int CreateSimpleArticleId(int contentId, IEnumerable<CsvDbUpdateFieldModel> csvRowFields, bool useExisting)
        {
            var result = Convert.ToInt32(csvRowFields.Single(StringFilter(FieldName.ContentItemId)).Value);
            return useExisting && _articleRepository.GetContentId(result) == contentId ? result : -result;
        }

        private static IList<ArticleData> InsertFields(int contentId, IList<ArticleData> dataToAdd, IEnumerable<Field> dbFields, IList<CsvDbUpdateFieldModel> csvRowFields, string contentNameFieldPrefix = "")
        {
            foreach (var dbf in dbFields)
            {
                var fieldData = new FieldData
                {
                    Id = dbf.Id,
                    Value = csvRowFields.Single(f => string.Equals(f.Name, $"{contentNameFieldPrefix}{dbf.Name}")).Value
                };

                if (dbf.RelationType != RelationType.None && !string.IsNullOrWhiteSpace(fieldData.Value))
                {
                    fieldData.ArticleIds = fieldData.Value.Split(CsvRelationSeparator).Select(f => -int.Parse(f)).ToArray();
                }
                else if (dbf.IsClassifier && !string.IsNullOrWhiteSpace(fieldData.Value))
                {
                    fieldData.ArticleIds = new[] { dataToAdd.Single(ad => ad.ContentId == Convert.ToInt32(fieldData.Value)).Id };
                }

                dataToAdd.Single(ad => ad.ContentId == contentId).Fields.Add(fieldData);
            }

            return dataToAdd;
        }

        private static Func<CsvDbUpdateFieldModel, bool> StringFilter(string fieldName)
        {
            return field => string.Equals(field.Name, fieldName, StringComparison.OrdinalIgnoreCase);
        }

        private static Func<CsvDbUpdateFieldModel, bool> FilterByPrefix(string contentNameFieldPrefix)
        {
            return field => field.Name.StartsWith(contentNameFieldPrefix);
        }
    }
}
