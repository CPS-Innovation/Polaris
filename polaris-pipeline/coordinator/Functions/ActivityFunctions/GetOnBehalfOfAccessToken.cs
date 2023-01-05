using System;
using System.Threading.Tasks;
using Common.Adapters;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetOnBehalfOfAccessToken
    {
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly ILogger<GetOnBehalfOfAccessToken> _log;

        public GetOnBehalfOfAccessToken(IIdentityClientAdapter identityClientAdapter, ILogger<GetOnBehalfOfAccessToken> logger)
        {
            _identityClientAdapter = identityClientAdapter;
            _log = logger;
        }

        [FunctionName("GetOnBehalfOfAccessToken")]
        public async Task<string> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(GetOnBehalfOfAccessToken)} - {nameof(Run)}";
            var request = context.GetInput<GetOnBehalfOfTokenRequest>();
            
            if (string.IsNullOrWhiteSpace(request.AccessToken))
                throw new ArgumentException("Access token cannot be null.");

            if (request.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must not be null");

            if (string.IsNullOrWhiteSpace(request.RequestedScopes))
                throw new ArgumentException("Cannot get an OBO token without at least one defined scope");
            
            _log.LogMethodEntry(request.CorrelationId, loggingName, request.ToJson());
            
            var onBehalfOfToken = await _identityClientAdapter.GetAccessTokenOnBehalfOfAsync(request.AccessToken, request.RequestedScopes, request.CorrelationId);
            
            _log.LogMethodExit(request.CorrelationId, loggingName, string.Empty);
            return onBehalfOfToken;
        }
    }
}
