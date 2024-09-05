using System.Net;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace PolarisGateway;

public static class StartupHelpers
{
    private const int RetryAttempts = 2;
    private const int FirstRetryDelaySeconds = 1;
    
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
            retryCount: RetryAttempts);

        return Policy
            .HandleResult<HttpResponseMessage>(result => ShouldRetry(result.RequestMessage, result))
            .WaitAndRetryAsync(delay);

        static bool ShouldRetry(HttpRequestMessage request, HttpResponseMessage response)
        {
            // Skip retry if the custom header is present
            if (request.Headers.Contains("X-Skip-Retry"))
            {
                return false;
            }

            // #27567 - retry on 404 as well as 5xx as coordinator can return 404 when the entity is not found in the durable entity store
            return response.StatusCode == HttpStatusCode.NotFound || response.StatusCode >= HttpStatusCode.InternalServerError;
        }
    }
    
    public static string GetValueFromConfig(IConfiguration configuration, string secretName)
    {
        var secret = configuration[secretName];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new Exception($"Secret cannot be null: {secretName}");
        }

        return secret;
    }
}