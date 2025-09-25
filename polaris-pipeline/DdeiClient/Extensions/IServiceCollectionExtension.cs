using Common.Streaming;
using Ddei.Domain.Response.Document;
using Ddei.Mappers;
using DdeiClient.Clients;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Net.Http.Headers;

namespace Ddei.Extensions;

public static class IServiceCollectionExtension
{
    private const string FunctionKey = "x-functions-key";
    private const string DdeiBaseUrlConfigKey = "DdeiBaseUrl";
    private const string MdsBaseUrlConfigKey = "MdsBaseUrl";
    private const string MdsMockBaseUrlConfigKey = "MdsMockBaseUrl";
    private const string DdeiAccessKeyConfigKey = "DdeiAccessKey";
    private const string MdsAccessKeyConfigKey = "MdsAccessKey";
    private const string MdsMockAccessKeyConfigKey = "MdsMockAccessKey";
    private const string DdeiClientTimeoutSecondsConfigKey = "DdeiClientTimeoutSeconds";
    private const string MdsClientTimeoutSecondsConfigKey = "MdsClientTimeoutSeconds";
    private const int RetryAttempts = 1;
    private const int FirstRetryDelaySeconds = 1;

    public static void AddDdeiClientGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMdsClientFactory, MdsClientFactory>();
        services.AddHttpClientWithDefaults<IDdeiAuthClient, DdeiAuthClient>(configuration, DdeiBaseUrlConfigKey, DdeiAccessKeyConfigKey, "Ddei", DdeiClientTimeoutSecondsConfigKey);
        services.AddHttpClientWithDefaults(configuration, MdsBaseUrlConfigKey, MdsAccessKeyConfigKey, nameof(MdsClients.Mds), MdsClientTimeoutSecondsConfigKey);
        services.AddHttpClientWithDefaults(configuration, MdsMockBaseUrlConfigKey, MdsMockAccessKeyConfigKey, nameof(MdsClients.MdsMock), MdsClientTimeoutSecondsConfigKey);

        services.AddScoped<IMdsClient, MdsClient>();

        services.AddDdeiServices();
    }

    private static void AddDdeiServices(this IServiceCollection services)
    {
        services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();
        services.AddTransient<IDdeiAuthClientRequestFactory, DdeiAuthClientRequestFactory>();
        services.AddTransient<IMdsClientRequestFactory, MdsClientRequestFactory>();
        services.AddTransient<ICaseDocumentMapper<DdeiDocumentResponse>, CaseDocumentMapper>();
        services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
        services.AddTransient<ICaseDocumentMapper<DdeiDocumentResponse>, CaseDocumentMapper>();
        services.AddTransient<ICaseDocumentNoteMapper, CaseDocumentNoteMapper>();
        services.AddTransient<ICaseDocumentNoteResultMapper, CaseDocumentNoteResultMapper>();
        services.AddTransient<ICaseExhibitProducerMapper, CaseExhibitProducerMapper>();
        services.AddTransient<ICaseWitnessMapper, CaseWitnessMapper>();
        services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
        services.AddTransient<ICaseIdentifiersMapper, CaseIdentifiersMapper>();
        services.AddTransient<ICmsMaterialTypeMapper, CmsMaterialTypeMapper>();
        services.AddTransient<ICaseWitnessStatementMapper, CaseWitnessStatementMapper>();
    }

    private static void AddHttpClientWithDefaults(this IServiceCollection services, IConfiguration configuration, string urlKey, string accessKey, string name, string timeoutKey)
    {
        services.AddHttpClient(name, (_, client) =>
            {
                client.BaseAddress = new Uri(configuration[urlKey]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration[accessKey]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                var hasTimeout = int.TryParse(configuration[timeoutKey], out var timeout);
                client.Timeout = TimeSpan.FromSeconds(hasTimeout ? timeout : 100);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy()).AddAsKeyed();
    }

    private static void AddHttpClientWithDefaults<TClient, TImplementation>(this IServiceCollection services, IConfiguration configuration, string urlKey, string accessKey, string name, string timeoutKey)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(name, (_, client) =>
            {
                client.BaseAddress = new Uri(configuration[urlKey]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration[accessKey]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                var hasTimeout = int.TryParse(configuration[timeoutKey], out var timeout);
                client.Timeout = TimeSpan.FromSeconds(hasTimeout ? timeout : 100);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy()).AddAsKeyed();
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