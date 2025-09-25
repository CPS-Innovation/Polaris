using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.PdfGeneratorDomain.Domain;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Extensions;
using Common.Streaming;

namespace Common.Clients.PdfGenerator;

public class PdfGeneratorClient : IPdfGeneratorClient
{
    private const string FiletypeKey = "Filetype";
    private readonly IPdfGeneratorRequestFactory _requestFactory;
    private readonly HttpClient _httpClient;
    private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;

    public PdfGeneratorClient(
        IPdfGeneratorRequestFactory pipelineClientRequestFactory,
        HttpClient httpClient,
        IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory)
    {
        _requestFactory = pipelineClientRequestFactory.ExceptionIfNull();
        _httpClient = httpClient.ExceptionIfNull();
        _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory.ExceptionIfNull();
    }

    public async Task<ConvertToPdfResponse> ConvertToPdfAsync(Guid correlationId, string caseUrn, int caseId, string documentId, long versionId, Stream documentStream, FileType fileType, string cmsAuthValues)
    {
        // note: it is useful for analytics to have our case and document ids etc in the url in the call to the pdf generator.  Not strictly necessary 
        //  as all it is doing is converting one stream to another.
        var request = _requestFactory.Create(HttpMethod.Post, $"{RestApi.GetConvertToPdfPath(caseUrn, caseId, documentId, versionId)}", correlationId, cmsAuthValues);
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