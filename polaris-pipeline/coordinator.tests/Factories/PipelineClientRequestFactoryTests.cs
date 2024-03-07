using System;
using System.Net.Http;
using AutoFixture;
using coordinator.Factories;
using FluentAssertions;
using Xunit;

namespace Common.Tests.Factories
{
    public class PipelineClientRequestFactoryTests
    {
        private readonly string _requestUri;
        private readonly string _accessToken;
        private readonly Guid _correlationId;

        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;

        public PipelineClientRequestFactoryTests()
        {
            var fixture = new Fixture();
            _requestUri = fixture.Create<string>();
            _accessToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();

            _pipelineClientRequestFactory = new PipelineClientRequestFactory();
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _pipelineClientRequestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _pipelineClientRequestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = _pipelineClientRequestFactory.Create(HttpMethod.Get, _requestUri, _correlationId, _accessToken);

            message.Headers.Authorization?.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}
