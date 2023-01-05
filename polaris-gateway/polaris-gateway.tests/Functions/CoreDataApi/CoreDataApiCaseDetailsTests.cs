// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Moq;
// using NSubstitute;
// using NSubstitute.ExceptionExtensions;
// using PolarisGateway.Clients.CoreDataApi;
// using PolarisGateway.Clients.OnBehalfOfTokenClient;
// using PolarisGateway.Domain.CaseData;
// using PolarisGateway.Tests.FakeData;
// using Xunit;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Identity.Client;
// using PolarisGateway.Domain.CoreDataApi;
// using System;
// using PolarisGateway.Domain.Validators;
// using PolarisGateway.Functions.CoreDataApi.Case;

// namespace PolarisGateway.Tests.Functions.CoreDataApi
// {
//     public class CoreDataApiCaseDetailsFunctionTests : SharedMethods.SharedMethods, IClassFixture<CaseInformationFake>
//     {
//         private readonly ILogger<CoreDataApiCaseDetails> _mockLogger = Substitute.For<ILogger<CoreDataApiCaseDetails>>();
//         private readonly IOnBehalfOfTokenClient _mockOnBehalfOfTokenClient = Substitute.For<IOnBehalfOfTokenClient>();
//         private readonly ICoreDataApiClient _mockCoreDataApiClient = Substitute.For<ICoreDataApiClient>();
//         private readonly IConfiguration _mockConfiguration = Substitute.For<IConfiguration>();
//         private readonly CaseInformationFake _caseInformationFake;
//         private readonly IAuthorizationValidator _mockTokenValidator = Substitute.For<IAuthorizationValidator>();

//         public CoreDataApiCaseDetailsFunctionTests(CaseInformationFake caseInformationFake)
//         {
//             _caseInformationFake = caseInformationFake;

//             _mockTokenValidator.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()).ReturnsForAnyArgs(true);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_400_When_No_CorrelationId_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequestWithoutCorrelationId(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(400, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_400_When_No_AuthToken_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequestWithoutToken(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(400, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_401_When_Invalid_AuthToken_Supplied()
//         {
//             //Arrange
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();
//             _mockTokenValidator.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()).ReturnsForAnyArgs(false);

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), string.Empty) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(401, results?.StatusCode);
//         }

//         [Theory]
//         [InlineData("")]
//         [InlineData("Not an int")]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_404_When_Case_Id_Is_Invalid(string caseId)
//         {
//             //Arrange
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(404, results?.StatusCode);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_No_Data_Found()
//         {
//             //Arrange
//             var caseId = "18846";
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(404, results?.StatusCode);
//             Assert.Contains(caseId, results?.Value?.ToString());

//         }


//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_200_When_Valid_Input_Supplied()
//         {
//             //Arrange
//             var caseId = 18868;
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();
//             _mockCoreDataApiClient.GetCaseDetailsByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ReturnsForAnyArgs(_caseInformationFake.GetCaseInformationByURN_Payload()
//                                                                                                                    .FirstOrDefault(x => x.Id == caseId));

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId.ToString()) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             var response = results?.Value as CaseDetails;
//             Assert.Equal(200, results?.StatusCode);
//             Assert.Equal(caseId, response?.Id);
//         }

//         [Fact]
//         public async Task CoreDataApiCaseDetailsFunction_Should_Return_Response_500_When_Http_Exception_Occurs()
//         {
//             //Arrange
//             var caseId = 18868;
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();
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
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();
//             _mockCoreDataApiClient.GetCaseDetailsByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ThrowsForAnyArgs(new CoreDataApiException("Test core data api exception", new Exception()));

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
//             var coreDataApiCaseDetailsFunction = GetCoreDataApiCaseDetailsFunction();
//             _mockCoreDataApiClient.GetCaseDetailsByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()).ThrowsForAnyArgs(new Exception());

//             //Act
//             var results = await coreDataApiCaseDetailsFunction.Run(CreateHttpRequest(), caseId.ToString()) as Microsoft.AspNetCore.Mvc.ObjectResult;

//             //Assert
//             Assert.Equal(500, results?.StatusCode);
//         }

//         private CoreDataApiCaseDetails GetCoreDataApiCaseDetailsFunction()
//         {
//             return new CoreDataApiCaseDetails(_mockLogger, _mockOnBehalfOfTokenClient, _mockCoreDataApiClient, _mockConfiguration, _mockTokenValidator);
//         }
//     }
// }
