using AutoFixture;
using Common.Factories;
using Common.Factories.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.tests.Factories
{
	public class HttpRequestFactoryTests
	{
        private readonly string _requestUri;
        private readonly string _accessToken;
        private readonly Guid _correlationId;

        private readonly IHttpRequestFactory _documentExtractionHttpRequestFactory;

        public HttpRequestFactoryTests()
        {
            var fixture = new Fixture();
            _requestUri = fixture.Create<string>();
            //_accessToken = fixture.Create<string>(); //until Polaris DDEI supports oAuth, this is hardcoded to a not-implemented-yet string
            _accessToken = "not-implemented-yet";
            _correlationId = fixture.Create<Guid>();

            var loggerMock = new Mock<ILogger<HttpRequestFactory>>();
            
            _documentExtractionHttpRequestFactory = new HttpRequestFactory(loggerMock.Object);
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _accessToken, _correlationId);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _accessToken, _correlationId);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = _documentExtractionHttpRequestFactory.CreateGet(_requestUri, _accessToken, _correlationId);

            message.Headers.Authorization?.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}

