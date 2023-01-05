// using RumpoleGateway.Extensions;
// using System;
// using System.Linq;
// using AutoFixture;
// using FluentAssertions;
// using FluentAssertions.Execution;
// using GraphQL.Client.Http;
// using GraphQL.Client.Serializer.Newtonsoft;
// using Xunit;

// namespace RumpoleGateway.Tests.Extensions
// {
//     public class AuthenticatedGraphQlHttpRequestTests
//     {
//         private readonly Fixture _fixture;

//         public AuthenticatedGraphQlHttpRequestTests()
//         {
//             _fixture = new Fixture();
//         }

//         [Fact]
//         public void WhenCallingANewInstance_WithValidParameters_ThenAValidObjectIsCreated()
//         {
//             var testInstance = new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<GraphQLHttpRequest>());

//             testInstance.Should().NotBeNull();
//         }

//         [Fact]
//         public void WhenCallingANewInstance_WithoutAnAccessToken_ThenAnArgumentExceptionIsThrown()
//         {
//             var testInstance = () => new AuthenticatedGraphQLHttpRequest(null, _fixture.Create<Guid>(), _fixture.Create<GraphQLHttpRequest>());

//             testInstance.Should().Throw<ArgumentException>().WithParameterName("accessToken");
//         }

//         [Fact]
//         public void WhenCallingANewInstance_WithoutACorrelationId_ThenAnArgumentExceptionIsThrown()
//         {
//             var testInstance = () => new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), Guid.Empty, _fixture.Create<GraphQLHttpRequest>());

//             testInstance.Should().Throw<ArgumentException>().WithParameterName("correlationId");
//         }

//         [Fact]
//         public void WhenCallingANewInstance_WithoutAnEmptyGraphQlRequest_ThenAnArgumentNullExceptionIsThrown()
//         {
//             var testInstance = () => new AuthenticatedGraphQLHttpRequest(_fixture.Create<string>(), _fixture.Create<Guid>(), null);

//             testInstance.Should().Throw<ArgumentNullException>();
//         }

//         [Fact]
//         public void WhenAValidInstanceHasBeenCreated_WhenCallingToHttpRequestMessage_TheResponseContainsTheExpectedHeaders()
//         {
//             var accessToken = _fixture.Create<string>();
//             var correlationId = _fixture.Create<Guid>();
//             var testInstance = new AuthenticatedGraphQLHttpRequest(accessToken, correlationId, _fixture.Create<GraphQLHttpRequest>());
//             var testRequestMessage = testInstance.ToHttpRequestMessage(new GraphQLHttpClientOptions(), new NewtonsoftJsonSerializer());

//             using (new AssertionScope())
//             {
//                 testRequestMessage.Headers.Should().Contain(x => x.Key == AuthenticationKeys.Authorization);

//                 var authHeaderValues = testRequestMessage.Headers.GetValues(AuthenticationKeys.Authorization);
//                 authHeaderValues.Should().Contain(x => x == $"{AuthenticationKeys.Bearer} {accessToken}");

//                 testRequestMessage.Headers.Should().Contain(x => x.Key == "Correlation-Id");
//                 testRequestMessage.Headers.FirstOrDefault(x => x.Key == "Correlation-Id").Value.Should().NotBeNull().And.BeEquivalentTo(correlationId.ToString());
//                 testRequestMessage.Headers.Should().Contain(x => x.Key == "Request-Ip-Address");
//             }
//         }
//     }
// }
