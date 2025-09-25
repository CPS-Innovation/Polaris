using Common.Domain.Document;
using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Factories;
using PolarisGateway.Models;

namespace DdeiClient.Services.DocumentRetrieval;

public class DocumentRetrievalService : IDocumentRetrievalService
{
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;

    public DocumentRetrievalService(IDdeiArgFactory ddeiArgFactory, IMdsClient mdsClient)
    {
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }
    public async Task<DocumentRetrievalDto> GetDocumentAsync(string cmsAuthValues, Guid correlationId, string caseUrn, int caseId, string documentId, long versionId)
    {
        var ddeiDocumentIdAndVersionIdArgDto = _ddeiArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId, versionId);
        var fileResult = await _mdsClient.GetDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return new DocumentRetrievalDto
        {
            Stream = fileResult.Stream,
            FileType = fileType,
            IsKnownFileType = isKnownFileType
        };
    }
}