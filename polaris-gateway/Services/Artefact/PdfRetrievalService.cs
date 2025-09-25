using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Extensions;
using DdeiClient.Factories;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public class PdfRetrievalService : IPdfRetrievalService
{
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IDocumentRetrievalServiceFactory _documentRetrievalServiceFactory;

    public PdfRetrievalService(
        IPdfGeneratorClient pdfGeneratorClient, 
        IDocumentRetrievalServiceFactory documentRetrievalServiceFactory)
    {
        _pdfGeneratorClient = pdfGeneratorClient.ExceptionIfNull();
        _documentRetrievalServiceFactory = documentRetrievalServiceFactory.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var documentRetrievalService = _documentRetrievalServiceFactory.Create(documentId);
        var documentRetrievalDto = await documentRetrievalService.GetDocumentAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        if (!documentRetrievalDto.IsKnownFileType)
        {
            return new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentTypeUnsupported,
            };
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, documentRetrievalDto.Stream, documentRetrievalDto.FileType, cmsAuthValues);

        return new DocumentRetrievalResult
        {
            PdfStream = pdfResult.Status == PdfConversionStatus.DocumentConverted ? pdfResult.PdfStream : null,
            Status = pdfResult.Status,
        };
    }
}