// using System;
// using System.Net.Http;
// using System.Threading.Tasks;
// using AutoFixture;
// using Common.Dto.Tracker;
// using PolarisGateway.Domain.Validators;
// using FluentAssertions;
// using PolarisGateway.Clients;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Primitives;
// using Moq;
// using PolarisGateway.Domain.Validation;
// using PolarisGateway.Functions;
// using Common.Telemetry.Wrappers.Contracts;
// using Xunit;

// namespace PolarisGateway.Tests.Functions
// {
//     public class PolarisPipelineGetTrackerTests : SharedMethods.SharedMethods
//     {
//         private readonly string _caseUrn;
//         private readonly int _caseId;
//         private readonly HttpResponseMessage _response;

//         private readonly Mock<ICoordinatorClient> _mockCoordinatorClient;
//         private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

//         private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;

//         private readonly PolarisPipelineGetCaseTracker _polarisPipelineGetTracker;

//         public PolarisPipelineGetTrackerTests()
//         {
//             var fixture = new Fixture();
//             _caseUrn = fixture.Create<string>();
//             _caseId = fixture.Create<int>();
//             _response = fixture.Create<HttpResponseMessage>();
//             fixture.Create<Guid>();

//             var mockLogger = new Mock<ILogger<PolarisPipelineGetCaseTracker>>();
//             _mockCoordinatorClient = new Mock<ICoordinatorClient>();
//             _mockTokenValidator = new Mock<IAuthorizationValidator>();

//             _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });
//             _mockCoordinatorClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
//                 .ReturnsAsync(_response);

//             _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
//             _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterUserName(It.IsAny<string>()));
//             _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterCorrelationId(It.IsAny<Guid>()));
//             _polarisPipelineGetTracker = new PolarisPipelineGetCaseTracker(mockLogger.Object, _mockCoordinatorClient.Object, _mockTokenValidator.Object, _mockTelemetryAugmentationWrapper.Object);
//         }

//         [Fact]
//         public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
//         {
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

//             response.Should().BeOfType<ObjectResult>()
//                 .And.Subject.As<ObjectResult>().StatusCode.Should().Be(400);
//         }

//         [Fact]
//         public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
//         {
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

//             response.Should().BeOfType<ObjectResult>()
//                 .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
//         }

//         [Fact]
//         public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
//         {
//             _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

//             response.Should().BeOfType<ObjectResult>()
//                 .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
//         }

//         [Fact]
//         public async Task Run_ReturnsBadRequestWhenCmsAuthValuesIsMissing()
//         {
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutCmsAuthValuesToken(), _caseUrn, _caseId);

//             response.Should().BeOfType<ObjectResult>();
//             ((response as ObjectResult)?.StatusCode).Should().Be(403);
//         }

//         [Fact]
//         public async Task Run_ReturnsOk()
//         {
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

//             response.Should().BeOfType<OkObjectResult>();
//         }

//         [Fact]
//         public async Task Run_ReturnsTracker()
//         {
//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as OkObjectResult;

//             response?.Value.Should().Be(_response);
//         }

//         [Fact]
//         public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
//         {
//             _mockCoordinatorClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
//                 .ThrowsAsync(new HttpRequestException());

//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

//             response?.StatusCode.Should().Be(500);
//         }

//         [Fact]
//         public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
//         {
//             _mockCoordinatorClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
//                 .ThrowsAsync(new Exception());

//             var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

//             response?.StatusCode.Should().Be(500);
//         }
//     }
// }

