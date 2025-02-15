﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Streaming;

namespace Common.Clients.PdfGenerator
{
    public class PdfGeneratorClient : IPdfGeneratorClient
    {
        private const string FiletypeKey = "Filetype";
        private readonly IPdfGeneratorRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

        public PdfGeneratorClient(IPdfGeneratorRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory)
        {
            _requestFactory = pipelineClientRequestFactory ?? throw new ArgumentNullException(nameof(pipelineClientRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory ?? throw new ArgumentNullException(nameof(httpResponseMessageStreamFactory));
        }

        public async Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string caseUrn, int caseId, string documentId, long versionId, Stream documentStream, FileType fileType)
        {
            // note: it is useful for analytics to have our case and document ids etc in the url in the call to the pdf generator.  Not strictly necessary 
            //  as all it is doing is converting one stream to another.
            var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}", correlationId);
            request.Headers.Add(FiletypeKey, fileType.ToString());

            using var requestContent = new StreamContent(documentStream);
            request.Content = requestContent;

            // do not dispose or use `using` on response
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                var result = await response.Content.ReadAsStringAsync();
                Enum.TryParse<PdfConversionStatus>(result, out var enumResult);

                return new ConvertToPdfResponse
                {
                    Status = enumResult,
                };
            }

            response.EnsureSuccessStatusCode();
            var streamResult = await _httpResponseMessageStreamFactory.Create(response);

            return new ConvertToPdfResponse
            {
                PdfStream = streamResult,
                Status = PdfConversionStatus.DocumentConverted
            };
        }
    }
}
