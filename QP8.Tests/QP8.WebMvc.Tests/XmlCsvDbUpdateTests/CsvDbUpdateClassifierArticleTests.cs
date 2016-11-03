using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Dsl;
using Ploeh.AutoFixture.Xunit2;
using QP8.WebMvc.Tests.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Db;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;
using Xunit;

namespace QP8.WebMvc.Tests.XmlCsvDbUpdateTests
{
    public class CsvDbUpdateClassifierArticleTests
    {
        private readonly IFixture _fixture;
        private readonly IPostprocessComposer<Field> _postProcessFieldComposer;

        public CsvDbUpdateClassifierArticleTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization()).Customize(new MultipleCustomization());
            _postProcessFieldComposer = _fixture.Build<Field>().FromFactory(CsvDbUpdateTestHelpers.GenerateField(_fixture)).OmitAutoProperties();
            _fixture.Customize<Field>(composer => _postProcessFieldComposer);

            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "ExtensionArticle")]
        public void GivenCsvRowDataCollection_WhenContainsExtension_ShouldCallServicesWithCorrectlyFormattedData(int contentId, int articleId, int externalContentId, int externalArticleId, string externalContentName)
        {
            // Fixture setup
            var contentRepository = _fixture.Freeze<Mock<IContentRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IArticleService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var simpleRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);

            var classifierDbField = _postProcessFieldComposer.With(f => f.IsClassifier, true).Create();
            var clasifierCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, classifierDbField.Name)
                .With(f => f.Value, externalContentId.ToString())
                .Create();

            dbFields.Add(classifierDbField);
            simpleRowFields.Add(clasifierCsvField);

            var externalDbFields = _fixture.CreateMany<Field>().ToList();
            var externalRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(externalArticleId, externalDbFields, _fixture, externalContentName);

            var csvRowFields = simpleRowFields.Concat(externalRowFields).ToList();
            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            fieldRepository
                .Setup(m => m.GetByNames(externalContentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(externalRowFields.Select(csvf => CsvDbUpdateTestHelpers.FilterFromPrefix(csvf.Name))))))
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
            contentRepository.Verify(m => m.GetById(It.IsAny<int>()), Times.Exactly(2));
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult))));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "ExtensionArticle")]
        public void GivenCsvRowDataCollection_WhenContainsEmptyExtensionContentId_ShouldBuildResultDataLikeSimpleArticle(int contentId, int articleId)
        {
            // Fixture setup
            var contentRepository = _fixture.Freeze<Mock<IContentRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IArticleService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var simpleRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);

            var classifierDbField = _postProcessFieldComposer.With(f => f.IsClassifier, true).Create();
            var emptyClasifierCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, classifierDbField.Name)
                .With(f => f.Value, string.Empty)
                .Create();

            dbFields.Add(classifierDbField);
            simpleRowFields.Add(emptyClasifierCsvField);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(simpleRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, simpleRowFields, _fixture);
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
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult))));
        }
    }
}
