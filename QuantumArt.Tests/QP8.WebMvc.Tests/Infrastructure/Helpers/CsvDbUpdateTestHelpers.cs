using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ploeh.AutoFixture;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Constants;

namespace QP8.WebMvc.Tests.Infrastructure.Helpers
{
    public class CsvDbUpdateTestHelpers
    {
        public static Func<Field> GenerateField(IFixture fixture)
        {
            return () => fixture.Build<Field>()
                .OmitAutoProperties()
                .Do(c => c.Init())
                .With(f => f.Id)
                .With(f => f.Name)
                .With(f => f.IsClassifier, false)
                .With(f => f.ExactType, FieldExactTypes.Undefined)
                .Create();
        }

        public static IEnumerable<CsvDbUpdateModel> GenerateCsvDbUpdateModel(int contentId, IList<CsvDbUpdateFieldModel> csvRowFields, IFixture fixture) => GenerateCsvDbUpdateModel(contentId, new List<IList<CsvDbUpdateFieldModel>> { csvRowFields }, fixture);

        public static IEnumerable<CsvDbUpdateModel> GenerateCsvDbUpdateModel(int contentId, IList<IList<CsvDbUpdateFieldModel>> csvFilesRowFieldData, IFixture fixture)
        {
            var csvFilesData = new Dictionary<int, IList<CsvDbUpdateFieldModel>>();
            for (var i = 0; i < csvFilesRowFieldData.Count; i++)
            {
                csvFilesData.Add(i, csvFilesRowFieldData[i]);
            }

            return fixture
                .Build<CsvDbUpdateModel>()
                .With(m => m.ContentId, contentId)
                .With(m => m.Fields, csvFilesData)
                .CreateMany(1);
        }

        public static List<CsvDbUpdateFieldModel> GenerateCsvRowFields(int articleId, IEnumerable<Field> dbFields, IFixture fixture, string externalPrefix = "")
        {
            externalPrefix = string.IsNullOrWhiteSpace(externalPrefix) ? externalPrefix : $"{externalPrefix}.";
            var csvRowFields = new List<CsvDbUpdateFieldModel>
            {
                new CsvDbUpdateFieldModel
                {
                    Name = $"{externalPrefix}{FieldName.ContentItemId}",
                    Value = $"{articleId}"
                }
            };

            csvRowFields.AddRange(dbFields.Select(dbf => fixture.Build<CsvDbUpdateFieldModel>().With(f => f.Name, $"{externalPrefix}{dbf.Name}").Create()));
            return csvRowFields;
        }

        public static string FilterFromPrefix(string entry, string externalPrefix)
        {
            externalPrefix = string.IsNullOrWhiteSpace(externalPrefix) ? externalPrefix : $"{externalPrefix}.";
            return new string(entry.Substring(externalPrefix.Length).ToArray());
        }

        public static Expression<Func<IList<string>, bool>> CompareStringCollections(IEnumerable<string> expectedFields)
        {
            return actualResult => actualResult.SequenceEqual(expectedFields);
        }

        public static Expression<Func<IList<ArticleData>, bool>> CompareArticleDataCollections(IList<ArticleData> expectedResult)
        {
            return actualResult => string.Equals(expectedResult.ToJsonLog(), actualResult.ToJsonLog());
        }
    }
}
