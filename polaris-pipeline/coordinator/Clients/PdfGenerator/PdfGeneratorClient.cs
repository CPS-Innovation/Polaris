﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Wrappers;
using Common.Streaming;
using Microsoft.Extensions.Configuration;

namespace coordinator.Clients.PdfGenerator
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private const string FiletypeKey = "Filetype";
        private readonly IRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public PdfGeneratorClient(IRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            IJsonConvertWrapper jsonConvertWrapper)
        {
            _requestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        }

        public async Task<Stream> ConvertToPdfAsync(Guid correlationId, string cmsAuthValues, string caseUrn, string caseId, string documentId, string versionId, Stream documentStream, FileType fileType)
        {
            var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}?code={_configuration[Constants.ConfigKeys.PipelineRedactPdfFunctionAppKey]}", correlationId);
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            request.Headers.Add(FiletypeKey, fileType.ToString());

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            // do not dispose or use `using` on response
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                throw new UnsupportedMediaTypeException(
                    $"Unsupported media type: {fileType}",
                    // todo: we do not have the *real* media type header to hand, so just use a generic one
                    //  Key thing is we communicate to the caller that the pdf-generator has rejected us on media type grounds
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                );
            }

            response.EnsureSuccessStatusCode();
            return await _httpResponseMessageStreamFactory.Create(response);
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(string caseUrn, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            HttpResponseMessage response;
            try
            {
                var requestMessage = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest), Encoding.UTF8, "application/json");

                var request = _requestFactory.Create(HttpMethod.Put, $"{RestApi.GetRedactPdfPath(caseUrn, caseId, documentId)}?code={_configuration[Constants.ConfigKeys.PipelineRedactPdfFunctionAppKey]}", correlationId);
                request.Content = requestMessage;

                response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // todo: check if ok to swallow a not found response?
                    return null;
                }

                throw;
            }

            var stringContent = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent);
        }
    }
}