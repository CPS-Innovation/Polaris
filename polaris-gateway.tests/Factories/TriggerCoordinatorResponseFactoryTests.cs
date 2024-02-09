using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PolarisGateway.common.Mappers;
using PolarisGateway.Factories;
using Xunit;

namespace PolarisGateway.Tests.Factories
{
	public class TriggerCoordinatorResponseFactoryTests
	{
		private readonly HttpRequest _httpRequest;
		private readonly Uri _trackerUrl;
		private readonly Guid _correlationId;

		private readonly TriggerCoordinatorResponseFactory _triggerCoordinatorResponseFactory;

		public TriggerCoordinatorResponseFactoryTests()
		{
			var fixture = new Fixture();
			_correlationId = fixture.Create<Guid>();

			var context = new DefaultHttpContext();
			_httpRequest = context.Request;
			_trackerUrl = new Uri("http://www.test.co.uk");

			var mockTrackerUrlMapper = new Mock<ITrackerUrlMapper>();

			mockTrackerUrlMapper.Setup(mapper => mapper.Map(_httpRequest, It.IsAny<Guid>())).Returns(_trackerUrl);

			_triggerCoordinatorResponseFactory = new TriggerCoordinatorResponseFactory(mockTrackerUrlMapper.Object);
		}

		[Fact]
		public void Map_ReturnsTrackerUrl()
		{
			var response = _triggerCoordinatorResponseFactory.Create(_httpRequest, _correlationId);

			response.TrackerUrl.Should().Be(_trackerUrl);
		}
	}
}

