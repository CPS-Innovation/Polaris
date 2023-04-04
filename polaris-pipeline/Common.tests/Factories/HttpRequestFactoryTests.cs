using AutoFixture;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using Ddei.Factories.Contracts;
using Ddei.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Common.tests.Factories
{
    public class HttpRequestFactoryTests
    {
        private readonly string _requestUri;
        private readonly string _cmsAuthValues;
        private readonly Guid _correlationId;

        private readonly IDdeiClientRequestFactory _documentExtractionHttpRequestFactory;

        public HttpRequestFactoryTests()
        {
            var fixture = new Fixture();
            _requestUri = fixture.Create<string>();
            _cmsAuthValues = "sample-token";
            _correlationId = fixture.Create<Guid>();

            Mock<IOptions<DdeiOptions>> ddeiOptions = new Mock<IOptions<DdeiOptions>>();

            _documentExtractionHttpRequestFactory = new DdeiClientRequestFactory(ddeiOptions.Object);
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _cmsAuthValues, _correlationId);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _cmsAuthValues, _correlationId);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsExpectedCustomHeaders()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _cmsAuthValues, _correlationId);
            var cmsAuthValues = message.Headers.FirstOrDefault(x => x.Key == HttpHeaderKeys.CmsAuthValues);
            var correlationId = message.Headers.FirstOrDefault(x => x.Key == HttpHeaderKeys.CorrelationId);

            using (new AssertionScope())
            {
                cmsAuthValues.Value.FirstOrDefault().Should().NotBeNull();
                cmsAuthValues.Value.FirstOrDefault().Should().Be(_cmsAuthValues);
                correlationId.Value.FirstOrDefault().Should().NotBeNull();
                correlationId.Value.FirstOrDefault().Should().Be(_correlationId.ToString());
            }
        }
    }
}

