using System;

namespace coordinator.Domain.Tracker
{
    public class RegisterPdfBlobNameArg
    {
        public RegisterPdfBlobNameArg(string documentId, long versionId, string blobName)
        {
            DocumentId = documentId ?? throw new ArgumentNullException(nameof(documentId));
            VersionId = versionId;
            BlobName = blobName ?? throw new ArgumentNullException(nameof(blobName));
        }
        
        public string DocumentId { get; set; }
        
        public long VersionId { get; set; }

        public string BlobName { get; set; }
    }
}
