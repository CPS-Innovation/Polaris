// <copyright file="MdsApiClientFactory.cs" company="TheCrownProsecutionService">
// Copyright (c) TheCrownProsecutionService. All rights reserved.
// </copyright>

namespace MasterDataServiceClient
{
    using Cps.MasterDataService.Infrastructure.ApiClient;
    using MasterDataServiceClient.Configuration;
    using Microsoft;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Mds api client factory.
    /// </summary>
    public class MdsApiClientFactory : IMdsApiClientFactory
    {
        private readonly ILogger<MdsApiClientFactory> logger;
        private readonly MdsClientOptions clientOptions;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MdsApiClientFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="clientOptions">The client endpoint options.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public MdsApiClientFactory(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IOptions<MdsClientOptions> clientOptions)
        {
            Requires.NotNull(nameof(loggerFactory));
            Requires.NotNull(nameof(clientOptions));
            Requires.NotNull(nameof(httpClientFactory));

            this.logger = loggerFactory.CreateLogger<MdsApiClientFactory>();
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
                var client = this.httpClientFactory.CreateClient("MdsClient");

                client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
                client.DefaultRequestHeaders.Add("Cms-Auth-Values", cookieHeader);

                return new MdsApiClient(client) { BaseUrl = baseUrl };
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
