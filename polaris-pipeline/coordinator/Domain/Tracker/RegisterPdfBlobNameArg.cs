using System;

namespace coordinator.Domain.Tracker
{
    public class RegisterPdfBlobNameArg
    {
        public RegisterPdfBlobNameArg(DateTime currentUtcDateTime, string documentId, long versionId, string blobName)
        {
            CurrentUtcDateTime = currentUtcDateTime;
            DocumentId = documentId ?? throw new ArgumentNullException(nameof(documentId));
            VersionId = versionId;
            BlobName = blobName ?? throw new ArgumentNullException(nameof(blobName));
        }

        public DateTime CurrentUtcDateTime { get; set; }
        
        public string DocumentId { get; set; }
        
        public long VersionId { get; set; }

        public string BlobName { get; set; }
    }
}
