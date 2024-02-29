using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using Ddei.Factories;
using DdeiClient.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.CmsAuthentication;
using Xunit;

namespace PolarisGateway.Tests.Functions.CmsAuthentication
{
  public class InitiateCookiesTests
  {
    private readonly InitiateCookies _initiateCookies;
    public InitiateCookiesTests()
    {
      var mockDdeiClient = new Mock<IDdeiClient>();
      var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
      var mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
      var mockLogger = new Mock<ILogger<InitiateCookies>>();
      var mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
      _initiateCookies = new InitiateCookies(
          mockDdeiClient.Object,
          mockDdeiArgFactory.Object,
          mockJsonConvertWrapper.Object,
          mockTelemetryAugmentationWrapper.Object,
          mockLogger.Object
      );
    }

    [Fact]
    public void InitiateCookies_CanInitialise()
    {
      _initiateCookies.Should().NotBeNull();
    }
  }
}