using System;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.CaseDataImplementations.Cda.Extensions;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Extensions;

namespace PolarisGateway.CaseDataImplementations.Cda.Factories
{
    // ReSharper disable once InconsistentNaming
    public class AuthenticatedGraphQLHttpRequestFactory : IAuthenticatedGraphQLHttpRequestFactory
    {
        private readonly ILogger<AuthenticatedGraphQLHttpRequestFactory> _logger;

        public AuthenticatedGraphQLHttpRequestFactory(ILogger<AuthenticatedGraphQLHttpRequestFactory> logger)
        {
            _logger = logger;
        }

        public AuthenticatedGraphQLHttpRequest Create(string accessToken, GraphQLHttpRequest graphQlHttpRequest, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), graphQlHttpRequest.ToJson());
            var authRequest = new AuthenticatedGraphQLHttpRequest(accessToken, correlationId, graphQlHttpRequest);
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return authRequest;
        }
    }
}
