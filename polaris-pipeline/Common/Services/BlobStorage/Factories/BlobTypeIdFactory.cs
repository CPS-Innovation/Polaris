
namespace Common.Services.BlobStorage.Factories;

public class BlobTypeIdFactory : IBlobTypeIdFactory
{
    public BlobIdType CreateBlobId(int caseId, string documentId, long versionId, BlobType blobType)
    {
        return new BlobIdType(caseId, documentId, versionId, blobType);
    }
}
