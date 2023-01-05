using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System;
using System.Net.Http;

namespace PolarisGateway.CaseDataImplementations.Cda.Extensions
{
    public class AuthenticatedGraphQLHttpRequest : GraphQLHttpRequest
    {

        private readonly string _accessToken;
        private readonly Guid _correlationId;

        public AuthenticatedGraphQLHttpRequest(string accessToken, Guid correlationId, GraphQLHttpRequest request)
                : base(request)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Parameter must be supplied.", nameof(accessToken));
            }

            if (correlationId == Guid.Empty)
            {
                throw new ArgumentException("Parameter must be supplied.", nameof(correlationId));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _accessToken = accessToken;
            _correlationId = correlationId;
        }

        public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
        {
            var message = base.ToHttpRequestMessage(options, serializer);
            message.Headers.Add(AuthenticationKeys.Authorization, $"{AuthenticationKeys.Bearer} {_accessToken}");
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            message.Headers.Add("Request-Ip-Address", "0.0.0.0");
            return message;
        }
    }
}
