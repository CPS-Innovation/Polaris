// <copyright file="RedactionLoggerClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Clients.RedactionLogger;

using Common.Configuration;
using Common.Dto.Request;
using Common.Wrappers;
using coordinator.Clients.PdfRedactor;
using coordinator.Constants;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class RedactionLoggerClient(
    HttpClient httpClient,
    IJsonConvertWrapper jsonConvertWrapper,
    IRequestFactory pipelineClientRequestFactory,
    IOptions<RedactionLoggerConfig> redactionLoggerConfigOptions)
    : IRedactionLoggerClient
{
    private readonly HttpClient httpClient = httpClient;
    private readonly IJsonConvertWrapper jsonConvertWrapper =
            jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));

    private readonly IRequestFactory pipelineClientRequestFactory =
            pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));

    private readonly RedactionLoggerConfig redactionLoggerConfig =
            redactionLoggerConfigOptions?.Value
            ?? throw new ArgumentNullException(nameof(redactionLoggerConfigOptions));

    public async Task<Stream> CreateRedactionLog(
        CreateRedactionLogsRequest redactionLogRequest,
        Guid correlationId = default)
    {
        var requestMessage = new StringContent(
            this.jsonConvertWrapper.SerializeObject(redactionLogRequest),
            Encoding.UTF8,
            "application/json");

        var request = this.pipelineClientRequestFactory.Create(
            HttpMethod.Post,
            $"{RedactionLog.RedactionLogApiUri}",
            correlationId);

        request.Content = requestMessage;

        var response = await this.httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}
