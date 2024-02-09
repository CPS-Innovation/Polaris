using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Mappers;
using Xunit;

namespace PolarisGateway.Tests.Mappers
{
	public class TrackerUrlMapperTests
	{
		private readonly Fixture _fixture;
		private readonly string _requestScheme;
		private readonly string _requestHost;
		private readonly ushort _requestPort;
		private readonly Guid _correlationId;

		private readonly Mock<HttpRequest> _mockRequest;

		private readonly TrackerUrlMapper _trackerUrlMapper;

		public TrackerUrlMapperTests()
		{
			_fixture = new Fixture();
			_requestScheme = "http";
			_requestHost = _fixture.Create<string>();
			_requestPort = _fixture.Create<ushort>();
			_correlationId = _fixture.Create<Guid>();

			_mockRequest = new Mock<HttpRequest>();

			_mockRequest.Setup(x => x.Scheme).Returns(_requestScheme);
			_mockRequest.Setup(x => x.Host).Returns(new HostString(_requestHost, _requestPort));

			_trackerUrlMapper = new TrackerUrlMapper();
		}

		[Fact]
		public void Map_ReturnsExpectedUriWhenDefinedPortIsSet()
		{
			var expectedAbsoluteUri = $"{_requestScheme}://{_requestHost}:{_requestPort}/tracker";

			var uri = _trackerUrlMapper.Map(_mockRequest.Object, _correlationId);

			uri.AbsoluteUri.Should().Be(expectedAbsoluteUri);
		}

		[Fact]
		public void Map_ReturnsExpectedUriWhenDefaultPortIsSet()
		{
			var expectedAbsoluteUri = $"{_requestScheme}://{_requestHost}/tracker";
			_mockRequest.Setup(x => x.Host).Returns(new HostString(_requestHost));

			var uri = _trackerUrlMapper.Map(_mockRequest.Object, _correlationId);

			uri.AbsoluteUri.Should().Be(expectedAbsoluteUri);
		}
	}
}

