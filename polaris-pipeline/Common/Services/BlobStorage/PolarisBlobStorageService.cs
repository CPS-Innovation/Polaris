using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorage;

public class PolarisBlobStorageService : IPolarisBlobStorageService
{
    private const string IsOcrProcessedInCms = nameof(IsOcrProcessedInCms);

    private readonly IBlobStorageService _blobStorageService;

    public PolarisBlobStorageService(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    }

    public Task<bool> BlobExistsAsync(BlobIdType blobId, bool? mustBeOcred = null)
        => _blobStorageService.BlobExistsAsync(GetBlobName(blobId), CreateMetaData(mustBeOcred));

    public Task DeleteBlobsByPrefixAsync(int prefix) => _blobStorageService.DeleteBlobsByPrefix(GetSafePrefix(prefix));

    public Task<Stream> GetBlobAsync(BlobIdType blobId) => _blobStorageService.GetBlob(GetBlobName(blobId));

    public Task<Stream> TryGetBlobAsync(BlobIdType blobId, bool? mustBeOcred = null)
        => _blobStorageService.TryGetBlobAsync(GetBlobName(blobId), CreateMetaData(mustBeOcred));

    public Task<T> TryGetObjectAsync<T>(BlobIdType blobId) => _blobStorageService.TryGetObjectAsync<T>(GetBlobName(blobId));

    public Task UploadBlobAsync(Stream stream, BlobIdType blobId) => _blobStorageService.UploadBlobAsync(stream, GetBlobName(blobId));

    public Task UploadBlobAsync(Stream stream, BlobIdType blobId, bool? isOcred = null)
        => _blobStorageService.UploadBlobAsync(stream, GetBlobName(blobId), CreateMetaData(isOcred));

    public Task UploadBlobAsync(Stream stream, BlobIdType blobId, int? pageIndex = null, int? maxDimensionPixel = null)
        => _blobStorageService.UploadBlobAsync(stream, GetBlobName(blobId, pageIndex, maxDimensionPixel));

    public Task UploadObjectAsync<T>(T obj, BlobIdType blobId) => _blobStorageService.UploadObjectAsync(obj, GetBlobName(blobId));

    private static string GetBlobName(BlobIdType blobId, int? pageIndex = null, int? maxDimensionPixel = null)
    {
        var (caseId, documentId, versionId, blobType) = blobId;
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the DocumentId for a DAC is DAC-12345
        //  The Pdf blobId has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.
        if (long.TryParse(documentId, out _))
        {
            throw new ArgumentOutOfRangeException(nameof(blobId), "blobId.documentId should not be a number");
            //documentId = $"CMS-{documentId}";
        }

        return blobType switch
        {
            BlobType.Pdf => $"{caseId}/pdfs/{documentId}-{versionId}.pdf",
            BlobType.Ocr => $"{caseId}/ocrs/{documentId}-{versionId}.json",
            BlobType.Pii => $"{caseId}/pii/{documentId}-{versionId}.json",
            BlobType.Thumbnail => $"{caseId}/thumbnails/{documentId}-{versionId}/{maxDimensionPixel}px{pageIndex}.jpg",
            BlobType.DocumentList => $"{caseId}/caseDocuments/caseDocuments-{caseId}.json",
            _ => throw new NotImplementedException()
        };
    }

    private static string GetSafePrefix(int caseId)
    {
        return $"{caseId}/";
    }

    private static Dictionary<string, string> CreateMetaData(bool? ocrFlag)
        => ocrFlag == true
            ? new Dictionary<string, string> { { IsOcrProcessedInCms, true.ToString() } }
            : null;
}