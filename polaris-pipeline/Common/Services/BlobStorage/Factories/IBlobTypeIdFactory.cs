
namespace Common.Services.BlobStorage.Factories;

public interface IBlobTypeIdFactory
{
    BlobIdType CreateBlobId(int caseId, string materialId, long documentId, BlobType blobType);
}
