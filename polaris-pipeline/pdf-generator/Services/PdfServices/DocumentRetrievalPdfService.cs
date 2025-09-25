using Aspose.Words;
using DdeiClient.Factories;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using pdf_generator.Models;
using System.Threading.Tasks;

namespace pdf_generator.Services.PdfServices;

public class DocumentRetrievalWordsPdfService : WordsPdfService
{
    private readonly IDocumentRetrievalServiceFactory _documentRetrievalServiceFactory;

    public DocumentRetrievalWordsPdfService(IDocumentRetrievalServiceFactory documentRetrievalServiceFactory, IAsposeItemFactory asposeItemFactory) : base(asposeItemFactory)
    {
        _documentRetrievalServiceFactory = documentRetrievalServiceFactory;
    }

    public override async Task<PdfConversionResult> ReadToPdfStreamAsync(ReadToPdfDto readToPdfDto)
    {
        try
        {
            var conversionResult = await base.ReadToPdfStreamAsync(readToPdfDto);
            return conversionResult;
        }
        catch (FileCorruptedException ex)
        {
            //Todo: log something
        }

        var documentRetrievalService = _documentRetrievalServiceFactory.Create(readToPdfDto.DocumentId);
        var documentRetrievalDto = await documentRetrievalService.GetDocumentAsync(readToPdfDto.CmsAuthValues, readToPdfDto.CorrelationId, readToPdfDto.Urn, readToPdfDto.CaseId, readToPdfDto.DocumentId, readToPdfDto.VersionId);
        var doc = new Document(documentRetrievalDto.Stream);

        readToPdfDto.Stream = null;
        readToPdfDto.Stream = documentRetrievalDto.Stream;
        return await base.ReadToPdfStreamAsync(readToPdfDto);
    }
}