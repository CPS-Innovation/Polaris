using System;
using System.Threading.Tasks;
using Common.Validators.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NSubstitute;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Wrappers;
using Xunit;

namespace PolarisGateway.Tests.Functions.Status
{
    public class CoreDataApiCaseInformationByUrnFunctionTests : SharedMethods.SharedMethods
    {
        private readonly ILogger<PolarisGateway.Functions.Health.Status> _mockLogger = Substitute.For<ILogger<PolarisGateway.Functions.Health.Status>>();
        private readonly IAuthorizationValidator _mockTokenValidator = Substitute.For<IAuthorizationValidator>();

        private readonly ITelemetryAugmentationWrapper _mockTelemetryAugmentationWrapper = Substitute.For<ITelemetryAugmentationWrapper>();


        public CoreDataApiCaseInformationByUrnFunctionTests()
        {


            _mockTokenValidator.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>()).ReturnsForAnyArgs(new ValidateTokenResult { IsValid = true, UserName = "user-name" });
        }

        [Fact]
        public async Task StatusFunction_Should_Return_Response_400_When_No_CorrelationId_Supplied()
        {
            //Arrange
            var statusFunction = GetStatusFunction();

            //Act
            var results = await statusFunction.Run(CreateHttpRequestWithoutCorrelationId()) as Microsoft.AspNetCore.Mvc.ObjectResult;

            //Assert
            Assert.Equal(400, results?.StatusCode);
        }

        [Fact]
        public async Task StatusFunction_Should_Return_Response_400_When_No_Authorization_Supplied()
        {
            //Arrange
            var statusFunction = GetStatusFunction();

            //Act
            var results = await statusFunction.Run(CreateHttpRequestWithoutToken()) as Microsoft.AspNetCore.Mvc.ObjectResult;

            //Assert
            Assert.Equal(400, results?.StatusCode);
        }

        [Fact]
        public async Task StatusFunction_Should_Return_Response_401_When_Invalid_Authorization_Supplied()
        {
            //Arrange
            var statusFunction = GetStatusFunction();
            _mockTokenValidator.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>()).ReturnsForAnyArgs(new ValidateTokenResult { IsValid = false });

            //Act
            var results = await statusFunction.Run(CreateHttpRequest()) as Microsoft.AspNetCore.Mvc.ObjectResult;

            //Assert
            Assert.Equal(401, results?.StatusCode);
        }

        [Fact]
        public async Task StatusFunction_Should_Return_Response_200()
        {
            //Arrange
            var statusFunction = GetStatusFunction();

            //Act
            var results = await statusFunction.Run(CreateHttpRequest()) as Microsoft.AspNetCore.Mvc.JsonResult;

            //Assert
            Assert.Equal(200, results?.StatusCode);
        }

        private PolarisGateway.Functions.Health.Status GetStatusFunction()
        {
            return new PolarisGateway.Functions.Health.Status(_mockLogger, _mockTokenValidator, _mockTelemetryAugmentationWrapper);
        }

    }
}
