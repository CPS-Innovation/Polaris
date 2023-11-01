using Common.Dto.Response;
using Common.Mappers;
using Common.Mappers.Contracts;
using Ddei.Factories.Contracts;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Mappers.Contract;
using DdeiClient.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Common.Services.DocumentToggle;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Ddei.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        private const string FunctionKey = "x-functions-key";

        public static void AddDdeiClient(this IServiceCollection services, IConfigurationRoot configuration)
        {                
            services.AddTransient<ICaseDataArgFactory, CaseDataArgFactory>();

            services.AddHttpClient<IDdeiClient, DdeiClient>((service, client) =>
            {
                client.BaseAddress = new Uri(configuration["DdeiBaseUrl"]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration["DdeiAccessKey"]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
              .AddPolicyHandler(GetRetryPolicy());

            services.AddTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>();
            services.AddTransient<ICaseDocumentMapper<DdeiCaseDocumentResponse>, DdeiCaseDocumentMapper>();
            services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
            services.AddTransient<IDocumentToggleService, DocumentToggleService>();
            services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(
              DocumentToggleService.ReadConfig()
            ));

        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(delay);
        }
    }
}