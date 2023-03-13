using System;
using System.Net.Http;
using AutoFixture;
using Common.Factories;
using Common.Factories.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PolarisGateway.Tests.Factories
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

            var loggerMock = new Mock<ILogger<PipelineClientRequestFactory>>();

            _pipelineClientRequestFactory = new PipelineClientRequestFactory(loggerMock.Object);
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _pipelineClientRequestFactory.CreateAuthenticatedGet(_requestUri, _accessToken, _correlationId);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _pipelineClientRequestFactory.CreateAuthenticatedGet(_requestUri, _accessToken, _correlationId);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = _pipelineClientRequestFactory.CreateAuthenticatedGet(_requestUri, _accessToken, _correlationId);

            message.Headers.Authorization?.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}
