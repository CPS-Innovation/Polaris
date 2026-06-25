using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

    public Task UploadBlobAsync(Stream stream, BlobIdType blobId, bool? isOcred = null, double? fileSizeInMb = null)
        => _blobStorageService.UploadBlobAsync(stream, GetBlobName(blobId), CreateMetaData(isOcred, fileSizeInMb));

    public Task<IDictionary<string, string>> GetMetadataAsync(BlobIdType blobId)
    {
        return _blobStorageService.GetMetadataAsync(GetBlobName(blobId));
    }

    public Task UploadBlobAsync(Stream stream, BlobIdType blobId, int? pageIndex = null, int? maxDimensionPixel = null)
        => _blobStorageService.UploadBlobAsync(stream, GetBlobName(blobId, pageIndex, maxDimensionPixel));

    public Task UploadObjectAsync<T>(T obj, BlobIdType blobId) => _blobStorageService.UploadObjectAsync(obj, GetBlobName(blobId));

    public Task UploadSizeAsync(string key, double fileSizeInMb)
    {
        var bytes = Encoding.UTF8.GetBytes(fileSizeInMb.ToString(CultureInfo.InvariantCulture));
        var stream = new MemoryStream(bytes);

        return _blobStorageService.UploadBlobAsync(stream, key);
    }

    public async Task<double?> GetSizeAsync(string key)
    {
        var stream = await _blobStorageService.TryGetBlobAsync(key);

        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
            return size;

        return null;
    }

    private static string GetBlobName(BlobIdType blobId, int? pageIndex = null, int? maxDimensionPixel = null)
    {
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the DocumentId for a DAC is DAC-12345
        //  The Pdf blobId has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.
        if (long.TryParse(blobId.DocumentId, out _))
        {
            throw new ArgumentOutOfRangeException(nameof(blobId), "blobId.documentId should not be a number");
            //documentId = $"CMS-{documentId}";
        }

        return blobId.BlobType switch
        {
            BlobType.Pdf => $"{blobId.CaseId}/pdfs/{blobId.DocumentId}-{blobId.VersionId}.pdf",
            BlobType.Ocr => $"{blobId.CaseId}/ocrs/{blobId.DocumentId}-{blobId.VersionId}.json",
            BlobType.Pii => $"{blobId.CaseId}/pii/{blobId.DocumentId}-{blobId.VersionId}.json",
            BlobType.Thumbnail => $"{blobId.CaseId}/thumbnails/{blobId.DocumentId}-{blobId.VersionId}/{maxDimensionPixel}px{pageIndex}.jpg",
            BlobType.DocumentsList => $"{blobId.CaseId}/caseState/caseDocuments-{blobId.CaseId}.json",
            BlobType.CaseState => $"{blobId.CaseId}/caseState/caseState-{blobId.CaseId}.json",
            BlobType.CaseDelta => $"{blobId.CaseId}/caseState/caseDelta-{blobId.CaseId}.json",
            BlobType.DocumentState => $"{blobId.CaseId}/caseState/caseDocumentsState-{blobId.CaseId}.json",
            _ => throw new NotImplementedException()
        };
    }

    private static string GetSafePrefix(int caseId)
    {
        return $"{caseId}/";
    }

    private static Dictionary<string, string> CreateMetaData(bool? ocrFlag, double? fileSizeInMb = null)
{
    var metadata = new Dictionary<string, string>();

    if (ocrFlag == true)
    {
        metadata[IsOcrProcessedInCms] = true.ToString();
    }

    if (fileSizeInMb.HasValue)
    {
        metadata["FileSizeInMb"] = fileSizeInMb.Value.ToString("F3"); // optional formatting
    }

    return metadata.Count > 0 ? metadata : null;
}
}