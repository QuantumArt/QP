using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate
{
    public class CsvDbUpdateService : ICsvDbUpdateService
    {
        private const char CsvRelationSeparator = ';';
        private readonly string _connectionString;
        private readonly ArticleService _articleService;

        public CsvDbUpdateService(int userId)
            : this(userId, QPConfiguration.ConfigConnectionString(QPContext.CurrentCustomerCode))
        {
        }

        public CsvDbUpdateService(int userId, string connectionString)
        {
            Ensure.NotNullOrWhiteSpace(connectionString, "Connection string should be initialized");

            _connectionString = connectionString;
            _articleService = new ArticleService(connectionString, userId);
        }

        public void Process(IEnumerable<CsvDbUpdateModel> data)
        {
            var articlesData = new List<ArticleData>();
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(_connectionString))
            {
                foreach (var csvFileData in data)
                {
                    foreach (var csvRowFields in csvFileData.Fields.Values)
                    {
                        var dbFields = FieldRepository.GetByNames(csvFileData.ContentId, csvRowFields.Select(f => f.Name).ToList());
                        var dataToAdd = CreateArticleDatas(csvFileData, csvRowFields, dbFields).ToList();
                        dataToAdd = AddFieldsToArticleDataEntry(dataToAdd, dbFields, csvRowFields, csvFileData.ContentId);
                        foreach (var extensionContentId in dataToAdd.Where(ad => ad.ContentId != csvFileData.ContentId).Select(ad => ad.ContentId))
                        {
                            dataToAdd = AddExtensionFieldsToArticleDataEntry(dataToAdd, csvRowFields, extensionContentId);
                        }

                        articlesData.AddRange(dataToAdd);
                    }
                }

                ts.Complete();
            }

            _articleService.BatchUpdate(articlesData);
        }

        private static List<ArticleData> AddFieldsToArticleDataEntry(List<ArticleData> dataToAdd, IEnumerable<Field> dbFields, IList<CsvDbUpdateFieldModel> csvRowFields, int contentId, string contentNameFieldPrefix = "")
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

        private static List<ArticleData> AddExtensionFieldsToArticleDataEntry(List<ArticleData> dataToAdd, IList<CsvDbUpdateFieldModel> csvRowFields, int contentId)
        {
            var contentNamePrefix = $"{GetContentNameById(contentId)}.";
            var dbFields = GetFieldsByNames(csvRowFields, contentId, contentNamePrefix);
            return AddFieldsToArticleDataEntry(dataToAdd, dbFields, csvRowFields, contentId, contentNamePrefix);
        }

        private static IEnumerable<Field> GetFieldsByNames(IEnumerable<CsvDbUpdateFieldModel> csvRowFields, int contentId, string contentNameFieldPrefix = "")
        {
            var fieldNames = csvRowFields.Select(fn => FilterFromPrefix(fn.Name, contentNameFieldPrefix)).ToList();
            return FieldRepository.GetByNames(contentId, fieldNames);
        }

        private static string FilterFromPrefix(string entry, string contentNameFieldPrefix = "")
        {
            if (!string.IsNullOrWhiteSpace(contentNameFieldPrefix))
            {
                if (entry.StartsWith(contentNameFieldPrefix))
                {
                    return entry.Substring(contentNameFieldPrefix.Length);
                }
            }

            return entry;
        }

        private static IEnumerable<ArticleData> CreateArticleDatas(CsvDbUpdateModel csvFileData, IList<CsvDbUpdateFieldModel> csvRowFields, IEnumerable<Field> dbFields)
        {
            var result = new List<ArticleData>
            {
                new ArticleData
                {
                    Id = CreateSimpleArticleId(csvRowFields),
                    ContentId = csvFileData.ContentId,
                }
            };

            foreach (var dbf in dbFields.Where(dbf => dbf.IsClassifier))
            {
                var fieldValue = GetFieldValueByName(csvRowFields, dbf.Name);
                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    var extensionContentId = Convert.ToInt32(fieldValue);
                    result.Add(new ArticleData
                    {
                        Id = CreateExtensionArticleId(extensionContentId, csvRowFields),
                        ContentId = extensionContentId
                    });
                }
            }

            return result;
        }

        private static int CreateSimpleArticleId(IEnumerable<CsvDbUpdateFieldModel> csvRowFields)
        {
            return -Convert.ToInt32(GetFieldValueByName(csvRowFields, FieldName.CONTENT_ITEM_ID));
        }

        private static int CreateExtensionArticleId(int extensionContentId, IEnumerable<CsvDbUpdateFieldModel> csvRowFields)
        {
            return -Convert.ToInt32(GetFieldValueByName(csvRowFields, $"{GetContentNameById(extensionContentId)}.{FieldName.CONTENT_ITEM_ID}"));
        }

        private static string GetContentNameById(int contentId)
        {
            return ContentRepository.GetById(contentId).Name;
        }

        private static string GetFieldValueByName(IEnumerable<CsvDbUpdateFieldModel> csvRowFields, string csvFieldName)
        {
            return csvRowFields.Single(f => string.Equals(f.Name, csvFieldName, StringComparison.OrdinalIgnoreCase)).Value;
        }
    }
}
