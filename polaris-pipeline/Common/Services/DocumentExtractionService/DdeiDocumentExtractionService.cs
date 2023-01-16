using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Responses;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Mappers.Contracts;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Services.DocumentExtractionService;

public class DdeiDocumentExtractionService : BaseDocumentExtractionService, IDdeiDocumentExtractionService
{
    private readonly ILogger<DdeiDocumentExtractionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;

    public DdeiDocumentExtractionService(HttpClient httpClient, IHttpRequestFactory httpRequestFactory, ILogger<DdeiDocumentExtractionService> logger, 
        IConfiguration configuration, IJsonConvertWrapper jsonConvertWrapper, ICaseDocumentMapper<DdeiCaseDocumentResponse> caseDocumentMapper)
        : base(logger, httpRequestFactory, httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
    }

    public async Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string accessToken, string upstreamToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}, DocumentId: {documentId}");
        
        var content = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.GetDocumentUrl], caseUrn, caseId, documentCategory, documentId), accessToken, upstreamToken, correlationId);
        var result = await content.ReadAsStreamAsync();
        
        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        return result;
    }

    public async Task<CaseDocument[]> ListDocumentsAsync(string caseUrn, string caseId, string accessToken, string upstreamToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}");

        var response = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.ListDocumentsUrl], caseUrn, caseId), accessToken, upstreamToken, correlationId);
        var stringContent = await response.ReadAsStringAsync();
        var ddeiResults = _jsonConvertWrapper.DeserializeObject<List<DdeiCaseDocumentResponse>>(stringContent);

        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        var results = ddeiResults.Where(x => !string.IsNullOrWhiteSpace(x.OriginalFileName))
            .Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult)).ToList();
        
        //there seem to be additional instances of a document, following its reclassification to a specific document type. Update the results collection with this information`
        var additionalClassifications = ddeiResults.Where(x => string.IsNullOrWhiteSpace(x.OriginalFileName)).ToList();
        foreach (var additionalInfo in additionalClassifications)
        {
            if (string.IsNullOrWhiteSpace(additionalInfo.DocumentType)) continue;
            
            var result = results.FirstOrDefault(x => x.DocumentId == additionalInfo.Id.ToString());
            if (result == null) continue;
            
            result.CmsDocType.DocumentCategory = additionalInfo.CmsDocCategory;
            result.CmsDocType.DocumentType = additionalInfo.DocumentType;
            result.CmsDocType.DocumentTypeId = additionalInfo.DocumentTypeId;
        }

        return results.ToArray();
    }
}
