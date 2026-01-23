// <copyright file="TestBase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace HkuiFunctionsIntegrationTests;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.ServiceClient.Uma;
using Cps.Fct.Hk.Ui.ServiceClient.Uma.Configuration;
using Cps.Fct.Hk.Ui.Services;
using Cps.Fct.Hk.Ui.Services.Validators;
using Cps.MasterDataService.Infrastructure.ApiClient;
using DdeiClient.Clients;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Configuration;
using Extensions;
using Microsoft;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for Housekeeping Ui Functions backend service,
/// verifying that messages can be reclassified in CMS.
/// </summary>
public class TestBase
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1600 // Elements should be documented

    protected internal readonly ITestOutputHelper testOutputHelper;
    protected internal readonly ICaseInfoService? caseInfoService;
    protected internal readonly IDocumentTypeMapper? documentTypeMapper;
    protected internal readonly ICommunicationMapper? communicationMapper;
    protected internal readonly ICommunicationService? communicationService;
    protected internal readonly ICaseMaterialService? caseMaterialService;
    protected internal readonly IBulkSetUnusedService? bulkSetUnusedService;
    protected internal readonly IUmaReclassifyService? umaReclassifyService;
    protected internal readonly RenameMaterialRequestValidator? renameMaterialRequestValidator;
    protected internal readonly IMasterDataServiceClient? apiClient;
    protected internal readonly IUmaServiceClient? umaClient;
    protected internal readonly IWitnessService? witnessService;
    protected internal readonly ICaseDefendantsService? caseDefendantsService;
    protected internal readonly IReclassificationService? reclassificationService;
    protected internal readonly IMaterialReclassificationOrchestrationService? materialReclassificationOrchestrationService;
    protected internal readonly ICaseActionPlanService? caseActionPlanService;
    protected internal readonly ICaseLockService? caseLockService;
    protected internal readonly UpdateStatementRequestValidator? updateStatementRequestValidator;
    protected internal readonly UpdateExhibitRequestValidator? updateExhibitRequestValidator;

    protected internal int baseCaseId;
    protected internal string baseUsername;
    protected internal string basePassword;
    protected internal AuthenticationContext baseAuthContext;
    protected internal HttpRequest baseRequest;
    
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1600 // Elements should be documented

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public TestBase(ITestOutputHelper testOutputHelper)
    {
        var assemblyInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);

        IHost host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? configurationBuilder.Build()["environment"];

                configurationBuilder
                    .AddJsonFile("integration-test.settings.json", optional: true, reloadOnChange: false)

