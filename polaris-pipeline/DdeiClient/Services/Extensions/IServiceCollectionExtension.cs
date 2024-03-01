using Common.Dto.Response;
using Ddei.Factories.Contracts;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Mappers;
using DdeiClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using Common.Streaming;

namespace Ddei.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        private const string FunctionKey = "x-functions-key";
        private const string DdeiBaseUrlConfigKey = "DdeiBaseUrl";
        private const string DdeiAccessKeyConfigKey = "DdeiAccessKey";
        private const int RetryAttempts = 1;
        private const int FirstRetryDelaySeconds = 1;

        public static void AddDdeiClient(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();

            services.AddHttpClient<IDdeiClient, DdeiClient>((service, client) =>
            {
                client.BaseAddress = new Uri(configuration[DdeiBaseUrlConfigKey]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration[DdeiAccessKeyConfigKey]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
              .AddPolicyHandler(GetRetryPolicy());

            services.AddTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>();
            services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, CaseDocumentMapper>();
            services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
            services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, CaseDocumentMapper>();
            services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
            services.AddTransient<ICaseIdentifiersMapper, CaseIdentifiersMapper>();
            services.AddTransient<ICmsAuthValuesMapper, CmsAuthValuesMapper>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
                retryCount: RetryAttempts);

            static bool responseStatusCodePredicate(HttpResponseMessage response) =>
                response.StatusCode >= HttpStatusCode.InternalServerError
                || response.StatusCode == HttpStatusCode.NotFound;

            static bool methodPredicate(HttpResponseMessage response) =>
                response.RequestMessage.Method != HttpMethod.Post
                && response.RequestMessage.Method != HttpMethod.Put;

            return Policy
                .HandleResult<HttpResponseMessage>(r => responseStatusCodePredicate(r) && methodPredicate(r))
                .WaitAndRetryAsync(delay);
        }
    }
}