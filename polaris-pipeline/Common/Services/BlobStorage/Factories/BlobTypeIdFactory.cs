
namespace Common.Services.BlobStorage.Factories;

public class BlobTypeIdFactory : IBlobTypeIdFactory
{
    public BlobIdType CreateBlobId(int caseId, string materialId, long documentId, BlobType blobType)
    {
        return new BlobIdType(caseId, materialId, documentId, blobType);
    }
}
