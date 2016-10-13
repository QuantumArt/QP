using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ploeh.AutoFixture;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;

namespace WebMvc.Tests.Helpers
{
    public class CsvDbUpdateHelpers
    {
        public static Func<Field> GenerateField(IFixture fixture)
        {
            return () => fixture.Build<Field>()
                .OmitAutoProperties()
                .Do(c => c.Init())
                .With(f => f.Id)
                .With(f => f.Name)
                .With(f => f.IsClassifier, false)
                .Create();
        }

        public static IEnumerable<CsvDbUpdateModel> GenerateCsvDbUpdateModel(int contentId, IList<CsvDbUpdateFieldModel> csvRowFields, IFixture fixture)
        {
            return fixture
                .Build<CsvDbUpdateModel>()
                .With(m => m.ContentId, contentId)
                .With(m => m.Fields, new Dictionary<int, IList<CsvDbUpdateFieldModel>> { { 0, csvRowFields } })
                .CreateMany(1);
        }

        public static IList<CsvDbUpdateFieldModel> GenerateCsvRowFields(int articleId, IEnumerable<Field> dbFields, IFixture fixture, string externalPrefix = "")
        {
            externalPrefix = string.IsNullOrWhiteSpace(externalPrefix) ? externalPrefix : $"{externalPrefix}.";
            var csvRowFields = new List<CsvDbUpdateFieldModel>
            {
                new CsvDbUpdateFieldModel
                {
                    Name = $"{externalPrefix}{FieldName.CONTENT_ITEM_ID}",
                    Value = $"{articleId}"
                }
            };

            csvRowFields.AddRange(dbFields.Select(dbf => fixture.Build<CsvDbUpdateFieldModel>().With(f => f.Name, $"{externalPrefix}{dbf.Name}").Create()));
            return csvRowFields;
        }

        public static string FilterFromPrefix(string entry)
        {
            return new string(entry.SkipWhile(ch => ch == '.').ToArray());
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
