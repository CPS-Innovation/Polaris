// <copyright file="MdsApiClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Clients
{
    using Cps.MasterDataService.Infrastructure.ApiClient;
    using DdeiClient.Clients.Interfaces;
    using DdeiClient.Configuration;
    using Microsoft;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Mds api client factory.
    /// </summary>
    public class MasterDataServiceApiClientFactory : IMasterDataServiceApiClientFactory
    {
        private readonly ILogger<MasterDataServiceApiClientFactory> logger;
        private readonly MasterDataServiceClientOptions clientOptions;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterDataServiceApiClientFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="clientOptions">The client endpoint options.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public MasterDataServiceApiClientFactory(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IOptions<MasterDataServiceClientOptions> clientOptions)
        {
            Requires.NotNull(nameof(loggerFactory));
            Requires.NotNull(nameof(clientOptions));
            Requires.NotNull(nameof(httpClientFactory));

            this.logger = loggerFactory.CreateLogger<MasterDataServiceApiClientFactory>();
            this.clientOptions = clientOptions.Value;
            this.httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public IMdsApiClient Create(string cookieHeader)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cookieHeader))
                {
                    this.logger.LogError("Cookie header is required.");
                    throw new ArgumentNullException(nameof(cookieHeader), "Cookie header is required.");
                }

                var functionKey = this.clientOptions.FunctionKey;
                if (string.IsNullOrWhiteSpace(functionKey))
                {
                    this.logger.LogError("Missing MDS function key in configuration.");
                    throw new InvalidOperationException("Missing MDS function key in configuration.");
                }

                var baseUrl = this.clientOptions.BaseAddress.ToString();
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    this.logger.LogError("Missing MDS base url in configuration.");
                    throw new InvalidOperationException("Missing MDS base url in configuration.");
                }


                // Create the HttpClient using a named client
                var httpClient = this.httpClientFactory.CreateClient("MdsClient");
                httpClient.DefaultRequestHeaders.Add("x-functions-key", functionKey);
                httpClient.DefaultRequestHeaders.Add("Cms-Auth-Values", cookieHeader);

                var apiClient = new MdsApiClient(httpClient)
                {
                    BaseUrl = baseUrl,
                };

                // --- Mutate existing JsonSerializerSettings instead of replacing it ---
                apiClient.JsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                apiClient.JsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
                apiClient.JsonSerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = true,
                };

                apiClient.JsonSerializerSettings.Error += (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                };

                return apiClient;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while creating Mds api client.");
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
