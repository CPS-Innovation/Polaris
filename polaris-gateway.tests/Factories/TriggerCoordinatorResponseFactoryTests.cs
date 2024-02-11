using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PolarisGateway.Factories;
using Xunit;

namespace PolarisGateway.Tests.Factories
{
	public class TriggerCoordinatorResponseFactoryTests
	{
		private readonly string _requestScheme;
		private readonly string _requestHost;
		private readonly ushort _requestPort;
		private readonly Mock<HttpRequest> _mockRequest;
		private readonly Guid _correlationId;

		private readonly TrackerResponseFactory _triggerCoordinatorResponseFactory;

		public TriggerCoordinatorResponseFactoryTests()
		{
			var fixture = new Fixture();
			_correlationId = fixture.Create<Guid>();


			_requestScheme = "http";
			_requestHost = fixture.Create<string>();
			_requestPort = fixture.Create<ushort>();
			var correlationId = fixture.Create<Guid>();

			_mockRequest = new Mock<HttpRequest>();
			_mockRequest.Setup(x => x.Scheme).Returns(_requestScheme);
			_mockRequest.Setup(x => x.Host).Returns(new HostString(_requestHost, _requestPort));

			_triggerCoordinatorResponseFactory = new TrackerResponseFactory();
		}

		[Fact]
		public void Create_ReturnsTrackerUrl_WhenDefinedPortIsSet()
		{
			// Arrange
			var expectedAbsoluteUri = $"{_requestScheme}://{_requestHost}:{_requestPort}/tracker";

			// Act
			var response = _triggerCoordinatorResponseFactory.Create(_mockRequest.Object, _correlationId);

			// Assert
			response.TrackerUrl.AbsoluteUri.Should().Be(expectedAbsoluteUri);
		}

		[Fact]
		public void Create_ReturnsTrackerUrl_WhenDefinedPortIsNotSet()
		{
			// Arrange
			var expectedAbsoluteUri = $"{_requestScheme}://{_requestHost}/tracker";
			_mockRequest.Setup(x => x.Host).Returns(new HostString(_requestHost));

			// Act
			var response = _triggerCoordinatorResponseFactory.Create(_mockRequest.Object, _correlationId);

			// Assert
			response.TrackerUrl.AbsoluteUri.Should().Be(expectedAbsoluteUri);
		}

	}
}

