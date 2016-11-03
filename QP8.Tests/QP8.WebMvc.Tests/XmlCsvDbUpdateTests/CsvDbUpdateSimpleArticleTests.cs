using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;
using QP8.WebMvc.Tests.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Interfaces.Db;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate;
using Xunit;

namespace QP8.WebMvc.Tests.XmlCsvDbUpdateTests
{
    public class CsvDbUpdateSimpleArticleTests
    {
        private readonly IFixture _fixture;

        public CsvDbUpdateSimpleArticleTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization()).Customize(new MultipleCustomization());
            _fixture.Customize<Field>(composer => composer.FromFactory(CsvDbUpdateTestHelpers.GenerateField(_fixture)).OmitAutoProperties());

            QPContext.CurrentDbConnectionString = _fixture.Create<string>();
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
            var csvRowFields = CsvDbUpdateTestHelpers.GenerateCsvRowFields(articleId, dbFields, _fixture);
            var csvData = CsvDbUpdateTestHelpers.GenerateCsvDbUpdateModel(contentId, csvRowFields, _fixture);
            fieldRepository
                .Setup(m => m.GetByNames(contentId, It.Is(CsvDbUpdateTestHelpers.CompareStringCollections(csvRowFields.Select(csvf => csvf.Name)))))
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
            articleService.Verify(m => m.BatchUpdate(It.Is(CsvDbUpdateTestHelpers.CompareArticleDataCollections(expectedResult))));
        }
    }
}
