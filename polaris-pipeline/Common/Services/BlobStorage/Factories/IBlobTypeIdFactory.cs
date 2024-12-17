
namespace Common.Services.BlobStorage.Factories;

public interface IBlobTypeIdFactory
{
    BlobIdType CreateBlobId(int caseId, string documentId, long versionId, BlobType blobType);
}
