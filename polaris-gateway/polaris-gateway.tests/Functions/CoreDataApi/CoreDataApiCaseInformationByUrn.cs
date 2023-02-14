// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Moq;
// using NSubstitute;
// using PolarisGateway.Clients.CoreDataApi;
// using PolarisGateway.Domain.CaseData;
// using PolarisGateway.Tests.FakeData;
// using Xunit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Identity.Client;
// using NSubstitute.ExceptionExtensions;
// using PolarisGateway.Domain.CoreDataApi;
// using System;
// using PolarisGateway.Domain.Validators;
// using PolarisGateway.Functions.CoreDataApi.Case;

// namespace PolarisGateway.Tests.Functions.CoreDataApi
// {
//     public class CoreDataApiCaseInformationByUrnTests : SharedMethods.SharedMethods, IClassFixture<CaseInformationFake>
//     {
//         private readonly ILogger<CoreDataApiCaseInformationByUrn> _mockLogger = Substitute.For<ILogger<CoreDataApiCaseInformationByUrn>>();
//         private readonly ICoreDataApiClient _mockCoreDataApiClient = Substitute.For<ICoreDataApiClient>();
//         private readonly IConfiguration _mockConfiguration = Substitute.For<IConfiguration>();
//         private readonly CaseInformationFake _caseInformationFake;

//         public CoreDataApiCaseInformationByUrnTests(CaseInformationFake caseInformationFake)
//         {
//             _caseInformationFake = caseInformationFake;
//         }

//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_400_When_No_CorrelationId_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequestWithoutCorrelationId(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(400, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_400_When_No_Token_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequestWithoutToken(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(400, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_401_When_Invalid_Authorization_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();
//             _mockTokenValidator.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()).ReturnsForAnyArgs(false);

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequest(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(401, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_400_When_URN_Not_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequest(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(400, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_No_Data_Found()
//         {
//             //Arrange
//             var urn = "10OF1234521";
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequest(), urn) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(404, results?.StatusCode);
//             Assert.Contains(urn, results?.Value?.ToString());

//         }


//         [Fact]
//         public async Task CoreDataApiCaseInformationByUrnFunction_Should_Return_Response_200_When_Valid_Input_Supplied()
//         {
//             //Arrange
//             var urn = "10OF1234520";
//             var coreDataApiCaseInformationByUrnFunction = GetCoreDataApiCaseInformationByUrnFunction();
//             _mockCoreDataApiClient.GetCaseInformationByUrnAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ReturnsForAnyArgs(_caseInformationFake.GetCaseInformationByURN_Payload());

//             //Act
//             var results = await coreDataApiCaseInformationByUrnFunction.Run(CreateHttpRequest(), urn) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             var response = results?.Value as List<CaseDetails>;
//             Assert.Equal(200, results?.StatusCode);
//             Assert.True(response?.Any());
//             Assert.Equal(urn, response?.FirstOrDefault()?.UniqueReferenceNumber);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_500_When_Http_Exception_Occurs()
//         {
//             //Arrange
//             var caseId = 18868;
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseInformationByUrnFunction();
//             _mockOnBehalfOfTokenClient.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ThrowsForAnyArgs(new MsalException());

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId.ToString()) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(500, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_500_When_Core_Data_Api_Exception_Occurs()
//         {
//             //Arrange
//             var caseId = 18868;
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseInformationByUrnFunction();
//             _mockCoreDataApiClient.GetCaseInformationByUrnAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ThrowsForAnyArgs(new CoreDataApiException("Test core data api exception", new Exception()));

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId.ToString()) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(500, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_500_When_Unhandled_Exception_Occurs()
//         {
//             //Arrange
//             var caseId = 18868;
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseInformationByUrnFunction();
//             _mockCoreDataApiClient.GetCaseInformationByUrnAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ThrowsForAnyArgs(new Exception());

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId.ToString()) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(500, results?.StatusCode);
//         }

//         private CoreDataApiCaseInformationByUrn GetCoreDataApiCaseInformationByUrnFunction()
//         {
//             return new CoreDataApiCaseInformationByUrn(_mockLogger, _mockOnBehalfOfTokenClient, _mockCoreDataApiClient, _mockConfiguration, _mockTokenValidator);
//         }
//     }
// }