#if DEBUG
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
#endif

                    .AddEnvironmentVariables();

                this.Configuration = configurationBuilder.Build();
            })
            .ConfigureServices((context, services) =>
            {
                // Register MasterDataService client options.
                //services.AddServiceOptions<MasterDataServiceClientOptions>("DDEIClient");
                services.AddServiceOptions<MasterDataServiceClientOptions>(MasterDataServiceClientOptions.DefaultSectionName);
                services.AddServiceOptions<UmaClientOptions>(UmaClientOptions.DefaultSectionName);

                services.AddHttpClient(
                    name: MasterDataServiceClientOptions.DefaultSectionName,
                    configureClient: (services, options) =>
                    {
                        MasterDataServiceClientOptions endpointOptions = services.GetRequiredService<IOptions<MasterDataServiceClientOptions>>().Value;
                        options.BaseAddress = endpointOptions.BaseAddress;
                        options.DefaultRequestHeaders
                            .Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
                    });
                services.AddSingleton<ICaseInfoService, CaseInfoService>();
                services.AddSingleton<IDocumentTypeMapper, DocumentTypeMapper>();
                services.AddSingleton<ICommunicationMapper, CommunicationMapper>();
                services.AddSingleton<ICommunicationService, CommunicationService>();
                services.AddSingleton<ICaseMaterialService, CaseMaterialService>();
                //services.AddDdeiApiClient<ClientEndpointOptions>("DDEIClient");
                services.AddSingleton<IReclassificationService, ReclassificationService>();
                services.AddSingleton<IWitnessService, WitnessService>();
                services.AddSingleton<ICaseDefendantsService, CaseDefendantsService>();
                services.AddSingleton<IMaterialReclassificationOrchestrationService, MaterialReclassificationOrchestrationService>();
                services.AddSingleton<ICaseLockService, CaseLockService>();
                services.AddSingleton<ICaseActionPlanService, CaseActionPlanService>();
                services.AddSingleton<IMasterDataServiceClient, MasterDataServiceClient>();
                services.AddSingleton<IMasterDataServiceApiClientFactory, MasterDataServiceApiClientFactory>(); 
                
                //  services.AddDDEIProvider();

                // Add validators
                services.AddSingleton<RenameMaterialRequestValidator>();
                services.AddSingleton<UpdateStatementRequestValidator>();
                services.AddSingleton<UpdateExhibitRequestValidator>();

                // Uma service client
                services.Configure<UmaClientOptions>(context.Configuration.GetSection("UMAClient"));
                services.AddSingleton<IUmaServiceClient, UmaServiceClient>();
                services.AddSingleton<IBulkSetUnusedService, BulkSetUnusedService>();
                services.AddSingleton<IUmaReclassifyService, UmaReclassifyService>();

                this.ServiceProvider = services.BuildServiceProvider();
            })
            .Build();

        this.testOutputHelper = testOutputHelper;
        this.caseInfoService = this.ServiceProvider?.GetService<ICaseInfoService>();
        this.documentTypeMapper = this.ServiceProvider?.GetService<IDocumentTypeMapper>();
        this.communicationMapper = this.ServiceProvider?.GetService<ICommunicationMapper>();
        this.communicationService = this.ServiceProvider?.GetService<ICommunicationService>();
        this.caseMaterialService = this.ServiceProvider?.GetService<ICaseMaterialService>();
        this.bulkSetUnusedService = this.ServiceProvider?.GetService<IBulkSetUnusedService>();
        this.umaReclassifyService = this.ServiceProvider?.GetService<IUmaReclassifyService>();
        this.renameMaterialRequestValidator = this.ServiceProvider?.GetService<RenameMaterialRequestValidator>();
        this.apiClient = this.ServiceProvider?.GetService<IMasterDataServiceClient>();
        this.umaClient = this.ServiceProvider?.GetService<IUmaServiceClient>();
        this.reclassificationService = this.ServiceProvider?.GetService<IReclassificationService>();
        this.witnessService = this.ServiceProvider?.GetService<IWitnessService>();
        this.caseDefendantsService = this.ServiceProvider?.GetService<ICaseDefendantsService>();
        this.materialReclassificationOrchestrationService = this.ServiceProvider?.GetService<IMaterialReclassificationOrchestrationService>();
        this.caseLockService = this.ServiceProvider?.GetService<ICaseLockService>();
        this.caseActionPlanService = this.ServiceProvider?.GetService<ICaseActionPlanService>();
        this.updateStatementRequestValidator = this.ServiceProvider?.GetService<UpdateStatementRequestValidator>();
        this.updateExhibitRequestValidator = this.ServiceProvider?.GetService<UpdateExhibitRequestValidator>();

        this.ValidateTestDependencies();

        (this.baseCaseId, _, this.baseUsername, this.basePassword) = this.GetTestConfiguration();
    }

    /// <summary>
    /// Gets or sets configuration.
    /// </summary>
    protected internal IConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets serviceProvider.
    /// </summary>
    protected internal IServiceProvider? ServiceProvider { get; set; }


    /// <summary>
    /// Creates a mock <see cref="HttpRequest"/> with an HSK cookie containing authentication context data.
    /// </summary>
    /// <param name="caseId">The case ID to include in the cookie.</param>
    /// <param name="authContext">The authentication context containing cookies and token.</param>
    /// <param name="jsonBody">The optional JSON body to include in the request, if applicable.</param>
    /// <param name="method">The HTTP method for the request (e.g., GET, POST). Defaults to "GET".</param>
    /// <returns>A <see cref="HttpRequest"/> with the HSK cookie set.</returns>
    protected internal HttpRequest CreateHttpRequestWithCookie(
        int caseId,
        AuthenticationContext authContext,
        string? jsonBody = null,
        string method = "GET")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;

        request.Method = method.ToUpperInvariant();

        var polarisGatewayCookie = $"Token:{authContext.Token},SessionCorrelationId:{Guid.NewGuid()},SessionCreatedTime:{DateTime.Now},CmsVersionId:CMS.24.0.01,Cookies:{authContext.Cookies},UserIpAddress:0.0.0.0";

        request.Headers.Cookie = $"{HttpHeaderKeys.CmsAuthValues}={Uri.EscapeDataString(polarisGatewayCookie)}";

        if (!string.IsNullOrWhiteSpace(jsonBody) && (method == "POST" || method == "PUT" || method == "PATCH"))
        {
            request.ContentType = "application/json";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);
            request.Body = new MemoryStream(byteArray);
            request.ContentLength = byteArray.Length;
        }

        return request;
    }

    /// <summary>
    /// Validates that all required service dependencies have been initialized.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any required service is null.
    /// </exception>
    protected internal void ValidateTestDependencies()
    {
        if (this.caseInfoService == null)
        {
            throw new InvalidOperationException("caseInfoService is not initialized.");
        }

        if (this.documentTypeMapper == null)
        {
            throw new InvalidOperationException("documentTypeMapper is not initialized.");
        }

        if (this.communicationMapper == null)
        {
            throw new InvalidOperationException("communicationMapper is not initialized.");
        }

        if (this.communicationService == null)
        {
            throw new InvalidOperationException("communicationService is not initialized.");
        }

        if (this.caseMaterialService == null)
        {
            throw new InvalidOperationException("caseMaterialService is not initialized.");
        }

        if (this.bulkSetUnusedService == null)
        {
            throw new InvalidOperationException("bulkSetUnusedService is not initialized.");
        }

        if (this.umaReclassifyService == null)
        {
            throw new InvalidOperationException("umaReclassifyService is not initialized.");
        }

        if (this.renameMaterialRequestValidator == null)
        {
            throw new InvalidOperationException("renameMaterialRequestValidator is not initialized.");
        }

        if (this.apiClient == null)
        {
            throw new InvalidOperationException("apiClient is not initialized.");
        }

        if (this.umaClient == null)
        {
            throw new InvalidOperationException("umaClient is not initialized.");
        }

        if (this.reclassificationService == null)
        {
            throw new InvalidOperationException("reclassificationService is not initialized.");
        }

        if (this.witnessService == null)
        {
            throw new InvalidOperationException("witnessService is not initialised.");
        }

        if (this.materialReclassificationOrchestrationService == null)
        {
            throw new InvalidOperationException("materialReclassificationOrchestrationService is not initialised");
        }

        if (this.caseLockService == null)
        {
            throw new InvalidOperationException("caseLockService is not initialised");
        }

        if (this.caseActionPlanService == null)
        {
            throw new InvalidOperationException("caseActionPlanService is not initialised");
        }

        if (this.updateStatementRequestValidator == null)
        {
            throw new InvalidOperationException("updateStatementRequestValidator is not initialised");
        }

        if (this.updateExhibitRequestValidator == null)
        {
            throw new InvalidOperationException("updateExhibitRequestValidator is not initialised.");
        }
    }

    /// <summary>
    /// Retrieves test configuration values required for authentication and test data.
    /// </summary>
    /// <returns>A tuple containing the case ID, username, and password.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any required configuration value is missing or invalid.</exception>
    protected internal (int CaseId, int TestDelay, string Username, string Password) GetTestConfiguration()
    {
        int? caseId = this.Configuration?.GetValue<int>("TestCaseId");
        int? testDelay = this.Configuration?.GetValue<int>("TestDelayInMilliseconds");
        string? username = this.Configuration?.GetValue<string>("MasterDataServiceClient:CredentialName");
        string? password = this.Configuration?.GetValue<string>("MasterDataServiceClient:CredentialPassword");

        Assert.True(caseId.HasValue && caseId.Value > 0, "TestCaseId configuration is missing.");
        Assert.True(testDelay.HasValue && testDelay.Value > 0, "TestDelayInMilliseconds configuration is missing.");
        Assert.False(string.IsNullOrWhiteSpace(username), "MasterDataServiceClient:CredentialName configuration is missing.");
        Assert.False(string.IsNullOrWhiteSpace(password), "MasterDataServiceClient:CredentialPassword configuration is missing.");

        return (caseId.Value, testDelay.Value, username!, password!);
    }

    /// <summary>
    /// Creates an <see cref="AuthenticationContext"/> for use in integration tests.
    /// </summary>
    /// <param name="username">The CMS username.</param>
    /// <param name="password">The CMS password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AuthenticationContext"/>.</returns>
    protected internal async Task<AuthenticationContext> GetAuthenticationContextAsync(string username, string password)
    {
        var request = new AuthenticationRequest(Guid.NewGuid(), username, password);
        return await this.AuthenticateAsync(request).ConfigureAwait(true);
    }


    public async Task<AuthenticationContext> AuthenticateAsync(AuthenticationRequest request)
    {
        Requires.NotNull(request);

        AuthenticationContext result = null;
        try
        {
            // Create the HttpClient using a named client
            (int caseId, int _, string username, string password) = this.GetTestConfiguration();

            string? funcKey = this.Configuration?.GetValue<string>("MasterDataServiceClient:FunctionKey");
            string? baseUrl = this.Configuration?.GetValue<string>("MasterDataServiceClient:BaseAddress");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-functions-key", funcKey);

            var apiClient = new MdsApiClient(httpClient)
            {
                BaseUrl = baseUrl,
            };

            AuthenticateOpenApiDefinition authRequest = new AuthenticateOpenApiDefinition()
            {
                Username = username,
                Password = password,
            };

            var authResult = await apiClient.AuthenticateAsync(authRequest);

            if (authResult.Cookies != null)
            {
                result = new AuthenticationContext(cookies:authResult.Cookies, token: authResult.Token!, authResult.ExpiryTime.Value);
            }
        }
        catch (Exception exception)
        {
            throw;
        }

        return result;
    }

    ///// <summary>
    /// ResetAllCommunications.
    /// </summary>
    /// <returns>Return completed task.</returns>
    protected internal async Task ResetAllCommunications()
    {
        // Arrange
        this.ValidateTestDependencies();
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();

        AuthenticationRequest authenticationRequest = new(
          CorrespondenceId: Guid.NewGuid(),
          Username: username,
          Password: password);

        AuthenticationContext authContext = await this.AuthenticateAsync(authenticationRequest).ConfigureAwait(true);

        CmsAuthValues cmsAuthValues = new CmsAuthValues(authContext.Cookies, authContext.Token);

        var request = new ListCommunicationsHkRequest(caseId, Guid.NewGuid());

        IReadOnlyCollection<Communication> communications = await this.apiClient!.ListCommunicationsHkAsync(request, cmsAuthValues).ConfigureAwait(false);

        foreach (Communication communication in communications)
        {
            if (communication.DocumentTypeId != 1029)
            {
                var correspondenceId = Guid.NewGuid();
                string classification = "OTHER";
                int materialId = communication.MaterialId;
                int documentTypeId = 1029;
                bool used = false;
                string subject = communication.Subject;

                var reclassifyCommunicationRequest = new ReclassifyCommunicationRequest(
                    correspondenceId,
                    classification,
                    materialId,
                    documentTypeId,
                    used,
                    subject);

                await this.apiClient.ReclassifyCommunicationAsync(reclassifyCommunicationRequest, cmsAuthValues).ConfigureAwait(false);
            }
        }
    }
}
