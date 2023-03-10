using AutoFixture;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Common.Tests.Factories;

public class ComputerVisionClientFactoryTests
{
	private readonly string _serviceUrl;

	private readonly IComputerVisionClientFactory _computerVisionClientFactory;

	public ComputerVisionClientFactoryTests()
	{
		var fixture = new Fixture();
		_serviceUrl = fixture.Create<string>();
		var configuration = new Mock<IConfiguration>();

		configuration.Setup(x => x[ConfigKeys.TextExtractorKeys.ComputerVisionClientServiceUrl]).Returns(_serviceUrl);
			
		_computerVisionClientFactory = new ComputerVisionClientFactory(configuration.Object);
	}

    [Fact]
	public void Create_ReturnsComputerVisionClient()
    {
		var client = _computerVisionClientFactory.Create();

		client.Should().BeOfType<ComputerVisionClient>();
    }

	[Fact]
	public void Create_SetsExpectedEndpointUrl()
	{
		var client = _computerVisionClientFactory.Create();

		client.Endpoint.Should().Be(_serviceUrl);
	}
}
