using QP8.Infrastructure.Tests.TestData;
using QP8.Infrastructure.Web.Helpers;
using Xunit;

namespace QP8.Infrastructure.Tests.HelpersTests
{
    public class UrlHelpersTests
    {
        [MemberData(nameof(UrlHelpersTestData.GetInvalidUrls), MemberType = typeof(UrlHelpersTestData))]
        [Theory, Trait("Helpers", "UrlHelpers")]
        public void GivenUrlsList_WhenAbsoluteAndInvalid_ShouldNotPassValidation(string url)
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            Assert.False(UrlHelpers.IsValidUrl(url));
        }

        [MemberData(nameof(UrlHelpersTestData.GetValidAbsoluteUrls), MemberType = typeof(UrlHelpersTestData))]
        [Theory, Trait("Helpers", "UrlHelpers")]
        public void GivenUrlsList_WhenAbsoluteAndValid_ShouldPassValidation(string url)
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            Assert.True(UrlHelpers.IsAbsoluteUrl(url));
        }

        [MemberData(nameof(UrlHelpersTestData.GetValidAbsoluteWebFolderUrls), MemberType = typeof(UrlHelpersTestData))]
        [Theory, Trait("Helpers", "UrlHelpers")]
        public void GivenUrlsList_WhenAbsoluteWithoutQueryAndValid_ShouldPassValidation(string url)
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            Assert.True(UrlHelpers.IsAbsoluteWebFolderUrl(url));
        }

        [MemberData(nameof(UrlHelpersTestData.GetValidRelativeUrls), MemberType = typeof(UrlHelpersTestData))]
        [Theory, Trait("Helpers", "UrlHelpers")]
        public void GivenUrlsList_WhenRelativeAndValid_ShouldPassValidation(string url)
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            Assert.True(UrlHelpers.IsRelativeUrl(url));
        }

        [MemberData(nameof(UrlHelpersTestData.GetValidRelativeWebFolderUrls), MemberType = typeof(UrlHelpersTestData))]
        [Theory, Trait("Helpers", "UrlHelpers")]
        public void GivenUrlsList_WhenRelativeWithoutQueryAndValid_ShouldPassValidation(string url)
        {
            // Fixture setup
            // Exercise system
            // Verify outcome
            Assert.True(UrlHelpers.IsRelativeWebFolderUrl(url));
        }
    }
}
