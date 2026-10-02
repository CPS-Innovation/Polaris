// <copyright file="ServiceExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.ApplicationStartup;

using Common.Domain.Document;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Factories.ComputerVisionClientFactory;
using Common.Handlers;
using Common.Mappers;
using Common.Services.BlobStorage;
using Common.Services.DocumentToggle;
using Common.Services.OcrService;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;
using Common.Streaming;
using Common.Telemetry;
using Common.Wrappers;
using coordinator.Builders;
using coordinator.Clients;
using coordinator.Constants;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Factories.UploadFileNameFactory;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Mappers;
using coordinator.Search;
using coordinator.Services;
using coordinator.Services.ClearDownService;
using coordinator.Validators;
using Ddei.Extensions;
using DdeiClient.Clients;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Configuration;
using DdeiClient.Services.CaseUrnResolver;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using PdfGenerator = Common.Clients.PdfGenerator;
using PdfRedactor = coordinator.Clients.PdfRedactor;
using RedactionLogger = coordinator.Clients.RedactionLogger;
using TextExtractor = coordinator.Clients.TextExtractor;

public static class ServiceExtensions
{
    private const int FirstRetryDelaySeconds = 1;

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        var configuration = sp.GetService<IConfiguration>();

        services.AddSingleton(configuration);
        BuildOcrService(services, configuration);

        services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddTransient<IValidatorWrapper<DocumentPayload>, ValidatorWrapper<DocumentPayload>>();
        services.AddSingleton<IConvertModelToHtmlService, ConvertModelToHtmlService>();
        services.AddTransient<TextExtractor.IRequestFactory, TextExtractor.RequestFactory>();
        services.AddTransient<PdfGenerator.IPdfGeneratorRequestFactory, PdfGenerator.PdfGeneratorRequestFactory>();
        services.AddTransient<PdfRedactor.IRequestFactory, PdfRedactor.RequestFactory>();
        services.AddTransient<TextExtractor.ISearchDtoContentFactory, TextExtractor.SearchDtoContentFactory>();
        services.AddTransient<IQueryConditionFactory, QueryConditionFactory>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IHttpResponseMessageStreamFactory, HttpResponseMessageStreamFactory>();
        services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
        services.AddBlobStorageWithDefaultAzureCredential(configuration);
        services.AddPiiService();

        services.AddSingleton<IUploadFileNameFactory, UploadFileNameFactory>();
        services
            .AddHttpClientWithDefaults<
                PdfGenerator.IPdfGeneratorClient,
                PdfGenerator.PdfGeneratorClient,
                coordinator.Clients.PdfGenerator.GeneratorConfig>()
                    .AddPolicyHandler((serviceProvider, _) =>
                    {
                        var config = serviceProvider
                            .GetRequiredService<IOptions<coordinator.Clients.PdfGenerator.GeneratorConfig>>()
                            .Value;

                        return GetRetryPolicy(config.MaxRetries);
                    });

        services
            .AddHttpClientWithDefaults<
                TextExtractor.ITextExtractorClient,
                TextExtractor.TextExtractorClient,
                TextExtractor.TextExtractorConfig>();

        services
            .AddHttpClientWithDefaults<
                RedactionLogger.IRedactionLoggerClient,
                RedactionLogger.RedactionLoggerClient,
                RedactionLogger.RedactionLoggerConfig>()
            .AddHttpMessageHandler<RedactionLogger.RedactionLoggerAuthDelegatingHandler>()
            .AddPolicyHandler((serviceProvider, _) =>
            {
                var config = serviceProvider
                    .GetRequiredService<IOptions<RedactionLogger.RedactionLoggerConfig>>()
                    .Value;

                return GetRetryPolicy(config.MaxRetries);
            });

        services
            .AddHttpClientWithDefaults<
                PdfRedactor.IPdfRedactorClient,
                PdfRedactor.PdfRedactorClient,
                PdfRedactor.PdfRedactorConfig>()
            .AddPolicyHandler((serviceProvider, _) =>
            {
                var config = serviceProvider
                    .GetRequiredService<IOptions<PdfRedactor.PdfRedactorConfig>>()
                    .Value;

                return GetRetryPolicy(config.MaxRetries);
            });

