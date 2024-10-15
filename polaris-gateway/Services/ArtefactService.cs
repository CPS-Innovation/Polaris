using System.Text.RegularExpressions;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Helpers;
using Common.Services.BlobStorageService;
using Ddei;
using Ddei.Factories;

namespace PolarisGateway.Services;

public class ArtefactService : IArtefactService
{
    private readonly IV2PolarisBlobStorageService _blobStorageService;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;

    public ArtefactService(
        IV2PolarisBlobStorageService blobStorageService,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _pdfGeneratorClient = pdfGeneratorClient ?? throw new ArgumentNullException(nameof(pdfGeneratorClient));
    }

    public async Task<Stream> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var blobName = BlobNameHelper.GetBlobName(caseId, documentId, versionId, BlobNameHelper.BlobType.Pdf);
        var metaData = new Dictionary<string, string> { { "isOcrProcessed", isOcrProcessed.ToString() } };

        var blobStream = await _blobStorageService.GetDocumentAsync(blobName, mustMatchMetadata: metaData);
        if (blobStream != null)
        {
            return blobStream;
        }

        var documentIdWithoutPrefix = long.Parse(Regex.Match(documentId, @"\d+").Value);
        var ddeiArgs = _ddeiArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, documentIdWithoutPrefix, versionId);

        var fileResult = await _ddeiClient.GetDocumentAsync(ddeiArgs);
        var fileExtension = Path.GetExtension(fileResult.FileName)
            .Replace(".", string.Empty)
            .ToUpperInvariant();

        var isRecognisedFileType = Enum.TryParse<FileType>(fileExtension, out var fileType);
        if (!isRecognisedFileType)
        {
            return null;
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, fileResult.Stream, fileType);
        if (pdfResult.Status != PdfConversionStatus.DocumentConverted)
        {
            return null;
        }

        await _blobStorageService.UploadDocumentAsync(pdfResult.PdfStream, blobName, metaData);
        return await _blobStorageService.GetDocumentAsync(blobName, metaData);
    }

    public async Task<Stream> InitiateOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        throw new NotImplementedException();
    }
}
