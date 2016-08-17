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

namespace Quantumart.QP8.WebMvc.Infrastructure.Services
{
    public class CsvDbUpdateService
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
            Ensure.NotNullOrEmpty(connectionString, "Connection string should be initialized");

            _connectionString = connectionString;
            _articleService = new ArticleService(connectionString, userId);
        }

        public void Process(IEnumerable<CsvDbUpdateModel> data)
        {
            var articlesData = new List<ArticleData>();
            using (var ts = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            using (new QPConnectionScope(_connectionString))
            {
                articlesData.AddRange(from csvFileData in data
                                      from csvRowFields in csvFileData.Fields.Values
                                      select CreateArticleData(csvFileData, csvRowFields));

                ts.Complete();
            }

            _articleService.BatchUpdate(articlesData);
        }

        private static ArticleData CreateArticleData(CsvDbUpdateModel csvFileData, IList<CsvDbUpdateFieldModel> csvRowFields)
        {
            var dbFields = FieldRepository.GetByNames(csvFileData.ContentId, csvRowFields.Select(f => f.Name).ToList());
            return new ArticleData
            {
                Id = -Convert.ToInt32(csvRowFields.Single(f => f.Name == FieldName.CONTENT_ITEM_ID).Value),
                ContentId = csvFileData.ContentId,
                Fields = dbFields.Select(dbf => CreateFieldData(dbf, csvRowFields)).ToList()
            };
        }

        private static FieldData CreateFieldData(Field dbf, IEnumerable<CsvDbUpdateFieldModel> csvRowFields)
        {
            var result = new FieldData
            {
                Id = dbf.Id
            };

            var fieldValue = csvRowFields.Single(f => f.Name == dbf.Name).Value;
            if (dbf.RelationType != RelationType.None && !string.IsNullOrWhiteSpace(fieldValue))
            {
                result.ArticleIds = fieldValue.Split(CsvRelationSeparator).Select(f => -int.Parse(f)).ToArray();
            }
            else
            {
                result.Value = fieldValue;
            }

            return result;
        }
    }
}
