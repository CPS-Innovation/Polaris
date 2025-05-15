using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Streaming;
using Ddei.Factories;
using Ddei.Factories.Contracts;
using Ddei.Mappers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Ddei.Domain.Response.Document;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;

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
    private const int RetryAttempts = 1;
    private const int FirstRetryDelaySeconds = 1;

    public static void AddDdeiClientGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDdeiClientFactory, DdeiClientFactory>();

        services.AddHttpClientWithDefaults<IDdeiClient, DdeiClient.Clients.DdeiClient>(configuration, DdeiBaseUrlConfigKey, DdeiAccessKeyConfigKey, nameof(DdeiClients.Ddei));
        services.AddHttpClientWithDefaults<IDdeiClient, DdeiClient.Clients.MdsClient>(configuration, MdsBaseUrlConfigKey, MdsAccessKeyConfigKey, nameof(DdeiClients.Mds));
        services.AddHttpClientWithDefaults<IDdeiClient, DdeiClient.Clients.MdsMockClient>(configuration, MdsMockBaseUrlConfigKey, MdsMockAccessKeyConfigKey, nameof(DdeiClients.MdsMock));

        services.AddKeyedScoped<IDdeiClient, DdeiClient.Clients.DdeiClient>(DdeiClients.Ddei);
        services.AddKeyedScoped<IDdeiClient, DdeiClient.Clients.MdsClient>(DdeiClients.Mds);
        services.AddKeyedScoped<IDdeiClient, DdeiClient.Clients.MdsMockClient>(DdeiClients.MdsMock);

        services.AddDdeiServices();
    }

    public static void AddDdeiClientCoordinator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDdeiClientGateway(configuration);
        services.AddScoped<IDdeiClient, DdeiClient.Clients.DdeiClient>();
    }

    private static void AddDdeiServices(this IServiceCollection services)
    {
        services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();
        services.AddKeyedTransient<IDdeiClientRequestFactory, DdeiClientRequestFactory>(DdeiClients.Ddei);
        services.AddKeyedTransient<IDdeiClientRequestFactory, MdsClientRequestFactory>(DdeiClients.Mds);
        services.AddKeyedTransient<IDdeiClientRequestFactory, MdsClientRequestFactory>(DdeiClients.MdsMock);
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

    private static void AddHttpClientWithDefaults<TClient, TImplementation>(this IServiceCollection services, IConfiguration configuration, string urlKey, string accessKey, string name)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(name, (_, client) =>
            {
                client.BaseAddress = new Uri(configuration[urlKey]);
                client.DefaultRequestHeaders.Add(FunctionKey, configuration[accessKey]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
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