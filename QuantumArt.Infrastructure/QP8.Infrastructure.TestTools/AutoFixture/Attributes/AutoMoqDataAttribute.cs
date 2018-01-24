using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace QP8.Infrastructure.TestTools.AutoFixture.Attributes
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        protected AutoMoqDataAttribute()
            : base(() => new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }
}
