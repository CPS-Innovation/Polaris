// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using AutoFixture;
// using FluentAssertions;
// using GraphQL;
// using GraphQL.Client.Abstractions;
// using GraphQL.Client.Http;
// using Microsoft.Extensions.Logging;
// using Moq;
// using RumpoleGateway.Clients.CoreDataApi;
// using RumpoleGateway.Domain.CaseData;
// using RumpoleGateway.Domain.CoreDataApi.ResponseTypes;
// using RumpoleGateway.Extensions;
// using RumpoleGateway.Factories.AuthenticatedGraphQLHttpRequestFactory;
// using RumpoleGateway.Tests.FakeData;
// using Xunit;

// namespace RumpoleGateway.Tests.Clients.CoreDataApi
// {
//     public class CoreDataApiClientTests : IClassFixture<ResponseCaseDetailsFake>
//     {
//         private readonly Mock<IGraphQLClient> _coreDataApiClientMock;
//         private readonly Mock<IAuthenticatedGraphQLHttpRequestFactory> _authenticatedGraphQLHttpRequestFactoryMock;
//         private readonly ResponseCaseDetailsFake _responseCaseDetailsFake;
//         private readonly Fixture _fixture;
//         private readonly Guid _correlationId;
//         private readonly Mock<ILogger<CoreDataApiClient>> _mockCoreDataApiClientLogger;

//         public CoreDataApiClientTests()
//         {
//             _coreDataApiClientMock = new Mock<IGraphQLClient>();
//             _authenticatedGraphQLHttpRequestFactoryMock = new Mock<IAuthenticatedGraphQLHttpRequestFactory>();
//             _fixture = new Fixture();
//             _responseCaseDetailsFake = new ResponseCaseDetailsFake();
//             _correlationId = _fixture.Create<Guid>();

//             _mockCoreDataApiClientLogger = new Mock<ILogger<CoreDataApiClient>>();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseDetailsById_Should_Return_Response_Valid_response()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                                                                     .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = new GraphQLResponse<ResponseCaseDetails>
//             {
//                 Data = _responseCaseDetailsFake.GetCaseDetailsResponse_Payload()
//             };
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseDetails>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseDetailsByIdAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Id.Should().Be(fakedResponse.Data.CaseDetails.Id);
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseDetailsById_WhenResponseData_IsNull_ReturnsNull()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = new GraphQLResponse<ResponseCaseDetails>
//             {
//                 Data = null
//             };
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseDetails>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseDetailsByIdAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Should().BeNull();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseDetailsById_WhenCaseDetails_IsNull_ReturnsNull()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = new GraphQLResponse<ResponseCaseDetails>
//             {
//                 Data = _responseCaseDetailsFake.GetCaseDetailsResponse_Payload()
//             };
//             fakedResponse.Data.CaseDetails = null;
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseDetails>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseDetailsByIdAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Should().BeNull();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseDetailsById_WhenCaseDetails_ThrowsException_IsCaughtSuccessfully()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = new GraphQLResponse<ResponseCaseDetails>
//             {
//                 Data = _responseCaseDetailsFake.GetCaseDetailsResponse_Payload()
//             };
//             fakedResponse.Data.CaseDetails = null;
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseDetails>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
//                 .Throws<Exception>();

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = async () => await coreDataApiClient.GetCaseDetailsByIdAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             await results.Should().ThrowAsync<Exception>();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseInformationByUrnAsync_Should_Return_Response_Valid_response()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                                                                     .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = _fixture.Create<GraphQLResponse<ResponseCaseInformationByUrn>>();
//             fakedResponse.Data.CaseDetails = _fixture.CreateMany<CaseDetails>(5).ToList();
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseInformationByUrn>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseInformationByUrnAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Count.Should().Be(5);
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseInformationByUrnAsync_WhenBothResponseData_AndErrorsCollection_IsNull_ReturnsNull()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = _fixture.Create<GraphQLResponse<ResponseCaseInformationByUrn>>();
//             fakedResponse.Data = null;
//             fakedResponse.Errors = null;
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseInformationByUrn>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseInformationByUrnAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Should().BeNull();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseInformationByUrnAsync_WhenBothResponseData_AndErrorsCollection_IsNOTNull_ThrowsException()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = _fixture.Create<GraphQLResponse<ResponseCaseInformationByUrn>>();
//             fakedResponse.Data = null;
//             fakedResponse.Errors = _fixture.CreateMany<GraphQLError>(3).ToArray();
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseInformationByUrn>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = async () => await coreDataApiClient.GetCaseInformationByUrnAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             await results.Should().ThrowAsync<Exception>();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseInformationByUrnAsync_WhenCaseDetails_IsNull_ReturnsNull()
//         {
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = _fixture.Create<GraphQLResponse<ResponseCaseInformationByUrn>>();
//             fakedResponse.Data.CaseDetails = null;
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseInformationByUrn>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakedResponse);

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = await coreDataApiClient.GetCaseInformationByUrnAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             results.Should().BeNull();
//         }

//         [Fact]
//         public async Task CoreDataApiClient_GetCaseInformationByUrnAsync_ThrowsException_IsCaughtSuccessfully()
//         {
//             //Arrange
//             _authenticatedGraphQLHttpRequestFactoryMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<GraphQLHttpRequest>(), It.IsAny<Guid>()))
//                 .Returns(new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _correlationId, _fixture.Create<GraphQLHttpRequest>()));

//             var fakedResponse = _fixture.Create<GraphQLResponse<ResponseCaseInformationByUrn>>();
//             fakedResponse.Data.CaseDetails = _fixture.CreateMany<CaseDetails>(5).ToList();
//             _coreDataApiClientMock.Setup(x => x.SendQueryAsync<ResponseCaseInformationByUrn>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
//                 .Throws<Exception>();

//             var coreDataApiClient = GetCoreDataApiClient();

//             //Act
//             var results = async () => await coreDataApiClient.GetCaseInformationByUrnAsync(_fixture.Create<string>(), _fixture.Create<string>(), _correlationId);

//             //Assert
//             await results.Should().ThrowAsync<Exception>();
//         }

//         #region private methods

//         private CoreDataApiClient GetCoreDataApiClient()
//         {
//             return new CoreDataApiClient(_coreDataApiClientMock.Object, _authenticatedGraphQLHttpRequestFactoryMock.Object, _mockCoreDataApiClientLogger.Object);
//         }

//         #endregion private methods
//     }

// }
