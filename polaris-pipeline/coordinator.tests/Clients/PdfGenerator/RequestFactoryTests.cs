using System;
using System.Net.Http;
using AutoFixture;
using coordinator.Clients.PdfGenerator;
using FluentAssertions;
using Xunit;

namespace coordinator.Tests.Clients.PdfGenerator
{
    public class RequestFactoryTests
    {
        private readonly string _requestUri;
        private readonly string _accessToken;
        private readonly Guid _correlationId;

        private readonly IRequestFactory _requestFactory;

        public RequestFactoryTests()
        {
            var fixture = new Fixture();
            _requestUri = fixture.Create<string>();
            _accessToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();

            _requestFactory = new RequestFactory();
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _requestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _requestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = _requestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.Headers.Authorization?.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}
