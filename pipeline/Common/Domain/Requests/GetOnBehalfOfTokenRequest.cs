using System;

namespace Common.Domain.Requests
{
    public class GetOnBehalfOfTokenRequest
    {
        public GetOnBehalfOfTokenRequest(string accessToken, string requestedScopes, Guid correlationId)
        {
            AccessToken = accessToken;
            RequestedScopes = requestedScopes;
            CorrelationId = correlationId;
        }
        
        public string AccessToken { get; set; }

        public string RequestedScopes { get; set; }

        public Guid CorrelationId { get; set; }
    }
}