        services.AddTransient<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
        services.AddScoped<IRedactionService, RedactionService>();
        services.AddScoped<IValidator<RedactPdfRequestWithDocumentDto>, RedactPdfRequestWithDocumentValidator>();
        services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
        services.AddScoped<IValidator<AddDocumentNoteDto>, DocumentNoteValidator>();
        services.AddScoped<IValidator<RenameDocumentDto>, RenameDocumentValidator>();
        services.AddScoped<IValidator<ReclassifyDocumentDto>, ReclassifyDocumentValidator>();
        services.AddScoped<IValidator<ModifyDocumentWithDocumentDto>, ModifyDocumentWithDocumentValidator>();
        services.AddSingleton<ICmsDocumentsResponseValidator, CmsDocumentsResponseValidator>();
        services.AddSingleton<IClearDownService, ClearDownService>();
        services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();
        services.RegisterCoordinatorMapsterConfiguration();
        services.AddDdeiClientGateway(configuration);
        services.AddSingleton<IDocumentToggleService>(new DocumentToggleService(DocumentToggleService.ReadConfig()));

        services.AddSingleton<ICaseDurableEntityMapper, CaseDurableEntityMapper>();
        services.AddSingleton<IStateStorageService, StateStorageService>();
        services.AddSingleton<IRedactionSearchDtoMapper, RedactionSearchDtoMapper>();

        services.AddSingleton<IOcrDocumentSearch, OcrDocumentSearch>();
        services.AddScoped<IBulkRedactionSearchResponseBuilder, BulkRedactionSearchResponseBuilder>();
        services.AddScoped<IBulkRedactionSearchService, BulkRedactionSearchService>();

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<ICaseUrnResolver, CaseUrnResolver>();
        services.AddTransient<RedactionLogger.RedactionLoggerAuthDelegatingHandler>();
        services.AddServiceOptions<RedactionLogger.RedactionLoggerConfig>(RedactionLogger.RedactionLoggerConfig.DefaultSectionName);
        services.AddServiceOptions<TextExtractor.TextExtractorConfig>(TextExtractor.TextExtractorConfig.DefaultSectionName);
        services.AddServiceOptions<PdfRedactor.PdfRedactorConfig>(PdfRedactor.PdfRedactorConfig.DefaultSectionName);
        services.AddServiceOptions<coordinator.Clients.PdfGenerator.GeneratorConfig>(coordinator.Clients.PdfGenerator.GeneratorConfig.DefaultSectionName);
        services.AddSingleton<IMasterDataServiceApiClientFactory, MasterDataServiceApiClientFactory>();
        services.AddSingleton<IMasterDataServiceClient, MasterDataServiceClient>();
        services.AddServiceOptions<MasterDataServiceClientOptions>(MasterDataServiceClientOptions.DefaultSectionName);
        return services;
    }

    public static IHttpClientBuilder AddHttpClientWithDefaults<
        TInterface,
        TImplementation,
        TConfig>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TConfig : class, IHttpClientConfig
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClient<TInterface, TImplementation>(
            (serviceProvider, client) =>
            {
                var logger = serviceProvider
                            .GetRequiredService<ILogger<TImplementation>>();

                var config = serviceProvider
                    .GetRequiredService<IOptions<TConfig>>()
                    .Value;

                logger.LogInformation(
                    "Configuring HttpClient {ImplementationType} using {ConfigType}. BaseUrl: '{BaseUrl}', TimeoutSeconds: {TimeoutSeconds}",
                    typeof(TImplementation).FullName,
                    typeof(TConfig).FullName,
                    config.BaseUrl,
                    config.TimeoutSeconds);

                client.BaseAddress = new Uri(config.BaseUrl);

                client.DefaultRequestHeaders.CacheControl =
                    new CacheControlHeaderValue
                    {
                        NoCache = true,
                    };

                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            });
    }

    private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(
         int retryAttempts)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(
                result => ShouldRetry(result.RequestMessage, result))
            .WaitAndRetryAsync(
                Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
                    retryCount: retryAttempts));
    }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
    private static bool ShouldRetry(HttpRequestMessage _, HttpResponseMessage response) =>
        response.StatusCode >= HttpStatusCode.InternalServerError;
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

    private static void BuildOcrService(IServiceCollection services, IConfiguration configuration)
    {
#if DEBUG
        if (configuration.IsSettingEnabled(MockOcrService.MockOcrServiceSetting))
        {
            services.AddSingleton<IOcrService, MockOcrService>();
        }
        else
        {
            services.AddSingleton<IOcrService, OcrService>();
        }
#else
            services.AddSingleton<IOcrService, OcrService>();
#endif
    }
}
