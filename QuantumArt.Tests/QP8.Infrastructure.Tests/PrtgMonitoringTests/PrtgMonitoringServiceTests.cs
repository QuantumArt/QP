using Flurl.Http.Testing;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring;
using QP8.Infrastructure.Logging.PrtgMonitoring.Data;
using QP8.Infrastructure.Tests.Infrastructure.Specimens;
using System.Net.Http;
using Xunit;

namespace QP8.Infrastructure.Tests.PrtgMonitoringTests
{
    public class PrtgMonitoringServiceTests
    {
        private readonly IFixture _fixture;
        private readonly HttpTest _httpTest;

        private const string SuccessRequestData = @"
<prtg>
    <text>test success message</text>
    <result><channel>MyChannel</channel><value>17</value></result>
    <result><channel>AnotherChannel</channel><value>31</value></result>
</prtg>";

        private const string ErrorRequestData = @"<prtg><text>test error message</text><error>22</error></prtg>";

        public PrtgMonitoringServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization()).Customize(new MultipleCustomization());
            _fixture.Customizations.Add(new PrtgMonitoringServiceSpecimenBuilder());

            _httpTest = new HttpTest();
            _httpTest.RespondWithJson(new PrtgHttpResponse());

            LogProvider.LogFactory = new NullLogFactory();
        }

        [Fact]
        public async void PrtgMonitoring_SendStringSuccessGetRequest_SendExpectedData()
        {
            // Fixture setup
            const string expectedUri = "http://mscmonitor01:5050/qp.articlescheduler.mscdev02?Content=%0D%0A%3Cprtg%3E%0D%0A%20%20%20%20%3Ctext%3Etest%20success%20message%3C%2Ftext%3E%0D%0A%20%20%20%20%3Cresult%3E%3Cchannel%3EMyChannel%3C%2Fchannel%3E%3Cvalue%3E17%3C%2Fvalue%3E%3C%2Fresult%3E%0D%0A%20%20%20%20%3Cresult%3E%3Cchannel%3EAnotherChannel%3C%2Fchannel%3E%3Cvalue%3E31%3C%2Fvalue%3E%3C%2Fresult%3E%0D%0A%3C%2Fprtg%3E";
            var sut = _fixture.Create<PrtgMonitoringService>();

            // Exercise system
            await sut.SendStringGetRequest(SuccessRequestData);

            // Verify outcome
            _httpTest
                .ShouldHaveMadeACall()
                .WithVerb(HttpMethod.Get)
                .WithQueryParamValue("Content", SuccessRequestData)
                .With(ctx => string.Equals(ctx.Request.RequestUri.AbsoluteUri, expectedUri));
        }

        [Fact]
        public async void PrtgMonitoring_SendStringErrorGetRequest_SendExpectedData()
        {
            // Fixture setup
            const string expectedUri = "http://mscmonitor01:5050/qp.articlescheduler.mscdev02?Content=%3Cprtg%3E%3Ctext%3Etest%20error%20message%3C%2Ftext%3E%3Cerror%3E22%3C%2Ferror%3E%3C%2Fprtg%3E";
            var sut = _fixture.Create<PrtgMonitoringService>();

            // Exercise system
            await sut.SendStringGetRequest(ErrorRequestData);

            // Verify outcome
            _httpTest
                .ShouldHaveMadeACall()
                .WithVerb(HttpMethod.Get)
                .WithQueryParamValue("Content", ErrorRequestData)
                .With(ctx => string.Equals(ctx.Request.RequestUri.AbsoluteUri, expectedUri));
        }

        [Fact]
        public async void PrtgMonitoring_SendStringPostRequest_SendExpectedData()
        {
            // Fixture setup
            var sut = _fixture.Create<PrtgMonitoringService>();

            // Exercise system
            await sut.SendStringPostRequest(ErrorRequestData);

            // Verify outcome
            _httpTest
                .ShouldHaveMadeACall()
                .WithVerb(HttpMethod.Post)
                .WithoutQueryParams()
                .WithRequestBody(ErrorRequestData);
        }

        [Fact]
        public async void PrtgMonitoring_SendSuccessJsonRequest_SendExpectedData()
        {
            // Fixture setup
            var request = _fixture.Create<PrtgSuccessHttpRequest>();
            var sut = _fixture.Create<PrtgMonitoringService>();

            // Exercise system
            await sut.SendJsonRequest(request);

            // Verify outcome
            _httpTest
                .ShouldHaveMadeACall()
                .WithVerb(HttpMethod.Post)
                .WithoutQueryParams()
                .WithRequestBody(JsonConvert.SerializeObject(request));
        }

        [Fact]
        public async void PrtgMonitoring_SendErrorJsonRequest_SendExpectedData()
        {
            // Fixture setup
            var request = _fixture.Create<PrtgErrorHttpRequest>();
            var sut = _fixture.Create<PrtgMonitoringService>();

            // Exercise system
            await sut.SendJsonRequest(request);

            // Verify outcome
            _httpTest
                .ShouldHaveMadeACall()
                .WithVerb(HttpMethod.Post)
                .WithoutQueryParams()
                .WithRequestBody(JsonConvert.SerializeObject(request));
        }
    }
}
