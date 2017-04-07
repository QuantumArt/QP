using Ploeh.AutoFixture;
using Quantumart.QP8.BLL;

namespace QP8.Services.Tests.Infrastructure.Helpers
{
    internal class FixtureArticleHelpers
    {
        public static void InjectSimpleArticle(IFixture fixture)
        {
            fixture.Inject(GenerateSimpleArticle(fixture));
        }

        public static Article GenerateSimpleArticle(IFixture fixture)
        {
            return fixture.Build<Article>()
                .FromSeed(article => new Article())
                .OmitAutoProperties()
                .With(f => f.Id)
                .With(f => f.Name)
                .Create();
        }
    }
}
