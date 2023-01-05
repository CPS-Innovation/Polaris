using System;
using GraphQL.Client.Http;
using PolarisGateway.CaseDataImplementations.Cda.Extensions;

namespace PolarisGateway.CaseDataImplementations.Cda.Factories
{
    public interface IAuthenticatedGraphQLHttpRequestFactory
    {
        AuthenticatedGraphQLHttpRequest Create(string accessToken, GraphQLHttpRequest graphQlHttpRequest, Guid correlationId);
    }
}
