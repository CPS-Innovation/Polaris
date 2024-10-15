using System.Text.RegularExpressions;
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

    public ArtefactService(
        IV2PolarisBlobStorageService blobStorageService,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
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

        var documentStream = await _ddeiClient.GetDocumentAsync(ddeiArgs);
        await _blobStorageService.UploadDocumentAsync(documentStream, blobName, metaData);

        return await _blobStorageService.GetDocumentAsync(blobName, metaData);
    }

    public async Task<Stream> InitiateOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        throw new NotImplementedException();
    }
}
