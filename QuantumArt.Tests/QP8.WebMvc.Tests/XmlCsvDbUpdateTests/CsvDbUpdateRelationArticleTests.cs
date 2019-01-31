using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using AutoFixture.Xunit2;
using Moq;
using QP8.Infrastructure.Logging.Factories;
using QP8.WebMvc.Tests.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;
using Xunit;

namespace QP8.WebMvc.Tests.XmlCsvDbUpdateTests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CsvDbUpdateRelationArticleTests
    {
        private readonly IFixture _fixture;
        private readonly IPostprocessComposer<Field> _postProcessFieldComposer;

        public CsvDbUpdateRelationArticleTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization(){ ConfigureMembers = true});
            _postProcessFieldComposer = _fixture.Build<Field>().FromFactory(CsvDbUpdateTestHelpers.GenerateField(_fixture)).OmitAutoProperties();
            _fixture.Customize<Field>(composer => _postProcessFieldComposer);

            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "RelatedArticle")]
        public void GivenCsvRowDataCollection_WhenWithDbRelations_ShouldCallServicesWithCorrectlyFormattedData(
            int contentId,
            int articleId,
            int relatedArticleId,
            Generator<FieldExactTypes> fetGenerator)
        {
            // Fixture setup
            var articleRepository = _fixture.Freeze<Mock<IArticleRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IBatchUpdateService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var csvRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);
            var m2mDbField = _postProcessFieldComposer.With(f => f.ExactType, fetGenerator.First(fet => new[]
            {
                FieldExactTypes.O2MRelation,
                FieldExactTypes.M2ORelation,
                FieldExactTypes.M2MRelation
            }.Contains(fet))).Create();

            var m2mCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, m2mDbField.Name)
                .With(f => f.Value, relatedArticleId.ToString())
                .Create();

            dbFields.Add(m2mDbField);
            csvRowFields.Add(m2mCsvField);
            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            articleRepository
                .Setup(m => m.IsExist(relatedArticleId))
                .Returns(true)
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
                        Value = csvf.Value,
                        ArticleIds = dbf.ExactType == m2mDbField.ExactType ? new[] { relatedArticleId } : Enumerable.Empty<int>().ToArray()
                    }).ToList()
                }
            };

            // Exercise system
            sut.Process(csvData);

            // Verify outcome
            fieldRepository.Verify();
            articleRepository.Verify();
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult)), false));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "RelatedArticle")]
        public void GivenCsvRowDataCollection_WhenWithEmptyDbRelations_ShouldRemoveNullReferences(
            int contentId,
            int articleId,
            int relatedArticleId,
            Generator<FieldExactTypes> fetGenerator)
        {
            // Fixture setup
            var articleRepository = _fixture.Freeze<Mock<IArticleRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IBatchUpdateService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var csvRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);
            var m2mDbField = _postProcessFieldComposer.With(f => f.ExactType, fetGenerator.First(fet => new[]
            {
                FieldExactTypes.O2MRelation,
                FieldExactTypes.M2ORelation,
                FieldExactTypes.M2MRelation
            }.Contains(fet))).Create();

            var m2mCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, m2mDbField.Name)
                .With(f => f.Value, relatedArticleId.ToString())
                .Create();

            dbFields.Add(m2mDbField);
            csvRowFields.Add(m2mCsvField);
            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            articleRepository
                .Setup(m => m.IsExist(relatedArticleId))
                .Returns(false)
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
                        Value = csvf.Value,
                        ArticleIds = Enumerable.Empty<int>().ToArray()
                    }).ToList()
                }
            };

            // Exercise system
            sut.Process(csvData);

            // Verify outcome
            //logger.Verify(m => m.Warn(It.IsAny<string>()));
            fieldRepository.Verify();
            articleRepository.Verify(m => m.IsExist(relatedArticleId), Times.Once);
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult)), false));
        }

        [Theory, AutoData, Trait("CsvDbUpdate", "RelatedArticle")]
        public void GivenCsvRowDataCollection_WhenWithEmptyDbRelationsButExistInCsv_ShouldCallServicesWithCorrectlyFormattedData(
            int contentId,
            int articleId,
            int relatedArticleId,
            Generator<FieldExactTypes> fetGenerator)
        {
            // Fixture setup
            var articleRepository = _fixture.Freeze<Mock<IArticleRepository>>();
            var fieldRepository = _fixture.Freeze<Mock<IFieldRepository>>();
            var articleService = _fixture.Freeze<Mock<IBatchUpdateService>>();
            var sut = _fixture.Create<CsvDbUpdateService>();

            var dbFields = _fixture.CreateMany<Field>().ToList();
            var csvRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);
            var m2mDbField = _postProcessFieldComposer.With(f => f.ExactType, fetGenerator.First(fet => new[]
            {
                FieldExactTypes.O2MRelation,
                FieldExactTypes.M2ORelation,
                FieldExactTypes.M2MRelation
            }.Contains(fet))).Create();

            var m2mCsvField = _fixture.Build<CsvDbUpdateFieldModel>()
                .With(f => f.Name, m2mDbField.Name)
                .With(f => f.Value, relatedArticleId.ToString())
                .Create();

            dbFields.Add(m2mDbField);
            csvRowFields.Add(m2mCsvField);

            var m2mRelatedDbFields = _fixture.CreateMany<Field>(1).ToList();
            var m2mRelatedCsvRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(relatedArticleId, m2mRelatedDbFields, _fixture);
            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, new List<IList<CsvDbUpdateFieldModel>>
            {
                csvRowFields,
                m2mRelatedCsvRowFields
            }, _fixture);

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
                .Returns(dbFields)
                .Verifiable();

            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(m2mRelatedCsvRowFields.Select(csvf => csvf.Name)))))
                .Returns(m2mRelatedDbFields)
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
                        Value = csvf.Value,
                        ArticleIds = dbf.ExactType == m2mDbField.ExactType ? new[] { -relatedArticleId } : Enumerable.Empty<int>().ToArray()
                    }).ToList()
                },
                new ArticleData
                {
                    Id = -relatedArticleId,
                    ContentId = contentId,
                    Fields = m2mRelatedDbFields.Zip(m2mRelatedCsvRowFields.Skip(1), (dbf, csvf) => new FieldData
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
            articleRepository.Verify(m => m.IsExist(It.IsAny<int>()), Times.Never);
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult)), false));
        }
    }
}
