using System;
using GraphQL.Client.Http;
using RumpoleGateway.CaseDataImplementations.Cda.Extensions;

namespace RumpoleGateway.CaseDataImplementations.Cda.Factories
{
    public interface IAuthenticatedGraphQLHttpRequestFactory
    {
        AuthenticatedGraphQLHttpRequest Create(string accessToken, GraphQLHttpRequest graphQlHttpRequest, Guid correlationId);
    }
}
