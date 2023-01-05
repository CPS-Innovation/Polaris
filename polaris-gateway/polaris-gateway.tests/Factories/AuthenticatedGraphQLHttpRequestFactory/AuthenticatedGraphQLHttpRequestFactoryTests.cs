// using System;
// using AutoFixture;
// using FluentAssertions;
// using GraphQL.Client.Http;
// using Microsoft.Extensions.Logging;
// using Moq;
// using PolarisGateway.Extensions;
// using Xunit;

// namespace PolarisGateway.tests.Factories.AuthenticatedGraphQLHttpRequestFactory
// {
//     // ReSharper disable once InconsistentNaming
//     public class AuthenticatedGraphQLHttpRequestFactoryTests
//     {
//         [Fact]
//         public void Create_CreatesAuthenticatedRequest()
//         {
//             var fixture = new Fixture();
//             var correlationId = fixture.Create<Guid>();
//             var loggerMock = new Mock<ILogger<PolarisGateway.Factories.AuthenticatedGraphQLHttpRequestFactory.AuthenticatedGraphQLHttpRequestFactory>>();

//             var factory = new PolarisGateway.Factories.AuthenticatedGraphQLHttpRequestFactory.AuthenticatedGraphQLHttpRequestFactory(loggerMock.Object);

//             var authenticatedRequest = factory.Create( "accessToken", new GraphQLHttpRequest(), correlationId);

//             authenticatedRequest.Should().BeOfType<AuthenticatedGraphQLHttpRequest>();
//         }
//     }
// }
