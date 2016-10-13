using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Db;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;
using WebMvc.Tests.Helpers;
using Xunit;

namespace WebMvc.Tests
{
    public class CsvDbUpdateTests
    {
        private readonly IFixture _fixture;

        public CsvDbUpdateTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization()).Customize(new MultipleCustomization());
            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
            _fixture.Customize<Field>(composer => composer.FromFactory(CsvDbUpdateHelpers.GenerateField(_fixture)).OmitAutoProperties().With(f => f.ExactType, FieldExactTypes.Undefined));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "SimpleArticle")]
        public void GivenCsvRowDataCollection_WhenWithoutExtensionsAndRelations_ShouldCallServicesWithCorrectlyFormattedData(int contentId, int articleId)
        {
            // Fixture setup
            var contentRepository = _fixture.Freeze<Mock<IContentRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IArticleService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var csvRowFields = CsvDbUpdateHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);
            var csvData = CsvDbUpdateHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);
            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            var expectedResult = new List<ArticleData>
            {
                new ArticleData
                {
                    Id = -articleId,
                    ContentId = contentId,
                    Fields = dbFields.Zip(csvRowFields.Skip(1), (dbf, csvf) => new FieldData
                    {
                        Id = dbf.Id,
                        Value = csvf.Value
                    }).ToList()
                }
            };

            // Exercise system
            sut.Process(csvData);

            // Verify outcome
            fieldRepository.Verify();
            contentRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Never);
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateHelpers.CompareArticleDataCollections(expectedResult))));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "ExtensionArticle")]
        public void GivenCsvRowDataCollection_WhenContainsExtensionAndWithoutRelations_ShouldCallServicesWithCorrectlyFormattedData(int contentId, int articleId, int externalContentId, int externalArticleId, string externalContentName)
        {
            // Fixture setup
            var contentRepository = _fixture.Freeze<Mock<IContentRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IArticleService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var simpleRowFields = CsvDbUpdateHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);

            var classifierDbField = _fixture.Build<Field>().FromFactory(CsvDbUpdateHelpers.GenerateField(_fixture)).OmitAutoProperties().With(f => f.IsClassifier, true).Create();
            var clasifierCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, classifierDbField.Name)
                .With(f => f.Value, externalContentId.ToString())
                .Create();

            dbFields.Add(classifierDbField);
            simpleRowFields.Add(clasifierCsvField);

            var externalDbFields = _fixture.CreateMany<Field>().ToList();
            var externalRowFields = CsvDbUpdateHelpers.GenerateCsvRowFields(externalArticleId, externalDbFields, _fixture, externalContentName);

            var csvRowFields = simpleRowFields.Concat(externalRowFields).ToList();
            var csvData = CsvDbUpdateHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            fieldRepository
                .Setup(m => m.GetByNames(externalContentId, It.Is(CsvDbUpdateHelpers.CompareStringCollections(externalRowFields.Select(csvf => CsvDbUpdateHelpers.FilterFromPrefix(csvf.Name))))))
                .Returns(externalDbFields)
                .Verifiable();

            contentRepository
                .Setup(m => m.GetById(externalContentId))
                .Returns(new Content { Name = externalContentName })
                .Verifiable();

            var expectedResult = new List<ArticleData>
            {
                new ArticleData
                {
                    Id = -articleId,
                    ContentId = contentId,
                    Fields = dbFields.Zip(simpleRowFields.Skip(1), (dbf, csvf) => new FieldData
                    {
                        Id = dbf.Id,
                        Value = csvf.Value,
                        ArticleIds = dbf.IsClassifier ? new[] { -externalArticleId } : Enumerable.Empty<int>().ToArray()
                    }).ToList(),
                },
                new ArticleData
                {
                    Id = -externalArticleId,
                    ContentId = externalContentId,
                    Fields = externalDbFields.Zip(externalRowFields.Skip(1), (dbf, csvf) => new FieldData
                    {
                        Id = dbf.Id,
                        Value = csvf.Value,
                        ArticleIds = Enumerable.Empty<int>().ToArray()
                    }).ToList()
                }
            };

            // Exercise system
            sut.Process(csvData);

            // Verify outcome
            fieldRepository.Verify();
            contentRepository.Verify();
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateHelpers.CompareArticleDataCollections(expectedResult))));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "ComplexTest")]
        public void GivenCsvRowDataCollection_WhenContainsEmptyExtensions_ShouldBuildResultDataLikeSimpleArticle(int contentId, int articleId)
        {
            // Fixture setup
            var contentRepository = _fixture.Freeze<Mock<IContentRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IArticleService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var simpleRowFields = CsvDbUpdateHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);

            var classifierDbField = _fixture.Build<Field>().FromFactory(CsvDbUpdateHelpers.GenerateField(_fixture)).OmitAutoProperties().With(f => f.IsClassifier, true).Create();
            var emptyClasifierCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, classifierDbField.Name)
                .With(f => f.Value, string.Empty)
                .Create();

            dbFields.Add(classifierDbField);
            simpleRowFields.Add(emptyClasifierCsvField);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateHelpers.CompareStringCollections(simpleRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            var csvData = CsvDbUpdateHelpers.GenerateCsvDbUpdateModel(contentId, simpleRowFields, _fixture);
            var expectedResult = new List<ArticleData>
            {
                new ArticleData
                {
                    Id = -articleId,
                    ContentId = contentId,
                    Fields = dbFields.Zip(simpleRowFields.Skip(1), (dbf, csvf) => new FieldData
                    {
                        Id = dbf.Id,
                        Value = csvf.Value,
                        ArticleIds = Enumerable.Empty<int>().ToArray()
                    }).ToList(),
                }
            };

            // Exercise system
            sut.Process(csvData);

            // Verify outcome
            fieldRepository.Verify();
            contentRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Never);
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateHelpers.CompareArticleDataCollections(expectedResult))));
        }
    }
}
